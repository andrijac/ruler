using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Models
{
    public class MagnifierState
    {
        private double _zoomLevel = 2.0;
        private const double MinZoom = 1.5;
        private const double MaxZoom = 10.0;

        public bool IsActive { get; set; }

        public double ZoomLevel
        {
            get => _zoomLevel;
            set => _zoomLevel = Math.Max(MinZoom, Math.Min(MaxZoom, value));
        }

        public int ScreenX { get; set; }
        public int ScreenY { get; set; }

        // The physical size of the popup window on screen
        public int ViewportSize { get; set; } = 150;

        /// <summary>
        /// Calculates the exact bounding box dimension to capture from the desktop screen screen context.
        /// </summary>
        public int CapturePixelSize
        {
            get
            {
                // Divide viewport bounding boxes by the zoom modifier to determine source area size
                return (int)Math.Round(ViewportSize / ZoomLevel);
            }
        }
    }
}
