using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ruler.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for ShowShortcutsWindow.xaml
    /// </summary>
    public partial class ShortcutWindow : Window
    {
        private int _currentRow = 0;

        public ShortcutWindow()
        {
            InitializeComponent();
            PopulateShortcuts();
        }

        private void PopulateShortcuts()
        {
            // --- SECTION 1: CORE CONTROLS ---
            AddShortcutHeader("Core Controls");
            AddShortcutRow("Space / O", "Flip Orientation", Brushes.White);
            AddShortcutRow("L", "Lock / Unlock Resizing", Brushes.White);
            AddShortcutRow("G", "Toggle Guideline Visibility", Brushes.White);
            AddShortcutRow("D", "Duplicate Active Ruler", Brushes.White);
            AddShortcutRow("Esc", "Close Active Ruler", Brushes.White);
            AddShortcutRow("Ctrl + Esc", "Exit All Application Instances", Brushes.Red);

            // --- SECTION 2: NUDGING & SIZING ---
            AddShortcutHeader("Nudging & Sizing");
            AddShortcutRow("Arrow Keys", "Ruler Nudge (10px)", Brushes.White);
            AddShortcutRow("Ctrl + Arrow Keys", "Fine Nudge (1px)", Brushes.White);
            AddShortcutRow("Ctrl + R", "Reset Ruler to Defaults", Brushes.White);

            // --- SECTION 3: OPACITY TWEAKS ---
            AddShortcutHeader("Opacity Tweaks");
            AddShortcutRow("Page Up / Dn", "Nudge Opacity 10%", Brushes.White);
            AddShortcutRow("Shift + PgUp / Dn", "Nudge Opacity 5%", Brushes.DarkCyan, isBold: true);
            AddShortcutRow("Ctrl + Page Up", "Instant Snap to 100% Opacity", Brushes.White);
            AddShortcutRow("Ctrl + Page Down", "Instant Snap to 10% Opacity", Brushes.White);
        }

        private void AddShortcutHeader(string categoryTitle)
        {
            var header = new TextBlock
            {
                Text = categoryTitle,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkCyan,
                Margin = new Thickness(0, 18, 0, 6)
            };

            Grid.SetColumnSpan(header, 2);
            Grid.SetRow(header, _currentRow++);
            ShortcutGrid.Children.Add(header);
        }

        private void AddShortcutRow(string key, string desc, Brush color, bool isBold = false)
        {
            ShortcutGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var lblKey = new TextBlock
            {
                Text = key,
                Foreground = isBold ? Brushes.DarkCyan : new SolidColorBrush(Color.FromRgb(170, 170, 170)),
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 4, 0, 4)
            };

            var lblDesc = new TextBlock
            {
                Text = desc,
                Foreground = color,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 4, 0, 4)
            };

            Grid.SetRow(lblKey, _currentRow);
            Grid.SetColumn(lblKey, 0);

            Grid.SetRow(lblDesc, _currentRow);
            Grid.SetColumn(lblDesc, 1);

            ShortcutGrid.Children.Add(lblKey);
            ShortcutGrid.Children.Add(lblDesc);
            _currentRow++;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}
