using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using UmengAnalyticsNet;
using UmengSDK;
using System.Diagnostics;

namespace DanmakuPie
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            UmengAnalyticsNet.Wpf.UmengAnalyticsApp.Initialize("576f96ce67e58eed3d002295", "website");
            //Debug.WriteLine(UmengAnalyticsNet.OS.GetOSVersion().ToString());
            //Debug.WriteLine(UmengAnalyticsNet.OS.GetOSString().ToString());
            //Debug.WriteLine(UmengAnalyticsNet.Token.GetToken().ToString());
            UmengAnalyticsNet.Wpf.AppInfo info = new UmengAnalyticsNet.Wpf.AppInfo();
            UmengAnalyticsNet.Wpf.
            Debug.WriteLine(info.GetAppVersion().ToString());
            Debug.WriteLine(info.GetResolution().ToString());
            Debug.WriteLine(info.GetUserId().ToString());
        }
    }
}
