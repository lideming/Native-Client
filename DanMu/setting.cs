using System;
using System.Windows;
using System.Windows.Media;
using System.IO;
using System.Security.Permissions;
using System.Security;
using System.Text;

namespace DanMu
{
    class setting
    {
        private static int NUM = 40; //弹幕数量
        private static int DURATION = 40; //弹幕获取间隔
        private static int SPEED = 2; //弹幕移动速度
        private static Brush background = Brushes.Transparent;
        private static Brush foreground = Brushes.Black;
        private static FontFamily fontFamily = new FontFamily("微软雅黑");
        private static FontStyle fontStyle = FontStyles.Normal;
        private static FontWeight fontWeight = FontWeights.Normal;
        private static double fontSize = 36;
        private static double opactity = 1; // (Between 0.0-1.0)
        private static bool randomColor = true;
        private static bool randomFontFamily = true;
        private static string roomID;

        public static int getNUM() { return NUM; }
        public static int getDURATION() { return DURATION; }
        public static int getSPEED() { return SPEED; }
        public static String getSOURCE() { return "http://danmu.zhengzi.me/controller/desktop.php?user=" + account.name + "&hashPass=" + account.passwordMD5 + "&func=getSeq&para=" + setting.getNUM().ToString(); }
        public static Brush getBackground() { return background; }
        public static Brush getForeground() { return foreground; }
        public static FontFamily getFontFamily() { return fontFamily; }
        public static FontStyle getFontStyle() { return fontStyle; }
        public static FontWeight getFontWeight() { return fontWeight; }
        public static double getFontSize() { return fontSize; }
        public static double getOpactity() { return opactity; }
        public static bool getRandomColor() { return randomColor; }
        public static bool getRandomFontFamily (){ return randomFontFamily; }
        public static string getRoomId() { return roomID; }


        public static void setNUM(int num) { NUM = num; }
        public static void setDURATION(int duration) { DURATION = duration; }
        public static void setSPEED(int speed) { SPEED = speed; }
        public static void setFontFamily(string fontFamilyText) { fontFamily = new FontFamily(fontFamilyText); }
        public static void setBackground(string backgroundColor) {
            Color newBackground = (Color)ColorConverter.ConvertFromString(backgroundColor);
            background = new SolidColorBrush(newBackground);
        }
        public static void setForeground(string fontColor) {
            Color foregroundColor=(Color)ColorConverter.ConvertFromString(fontColor);
            foreground = new SolidColorBrush(foregroundColor);
        }
        public static void setFontSize(int newFontSize) { fontSize = (double)newFontSize; }
        public static void setFontStyle(FontStyle newFontStyle) { fontStyle = newFontStyle; }
        public static void setFontWeight(FontWeight newFontWeight) { fontWeight = newFontWeight; }
        public static void setOpactity(double newOpactity) { opactity = newOpactity; }
        public static void setRandomColor(bool newRandomColor) { randomColor = newRandomColor; }
        public static void setRandomFontFamily(bool newRandomFontFamily) { randomFontFamily = newRandomFontFamily; }
        public static void setRoomId(string newRoomId) { roomID = newRoomId; }

        public static bool SaveSetting() {
            string settingFilePath = System.IO.Directory.GetCurrentDirectory() + "\\setting.ini";
            FileIOPermission f = new FileIOPermission(FileIOPermissionAccess.Read, System.IO.Directory.GetCurrentDirectory());
            try {
                f.Demand();
            }
            catch (SecurityException e) {
                System.Windows.MessageBox.Show("文件夹权限错误，请检查UAC权限，无法保存设置。", "云弹幕",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                return false;
            }
            FileStream fs = new FileStream(settingFilePath, FileMode.Create);
            StreamWriter settingFileSW = new StreamWriter(fs);
            try {
                settingFileSW.WriteLine("[Basic]");
                settingFileSW.WriteLine("Num = " + NUM.ToString());
                settingFileSW.WriteLine("Duration = " + DURATION.ToString());
                settingFileSW.WriteLine("Speed = " + SPEED.ToString());
                settingFileSW.WriteLine("[Font]");
                settingFileSW.WriteLine("Background = " + background.ToString());
                settingFileSW.WriteLine("Foreground = " + foreground.ToString());
                settingFileSW.WriteLine("FontFamily = " + fontFamily.ToString());
                settingFileSW.WriteLine("FontStyle = " + fontStyle.ToString());
                settingFileSW.WriteLine("FontWeight = " + fontWeight.ToString());
                settingFileSW.WriteLine("FontSize = " + fontSize.ToString());
                settingFileSW.WriteLine("Opactity = " + opactity.ToString());
                settingFileSW.WriteLine("Random Color = " + randomColor.ToString());
                settingFileSW.WriteLine("Random FontFamily = " + randomFontFamily.ToString());         
                settingFileSW.Close();
                fs.Close();
                return true;
            }
            catch(IOException e) {
                System.Windows.MessageBox.Show("写入配置文件时错误。", "云弹幕",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
            if(settingFileSW != null)
                settingFileSW.Close();
            if(fs!=null)
                fs.Close();
            return false;
        }

        /// <summary>
        /// 从本地setting.ini恢复设置
        /// </summary>
        public static void RestoreSetting() {
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
                if (str == "Italic") {
                    setting.setFontStyle(FontStyles.Italic);
                }
                else {
                    setting.setFontStyle(FontStyles.Normal);
                }
                str = settingFileSR.ReadLine();
                str = str.Substring("FontWeight = ".Length, str.Length - "FontWeight = ".Length);
                if (str == "Bold") {
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
                str = settingFileSR.ReadLine();
                str = str.Substring("Random Color = ".Length, str.Length - "Random Color = ".Length);
                if (str == "True") {
                    setting.setRandomColor(true);
                }
                else {
                    setting.setRandomColor(false);
                }
                str = settingFileSR.ReadLine();
                str = str.Substring("Random FontFamily = ".Length, str.Length - "Random FontFamily = ".Length);
                if (str == "True") {
                    setting.setRandomFontFamily(true);
                }
                else {
                    setting.setRandomFontFamily(false);
                }
                settingFileSR.Close();
            }
            catch (FileNotFoundException e) {
                System.Windows.MessageBox.Show("未找到配置文件，将默认初始化。", "弹幕派",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                setting.SaveSetting();
            }
            catch (FormatException e) {
                System.Windows.MessageBox.Show("配置文件中存在格式错误。", "弹幕派",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
            catch (OverflowException e) {
                System.Windows.MessageBox.Show("配置文件中存在参数错误。", "弹幕派",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
            catch (IOException e) {
                System.Windows.MessageBox.Show("配置文件存在错误。", "弹幕派",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
            catch (NullReferenceException e) {
                System.Windows.MessageBox.Show("配置文件存在错误。", "弹幕派",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
            catch (ArgumentOutOfRangeException e) {
                System.Windows.MessageBox.Show("配置文件存在错误。", "弹幕派",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
            catch {
                System.Windows.MessageBox.Show("Fatal Error.", "弹幕派",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
        }
    }
}
