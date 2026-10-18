using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ruler.Shared.Models;

namespace Ruler.Shared.Services
{
    public static class RulerCalibration
    {
        private static double _userMultiplier = 1.0;
        private static double _currentOsDpi = 96.0;

        public static event EventHandler CalibrationChanged;

        /// <summary>
        /// The live calculation used by units (Inches, Millimeters, etc.) to render accurate physical sizing.
        /// </summary>
        public static double EffectiveDpi => _currentOsDpi * _userMultiplier;

        /// <summary>
        /// Gets or sets the fine-tuning hardware correction multiplier.
        /// </summary>
        public static double UserMultiplier
        {
            get => _userMultiplier;
            set
            {
                if (Math.Abs(_userMultiplier - value) > 0.0001)
                {
                    _userMultiplier = value;
                    CalibrationChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Updates the current live OS DPI reported by Windows (e.g., when moving between displays or changing scaling).
        /// </summary>
        public static void SetCurrentOsDpi(double dpi)
        {
            if (Math.Abs(_currentOsDpi - dpi) > 0.1)
            {
                _currentOsDpi = dpi;
                CalibrationChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Synchronizes the runtime engine with a monitor profile. 
        /// Automatically scales the multiplier if the resolution changed on the same hardware ($W_{new} / W_{old}$).
        /// </summary>
        public static void SyncWithMonitor(MonitorCalibrationProfile profile, int currentWidth, int currentHeight)
        {
            if (profile == null) return;

            if (profile.RecordedWidth > 0 && profile.RecordedHeight > 0)
            {
                // Check if resolution changed on the same physical monitor
                if (profile.RecordedWidth != currentWidth || profile.RecordedHeight != currentHeight)
                {
                    // Automatically calculate the ratio shift without forcing recalibration!
                    double resolutionRatio = (double)currentWidth / profile.RecordedWidth;
                    profile.UserCalibrationMultiplier *= resolutionRatio;

                    // Update the recorded dimensions to the new resolution
                    profile.RecordedWidth = currentWidth;
                    profile.RecordedHeight = currentHeight;
                }
            }
            else
            {
                // First time seeing this monitor's resolution
                profile.RecordedWidth = currentWidth;
                profile.RecordedHeight = currentHeight;
            }

            // Apply the profile's multiplier to the live runtime engine
            UserMultiplier = profile.UserCalibrationMultiplier;
        }
    }
}
