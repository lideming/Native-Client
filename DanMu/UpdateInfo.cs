using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DanmakuPie
{
    public class UpdateInfo
    {
        public string AppName { get; set; }

        public Version AppVersion { get; set; }

        public Version RequiredMinVersion { get; set; }

        public string UpdateMode { get; set; }

        public Guid MD5 { get; set; }

        private string _desc;

        public string Desc {
            get {
                return _desc;
            }
            set {
                _desc = string.Join(Environment.NewLine, value.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }
    }
}
