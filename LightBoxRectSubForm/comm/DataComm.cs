using LightBoxRectSubForm.app;
using LightBoxRectSubForm.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightBoxRectSubForm.comm {

    /// <summary>
    /// 灯箱数据串口
    /// </summary>
    public class DataComm : Comm {

        public event EventHandler<BoxStateEventArgs> eventBoxStateChanged;

        public static DataComm ins = new DataComm();

        private DataComm() {
            UartNote = "UART";
        }

        /// <summary>
        /// 解析收到的数据
        /// </summary>
        /// <param name="obj"></param>
        public override void analysis(object obj) {
            while (haveByteToRead()) {
                byte by = readByte();
                //是否以 40 00 00 开头
                if (by == 0x40) {
                    int b1 = readByte();
                    int b2 = readByte();
                    if (b1 == 0 && b2 == 0) {
                        //canid 是否是04
                        int canIdH = readByte();
                        int canIdL = readByte();
                        int canId = canIdH << 8 | canIdL;
                        if (canId == 0x04) {
                            //读取数据长度
                            int len = readByte();
                            //读6个字节的数据
                            byte[] byData = readByte(len);
                            int boxId = byData[1] << 8 | byData[0];
                            int boxState = byData[2];
                            eventBoxStateChanged?.Invoke(this, new BoxStateEventArgs(boxId, boxState));
                        } else if (canId == 0x06) {
                            //第一个视频开始播放命令
                            LightBoxHelper.videoBegin = true;
                        }
                    }
                }else if (by == 0xFe) {
                    byte b1 = readByte();
                    byte b2 = readByte();
                    if (b1 == 1 && b2 == 0x40) {
                        //第一个视频开始播放命令
                        LightBoxHelper.videoBegin = true;
                    }
                }
            }
            analysisRunning = false;
        }

        /// <summary>
        /// 设置灯箱ID
        /// </summary>
        /// <param name="oldId"></param>
        /// <param name="newId"></param>
        public void setBoxId(int oldId, int newId) {

            byte[] byData = new byte[8];
            byData[0] = 1;
            byData[1] = (byte)oldId;
            byData[2] = (byte)(newId >> 8);

            byData[3] = (byte)newId;
            byData[4] = (byte)(newId >> 8);

            byte[] bySend = createBytes(0x03, byData);
            sendMessage(bySend);

        }

        /// <summary>
        /// 查询单个电机的状态
        /// </summary>
        /// <param name="id"></param>
        public void getBoxState(int id) {
            byte[] byDate = new byte[3];
            byDate[0] = 4;
            byDate[1] = (byte)id;
            byDate[2] = (byte)(id >> 8);
            byte[] bySend = createBytes(0x01, byDate);
            sendMessage(bySend);
        }

        /// <summary>
        /// 开线程发送灯箱运行命令, 灯箱运行一个周期的所有命令, 包括正转\ 停留\ 反转
        /// </summary>
        /// <param name="box"></param>
        public void startRunBox(LBMsg box) {
            ThreadPool.QueueUserWorkItem(new WaitCallback(runBox), box);
        }

        //发送字节数据
        public void runBox(object obj) {
            LBMsg lBMsg = obj as LBMsg;
            int canId = createCanId(lBMsg.Id);
            byte[] by = new byte[8];
            //正转时间
            int runTime = (int)(lBMsg.RunTime);
            //保持时间
            int keepTime = (int)lBMsg.KeepTime;

            //Console.WriteLine("id:" + lBMsg.Id +"-" +  runTime + ":" + keepTime);
            //灯箱属于 canid-电机id对应表中的那一列, 共4列
            int boxColumnInCanIdTable = createBoxByteIndex(lBMsg.Id);

            //正转
            //运转命令 00保留, 01正转, 10反转, 11停留
            byte[] bySend = createRunBySend(boxColumnInCanIdTable, canId, 1, runTime);
            sendMessageWithInfo(bySend, "id:" + lBMsg.Id + " 正");
            //Console.WriteLine("id:" + lBMsg.Id + "-正");

            if (runTime > 1) {
                Thread.Sleep((runTime - 1) * 1000);
            }
            //停留
            byte[] bySend1 = createRunBySend(boxColumnInCanIdTable, canId, 3, keepTime);
            sendMessageWithInfo(bySend1, "id:" + lBMsg.Id + " 停");
            //Console.WriteLine("id:" + lBMsg.Id + "-停");

            if (keepTime > 1) {
                Thread.Sleep((keepTime - 1) * 1000);
            }
            //反转
            byte[] bySend2 = createRunBySend(boxColumnInCanIdTable, canId, 2, runTime);
            sendMessageWithInfo(bySend2, "id:" + lBMsg.Id + " 反");
            //Console.WriteLine("id:" + lBMsg.Id + "-反");

        }

        /// <summary>
        /// 计算生成控制灯箱命令的数据部分
        /// </summary>
        /// <param name="boxIndex">灯箱在canid对应表中的那一列</param>
        /// <param name="canId"></param>
        /// <param name="runOrder">运行命令 00保留, 01正转, 10反转, 11停留</param>
        /// <param name="time">运行时间, 单位秒</param>
        /// <returns></returns>
        private byte[] createRunBySend(int boxIndex, int canId, byte runOrder, int time) {
            byte[] by = new byte[8];
            byte[] byBox = new byte[2];

            byBox[0] = (byte)time;
            byBox[1] = (byte)(runOrder << 6 | time >> 10);
            by[boxIndex + 1] = byBox[1];
            by[boxIndex] = byBox[0];
            byte[] bySend = createBytes(canId, by);
            return bySend;
        }

        /// <summary>
        /// 根据灯箱id计算canid, 4个灯箱公用一个canid
        /// </summary>
        /// <param name="boxId"></param>
        /// <returns></returns>
        private int createCanId(int boxId) {
            int canId = 0;
            int num = boxId / 4;
            //if (boxId % 4 == 0) {
            //    num -= 1;
            //}
            canId = 0x100 + num;
            return canId;
        }

        /// <summary>
        /// 计算灯箱在canid对于表中的那一列, 共4列
        /// </summary>
        /// <param name="boxId"></param>
        /// <returns></returns>
        private int createBoxByteIndex(int boxId) {
            int index = boxId % 4;
            //if(index == 0) {
            //    index = 4;
            //}
            return index * 2;
        }

        /// <summary>
        /// 生成可以发送的字节数组
        /// </summary>
        /// <param name="canId"></param>
        /// <param name="byData">数据字节数组</param>
        /// <returns></returns>
        private byte[] createBytes(int canId, byte[] byData) {
            int dataLen = byData.Length;
            byte[] bySend = new byte[6 + dataLen];
            bySend[0] = 0x40;
            bySend[1] = 0;
            bySend[2] = 0;
            bySend[3] = (byte)(canId >> 8);
            bySend[4] = (byte)canId;
            bySend[5] = (byte)dataLen;

            Array.Copy(byData, 0, bySend, 6, dataLen);
            return bySend;
        }
    }
}
