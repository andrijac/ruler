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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;


namespace Ruler.Forms
{
    //    public partial class MainForm : Form, IRulerInfo
    //    {
    //        #region Fields & Properties

    //        // Data and Menu tracking
    //        private RulerInfo _rulerInfo;
    //        private List<MenuItemHolder> menuItemList;

    //        // Interaction state for dragging/resizing
    //        private bool isMouseResizeCommand;
    //        private Point offset;
    //        private Point mouseDownPoint;
    //        private Rectangle mouseDownRect;
    //        private Point mouseDownFormLocation;
    //        private int staticMarkerDelta=0;

    //        // Properties to quickly access RulerInfo states
    //        //public bool IsLocked => _rulerInfo.IsLocked;
    //        //public bool IsVertical => _rulerInfo.IsVertical;
    //        public RulerInfo RulerData => _rulerInfo;
    //        #endregion

    //        public MainForm(RulerInfo info)
    //        {

    //            InitializeComponent();
    //            _rulerInfo = info;
    //            // Setup Form Style
    //            this.FormBorderStyle = FormBorderStyle.None;
    //            this.DoubleBuffered = true;
    //            this.ShowInTaskbar = true;
    //            // Apply initial state from RulerInfo
    //           \

    //            // Initialize the Context Menu
    //            CreateMenuItems(_rulerInfo);
    //            this.StartPosition = FormStartPosition.Manual;
    //            this.DoubleBuffered = true;
    //            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
    //                          ControlStyles.UserPaint |
    //                          ControlStyles.OptimizedDoubleBuffer, true);
    //        }


    //        //private void ApplyRulerSettings(IRulerInfo rulerInfo)
    //        //{
    //        //    Debug.WriteLine($"RulerInfo {rulerInfo.ToString()}");
    //        //    this.SuspendLayout();
    //        //    // Use the interface type instead of the class type
    //        //    PropertyInfo[] properties = typeof(IRulerInfo).GetProperties();

    //        //    foreach (PropertyInfo sourceProp in properties)
    //        //    {
    //        //        // Skip properties that need special handling
    //        //        if (sourceProp.Name == "Location" || sourceProp.Name == "DisplayLocation") continue;
    //        //        if (sourceProp.Name == "Orientation") Debug.WriteLine($"Applying Orientation: {sourceProp.GetValue(rulerInfo)}");
    //        //        // Map interface property to Form property
    //        //        PropertyInfo targetProp = this.GetType().GetProperty(sourceProp.Name);

    //        //        if (targetProp != null && targetProp.CanWrite)
    //        //        {
    //        //            object value = sourceProp.GetValue(rulerInfo);
    //        //            targetProp.SetValue(this, value);
    //        //        }
    //        //    }

    //        //    if (!ValidateScreenBounds())
    //        //    {
    //        //        MessageBox.Show("Saved ruler position is off-screen. Resetting to default location.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    //        //        this.Location = new Point(100, 100);
    //        //    }
    //        //    else
    //        //    {
    //        //        // Explicitly apply the location
    //        //        this.Location = rulerInfo.DisplayLocation;
    //        //    }
    //        //    this.ResumeLayout();
    //        //    this.Invalidate();
    //        //}

    //        #region Resizing and Dragging

    //        private bool GetIsInResizableArea()
    //        {
    //            if (this.IsLocked) return false;

    //            const int border = 8;
    //            Point pt = this.PointToClient(Control.MousePosition);

    //            // Check edges
    //            return pt.X <= border || pt.X >= (this.Width - border) ||
    //                   pt.Y <= border || pt.Y >= (this.Height - border);
    //        }

    //        protected override void OnMouseDown(MouseEventArgs e)
    //        {
    //            this.TopMost = false;
    //            // 1. Call base first to let the form handle internal events
    //            base.OnMouseDown(e);

    //            // 2. Add your logic
    //            if (e.Button == MouseButtons.Left)
    //            {
    //                this.isMouseResizeCommand = GetIsInResizableArea();
    //                this.offset = e.Location;

    //                // Guideline calculation
    //                this.staticMarkerDelta = (_rulerInfo.Orientation == Orientation.Vertical)
    //                    ? e.Location.Y : e.Location.X;

    //                this.mouseDownPoint = e.Location;
    //            }
    //            this.mouseDownPoint = e.Location;
    //            this.mouseDownFormLocation = this.Location;
    //            this.mouseDownRect = new Rectangle(this.Location, this.Size);
    //        }
    //        //    base.OnMouseDown(e); // Best practice: call base first

    //        //    if (e.Button == MouseButtons.Left)
    //        //    {
    //        //        // 1. Setup Drag/Resize logic
    //        //        this.isMouseResizeCommand = GetIsInResizableArea();

    //        //        // Use e.Location (Client coordinates) instead of MousePosition. 
    //        //        // This avoids window border/title bar offset issues.
    //        //        this.offset = e.Location;

    //        //        // 2. Setup Guideline Logic
    //        //        // Update the marker position based on the click
    //        //        if (_rulerInfo.Orientation == Orientation.Vertical)
    //        //        {
    //        //            this.staticMarkerDelta = e.Location.Y;
    //        //        }
    //        //        else
    //        //        {
    //        //            this.staticMarkerDelta = e.Location.X;
    //        //        }

    //        //        // 3. Track state for the potential "Click vs Drag" distinction
    //        //        this.mouseDownPoint = e.Location;
    //        //        this.mouseDownFormLocation = this.Location;
    //        //        this.mouseDownRect = new Rectangle(this.Location, this.Size);

    //        //        // 4. Force a redraw to show the guideline line immediately
    //        //        // this.Invalidate();
    //        //        //if (e.Button == MouseButtons.Left)
    //        //        //{
    //        //        //    this.isMouseResizeCommand = GetIsInResizableArea();

    //        //        //    // Capture all starting coordinates for the math in OnMouseMove
    //        //        //    this.offset = new Point(MousePosition.X - this.Location.X, MousePosition.Y - this.Location.Y);
    //        //        //    this.mouseDownPoint = MousePosition;
    //        //        //    this.mouseDownRect = new Rectangle(this.Location, this.Size);
    //        //        //    this.mouseDownFormLocation = this.Location;
    //        //        //}
    //        //        //base.OnMouseDown(e);
    //        //    }
    //        //  }
    //        protected override void OnMouseUp(MouseEventArgs e)
    //        {
    //            this.TopMost = true;
    //            // If the mouse hasn't moved significantly, we treat it as a "Click"
    //            // and show/toggle the guideline.
    //            if (e.Button == MouseButtons.Left)
    //            {
    //                // Add a small threshold (e.g., 3 pixels) to differentiate drag from click
    //                if (Math.Abs(e.Location.X - mouseDownPoint.X) < 3 &&
    //                    Math.Abs(e.Location.Y - mouseDownPoint.Y) < 3)
    //                {
    //                    _rulerInfo.IsGuideline = true; // Activate the line
    //                    this.Invalidate();
    //                }
    //            }
    //            base.OnMouseUp(e);
    //        }
    //        protected override void OnResize(EventArgs e)
    //        {
    //            base.OnResize(e);
    //            this.Invalidate(); // Redraw to adjust the ruler markings to the new size
    //        }
    //        public bool ValidateScreenBounds()
    //        {
    //            // Define the full rectangle of the ruler
    //            Rectangle rulerRect = new Rectangle(_rulerInfo.Left, _rulerInfo.Top, _rulerInfo.Width, _rulerInfo.Height);

    //            // Check if the ruler intersects with ANY monitor
    //            bool isVisible = false;
    //            foreach (Screen screen in Screen.AllScreens)
    //            {
    //                if (screen.WorkingArea.IntersectsWith(rulerRect))
    //                {
    //                    isVisible = true;
    //                    break;
    //                }
    //            }

    //            if (!isVisible)  return false;
    //                return true;
    //            //{
    //            //    // Snap to the primary screen if completely lost
    //            //    _rulerInfo.Left = 100;
    //            //    _rulerInfo.Top = 100;

    //            //    // Update the form itself if this is called from MainForm
    //            //    this.Location = new Point(100, 100);
    //            //}
    //        }
    //        protected override void OnMouseMove(MouseEventArgs e)
    //        {

    //            if (e.Button == MouseButtons.None)
    //            {
    //                this.Cursor = GetIsInResizableArea() ? Cursors.SizeAll : Cursors.Default;
    //            }
    //            else if (e.Button == MouseButtons.Left)
    //            {
    //                if (isMouseResizeCommand)
    //                {
    //                    // Basic stretching from the bottom-right
    //                    int newWidth = e.Location.X;
    //                    int newHeight = e.Location.Y;
    //                    if (this.Width != newWidth || this.Height != newHeight)
    //                    {
    //                        this.Size = new Size(Math.Max(50, newWidth), Math.Max(50, newHeight));
    //                        _rulerInfo.Width = this.Width;
    //                        _rulerInfo.Height = this.Height;
    //                    }
    //                }
    //                else
    //                {
    //                    // Move the entire window
    //                    Point screenPos = Control.MousePosition;
    //                    screenPos.Offset(-offset.X, -offset.Y);

    //                    this.Location = screenPos;

    //                    // Update your info object
    //                    _rulerInfo.Left = screenPos.X;
    //                    _rulerInfo.Top = screenPos.Y;
    //                    this.DisplayLocation = _rulerInfo.DisplayLocation;
    //                }
    //            }
    //            base.OnMouseMove(e);
    //        }

    //        #endregion

    //        #region Context Menu Construction

    //        private void CreateMenuItems(RulerInfo rulerInfo)
    //        {
    //            this.ContextMenu = new ContextMenu();

    //            var list = new List<MenuItemHolder>()
    //            {
    //                new MenuItemHolder(MenuItemEnum.TopMost, "Stay On Top", this.TopMostHandler, rulerInfo.TopMost),
    //                new MenuItemHolder(MenuItemEnum.Vertical, "Vertical", this.VerticalHandler, rulerInfo.IsVertical),
    //                new MenuItemHolder(MenuItemEnum.ShowToolTip, "Tool Tip", this.ShowToolTipHandler, rulerInfo.ShowToolTip),
    //                new MenuItemHolder(MenuItemEnum.Opacity, "Opacity", null, false),
    //                new MenuItemHolder(MenuItemEnum.LockResize, "Lock Resizing", this.LockResizeHandler, rulerInfo.IsLocked),
    //                new MenuItemHolder(MenuItemEnum.SetSize, "Set size...", this.SetSizeHandler, false),
    //                new MenuItemHolder(MenuItemEnum.Duplicate, "Duplicate", this.DuplicateHandler, false),
    //                new MenuItemHolder(MenuItemEnum.Update, "Check for updates...", async (s, e) => await this.UpdateHandler(s, e), false),
    //                MenuItemHolder.Separator,
    //                new MenuItemHolder(MenuItemEnum.Reset, "Reset To Default", this.ResetToDefaultHandler, false),
    //                new MenuItemHolder(MenuItemEnum.ClearSaved, "Clear Saved Rulers",ResetAllRulersHandler, false),
    //                MenuItemHolder.Separator,
    //                new MenuItemHolder(MenuItemEnum.About, "About...", this.AboutHandler, false),
    //                MenuItemHolder.Separator,

    //#if DEBUG
    //              //  new MenuItemHolder(MenuItemEnum.RulerInfo, "Copy RulerInfo", (s,e) => Clipboard.SetText(_rulerInfo.Id.ToString()), false),
    //              //  MenuItemHolder.Separator,
    //#endif
    //                new MenuItemHolder(MenuItemEnum.Save, "Save Settings?", this.SaveHandler, false),
    //                new MenuItemHolder(MenuItemEnum.Exit, "Exit", this.ExitHandler, false)
    //            };

    //            // Build Opacity Sub-menu
    //            MenuItem opacityMenuItem = list.Find(m => m.MenuItemEnum == MenuItemEnum.Opacity).MenuItem;
    //            for (int i = 10; i <= 100; i += 10)
    //            {
    //                opacityMenuItem.MenuItems.Add(new MenuItem($"{i}%", this.OpacityMenuHandler)
    //                {
    //                    Checked = i == (int)(rulerInfo.Opacity * 100)
    //                });
    //            }

    //            // Build Save Sub-menu
    //            MenuItem saveMenuItem = list.Find(m => m.MenuItemEnum == MenuItemEnum.Save).MenuItem;
    //            saveMenuItem.MenuItems.Add(new MenuItem("Do not Save", (s, e) => SetSaveType(SaveTypes.none)) { Checked = rulerInfo.SaveType == SaveTypes.none });
    //            saveMenuItem.MenuItems.Add(new MenuItem("Save Location", (s, e) => SetSaveType(SaveTypes.location)) { Checked = rulerInfo.SaveType == SaveTypes.location });
    //            saveMenuItem.MenuItems.Add(new MenuItem("Save Size", (s, e) => SetSaveType(SaveTypes.size)) { Checked = rulerInfo.SaveType == SaveTypes.size });
    //            saveMenuItem.MenuItems.Add(new MenuItem("Save complete ruler", (s, e) => SetSaveType(SaveTypes.all)) { Checked = rulerInfo.SaveType == SaveTypes.all });

    //            list.ForEach(mh => this.ContextMenu.MenuItems.Add(mh.MenuItem));
    //            this.menuItemList = list;
    //        }
    //        private void SetSaveType(SaveTypes type)
    //        {
    //            // 1. Update the property (which syncs with _rulerInfo)
    //            this.SaveType = type;

    //            // 2. Sync the UI checkmarks
    //            // First, find the 'Save' menu holder in our tracked list
    //            var saveWrapper = menuItemList.Find(m => m.MenuItemEnum == MenuItemEnum.Save);

    //            if (saveWrapper != null && saveWrapper.MenuItem != null)
    //            {
    //                foreach (MenuItem subItem in saveWrapper.MenuItem.MenuItems)
    //                {
    //                    // The checkmark logic depends on matching the SaveType
    //                    // We can determine the type by the text of the menu item
    //                    switch (subItem.Text)
    //                    {
    //                        case "Do not Save":
    //                            subItem.Checked = (type == SaveTypes.none);
    //                            break;
    //                        case "Save Location":
    //                            subItem.Checked = (type == SaveTypes.location);
    //                            break;
    //                        case "Save Size":
    //                            subItem.Checked = (type == SaveTypes.size);
    //                            break;
    //                        case "Save complete ruler":
    //                            subItem.Checked = (type == SaveTypes.all);
    //                            break;
    //                    }
    //                }
    //            }
    //        }
    //        #endregion
    //        public bool IsVertical
    //        {
    //            get => _rulerInfo.IsVertical;
    //            set
    //            {
    //                if (_rulerInfo.IsVertical != value)
    //                {
    //                    _rulerInfo.IsVertical = value;
    //                    //    // Physical rotation logic
    //                    //    int temp = this.Width;
    //                    //    this.Width = this.Height;
    //                    //    this.Height = temp;
    //                }
    //            }
    //        }
    //        public new int Top
    //        {
    //            get => _rulerInfo.Top;
    //            set => _rulerInfo.Top = value;
    //        }
    //        public bool IsGuideline
    //        {
    //            get => _rulerInfo.IsGuideline;
    //            set => _rulerInfo.IsGuideline = value;
    //        }
    //         public double GuidelineLocation
    //        {
    //            get => _rulerInfo.GuidelineLocation;
    //            set => _rulerInfo.GuidelineLocation = value;
    //        }
    //        public Orientation Orientation
    //        {
    //            get { Debug.WriteLine($"Getting Orientation: {_rulerInfo.Orientation}");
    //                return _rulerInfo.Orientation; }
    //            }
    //        public new  int Left
    //        {
    //            get => _rulerInfo.Left;
    //            set => _rulerInfo.Left = value;
    //        }
    //        public string DisplayLocationString
    //        {
    //            get => _rulerInfo.DisplayLocationString;
    //            set => _rulerInfo.DisplayLocationString = value;
    //        }
    //        public bool IsLocked
    //        {
    //            get => _rulerInfo.IsLocked;
    //            set => _rulerInfo.IsLocked = value;
    //        }

    //        public bool ShowToolTip
    //        {
    //            get => _rulerInfo.ShowToolTip;
    //            set => _rulerInfo.ShowToolTip = value;
    //        }

    //        public SaveTypes SaveType
    //        {
    //            get => _rulerInfo.SaveType;
    //            set => _rulerInfo.SaveType = value;
    //        }
    //        public Point DisplayLocation
    //        {
    //            get => this.Location;
    //            set
    //            {
    //                this.Location = value;
    //                _rulerInfo.Left = value.X;
    //                _rulerInfo.Top = value.Y;
    //            }
    //        }

    //        // Wrapping standard Form properties to sync with RulerInfo
    //        public new bool TopMost
    //        {
    //            get => base.TopMost;
    //            set
    //            {
    //                base.TopMost = value;
    //                _rulerInfo.TopMost = value;
    //            }
    //        }

    //        public new double Opacity
    //        {
    //            get => base.Opacity;
    //            set
    //            {
    //                base.Opacity = value;
    //                _rulerInfo.Opacity = value;
    //            }
    //        }

    //        #region Action Handlers

    //#if DEBUG
    //private void CopyRulerInfo(object sender, EventArgs e)
    //{
    //    // Logic: Convert current state to parameters (Shared Library logic)
    //    string parameters = RulerFactory.ToParameterString(this.RulerData);
    //    Clipboard.SetText(parameters);
    //    MessageBox.Show($"Copied to clipboard:{Environment.NewLine}{parameters}");
    //}
    //#endif

    //        private void SetSizeHandler(object sender, EventArgs e)
    //        {
    //            using (SetSizeForm form = new SetSizeForm(this.Width, this.Height))
    //            {
    //                // Keep the dialog accessible if the ruler is pinned
    //                if (this.TopMost) form.TopMost = true;

    //                if (form.ShowDialog() == DialogResult.OK)
    //                {
    //                    Size size = form.GetNewSize();
    //                    // Update through properties to sync with _rulerInfo
    //                    this.Width = size.Width;
    //                    this.Height = size.Height;
    //                }
    //            }
    //        }
    //        private void ResetAllRulersHandler(object sender, EventArgs e)
    //        {
    //            if (MessageBox.Show("Are you sure you want to clear all saved rulers? This cannot be undone.", "Confirm Clear", MessageBoxButtons.YesNo) == DialogResult.Yes)
    //            {
    //                RulerApplicationContext.ClearAll();
    //                MessageBox.Show("All saved rulers have been cleared.");
    //               foreach (MainForm form in Application.OpenForms.OfType<MainForm>())
    //                {
    //                   form.SaveType = SaveTypes.none;
    //                   RulerApplicationContext.Register(form); // Re-register to update the context's tracking of open forms
    //                }
    //                MessageBox.Show("Current ruler's save type set to 'Do not Save' to reflect cleared saved data.");
    //            }
    //        }

    //        private void DuplicateHandler(object sender, EventArgs e)
    //        {
    //           RulerInfo newInfo = new RulerInfo();
    //            RulerFactory.CopyValues(this.RulerData, newInfo);
    //            MainForm newForm = new MainForm(newInfo);
    //            RulerApplicationContext.Register(newForm);
    //            newForm.Show();
    //        }

    //        private void OpacityMenuHandler(object sender, EventArgs e)
    //        {
    //            MenuItem item = (MenuItem)sender;

    //            // Clear other checkmarks in sub-menu
    //            foreach (MenuItem mi in item.Parent.MenuItems) mi.Checked = false;
    //            item.Checked = true;

    //            // Use property to update both UI and RulerInfo
    //            string val = item.Text.Replace("%", string.Empty);
    //            if (double.TryParse(val, out double opacity))
    //            {
    //                this.Opacity = _rulerInfo.Opacity = opacity / 100;
    //            }
    //        }

    //        private async Task UpdateHandler(object sender, EventArgs e)
    //        {
    //            UpdateService updateService = new UpdateService();

    //            // Subscribe to the restart request
    //            updateService.OnRequestRestart = () =>
    //            {
    //                // This calls the static method in your context
    //                RulerApplicationContext.CloseAll();

    //                // Optionally launch the updater process here
    //                // Process.Start("Updater.exe");
    //            };

    //            await updateService.CheckForUpdates();

    //            if (updateService.UpdateAvailable)
    //            {
    //                if (MessageBox.Show("Update found! Restart amd update?", "Update", MessageBoxButtons.YesNo) == DialogResult.Yes)
    //                {
    //                    bool success = await updateService.DownloadUpdateAsync();
    //                    if (success)
    //                    {
    //                        updateService.InstallUpdate();
    //                    }
    //                }
    //            }
    //        }

    //        private void ExitHandler(object sender, EventArgs e)
    //        {
    //            this.Close();
    //        }

    //        public void SaveHandler(object sender, EventArgs e)
    //        {
    //            MenuItem item = (MenuItem)sender;
    //            foreach (MenuItem mi in item.Parent.MenuItems) mi.Checked = false;
    //            item.Checked = true;

    //            // Standard .NET 4.8 switch statement
    //            switch (item.Text)
    //            {
    //                case "Save Location":
    //                    this.SaveType = SaveTypes.location;
    //                    break;
    //                case "Save Size":
    //                    this.SaveType = SaveTypes.size;
    //                    break;
    //                case "Save complete ruler":
    //                    this.SaveType = SaveTypes.all;
    //                    break;
    //                default:
    //                    this.SaveType = SaveTypes.none;
    //                    break;
    //            }
    //        }
    //        private void ApplyOrientationChanged()
    //        {
    //            // 3. Update Stat
    //            Debug.WriteLine($" Orientation after update: {_rulerInfo.Orientation}");
    //    //        _rulerInfo.IsVertical = this.IsVertical;

    //            // 4. Perform the swap
    //            Size temp = this.Size;
    //            this.Size = new Size(temp.Height, temp.Width);

    //            this.Invalidate();
    //        }
    //        private void VerticalHandler(object sender, EventArgs e)
    //        {
    //            this.IsVertical = !this.IsVertical;
    //            ApplyOrientationChanged();
    //        }
    //        private void TopMostHandler(object sender, EventArgs e) => this.TopMost = !this.TopMost;
    //        private void ShowToolTipHandler(object sender, EventArgs e) => this.ShowToolTip = !this.ShowToolTip;
    //        private void LockResizeHandler(object sender, EventArgs e) => this.IsLocked = !this.IsLocked;

    //        private void ResetToDefaultHandler(object sender, EventArgs e)
    //        {
    //            // Get fresh defaults from the Library Factory
    //            var defaults = RulerFactory.CreateDefault();
    //            RulerFactory.CopyValues(_rulerInfo, defaults);
    //            ApplyRulerSettings(_rulerInfo); // Refresh UI
    //        }
    //        private void AboutHandler(object sender, EventArgs e)
    //        {
    //            // Gather version info from the assembly
    //            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
    //            string versionDisplay = $"{version.Major}.{version.Minor}.{version.Build}";

    //            string message = string.Format(
    //                "Ruler Tool\n\n" +
    //                "Version: {0}\n\n" +
    //                "Credits:\n" +
    //                "• Original implementation by Jeff Key (sliver.com)\n" +
    //                "• Maintained by Andrija Cacanovic\n" +
    //                "• Modified and modernized by Isaac Morris\n\n" +
    //                "Icons by Kristen Magee (kbecca.com)\n" +
    //                "Hosted on GitHub: github.com/andrijac/ruler",
    //                versionDisplay);

    //            MessageBox.Show(message, "About Ruler", MessageBoxButtons.OK, MessageBoxIcon.Information);
    //        }

    //        #endregion

    //        #region Paint

    //        protected override void OnPaint(PaintEventArgs e)
    //        {
    //            base.OnPaint(e);
    //            e.Graphics.Clear(this.BackColor);
    //            if (_rulerInfo.IsGuideline)
    //            {
    //                DrawGuideline(e.Graphics);
    //            }
    //            if (_rulerInfo.Orientation == Orientation.Vertical)
    //            {
    //                DrawVerticalRuler(e.Graphics);
    //            }
    //            else
    //            {
    //                DrawHorizontalRuler(e.Graphics);
    //            }
    //            //Graphics graphics = e.Graphics;

    //            //int height = this.Height;
    //            //int width = this.Width;

    //            //if (this.IsVertical)
    //            //{
    //            //    graphics.RotateTransform(90);
    //            //    graphics.TranslateTransform(0, -this.Width + 1);
    //            //    height = this.Width;
    //            //    width = this.Height;
    //            //}

    //            //DrawRuler(graphics, width, height, this.Font, this.staticMarkerDelta);

    //            //base.OnPaint(e);
    //        }
    //        private void DrawHorizontalRuler(Graphics g)
    //        {
    //            int width = this.ClientSize.Width;
    //            int height = this.ClientSize.Height;

    //            // Draw main scale line
    //            g.DrawLine(Pens.Black, 0, height - 1, width, height - 1);

    //            for (int x = 0; x <= width; x += 10)
    //            {
    //                // Draw tick marks
    //                int tickHeight = (x % 50 == 0) ? 15 : 5; // Major ticks longer
    //                g.DrawLine(Pens.Black, x, height - tickHeight, x, height);
    //                g.DrawLine(Pens.Black, x, 0, x, tickHeight); // Top ticks

    //                // Draw labels for major ticks
    //                if (x % 50 == 0)
    //                {
    //                    // Define a format that centers the text
    //                    StringFormat centerFormat = new StringFormat();
    //                    centerFormat.Alignment = StringAlignment.Center; // Horizontal center
    //                    centerFormat.LineAlignment = StringAlignment.Center; // Vertical center (relative to bounding box)
    //                    //g.DrawString(x.ToString(), this.Font, Brushes.Black, x, height - 30);
    //                    if (height > 70)
    //                    {
    //                        g.DrawString(x.ToString(), this.Font, Brushes.Black, x, 22,centerFormat);
    //                        g.DrawString(x.ToString(), this.Font, Brushes.Black, x, height - 22,centerFormat);
    //                    }
    //                    else
    //                    {
    //                        g.DrawString(x.ToString(), this.Font, Brushes.Black, x, height / 2);
    //                    }
    //                }
    //            }
    //        }
    //        private void DrawVerticalRuler(Graphics g)
    //        {
    //            int width = this.ClientSize.Width;
    //            int height = this.ClientSize.Height;

    //            // Draw main scale line
    //            g.DrawLine(Pens.Black, width - 1, 0, width - 1, height);

    //            for (int y = 0; y <= height; y += 10)
    //            {
    //                // Draw tick marks (extending horizontally)
    //                int tickWidth = (y % 50 == 0) ? 15 : 5;
    //                g.DrawLine(Pens.Black, width - tickWidth, y, width, y);
    //                g.DrawLine(Pens.Black, 0, y, tickWidth, y);

    //                // Draw labels for major ticks
    //                if (y % 50 == 0)
    //                {
    //                    StringFormat centerFormat = new StringFormat();
    //                    centerFormat.Alignment = StringAlignment.Center; // Horizontal center
    //                    centerFormat.LineAlignment = StringAlignment.Center; // Vertical center (relative to bounding box)

    //                    if (width > 80)
    //                    {
    //                        g.DrawString(y.ToString(), this.Font, Brushes.Black, width - 30, y, centerFormat);
    //                        g.DrawString(y.ToString(), this.Font, Brushes.Black, 30, y, centerFormat);
    //                    }
    //                    else
    //                    {
    //                        string label = y.ToString();
    //                        // 1. Measure how wide the text is
    //                        SizeF textSize = g.MeasureString(label, this.Font);

    //                        // 2. Subtract half the text width from the center coordinate
    //                        float xPosition = (width / 2) - (textSize.Width / 2);

    //                        // 3. Draw using that calculated position
    //                        g.DrawString(label, this.Font, Brushes.Black, xPosition, y);
    //                    }
    //                }
    //            }
    //        }

    //        private static void DrawRuler(Graphics g, int formWidth, int formHeight, Font font, int staticMarker)
    //        {
    //            float markerLoc = staticMarker;
    //            // Border
    //            g.DrawRectangle(Pens.Black, 0, 0, formWidth - 1, formHeight - 1);

    //            // Width
    //            g.DrawString(markerLoc + " pixels", font, Brushes.Black, 10, (formHeight / 2) - (font.Height / 2));

    //            // Ticks
    //            for (int i = 0; i < formWidth; i++)
    //            {
    //                if (i % 2 == 0)
    //                {
    //                    int tickHeight;

    //                    if (i % 100 == 0)
    //                    {
    //                        tickHeight = 15;
    //                        DrawTickLabel(g, i.ToString(), i, formHeight, tickHeight, font);
    //                    }
    //                    else if (i % 10 == 0)
    //                    {
    //                        tickHeight = 10;
    //                    }
    //                    else
    //                    {
    //                        tickHeight = 5;
    //                    }

    //                    DrawTick(g, i, formHeight, tickHeight);
    //                    if (i == staticMarker)
    //                    {
    //                        g.DrawLine(Pens.BlueViolet, i, 0, i, formHeight);
    //                    }
    //                }
    //            }
    //        }

    //        private static void DrawTick(Graphics g, int xPos, int formHeight, int tickHeight)
    //        {
    //            // Top
    //            g.DrawLine(Pens.Black, xPos, 0, xPos, tickHeight);

    //            // Bottom
    //            g.DrawLine(Pens.Black, xPos, formHeight, xPos, formHeight - tickHeight);
    //        }

    //        private static void DrawTickLabel(Graphics g, string text, int xPos, int formHeight, int height, Font font)
    //        {
    //            // Top
    //            g.DrawString(text, font, Brushes.Black, xPos, height);

    //            // Bottom
    //            g.DrawString(text, font, Brushes.Black, xPos, formHeight - height - font.Height);
    //        }
    //        private void DrawGuideline(Graphics g)
    //        {
    //            using (Pen guidelinePen = new Pen(Color.Red, 2))
    //            {
    //                if (_rulerInfo.Orientation == Orientation.Horizontal)
    //                {
    //                    // Draw a horizontal line at the captured Y position
    //                    g.DrawLine(guidelinePen, staticMarkerDelta,0,staticMarkerDelta,this.ClientSize.Height);
    //                }
    //                else
    //                {
    //                    // Draw a vertical line at the captured X position
    //                    g.DrawLine(guidelinePen,0, staticMarkerDelta, this.ClientSize.Width,staticMarkerDelta);
    //                }
    //            }
    //        }

    //        #endregion Paint

    //        private void MainForm_Load(object sender, EventArgs e)
    //        {
    //         //   this.Location = _rulerInfo.DisplayLocation;
    //        }
    //    }
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
            Debug.WriteLine(_rulerInfo.ToString());
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
            Debug.WriteLine(_rulerInfo.ToString());
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
                else
                {
                    _rulerInfo.IsGuidelineEnabled = false;
                   
                }
                
            }
            Cursor = Cursors.Default;
            this.Invalidate();
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

                // 1. Check for Resize
                HitArea area = GetHitArea(e.Location);
                if (area==HitArea.None)
                {
                    _hasMoved = true;
                }
                _currentMode = (area == HitArea.None) ? InteractionMode.Drag  : InteractionMode.Resize;
                _activeArea = area;

                _rulerInfo.Guideline.Position = _rulerInfo.IsVertical ? e.Location.Y : e.Location.X;
            }
         
           
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            Debug.WriteLine("In Mouseup event");
            if (e.Button == MouseButtons.Left)
            {
                //Compare the current position to the start position
                if (e.Location != _startLocation)
                {
                    // The window moved! Update the model
                    _rulerInfo.Left = e.Location.X;
                    _rulerInfo.Top = e.Location.Y;
                    this.Location = new Point(_rulerInfo.Left, _rulerInfo.Top);
                }
                else
                {
                   _rulerInfo.IsGuidelineEnabled = true;
                    _rulerInfo.Guideline.Position = _rulerInfo.IsVertical ? _startLocation.Y : _startLocation.X;
                    this.Invalidate();
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                // Right-click: Show context menu
                _contextMenuStrip.Show(e.Location);
                return;
            }
            _currentMode = InteractionMode.None; // Reset interaction mode after mouse up

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
                    int dx = Math.Abs(e.Location.X - _startLocation.X);
                    int dy = Math.Abs(e.Location.Y - _startLocation.Y);
                    if (dx > SystemInformation.DragSize.Width || dy > SystemInformation.DragSize.Height)
                    {
                        _hasMoved = true; // The user has moved the mouse enough to be considered a drag
                    }
                }
                if (_hasMoved)
                {
                    ExecuteInteraction(Cursor.Position);
                }
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
            Point currentCursorPos = Cursor.Position;
            int dx = currentCursorPos.X - _dragStartCursorPos.X;
            int dy = currentCursorPos.Y - _dragStartCursorPos.Y;
            //int deltaX = currentPos.X - _startLocation.X;
            //int deltaY = currentPos.Y - _startLocation.Y;
            Debug.WriteLine("DeltaX "+dx);
            Debug.WriteLine("DeltaY" +dy);

            if (_currentMode == InteractionMode.Resize)
            {
                // Calculate new bounds based on the initial state
                int newLeft = _dragStartFormPos.X;
                int newTop = _dragStartFormPos.Y;
                int newWidth = _dragStartFormSize.Width;
                int newHeight = _dragStartFormSize.Height;

                if (_activeArea.HasFlag(HitArea.Right)) newWidth += dx;
                if (_activeArea.HasFlag(HitArea.Bottom)) newHeight += dy;

                if (_activeArea.HasFlag(HitArea.Left)) { newLeft += dx; newWidth -= dx; }
                if (_activeArea.HasFlag(HitArea.Top)) { newTop += dy; newHeight -= dy; }
                // Apply all changes atomically to prevent flickering
                this.SetBounds(newLeft, newTop, newWidth, newHeight);
            }
            else if (_currentMode == InteractionMode.Drag)
            {
                // Simply offset the current position
                this.Location = new Point(_dragStartFormPos.X + dx, _dragStartFormPos.Y + dy);
            }

            // Update the starting location so movement remains relative 
            // to the previous delta, preventing exponential snapping
            //_startLocation = currentPos;

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

            if (_rulerInfo.IsGuidelineEnabled)
            {
                DrawGuideline(e.Graphics);
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

