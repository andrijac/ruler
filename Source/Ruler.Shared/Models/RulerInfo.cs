using Newtonsoft.Json;
using Ruler.Shared.Attributes;
using Ruler.Shared.Enums;
using System;
using System.Windows.Forms;

namespace Ruler.Shared.Models
{
    public class RulerInfo : ModelBase
    {
       
     private Guid _id;
        [JsonIgnore]
        public Guid ID
        {
            get
            {
                if (Guid.Empty == _id)
                {
                    _id = new Guid();
                    return _id;
                }
                return _id;
            }
            set
            {
                SetProperty(ref _id, value);
            }
        }
        private int _width;
        [SyncWithUI]
        [JsonProperty("Width")]
        public int Width
        {
            get
            {
                if (_width<85)
                {
                    _width = 85;
                }
                return _width;
            }
            set
            {
                SetProperty(ref _width, value);
            }
        }
        private int _height;
        [SyncWithUI]
        [JsonProperty("Height")]
        public int Height
        {
            get
            {
                if (_height<85)
                {
                    _height = 85;
                }
                return _height;
            }
            set
            {
                SetProperty(ref _height, value);
            }
        }
        private bool _isVertical;
        /// <summary>
        /// TODO
        /// </summary>
        [JsonProperty("IsVertical")]
        public bool IsVertical
        {
            get
            {
             return _isVertical;
            }
            set
            {
                SetProperty(ref _isVertical, value);
            }
        }
        private double _opacity;
        [SyncWithUI]
        [JsonProperty("Opacity")]
        public double Opacity
        {
            get
            {
                return _opacity;
            }
            set
            {
                SetProperty(ref _opacity, value);
            }
        }
        private bool _ShowToolTip;
        /// <summary>
        /// TODO
        /// </summary>
        [JsonProperty("ShowToolTip")]
        public bool ShowToolTip
        {
            get
            {
                return _ShowToolTip;
            }
            set
            {
                SetProperty(ref _ShowToolTip, value);
            }
        }
        private bool _isLocked;
        /// <summary>
        /// TODO
        /// </summary>
        [JsonProperty("IsLocked")]
        public bool IsLocked
        {
            get
            {
                return _isLocked;
            }
            set
            {
                SetProperty(ref _isLocked, value);
            }
        }
        private bool _topMost;
        [SyncWithUI]
        [JsonProperty("TopMost")]
        public bool TopMost
        {
            get
            {
                return _topMost;
            }
            set
            {
                SetProperty(ref _topMost, value);
            }
        }
        private int _top;
        [JsonProperty("Top")]
        public int Top
        {
            get
            {
                return _top;
            }
            set
            {
                SetProperty(ref _top, value);
            }
        }
        private int _left;
        [JsonProperty("Left")]
        public int Left
        {    get
            {
                return _left;
            }
            set
            {
                SetProperty(ref _left, value);
            }
        }
        private SaveTypes _saveType;
        [JsonProperty("SaveType")]
        public SaveTypes SaveType
        {
            get
            {
                if (Enum.TryParse(_saveType.ToString(),true,out SaveTypes saved))
                {
                    return saved;
                }
                return SaveTypes.None;
            }
            set
            {
                SetProperty(ref _saveType,value);
            }
        }
        private RulerGuideline _guideline;
        public RulerGuideline Guideline
        {
            get
            {
                if (_guideline == null)
                {
                    _guideline = new RulerGuideline();
                }
                return _guideline;
            }
            set 
            {
                   SetProperty(ref _guideline,value); 
            }
        }
        private MagnifierState _magnifier;
        public MagnifierState Magnifier
        {
            get
            {
                if (_magnifier==null)
                {
                    _magnifier = new MagnifierState();
                }
                return _magnifier;
            }
            set
            {
                SetProperty(ref _magnifier,value);
            }
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