using LightBoxRectSubForm.app;
using LightBoxRectSubForm.data;
using LightBoxRectSubForm.myView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// widShowBoxes.xaml 的交互逻辑
    /// </summary>
    public partial class WidShowBoxes : Window {

        public static LBModel lBModel;

        private int minId = 0;
        List<Color> listColors = new List<Color>();
        private List<LightBox> lightBoxes = new List<LightBox>();

        public WidShowBoxes() {
            InitializeComponent();
            minId = lBModel.getMinId();
        }

        private void cbShowCheckBox_Checked(object sender, RoutedEventArgs e) {
            foreach (LightBox light in lightBoxes) {
                light.cbAddr.Visibility = Visibility.Visible;
            }
        }

        private void cbShowCheckBox_Unchecked(object sender, RoutedEventArgs e) {
            foreach (LightBox light in lightBoxes) {
                light.cbAddr.Visibility = Visibility.Hidden;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            foreach (LBMsg lBMsg in lBModel.ListLBMsg) {
                LightBox light = new LightBox(lBMsg);
                lightBoxes.Add(light);
                canvasBoxContainer.Children.Add(light);
                Canvas.SetLeft(light, 86 * (lBMsg.Column - 1) + lBMsg.Column * 1);
                Canvas.SetTop(light, 42 * (lBMsg.Row1 - 1) + lBMsg.Row1 * 7);
                light.rightClickEvent += Light_rightClickEvent;
                light.clickEvent += Light_clickEvent;
            }

            double width = BaseConfig.ins.Columns * (86 + 2) + 10;
            Console.WriteLine("width:" + width);
            canvasBoxContainer.Width = width;
            canvasBoxContainer.Height = BaseConfig.ins.Rows * (42 + 2) + 100;

            List<double> listWaitTime = LightBoxHelper.SelectedModel.getListWaitTime();
            for (int i = 0; i < listWaitTime.Count; i++) {
                double time = listWaitTime[i];
                Color color = GetRandomColor();
                foreach (LightBox box in lightBoxes) {
                    if (box.lBMsg.WaitTime == time) {
                        box.setBackGroundColor(color);
                    }
                }
            }
        }

        private void Light_clickEvent(object sender, EventArgs e) {
            LightBox boxSrc = sender as LightBox;
            List<LightBox> listSelectedBox = new List<LightBox>();
            listSelectedBox.Add(boxSrc);
            setListBox(listSelectedBox);
        }

        private void Light_rightClickEvent(object sender, EventArgs e) {
            LightBox boxSrc = sender as LightBox;
            foreach (LightBox box in lightBoxes) {
                if (box.lBMsg.WaitTime == boxSrc.lBMsg.WaitTime) {
                    box.setSelected();
                }
            }
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e) {
            foreach (LightBox light in lightBoxes) {
                light.cbAddr.IsChecked = true;
            }
        }

        private void btnUnSelectAll_Click(object sender, RoutedEventArgs e) {
            foreach (LightBox light in lightBoxes) {
                light.cbAddr.IsChecked = false;
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e) {
            List<LightBox> listSelectedBox = new List<LightBox>();
            foreach (LightBox box in lightBoxes) {
                if (box.isSelected) {
                    listSelectedBox.Add(box);
                }
            }
            setListBox(listSelectedBox);
        }

        private void setListBox(List<LightBox> listSelectedBox) {
            if (listSelectedBox.Count > 0) {
                WidLightBoxEdit wid = new WidLightBoxEdit(listSelectedBox[0].lBMsg);
                wid.ShowDialog();
                if (wid.DialogResult == true) {
                    double waitTime = wid.waitTime;
                    Color color = Colors.Gray;
                    foreach (LightBox box in lightBoxes) {
                        if (box.lBMsg.WaitTime == waitTime) {
                            color = box.oldColor;
                            break;
                        }
                    }
                    if (color == Colors.Gray) {
                        color = GetRandomColor();
                    }
                    foreach (LightBox box in listSelectedBox) {
                        box.lBMsg.WaitTime = waitTime;
                        box.lBMsg.RunTime = wid.runTime;
                        box.lBMsg.KeepTime = wid.keepTime;
                        box.lBMsg.RepeatCount = wid.repeatCount;
                        box.refresh();
                        box.setSelected();
                        box.setBackGroundColor(color);
                        LightBoxHelper.updateModelLBMsg(box.lBMsg);
                    }

                }
            }
        }

        private void Window_Closed(object sender, EventArgs e) {
            WriteXmlHelper.saveModel(LightBoxHelper.SelectedModel);
        }

        public Color GetRandomColor() {
            Random RandomNum_First = new Random((int)DateTime.Now.Ticks);
            //  对于C#的随机数，没什么好说的
            System.Threading.Thread.Sleep(RandomNum_First.Next(50));
            Random RandomNum_Sencond = new Random((int)DateTime.Now.Ticks);
            System.Threading.Thread.Sleep(RandomNum_First.Next(50));
            Random RandomNum_Third = new Random((int)DateTime.Now.Ticks);

            //  为了在白色背景上显示，尽量生成深色
            int int_Red = RandomNum_First.Next(256);
            int int_Green = RandomNum_Sencond.Next(256);
            int int_Blue = RandomNum_Third.Next(256);
            //int int_Blue = (int_Red + int_Green > 400) ? 0 : 400 - int_Red - int_Green;
            //int_Blue = (int_Blue > 255) ? 255 : int_Blue;

            return Color.FromArgb(255, (byte)int_Red, (byte)int_Green, (byte)int_Blue);
        }

        private void btnAllRunTime_Click(object sender, RoutedEventArgs e) {
            new WidAllRunTime(lBModel).ShowDialog();
        }

        private void btnAllKeepTime_Click(object sender, RoutedEventArgs e) {
            new WidAllKeepTime(lBModel).ShowDialog();
        }

        private void btnFirstId_Click(object sender, RoutedEventArgs e) {
            WidFirstId wid = new WidFirstId(minId);
            wid.ShowDialog();
            if(wid.DialogResult == true) {
                foreach(LBModel model in LightBoxHelper.listModel) {
                    refreshModelId(wid.newId, model);
                    WriteXmlHelper.saveModel(model);
                }
                refreshModelId(wid.newId, LightBoxHelper.SelectedModel);

                foreach (LightBox box in lightBoxes) {
                    box.refreshId();
                }
                minId = lBModel.getMinId();
            }

        }

        private void refreshModelId(int id, LBModel model) {
            foreach(LBMsg msg in model.ListLBMsg) {
                msg.Id = msg.Id - minId + id;
            }
        }
    }
}
