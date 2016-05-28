using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Windows;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace DanmakuPie
{
    /// <summary>
    /// UpdateWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateWindow : Window
    {
        private UpdateInfo updateInfo = null;

        public UpdateWindow() {
            InitializeComponent();
        }
        public UpdateWindow(string mode, UpdateInfo updateInfo) {
            if(mode == "UpdateManually") {
                CheckUpdate();
            }
            InitializeComponent();
            if(updateInfo != null) {
                this.updateInfo = updateInfo;
                labelVersion.Content = "已检测到新版本 v"+updateInfo.AppVersion.ToString();
                textBlockDisc.Text = updateInfo.Desc;
            }
        }

        public void CheckUpdate() {
            System.Threading.ThreadPool.QueueUserWorkItem((s) =>
            {
                string url = "http://7xr64j.com1.z0.glb.clouddn.com/update1.xml";
                var client = new System.Net.WebClient();
                client.DownloadDataCompleted += (x, y) =>
                {
                    try {
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
                        if (CheckUpdateInfo(updateInfo)) {
                            System.Windows.MessageBox.Show("现在是最新版本 v"+ System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + "。", "弹幕派",
                                MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                            this.Close();
                        }
                        else {
                            this.updateInfo = updateInfo;
                        }
                    }
                    catch {
                        Debug.WriteLine("Error When Unpacking XML.");
                    }
                };
                client.DownloadDataAsync(new Uri(url));
            });
        }

        public bool CheckUpdateInfo(UpdateInfo updateInfo) {
            if (updateInfo.UpdateMode == "UpdateToMin")
                if (updateInfo.RequiredMinVersion != null && System.Reflection.Assembly.GetExecutingAssembly().GetName().Version >= updateInfo.RequiredMinVersion)
                    return true;
            if (updateInfo.UpdateMode == "UpdateToNew")
                if (updateInfo.AppVersion != null && System.Reflection.Assembly.GetExecutingAssembly().GetName().Version >= updateInfo.AppVersion)
                    return true;
            return false;
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e) {
            buttonOk.Content = "更新中";
            buttonOk.IsEnabled = false;
            DownloadUpdateFile();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void DownloadUpdateFile() {
            FileIOPermission f = new FileIOPermission(FileIOPermissionAccess.Write, System.IO.Directory.GetCurrentDirectory());
            try {
                f.Demand();
            }
            catch (SecurityException e) {
                System.Windows.MessageBox.Show("文件夹权限错误，请检查UAC权限，无法启动更新程序。", "弹幕派",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                return;
            }
            string appDir = System.IO.Path.Combine(System.Reflection.Assembly.GetEntryAssembly().Location.Substring(0, 
                System.Reflection.Assembly.GetEntryAssembly().Location.LastIndexOf(System.IO.Path.DirectorySeparatorChar)));
            string updateFileDir = System.IO.Path.Combine(System.IO.Path.Combine(appDir.Substring(0, 
                appDir.LastIndexOf(System.IO.Path.DirectorySeparatorChar))), "Danmakupie v" + updateInfo.AppVersion.ToString());
            string parentDir = appDir.Substring(0, appDir.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
            string fileName = "Danmakupie v" + updateInfo.AppVersion.ToString() + ".zip";
            string url = "http://7xr64j.com1.z0.glb.clouddn.com/" + fileName;
            var client = new System.Net.WebClient();
            client.DownloadProgressChanged += (sender, e) =>
            {
                UpdateProgressBar(e.BytesReceived / e.TotalBytesToReceive);
            };
            client.DownloadDataCompleted += (sender, e) =>
            {
                if (e.Error == null) {
                    string zipFilePath = System.IO.Path.Combine(parentDir, fileName);
                    byte[] data = e.Result;
                    BinaryWriter writer = new BinaryWriter(new FileStream(zipFilePath, FileMode.OpenOrCreate));
                    writer.Write(data);
                    writer.Flush();
                    writer.Close();
                    try {
                        ExtractZipFile(zipFilePath, "", parentDir);
                        System.IO.File.Delete(zipFilePath);
                        string exePath = parentDir + "\\Danmakupie v" + updateInfo.AppVersion.ToString() + "\\DanmakuPie.exe";
                        var info = new System.Diagnostics.ProcessStartInfo(exePath);
                        info.UseShellExecute = true;
                        info.WorkingDirectory = parentDir + "\\Danmakupie v" + updateInfo.AppVersion.ToString();
                        System.Windows.MessageBox.Show("更新完成，请删除旧版本程序文件。", "弹幕派",
                            MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                        System.Diagnostics.Process.Start(info);
                    }
                    catch {
                        System.Windows.MessageBox.Show("数据包损坏，请重试。", "弹幕派",
                            MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                    }
                    finally {
                        Application.Current.Shutdown();
                    }
                }
            };
            client.DownloadDataAsync(new Uri(url));
        }

        private void UpdateProgressBar(float progress) {
            this.labelProgress.Content = Math.Round(progress * 100).ToString() + "%";
            this.progressBar.Value = progress * 100;
        }

        public void ExtractZipFile(string archiveFilenameIn, string password, string outFolder) {
            ZipFile zf = null;
            try {
                FileStream fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);
                if (!String.IsNullOrEmpty(password)) {
                    zf.Password = password;     // AES encrypted entries are handled automatically
                }
                foreach (ZipEntry zipEntry in zf) {
                    if (!zipEntry.IsFile) {
                        continue;           // Ignore directories
                    }
                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    String fullZipToPath = System.IO.Path.Combine(outFolder, entryFileName);
                    string directoryName = System.IO.Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (FileStream streamWriter = File.Create(fullZipToPath)) {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            finally {
                if (zf != null) {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void buttonMinimize_Click(object sender, RoutedEventArgs e) {
            this.WindowState = WindowState.Minimized;
        }

        private void Image_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            if(e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) {
                this.DragMove();
            }
        }
    }
}
