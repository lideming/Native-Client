using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace DanmakuPie
{
    class Danmaku
    {
        public string Text;
        public Color Color;
        public Font Font;
        public int Height = 0;
        public bool AutoMoveDown = true;

        public bool Passing = false;

        public event Action<Danmaku> DanmakuPassed;
        public void InvokeDanmakuPassed() => DanmakuPassed?.Invoke(this);

        public DanmakuWindow Window;
        public void Remove() => Window?.Remove();
    }
}
