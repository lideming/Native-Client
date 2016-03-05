using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;
using System.IO;
using Newtonsoft.Json.Linq;

namespace DanMu
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static int NUM = setting.getNUM(); //在开始时获取设定的弹幕数量，重新设置弹幕数量需要重启方可生效
        private static double screenWidth = SystemParameters.PrimaryScreenWidth;
        private static double screenHeight = SystemParameters.PrimaryScreenHeight;
        private static List<string> danmuStorage = new List<string>();
        private static bool BOOLDISPLAYTIP = false;
        Boolean [] isExist = new Boolean[NUM]; //栈模式，栈的对应空间是否有弹幕
        Boolean isStop = false;
        int time = setting.getDURATION() -1; //时间片，用于计算弹幕获取间隔，起始时间设置为间隔-1，方便一运行就出弹幕
        private System.Timers.Timer mainTimer = null; //计时器
        private Timer getWebContentTimer = null;
        private Timer displayRoomNumTimer = null;

        private BackgroundWorker fetchBW = new BackgroundWorker();

        private delegate void DispatcherDelegateTimer(); //时间更新函数
        private delegate void DispatcherDelegateFetchWebContent(int num);

        private System.Windows.Forms.NotifyIcon notifyIcon;

        private TextBlock textRoomNum;

        class fetchedDanmu
        {
            public int num;
            public List<string> contentList;

            public fetchedDanmu(int num, List<string> contentList) {
                this.num = num;
                this.contentList = contentList;
            }
        }

        public MainWindow() {

            //初始化isExist
            for (int i = 0; i < NUM; i++) {
                isExist[i] = false;
            }

            //初始化窗口元素并显示
            InitializeComponent();

            InitialTray();

            RestoreSetting();

            fetchBW.WorkerReportsProgress = true;
            fetchBW.WorkerSupportsCancellation = true;
            fetchBW.DoWork += new DoWorkEventHandler(FetchBW_DoWork);
            fetchBW.ProgressChanged += new ProgressChangedEventHandler(FetchBW_ProgressChanged);
            fetchBW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(FetchBW_RunWorkerCompleted);
            for (int i = 0; i < NUM; i++) {
                TextBlock text1 = new TextBlock();
                text1.Text = "";
                text1.VerticalAlignment = VerticalAlignment.Top;
                text1.HorizontalAlignment = HorizontalAlignment.Right;
                text1.FontSize = setting.getFontSize();
                text1.Foreground = setting.getForeground();
                text1.Background = setting.getBackground();
                text1.FontFamily = setting.getFontFamily();
                text1.FontStyle = setting.getFontStyle();
                text1.FontWeight = setting.getFontWeight();
                grid.Children.Add(text1);
                grid.RegisterName("newText" + i.ToString(), text1);
            }

            textRoomNum = new TextBlock();
            textRoomNum.Text = "房间号：";
            textRoomNum.Foreground = Brushes.Black;
            textRoomNum.Background = Brushes.White;
            textRoomNum.FontSize = 36;
            grid.Children.Add(textRoomNum);
            grid.RegisterName("textblockRoomNum",textRoomNum);
            textRoomNum.Margin = new Thickness(screenWidth / 2 - 200, screenHeight / 2 - 20, screenWidth / 2 - 200, screenHeight / 2 - 20);
            textRoomNum.Visibility = Visibility.Collapsed;
            
            
            //开始计时
            mainTimer = new System.Timers.Timer();//这个1000是计时器的计时区间
            mainTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            mainTimer.Interval = 10; //计时间隔

            getWebContentTimer = new Timer();
            getWebContentTimer.Elapsed += new ElapsedEventHandler(getWebContentTimeOut);
            getWebContentTimer.Interval = 2000;
            getWebContentTimer.AutoReset = false;

            displayRoomNumTimer = new Timer();
            displayRoomNumTimer.Elapsed += new ElapsedEventHandler(displayRoomNumTimeOut);
            displayRoomNumTimer.Interval = 10000;
            displayRoomNumTimer.AutoReset = false;

            mainTimer.Start();
        }

        private void OnTimedEvent(object sender, EventArgs e){
            this.Dispatcher.Invoke(DispatcherPriority.Normal,new DispatcherDelegateTimer(UpdateUI));
        }
 
        private void UpdateUI(){

            time++; //时间片增加
            Boolean isTime = false; //判断是否该去服务器fetch内容

            if (time >= setting.getDURATION()) {
                isTime = true;
                time = 0;
            }

            for(int i = 0; i < NUM; i++) { //对isExist进行遍历
                if (isExist[i] == true) { //如果说有弹幕，就把弹幕移动一下，当移出屏幕时将弹幕删除
                    TextBlock textTemp = grid.FindName("newText"+i.ToString()) as TextBlock;
                    //Debug.WriteLine(textTemp.Text);
                    if (textTemp != null) {
                        //先移动弹幕
                        textTemp.Margin = new Thickness(textTemp.Margin.Left - setting.getSPEED(),
                            textTemp.Margin.Top, textTemp.Margin.Right + setting.getSPEED(), textTemp.Margin.Bottom);
                        if (textTemp.Margin.Left < -1600) {
                            //如果移出屏幕了，就把这个弹幕移除
                            RemoveText(i);
                            if((grid.FindName("newText" + i.ToString()) as TextBlock) != null) {
                                //写一个error函数
                            }//if
                        }//if
                    }//if
                }//if
                else {
                    if (isTime == true) {
                        UpdateText(i);
                        isTime = false;//每一个获取周期仅获取一次
                    }//if
                }//else
            }//for
        }

        private void FetchBW_DoWork(Object sender, DoWorkEventArgs e) {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            //currentFetchNum = (int)e.Argument;
            //if (isExist[currentFetchNum] != true) {
            //建立一个新的文字块，然后从网上获取信息，设置好文字块的属性，加入到Grid中
            string textFetched = GetWebContent(setting.getSOURCE());
            int num;
            List<string> contentList = new List<string>();
            if (textFetched == "网络未连接。") {
                num = 1;
                contentList.Add(textFetched);
            }
            else {
                num = 0;
                try {
                    JObject parseResult = JObject.Parse(textFetched);
                    dynamic dy1 = parseResult as dynamic;
                    num = (int)dy1["seqNum"];
                    JArray dataArray = ((JArray)dy1["seqData"]);
                    if (dataArray != null) {
                        JToken data = dataArray.First;
                        while (data != null) {
                            contentList.Add(data.ToString());
                            data = data.Next;
                        }
                    }
                }
                catch (Newtonsoft.Json.JsonReaderException error) {
                    Debug.WriteLine("Error: JsonReaderException");
                    Debug.WriteLine("Fetch Text: " + textFetched);
                }
            }
            fetchedDanmu result = new fetchedDanmu(num, contentList);
            Debug.WriteLine("Get " + contentList.Count.ToString() + " Results.");
            e.Result = result;
            backgroundWorker.ReportProgress(100);
            //}
        }

        private void FetchBW_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            return;
        }

        private void FetchBW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            //string textFetched;
            if (e.Cancelled == false && e.Error == null) {
                fetchedDanmu result = e.Result as fetchedDanmu;
                while (result.contentList.Count > 0) {
                    danmuStorage.Add(result.contentList[0]);
                    result.contentList.RemoveAt(0);
                }
            }
            else {
                Debug.WriteLine("获取时出现错误");
            }
            getWebContentTimer.Stop();
            //写两个函数 一个取数据 一个更新界面
        }

        private void getWebContentTimeOut(object sender, EventArgs e) {
            fetchBW.CancelAsync();
            Debug.WriteLine("Time Out When Get Web Content.");
        }

        private void UpdateText(int num) {
            //从网络上获取字符串
            if (isExist[num] != true) {
                //建立一个新的文字块，然后从网上获取信息，设置好文字块的属性，加入到Grid中
                if (danmuStorage.Count > 0) {
                    TextBlock text1 = grid.FindName("newText" + num.ToString()) as TextBlock;
                    text1.Text = danmuStorage[0];
                    danmuStorage.RemoveAt(0);
                    //设置对齐方式
                    Random ran = new Random();
                    int RandKey = ran.Next(0, (int)screenHeight - 10);
                    text1.Margin = new Thickness(0, RandKey, 0, 0); //LEFT TOP RIGHT BOTTOM
                    isExist[num] = true;
                }
                if(danmuStorage.Count < NUM) {
                    if (fetchBW.IsBusy == false) {
                        getWebContentTimer.Start();
                        fetchBW.RunWorkerAsync();
                    }
                    //this.Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherDelegateFetchWebContent(this.updateText), i);
                }
            }
        }

        private void RemoveText(int num) {
            //移除弹幕
            TextBlock textTemp = grid.FindName("newText"+num.ToString()) as TextBlock;
            if (textTemp != null) {
                textTemp.Text = "";
                isExist[num] = false;
            }
            isExist[num] = false;
        }

        private string GetWebContent(string url) {
            //获取网页信息，当无法连接到网络时提示
            try {
                using (WebClient client = new WebClient()) {
                    byte[] buffer = client.DownloadData(url);
                    string str = Encoding.GetEncoding("UTF-8").GetString(buffer, 0, buffer.Length);
                    return str;
                }
            }
            catch (System.Net.WebException) {
                return "网络未连接。";
            }
        }

        private void RestoreSetting() {
            try {
                string settingFilePath = System.Windows.Forms.Application.StartupPath + "\\setting.ini";
                StreamReader settingFileSR = new StreamReader(settingFilePath, Encoding.UTF8);
                settingFileSR.ReadLine();
                string str = settingFileSR.ReadLine();
                str = str.Substring("Num = ".Length, str.Length - "Num = ".Length);
                setting.setNUM(Int32.Parse(str));
                str = settingFileSR.ReadLine();
                str = str.Substring("Duration = ".Length, str.Length - "Duration = ".Length);
                setting.setDURATION(Int32.Parse(str));
                str = settingFileSR.ReadLine();
                str = str.Substring("Speed = ".Length, str.Length - "Speed = ".Length);
                setting.setSPEED(Int32.Parse(str));
                str = settingFileSR.ReadLine();
                str = str.Substring("Source = ".Length, str.Length - "Source = ".Length);
                setting.setSOURCE(str);
                settingFileSR.ReadLine();
                str = settingFileSR.ReadLine();
                str = str.Substring("Background = ".Length, str.Length - "Background = ".Length);
                setting.setBackground(str);
                str = settingFileSR.ReadLine();
                str = str.Substring("Foreground = ".Length, str.Length - "Foreground = ".Length);
                setting.setForeground(str);
                str = settingFileSR.ReadLine();
                str = str.Substring("FontFamily = ".Length, str.Length - "FontFamily = ".Length);
                setting.setFontFamily(str);
                str = settingFileSR.ReadLine();
                str = str.Substring("FontStyle = ".Length, str.Length - "FontStyle = ".Length);
                if(str == "Italic") {
                    setting.setFontStyle(FontStyles.Italic);
                }
                else {
                    setting.setFontStyle(FontStyles.Normal);
                }
                str = settingFileSR.ReadLine();
                str = str.Substring("FontWeight = ".Length, str.Length - "FontWeight = ".Length);
                if(str == "Bold") {
                    setting.setFontWeight(FontWeights.Bold);
                }
                else {
                    setting.setFontWeight(FontWeights.Normal);
                }
                str = settingFileSR.ReadLine();
                str = str.Substring("FontSize = ".Length, str.Length - "FontSize = ".Length);
                setting.setFontSize(Int32.Parse(str));
                str = settingFileSR.ReadLine();
                str = str.Substring("Opactity = ".Length, str.Length - "Opactity = ".Length);
                setting.setOpactity(Double.Parse(str));
                settingFileSR.Close();
            }
            catch (FileNotFoundException e) {
                System.Windows.MessageBox.Show("未找到配置文件，将默认初始化。", "云弹幕",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                setting.SaveSetting();
            }
            catch(FormatException e) {
                System.Windows.MessageBox.Show("配置文件中存在格式错误。", "云弹幕",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
            catch(OverflowException e) {
                System.Windows.MessageBox.Show("配置文件中存在参数错误。", "云弹幕",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
            catch(IOException e) {
                System.Windows.MessageBox.Show("配置文件存在错误。", "云弹幕",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
            catch(NullReferenceException e) {
                System.Windows.MessageBox.Show("配置文件存在错误。", "云弹幕",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
        }

        System.Windows.Forms.MenuItem menuStop;
        System.Windows.Forms.MenuItem menuDisplayRoomNum;

        void InitialTray() {
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Text = "Danmu";
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            notifyIcon.Visible = true;
            notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(notifyIcon_MouseClick);

            menuDisplayRoomNum = new System.Windows.Forms.MenuItem("显示房间号");
            menuStop = new System.Windows.Forms.MenuItem("停止");
            System.Windows.Forms.MenuItem menuSetting = new System.Windows.Forms.MenuItem("设置...");
            System.Windows.Forms.MenuItem menuAbout = new System.Windows.Forms.MenuItem("关于...");
            System.Windows.Forms.MenuItem menuExit = new System.Windows.Forms.MenuItem("退出");

            menuDisplayRoomNum.Click += new EventHandler(display_Click);
            menuStop.Click += new EventHandler(stop_Click);
            menuSetting.Click += new EventHandler(setting_Click);
            menuAbout.Click += new EventHandler(about_Click);
            menuExit.Click += new EventHandler(exit_Click);

            System.Windows.Forms.MenuItem[] children = new System.Windows.Forms.MenuItem[]
            {
                menuDisplayRoomNum, menuStop, menuSetting, menuAbout, menuExit
            };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(children);

            this.StateChanged += new EventHandler(Systray_StateChanged);
        }

        void notifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e) {
            if(e.Button == System.Windows.Forms.MouseButtons.Left) {
                if(this.Visibility == Visibility.Visible) {
                    this.Visibility = Visibility.Hidden;
                    if (BOOLDISPLAYTIP == false) {
                        notifyIcon.BalloonTipText = "程序已最小化至系统托盘。";
                        notifyIcon.ShowBalloonTip(2000);
                        BOOLDISPLAYTIP = true;
                    }
                }
                else {
                    this.Visibility = Visibility.Visible;
                    this.Activate();
                }
            }
        }

        void display_Click(object sender, EventArgs e) {
            if (textRoomNum.Visibility == Visibility.Collapsed) {
                textRoomNum.Visibility = Visibility.Visible;
                displayRoomNumTimer.Start();
                menuDisplayRoomNum.Text = "隐藏房间号";
            }
            else {
                textRoomNum.Visibility = Visibility.Collapsed;
                displayRoomNumTimer.Stop();
                menuDisplayRoomNum.Text = "显示房间号";
            }
        }

        private void displayRoomNumTimeOut(object sender, EventArgs e) {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new DispatcherDelegateTimer(SetRoomNumInvisible));           
        }

        private void SetRoomNumInvisible() {
            textRoomNum.Visibility = Visibility.Collapsed;
            menuDisplayRoomNum.Text = "显示房间号";
        }

        void stop_Click(object sender, EventArgs e) {
            if (isStop) {
                mainTimer.Start();
                menuStop.Text = "停止";
                isStop = false;
            }
            else {
                mainTimer.Stop();
                menuStop.Text = "继续";
                isStop = true;
            }
        }

        void setting_Click(object sender, EventArgs e) {
            UserSetting userSetting = new UserSetting();
            userSetting.settingChangeEvent += new UserSetting.settingChangeDelegate(settingChangeFunction);
            userSetting.Show();
        }

        void about_Click(object sender, EventArgs e) {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Show();
        }

        void exit_Click(object sender, EventArgs e) {
            this.Close();
        }

        void Systray_StateChanged(object sender, EventArgs e) {
            if(this.WindowState == WindowState.Minimized) {
                this.Visibility = Visibility.Hidden;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            if (System.Windows.MessageBox.Show("退出云弹幕？", "云弹幕", 
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes) {
                notifyIcon.Dispose();
                System.Windows.Application.Current.Shutdown();
            }
            else {
                e.Cancel = true;
            }
        }

        private void settingChangeFunction(object sender, EventArgs e) {
            Debug.WriteLine("Setting Changed.");
            if(setting.SaveSetting()== false) {
                Debug.WriteLine("设置文件未能保存。");
            }
            for(int i = 0;i< NUM; i++) {
                TextBlock textTemp = grid.FindName("newText" + i.ToString()) as TextBlock;
                textTemp.FontSize = setting.getFontSize();
                textTemp.FontFamily = setting.getFontFamily();
                textTemp.Foreground = setting.getForeground();
                textTemp.FontStyle = setting.getFontStyle();
                textTemp.FontWeight = setting.getFontWeight();
            }
        }
    }
}
