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
            System.Diagnostics.Process.Start("https://github.com/Project-Danmu");
        }
        private void hyperlinkMailTo_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("mailto:thesharing@163.com?subject=弹幕派使用反馈-版本号 v"+ System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());  
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
