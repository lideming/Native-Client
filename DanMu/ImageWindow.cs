using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DanmakuPie
{
    class ImageWindow : MyWindow
    {
        Bitmap image;
        public bool Moveable { get; set; } = true;
        public bool FadeIn { get; set; } = true;
        public bool FadeOut { get; set; } = true;

        public ImageWindow(Image image) {
            NoActivate = true;
            Layered = true;
            ShowInTaskbar = false;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;

            this.image = new Bitmap(image); ;
            this.Size = image.Size;
            Load += ImageWindow_Load;
            FormClosing += ImageWindow_FormClosing;
        }

        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;

        protected override void WndProc(ref Message m) {
            if (Moveable && m.Msg == WM_NCHITTEST) {
                base.WndProc(ref m);
                if ((int)m.Result == HTCLIENT)
                    m.Result = (IntPtr)HTCAPTION;
                return;
            }
            base.WndProc(ref m);
        }

        private void ImageWindow_Load(object sender, EventArgs e) {
            UpdateLayeredWindow(image);
            if (FadeIn) {
                FadeOpacity = 0.01;
                FadeTo(1);
            }
        }

        bool fadedout = false;
        private void ImageWindow_FormClosing(object sender, FormClosingEventArgs e) {
            if (FadeOut && fadedout == false) {
                e.Cancel = true;
                FadeTo(0, callback: (s, d) => {
                    fadedout = true;
                    this.Invoke(new MethodInvoker(() => this.Close()));
                });
            }
        }
    }
}
