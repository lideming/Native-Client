using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Windows;

namespace DanmakuPie
{
    class Log : IDisposable
    {
        private static Log instance;
        private bool isLogOpen;
        private FileStream fs;
        private StreamWriter sw;
        private Log() {
            if (!Directory.Exists(System.IO.Directory.GetCurrentDirectory() + "\\Logs")) {
                Directory.CreateDirectory(System.IO.Directory.GetCurrentDirectory() + "\\Logs");
            }
            string settingFilePath = System.IO.Directory.GetCurrentDirectory() + "\\Logs\\"+DateTime.Now.ToString()+".log";
            FileIOPermission f = new FileIOPermission(FileIOPermissionAccess.Write, System.IO.Directory.GetCurrentDirectory());
            try {
                f.Demand();
            }
            catch (SecurityException e) {
                System.Windows.MessageBox.Show("文件夹权限错误，请检查UAC权限，无法保存设置。", "弹幕派",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                isLogOpen = false;
                return;
            }
            fs = new FileStream(settingFilePath, FileMode.Create);
            sw = new StreamWriter(fs);
            isLogOpen = true;
        }

        public bool getOpenStatus() {
            return isLogOpen;
        }

        public bool Write(string str) {
            if(instance == null) {
                instance = new Log();
                if(isLogOpen == false) {
                    return false;
                }
            }
            try {
                sw.WriteLine(DateTime.Now.ToString() + " " + str);
                return true;
            }
            catch {
                return false;
            }  
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Log() {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (sw != null)
                    sw.Dispose();
                if (fs != null)
                    fs.Dispose();
            }
        }
    }
}
