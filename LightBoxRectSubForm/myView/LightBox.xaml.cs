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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LightBoxRectSubForm.myView {
    /// <summary>
    /// LightBox.xaml 的交互逻辑
    /// </summary>
    public partial class LightBox : UserControl {

        public event EventHandler rightClickEvent;
        public event EventHandler clickEvent;
        public event EventHandler dblClickEvent;

        private DateTime clickTime = DateTime.Now;
        public LBMsg lBMsg;
        public bool isSelected = false;
        public Color oldColor = Colors.Gray;

        public LightBox(LBMsg lBMsg) {
            InitializeComponent();
            this.lBMsg = lBMsg;
            //cbAddr.Content = lBMsg.Row1 + "-" + lBMsg.Column;
            labelAddr.Content = lBMsg.Row1 + "-" + lBMsg.Column;
            labelId.Content = lBMsg.Id;
            labelWaitTime.Content = lBMsg.WaitTime;
            labelWaitTime.Background = new SolidColorBrush(Color.FromArgb(150, 10, 10, 10));
            labelAddr.Background = new SolidColorBrush(Color.FromArgb(150, 10, 10, 10));
            labelId.Background = new SolidColorBrush(Color.FromArgb(150, 10, 10, 10));

            //Point p = new Point(this.Width * lBMsg.Column + lBMsg.Column * 1 + 1,
            //    this.Height * lBMsg.Row1 + lBMsg.Row1 * 1 + 1);

            //Canvas.SetLeft(this, this.Width * lBMsg.Column + lBMsg.Column * 1 + 2);
            //Canvas.SetTop(this, this.Height * lBMsg.Row1 + lBMsg.Row1 * 1 + 200);

        }

        public void refresh() {
            labelWaitTime.Content = lBMsg.WaitTime;
        }

        public void refreshId() {
            labelId.Content = lBMsg.Id;
        }

        public void setBackGroundColor(Color color) {
            oldColor = color;
            root.Background = new SolidColorBrush(color);
        }

        private void cbAddr_Checked(object sender, RoutedEventArgs e) {
            isSelected = true;
            root.Background = new SolidColorBrush(Colors.White);
        }

        private void cbAddr_Unchecked(object sender, RoutedEventArgs e) {
            isSelected = false;
            root.Background = new SolidColorBrush(oldColor);
        }

        public void setSelected() {
            cbAddr.IsChecked = !cbAddr.IsChecked;
        }

        public static Color GetIndexColor(int index) {
            int c = 0x000010 + (index * 100);
            return RgbToColor(c);
        }

        public static Color RgbToColor(int color) {
            return Color.FromArgb(255, (byte)((color & 0xff0000) >> 16), (byte)((color & 0x00ff00) >> 8), (byte)(color & 0x0000ff));
        }

        private void root_MouseDown(object sender, MouseButtonEventArgs e) {
            TimeSpan span = DateTime.Now - clickTime;
            if(span.Milliseconds < 1000) {
                clickEvent?.Invoke(this, null);
            } else {
                clickTime = DateTime.Now;
            }

            //if (e.RightButton == MouseButtonState.Pressed) {
            //    rightClickEvent?.Invoke(this, null);
            //    //cbAddr.IsChecked = true;
            //} else if(e.LeftButton == MouseButtonState.Pressed){
            //    clickEvent?.Invoke(this, null);
            //}
        }

        private void root_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            TimeSpan span = DateTime.Now - clickTime;
            if (span.TotalMilliseconds < 260) {
                clickEvent?.Invoke(this, null);
                cbAddr.IsChecked = false;
            } else {
                clickTime = DateTime.Now;
                setSelected();
                //clickEvent?.Invoke(this, null);
            }
        }

        private void root_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            rightClickEvent?.Invoke(this, null);
        }
    }
}
