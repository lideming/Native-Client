using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DanMu
{
    public class KeyboardHandler : IDisposable
    {
        public enum KeyFlags    //控制键编码
        {  
            MOD_NONE = 0x0,
            MOD_ALT =0x1,
            MOD_CONTROL =0x2,
            MOD_SHIFT =0x4,
            MOD_WIN =0x8
        }

        public const int WM_HOTKEY = 0x0312;
        public const int VK_SPACE = 0x20;  // A virtual-key, refer to: http://msdn.microsoft.com/en-us/library/windows/desktop/dd375731(v=vs.85).aspx
        public const int VK_H = 0x48;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private readonly Window _window;
        private MainWindow _mainWindow;
        WindowInteropHelper _host;

        public KeyboardHandler(Window window,MainWindow mainWindow) {
            _window = window;
            this._mainWindow = mainWindow;
            _host = new WindowInteropHelper(_window);

            SetupHotKey(_host.Handle);
            ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;
        }

        void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled) {
            if (msg.message == WM_HOTKEY) {
                if(msg.wParam.ToInt32() == 100)
                    _mainWindow.stop_Click(_mainWindow, null);
                else if(msg.wParam.ToInt32() == 200)
                    _mainWindow.hide_Click(_mainWindow, null);
            }
        }

        private void SetupHotKey(IntPtr handle) {
            RegisterHotKey(handle, 100, (int)KeyFlags.MOD_CONTROL, VK_SPACE);
            RegisterHotKey(handle, 200, (int)KeyFlags.MOD_CONTROL, VK_H);

        }

        public void Dispose() {
            //Dispose() calls Dispose(true)
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~KeyboardHandler() {
            //Finalizer calls Dispose(false)
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                //free managed resourses
                UnregisterHotKey(_host.Handle, 100);
                UnregisterHotKey(_host.Handle, 200);
            }
        }
    }
}
