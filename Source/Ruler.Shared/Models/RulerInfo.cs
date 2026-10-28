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
        [JsonIgnore]
        public Guid ID { get; set; } = Guid.NewGuid();

        private int _width = 400;
        [JsonProperty("Width")]
        public int Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        private int _height = 75;
        [JsonProperty("Height")]
        public int Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        private string _lastDeviceName = string.Empty;
        [JsonProperty("LastDeviceName")]
        public string LastDeviceName
        {
            get => _lastDeviceName;
            set => SetProperty(ref _lastDeviceName, value);
        }

        [JsonProperty("IsVertical")]
        public bool IsVertical { get; set; }

        [JsonProperty("Opacity")]
        public double Opacity { get; set; } = 0.60;

        [JsonProperty("ShowToolTip")]
        public bool ShowToolTip { get; set; }

        [JsonProperty("IsLocked")]
        public bool IsLocked { get; set; }

        [JsonProperty("TopMost")]
        public bool TopMost { get; set; } = true;

        [JsonProperty("Top")]
        public int Top { get; set; }

        [JsonProperty("Left")]
        public int Left { get; set; }

        [JsonProperty("SaveType")]
        public SaveTypes SaveType { get; set; } = SaveTypes.None;

        [JsonProperty("Guideline")]
        public RulerGuideline Guideline { get; set; } = new RulerGuideline();

        [JsonProperty("Magnifier")]
        public MagnifierState Magnifier { get; set; } = new MagnifierState();
        private double _rulerScale = 1.0;
        [JsonProperty("RulerScale")]
        public double RulerScale
        {
            get => _rulerScale;
            set
            {
                if (_rulerUnit != RulerUnit.Pixels)
                {
                    value = 1.0;
                }
                SetProperty(ref _rulerScale, value);
            }
        }
        private RulerUnit _rulerUnit = RulerUnit.Pixels;
        [JsonProperty("RulerUnits")]
        public RulerUnit RulerUnits
        {
            get => _rulerUnit;
            set
            {
                if (SetProperty(ref _rulerUnit, value))
                {
                    if (value != RulerUnit.Pixels)
                    {
                        RulerScale = 1.0;
                    }
                }
            }
        }

        //[JsonIgnore]
        //public Orientation RulerOrientation => IsVertical ? Orientation.Vertical : Orientation.Horizontal;

        public override string ToString()
        {
            return $"[RulerInfo Details]" + Environment.NewLine +
                   $"  IsVertical: {IsVertical}" + Environment.NewLine +
                   $"  Size: {Width}x{Height}" + Environment.NewLine +
                   $"  Location: {Left},{Top}" + Environment.NewLine +
                   $"  Opacity: {Opacity}" + Environment.NewLine +
                   $"  TopMost: {TopMost} | ShowToolTip: {ShowToolTip}" + Environment.NewLine +
                   $"  IsLocked: {IsLocked}" + Environment.NewLine +
                   $"  SaveType: {SaveType}" + Environment.NewLine +
                   $"  Guideline: {Guideline}" + Environment.NewLine +
                   $"  RulerScale: {RulerScale}" + Environment.NewLine +
                   $"  RulerUnits: {RulerUnits}";
        }

        public void ToggleOrientation()
        {
            IsVertical = !IsVertical;
            int oldWidth = Width;
            Width = Height;
            Height = oldWidth;
        }
    }

}