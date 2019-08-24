using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightBoxRectSubForm.comm {

    /// <summary>
    /// 短信模块串口
    /// </summary>
    public class SmsComm : Comm{

        public static SmsComm ins = new SmsComm();

        public event EventHandler<SmsCommDtuState> eventDtuSetState;
        private bool received = false;
        private bool isSetting = false;

        private SmsComm() {
            UartNote = "UART_SMS";
        }

        public override void ReceivedData(byte[] by) {
            base.ReceivedData(by);
            received = true;
        }

        public override void analysis(object obj) {
                byte[] by = obj as byte[];
                string str = Encoding.ASCII.GetString(by);
                Debug.WriteLine("recd: " + str, "SmsComm");
                dtuStatePush("recd: " + str);
                if (str.Contains("+WKMOD") && str.Length >= 12) {
                    int index = str.IndexOf("\"") + 1;
                    int lastIndex = str.LastIndexOf("\"") - index;
                    if (index > 0 && lastIndex > index) {
                        string smsModel = str.Substring(index, str.LastIndexOf("\"") - index);
                        dtuStatePush("透传模式:" + smsModel);
                    }
                } else if (str.Contains("+DSTNUM") && str.Length >= 12) {
                    int index = str.IndexOf("\"") + 1;
                    int lastIndex = str.LastIndexOf("\"") - index;
                    if (index > 0 && lastIndex > index) {
                        string smsModel = str.Substring(index, str.LastIndexOf("\"") - index);
                        dtuStatePush("手机号码:" + smsModel);
                    }
                }
            analysisRunning = false;
        }

        /// <summary>
        /// 获取目标手机号码
        /// </summary>
        public void getTelNumInThread(string telNum) {
            if (checkIsOpened()) {
                ThreadPool.QueueUserWorkItem(new WaitCallback(getTelNum));
            }
        }
        
        // 获取目标手机号码
        public void getTelNum(object obj) {
            isSetting = true;
            
            try {
                toAt();
                //发送查询目标号码指令, 返回一行+DSTNUM:"xxx", 和一行OK
                sendAndWait("AT+DSTNUM\r\n");
                exitAt();
            } catch (TimeoutException) { }
            isSetting = false;
        }

        /// <summary>
        /// 设置手机号码
        /// </summary>
        /// <param name="telNum"></param>
        public void setTelNumInThread(string telNum) {
            if (checkIsOpened()) {
                ThreadPool.QueueUserWorkItem(new WaitCallback(setTelNum), telNum);
            }
        }
        
        // 设置手机号码
        private void setTelNum(object obj) {
            string telNum = obj as string;
            bool res = false;
            isSetting = true;
            try {
                toAt();
                //发送设置目标号码指令
                sendAndWait("AT+DSTNUM=\"" + telNum + "\"\r\n");
                sendSaveAt();
                res = true;
            } catch (TimeoutException) {
                res = false;
            }
            if (res) {
                dtuStatePush("设置手机号码成功");
            } else {
                dtuStatePush("设置手机号码失败");
            }
            isSetting = false;
        }

        /// <summary>
        /// 开线程获取透传模式
        /// </summary>
        public void getSmsModelInThread() {
            if (checkIsOpened()) {
                ThreadPool.QueueUserWorkItem(new WaitCallback(getSmsModel));
            }
        }
        
        // 获取工作模式
        public void getSmsModel(object obj) {
            isSetting = true;
            try {
                toAt();
                //发送查询目标号码指令, 返回一行+WKMOD:"SMS"
                sendAndWait("AT+WKMOD\r\n");
                exitAt();
            } catch (TimeoutException) {

            }
            isSetting = false;
        }

        /// <summary>
        /// 开线程设置为短信模式
        /// </summary>
        public void setSmsModelInThread() {
            if (checkIsOpened()) {
                ThreadPool.QueueUserWorkItem(new WaitCallback(setSmsModel));
            }
        }
        
        // 设置为短信模式
        private void setSmsModel(object obj) {
            isSetting = true;
            bool res = false;
            try {
                toAt();
                //发送设置目标号码指令
                sendAndWait("AT+WKMOD=\"SMS\"\r\n");
                sendSaveAt();
                res = true;
            } catch (TimeoutException) {
                res = false;
            }
            isSetting = false;
            if (res) {
                dtuStatePush("配置短信透传模式成功");
            } else {
                dtuStatePush("配置短信透传模式失败");
            }
        }

        private bool checkIsOpened() {
            if (!isOpen()) {
                dtuStatePush("串口未打开");
                return false;
            }
            return false;
        }

        private void dtuStatePush(string msg) {
            if(null != eventDtuSetState) {
                SmsCommDtuState smsCommDtuState = new SmsCommDtuState();
                smsCommDtuState.message = msg;
                eventDtuSetState(this, smsCommDtuState);
            }
        }

        public void toAt() {
            //进入AT指令模式
            //发送+++, 读取a
            sendAndWait("+++");
            //发送a, 读取+ok
            sendAndWait("a");
        }

        private void sendSaveAt() {
            //发送保存指令, 读取一行数据OK\r\n
            sendAndWait("AT+S\r\n");
        }

        private void exitAt() {
            //退出AT指令模式, 返回+OK
            sendAndWait("AT+ENTM\r\n");
        }

        //发送信息,并等待返回, 等待超时10s
        private void sendAndWait(string msg) {
            received = false;
            sendMessage(msg);
            waitToReceived();
        }

        private void waitToReceived() {
            int i = 0;
            while (!received) {
                Thread.Sleep(100);
                i++;
                if (i > 100) {
                    return;
                }
            }
        }
    }
}
