using System;
using System.Windows;
using System.Windows.Media;
using System.IO;
using System.Security.Permissions;
using System.Security;

namespace DanMu
{
    class setting
    {
        private static int NUM = 100; //弹幕数量
        private static int DURATION = 1; //弹幕获取间隔
        private static int SPEED = 2; //弹幕移动速度
        private static String SOURCE = "http://danmu.zhengzi.me/controller/desktop.php?hashUser=b3e72f25cea55fd55623124cc59c4b0c&hashPass=202cb962ac59075b964b07152d234b70&function=getSeq&para=1";

        private static Brush background = Brushes.Transparent;
        private static Brush foreground = Brushes.Red;
        private static FontFamily fontFamily = new FontFamily("微软雅黑");
        private static FontStyle fontStyle = FontStyles.Italic;
        private static FontWeight fontWeight = FontWeights.Bold;
        private static double fontSize = 36;
        private static double opactity = 1; // (Between 0.0-1.0)

        public static int getNUM() { return NUM; }
        public static int getDURATION() { return DURATION; }
        public static int getSPEED() { return SPEED; }
        public static String getSOURCE() { return SOURCE; }
        public static Brush getBackground() { return background; }
        public static Brush getForeground() { return foreground; }
        public static FontFamily getFontFamily() { return fontFamily; }
        public static FontStyle getFontStyle() { return fontStyle; }
        public static FontWeight getFontWeight() { return fontWeight; }
        public static double getFontSize() { return fontSize; }
        public static double getOpactity() { return opactity; }


        public static void setNUM(int num) { NUM = num; }
        public static void setDURATION(int duration) { DURATION = duration; }
        public static void setSPEED(int speed) { SPEED = speed; }
        public static void setSOURCE(string source) { SOURCE = source; }
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
                settingFileSW.WriteLine("Source = " + SOURCE.ToString());
                settingFileSW.WriteLine("[Font]");
                settingFileSW.WriteLine("Background = " + background.ToString());
                settingFileSW.WriteLine("Foreground = " + foreground.ToString());
                settingFileSW.WriteLine("FontFamily = " + fontFamily.ToString());
                settingFileSW.WriteLine("FontStyle = " + fontStyle.ToString());
                settingFileSW.WriteLine("FontWeight = " + fontWeight.ToString());
                settingFileSW.WriteLine("FontSize = " + fontSize.ToString());
                settingFileSW.WriteLine("Opactity = " + opactity.ToString());
                settingFileSW.Close();
                fs.Close();
                return true;
            }
            catch(IOException e) {
                System.Windows.MessageBox.Show("写入配置文件时错误。", "云弹幕",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
            finally {
                fs.Close();
                settingFileSW.Close();
            }         
            return false;
        }
    }
}
