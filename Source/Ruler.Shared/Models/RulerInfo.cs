using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

using Newtonsoft.Json;
using Ruler.Shared.Models;

using Ruler.Shared.Enums;
using Ruler.Shared.Interfaces;

namespace Ruler.Shared.Models
{
    public class RulerInfo : ModelBase
    {
       
        private RulerGuideline _guideline;
        
       
        [JsonProperty("Width")]
        public int Width
        {
            get;
            set;
        }
        [JsonProperty("Height")]
        public int Height
        {
            get;
            set;
        }

        /// <summary>
        /// TODO
        /// </summary>
        [JsonProperty("IsVertical")]
        public bool IsVertical
        {
            get;
            set;
        }
        
        [JsonProperty("Opacity")]
        public double Opacity
        {
            get;
            set;
        }

        /// <summary>
        /// TODO
        /// </summary>
        [JsonProperty("ShowToolTip")]
        public bool ShowToolTip
        {
            get;
            set;
        }

        /// <summary>
        /// TODO
        /// </summary>
        [JsonProperty("IsLocked")]
        public bool IsLocked
        {
            get;
            set;
        }

        [JsonProperty("TopMost")]
        public bool TopMost
        {
            get;
            set;
        }
        [JsonProperty("Top")]
        public int Top
        {
            get;
            set;
        }
        [JsonProperty("Left")]
        public int Left
        {    get;
            set;
        }
        [JsonProperty("SaveType")]
        public SaveTypes SaveType
        {
            get;
            set;
        }
        public RulerGuideline Guideline
        {
            get => _guideline;
            set { _guideline = value; }
        }

       
        // The property the Serializer uses
        [JsonProperty("DisplayLocation")]
        public string DisplayLocationString
        {
            get => $"{Left},{Top}"; 
            set
            {
                var parts = value.Split(',');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int x) &&
                    int.TryParse(parts[1], out int y))
                {
                    Left = x;
                    Top = y;
                }
            }
        }
        [JsonIgnore]
       public Orientation RulerOrientation
        {
            get => IsVertical ? Orientation.Vertical : Orientation.Horizontal;
        }
        public override string ToString()
        {
            return $"[RulerInfo Details]" + Environment.NewLine +
                   $"  IsVertical: {IsVertical})" + Environment.NewLine +
                   $"  Size: {Width}x{Height}" + Environment.NewLine +
                   $"  Location: {Left},{Top} (Display: {DisplayLocationString})" + Environment.NewLine +
                   $"  Opacity: {Opacity}" + Environment.NewLine +
                   $"  TopMost: {TopMost} | ShowToolTip: {ShowToolTip}" + Environment.NewLine +
                   $"  IsLocked: {IsLocked}" + Environment.NewLine +
                   $"  SaveType: {SaveType}" +
                   $"  Guideline:  {Guideline}"; 
        }
        public void ToggleOrientation()
        {
            Console.WriteLine(this.ToString());
            IsVertical = !IsVertical;
            var oldWidth = Width;
            Width = Height;
            Height = oldWidth;
            Console.WriteLine("Orientation toggled. New state:");
            Console.WriteLine(this.ToString());
        }
    }

}