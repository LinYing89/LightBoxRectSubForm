using LightBoxRectSubForm.app;
using System;
using System.Collections.Generic;
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

namespace LightBoxRectSubForm.windows {
    /// <summary>
    /// WidOnOffTimeSet.xaml 的交互逻辑
    /// </summary>
    public partial class WidOnOffTimeSet : Window {
        public WidOnOffTimeSet() {
            InitializeComponent();

            string[] powerOnA = BaseConfig.ins.PowerOnA.Split(':');
            string[] powerOffA = BaseConfig.ins.PowerOffA.Split(':');

            string[] powerOnB = BaseConfig.ins.PowerOnB.Split(':');
            string[] powerOffB = BaseConfig.ins.PowerOffB.Split(':');

            powerOnAHour.SelectedIndex = int.Parse(powerOnA[0]);
            powerOnAMinute.SelectedIndex = int.Parse(powerOnA[1]);
            powerOffAHour.SelectedIndex = int.Parse(powerOffA[0]);
            powerOffAMinute.SelectedIndex = int.Parse(powerOffA[1]);

            powerOnBHour.SelectedIndex = int.Parse(powerOnB[0]);
            powerOnBMinute.SelectedIndex = int.Parse(powerOnB[1]);
            powerOffBHour.SelectedIndex = int.Parse(powerOffB[0]);
            powerOffBMinute.SelectedIndex = int.Parse(powerOffB[1]);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            BaseConfig.ins.PowerOnA = powerOnAHour.SelectedIndex + ":" + powerOnAMinute.SelectedIndex;
            BaseConfig.ins.PowerOffA = powerOffAHour.SelectedIndex + ":" + powerOffAMinute.SelectedIndex;

            BaseConfig.ins.PowerOnB = powerOnBHour.SelectedIndex + ":" + powerOnBMinute.SelectedIndex;
            BaseConfig.ins.PowerOffB = powerOffBHour.SelectedIndex + ":" + powerOffBMinute.SelectedIndex;
            BaseConfig.ins.writeConfig();
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
