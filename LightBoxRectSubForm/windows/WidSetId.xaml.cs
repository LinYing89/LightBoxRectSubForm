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
    /// WidSetId.xaml 的交互逻辑
    /// </summary>
    public partial class WidSetId : Window {
        public WidSetId() {
            InitializeComponent();
        }

        private void btnSet_Click(object sender, RoutedEventArgs e) {
            try {
                int oldId = Convert.ToInt32(tbOldId.Text);
                int newId = Convert.ToInt32(tbNewId.Text);
                MessageHelper.ins.setBoxId(oldId, newId);
            } catch (Exception) { }
        }
    }
}
