using LightBoxRectSubForm.app;
using LightBoxRectSubForm.data;
using LightBoxRectSubForm.dll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LightBoxRectSubForm.windows {
    /// <summary>
    /// WidId.xaml 的交互逻辑
    /// </summary>
    public partial class WidId : Window {
        
        public WidId() {
            InitializeComponent();
            ECANHelper.ins.eventBoxStateChanged += Ins_eventBoxStateChanged;
        }

        private void Ins_eventBoxStateChanged(object sender, BoxStateEventArgs e) {
            labelId.Dispatcher.Invoke(new Action(() => {
                string stateInfo = e.getStateInfo();
                string state = " id:" + e.boxId + " 状态:" + e.stateCode + " 状态描述:" + stateInfo;
                labelId.Content = state;
            }));
        }

        private void btnGetId_Click(object sender, RoutedEventArgs e) {
            MessageHelper.ins.getBoxId();
        }

        private void btnSetId_Click(object sender, RoutedEventArgs e) {
            new WidSetId().ShowDialog();
        }

        private void btnRun_Click(object sender, RoutedEventArgs e) {
            var idText = tbId.Text;
            if (string.IsNullOrWhiteSpace(idText)) {
                MessageBox.Show("灯箱id为空");
            }
            var runTimeText = tbRunTime.Text;
            if (string.IsNullOrWhiteSpace(runTimeText)) {
                MessageBox.Show("运行时间为空");
            }
            var keepTimeText = tbKeepTime.Text;
            if (string.IsNullOrWhiteSpace(keepTimeText)) {
                MessageBox.Show("停留时间为空");
            }

            try {
                var runTime = Convert.ToInt32(runTimeText);
                var keepTime = Convert.ToInt32(keepTimeText);
                LBMsg box = new LBMsg();
                box.Id = Convert.ToInt32(idText);
                box.RunTime = runTime;
                box.KeepTime = keepTime;
                MessageHelper.ins.startRunBox(box);
                //DataComm.ins.startRunBox(box);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAllRun_Click(object sender, RoutedEventArgs e) {
            var idText = tbId.Text;
            if (string.IsNullOrWhiteSpace(idText)) {
                MessageBox.Show("灯箱id为空");
            }
            var runTimeText = tbRunTime.Text;
            if (string.IsNullOrWhiteSpace(runTimeText)) {
                MessageBox.Show("运行时间为空");
            }
            var keepTimeText = tbKeepTime.Text;
            if (string.IsNullOrWhiteSpace(keepTimeText)) {
                MessageBox.Show("停留时间为空");
            }
            try {
                var runTime = Convert.ToInt32(runTimeText);
                var keepTime = Convert.ToInt32(keepTimeText);
                LBMsg box = new LBMsg();
                box.Id = Convert.ToInt32(idText);
                box.RunTime = runTime;
                box.KeepTime = keepTime;
                byte time = (byte)(box.RunTime);
                CanDataWithInfo can1 = new CanDataWithInfo(0x700, new byte[] { time, 0x40, }, "can:0x700, len:7 全网正");
                ECANHelper.ins.sendMessageWithInfo(can1);
                Thread.Sleep((int)(box.RunTime * 1000));
                Thread.Sleep((int)(box.KeepTime * 1000));
                CanDataWithInfo can3 = new CanDataWithInfo(0x700, new byte[] { time, 0x80, }, "can:0x700, len:7 全网反");
                ECANHelper.ins.sendMessageWithInfo(can3);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            ECANHelper.ins.eventBoxStateChanged -= Ins_eventBoxStateChanged;
        }
    }
}
