using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ruler.Forms
{
    public class ShortcutWindow : Form
    {
        private Panel _containerPanel;
        private TableLayoutPanel _shortcutGrid;

        public ShortcutWindow()
        {
            InitializeWindowSettings();
            InitializeComponentLayout();
            PopulateAllWpfShortcuts();
        }

        private void InitializeWindowSettings()
        {
            // Set up native dialog boundaries (expanded slightly to comfortably fit all items without clipping)
            this.ClientSize = new Size(480, 520);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Keyboard Shortcuts";

            // Layout and focus constraints
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            // Deep Dark Canvas Aesthetic 
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Force hardware rendering layers to eliminate scrolling flicker
            this.DoubleBuffered = true;
        }

        private void InitializeComponentLayout()
        {
            this.SuspendLayout();

            // 1. Setup Scrollable Surface Canvas (Emulating the WPF ScrollViewer)
            _containerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(25, 15, 25, 25)
            };

            // 2. Setup Structured Dual-Column Layout Engine (Emulating the UniformGrid Columns="2")
            _shortcutGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 0,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Allocate 40% left space for Keys (high contrast), 60% right aligned for Action Descriptions
            _shortcutGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            _shortcutGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));

            // Chain visual components to the UI hierarchy
            _containerPanel.Controls.Add(_shortcutGrid);
            this.Controls.Add(_containerPanel);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void PopulateAllWpfShortcuts()
        {
            _shortcutGrid.SuspendLayout();

            // --- SECTION 1: CORE CONTROLS ---
            AddShortcutHeader("Core Controls");
            AddShortcutRow("Space / O", "Flip Orientation", Color.White);
            AddShortcutRow("L", "Lock / Unlock Resizing", Color.White);
            AddShortcutRow("G", "Toggle Guideline Visibility", Color.White);
            AddShortcutRow("D", "Duplicate Active Ruler", Color.White);
            AddShortcutRow("Esc", "Close Active Ruler", Color.White);
            AddShortcutRow("Ctrl + Esc", "Exit All Application Instances", Color.Red); // Exact #C62828

            // --- SECTION 2: NUDGING & SIZING ---
            AddShortcutHeader("Nudging & Sizing");
            AddShortcutRow("Arrow Keys", "Ruler Nudge (10px)", Color.White);
            AddShortcutRow("Ctrl + Arrow Keys", "Fine Nudge (1px)", Color.White);
            AddShortcutRow("Ctrl + R", "Reset Ruler to Defaults", Color.White);

            // --- SECTION 3: OPACITY TWEAKS ---
            AddShortcutHeader("Opacity Tweaks");
            AddShortcutRow("Page Up / Dn", "Nudge Opacity 10%", Color.White);
            AddShortcutRow("Shift + PgUp / Dn", "Nudge Opacity 5%", Color.White, isCustomHeaderColor: true);
            AddShortcutRow("Ctrl + Page Up", "Instant Snap to 100% Opacity", Color.White);
            AddShortcutRow("Ctrl + Page Down", "Instant Snap to 10% Opacity", Color.White);



            _shortcutGrid.ResumeLayout(false);
            _shortcutGrid.PerformLayout();
        }

        private void AddShortcutHeader(string categoryTitle)
        {
            _shortcutGrid.RowCount++;
            int activeRow = _shortcutGrid.RowCount - 1;

            _shortcutGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Label lblHeader = new Label
            {
                Text = categoryTitle,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.DarkCyan, // Matches Foreground="DarkCyan"
                Margin = new Padding(0, 18, 0, 6),
                TextAlign = ContentAlignment.BottomLeft,
                AutoSize = true
            };

            _shortcutGrid.Controls.Add(lblHeader, 0, activeRow);
            _shortcutGrid.SetColumnSpan(lblHeader, 2);
        }

        private void AddShortcutRow(string keySequence, string description, Color descriptionColor, bool isCustomHeaderColor = false)
        {
            _shortcutGrid.RowCount++;
            int activeRow = _shortcutGrid.RowCount - 1;

            _shortcutGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Keys Column (Left Side) - Matches Foreground="#AAAAAA"
            Label lblKeys = new Label
            {
                Text = keySequence,
                Font = new Font("Segoe UI Semibold", 9F, isCustomHeaderColor ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = isCustomHeaderColor ? Color.DarkCyan : Color.FromArgb(170, 170, 170),
                TextAlign = ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                AutoSize = true,
                Padding = new Padding(0, 4, 0, 4)
            };

            // Description Column (Right Side) - Matches Foreground="White" / Custom styles
            Label lblDescription = new Label
            {
                Text = description,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = descriptionColor,
                TextAlign = ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                AutoSize = true,
                Padding = new Padding(0, 4, 0, 4)
            };

            // Inject elements explicitly into the respective alignment grids
            _shortcutGrid.Controls.Add(lblKeys, 0, activeRow);
            _shortcutGrid.Controls.Add(lblDescription, 1, activeRow);
        }

        /// <summary>
        /// Native command key message loop processing to close on Escape instantly.
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
