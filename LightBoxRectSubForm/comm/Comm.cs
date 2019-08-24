using LightBoxRectSubForm.app;
using LightBoxRectSubForm.data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightBoxRectSubForm.comm
{
    public abstract class Comm
    {
        /// <summary>
        /// 串口名
        /// </summary>
        public static string uartNameKey = "uartName";
        /// <summary>
        /// 波特率
        /// </summary>
        public static string uartBandRate = "bandRate";
        /// <summary>
        /// 字节位数
        /// </summary>
        public static string uartDataBit = "dataBit";
        /// <summary>
        /// 停止位
        /// </summary>
        public static string uartStopBit = "stopBit";
        /// <summary>
        /// 校验
        /// </summary>
        public static string uartVerify = "verify";
        
        public event EventHandler<CommStateEventArgs> eventStateChanged;

        public event EventHandler<ByteEventArgs> eventSended;
        public event EventHandler<ByteEventArgs> eventReceived;

        private SerialPort serial;
        public string uartName;
        public int bandRate;
        public int dataBit;
        public int stopBit;
        public int verify = -1;
        private bool sendThreaIsRunning = false;

        public bool analysisRunning = false;

        public ConcurrentQueue<BytesWithInfo> queueSendBuffer = new ConcurrentQueue<BytesWithInfo>();

        //接收缓存
        public byte[] receiveBuffer = new byte[1024];
        //收缓存的写指针
        public int writeIndex = 0;
        //写指针回头
        public bool writeIndexReturned = false;
        //收缓存的读指针
        public int readIndex = 0;

        public string UartNote { get; set; } = "UART";

        public bool isOpen() {
            if (null == serial) {
                return false;
            }
            return serial.IsOpen;
        }

        public void open() {
            if (null == serial) {
                openResult(false);
            }
            try {
                serial.Open();
                openResult(isOpen());
                startSendThread();
            } catch (Exception e) {
                openResult(false);
            }
        }

        private void startSendThread() {
            if (!sendThreaIsRunning) {
                sendThreaIsRunning = true;
                ThreadPool.QueueUserWorkItem(new WaitCallback(sendBytesWithInfo));
            }
        }


        private void openResult(bool result) {
            if (null != eventStateChanged) {
                CommStateEventArgs args = new CommStateEventArgs();
                args.opened = result;
                if (args.opened) {
                    args.message = "打开成功";
                    serial.DataReceived += Serial_DataReceived;
                } else {
                    args.message = "打开失败";
                }
                eventStateChanged(this, args);
            }
        }

        private void closeResult(bool result) {
            if (null != eventStateChanged) {
                CommStateEventArgs args = new CommStateEventArgs();
                args.opened = isOpen();
                if (!args.opened) {
                    args.message = "已关闭";
                    serial.DataReceived += Serial_DataReceived;
                } else {
                    args.message = "关闭失败";
                }
                eventStateChanged(this, args);
            }
        }

        /// <summary>
        /// 是否有可读字节
        /// </summary>
        /// <returns></returns>
        public bool haveByteToRead() {
            if (writeIndexReturned) {
                return true;
            } else if (readIndex < writeIndex) {
                return true;
            }
            return false;
        }

        private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e) {
            int count = serial.BytesToRead;
            if (count <= 0)
                return;
            byte[] by = new byte[count];
            serial.Read(by, 0, count);
            writeToBuffer(by);
            ReceivedData(by);
        }

        //将字节数组写入缓存, 从写指针所在的位置开始写
        private void writeToBuffer(byte[] bys) {
            foreach (byte by in bys) {
                writeByteToBuffer(by);
            }
        }

        //将一个字节写入缓存, 位置为写指针的位置, 写完指针+1
        private void writeByteToBuffer(byte by) {
            if (writeIndex < receiveBuffer.Length) {
                receiveBuffer[writeIndex] = by;
                writeIndex++;
            } else {
                writeIndex = 0;
                receiveBuffer[writeIndex] = by;
                writeIndexReturned = true;
                writeIndex++;
            }
        }

        public byte readByte() {
            //阻塞, 如果没有可读字节, 等待
            while (!haveByteToRead()) { continue; }
            byte by = receiveBuffer[readIndex];

            //如果读到了最后一个, 读指针回头, 写指针回头为false
            //否则读指针+1
            if (readIndex == receiveBuffer.Length - 1) {
                readIndex = 0;
                writeIndexReturned = false;
            } else {
                readIndex++;
            }
            return by;
        }

        public byte[] readByte(int readLength) {
            byte[] bys = new byte[readLength];
            for (int i = 0; i < readLength; i++) {
                bys[i] = readByte();
            }
            return bys;
        }

        public virtual void ReceivedData(byte[] by) {
            ThreadPool.QueueUserWorkItem(new WaitCallback(notifyReceivedData), by);
            if (!analysisRunning) {
                analysisRunning = true;
                ThreadPool.QueueUserWorkItem(new WaitCallback(analysis), by);
            }
        }

        public void notifyReceivedData(object obj) {
            eventReceived?.Invoke(this, new ByteEventArgs(DateTime.Now, new BytesWithInfo(obj as byte[], "")));
        }

        public abstract void analysis(object obj);

        public void close() {
            sendThreaIsRunning = false;
            if (null != serial) {
                serial.DataReceived -= Serial_DataReceived;
                try {
                    if (serial.IsOpen) {
                        serial.Close();
                        closeResult(true);
                    }
                } catch (Exception e) {
                    closeResult(false);
                }
            }
        }

        /// <summary>
        /// 发送字节数据, 开启线程发送
        /// </summary>
        /// <param name="by"></param>
        public void sendMessage(byte[] by) {
            if (null != serial && null != by && by.Length > 0) {
                ThreadPool.QueueUserWorkItem(new WaitCallback(sendBytes), by);
            }
        }

        //发送字节数据
        private void sendBytes(object obj) {
            BytesWithInfo bi = obj as BytesWithInfo;
            bi.bytes = obj as byte[];
            try {
                serial.Write(bi.bytes, 0, bi.bytes.Length);
                eventSended?.Invoke(this, new ByteEventArgs(DateTime.Now, bi));
            } catch (Exception e) {
                CommStateEventArgs args = new CommStateEventArgs();
                args.opened = false;
                args.message = "发送异常, 请检查串口是否正常";
                eventStateChanged?.Invoke(this, args);
            }
        }

        /// <summary>
        /// 发送字节数据, 开启线程发送
        /// </summary>
        /// <param name="by"></param>
        public void sendMessageWithInfo(byte[] by, string info) {
            //Console.WriteLine("comm id:" + info);
            BytesWithInfo bi = new BytesWithInfo(by, info);
            queueSendBuffer.Enqueue(bi);
            //if (!sendThreaIsRunning) {
            //    sendThreaIsRunning = true;
            //    ThreadPool.QueueUserWorkItem(new WaitCallback(sendBytesWithInfo));
            //}
        }
        
        //发送字节数据
        private void sendBytesWithInfo(object obj) {
            while (sendThreaIsRunning) {
                while (!queueSendBuffer.IsEmpty) {
                    BytesWithInfo bi = null;
                    bool result = queueSendBuffer.TryDequeue(out bi);
                    //Console.WriteLine("queue count : " + queueSendBuffer.Count);
                    if (!result || null == bi) {
                        continue;
                    }
                    try {
                        serial.Write(bi.bytes, 0, bi.bytes.Length);
                        eventSended?.Invoke(this, new ByteEventArgs(DateTime.Now, bi));
                    } catch (Exception e) {
                        CommStateEventArgs args = new CommStateEventArgs();
                        args.opened = false;
                        args.message = "发送异常, 请检查串口是否正常 : " + e.Message;
                        eventStateChanged?.Invoke(this, args);
                    }
                    Thread.Sleep(5);
                }
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// 发送字符串数据, 开启线程发送
        /// </summary>
        /// <param name="by"></param>
        public void sendMessage(string str) {
            if (null != serial && null != str && str.Length > 0) {
                ThreadPool.QueueUserWorkItem(new WaitCallback(sendString), str);
            }
        }
        //发送字符串
        private void sendString(object obj) {
            if (null != serial && serial.IsOpen) {
                string str = obj as string;
                serial.Write(str);
            }
        }

        private void initData() {
            uartName = ConfigHelper.ReadConfig(UartNote, uartNameKey);
            bandRate = Convert.ToInt32(ConfigHelper.ReadConfig(UartNote, uartBandRate));
            dataBit = Convert.ToInt16(ConfigHelper.ReadConfig(UartNote, uartDataBit));
            stopBit = Convert.ToInt16(ConfigHelper.ReadConfig(UartNote, uartStopBit));
            verify = Convert.ToInt16(ConfigHelper.ReadConfig(UartNote, uartVerify));
        }

        public void init() {
            initData();
            serial = new SerialPort();

            refreshConfig();
            serial.WriteBufferSize = 10000;
            serial.ReadTimeout = 1500;
            serial.WriteTimeout = 1500;

            startSendThread();
        }

        public void refreshConfig() {
            serial.PortName = uartName;
            //波特率
            serial.BaudRate = bandRate;
            //数据位
            serial.DataBits = dataBit;
            //停止位
            if (stopBit == 1) {
                serial.StopBits = System.IO.Ports.StopBits.One;
            } else {
                serial.StopBits = System.IO.Ports.StopBits.Two;
            }
            //奇偶校验位
            if (verify == 0) {
                serial.Parity = System.IO.Ports.Parity.None;
            } else if (verify == 1) {
                serial.Parity = System.IO.Ports.Parity.Odd;
            } else {
                serial.Parity = System.IO.Ports.Parity.Even;
            }
        }

        public void save() {
            ConfigHelper.WriteConfig(UartNote, uartNameKey, uartName);
            ConfigHelper.WriteConfig(UartNote, uartBandRate, Convert.ToString(bandRate));
            ConfigHelper.WriteConfig(UartNote, uartDataBit, Convert.ToString(dataBit));
            ConfigHelper.WriteConfig(UartNote, uartStopBit, Convert.ToString(stopBit));
            ConfigHelper.WriteConfig(UartNote, uartVerify, Convert.ToString(verify));
        }
    }
}
