using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Models
{
    public class RulerGuideline: ModelBase
    {
        private double _position;
        private bool _isLocked;
        public RulerGuideline()
        {
            Position = 0;
            IsLocked = false;
        }
        [JsonProperty("Position")]
        public double Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }
        [JsonProperty("IsLocked")]
        public bool IsLocked
        {
            get => _isLocked;
            set => SetProperty(ref _isLocked, value);
        }
        public Color GuidelineColor
        {
            get             
            {
                return IsLocked ? Color.Blue : Color.Red;
            }
        }
        public override string ToString()
        {
            return $"[Guideline Details]" + Environment.NewLine +
                   $"  Position: {Position}" + Environment.NewLine +
                   $"  IsLocked: {IsLocked}" + Environment.NewLine +
                   $"  GuidelineColor: {GuidelineColor}";
        }
    }
}
