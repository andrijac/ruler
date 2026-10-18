using Ruler.Shared.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Models
{
    public class MonitorCalibrationProfile
    {
        public string DeviceName { get; set; }

        private double _userMultiplier = 1.0;

        /// <summary>
        /// Gets or sets the fine-tuning hardware correction multiplier.
        /// Setting this directly triggers the transactional Update method.
        /// </summary>
        public double UserCalibrationMultiplier
        {
            get => _userMultiplier;
            set => Update(newMultiplier: value);
        }

        public int RecordedWidth { get; set; }
        public int RecordedHeight { get; set; }

        /// <summary>
        /// Fired whenever this specific monitor's profile updates or recalibrates.
        /// </summary>
        public event EventHandler CalibrationChanged;

        /// <summary>
        /// Safely updates profile state in a single transaction using nullable types (.NET 4.8 safe),
        /// firing events and saving to the registry only if actual changes occur.
        /// </summary>
        public void Update(double? newMultiplier = null, int? newWidth = null, int? newHeight = null)
        {
            bool hasChanged = false;

            if (newMultiplier.HasValue && Math.Abs(_userMultiplier - newMultiplier.Value) > 0.0001)
            {
                _userMultiplier = newMultiplier.Value;
                hasChanged = true;
            }

            if (newWidth.HasValue && RecordedWidth != newWidth.Value)
            {
                RecordedWidth = newWidth.Value;
                hasChanged = true;
            }

            if (newHeight.HasValue && RecordedHeight != newHeight.Value)
            {
                RecordedHeight = newHeight.Value;
                hasChanged = true;
            }

            if (hasChanged)
            {
                // 1. Notify any active rulers tied to this monitor profile to redraw
                CalibrationChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Calculates the effective DPI for this specific monitor profile.
        /// </summary>
        public double GetEffectiveDpi(double osDpi)
        {
            return osDpi * _userMultiplier;
        }

        /// <summary>
        /// Automatically handles resolution shifts on this specific monitor's hardware 
        /// without forcing the user to recalibrate.
        /// </summary>
        public void SyncResolution(int currentWidth, int currentHeight)
        {
            if (RecordedWidth > 0 && RecordedHeight > 0)
            {
                if (RecordedWidth != currentWidth || RecordedHeight != currentHeight)
                {
                    double resolutionRatio = (double)currentWidth / RecordedWidth;

                    // Atomically update the multiplier and new dimensions together
                    Update(
                        newMultiplier: _userMultiplier * resolutionRatio,
                        newWidth: currentWidth,
                        newHeight: currentHeight
                    );
                }
            }
            else
            {
                // First time recording this monitor's resolution
                Update(newWidth: currentWidth, newHeight: currentHeight);
            }
        }
    }
}
