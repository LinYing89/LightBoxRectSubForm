using LightBoxRectSubForm.comm;
using LightBoxRectSubForm.data;
using LightBoxRectSubForm.dll;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightBoxRectSubForm.app {

    public class LightBoxHelper {

        public static event EventHandler modelChanged;
        public static Logger logger = LogManager.GetLogger("LightBoxHelper");

        public static bool isRunning;
        public static bool videoBegin = false;
        public static bool quit = false;
        public static bool modelChanging = false;

        private static LBModel selectedModel;
        public static int modelIndex = -1;
        public static List<LBModel> listModel = new List<LBModel>();

        private static Thread th;

        //线程是否临时等待
        private static bool WAITED = false;
        //线程是否临临时运行一次
        private static bool RUN_ONCE = false;
        //当前模式结束后暂停
        private static bool PUSED_WHEN_MODEL_OVER = false;

        public static LBModel SelectedModel {
            get {
                if (null == selectedModel) {
                    selectedModel = createInitModel();
                    //if (listModel.Count > 0) {
                    //    modelIndex = 0;
                    //    setModelSelected(listModel[0]);
                    //}
                }
                return selectedModel;
            }

            set {
                selectedModel = value;
            }
        }

        /// <summary>
        /// 删除模式
        /// </summary>
        /// <param name="modelName"></param>
        public static void delModel(string modelName) {
            foreach (LBModel model in listModel) {
                if (model.ModelName.Equals(modelName)) {
                    listModel.Remove(model);
                    FileInfo file = new FileInfo(WriteXmlHelper.FILE_MODEL + modelName + ".xml");
                    file.Delete();
                    modelIndex = -1;
                    SelectedModel = null;
                    break;
                }
            }
        }

        public static void modelRename(string oldName, string newName) {
            foreach (LBModel model in listModel) {
                if (model.ModelName.Equals(oldName)) {
                    model.ModelName = newName;
                    FileInfo fileInfo = new FileInfo(WriteXmlHelper.FILE_MODEL + oldName + ".xml");
                    fileInfo.MoveTo(WriteXmlHelper.FILE_MODEL + newName + ".xml");
                    break;
                }
            }
        }

        public static bool modelNameIsHaved(string newName) {
            foreach (LBModel model in listModel) {
                if (model.ModelName.Equals(newName)) {
                    return true;
                }
            }
            return false;
        }

        public static void updateModelLBMsg(LBMsg lBMsg) {
            foreach (LBModel model in listModel) {
                if (model.ModelName.Equals(SelectedModel.ModelName)) {
                    LBMsg msg = model.getLightBoxMsg(lBMsg.Id);
                    msg.RunTime = lBMsg.RunTime;
                    msg.KeepTime = lBMsg.KeepTime;
                    msg.WaitTime = lBMsg.WaitTime;
                    break;
                }
            }
        }

        public static void updateModel(LBModel lbmodel) {
            foreach (LBModel model in listModel) {
                if (model.ModelName.Equals(lbmodel.ModelName)) {
                    model.MWaitTime.WaitMM = lbmodel.MWaitTime.WaitMM;
                    model.MWaitTime.WaitSS = lbmodel.MWaitTime.WaitSS;
                    model.runAll = lbmodel.runAll;
                    break;
                }
            }
        }

        public static LBModel createInitModel() {
            LBModel model = new LBModel();
            for (int i = 1; i <= BaseConfig.ins.Rows; i++) {
                for (int j = 1; j <= BaseConfig.ins.Columns; j++) {
                    LBMsg lBMsg = new LBMsg();
                    lBMsg.Id = getLightBoxId(i, j);
                    lBMsg.Row1 = i;
                    lBMsg.Column = j;
                    model.ListLBMsg.Add(lBMsg);
                }
            }
            return model;
        }

        /// <summary>
        /// 从文件中读取所有模式
        /// </summary>
        public static void initModels() {
            listModel = FindModels(FindModelNames());
            listModel.Sort();
            for(int i=0; i<listModel.Count; i++) {
                LBModel model = listModel[i];
                if (model.sortIndex != i) {
                    model.sortIndex = i;
                    WriteXmlHelper.saveModel(model);
                }
            }
            if(listModel.Count > 0) {
                setModelSelected(listModel[0]);
            }
        }

        /// <summary>
        /// 获取所有模式的名称
        /// </summary>
        /// <returns></returns>
        public static List<string> FindModelNames() {
            DirectoryInfo directory = new DirectoryInfo(WriteXmlHelper.FILE_MODEL);
            FileInfo[] listModels = directory.GetFiles();
            List<string> listModelNames = new List<string>();
            foreach (FileInfo fi in listModels) {
                string modelName = fi.Name.Substring(0, fi.Name.LastIndexOf("."));
                listModelNames.Add(modelName);
            }
            listModelNames.Sort();
            return listModelNames;
        }

        /// <summary>
        /// 获取所有模式
        /// </summary>
        /// <returns></returns>
        public static List<LBModel> FindModels(List<string> listModelNames) {
            List<LBModel> listLBModels = new List<LBModel>();
            foreach (string name in listModelNames) {
                LBModel lbModel = ReadXmlHelper.getListBoxMsgs(WriteXmlHelper.FILE_MODEL + name + ".xml");
                lbModel.ModelName = name;
                listLBModels.Add(lbModel);
                //if (lbModel.Active) {
                //    listLBModels.Add(lbModel);
                //}
            }
            return listLBModels;
        }

        public static void nowModelUp() {
            if (modelIndex == 0) {
                return;
            }
            var previousModel = listModel[modelIndex - 1];
            var previousModelIndex = previousModel.sortIndex;
            var nowModel = listModel[modelIndex];
            previousModel.sortIndex = nowModel.sortIndex;
            nowModel.sortIndex = previousModelIndex;
            listModel.Sort();
            modelIndex = listModel.IndexOf(nowModel);
            
            WriteXmlHelper.saveModel(previousModel);
            WriteXmlHelper.saveModel(nowModel);
        }

        public static void nowModelDown() {
            if (modelIndex >= listModel.Count - 1) {
                return;
            }
            var nextModel = listModel[modelIndex + 1];
            var nextModelIndex = nextModel.sortIndex;
            var nowModel = listModel[modelIndex];
            nextModel.sortIndex = nowModel.sortIndex;
            nowModel.sortIndex = nextModelIndex;
            listModel.Sort();
            modelIndex = listModel.IndexOf(nowModel);
            
            WriteXmlHelper.saveModel(nextModel);
            WriteXmlHelper.saveModel(nowModel);
        }

        //复位
        public static void reset() {
            if (listModel.Count == 0) {
                return;
            }
            WAITED = true;
            changeModel(listModel[0]);
            if (th != null) {
                th.Interrupt();
                RUN_ONCE = true;
                PUSED_WHEN_MODEL_OVER = true;
                setResume();
            }

            //Form1.selectLBModel = Form1.listLBModels[0];
            WAITED = false;
        }


        //上一页
        public static void prevPage() {
            if (listModel.Count == 0) {
                return;
            }
            prevModel(null);
            WAITED = true;
            if (th != null) {
                th.Interrupt();
                if (th.ThreadState.ToString().Contains(System.Threading.ThreadState.Suspended.ToString())) {
                    Console.WriteLine("暂停");
                    RUN_ONCE = true;
                    setResume();
                }
            }
            WAITED = false;
        }

        //下一页
        public static void nextPage() {
            if (listModel.Count == 0) {
                return;
            }
            WAITED = true;
            nextModel(null);
            if (th != null) {
                th.Interrupt();
                if (th.ThreadState.ToString().Contains(System.Threading.ThreadState.Suspended.ToString())) {
                    Console.WriteLine("暂停");
                    RUN_ONCE = true;
                    setResume();
                }
            }

            WAITED = false;

        }


        private static void startTurnPicThread() {
            th = new Thread(new ThreadStart(analysis));
            th.Name = "turnPic";
            th.IsBackground = true;
            th.Start();
        }

        public static void start() {
            if (!isRunning) {
                //setModelSelected(listModel[0]);
                isRunning = true;
                startTurnPicThread();
            }
        }

        public static void stop() {
            RUN_ONCE = false;
            WAITED = false;
            PUSED_WHEN_MODEL_OVER = false;
            if (th != null) {
                isRunning = false;
                th.Interrupt();
            }
        }

        public static void setSuspend() {
            if (th != null) {
                try {
                    if (!th.ThreadState.ToString().Contains(System.Threading.ThreadState.Suspended.ToString())) {
                        th.Suspend();
                    }
                } catch (Exception) { }
            }
        }

        public static void setResume() {
            if (th != null) {
                try {
                    th.Resume();
                } catch (Exception) { }
            }
        }

        private static void runModel() {
            if (!SelectedModel.Active) {
                nextModel(null);
            }
        }

        private static void nextModelThread() {
            if (!modelChanging) {
                modelChanging = true;
                ThreadPool.QueueUserWorkItem(new WaitCallback(nextModel), null);
            }
        }

        private static void prevModel(object obj) {
            modelIndex--;
            if(modelIndex <= 0){
                modelIndex = listModel.Count - 1;
            }
            LBModel nextModel = prevActiveModel();
            if (null == nextModel) {
                return;
            }
            if (!nextModel.ModelName.Equals(SelectedModel)) {
                setModelSelected(nextModel);
            }
            //初始化等待时间
            //nextModel.getListWaitTime();
            modelChanging = false;

        }

        private static void nextModel(object obj) {
            if (modelIndex == -1) {
                modelIndex = 0;
            } else {
                modelIndex++;
            }
            LBModel nextModel = nextActiveModel();
            if (null == nextModel) {
                return;
            }
            if (!nextModel.ModelName.Equals(SelectedModel)) {
                setModelSelected(nextModel);
            }
            //初始化等待时间
            //nextModel.getListWaitTime();
            modelChanging = false;

        }

        private static void changeModel(LBModel nextModel) {
            SelectedModel.ModelName = nextModel.ModelName;
            SelectedModel.MWaitTime = nextModel.MWaitTime;
            SelectedModel.runAll = nextModel.runAll;
            foreach (LBMsg lb in nextModel.ListLBMsg) {
                LBMsg lBMsg = SelectedModel.getLightBoxMsg(lb.Row1, lb.Column);
                if (null != lBMsg) {
                    lBMsg.Id = lb.Id;
                    lBMsg.WaitTime = lb.WaitTime;
                    lBMsg.RunTime = lb.RunTime;
                    lBMsg.KeepTime = lb.KeepTime;
                }
            }
            nextModel.getListWaitTime();
            modelChanged?.Invoke(nextModel, null);
        }

        private static LBModel nextActiveModel() {
            LBModel model = null;
            //先从modelIndex位置往后找
            for (int i = modelIndex; i < listModel.Count; i++) {
                LBModel m = listModel[i];
                if (m.Active) {
                    modelIndex = i;
                    model = m;
                    break;
                }
            }

            if(null == model) {
                videoBegin = false;
                //选择第一个模式
                //modelIndex = 1;
            }

            //如果没找到再从0的位置找到modelIndex位置
            if (null == model) {
                for (int i = 0; i < modelIndex; i++) {
                    LBModel m = listModel[i];
                    if (m.Active) {
                        modelIndex = i;
                        model = m;
                        break;
                    }
                }
            }
            return model;
        }

        private static LBModel prevActiveModel() {
            LBModel model = null;
            //先从modelIndex位置往前找
            for (int i = modelIndex; i >= 0; i--) {
                LBModel m = listModel[i];
                if (m.Active) {
                    modelIndex = i;
                    model = m;
                    break;
                }
            }

            if (null == model) {
                videoBegin = false;
                //选择第一个模式
                //modelIndex = 1;
            }

            //如果没找到再从0的位置找到modelIndex位置
            if (null == model) {
                for (int i = listModel.Count - 1; i >= modelIndex; i--) {
                    LBModel m = listModel[i];
                    if (m.Active) {
                        modelIndex = i;
                        model = m;
                        break;
                    }
                }
            }
            return model;
        }

        public static void setModelActive(string name, bool active) {
            foreach (LBModel model in listModel) {
                if (model.ModelName.Equals(name)) {
                    model.Active = active;
                    break;
                }
            }
        }

        public static void setModelSelected(LBModel model) {
            foreach (LBModel m in listModel) {
                if (m == model) {
                    m.Selected = true;
                } else {
                    m.Selected = false;
                }
            }
            changeModel(model);
            modelIndex = listModel.IndexOf(model);
        }

        public static void startCheckSysTimeThread() {
            Thread th = new Thread(new ThreadStart(checkSysTime));
            th.Name = "CheckSysTimeThread";
            th.IsBackground = true;
            th.Start();
        }

        private static void analysis() {
            LBMsg lastLBMsg = null;
            while (!quit && isRunning) {
                try {
                    //BaseConfig.ins.IsPowerOn = true;
                    //如果开机时间没到，不往下运行
                    if (!BaseConfig.ins.IsPowerOn || WAITED) {
                        //Console.WriteLine(" off time");
                        Console.WriteLine(" Power Off");
                        logger.Info(" Power Off");
                        try {
                            Thread.Sleep(1000);
                        } catch (Exception) {
                            return;
                        }
                        continue;
                    }
                    Console.WriteLine(" Power On");
                    logger.Info(" Power On");
                    //videoBegin = true;
                    if (ConfigHelper.IS_WAIT_VIDEO) {
                        //视频未开始, 等待
                        if (!videoBegin) {
                            Thread.Sleep(200);
                            continue;
                        } else {
                            videoBegin = false;
                        }
                    }

                    if (null == SelectedModel) {
                        //Thread.Sleep(1000);
                        nextModel(null);
                        continue;
                    }
                    //MessageHelper.ins.beginModel();

                    //CanDataWithInfo can = new CanDataWithInfo((uint)0x02, new byte[] {0x02, 0, 0 }, "can:0x02, len:3 停全网");
                    //Debug.Log("模式开始:" + SelectedModel.ModelName);
                    //SelectedModel.cleanEndCount();

                    //获取模式中延时时间的集合
                    //List<double> listWaitTime = SelectedModel.listWaitTime;
                    if (SelectedModel.runAll) {
                        LBMsg lBMsg = SelectedModel.ListLBMsg[0];
                        byte time = (byte)(lBMsg.RunTime);
                        CanDataWithInfo can1 = new CanDataWithInfo(0x700, new byte[] { time, 0x40, }, "can:0x700, len:7 全网正");
                        ECANHelper.ins.sendMessageWithInfo(can1);
                        Thread.Sleep((int)(lBMsg.RunTime * 1000));
                        CanDataWithInfo can2 = new CanDataWithInfo(0x700, new byte[] { time, 0xC0, }, "can:0x700, len:7 全网停");
                        ECANHelper.ins.sendMessageWithInfo(can2);
                        Thread.Sleep((int)(lBMsg.KeepTime * 1000));
                        CanDataWithInfo can3 = new CanDataWithInfo(0x700, new byte[] { time, 0x80, }, "can:0x700, len:7 全网反");
                        ECANHelper.ins.sendMessageWithInfo(can3);
                        Thread.Sleep((int)((lBMsg.RunTime + 2) * 1000));
                    } else {
                        List<double> listWaitTime = SelectedModel.getListWaitTime();
                        for (int i = 0; i < listWaitTime.Count; i++) {

                            double waitTime = 0;
                            double totalTime = listWaitTime[i];
                            if (i == 0) {
                                waitTime = totalTime;
                            } else {
                                waitTime = totalTime - listWaitTime[i - 1];
                            }
                            Thread.Sleep((int)(waitTime * 1000));
                            foreach (LBMsg lbMsg in SelectedModel.ListLBMsg) {
                                if (lbMsg.WaitTime == totalTime) {
                                    //Debug.Log("灯箱 :" + lbMsg.Row1 + ":" + lbMsg.Column + " 开始");
                                    //lbMsg.runAble = true;
                                    //DataComm.ins.startRunBox(lbMsg);
                                    if (lbMsg.RunTime > 0) {
                                        MessageHelper.ins.startRunBox(lbMsg);
                                        lastLBMsg = lbMsg;
                                    }
                                    
                                }
                            }
                        }
                        if (null != lastLBMsg) {
                            int lastTime = (int)(lastLBMsg.RunTime * 2 + lastLBMsg.KeepTime);
                            Thread.Sleep(lastTime * 1000);
                        }
                    }
                    //等待灯箱转完
                    //while (!MessageHelper.ins.modelIsEnd()) {
                    //    Thread.Sleep(20);
                    //}
                    //Console.WriteLine("model is end ");
                    //int outTimeCount = 0;
                    //while (!SelectedModel.isRunEnd() && outTimeCount < 60) {
                    //    Thread.Sleep(2000);
                    //    outTimeCount++;
                    //}
                    if (RUN_ONCE) {
                        //临时运行一次结束
                        RUN_ONCE = false;
                        //暂停
                        MainWindow.ins.pasued();
                    } else {
                        int modelWaitTime = SelectedModel.getWaitTimeMS();
                        byte backTime = 1;
                        if (modelWaitTime > 1) {
                            backTime = (byte)(modelWaitTime - 1);
                        }
                        //nextModelThread();
                        //nextModel(null);
                        //全网反转
                        CanDataWithInfo can = new CanDataWithInfo(0x700, new byte[] { backTime, 0x80, }, "can:0x700, len:7 退全网");
                        //ECANHelper.ins.sendMessageWithInfo(can);
                        //Debug.Log("进入等待时间 :" + modelWaitTime);
                        //这里的模式停留时间要从最后一个灯箱转完算起
                        Thread.Sleep(modelWaitTime);
                        nextModel(null);
                    }
                    //Thread.Sleep(2000);
                    //Debug.Log("等待时间结束 :" + modelWaitTime);
                } catch (Exception e) {
                    Console.WriteLine(e.StackTrace + ":" + e.Message);
                    return;
                }
            }
        }

        private static void analysis2() {
            LBMsg lastLBMsg = null;
            while (!quit && isRunning) {
                try {
                    //BaseConfig.ins.IsPowerOn = true;
                    //如果开机时间没到，不往下运行
                    if (!BaseConfig.ins.IsPowerOn || WAITED) {
                        //Console.WriteLine(" off time");
                        Console.WriteLine(" Power Off");
                        logger.Info(" Power Off");
                        try {
                            Thread.Sleep(1000);
                        } catch (Exception) {
                            return;
                        }
                        continue;
                    }
                    Console.WriteLine(" Power On");
                    logger.Info(" Power On");
                    //videoBegin = true;
                    if (ConfigHelper.IS_WAIT_VIDEO) {
                        //视频未开始, 等待
                        if (!videoBegin) {
                            Thread.Sleep(200);
                            continue;
                        } else {
                            videoBegin = false;
                        }
                    }

                    if (null == SelectedModel) {
                        //Thread.Sleep(1000);
                        nextModel(null);
                        continue;
                    }
                    MessageHelper.ins.beginModel();
                    
                    //获取模式中延时时间的集合
                    if (SelectedModel.runAll) {
                        LBMsg lBMsg = SelectedModel.ListLBMsg[0];
                        byte time = (byte)(lBMsg.RunTime);
                        CanDataWithInfo can1 = new CanDataWithInfo(0x700, new byte[] { time, 0x40, }, "can:0x700, len:7 全网正");
                        ECANHelper.ins.sendMessageWithInfo(can1);
                        Thread.Sleep((int)(lBMsg.RunTime * 1000));
                        CanDataWithInfo can2 = new CanDataWithInfo(0x700, new byte[] { time, 0xC0, }, "can:0x700, len:7 全网停");
                        ECANHelper.ins.sendMessageWithInfo(can2);
                        Thread.Sleep((int)(lBMsg.KeepTime * 1000));
                        CanDataWithInfo can3 = new CanDataWithInfo(0x700, new byte[] { time, 0x80, }, "can:0x700, len:7 全网反");
                        ECANHelper.ins.sendMessageWithInfo(can3);
                        Thread.Sleep((int)((lBMsg.RunTime + 2) * 1000));
                    } else {
                        foreach (LBMsg lbMsg in SelectedModel.ListLBMsg) {
                            MessageHelper.ins.startRunBox(lbMsg);
                            lastLBMsg = lbMsg;
                        }
                        while (!MessageHelper.ins.modelIsEnd()) {
                            Thread.Sleep(200);
                        }
                        
                    }
                    if (RUN_ONCE) {
                        //临时运行一次结束
                        RUN_ONCE = false;
                        //暂停
                        MainWindow.ins.pasued();
                    } else {
                        int modelWaitTime = SelectedModel.getWaitTimeMS();
                        byte backTime = 1;
                        if (modelWaitTime > 1) {
                            backTime = (byte)(modelWaitTime - 1);
                        }
                        //全网反转
                        CanDataWithInfo can = new CanDataWithInfo(0x700, new byte[] { backTime, 0x80, }, "can:0x700, len:7 退全网");
                        //ECANHelper.ins.sendMessageWithInfo(can);
                        //Debug.Log("进入等待时间 :" + modelWaitTime);
                        if (!ConfigHelper.IS_WAIT_VIDEO) {
                            Thread.Sleep(modelWaitTime);
                        }
                        nextModel(null);
                    }
                    //Thread.Sleep(2000);
                    //Debug.Log("等待时间结束 :" + modelWaitTime);
                } catch (Exception e) {
                    Console.WriteLine(e.StackTrace + ":" + e.Message);
                    return;
                }
            }
        }
        private static void checkSysTime() {
            DateTime now;
            while (!quit) {
                while (isRunning) {
                    now = DateTime.Now;
                    int time = now.Hour * 60 + now.Minute;
                    string[] pna = BaseConfig.ins.PowerOnA.Split(':');
                    string[] pfa = BaseConfig.ins.PowerOffA.Split(':');
                    string[] pnb = BaseConfig.ins.PowerOnB.Split(':');
                    string[] pfb = BaseConfig.ins.PowerOffB.Split(':');
                    int ipna = Convert.ToInt16(pna[0]) * 60 + Convert.ToInt16(pna[1]);
                    int ipfa = Convert.ToInt16(pfa[0]) * 60 + Convert.ToInt16(pfa[1]);
                    int ipnb = Convert.ToInt16(pnb[0]) * 60 + Convert.ToInt16(pnb[1]);
                    int ipfb = Convert.ToInt16(pfb[0]) * 60 + Convert.ToInt16(pfb[1]);

                    if ((time >= ipna && time < ipfa)
                        || (time >= ipnb && time < ipfb)) {
                        BaseConfig.ins.IsPowerOn = true;
                    } else {
                        BaseConfig.ins.IsPowerOn = false;
                    }
                    if (now.Hour == 23 && now.Minute == 59 && now.Second == 59) {
                        MessageHelper.ins.searchBoxes();
                    }
                    Thread.Sleep(3000);
                }
                Thread.Sleep(3000);
            }
        }

        //获取灯箱所在的大箱的id 从1开始
        private static int getBigBoxGroupId(int row, int column) {
            int groupId = 0;
            //一行最多几个组
            int rowMaxGroupNum = BaseConfig.ins.Columns / 4;

            //当前灯箱列所在的组, 从1开始
            int columnInGroup = column / 4;
            if (column % 4 != 0) {
                columnInGroup++;
            }

            //当前灯箱行所在的组, 从1开始
            int rowInGroup = row / 4;
            if (row % 4 != 0) {
                rowInGroup++;
            }

            groupId = rowMaxGroupNum * (rowInGroup - 1) + columnInGroup;
            return groupId;
        }

        private static int getLightBoxIdInGroup(int row, int column) {
            int boxIdInGroup = 0;
            //一行最多4个
            int rowMaxNum = 4;

            //当前灯箱在组中所在的列所, 从1开始
            int columnInGroup = column % 4;
            if (column % 4 == 0) {
                columnInGroup = 4;
            }

            //当前灯箱在组中所在的行, 从1开始
            int rowInGroup = row % 4;
            if (row % 4 == 0) {
                rowInGroup = 4;
            }

            boxIdInGroup = rowMaxNum * (rowInGroup - 1) + columnInGroup;
            return boxIdInGroup;
        }

        /// <summary>
        /// 获取灯箱id
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private static int getLightBoxId(int row, int column) {
            int groupId = getBigBoxGroupId(row, column);
            int idInGroup = getLightBoxIdInGroup(row, column);
            return (groupId - 1) * 16 + idInGroup;
        }
    }
}
