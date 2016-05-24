using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using Microsoft.Win32;
using System.ComponentModel;
using System;
using System.Timers;
using WpfAnimatedGif;
using System.IO;
using System.Xml.Linq;
using System.Security.Permissions;
using System.Security;
using System.Threading;
using System.Reflection;

namespace DanmakuPie
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window, IDisposable
    {
        private BackgroundWorker loginBW = new BackgroundWorker(); // 后台获取网络数据的后台进程
        private System.Timers.Timer loginTimer = null;

        private string accountText;
        private string passwordText;

        public LoginWindow() {
            if (RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "").
                OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4.0\") == null) {
                this.Close();
            }

            InitializeComponent();

            loginBW.WorkerReportsProgress = true;
            loginBW.WorkerSupportsCancellation = true;
            loginBW.DoWork += new DoWorkEventHandler(LoginBW_DoWork);
            loginBW.ProgressChanged += new ProgressChangedEventHandler(LoginBW_ProgressChanged);
            loginBW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(LoginBW_RunWorkerCompleted);

            loginTimer = new System.Timers.Timer();
            loginTimer.Elapsed += new ElapsedEventHandler(LoginTimeOut);
            loginTimer.Interval = 15000;
            loginTimer.AutoReset = false;

            imageLoading.IsEnabled = false;
            CheckUpdate();
        }

        private void LoginWindow_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                buttonOk_Click(this, null);
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
            this.buttonOk.IsEnabled = false;
            this.buttonOk.Content = "登录中";
            imageLoading.IsEnabled = true;
            imageLoading.Visibility = Visibility.Visible;
            accountText = textBoxAccount.Text;
            passwordText = passwordBox.Password;
            loginTimer.Start();
            loginBW.RunWorkerAsync();
        }

        private bool Login() {
            string nameMD5;
            string passwordMD5;
            using (MD5 md5Hash = MD5.Create()) {
                nameMD5 = account.GetMd5Hash(md5Hash, accountText);
                passwordMD5 = account.GetMd5Hash(md5Hash, passwordText);
                Debug.WriteLine("Account: " + nameMD5);
                Debug.WriteLine("Password: " + passwordMD5);
            }

            try {
                using (WebClient client = new WebClient()) {
                    byte[] buffer = client.DownloadData("http://danmu.zhengzi.me/controller/desktop.php?user=" + accountText + "&hashPass=" + passwordMD5 + "&func=checkUsr");
                    string str = Encoding.GetEncoding("UTF-8").GetString(buffer, 0, buffer.Length);
                    if (str == "true") {
                        account.name = accountText;
                        account.password = passwordText;
                        account.nameMD5 = nameMD5;
                        account.passwordMD5 = passwordMD5;
                        Debug.WriteLine("Login Success.");
                        byte[] buffer2 = client.DownloadData("http://danmu.zhengzi.me/controller/desktop.php?user=" + accountText + "&hashPass=" + passwordMD5 + "&func=getRoomId");
                        string str2 = Encoding.GetEncoding("UTF-8").GetString(buffer2, 0, buffer2.Length);
                        if (str2 == "false") {
                            System.Windows.MessageBox.Show("未注册房间，请扫描二维码进入官网进行注册。", "登录失败",
                                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                        }
                        else {
                            if (str2.Length > 2)
                                setting.setRoomId(str2.Substring(1, str2.Length - 2));
                            System.Windows.MessageBox.Show("登录成功。", "弹幕派",
                                MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                            // 获取房间号
                            return true;
                        }
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
            return false;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void LoginBW_DoWork(Object sender, DoWorkEventArgs e) {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            e.Result = Login();
            backgroundWorker.ReportProgress(100);
        }

        private void LoginBW_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            return;
        }

        private void LoginBW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            buttonOk.IsEnabled = true;
            buttonOk.Content = "登录";
            loginTimer.Stop();
            imageLoading.Visibility = Visibility.Collapsed;
            imageLoading.IsEnabled = false;
            bool result = (bool)e.Result;
            if (e.Cancelled == false && e.Error == null && result) {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
        }

        private void LoginTimeOut(object sender, EventArgs e) {
            loginBW.CancelAsync();
            Debug.WriteLine("Time Out When Login In.");
            System.Windows.MessageBox.Show("登录超时，请重试。", "登录失败",
                                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~LoginWindow() {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (loginBW != null)
                    loginBW.Dispose();
                if (loginTimer != null)
                    loginTimer.Dispose();
            }
        }

        public void CheckUpdate() {
            Debug.WriteLine("Start.");
            System.Threading.ThreadPool.QueueUserWorkItem((s) =>
            {
                Debug.WriteLine("Checking Update.");
                string url = "http://7xr64j.com1.z0.glb.clouddn.com/update2.xml";
                var client = new System.Net.WebClient();
                client.DownloadDataCompleted += (x, y) =>
                {
                    Debug.WriteLine("Download Completed.");
                    if (y.Error == null) {
                        MemoryStream memoryStream = new MemoryStream(y.Result);
                        XDocument xDoc = XDocument.Load(memoryStream);
                        UpdateInfo updateInfo = new UpdateInfo();
                        XElement root = xDoc.Element("UpdateInfo");
                        updateInfo.AppName = root.Element("AppName").Value;
                        updateInfo.AppVersion = root.Element("AppVersion") == null ||
                        string.IsNullOrEmpty(root.Element("AppVersion").Value) ? null : new Version(root.Element("AppVersion").Value);
                        updateInfo.RequiredMinVersion = root.Element("RequiredMinVersion") == null || string.IsNullOrEmpty(root.Element("RequiredMinVersion").Value) ? null : new Version(root.Element("RequiredMinVersion").Value);
                        updateInfo.UpdateMode = root.Element("UpdateMode").Value;
                        updateInfo.Desc = root.Element("Description").Value;
                        updateInfo.MD5 = Guid.NewGuid();
                        memoryStream.Close();
                        CheckUpdateInfo(updateInfo);
                    }
                };
                client.DownloadDataAsync(new Uri(url));
            });
        }

        public void CheckUpdateInfo(UpdateInfo updateInfo) {
            if(updateInfo.UpdateMode == "UpdateToMin")
                if (updateInfo.RequiredMinVersion != null && System.Reflection.Assembly.GetExecutingAssembly().GetName().Version >= updateInfo.RequiredMinVersion)
                    return;
            if (updateInfo.UpdateMode == "UpdateToNew")
                if (updateInfo.AppVersion != null && System.Reflection.Assembly.GetExecutingAssembly().GetName().Version >= updateInfo.AppVersion)
                    return;
            Thread t = new Thread(new ThreadStart(() => {
                Dispatcher.BeginInvoke(new Action(() => {
                    UpdateWindow updateWindow = new UpdateWindow("UpdateAtStart", updateInfo);
                    updateWindow.Show();
                    this.Close();
                }));
            }));
            t.Start();    
        }
    }
}
