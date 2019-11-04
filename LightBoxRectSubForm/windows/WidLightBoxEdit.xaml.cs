using LightBoxRectSubForm.data;
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
    /// WidLightBoxEdit.xaml 的交互逻辑
    /// </summary>
    public partial class WidLightBoxEdit : Window {

        public double waitTime = 0;
        public int runTime = 0;
        public int keepTime = 0;
        public int repeatCount = 0;
        private LBMsg lBMsg;

        public WidLightBoxEdit(LBMsg lBMsg) {
            InitializeComponent();
            this.lBMsg = lBMsg;
            tbWaitTime.Text = Convert.ToString(lBMsg.WaitTime);
            tbRunTime.Text = Convert.ToString(lBMsg.RunTime);
            tbKeepTime.Text = Convert.ToString(lBMsg.KeepTime);
            tbRepeatCount.Text = Convert.ToString(lBMsg.RepeatCount);
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            try {
                waitTime = Convert.ToDouble(tbWaitTime.Text);
                runTime = Convert.ToInt32(tbRunTime.Text);
                keepTime = Convert.ToInt32(tbKeepTime.Text);
                repeatCount = Convert.ToInt16(tbRepeatCount.Text);
            } catch (Exception) {
                MessageBox.Show("输入的值不正确");
            }
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }
    }
}
