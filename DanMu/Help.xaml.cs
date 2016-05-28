using System.Windows;
using System.Windows.Input;

namespace DanmakuPie
{
    /// <summary>
    /// Help.xaml 的交互逻辑
    /// </summary>
    public partial class Help : Window
    {
        public Help() {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Image_MouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) {
                this.DragMove();
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void buttonMinimize_Click(object sender, RoutedEventArgs e) {
            this.WindowState = WindowState.Minimized;
        }
    }
}
