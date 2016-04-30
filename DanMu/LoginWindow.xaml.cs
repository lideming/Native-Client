using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using Microsoft.Win32;

namespace DanMu
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow() {
            if(RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "").
                OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4.0\") == null) {
                this.Close();
            }

            InitializeComponent();
        }

        private void LoginWindow_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                Login();
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
            Login();
        }

        private void Login() {
            string nameMD5;
            string passwordMD5;
            using (MD5 md5Hash = MD5.Create()) {
                nameMD5 = account.GetMd5Hash(md5Hash, textBoxAccount.Text);
                passwordMD5 = account.GetMd5Hash(md5Hash, passwordBox.Password);
                Debug.WriteLine("Account: " + nameMD5);
                Debug.WriteLine("Password: " + passwordMD5);
            }

            try {
                using (WebClient client = new WebClient()) {
                    byte[] buffer = client.DownloadData("http://danmu.zhengzi.me/controller/desktop.php?user=" + textBoxAccount.Text + "&hashPass=" + passwordMD5 + "&func=checkUsr");
                    string str = Encoding.GetEncoding("UTF-8").GetString(buffer, 0, buffer.Length);
                    if (str == "true") {
                        account.name = textBoxAccount.Text;
                        account.password = passwordBox.Password;
                        account.nameMD5 = nameMD5;
                        account.passwordMD5 = passwordMD5;
                        Debug.WriteLine("Login Success.");
                        byte[] buffer2 =  client.DownloadData("http://danmu.zhengzi.me/controller/desktop.php?user=" + textBoxAccount.Text + "&hashPass=" + passwordMD5 + "&func=getRoomId");
                        string str2 = Encoding.GetEncoding("UTF-8").GetString(buffer2, 0, buffer2.Length);
                        if (str2 == "false") {
                            System.Windows.MessageBox.Show("未注册房间，请扫描二维码进入官网进行注册。", "登录失败",
                                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                            return;
                        }
                        else {
                            if (str2.Length > 2)
                                setting.setRoomId(str2.Substring(1,str2.Length-2));
                        }
                        System.Windows.MessageBox.Show("登录成功。", "弹幕派",
                                MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                        // 获取房间号
                        MainWindow mainWindow = new MainWindow();
                        mainWindow.Show();
                        this.Close();
                    }
                    else {
                        try {
                            JObject parseResult = JObject.Parse(str);
                            dynamic dy1 = parseResult as dynamic;
                            if ((string)dy1["errType"] == "validErr") {
                                if ((int)dy1["errMark"] == 2) {
                                    Debug.WriteLine("Wrong UserName or Password.");
                                }
                            }
                        }
                        catch (Newtonsoft.Json.JsonReaderException error) {
                            // 处理JSON解析错误
                            Debug.WriteLine("Error: JsonReaderException");
                            Debug.WriteLine("Return: " + str);
                        }
                        finally {
                            System.Windows.MessageBox.Show("用户名或密码错误，请检查。", "登录失败",
                                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                        }
                    }
                }
            }
            catch (System.Net.WebException) {
                System.Windows.MessageBox.Show("未连接到网络，请检查网络设置。", "登录失败",
                    MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

    }
}
