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
        private static int NUM = 20; //弹幕数量
        private static int DURATION = 100; //弹幕获取间隔
        private static int SPEED = 2; //弹幕移动速度
        private static Brush background = Brushes.Transparent;
        private static Brush foreground = Brushes.Black;
        private static FontFamily fontFamily = new FontFamily("微软雅黑");
        private static FontStyle fontStyle = FontStyles.Normal;
        private static FontWeight fontWeight = FontWeights.Normal;
        private static double fontSize = 36;
        private static double opactity = 1; // (Between 0.0-1.0)
        private static bool randomColor = false;
        private static bool randomFontFamily = false;

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
    }
}
