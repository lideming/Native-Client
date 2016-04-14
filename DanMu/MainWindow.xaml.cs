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
        private static int NUM = setting.getNUM(); // 在开始时获取设定的弹幕数量，重新设置弹幕数量需要重启方可生效
        private static double screenWidth = SystemParameters.PrimaryScreenWidth; // 获取屏幕的长和宽
        private static double screenHeight = SystemParameters.PrimaryScreenHeight;

        private static List<string> danmuStorage = new List<string>(); // 存储从网络获取的弹幕信息
                                                                       //为了保证获取速度，先后台获取多条弹幕，然后缓存起来

        Boolean hasDisplayedBalloonTip = false; // 是否已经显示过气泡提示
        // 气泡提示用于第一次最小化时提示用户软件被最小化至系统托盘
        Boolean [] isExist = new Boolean[NUM]; // 用于显示对应Textblock是否对应的有弹幕
        Boolean isStop = false; // 是否被停止

        int time = setting.getDURATION() -1; // 时间片，用于计算弹幕获取间隔，起始时间设置为间隔-1，方便一运行就出弹幕
        private System.Timers.Timer mainTimer = null; // 主计时器
        private Timer getWebContentTimer = null; // 从网络获取数据的超时计时器
        private Timer displayRoomNumTimer = null; // 显示房间号的超时计时器

        private BackgroundWorker fetchBW = new BackgroundWorker(); // 后台获取网络数据的后台进程

        private delegate void DispatcherDelegateTimer(); // UI更新函数
        // 使用：this.Dispatcher.Invoke(DispatcherPriority.Normal,new DispatcherDelegateTimer(UpdateUI));
        // private delegate void DispatcherDelegateFetchWebContent(int num); //

        private System.Windows.Forms.NotifyIcon notifyIcon; // 托盘图标
        private TextBlock textBlockRoomNum; // 显示房间号的TextBlock

        class fetchedData
            // 用于存储从网络获取的弹幕内容
        {
            public int num;
            public List<string> contentList;

            public fetchedData(int num, List<string> contentList) {
                this.num = num;
                this.contentList = contentList;
            }
        }

        public MainWindow() {

            // 初始化isExist
            for (int i = 0; i < NUM; i++) {
                isExist[i] = false;
            }

            // 初始化窗口元素并显示
            InitializeComponent();

            // 初始化系统托盘图标
            InitialTray();

            // 尝试从 setting.ini 中恢复设置
            RestoreSetting();

            // 设置后台进程的属性
            fetchBW.WorkerReportsProgress = true;
            fetchBW.WorkerSupportsCancellation = true;
            fetchBW.DoWork += new DoWorkEventHandler(FetchBW_DoWork);
            fetchBW.ProgressChanged += new ProgressChangedEventHandler(FetchBW_ProgressChanged);
            fetchBW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(FetchBW_RunWorkerCompleted);

            // 初始化弹幕Textblock的各属性
            for (int i = 0; i < NUM; i++) {
                TextBlock text = new TextBlock();
                text.Text = "";
                text.VerticalAlignment = VerticalAlignment.Top;
                text.HorizontalAlignment = HorizontalAlignment.Right;
                text.FontSize = setting.getFontSize();
                text.Foreground = setting.getForeground();
                text.Background = setting.getBackground();
                text.FontFamily = setting.getFontFamily();
                text.FontStyle = setting.getFontStyle();
                text.FontWeight = setting.getFontWeight();
                grid.Children.Add(text);
                grid.RegisterName("textBlockDanmu" + i.ToString(), text);
            }

            // 初始化房间号textBlockRoomNum
            textBlockRoomNum = new TextBlock();
            textBlockRoomNum.Text = "房间号：";
            textBlockRoomNum.Foreground = Brushes.Black;
            textBlockRoomNum.Background = Brushes.White;
            textBlockRoomNum.FontSize = 36;
            grid.Children.Add(textBlockRoomNum);
            grid.RegisterName("textblockRoomNum",textBlockRoomNum);
            textBlockRoomNum.Margin = new Thickness(screenWidth / 2 - 200, screenHeight / 2 - 20, 
                screenWidth / 2 - 200, screenHeight / 2 - 20);
            textBlockRoomNum.Visibility = Visibility.Collapsed;
            
            // 设置各计时器的属性
            mainTimer = new System.Timers.Timer();
            mainTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            mainTimer.Interval = 10; // 计时间隔 = 10

            getWebContentTimer = new Timer();
            getWebContentTimer.Elapsed += new ElapsedEventHandler(getWebContentTimeOut);
            getWebContentTimer.Interval = 2000;
            getWebContentTimer.AutoReset = false; // 不会自动重置计时器，即只计时一次

            displayRoomNumTimer = new Timer();
            displayRoomNumTimer.Elapsed += new ElapsedEventHandler(displayRoomNumTimeOut);
            displayRoomNumTimer.Interval = 10000;
            displayRoomNumTimer.AutoReset = false;

            // 主计时器开始计时
            mainTimer.Start();
        }




        /// <summary>
        /// 主计时器负责唤醒UpdateUI
        /// </summary>
        private void OnTimedEvent(object sender, EventArgs e){
            // displayRoomNumTimer.Elapsed += new ElapsedEventHandler(displayRoomNumTimeOut);
            this.Dispatcher.Invoke(DispatcherPriority.Normal,new DispatcherDelegateTimer(UpdateUI));
        }

        /// <summary>
        /// 更新界面并定期从网络获取弹幕数据
        /// </summary>
        private void UpdateUI(){

            Boolean isTime = false; // 用于判断是否该去服务器fetch内容

            time++; // 时间片增加
            if (time >= setting.getDURATION()) {
                isTime = true;
                time = 0;
            }

            // 对isExist进行遍历
            for (int i = 0; i < NUM; i++) { 
                if (isExist[i] == true) { // 如果说有弹幕，就把弹幕移动一下，当移出屏幕时将弹幕删除
                    TextBlock textTemp = grid.FindName("textBlockDanmu"+i.ToString()) as TextBlock;
                    if (textTemp != null) {
                        //先移动弹幕
                        textTemp.Margin = new Thickness(textTemp.Margin.Left - setting.getSPEED(),
                            textTemp.Margin.Top, textTemp.Margin.Right + setting.getSPEED(), textTemp.Margin.Bottom);
                        if (textTemp.Margin.Left < -1600) {
                            //如果移出屏幕了，就把这个弹幕移除
                            RemoveText(i);
                        }// if (textTemp.Margin.Left < -1600)
                    }// if (textTemp != null)
                }// if (isExist[i] == true)
                else {
                    if (isTime == true) {
                        // 从网络获取弹幕
                        UpdateText(i);
                        isTime = false; // 每一个获取周期仅获取一次
                    }// if (isTime == true)
                }// else
            }// for
        }

        /// <summary>
        /// 从网络上获取字符串
        /// </summary>
        /// <param name="num"></param>
        private void UpdateText(int num){
            if (isExist[num] != true){
                //建立一个新的文字块，然后从网上获取信息，设置好文字块的属性，加入到Grid中
                if (danmuStorage.Count > 0){
                    TextBlock text1 = grid.FindName("textBlockDanmu" + num.ToString()) as TextBlock;
                    text1.Text = danmuStorage[0];
                    danmuStorage.RemoveAt(0);
                    //设置对齐方式
                    Random ran = new Random();
                    int RandKey = ran.Next(0, (int)screenHeight - 10);
                    text1.Margin = new Thickness(0, RandKey, 0, 0); //LEFT TOP RIGHT BOTTOM
                    isExist[num] = true;
                }
                if (danmuStorage.Count < NUM){
                    if (fetchBW.IsBusy == false){
                        getWebContentTimer.Start();
                        fetchBW.RunWorkerAsync();
                    }
                    //this.Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherDelegateFetchWebContent(this.updateText), i);
                }
            }
        }

        


        /// <summary>
        /// DoWrok开始从后台获取弹幕内容
        /// 将获取到的内容保存至e.Result中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FetchBW_DoWork(Object sender, DoWorkEventArgs e) {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            string textFetched = GetWebContent(setting.getSOURCE());
            int num;
            List<string> contentList = new List<string>();

            if (textFetched == "网络未连接。") {
                num = 1;
                contentList.Add(textFetched); 
                // 如果网络未链接，则在弹幕中提醒
            }
            else {
                num = 0;
                try {
                    Debug.WriteLine("Fetch:"+textFetched);
                    // 将获取到的JSON进行解析得到获取的弹幕数量以及所有弹幕
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
                    // 处理JSON解析错误，这里Debug提示后直接舍弃
                    Debug.WriteLine("Error: JsonReaderException");
                    Debug.WriteLine("Fetch Text: " + textFetched);
                }
            }
            // 将解析结果保存至e.Result中供RunWorkerCompleted使用
            fetchedData result = new fetchedData(num, contentList);
            Debug.WriteLine("Get " + contentList.Count.ToString() + " Results.");
            e.Result = result;
            backgroundWorker.ReportProgress(100); // 当Dowork完成时直接将进度设为100%，执行RunWorkerCompleted
        }

        private void FetchBW_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            return;
        }

        /// <summary>
        /// DoWork完成时，将获取到的弹幕保存至fetchedData供UpdateText使用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FetchBW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (e.Cancelled == false && e.Error == null) {
                fetchedData result = e.Result as fetchedData;
                while (result.contentList.Count > 0) {
                    danmuStorage.Add(result.contentList[0]);
                    result.contentList.RemoveAt(0);
                }
            }
            else {
                Debug.WriteLine("获取时出现错误");
            }
            getWebContentTimer.Stop();
        }

        /// <summary>
        /// 获取网页信息，当无法连接到网络时提示
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string GetWebContent(string url) {
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

        /// <summary>
        /// 当获取超时时终止后台进程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getWebContentTimeOut(object sender, EventArgs e) {
            fetchBW.CancelAsync();
            Debug.WriteLine("Time Out When Get Web Content.");
        }



        /// <summary>
        /// 移除弹幕，将isExist[i]设为false
        /// </summary>
        /// <param name="num"></param>
        private void RemoveText(int num) {
            TextBlock textTemp = grid.FindName("textBlockDanmu"+num.ToString()) as TextBlock;
            if (textTemp != null) {
                textTemp.Text = "";
                isExist[num] = false;
            }
            isExist[num] = false;
        }



        /// <summary>
        /// 从本地setting.ini恢复设置
        /// </summary>
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
            catch(ArgumentOutOfRangeException e) {
                System.Windows.MessageBox.Show("配置文件存在错误。", "云弹幕",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
            catch{
                System.Windows.MessageBox.Show("Fatal Error.", "云弹幕",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
        }

        System.Windows.Forms.MenuItem menuStop;
        System.Windows.Forms.MenuItem menuDisplayRoomNum;

        /// <summary>
        /// 初始化系统托盘
        /// </summary>
        void InitialTray() {
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Text = "云弹幕";
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

            System.Windows.Forms.MenuItem[] children = new System.Windows.Forms.MenuItem[]{
                menuDisplayRoomNum, menuStop, menuSetting, menuAbout, menuExit
            };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(children);

            this.StateChanged += new EventHandler(Systray_StateChanged);
        }

        // 在系统托盘上点击鼠标时最小化 
        void notifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e) {
            if(e.Button == System.Windows.Forms.MouseButtons.Left) {
                if(this.Visibility == Visibility.Visible) {
                    this.Visibility = Visibility.Hidden;
                    if (hasDisplayedBalloonTip == false) {
                        notifyIcon.BalloonTipText = "程序已最小化至系统托盘。";
                        notifyIcon.ShowBalloonTip(2000);
                        hasDisplayedBalloonTip = true;
                    }
                }
                else {
                    this.Visibility = Visibility.Visible;
                    this.Activate();
                }
            }
        }

        // 点击“显示房间号”时，在屏幕上显示房间号
        void display_Click(object sender, EventArgs e) {
            if (textBlockRoomNum.Visibility == Visibility.Collapsed) {
                textBlockRoomNum.Visibility = Visibility.Visible;
                displayRoomNumTimer.Start();
                menuDisplayRoomNum.Text = "隐藏房间号";
            }
            else {
                textBlockRoomNum.Visibility = Visibility.Collapsed;
                displayRoomNumTimer.Stop();
                menuDisplayRoomNum.Text = "显示房间号";
            }
        }

        // 当显示房间号一段时间后自动隐藏
        private void displayRoomNumTimeOut(object sender, EventArgs e) {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new DispatcherDelegateTimer(SetRoomNumInvisible));           
        }

        // 隐藏房间号
        private void SetRoomNumInvisible() {
            textBlockRoomNum.Visibility = Visibility.Collapsed;
            menuDisplayRoomNum.Text = "显示房间号";
        }

        // 点击“停止/继续”时，停止/继续主时钟
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

        // 点击“设置…”时，打开设置窗口
        void setting_Click(object sender, EventArgs e) {
            UserSetting userSetting = new UserSetting();
            userSetting.settingChangeEvent += new UserSetting.settingChangeDelegate(settingChangeFunction);
            userSetting.Show();
        }

        // 点击“关于…”时，打开关于窗口
        void about_Click(object sender, EventArgs e) {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Show();
        }

        // 点击“退出”时，触发Close事件
        void exit_Click(object sender, EventArgs e) {
            this.Close();
        }

        // 当窗口状态变化（最小化）时
        void Systray_StateChanged(object sender, EventArgs e) {
            if(this.WindowState == WindowState.Minimized) {
                this.Visibility = Visibility.Hidden;
            }
        }

        // 当退出时，进行问询
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




        /// <summary>
        /// 当用户设置完成时，重新渲染所有内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void settingChangeFunction(object sender, EventArgs e) {
            Debug.WriteLine("Setting Changed.");
            if(setting.SaveSetting()== false) {
                Debug.WriteLine("设置文件未能保存。");
            }
            for(int i = 0;i< NUM; i++) {
                TextBlock textTemp = grid.FindName("textBlockDanmu" + i.ToString()) as TextBlock;
                textTemp.FontSize = setting.getFontSize();
                textTemp.FontFamily = setting.getFontFamily();
                textTemp.Foreground = setting.getForeground();
                textTemp.FontStyle = setting.getFontStyle();
                textTemp.FontWeight = setting.getFontWeight();
            }
        }
    }
}
