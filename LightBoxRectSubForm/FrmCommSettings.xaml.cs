using LightBoxRectSubForm.app;
using LightBoxRectSubForm.comm;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LightBoxRectSubForm
{
    /// <summary>
    /// FrmCommSettings.xaml 的交互逻辑
    /// </summary>
    public partial class FrmCommSettings : Window
    {
        private Comm comm;

        public FrmCommSettings(Comm comm)
        {
            InitializeComponent();
            this.comm = comm;

            //初始化下拉串口名称列表框  
            string[] ports = SerialPort.GetPortNames();
            Array.Sort(ports);
            foreach(string name in ports) {
                ComboBoxItem cb = new ComboBoxItem();
                cb.Content = name;
                comboBoxCommName.Items.Add(cb);
            }
            comboBoxCommName.SelectedIndex = comboBoxCommName.Items.Count > 0 ? 0 : -1;

            if (comm.uartName != null && ports.Contains(comm.uartName)) {
                comboBoxCommName.Text = comm.uartName;
            }

            if (comm.bandRate == 0) {
                comboBoxBaudRate.SelectedIndex = 0;
            } else {
                comboBoxBaudRate.Text = Convert.ToString(comm.bandRate);
            }

            if (comm.dataBit == 0) {
                comboBoxDataBits.SelectedIndex = 0;
            } else {
                comboBoxDataBits.Text = Convert.ToString(comm.dataBit);
            }

            if (comm.stopBit == 0) {
                comboStopBits.SelectedIndex = 0;
            } else {
                comboStopBits.Text = Convert.ToString(comm.stopBit);
            }
            comboBoxParity.SelectedIndex = comm.verify;
        }

        private void comboBoxCommName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            string name = comboBoxCommName.Text;
            ConfigHelper.WriteConfig(comm.UartNote, Comm.uartNameKey, name);
            ConfigHelper.WriteConfig(comm.UartNote, Comm.uartBandRate, comboBoxBaudRate.Text);
            ConfigHelper.WriteConfig(comm.UartNote, Comm.uartDataBit, comboBoxDataBits.Text);
            ConfigHelper.WriteConfig(comm.UartNote, Comm.uartStopBit, comboStopBits.Text);
            ConfigHelper.WriteConfig(comm.UartNote, Comm.uartVerify, Convert.ToString(comboBoxParity.SelectedIndex));
            comm.uartName = name;
            comm.bandRate = Convert.ToInt32(comboBoxBaudRate.Text);
            comm.dataBit = Convert.ToInt16(comboBoxDataBits.Text);
            comm.stopBit = Convert.ToInt16(comboStopBits.Text);
            comm.verify = comboBoxParity.SelectedIndex;
            comm.refreshConfig();
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
