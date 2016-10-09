using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Ruler
{
    public class Config
    {
        public static String FileName = "ruler.cfg";
        public RulerInfo VerticalRulerInfo { get; set; }
        public RulerInfo HorizontalRulerInfo { get; set; }
        public static String ConfigPath
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), FileName);
            }
        }
        public static Config Load()
        {
            var config = new Config();
            var lines = File.Exists(ConfigPath) ? File.ReadAllLines(ConfigPath) : new string[0];
            if (lines.Length == 2)
            {
                config.HorizontalRulerInfo = RulerInfo.CovertToRulerInfo(lines[0].Split(' '));
                config.VerticalRulerInfo = RulerInfo.CovertToRulerInfo(lines[1].Split(' '));
            }
            else
            {
                config.HorizontalRulerInfo = RulerInfo.GetDefaultRulerInfo();
                config.VerticalRulerInfo = RulerInfo.GetDefaultRulerInfo(true);
            }
            return config;
        }

        public static void Save(Config config)
        {
            var lines = new String[2];
            lines[0] = config.HorizontalRulerInfo.ConvertToParameters();
            lines[1] = config.VerticalRulerInfo.ConvertToParameters();
            File.WriteAllLines(ConfigPath, lines);
        }
    }
}
