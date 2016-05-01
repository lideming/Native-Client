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

namespace DanMu
{
    /// <summary>
    /// AboutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow() {
            InitializeComponent();
        }
        private void hyperlinkMainSite_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("http://danmu.zhengzi.me");
        }
        private void hyperlinkGitHub_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/Project-Danmu");
        }
        private void hyperlinkMailTo_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("mailto:thesharing@163.com?subject=弹幕派使用反馈-版本号v1.2.0");  
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
