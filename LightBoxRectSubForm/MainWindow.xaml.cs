using LightBoxRectSubForm.app;
using LightBoxRectSubForm.comm;
using LightBoxRectSubForm.data;
using LightBoxRectSubForm.dll;
using LightBoxRectSubForm.netty;
using LightBoxRectSubForm.windows;
using Microsoft.Win32;
using NLog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LightBoxRectSubForm
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Logger logger = LogManager.GetLogger("MainWindow");
        public static MainWindow ins;
        private static System.Diagnostics.Process pPreview;

        public SynchronizationContext m_SyncContext = null;
        bool suspend = false;

        public MainWindow()
        {
            InitializeComponent();
            m_SyncContext = SynchronizationContext.Current;
            ins = this;
            //logger.Info("indo");
            //logger.Error("error");
            ThreadPool.SetMinThreads(1000, 500);
            BaseConfig.ins.init();
            ConfigHelper.init();
            labelRowColumn.Content = BaseConfig.ins.Rows + "行" + BaseConfig.ins.Columns + "列";
            menuRunOnPower.IsChecked = ConfigHelper.IS_POWER_RUN;
            menuWaitVideo.IsChecked = ConfigHelper.IS_WAIT_VIDEO;
            //LightBoxHelper.SelectedModel = null;
            LightBoxHelper.modelChanged += LightBoxHelper_modelChanged;

            LightBoxHelper.initModels();
            listBoxModels.ItemsSource = LightBoxHelper.listModel;

            //SmsComm.ins.eventStateChanged += SmsComm_eventStateChanged;
            DataComm.ins.eventStateChanged += DataVideo_eventStateChanged;
            ECANHelper.ins.eventStateChanged += DataSms_eventStateChanged;
            ECANHelper.ins.eventBoxStateChanged += Ins_eventBoxStateChanged;

            DataComm.ins.init();
            DataComm.ins.open();
            //SmsComm.ins.init();
            //SmsComm.ins.open();

            string path = AppDomain.CurrentDomain.BaseDirectory;
            string str1 = Process.GetCurrentProcess().MainModule.FileName;
            Console.WriteLine(path + ",," + str1);

            LightBoxHelper.startCheckSysTimeThread();

            NettyHelper.start();

            ECANHelper.ins.openDevice();
        }

        private void Ins_eventBoxStateChanged(object sender, BoxStateEventArgs e) {
            if(e.stateCode != 0) {
                LBMsg msg = LightBoxHelper.SelectedModel.getLightBoxMsg(e.boxId);

                string info = "id=" + e.boxId;
                if(null != msg) {
                    info += ",\n" + msg.Row1 + "行" + msg.Column + "列,\n" + e.getStateInfo();
                }
                logger.Error(info);
                SmsComm.ins.sendMessage(info);
            }
        }

        private void LightBoxHelper_modelChanged(object sender, EventArgs e) {
            LBModel model = sender as LBModel;
            listBoxModels.Dispatcher.Invoke(new Action(() => { listBoxModels.SelectedItem = model; }));
            labelModelName.Dispatcher.Invoke(new Action(() => { labelModelName.Content = model.ModelName; }));
        }

        private void DataSms_eventStateChanged(object sender, CommStateEventArgs e) {
            labelDataCommState.Dispatcher.Invoke(new Action(() => {
                labelDataCommState.Content = e.message;
                if (e.opened) {
                    btnDataCommOpenClose.Content = "关闭";
                    //打开后发一个全网回位
                    //CanDataWithInfo can = new CanDataWithInfo(0x700, new byte[] { 0x07, 0x80, }, "can:0x700, len:7 退全网");
                    //ECANHelper.ins.sendMessageWithInfo(can);
                } else {
                    btnDataCommOpenClose.Content = "打开";
                }
            }));
            
        }

        private void DataVideo_eventStateChanged(object sender, CommStateEventArgs e) {
            labelVideoCommState.Dispatcher.Invoke(new Action(() => {
                labelVideoCommState.Content = e.message;
                if (e.opened) {
                    btnVideoCommOpenClose_Copy.Content = "关闭";
                } else {
                    btnVideoCommOpenClose_Copy.Content = "打开";
                }
            }));

        }

        private void SmsComm_eventStateChanged(object sender, CommStateEventArgs e) {
            labelSmsCommState.Dispatcher.Invoke(new Action(()=> {
                labelSmsCommState.Content = e.message;
                if (e.opened) {
                    btnSmsCommOpenClose.Content = "关闭";
                } else {
                    btnSmsCommOpenClose.Content = "打开";
                }
            }));
            
        }

        private void MenuItemDataCommSet_Click(object sender, RoutedEventArgs e)
        {
            FrmCommSettings frm = new FrmCommSettings(DataComm.ins);
            frm.Show();
        }

        private void menuSmsCommSet_Click(object sender, RoutedEventArgs e) {
            FrmCommSettings frm = new FrmCommSettings(SmsComm.ins);
            frm.Show();
        }

        private void btnSmsCommOpenClose_Click(object sender, RoutedEventArgs e) {
            if (btnSmsCommOpenClose.Content.Equals("打开")) {
                SmsComm.ins.open();
            } else {
                SmsComm.ins.close();
            }
        }
        
        private void btnDataCommOpenClose_Click(object sender, RoutedEventArgs e) {
            if (btnDataCommOpenClose.Content.Equals("打开")) {
                //DataComm.ins.open();
                ECANHelper.ins.openDevice();
                //ECANHelper.ins.startSend();
            } else {
                //DataComm.ins.close();
                ECANHelper.ins.closeDevice();
            }
        }

        private void btnVideoCommOpenClose_Click(object sender, RoutedEventArgs e) {
            if (btnVideoCommOpenClose_Copy.Content.Equals("打开")) {
                DataComm.ins.open();
            } else {
                DataComm.ins.close();
            }
        }

        private void Window_Closed(object sender, EventArgs e) {
            SmsComm.ins.eventStateChanged -= SmsComm_eventStateChanged;
            DataComm.ins.eventStateChanged -= DataSms_eventStateChanged;
            DataComm.ins.close();
            ECANHelper.ins.closeDevice();
            SmsComm.ins.close();
        }

        public void runBegin() {
            if (LightBoxHelper.isRunning) {
                return;
            }
            //LightBoxHelper.isRunning = true;
            m_SyncContext.Post(runBeginOnUI, null);
        }

        private void runBeginOnUI(object obj) {
            btnRunStop.Content = "停止";
            btnPaush.Content = "暂停";
            btnPaush.IsEnabled = true;
            LightBoxHelper.start();
            btnReloadModels.IsEnabled = false;
        }

        public void runStop() {
            if (!LightBoxHelper.isRunning) {
                return;
            }
            LightBoxHelper.isRunning = false;
            m_SyncContext.Post(runStopOnUI, null);
        }

        private void runStopOnUI(object obj) {
            btnRunStop.Content = "运行";
            btnPaush.Content = "继续";
            btnPaush.IsEnabled = false;
            LightBoxHelper.stop();
            btnReloadModels.IsEnabled = true;
        }

        public void pasued() {
            m_SyncContext.Post(pasuedOnUI, null);
        }

        private void pasuedOnUI(object obj) {
            suspend = true;
            LightBoxHelper.setSuspend();
            btnPaush.Content = "继续";
        }

        public void resume() {
            if (!LightBoxHelper.isRunning) {
                runBegin();
                return;
            }
            m_SyncContext.Post(resumeOnUI, null);

        }

        private void resumeOnUI(object obj) {
            suspend = false;
            LightBoxHelper.setResume();
            btnPaush.Content = "暂停";
        }

        /// <summary>
        /// 暂停
        /// </summary>
        /// <param name="o"></param>
        public void Suspend(object o) {
            if (suspend) {
                resume();
            } else {
                pasued();
            }
        }
        
        private void btnRunStop_Click(object sender, RoutedEventArgs e) {
            if (LightBoxHelper.isRunning) {
                runStop();
            } else {
                runBegin();
            }
        }

        private void menuRunOnPower_Checked(object sender, RoutedEventArgs e) {
            bool powerRun = (bool)menuRunOnPower.IsChecked;
            ConfigHelper.WriteConfig(ConfigHelper.APPLICATION, ConfigHelper.POWER_RUN, Convert.ToString(powerRun));
            ConfigHelper.IS_POWER_RUN = powerRun;
            string appName = Process.GetCurrentProcess().MainModule.ModuleName;
            //string appPath = Process.GetCurrentProcess().MainModule.FileName;
            string appPath = System.AppDomain.CurrentDomain.BaseDirectory + appName;
            SelfRunning(true, appName, appPath);
            //设置开机自启动 
            //MessageBox.Show("设置开机自启动，需要修改注册表", "提示");
            //string path = Process.GetCurrentProcess().MainModule.FileName;
            //string path = Application.ExecutablePath;
            //RegistryKey rk = Registry.CurrentUser;
            //RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            //rk2.SetValue("LightBoxRect", path);
            //rk2.Close();
            //rk.Close();
        }

        private void menuRunOnPower_Unchecked(object sender, RoutedEventArgs e) {
            bool powerRun = (bool)menuRunOnPower.IsChecked;
            ConfigHelper.WriteConfig(ConfigHelper.APPLICATION, ConfigHelper.POWER_RUN, Convert.ToString(powerRun));
            ConfigHelper.IS_POWER_RUN = powerRun;
            string appName = Process.GetCurrentProcess().MainModule.ModuleName;
            //string appPath = Process.GetCurrentProcess().MainModule.FileName;
            string appPath = System.AppDomain.CurrentDomain.BaseDirectory + appName;
            SelfRunning(false, appName, appPath);

            //取消开机自启动 
            //MessageBox.Show("取消开机自启动，需要修改注册表", "提示");
            //string path = Process.GetCurrentProcess().MainModule.FileName;
            //RegistryKey rk = Registry.CurrentUser;
            //RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            //rk2.DeleteValue("LightBoxRect", false);
            //rk2.Close();
            //rk.Close();
        }

        /// <summary>
        /// 写入或删除注册表键值对,即设为开机启动或开机不启动
        /// </summary>
        /// <param name="isStart">是否开机启动</param>
        /// <param name="exeName">应用程序名</param>
        /// <param name="path">应用程序路径带程序名</param>
        /// <returns></returns>
        private static bool SelfRunning(bool isStart, string exeName, string path) {
            try {
                RegistryKey local = Registry.LocalMachine;
                RegistryKey key = local.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (key == null) {
                    local.CreateSubKey("SOFTWARE//Microsoft//Windows//CurrentVersion//Run");
                }
                //若开机自启动则添加键值对
                if (isStart) {
                    key.SetValue(exeName, path);
                    key.Close();
                } else//否则删除键值对
                  {
                    string[] keyNames = key.GetValueNames();
                    foreach (string keyName in keyNames) {
                        if (keyName.ToUpper() == exeName.ToUpper()) {
                            //key.DeleteValue(exeName);
                            //key.Close();
                        }
                    }
                }
            } catch (Exception ex) {
                string ss = ex.Message;
                return false;
                //throw;
            }

            return true;
        }

        private void btnReloadModels_Click(object sender, RoutedEventArgs e) {
            LightBoxHelper.initModels();
            listBoxModels.ItemsSource = LightBoxHelper.listModel;
        }

        private void listBoxModels_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            int index = listBoxModels.SelectedIndex;
            if(index >=0 && index < LightBoxHelper.listModel.Count){
                if (LightBoxHelper.isRunning) {
                    if (index == 0) {
                        LightBoxHelper.modelIndex = LightBoxHelper.listModel.Count;
                    } else {
                        LightBoxHelper.modelIndex = index - 1;
                    }
                } else {
                    LightBoxHelper.setModelSelected(LightBoxHelper.listModel[index]);
                }
            }
        }

        private void listBoxModels_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (LightBoxHelper.isRunning) {
                MessageBox.Show("请先停止运行");
                return;
            }

            int index = listBoxModels.SelectedIndex;
            if (index >= 0 && index < LightBoxHelper.listModel.Count) {
                WidEditLBMsg wid = new WidEditLBMsg(LightBoxHelper.listModel[index]);
                wid.ShowDialog();
                if (wid.DialogResult == true) {
                    listBoxModels.ItemsSource = null;
                    listBoxModels.ItemsSource = LightBoxHelper.listModel;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            LightBoxHelper.quit = true;
        }

        private void btnBegin_Click(object sender, RoutedEventArgs e) {
            LightBoxHelper.videoBegin = true;
        }

        private void menuWaitVideo_Checked(object sender, RoutedEventArgs e) {
            ConfigHelper.WriteConfig(ConfigHelper.APPLICATION, ConfigHelper.WAIT_VIDEO, Convert.ToString("True"));
            ConfigHelper.IS_WAIT_VIDEO = true;
        }

        private void menuWaitVideo_Unchecked(object sender, RoutedEventArgs e) {
            ConfigHelper.WriteConfig(ConfigHelper.APPLICATION, ConfigHelper.WAIT_VIDEO, Convert.ToString("False"));
            ConfigHelper.IS_WAIT_VIDEO = false;
        }

        private void menuBoxEdit_Click(object sender, RoutedEventArgs e) {
            WidShowBoxes.lBModel = LightBoxHelper.SelectedModel;
            WidShowBoxes widShowBoxes = new WidShowBoxes();
            widShowBoxes.Show();
        }

        private void btnUp_Click(object sender, RoutedEventArgs e) {
            LightBoxHelper.nowModelUp();
            listBoxModels.ItemsSource = null;
            listBoxModels.ItemsSource = LightBoxHelper.listModel;
        }

        private void btnDown_Click(object sender, RoutedEventArgs e) {
            LightBoxHelper.nowModelDown();
            listBoxModels.ItemsSource = null;
            listBoxModels.ItemsSource = LightBoxHelper.listModel;
        }

        //查询灯箱状态
        private void btnSearchState_Click(object sender, RoutedEventArgs e) {
            LBModel model = null;
            if (LightBoxHelper.listModel.Count > 0) {
                model = LightBoxHelper.listModel[0];
            } else {
                model = LightBoxHelper.SelectedModel;
            }
            foreach(LBMsg msg in model.ListLBMsg) {
                byte[] byDate = new byte[3];
                byDate[0] = 4;
                byDate[1] = (byte)msg.Id;
                byDate[2] = (byte)(msg.Id >> 8);
                CanDataWithInfo can = new CanDataWithInfo(0x01, byDate, "查状态");
                ECANHelper.ins.sendMessageWithInfo(can);
            }

            //全网一条指令查询,可能造成同时回复数据过多,导致模块内缓存溢出
            //byte[] byDate = new byte[3];
            //byDate[0] = 6;
            //CanDataWithInfo can = new CanDataWithInfo(0x01, byDate, "");
            //ECANHelper.ins.sendMessageWithInfo(can);
        }

        //打开收发数据测试窗口
        private void menuTest_Click(object sender, RoutedEventArgs e) {
            new WidTest().Show();
        }

        //id相关
        private void menuId_Click(object sender, RoutedEventArgs e) {
            new WidId().ShowDialog();
        }

        private void menuPreview_Click(object sender, RoutedEventArgs e) {
            if (pPreview == null) {
                pPreview = new System.Diagnostics.Process();
                pPreview.StartInfo.FileName = "LightBoxRect.exe";
                pPreview.Start();
            } else {
                if (pPreview.HasExited) //是否正在运行
                {
                    pPreview.Start();
                }
            }
            pPreview.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) {
            CheckBox checkBox = sender as CheckBox;
            LBModel model = checkBox.DataContext as LBModel;
            WriteXmlHelper.saveModel(model);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
            CheckBox checkBox = sender as CheckBox;
            LBModel model = checkBox.DataContext as LBModel;
            WriteXmlHelper.saveModel(model);
        }

        private void menuAllBack_Click(object sender, RoutedEventArgs e) {
            CanDataWithInfo can3 = new CanDataWithInfo(0x700, new byte[] { 8, 0x80, }, "can:0x700, len:7 全网反");
            ECANHelper.ins.sendMessageWithInfo(can3);
        }

        private void btnPaush_Click(object sender, RoutedEventArgs e) {
            Suspend(null);
        }

        private void menuOnOffTimeSet_Click(object sender, RoutedEventArgs e) {
            WidOnOffTimeSet widOnOffTimeSet = new WidOnOffTimeSet();
            widOnOffTimeSet.ShowDialog();
        }
    }
}
