using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DanmakuPie
{
    /// <summary>
    /// Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class MessageDialog : Window
    {
        public MessageDialog(string title, string message, string icon) {
            InitializeComponent();
            this.Title = title;
            this.text.Text = message;
            if(icon == "Warning") {
                this.icon.Source = new BitmapImage(new Uri("Picture/Icon/Warning.png", UriKind.Relative));
            }
            else if(icon == "Error") {
                this.icon.Source = new BitmapImage(new Uri("Picture/Icon/Error.png", UriKind.Relative));
            }
            else if(icon == "Done") {
                this.icon.Source = new BitmapImage(new Uri("Picture/Icon/Done.png", UriKind.Relative));
            }
            else {
                this.icon.Source = new BitmapImage(new Uri("Picture/Icon/info.png", UriKind.Relative));
            }
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
