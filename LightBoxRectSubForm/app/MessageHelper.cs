using LightBoxRectSubForm.data;
using LightBoxRectSubForm.dll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightBoxRectSubForm.app {

    public class MessageHelper {

        public static MessageHelper ins = new MessageHelper();

        private int boxEndCount;

        private MessageHelper() { }

        public void beginModel() {
            boxEndCount = 0;
        }

        public bool modelIsEnd() {
            return boxEndCount >= BaseConfig.ins.boxCount;
        }

        public void searchBoxes() {
            byte[] byData = new byte[3] { 0x06, 0, 0};
            CanDataWithInfo can = new CanDataWithInfo(0x01, byData, "");
            ECANHelper.ins.sendMessageWithInfo(can);
        }

        /// <summary>
        /// 设置灯箱ID
        /// </summary>
        /// <param name="oldId"></param>
        /// <param name="newId"></param>
        public CanDataWithInfo setBoxId(int oldId, int newId) {

            byte[] byData = new byte[8];
            byData[0] = 1;
            byData[1] = (byte)oldId;
            byData[2] = (byte)(oldId >> 8);

            byData[3] = (byte)newId;
            byData[4] = (byte)(newId >> 8);

            CanDataWithInfo can = new CanDataWithInfo(0x03, byData, "");
            ECANHelper.ins.sendMessageWithInfo(can);
            return can;
        }

        public void getBoxId() {
            byte[] byDate = new byte[3];
            byDate[0] = 6;
            CanDataWithInfo can = new CanDataWithInfo(0x01, byDate, "");
            ECANHelper.ins.sendMessageWithInfo(can);
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
            CanDataWithInfo can = createRunBySend(boxColumnInCanIdTable, canId, 1, runTime);
            can.info = "id:" + lBMsg.Id + " 正 time:" + runTime;
            ECANHelper.ins.sendMessageWithInfo(can);
            //Console.WriteLine("send " + Untils.ToHexString(can.bytes));
            //sendMessageWithInfo(bySend, "id:" + lBMsg.Id + " 正");
            //Console.WriteLine("id:" + lBMsg.Id + "-正");

           // if (runTime > 1) {
           //     Thread.Sleep((runTime - 1) * 1000);
           // }
           // //停留
           // CanDataWithInfo can1 = createRunBySend(boxColumnInCanIdTable, canId, 3, keepTime);
           // can1.info = "id:" + lBMsg.Id + " 停";
           // ECANHelper.ins.sendMessageWithInfo(can1);
           //// Console.WriteLine("send " + Untils.ToHexString(can1.bytes));
           // //sendMessageWithInfo(bySend1, "id:" + lBMsg.Id + " 停");
           // //Console.WriteLine("id:" + lBMsg.Id + "-停");

           // if (keepTime > 1) {
           //     Thread.Sleep((keepTime - 1) * 1000);
           // }

            Thread.Sleep((runTime + keepTime) * 1000);
            //反转, 反转时间+2 , 保证转到底
            CanDataWithInfo can2 = createRunBySend(boxColumnInCanIdTable, canId, 2, runTime + 2);
            can2.info = "id:" + lBMsg.Id + " 反 ";
            ECANHelper.ins.sendMessageWithInfo(can2);
            //Thread.Sleep(runTime * 1000);
            //boxEndCount = boxEndCount + 1;
            //Console.WriteLine("boxEndCount " + boxEndCount);
            //sendMessageWithInfo(bySend2, "id:" + lBMsg.Id + " 反");
            //Console.WriteLine("id:" + lBMsg.Id + "-反");

        }

        public void runBoxRepeat(object obj) {
            LBMsg lBMsg = obj as LBMsg;
            if (lBMsg.RunTime > 0) {
                int canId = createCanId(lBMsg.Id);
                Thread.Sleep((int)lBMsg.WaitTime * 1000);
                for (int i = 0; i < lBMsg.RepeatCount; i++) {
                    byte[] by = new byte[8];
                    //正转时间
                    int runTime = (int)(lBMsg.RunTime);
                    //保持时间
                    int keepTime = (int)lBMsg.KeepTime;

                    //灯箱属于 canid-电机id对应表中的那一列, 共4列
                    int boxColumnInCanIdTable = createBoxByteIndex(lBMsg.Id);

                    //正转
                    //运转命令 00保留, 01正转, 10反转, 11停留
                    CanDataWithInfo can = createRunBySend(boxColumnInCanIdTable, canId, 1, runTime);
                    can.info = "id:" + lBMsg.Id + " 正 time:" + runTime;
                    ECANHelper.ins.sendMessageWithInfo(can);

                    Thread.Sleep((runTime + keepTime) * 1000);

                    //反转, 反转时间+2 , 保证转到底
                    CanDataWithInfo can2 = createRunBySend(boxColumnInCanIdTable, canId, 2, runTime + 1);
                    can2.info = "id:" + lBMsg.Id + " 反 ";
                    ECANHelper.ins.sendMessageWithInfo(can2);
                    Thread.Sleep((runTime + 1) * 1000);
                }
            }
            boxEndCount = boxEndCount + 1;
        }

        public void startRunAllBox(LBMsg box) {
            ThreadPool.QueueUserWorkItem(new WaitCallback(runBox), box);
        }
        //public void searchErr() {
        //    foreach(LBMsg msg in LightBoxHelper.SelectedModel.ListLBMsg) {
        //        int canId = createCanId(msg.Id);
        //        byte[] byDate = new byte[3];
        //        byDate[0] = 4;
        //        byDate[1]
        //        CanDataWithInfo can = new CanDataWithInfo(0x01, byDate, "");
        //        ECANHelper.ins.sendMessageWithInfo(can);

        //    }
        //}

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
        /// 计算生成控制灯箱命令的数据部分
        /// </summary>
        /// <param name="boxIndex">灯箱在canid对应表中的那一列</param>
        /// <param name="canId"></param>
        /// <param name="runOrder">运行命令 00保留, 01正转, 10反转, 11停留</param>
        /// <param name="time">运行时间, 单位秒</param>
        /// <returns></returns>
        private CanDataWithInfo createRunBySend(int boxIndex, int canId, byte runOrder, int time) {
            byte[] by = new byte[8];
            byte[] byBox = new byte[2];

            byBox[0] = (byte)time;
            byBox[1] = (byte)(runOrder << 6 | time >> 10);
            by[boxIndex + 1] = byBox[1];
            by[boxIndex] = byBox[0];
            CanDataWithInfo can = new CanDataWithInfo((uint)canId, by, "");
            return can;
        }
    }
}
