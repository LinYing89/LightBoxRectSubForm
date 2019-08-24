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
    /// WidFirstId.xaml 的交互逻辑
    /// </summary>
    public partial class WidFirstId : Window {

        public int newId;
        private int oldId;

        public WidFirstId(int id) {
            InitializeComponent();
            oldId = id;
            txtId.Text = Convert.ToString(oldId);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            try {
                newId = Convert.ToInt32(txtId.Text);
                if(newId <= 0) {
                    return;
                }
            } catch (Exception) {
                return;
            }
            DialogResult = true;
        }
    }
}
