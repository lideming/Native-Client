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
        /// <summary>
        /// 相对屏幕顶部的高度
        /// </summary>
        public int StartHeight = 0;
        public bool AutoMoveDown = true;

        public bool IsPassing = false;
        /// <summary>
        /// 弹幕经过屏幕后触发
        /// </summary>
        public event Action<Danmaku> DanmakuPassed;
        public void InvokeDanmakuPassed() => DanmakuPassed?.Invoke(this);

        public DanmakuWindow Window;
        public void Remove() => Window?.Remove();
    }
}
