using Newtonsoft.Json;

using System;


namespace Ruler.Shared.Models
{
    public class RulerGuideline: ModelBase
    {
        private double _position;
        private bool _isLocked;
        private bool _isEnabled;
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
            set
            {
                SetProperty(ref _isLocked, value);
                OnPropertyChanged(nameof(GuidelineColorHex));
            }

        }
        [JsonProperty("IsEnabled")]
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }
        [JsonIgnore]
        public string GuidelineColorHex
        {
            get
            {
                // Blue hex code if locked, otherwise Red hex code
                return IsLocked ? "#FF0000FF" : "#FFFF0000";
            }
        }
        public override string ToString()
        {
            return $"[Guideline Details]" + Environment.NewLine +
                   $"  Position: {Position}" + Environment.NewLine +
                   $"  IsLocked: {IsLocked}" + Environment.NewLine;
        }
    }
}
