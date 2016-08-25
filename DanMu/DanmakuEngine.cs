using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

#pragma warning disable CS1690 // 访问引用封送类的字段上的成员可能导致运行时异常
namespace DanmakuPie
{
    class DanmakuEngine
    {
        List<DanmakuWindow> danmakuWindowList = new List<DanmakuWindow>();
        public IList<DanmakuWindow> DanmakuWindowList => danmakuWindowList;
        public int DanmakuCount => danmakuWindowList.Count;
        object danmakuWindowListLock = new object();

        bool hidden = false;
        /// <summary>
        /// 获取或设置一个值，指示是否隐藏所有弹幕
        /// </summary>
        public bool Hidden
        {
            get {
                return hidden;
            }

            set {
                if (hidden == value)
                    return;
                hidden = value;
                for (int i = 0; i < danmakuWindowList.Count; i++) {
                    try {
                        var d = danmakuWindowList[i];
                        if (hidden) {
                            d.BeginInvoke(new MethodInvoker(() => d.Hide()));
                        }
                        else {
                            d.BeginInvoke(new MethodInvoker(() => d.Show()));
                        }
                    }
                    catch (Exception) { } // ignore
                }
            }
        }
        private volatile bool switchingScreen = false;
        private Rectangle currentBounds;
        public Rectangle CurrentBounds => currentBounds;
        private Screen currentScreen;
        public Screen CurrentScreen
        {
            get {
                return currentScreen;
            }

            set {
                if (value == currentScreen)
                    return;
                switchingScreen = true;
                try {
                    lock (danmakuWindowListLock) {
                        var newscreen = value;
                        var changedLocation = new Size(
                            newscreen.Bounds.X - currentBounds.X,
                            newscreen.Bounds.Y - currentBounds.Y
                            );
                        foreach (var dw in danmakuWindowList) {
                            dw.Invoke(new MethodInvoker(() => {
                                dw.Location += changedLocation;
                            }));
                        }
                        this.currentScreen = value;
                        this.currentBounds = value.Bounds;
                    }
                }
                finally {
                    switchingScreen = false;
                }
            }
        }

        public DanmakuEngine() {
            startEngineThread();
            CurrentScreen = Screen.PrimaryScreen;
        }

        /// <summary>
        /// 显示一条弹幕
        /// </summary>
        public void ShowDanmaku(Danmaku danmaku) {
            if (disposed)
                return;
            new Thread(() => {
                DanmakuWindow dw = null;
                lock (danmakuWindowListLock) {
                    dw = new DanmakuWindow(this, danmaku);
                    if (danmaku.AutoMoveDown)
                        checkIntersectAndMoveDown(dw);
                    if (hidden)
                        dw.Hide();
                    danmakuWindowList.Add(dw);
                }
                Application.Run(dw);
            }) { IsBackground = true }.Start();
        }

        void checkIntersectAndMoveDown(DanmakuWindow d) {
            lock (danmakuWindowList) {
                while (true) {
                    bool hasIntersect = false;
                    for (int i = 0; i < danmakuWindowList.Count; i++) {
                        var d2 = danmakuWindowList[i];
                        var bounds = d2.Bounds;
                        bounds.Width += 20;
                        if (bounds.IntersectsWith(d.Bounds)) {
                            hasIntersect = true;
                            d.Top = d2.Bounds.Bottom;
                            break;
                        }
                    }
                    if (!hasIntersect)
                        break;
                }
            }
        }

        void startEngineThread() {
            new Thread(() => {
                while (true) {
                    Thread.Sleep(16);
                    if (danmakuWindowList.Count < 0)
                        continue;
                    try {
                        for (int i = danmakuWindowList.Count - 1; i >= 0; i--) {
                            var d = danmakuWindowList[i];
                            if (d.IsShown) {
                                d.BeginInvoke(new MethodInvoker(() => {
                                    d.Left -= /* TODO */ setting.getSPEED();
                                    if (d.Bounds.Right < currentBounds.Left && switchingScreen == false) {
                                        d.Remove();
                                        d.danmaku.InvokeDanmakuPassed();
                                    }
                                }));
                            }
                        }
                    }
                    catch (Exception) { } // ignore
                }
            }) { IsBackground = true }.Start();
        }

        bool disposed = false;
        public void Dispose() {
            if (disposed)
                return;
            disposed = true;
            for (int i = danmakuWindowList.Count - 1; i >= 0; i--) {
                try {
                    danmakuWindowList[i].Close();
                }
                catch (Exception) { } // ignore
            }
        }
    }
}
#pragma warning restore CS1690
