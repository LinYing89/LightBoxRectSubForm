using LightBoxRectSubForm.app;
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
    /// WidAllKeepTime.xaml 的交互逻辑
    /// </summary>
    public partial class WidAllKeepTime : Window {

        private LBModel lBModel;

        public WidAllKeepTime(LBModel lBModel) {
            InitializeComponent();
            this.lBModel = lBModel;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            var runTime = tbKeepTime.Text;
            foreach (LBMsg msg in lBModel.ListLBMsg) {
                msg.KeepTime = Convert.ToInt16(runTime);
                LightBoxHelper.updateModelLBMsg(msg);
            }
            WriteXmlHelper.saveModel(lBModel);
            Close();
        }

        private void btnCacnel_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
