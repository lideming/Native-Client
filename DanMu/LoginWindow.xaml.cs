using System.Windows;
using System.Windows.Input;

namespace DanMu
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow() {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;

            InitializeComponent();

            imageBarCode.Visibility = Visibility.Collapsed;

            labelBarCode.MouseEnter += LabelBarCode_MouseEnter;
            labelBarCode.MouseLeave += LabelBarCode_MouseLeave;
        }

        private void LabelBarCode_MouseEnter(object sender, MouseEventArgs e) {
            imageBarCode.Visibility = Visibility.Visible;
        }
        private void LabelBarCode_MouseLeave(object sender, MouseEventArgs e) {
            imageBarCode.Visibility = Visibility.Collapsed;
        }
    }
}
