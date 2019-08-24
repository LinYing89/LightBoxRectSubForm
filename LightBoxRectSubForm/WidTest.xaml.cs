using LightBoxRectSubForm.comm;
using LightBoxRectSubForm.dll;
using System;
using System.Windows;

namespace LightBoxRectSubForm {
    /// <summary>
    /// WidTest.xaml 的交互逻辑
    /// </summary>
    public partial class WidTest : Window {

        public WidTest() {
            InitializeComponent();
            
            //DataComm.ins.eventReceived += Ins_eventReceived;
            //DataComm.ins.eventSended += Ins_eventSended;
            ECANHelper.ins.eventReceived += Ins_eventReceived;
            ECANHelper.ins.eventSended += Ins_eventSended;
        }

        //串口收到数据
        private void Ins_eventSended(object sender, ByteEventArgs e) {
            txtSend.Dispatcher.Invoke(new Action(() => {
                txtSend.Text = txtSend.Text + "\n" + e.ToString();
            }));

        }

        //串口发出数据
        private void Ins_eventReceived(object sender, ByteEventArgs e) {
            txtReceived.Dispatcher.Invoke(new Action(() => {
                txtReceived.Text = txtReceived.Text + "\n" + e.ToString();
            }));
        }

        private void btnCleanSend_Click(object sender, RoutedEventArgs e) {
            txtSend.Text = "";
        }

        private void btnCleanReceive_Click(object sender, RoutedEventArgs e) {
            txtReceived.Text = "";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            DataComm.ins.eventReceived -= Ins_eventReceived;
            DataComm.ins.eventSended -= Ins_eventSended;
        }
    }
}
