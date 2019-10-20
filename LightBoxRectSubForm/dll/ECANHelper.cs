using ECAN;
using LightBoxRectSubForm.app;
using LightBoxRectSubForm.comm;
using LightBoxRectSubForm.data;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightBoxRectSubForm.dll {

    public class ECANHelper {

        public static ECANHelper ins = new ECANHelper();

        public static Logger logger = LogManager.GetLogger("ECANHelper");

        private ECANHelper() { }

        public event EventHandler<CommStateEventArgs> eventStateChanged;

        public event EventHandler<ByteEventArgs> eventSended;
        public event EventHandler<ByteEventArgs> eventReceived;
        public event EventHandler<BoxStateEventArgs> eventBoxStateChanged;

        //发送模式, 0正常模式, 1只听模式, 2 自发自收模式
        private byte model = 0;
        //发送帧类型。=0时为正常发送，=1时为单次发送（不自动重发），=2时 为自发自收（用于测试CAN卡是否损坏），=3时为单次自发自收（只发送一 次，用于自测试）
        private byte sendType = 0;

        public ConcurrentQueue<CanDataWithInfo> queueSendBuffer = new ConcurrentQueue<CanDataWithInfo>();
        public ConcurrentQueue<CanDataWithInfo> queueReceiveBuffer = new ConcurrentQueue<CanDataWithInfo>();

        private bool receiveRunning = false;
        private bool sendRunning = false;
        private bool analysisRunning = false;

        public void openDevice() {
            try {
                INIT_CONFIG init_config = new INIT_CONFIG();

                init_config.AccCode = 0;
                init_config.AccMask = 0xffffff;
                init_config.Filter = 0;
                init_config.Timing0 = 0x00;
                init_config.Timing1 = 0x1c;
                init_config.Mode = model;
                if (ECANDLL.OpenDevice(4, 0, 0) != ECAN.ECANStatus.STATUS_OK) {
                    openResult(false);
                    return;
                }
                //Set can1 baud
                if (ECANDLL.InitCAN(4, 0, 0, ref init_config) != ECAN.ECANStatus.STATUS_OK) {
                    openResult(false);
                    ECANDLL.CloseDevice(1, 0);
                    return;
                }
                if (ECANDLL.StartCAN(1, 0, 0) == ECAN.ECANStatus.STATUS_OK) {
                    //MessageBox.Show("Start CAN1 Success");
                    openResult(true);
                } else {
                    openResult(false);
                }
            }catch(Exception e) {
                logger.Error("ECAN 打开失败: " + e);
            }
        }

        public void closeDevice() {
            try {
                ECANDLL.CloseDevice(4, 0);
            } catch (Exception) { }
            closeResult(true);
        }

        private void openResult(bool result) {
            if (null != eventStateChanged) {
                CommStateEventArgs args = new CommStateEventArgs();
                args.opened = result;
                if (args.opened) {
                    args.message = "打开成功";
                    startReceive();
                    startAnalysis();
                    startSend();
                } else {
                    args.message = "打开失败";
                }
                eventStateChanged(this, args);
            }
        }

        private void closeResult(bool result) {
            if (null != eventStateChanged) {
                CommStateEventArgs args = new CommStateEventArgs();
                args.opened = !result;
                if (!args.opened) {
                    args.message = "已关闭";
                    stopReceive();
                    stopAnalysis();
                    stopSend();
                } else {
                    args.message = "关闭失败";
                }
                eventStateChanged(this, args);
            }
        }

        public void startReceive() {
            if (!receiveRunning) {
                receiveRunning = true;
                ThreadPool.QueueUserWorkItem(new WaitCallback(receive), null);
            }
        }

        public void stopReceive() {
            receiveRunning = false;
        }

        private void receive(object obj) {
            CAN_OBJ vco1 = new CAN_OBJ();
            while (receiveRunning) {
                //CAN_OBJ vco1 = new CAN_OBJ();
                uint result = ECANDLL.Receive(4, 0, 0, out vco1, 1, 10000);
                //Console.WriteLine("GetReceiveNum result : " + result);
                Console.WriteLine("GetReceiveNum result data: " + Untils.ToHexString(vco1.data));
                if (result != 0xFFFFFFFF && result != 0) {
                    CanDataWithInfo can = new CanDataWithInfo(vco1.ID, vco1.data, "");
                    queueReceiveBuffer.Enqueue(can);
                } else {
                    //Console.WriteLine("receive fault");
                }
                Thread.Sleep(10);
            }
        }

        public void startAnalysis() {
            if (!analysisRunning) {
                analysisRunning = true;
                ThreadPool.QueueUserWorkItem(new WaitCallback(analysis));
            }
        }

        public void stopAnalysis() {
            analysisRunning = false;
        }

        private void analysis(object obj) {
            while (analysisRunning) {
                while (!queueReceiveBuffer.IsEmpty) {
                    CanDataWithInfo bi = null;
                    bool result = queueReceiveBuffer.TryDequeue(out bi);
                    if (!result || null == bi) {
                        continue;
                    }
                    if (bi.canId == 0) {
                        continue;
                    }
                    ByteEventArgs byteEventArgs = new ByteEventArgs(DateTime.Now, new BytesWithInfo(bi.bytes, ""));
                    eventReceived?.Invoke(this, byteEventArgs);
                    
                    //写入文件记录
                    logger.Info(byteEventArgs.getBytesString());

                    if (bi.canId == 0x04) {
                        //读6个字节的数据
                        byte[] byData = bi.bytes;
                        int boxId = byData[1] << 8 | byData[0];
                        int boxState = byData[2];
                        eventBoxStateChanged?.Invoke(this, new BoxStateEventArgs(boxId, boxState));
                    } else if (bi.canId == 0x06) {
                        //第一个视频开始播放命令
                        LightBoxHelper.videoBegin = true;
                    }
                    Thread.Sleep(1);
                }
                Thread.Sleep(1);
            }
        }

        public void startSend() {
            if (!sendRunning) {
                sendRunning = true;
                ThreadPool.QueueUserWorkItem(new WaitCallback(send));
            }
        }

        public void stopSend() {
            sendRunning = false;
        }

        private void send(object obj) {
            CAN_OBJ frameinfo = new CAN_OBJ();
            while (sendRunning) {
                while (!queueSendBuffer.IsEmpty) {
                    CanDataWithInfo bi = null;
                    
                    bool result = queueSendBuffer.TryDequeue(out bi);
                    //Console.WriteLine("queue count : " + queueSendBuffer.Count);
                    if (!result || null == bi) {
                        continue;
                    }
                    byte[] bySend;
                    if (bi.bytes.Length != 8) {
                        bySend = new byte[8];
                        Array.Copy(bi.bytes, 0, bySend, 0, bi.bytes.Length);
                    } else {
                        bySend = bi.bytes;
                    }
                    frameinfo.ID = bi.canId;
                    frameinfo.SendType = sendType;
                    frameinfo.RemoteFlag = 0;
                    frameinfo.ExternFlag = 0;
                    frameinfo.DataLen = (byte)bi.bytes.Length;
                    frameinfo.data = bySend;
                    //frameinfo.Reserved = new byte[2];
                    //logger.Info(Untils.ToHexString(frameinfo.data) + bi.info);
                    //Console.WriteLine(bi.info);
                    sendMsg(frameinfo, bi);
                    Thread.Sleep(1);
                }
                Thread.Sleep(5);
            }
        }

        private void sendMsg(CAN_OBJ frameinfo, CanDataWithInfo bi) {

            if (ECANDLL.Transmit(4, 0, 0, ref frameinfo, (ushort)1) != ECANStatus.STATUS_OK) {
                Console.WriteLine("Transmit : fault");
                ERR_INFO info = new ERR_INFO();
                if (ECANDLL.ReadErrInfo(4, 0, 0, out info) != ECANStatus.STATUS_OK) {
                    Console.WriteLine("ReadErrInfo : fault");
                } else {
                    Console.WriteLine("ReadErrInfo : code " + info.ErrCode);
                }
            } else {
                //写入文件记录
                //logger.Info(Untils.ToHexString(frameinfo.data) + bi.info);
                eventSended?.Invoke(this, new ByteEventArgs(DateTime.Now, new BytesWithInfo(frameinfo.data, bi.info)));
            }
        }

        public void sendMessageWithInfo(uint canId, byte[] by, string info) {
            CanDataWithInfo bi = new CanDataWithInfo(canId, by, info);
            queueSendBuffer.Enqueue(bi);
        }

        public void sendMessageWithInfo(CanDataWithInfo bi) {
            queueSendBuffer.Enqueue(bi);
        }
    }
}
