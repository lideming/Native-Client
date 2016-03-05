using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Security.Cryptography;

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

            textBoxAccount.TextChanged += TextBoxAccount_TextChanged;
            passwordBox.PasswordChanged += PasswordBox_PasswordChanged;

            KeyDown += LoginWindow_KeyDown;

            buttonOk.IsEnabled = false;
        }

        private void LoginWindow_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                if (textBoxAccount.Text != "" && passwordBox.Password != "") {
                    using (MD5 md5Hash = MD5.Create()) {
                        string nameMD5 = account.GetMd5Hash(md5Hash, textBoxAccount.Text);
                        string passwordMD5 = account.GetMd5Hash(md5Hash, passwordBox.Password);
                        Debug.WriteLine("Account: " + nameMD5);
                        Debug.WriteLine("Password: " + passwordMD5);
                    }
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
            }
        }

        private void TextBoxAccount_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            if (textBoxAccount.Text == "" || passwordBox.Password == "") {
                buttonOk.IsEnabled = false;
            }
            else {
                buttonOk.IsEnabled = true;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e) {
            if (textBoxAccount.Text == "" || passwordBox.Password == "") {
                buttonOk.IsEnabled = false;
            }
            else {
                buttonOk.IsEnabled = true;
            }
        }


        private void LabelBarCode_MouseEnter(object sender, MouseEventArgs e) {
            imageBarCode.Visibility = Visibility.Visible;
        }
        private void LabelBarCode_MouseLeave(object sender, MouseEventArgs e) {
            imageBarCode.Visibility = Visibility.Collapsed;
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e) {
            using (MD5 md5Hash = MD5.Create()) {
                string nameMD5 = account.GetMd5Hash(md5Hash, textBoxAccount.Text);
                string passwordMD5 = account.GetMd5Hash(md5Hash, passwordBox.Password);
                Debug.WriteLine("Account: " + nameMD5);
                Debug.WriteLine("Password: " + passwordMD5);
            }
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
