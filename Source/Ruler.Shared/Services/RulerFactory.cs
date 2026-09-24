using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Services
{
    public class RulerFactory : IRulerFactory
    {
        public RulerInfo CreateDefault()
        {
            return new RulerInfo
            {
                Width = 400,
                Height = 35,
                TopMost = true,
                IsVertical = false,
                IsLocked = false,
                Opacity = 1.0,
                ShowToolTip = true
            };
        }

        public RulerInfo CreateFromArguments(string[] args)
        {
            var info = CreateDefault();

            if (args == null || args.Length == 0)
                return info;

            // Simple command-line argument parser stub
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLowerInvariant();
                if ((arg == "-w" || arg == "--width") && i + 1 < args.Length && int.TryParse(args[i + 1], out int w))
                {
                    info.Width = w;
                }
                else if ((arg == "-h" || arg == "--height") && i + 1 < args.Length && int.TryParse(args[i + 1], out int h))
                {
                    info.Height = h;
                }
                else if (arg == "--vertical")
                {
                    info.IsVertical = true;
                }
            }

            return info;
        }

        public void CopyValues(RulerInfo source, RulerInfo target)
        {
            if (source == null || target == null) return;

            target.Width = source.Width;
            target.Height = source.Height;
            target.Left = source.Left;
            target.Top = source.Top;
            target.TopMost = source.TopMost;
            target.IsVertical = source.IsVertical;
            target.IsLocked = source.IsLocked;
            target.Opacity = source.Opacity;
            target.ShowToolTip = source.ShowToolTip;
            target.SaveType = source.SaveType;
        }

        public void CopyValuesWithGUID(RulerInfo source, RulerInfo target)
        {
            CopyValues(source, target);

            // Un-comment and adjust if your RulerInfo model includes a unique identifier property (e.g., Id or Guid)
            // target.Id = source.Id;
        }

        public string ToParameterString(RulerInfo info)
        {
            if (info == null) return string.Empty;

            return $"--width {info.Width} --height {info.Height} --top {info.Top} --left {info.Left} --vertical {info.IsVertical}";
        }
    }
}
