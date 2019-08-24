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

namespace LightBoxRectSubForm {
    /// <summary>
    /// WidEditLBMsg.xaml 的交互逻辑
    /// </summary>
    public partial class WidEditLBMsg : Window {

        private LBModel lBModel;

        public WidEditLBMsg(LBModel lBModel) {
            InitializeComponent();
            this.lBModel = lBModel;
            txtName.Text = lBModel.ModelName;

            comboBoxMinute.SelectedIndex = (int)lBModel.MWaitTime.WaitMM;
            comboBoxSeconds.SelectedIndex = (int)lBModel.MWaitTime.WaitSS;
            cbRunAll.IsChecked = lBModel.runAll;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            
            int waitMinute = comboBoxMinute.SelectedIndex;
            int waitSecond = comboBoxSeconds.SelectedIndex;
            bool runAll = (bool)(cbRunAll.IsChecked);
            if (lBModel.MWaitTime.WaitMM != waitMinute || lBModel.MWaitTime.WaitSS != waitSecond) {
                lBModel.MWaitTime.WaitMM = waitMinute;
                lBModel.MWaitTime.WaitSS = waitSecond;
                WriteXmlHelper.saveModel(lBModel);
            }
            lBModel.runAll = runAll;


            string newName = txtName.Text;
            string oldName = lBModel.ModelName;
            LightBoxHelper.updateModel(lBModel);
            WriteXmlHelper.saveModel(lBModel);
            if (!newName.Equals(oldName)) {
                if (string.IsNullOrEmpty(newName)) {
                    MessageBox.Show("模式名称不能为空");
                    return;
                }
                if (LightBoxHelper.modelNameIsHaved(newName)) {
                    MessageBox.Show("模式名称已存在");
                    return;
                }
                LightBoxHelper.modelRename(oldName, newName);
                lBModel.ModelName = newName;
            }

            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        private void comboBoxMinute_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        private void comboBoxSeconds_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            LightBoxHelper.delModel(lBModel.ModelName);
            DialogResult = true;
            //Close();
        }
    }
}
