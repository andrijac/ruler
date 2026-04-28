using Ruler.Properties;
using Ruler.Shared.Enums;
using Ruler.Shared.Factories;
using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;
using Ruler.Shared.Services;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;


namespace Ruler.Forms
{
    public partial class MainForm : Form
    {
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_SIZE = 0xF000;
        private const int WM_SIZING = 0x0214;
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        private const int HT_LEFT = 10;
        private const int HT_RIGHT = 11;
        private const int HT_TOP = 12;
        private const int HT_BOTTOM = 15;
        private const int HT_BOTTOMRIGHT = 17;
        private const int LabelPadding = 22;
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_RBUTTONDOWN = 0x0204;
        const int WM_LBUTTONUP = 0x0202;
        const int WM_RBUTTONUP = 0x0205;

        
        private Point _startLocation;
        private bool _hasMoved=false;
        private readonly RulerInfo _rulerInfo;
        private ContextMenuStrip _contextMenuStrip;
        private InteractionMode _currentMode = InteractionMode.None;
        private HitArea _activeArea;
        private Point _dragStartCursorPos;  // Global screen position
        private Point _dragStartFormPos;    // Original form location
        private Size _dragStartFormSize;

        public RulerInfo RulerData => _rulerInfo; // Expose the RulerInfo for external use (e.g., duplication)  
        public MainForm(RulerInfo info)
        {

            _rulerInfo = info;
            InitializeComponent();
          
            this.FormBorderStyle = FormBorderStyle.None;
            this.AutoScaleMode = AutoScaleMode.None;
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true;
            //this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.Selectable, true);
            //this.SetStyle(ControlStyles.AllPaintingInWmPaint |
            //              ControlStyles.UserPaint);
            this.UpdateStyles();
           
            using (Graphics g = this.CreateGraphics())
            {
                float dpiX = g.DpiX; // e.g., 144 for 150% scaling
                float scaleFactor = dpiX / 96f; // 144/96 = 1.5

                // 2. Adjust the desired size to compensate
                // If we want 400 logical pixels, we need 400 * 1.5 = 600 physical pixels
                int targetWidth = (int)(_rulerInfo.Width * scaleFactor);
                int targetHeight = (int)(_rulerInfo.Height * scaleFactor);

                this.Size = new Size(targetWidth, targetHeight);
            }
            // 2. Build the context menu
            _contextMenuStrip = new ContextMenuStrip();
            CreateMenuItems();
            _contextMenuStrip.Opening += (s, e) =>
            {
                ValidateAllItems(_contextMenuStrip.Items); // Sync checkmarks before showing
            };
            // 3. Initial safety check
            ValidateScreenBounds();
           
        }
        protected override void WndProc(ref Message m)
        {
            // If the ruler is locked, we want to block resize messages
            if (_rulerInfo.IsLocked)
            {
                // 1. Block manual edge dragging
                if (m.Msg == WM_SIZING)
                {
                    return; // Exit without calling base.WndProc, blocking the resize
                }

                // 2. Block system menu resizing (e.g., clicking the title bar icon)
                if (m.Msg == WM_SYSCOMMAND && (m.WParam.ToInt32() & 0xFFF0) == SC_SIZE)
                {
                    return; // Exit without calling base.WndProc
                }
            }

            // Always let the base class handle everything else
            base.WndProc(ref m);
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // Force the size to be exactly what you want, regardless of what Windows did
            this.Size = new Size(_rulerInfo.Width, _rulerInfo.Height);
            this.TopMost = _rulerInfo.TopMost;  
            this.Opacity = _rulerInfo.Opacity;
        }
        private void ValidateAllItems(ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    UpdateCheckedState(menuItem);
                    if (menuItem.HasDropDownItems)
                    {
                        ValidateAllItems(menuItem.DropDownItems); // Recurse!
                    }
                }
            }
        }

        private void UpdateCheckedState(ToolStripMenuItem item)
        {
            if (item.Tag == null) return;

            // 1. Check for boolean properties (like 'TopMost')
            if (item.Tag is string propName)
            {
                var prop = _rulerInfo.GetType().GetProperty(propName);
                if (prop != null && prop.PropertyType == typeof(bool))
                {
                    item.Checked = (bool)prop.GetValue(_rulerInfo);
                }
            }
            // 2. Check for double values (like 'Opacity')
            else if (item.Tag is double tagVal)
            {
                item.Checked = Math.Abs(_rulerInfo.Opacity - tagVal) < 0.001;
            }
            else if (item.Tag is SaveTypes typeValue)
            {
                item.Checked = (_rulerInfo.SaveType == typeValue);
            }
        }
        private void CreateMenuItems()
        {

            ToolStripMenuItem miTopMost = new ToolStripMenuItem("Stay On Top");
            miTopMost.Tag = nameof(_rulerInfo.TopMost); // Use Tag to link to the property for syncing
            miTopMost.Click += (s, e) =>
            {
                this.TopMost = _rulerInfo.TopMost = !_rulerInfo.TopMost;
            };
            _contextMenuStrip.Items.Add(miTopMost);
            ToolStripMenuItem miLockResize = new ToolStripMenuItem("Lock Resizing");
            miLockResize.Tag = nameof(_rulerInfo.IsLocked); // Link to the IsLocked property
            miLockResize.Click += (s, e) =>
            {
                _rulerInfo.IsLocked = !_rulerInfo.IsLocked;
            }; // Sync the checkmark
            _contextMenuStrip.Items.Add(miLockResize);
            ToolStripMenuItem miIsVertical = new ToolStripMenuItem("Is Vertical?");
            miIsVertical.Tag = nameof(_rulerInfo.IsVertical); // Link to the IsVertical property
            miIsVertical.Click += (s, e) =>
            {
                _rulerInfo.ToggleOrientation();
                this.Size = new Size(_rulerInfo.Width, _rulerInfo.Height); // Ensure size is correct after orientation change
                miIsVertical.Checked = _rulerInfo.IsVertical; // Sync the checkmark
                this.Invalidate();
                // Handle the actual rotation logic
            };
            _contextMenuStrip.Items.Add(miIsVertical);
            ToolStripMenuItem miShowTooltip = new ToolStripMenuItem("Show Tootip");
            miShowTooltip.Tag = nameof(_rulerInfo.ShowToolTip); // Link to the ShowToolTip property
            miShowTooltip.Click += (s, e) =>
            {
                _rulerInfo.ShowToolTip = !_rulerInfo.ShowToolTip;
                miShowTooltip.Checked = _rulerInfo.ShowToolTip; // Sync the checkmark
            };
            _contextMenuStrip.Items.Add(miShowTooltip);
            _contextMenuStrip.Items.Add(CreateOpacityMenu("Opacity"));
            ToolStripMenuItem miSetSize = new ToolStripMenuItem("Set Size");
            miSetSize.Click += (s, e) =>
            {
                using (SetSizeForm form = new SetSizeForm(_rulerInfo.Width, _rulerInfo.Height))
                {
                    // Keep the dialog accessible if the ruler is pinned
                    if (this.TopMost) form.TopMost = true;
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        Size size = form.GetNewSize();
                        // Update through properties to sync with _rulerInfo
                        _rulerInfo.Width = size.Width;
                        _rulerInfo.Height = size.Height;
                    }
                }
            };
            ToolStripMenuItem miDuplicate = new ToolStripMenuItem("Duplicate");
            miDuplicate.Click += (s, e) =>
            {
                RulerInfo newInfo = new RulerInfo();
                RulerFactory.CopyValues(this._rulerInfo, newInfo);
                MainForm newForm = new MainForm(newInfo);
                RulerApplicationContext.Register(newForm);
                newForm.Show();
            };
            _contextMenuStrip.Items.Add(miSetSize);
            _contextMenuStrip.Items.Add(miDuplicate);
            ToolStripMenuItem miShowGuideline = new ToolStripMenuItem("Show Guideline");
            miShowGuideline.Tag = nameof(_rulerInfo.IsGuidelineEnabled); // Link to the IsGuidelineEnabled property
            miShowGuideline.Click += (s, e) =>
            {
                _rulerInfo.IsGuidelineEnabled = !_rulerInfo.IsGuidelineEnabled;
                this.Invalidate(); // Trigger a repaint to show/hide the guideline
            };
            _contextMenuStrip.Items.Add(miShowGuideline);
            ToolStripMenuItem miUpdate = new ToolStripMenuItem("Check for updates...");
            miUpdate.Click += async (s, e) =>
            {
                UpdateService updateService = new UpdateService();
                // Subscribe to the restart request
                updateService.OnRequestRestart = () =>
                {
                    // This calls the static method in your context
                    RulerApplicationContext.CloseAll();
                    // Optionally launch the updater process here
                    // Process.Start("Updater.exe");
                };
                await updateService.CheckForUpdates();
                if (updateService.UpdateAvailable)
                {
                    if (MessageBox.Show("Update found! Restart and update?", "Update", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        bool success = await updateService.DownloadUpdateAsync();
                        if (success)
                        {
                            updateService.InstallUpdate();
                        }
                    }
                }
            };
            _contextMenuStrip.Items.Add(miUpdate);
            ToolStripMenuItem miReset = new ToolStripMenuItem("Reset");
            ToolStripMenuItem miResetDefault = new ToolStripMenuItem("Reset To Default");
            miResetDefault.Click += (s, e) =>
            {
                // Get fresh defaults from the Library Factory
                var defaults = RulerFactory.CreateDefault();
                RulerFactory.CopyValues(defaults, _rulerInfo);
                // Refresh UI by invalidating the form (which triggers a repaint)
                this.Invalidate();
            };
            ToolStripMenuItem miClearSaved = new ToolStripMenuItem("Reset All Saved Rulers to Default");
            miClearSaved.Click += (s, e) =>
            {
                if (MessageBox.Show("Are you sure you want to clear all saved rulers? This cannot be undone.", "Confirm Clear", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    RulerApplicationContext.ClearAll();
                    MessageBox.Show("All saved rulers have been cleared.");
                    foreach (MainForm form in Application.OpenForms.OfType<MainForm>())
                    {
                        form._rulerInfo.SaveType = SaveTypes.none;
                        RulerApplicationContext.Register(form); // Re-register to update the context's tracking of open forms
                    }
                    MessageBox.Show("All ruler's save type set to 'Do not Save' to reflect cleared saved data.");
                }
            };
            miReset.DropDownItems.Add(miResetDefault);
            miReset.DropDownItems.Add(miClearSaved);
            _contextMenuStrip.Items.Add(CreateEnumMenu("Save Type?"));
            ToolStripMenuItem miAbout = new ToolStripMenuItem("About...");
            miAbout.Click += (s, e) =>
            {
                string message = string.Format(
              "Original Ruler implemented by Jeff Key\n" +
              "www.sliver.com\n" +
              "ruler.codeplex.com\n" +
              "Icon by Kristen Magee @ www.kbecca.com.\n" +
              "Maintained by Andrija Cacanovic\n" +
              "Hosted on \n" +
              "https://github.com/andrijac/ruler\n" +
              "Version {0}",
              Application.ProductVersion);
                MessageBox.Show(message, "About Ruler", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            _contextMenuStrip.Items.Add(miAbout);
            ToolStripMenuItem miClose = new ToolStripMenuItem("Close");
            miClose.Click += (s, e) => { this.Close(); };
            ToolStripMenuItem miExitApplication = new ToolStripMenuItem("Exit");
            miExitApplication.Click += (s, e) =>
            {
                RulerApplicationContext.CloseAll();
                Application.Exit();
            };
            _contextMenuStrip.Items.Add(miClose);
            _contextMenuStrip.Items.Add(miExitApplication);
        }
        private ToolStripMenuItem CreateEnumMenu(string label)
        {
            var subMenu = new ToolStripMenuItem(label);

            foreach (SaveTypes type in Enum.GetValues(typeof(SaveTypes)))
            {
                var item = new ToolStripMenuItem(type.ToString());

                // 1. Click logic: Update the source of truth
                item.Click += (s, e) => _rulerInfo.SaveType = type;
                item.Tag = type;
                // Initial state
                item.Checked = (_rulerInfo.SaveType == type);
                subMenu.DropDownItems.Add(item);
            }

            return subMenu;
        }

        private ToolStripMenuItem CreateOpacityMenu(string label)
        {
            var subMenu = new ToolStripMenuItem(label);
            subMenu.Tag = nameof(_rulerInfo.Opacity); // Tag to identify this menu for syncing      
            double[] doubles = { 0.05, 0.1, 0.15, 0.2, 0.25, 0.3, 0.35, 0.4, 0.45, 0.5, 0.55, 0.6, 0.65, 0.7, 0.75, 0.8, 0.85, 0.9, 0.95, 1.0 };
            foreach (double op in doubles)
            {
                var item = new ToolStripMenuItem((op * 100).ToString());
                item.Tag = op; // Use Tag to store the actual double value for comparison
                // 1. Click logic: Update the source of truth
                item.Click += (s, e) =>
                {
                    _rulerInfo.Opacity = op;
                    if (item.Owner is ToolStripDropDown parentMenu)
                    {
                        foreach (ToolStripItem sibling in parentMenu.Items)
                        {
                            if (sibling is ToolStripMenuItem menuSibling)
                            {
                                menuSibling.Checked = false;
                            }
                        }
                    }
                    item.Checked = _rulerInfo.Opacity == op; // Sync the checkmark immediately on click
                    this.Invalidate();
                };

                subMenu.DropDownItems.Add(item);
            }

            return subMenu;
        }

        private void ValidateScreenBounds()
        {
            // Logic to ensure the ruler is on-screen
            Rectangle rulerRect = new Rectangle(_rulerInfo.Left, _rulerInfo.Top, _rulerInfo.Width, _rulerInfo.Height);
            if (!Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(rulerRect)))
            {
                _rulerInfo.Left = 100; // Reset to safe default
                _rulerInfo.Top = 100;
                this.Location = new Point(_rulerInfo.Left, _rulerInfo.Top);
            }
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            // Sync size changes back to RulerInfo
            _rulerInfo.Width = this.Width;
            _rulerInfo.Height = this.Height;
            this.Invalidate(); // Trigger a repaint to reflect size changes
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
          if (_rulerInfo.IsGuidelineEnabled)
            {
              if (_rulerInfo.Guideline.IsLocked)
                {
                    return;
                }     
            }
            Cursor = Cursors.Default;
            this.Invalidate();
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (_rulerInfo.IsGuidelineEnabled)
            {
                this.Invalidate();
            }

        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _dragStartCursorPos = Cursor.Position;
                _dragStartFormPos = this.Location;
                _dragStartFormSize = this.Size;
                _startLocation = e.Location; // Capture the initial click position for later comparison
                _hasMoved = false;


                if (_rulerInfo.IsGuidelineEnabled && _rulerInfo.Guideline != null)
                {
                    int distanceToGuideline = _rulerInfo.IsVertical
                        ? Math.Abs(e.Y - (int)_rulerInfo.Guideline.Position)
                        : Math.Abs(e.X - (int)_rulerInfo.Guideline.Position);
                    if (distanceToGuideline < 5)
                    {
                        _rulerInfo.Guideline.IsLocked = !_rulerInfo.Guideline.IsLocked;
                        this.Invalidate();
                        return;
                    }
                }

               
                _activeArea = GetHitArea(e.Location);
                _currentMode = (_activeArea == HitArea.None) ? InteractionMode.Drag : InteractionMode.Resize;

                _rulerInfo.Guideline.Position = _rulerInfo.IsVertical ? e.Location.Y : e.Location.X;
            }
         
           
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left)
            {
                // Only trigger the guideline toggle if no significant movement occurred
                if (!_hasMoved)
                {
                    _rulerInfo.IsGuidelineEnabled = true;
                    _rulerInfo.Guideline.Position = _rulerInfo.IsVertical ? e.Location.Y : e.Location.X;
                    this.Invalidate();
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                _contextMenuStrip.Show(Cursor.Position);
            }

            _currentMode = InteractionMode.None;
            _hasMoved = false; // Reset for next click
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool d = _hasMoved;
            if (_rulerInfo.IsGuidelineEnabled)
            {
                if (!_rulerInfo.Guideline.IsLocked && _rulerInfo.Guideline != null  )
                {
                    _rulerInfo.Guideline.Position = _rulerInfo.IsVertical ? e.Location.Y : e.Location.X;
                    this.Invalidate(); // Trigger a repaint to move the guideline
                }
            }
            if (e.Button == MouseButtons.Left)
            {
                if (!_hasMoved)
                {
                    // Check if the mouse has moved far enough to be a real drag
                    int dx = Math.Abs(e.Location.X - _startLocation.X);
                    int dy = Math.Abs(e.Location.Y - _startLocation.Y);

                    if (dx > SystemInformation.DragSize.Width || dy > SystemInformation.DragSize.Height)
                    {
                        _hasMoved = true;
                    }
                }

                // Only perform the action if the move threshold has been crossed
                if (_hasMoved)
                {
                    ExecuteInteraction(Cursor.Position);
                }
            }
            else
            {
                // Update cursor visual without triggering movement
                UpdateCursor(e.Location);
            }

            // Only set the cursor if we are NOT in the middle of a move/resize

        }
        private void UpdateCursor(Point mousePos)
        {
            HitArea area = GetHitArea(mousePos);

            // 1. Diagonal Resizing
            if (area.HasFlag(HitArea.Top | HitArea.Left) || area.HasFlag(HitArea.Bottom | HitArea.Right))
            {
                this.Cursor = Cursors.SizeNWSE;
            }


            else if (area.HasFlag(HitArea.Top | HitArea.Right) || area.HasFlag(HitArea.Bottom | HitArea.Left))
            {
                this.Cursor = Cursors.SizeNESW;
            }

            // 2. Horizontal/Vertical Resizing
            else if (area.HasFlag(HitArea.Left) || area.HasFlag(HitArea.Right))
            {
                this.Cursor = Cursors.SizeWE;
            }
            else if (area.HasFlag(HitArea.Top) || area.HasFlag(HitArea.Bottom))
            {
                this.Cursor = Cursors.SizeNS;
            }

            // 3. Move (Anywhere else)
            else
            {
                this.Cursor = Cursors.Arrow;
            }
            }
        
        private void ExecuteInteraction(Point currentPos)
        {
            int dx = Cursor.Position.X - _dragStartCursorPos.X;
            int dy = Cursor.Position.Y - _dragStartCursorPos.Y;

            if (_currentMode == InteractionMode.Resize)
            {
                // Calculate bounds based on original state
                int newWidth = Math.Max(50, _dragStartFormSize.Width + (_activeArea.HasFlag(HitArea.Right) ? dx : (_activeArea.HasFlag(HitArea.Left) ? -dx : 0)));
                int newHeight = Math.Max(50, _dragStartFormSize.Height + (_activeArea.HasFlag(HitArea.Bottom) ? dy : (_activeArea.HasFlag(HitArea.Top) ? -dy : 0)));

                // Calculate new origin if resizing from Left or Top
                int newLeft = _activeArea.HasFlag(HitArea.Left) ? _dragStartFormPos.X + dx : _dragStartFormPos.X;
                int newTop = _activeArea.HasFlag(HitArea.Top) ? _dragStartFormPos.Y + dy : _dragStartFormPos.Y;

                this.SetBounds(newLeft, newTop, newWidth, newHeight);
            }
            else if (_currentMode == InteractionMode.Drag)
            {
                // Smoothly update location relative to the start position
                this.Location = new Point(_dragStartFormPos.X + dx, _dragStartFormPos.Y + dy);
            }

            this.Invalidate();
        }
        private HitArea GetHitArea(Point pt)
        {
            HitArea area = HitArea.None;
            int margin = 8; // Size of the "grab" zone in pixels

            // Check Horizontal bounds
            if (pt.X < margin)
                area |= HitArea.Left;
            else if (pt.X > this.Width - margin)
                area |= HitArea.Right;

            // Check Vertical bounds
            if (pt.Y < margin)
                area |= HitArea.Top;
            else if (pt.Y > this.Height - margin)
                area |= HitArea.Bottom;

            return area;
        }
        private void DrawHorizontalRuler(Graphics g)
        {
            // Start at 0, increment by 2, go until width
            for (int i = 0; i <= this.Width; i += 2)
            {
                DrawTickWithLogic(g, i, this.Height, Orientation.Horizontal);
            }
        }

        private void DrawVerticalRuler(Graphics g)
        {
            for (int i = 0; i <= this.Height; i += 2)
            {
                DrawTickWithLogic(g, i, this.Width, Orientation.Vertical);
            }
        }
        private void DrawTickWithLogic(Graphics g, int i, int crossAxis, Orientation orientation)
        {
            // Determine Tick Height
            int tickHeight = (i % 100 == 0) ? 15 : ((i % 10 == 0) ? 10 : 5);

            // Draw Ticks on both sides
            if (orientation == Orientation.Horizontal)
            {
                g.DrawLine(Pens.Black, i, 0, i, tickHeight);               // Top
                g.DrawLine(Pens.Black, i, this.Height, i, this.Height - tickHeight); // Bottom
            }
            else
            {
                g.DrawLine(Pens.Black, 0, i, tickHeight, i);               // Left
                g.DrawLine(Pens.Black, this.Width, i, this.Width - tickHeight, i);   // Right
            }

            // Label Logic: Handle 100s, 10s, and the special 85 marker
            if (i % 100 == 0 || i == 85)
            {
                string label = i.ToString();
                DrawLabelOnBothSides(g, i, label, orientation);
            }
        }
        private void DrawGuideline(Graphics g)
        {
            // A single line crossing the ruler at the current mouse position
            Pen p = Pens.Red;   

            if (!_rulerInfo.IsVertical)
                g.DrawLine(p, (float)_rulerInfo.Guideline.Position, 0, (float)_rulerInfo.Guideline.Position, this.Height);
            else
                g.DrawLine(p, 0, (float)_rulerInfo.Guideline.Position, this.Width, (float)_rulerInfo.Guideline.Position);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (!_rulerInfo.IsVertical)
            {
                DrawHorizontalRuler(e.Graphics);
            }
            else
            {
                DrawVerticalRuler(e.Graphics);
            }
            bool isMouseOver = this.ClientRectangle.Contains(this.PointToClient(Cursor.Position));

            if (_rulerInfo.IsGuidelineEnabled && (isMouseOver || _rulerInfo.IsLocked))
            {
                DrawGuideline(e.Graphics);
            }
            if (_rulerInfo.ShowToolTip)
            {
                StringFormat centerFormat = new StringFormat();
                centerFormat.Alignment = StringAlignment.Center;      // Horizontal center
                centerFormat.LineAlignment = StringAlignment.Center;  // Vertical center

                // 2. Define the area you want the text to be centered in
                RectangleF centerRect = new RectangleF(0, 0, this.Width, this.Height);

                if (_rulerInfo.IsGuidelineEnabled)
                {
                    string toolTipText = $"Size: {this.Width} x {this.Height}\nGuideline at: {(int)_rulerInfo.Guideline.Position}";
                    // 3. Draw the shadow (shifted 1 pixel)
                    e.Graphics.DrawString(toolTipText, this.Font, Brushes.White,
                        new RectangleF(centerRect.X + 1, centerRect.Y + 1, centerRect.Width, centerRect.Height),
                        centerFormat);

                    // 4. Draw the main text
                    e.Graphics.DrawString(toolTipText, this.Font, Brushes.Blue, centerRect, centerFormat);
                }
                else
                {
                    string tooltipText = $"Size: {this.Width} x {this.Height}";
                    // 3. Draw the shadow (shifted 1 pixel)
                    e.Graphics.DrawString(tooltipText, this.Font, Brushes.White,
                        new RectangleF(centerRect.X + 1, centerRect.Y + 1, centerRect.Width, centerRect.Height),
                        centerFormat);

                    // 4. Draw the main text
                    e.Graphics.DrawString(tooltipText, this.Font, Brushes.Blue, centerRect, centerFormat);
                }
            }
        }
        private void DrawLabelOnBothSides(Graphics g, int pos, string text, Orientation orientation)
        {
            Font font = this.Font;
            SizeF size = g.MeasureString(text, font);
            int threshold = 85;

            if (orientation == Orientation.Horizontal)
            {
                if (this.Height < threshold)
                {
                    float centerX = pos - (size.Width / 2);
                    float centerY = (this.Height - size.Height) / 2;
                    g.DrawString(text, font, Brushes.Black, centerX, centerY);
                }
                else
                {
                    // Using LabelPadding instead of hardcoded 16
                    g.DrawString(text, font, Brushes.Black, pos - (size.Width / 2), LabelPadding);
                    g.DrawString(text, font, Brushes.Black, pos - (size.Width / 2), this.Height - LabelPadding - size.Height);
                }
            }
            else // Vertical
            {
                if (this.Width < threshold)
                {
                    float centerX = (this.Width - size.Width) / 2;
                    float centerY = pos - (size.Height / 2);
                    g.DrawString(text, font, Brushes.Black, centerX, centerY);
                }
                else
                {
                    g.DrawString(text, font, Brushes.Black, LabelPadding, pos - (size.Height / 2));
                    g.DrawString(text, font, Brushes.Black, this.Width - LabelPadding - size.Width, pos - (size.Height / 2));
                }
            }
        }
        public void SetTooltip(string text)
        {
            ToolTip tt = new ToolTip();
            tt.Show(text, this, 1000);
        }
    
        private void MainForm_Click(object sender, EventArgs e)
        {
   //         MessageBox.Show("Main form clicked!");
        }
    }
}

