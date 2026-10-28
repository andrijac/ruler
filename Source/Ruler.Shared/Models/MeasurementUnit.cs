using Ruler.Shared.Enums;

    using System;
    using System.Collections.Generic;
using System.Globalization;
using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Ruler.Shared.Models
    {
    public class MeasurementUnit
    {
        public string Name { get; set; }
        public string Abbreviation { get; set; }

        /// <summary>
        /// The interval between major labeled ticks in this unit's scale (e.g., 50 pixels, 1 inch, 10 cm).
        /// </summary>
        public double MajorTickInterval { get; set; }

        /// <summary>
        /// How many subdivisions exist between major ticks for minor/medium tick lines.
        /// </summary>
        public int MinorTickSubdivisions { get; set; } = 5;

        /// <summary>
        /// True if the unit scales relative to window size (e.g., Percentage); false for absolute units (Inches, Pixels).
        /// </summary>
        public bool IsRelative { get; set; } = false;

        /// <summary>
        /// The height/length of major ticks in pixels. Defaults to 12.
        /// </summary>
        public double MajorTickLength { get; set; } = 12.0;

        /// <summary>
        /// Converts screen pixels to this unit value. 
        /// Parameters: (pixelValue, dpi, totalWindowLength)
        /// </summary>
        public Func<double, double, double, double> PixelToUnit { get; set; }

        /// <summary>
        /// Converts this unit value back to screen pixels.
        /// Parameters: (unitValue, dpi, totalWindowLength)
        /// </summary>
        public Func<double, double, double, double> UnitToPixel { get; set; }

        /// <summary>
        /// Formats a numerical value into a clean display string for labels or tooltips, taking orientation (isVertical) into account.
        /// </summary>
        public Func<double, bool, string> FormatLabel { get; set; }

        /// <summary>
        /// Function to dynamically determine minor/medium tick lengths based on subdivision index. Defaults to 4px.
        /// </summary>
        public Func<int, double> GetMinorTickLength { get; set; } = index => 4.0;

        /// <summary>
        /// Determines whether a specific unit value should be labeled.
        /// </summary>
        public Func<double, bool> ShouldLabel { get; set; } = val => val > 0;

        /// <summary>
        /// Optional custom render delegate for advanced units (e.g., stacked fractional inch rendering).
        /// Parameters: (dc, typeface, brush, pen, x, centerY, u, dpi, lastLabelX, actualHeight)
        /// </summary>
        public Action<DrawingContext, Typeface, Brush, Pen, double, double, double, double, double, double> CustomDraw { get; set; }
        public bool SupportsFractions { get; set; } = false;
        public Func<double, string> FormatFraction { get; set; }

        // ==========================================
        // PREDEFINED STANDARD UNITS REGISTRY
        // ==========================================

        public static MeasurementUnit Get(RulerUnit unit)
        {
            switch (unit)
            {
                case RulerUnit.Pixels:
                    return Pixels;
                case RulerUnit.Inches:
                    return Inches;
                case RulerUnit.Centimeters:
                    return Centimeters;
                case RulerUnit.Millimeters:
                    return Millimeters;
                case RulerUnit.Points:
                    return Points;
                case RulerUnit.Percentage:
                    return Percentage;
                default:
                    return Pixels;
            }
        }

        public static readonly MeasurementUnit Pixels = new MeasurementUnit
        {
            Name = "Pixels",
            Abbreviation = "px",
            MajorTickInterval = 100,
            MinorTickSubdivisions = 50,
            IsRelative = false,
            MajorTickLength = 12,
            PixelToUnit = (px, dpi, total) => px,
            UnitToPixel = (val, dpi, total) => val,
            FormatLabel = (val, isVertical) => ((int)val).ToString(),
            ShouldLabel = val => val > 0 && Math.Abs(val % 100.0) < 0.001,
            GetMinorTickLength = index =>
            {
                if (index % 25 == 0) return 10.0; // 50px mark (halfway of 100px interval)
                if (index % 10 == 0) return 8.0;  // 20px mark
                if (index % 5 == 0) return 6.0;   // 10px mark
                return 4.0;
            }
        };
        public static readonly MeasurementUnit Inches = new MeasurementUnit
        {
            Name = "Inches",
            Abbreviation = "in",
            MajorTickInterval = 1.0,
            MinorTickSubdivisions = 16,
            IsRelative = false,
            MajorTickLength = 14,
            PixelToUnit = (px, dpi, total) => px / dpi,
            UnitToPixel = (val, dpi, total) => val * dpi,
            ShouldLabel = val => val > 0 && Math.Abs(val % 0.25) < 0.001,
            FormatLabel = (val, isVertical) => $"{val} in",
            SupportsFractions = true,
            CustomDraw = (dc, typeface, brush, pen, x, centerY, u, dpi, lastLabelX, actualHeight) =>
            {
                int whole = (int)Math.Floor(u);
                double remainder = u - whole;

                if (remainder < 0.001 || Math.Abs(remainder - 1.0) < 0.001)
                {
                    // Whole number case: append "in" (or "ft" if a multiple of 12)
                    string labelText = (u >= 12.0 && Math.Abs(u % 12.0) < 0.001)
                        ? $"{whole} in ({(int)(u / 12.0)} ft)"
                        : $"{whole} in";

                    var formattedText = new FormattedText(labelText, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 9, brush, dpi);
                    dc.DrawText(formattedText, new Point(x - (formattedText.Width / 2), centerY - (formattedText.Height / 2)));
                }
                else
                {
                    // Fractional case: calculate numerator/denominator and call DrawFractionStack with "in"
                    int sixteenths = (int)Math.Round(remainder * 16.0);
                    int gcd = GCD(sixteenths, 16);
                    int num = sixteenths / gcd;
                    int den = 16 / gcd;

                    DrawFractionStack(dc, typeface, brush, pen, x, centerY, whole, num, den, dpi, "in");
                }
            },
            
            FormatFraction = (value) => {
                int whole = (int)Math.Floor(value);
                double remainder = value - whole;
                int sixteenths = (int)Math.Round(remainder * 16.0);
                if (sixteenths == 16) { whole++; sixteenths = 0; }
                if (sixteenths == 0) return whole == 0 ? "0 in" : $"{whole} in";

                int gcd = GCD(sixteenths, 16);
                int num = sixteenths / gcd;
                int den = 16 / gcd;

                return whole == 0 ? $"{num}/{den} in" : $"{whole} {num}/{den} in";
            },
            GetMinorTickLength = index =>
            {
                if (index % 8 == 0) return 10.0; // Half-inch (8/16)
                if (index % 4 == 0) return 8.0;  // Quarter-inch (4/16, 12/16)
                if (index % 2 == 0) return 6.0;  // Eighth-inch (2/16, 6/16, etc.)
                return 4.0;                      // Sixteenth-inch (odd indices)
            }
        };

        public static readonly MeasurementUnit Centimeters = new MeasurementUnit
        {
            Name = "Centimeters",
            Abbreviation = "cm",
            MajorTickInterval = 1.0,
            MinorTickSubdivisions = 10, // 10 subdivisions = 1mm steps per cm
            IsRelative = false,
            MajorTickLength = 12,
            PixelToUnit = (px, dpi, total) => (px / dpi) * 2.54,
            UnitToPixel = (val, dpi, total) => (val / 2.54) * dpi,
            ShouldLabel = val => val > 0 && Math.Abs(val % 1.0) < 0.001,
            FormatLabel = (val, isVertical) => isVertical ? $"{val:0.#} cm" : $"{val:0.#}\ncm",
            GetMinorTickLength = index =>
            {
                if (index % 5 == 0) return 9.0; // 0.5 cm mark (5mm halfway)
                return 4.0;                     // 0.1 cm marks (1mm fine ticks)
            }
        };

        public static readonly MeasurementUnit Millimeters = new MeasurementUnit
        {
            Name = "Millimeters",
            Abbreviation = "mm",
            MajorTickInterval = 10.0,
            MinorTickSubdivisions = 10,
            IsRelative = false,
            MajorTickLength = 12,
            PixelToUnit = (px, dpi, total) => (px / dpi) * 25.4,
            UnitToPixel = (val, dpi, total) => (val / 25.4) * dpi,
            ShouldLabel = val => val > 0 && Math.Abs(val % 10.0) < 0.001,
            FormatLabel = (val, isVertical) =>
            {
                if (Math.Abs(val % 10.0) < 0.001 && val > 0)
                {
                    int cm = (int)(val / 10.0);
                    return isVertical ? $"{val:0}mm ({cm}cm)" : $"{val:0}mm\n({cm}cm)";
                }
                return isVertical ? $"{val:0}mm" : $"{val:0}\nmm";
            },
            GetMinorTickLength = index =>
            {
                if (index % 5 == 0) return 9.0; // 5 mm mark (halfway)
                return 4.0;                     // 1 mm marks
            }
        };

        public static readonly MeasurementUnit Points = new MeasurementUnit
        {
            Name = "Points",
            Abbreviation = "pt",
            MajorTickInterval = 12.0,
            MinorTickSubdivisions = 12,
            IsRelative = false,
            MajorTickLength = 12,
            PixelToUnit = (px, dpi, total) => (px / dpi) * 72.0,
            UnitToPixel = (val, dpi, total) => (val / 72.0) * dpi,
            ShouldLabel = val => val > 0 && Math.Abs(val % 12.0) < 0.001,
            FormatLabel = (val, isVertical) => isVertical ? $"{val:0} pt" : $"{val:0}\npt",
            GetMinorTickLength = index =>
            {
                if (index % 6 == 0) return 8.0; // 6pt mark (halfway)
                if (index % 3 == 0) return 6.0; // 3pt mark
                return 4.0;
            }
        };

        public static readonly MeasurementUnit Percentage = new MeasurementUnit
        {
            Name = "Percentage",
            Abbreviation = "%",
            MajorTickInterval = 50.0,
            MinorTickSubdivisions = 10,
            IsRelative = true,
            MajorTickLength = 12,
            PixelToUnit = (px, dpi, total) => total > 0 ? (px / total) * 100.0 : 0,
            UnitToPixel = (val, dpi, total) => (val / 100.0) * total,
            ShouldLabel = val => val > 0 && Math.Abs(val % 50.0) < 0.001,
            FormatLabel = (val, isVertical) => isVertical ? $"{val:0}%" : $"{val:0}\n%",
            GetMinorTickLength = index =>
            {
                if (index % 5 == 0) return 10.0; // 25% mark
                if (index % 2 == 0) return 7.0;  // 10% marks
                return 4.0;                      // 5% marks
            }
        };
        private static int GCD(int a, int b) => b == 0 ? a : GCD(b, a % b);

        public static void DrawFractionStack(DrawingContext dc, Typeface typeface, Brush brush, Pen pen,
          double x, double centerY, int whole, int num, int den, double dpi, string abbreviation = "in")
        {
            double currentX = x;

            // 1. Draw whole number if greater than 0
            if (whole > 0)
            {
                var wholeText = new FormattedText(whole.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 9, brush, dpi);
                dc.DrawText(wholeText, new Point(currentX - (wholeText.Width / 2), centerY - (wholeText.Height / 2)));
                currentX += (wholeText.Width / 2) + 6; // Shift right for the fraction stack
            }

            // 2. Measure numerator and denominator for the stack
            var numText = new FormattedText(num.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 6, brush, dpi);
            var denText = new FormattedText(den.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 6, brush, dpi);
            double maxWidth = Math.Max(numText.Width, denText.Width);

            // Draw fraction stack centered at currentX
            double fractionLeft = currentX;
            dc.DrawText(numText, new Point(fractionLeft + (maxWidth - numText.Width) / 2, centerY - 9));
            dc.DrawLine(pen, new Point(fractionLeft - 1, centerY), new Point(fractionLeft + maxWidth + 1, centerY));
            dc.DrawText(denText, new Point(fractionLeft + (maxWidth - denText.Width) / 2, centerY + 1));

            // 3. Draw the Abbreviation ("in") right after the fraction stack!
            double afterFractionX = fractionLeft + maxWidth + 4;
            var abbrevText = new FormattedText(abbreviation, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 8, brush, dpi);
            dc.DrawText(abbrevText, new Point(afterFractionX, centerY - (abbrevText.Height / 2)));
        }
    }
}
