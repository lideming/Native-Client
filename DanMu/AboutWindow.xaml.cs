using System.Windows;

namespace DanmakuPie
{
    /// <summary>
    /// AboutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow() {
            InitializeComponent();
            labelVersion.Content = "v"+System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        private void hyperlinkMainSite_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("http://danmu.zhengzi.me");
        }
        private void hyperlinkGitHub_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/DanmakuPie");
        }
        private void hyperlinkMailTo_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("mailto:thesharing@163.com?subject=弹幕派使用反馈-版本号 v"+ System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());  
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void buttonMinimize_Click(object sender, RoutedEventArgs e) {
            this.WindowState = WindowState.Minimized;
        }

        private void Image_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) {
                this.DragMove();
            }
        }
    }
}
