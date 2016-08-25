using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DanmakuPie
{
    class DanmakuWindow : MyWindow
    {
        public DanmakuEngine engine;
        public Danmaku danmaku;

        public DanmakuWindow(DanmakuEngine engine, Danmaku danmaku) {
            NoActivate = true;
            Penetrate = true;
            Layered = true;

            this.engine = engine;
            this.danmaku = danmaku;
            danmaku.Window = this;
            danmaku.IsPassing = true;

            this.Opacity = setting.getOpactity();
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(engine.CurrentBounds.Right, engine.CurrentBounds.Top + danmaku.StartHeight);

            update();
            this.Shown += Danmaku_Shown;
        }

        private void Danmaku_Shown(object sender, EventArgs e) {
            this.TopMost = true;
            IsShown = true;
            Penetrate = true;
        }

        public bool IsShown = false;

        public void Remove() {
            try {
                danmaku.IsPassing = false;
                danmaku.Window = null;
                engine.DanmakuWindowList.Remove(this);
                if (IsDisposed == false) {
                    Invoke(new MethodInvoker(() => this.Close()));
                }
                System.Diagnostics.Debug.WriteLine(danmaku.Text + " Removed.");
            }
            catch (Exception) { } // ignore
        }

        void update() {
            Graphics g = CreateGraphics();
            Size = g.MeasureString(danmaku.Text, danmaku.Font).ToSize();
            using (Bitmap bitmap = new Bitmap(Width, Height))
            using (Graphics gb = Graphics.FromImage(bitmap)) {
                paint(gb);
                UpdateLayeredWindow(bitmap);
            }
        }

        void paint(Graphics g) {
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            Brush b = new SolidBrush(danmaku.Color);
            g.DrawString(danmaku.Text, danmaku.Font, b, 0, 0);
        }

    }
}
