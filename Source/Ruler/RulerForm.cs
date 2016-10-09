using System;
using System.Diagnostics;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace Ruler
{
    sealed public class RulerForm : Form, IRulerInfo
    {
        #region ResizeRegion enum
        private enum ResizeRegion
        {
            None, N, NE, E, SE, S, SW, W, NW
        }
        #endregion
        #region DragMode enum
        private enum DragMode
        {
            None, Move, Resize
        }
        #endregion
        private ToolTip _toolTip = new ToolTip();
        private Point _offset;
        private Rectangle _mouseDownRect;
        private int _resizeBorderWidth = 5;
        private Point _mouseDownPoint;
        private ResizeRegion _resizeRegion = ResizeRegion.None;
        private ContextMenu _menu = new ContextMenu();
        private MenuItem _verticalMenuItem;
        private MenuItem _toolTipMenuItem;
        private MenuItem _lockedMenuItem;
        private MenuItem _topMostMenuItem;
        private DragMode _dragMode = DragMode.None;
        private RulerApplicationContext _context;
        private RulerInfo _info;

        public RulerForm(RulerApplicationContext context)
        {
            _context = context;
            _info = RulerInfo.GetDefaultRulerInfo();
            this.Init();
        }

        public RulerForm(RulerApplicationContext context, RulerInfo rulerInfo)
        {
            _context = context;
            _info = rulerInfo;
            this.Init();
        }

        public bool IsVertical
        {
            get { return this._verticalMenuItem.Checked; }
            set { this._verticalMenuItem.Checked = value; }
        }

        public bool IsLocked
        {
            get;
            set;
        }

        public bool ShowToolTip
        {
            get
            {
                return this._toolTipMenuItem.Checked;
            }
            set
            {
                this._toolTipMenuItem.Checked = value;

                if (value)
                {
                    this.SetToolTip();
                }
            }
        }

        private void Init()
        {
            this.MinimumSize = new Size(1,1);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();

            ResourceManager resources = new ResourceManager(typeof(RulerForm));
            this.Icon = ((Icon)(resources.GetObject("$this.Icon")));

            this.SetUpMenu();

            this.Text = "Ruler";
            this.BackColor = Color.White;


            this.FormBorderStyle = FormBorderStyle.None;

            this.ContextMenu = _menu;
            this.Font = new Font("Tahoma", 10);

            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

            _info.CopyInto(this);
            _topMostMenuItem.Checked = this.TopMost;
            _lockedMenuItem.Checked = this.IsLocked;
            this.ShowInTaskbar = false;

        }

        private RulerInfo GetRulerInfo()
        {
            RulerInfo rulerInfo = new RulerInfo();

            this.CopyInto(rulerInfo);

            return rulerInfo;
        }

        private void SetUpMenu()
        {
            this._topMostMenuItem = this.AddMenuItem("Stay On Top");
            this._verticalMenuItem = this.AddMenuItem("Vertical");
            this._toolTipMenuItem = this.AddMenuItem("Tool Tip");
            MenuItem opacityMenuItem = this.AddMenuItem("Opacity");
            this._lockedMenuItem = this.AddMenuItem("Lock resizing", Shortcut.None, this.LockHandler);
            this.AddMenuItem("Set size...", Shortcut.None, this.SetWidthHeightHandler);
            this.AddMenuItem("-");
            this.AddMenuItem("Duplicate", Shortcut.None, this.DuplicateHandler);
            this.AddMenuItem("Save as default", Shortcut.None, this.SaveAsDefault);
            this.AddMenuItem("-");
            this.AddMenuItem("Close");

            for (int i = 10; i <= 100; i += 10)
            {
                MenuItem subMenu = new MenuItem(i + "%");
                subMenu.Checked = i == _info.Opacity * 100;
                subMenu.Click += new EventHandler(OpacityMenuHandler);
                opacityMenuItem.MenuItems.Add(subMenu);
            }
        }

        private void SetWidthHeightHandler(object sender, EventArgs e)
        {
            SetSizeForm form = new SetSizeForm(this.Width, this.Height);

            if (this.TopMost)
            {
                form.TopMost = true;
            }

            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Size size = form.GetNewSize();

                this.Width = size.Width;
                this.Height = size.Height;
            }
        }
        private void SaveAsDefault(object sender, EventArgs e)
        {
            this.CopyInto(_info);
            _context.SaveConfig();
        }
        private void LockHandler(object sender, EventArgs e)
        {
            this.IsLocked = !this.IsLocked;
            this._lockedMenuItem.Checked = this.IsLocked;
        }

        private void DuplicateHandler(object sender, EventArgs e)
        {
            var copy = new RulerForm(_context, _info);
            copy.Show();
        }

        private MenuItem AddMenuItem(string text)
        {
            return AddMenuItem(text, Shortcut.None, this.MenuHandler);
        }

        private MenuItem AddMenuItem(string text, Shortcut shortcut, EventHandler handler)
        {
            MenuItem mi = new MenuItem(text);
            mi.Click += new EventHandler(handler);
            mi.Shortcut = shortcut;
            _menu.MenuItems.Add(mi);

            return mi;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _offset = new Point(MousePosition.X - Location.X, MousePosition.Y - Location.Y);
            _mouseDownPoint = MousePosition;
            _mouseDownRect = ClientRectangle;
            if (IsInResizableArea())
                _dragMode = DragMode.Resize;
            else
                _dragMode = DragMode.Move;
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _resizeRegion = ResizeRegion.None;
            _dragMode = DragMode.None;
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            switch (_dragMode)
            {
                case DragMode.Move:
                    Location = new Point(MousePosition.X - _offset.X, MousePosition.Y - _offset.Y);
                    break;
                case DragMode.Resize:
                    HandleResize();
                    break;
                default:
                    if (IsInResizableArea())
                    {
                        Point clientCursorPos = PointToClient(MousePosition);
                        _resizeRegion = GetResizeRegion(clientCursorPos);
                        SetResizeCursor(_resizeRegion);
                    }
                    else
                    {
                        Cursor = Cursors.Default;
                    }

                    break;
            }

            base.OnMouseMove(e);
        }

        private bool IsInResizableArea()
        {
            Point clientCursorPos = PointToClient(MousePosition);
            Rectangle resizeInnerRect = ClientRectangle;
            resizeInnerRect.Inflate(-_resizeBorderWidth, -_resizeBorderWidth);

            return ClientRectangle.Contains(clientCursorPos) && !resizeInnerRect.Contains(clientCursorPos);
        }

        protected override void OnResize(EventArgs e)
        {
            if (this.ShowToolTip)
            {
                this.SetToolTip();
            }

            base.OnResize(e);
        }

        private void SetToolTip()
        {
            _toolTip.SetToolTip(this, string.Format("Width: {0} pixels\nHeight: {1} pixels", Width, Height));
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Right:
                case Keys.Left:
                case Keys.Up:
                case Keys.Down:
                    HandleMoveResizeKeystroke(e);
                    break;

                case Keys.Space:
                    ChangeOrientation();
                    break;
            }

            base.OnKeyDown(e);
        }

        private void HandleMoveResizeKeystroke(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right)
            {
                if (e.Control)
                {
                    if (e.Shift)
                    {
                        Width += 1;
                    }
                    else
                    {
                        Left += 1;
                    }
                }
                else
                {
                    Left += 5;
                }
            }
            else if (e.KeyCode == Keys.Left)
            {
                if (e.Control)
                {
                    if (e.Shift)
                    {
                        Width -= 1;
                    }
                    else
                    {
                        Left -= 1;
                    }
                }
                else
                {
                    Left -= 5;
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (e.Control)
                {
                    if (e.Shift)
                    {
                        Height -= 1;
                    }
                    else
                    {
                        Top -= 1;
                    }
                }
                else
                {
                    Top -= 5;
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (e.Control)
                {
                    if (e.Shift)
                    {
                        Height += 1;
                    }
                    else
                    {
                        Top += 1;
                    }
                }
                else
                {
                    Top += 5;
                }
            }
        }

        private void HandleResize()
        {
            if (this.IsLocked)
            {
                return;
            }
            int xdiff = MousePosition.X - _mouseDownPoint.X;
            int ydiff = MousePosition.Y - _mouseDownPoint.Y;
            switch (_resizeRegion)
            {
                case ResizeRegion.W:
                    {
                        Width = _mouseDownRect.Width - xdiff;
                        Location = new Point(_mouseDownPoint.X + xdiff, Location.Y);
                        break;
                    }
                case ResizeRegion.NW:
                    {
                        Width = _mouseDownRect.Width - xdiff;
                        Height = _mouseDownRect.Height - ydiff;
                        Location = new Point(_mouseDownPoint.X + xdiff, _mouseDownPoint.Y + ydiff);
                        break;
                    }
                case ResizeRegion.SW:
                    {
                        Width = _mouseDownRect.Width - xdiff;
                        Height = _mouseDownRect.Height + ydiff;
                        Location = new Point(_mouseDownPoint.X + xdiff, Location.Y);
                        break;
                    }
                case ResizeRegion.N:
                    {
                        Height = _mouseDownRect.Height - ydiff;
                        Location = new Point(Location.X, _mouseDownPoint.Y + ydiff);
                        break;
                    }
                case ResizeRegion.NE:
                    {
                        Height = _mouseDownRect.Height - ydiff;
                        Width = _mouseDownRect.Width + xdiff;
                        Location = new Point(Location.X, _mouseDownPoint.Y + ydiff);
                        break;
                    }
                case ResizeRegion.E:
                    {
                        Width = _mouseDownRect.Width + xdiff;
                        break;
                    }
                case ResizeRegion.S:
                    {
                        Height = _mouseDownRect.Height + ydiff;
                        break;
                    }
                case ResizeRegion.SE:
                    {
                        Width = _mouseDownRect.Width + xdiff;
                        Height = _mouseDownRect.Height + ydiff;
                        break;
                    }
            }
        }

        private void SetResizeCursor(ResizeRegion region)
        {
            switch (region)
            {
                case ResizeRegion.N:
                case ResizeRegion.S:
                    Cursor = Cursors.SizeNS;
                    break;

                case ResizeRegion.E:
                case ResizeRegion.W:
                    Cursor = Cursors.SizeWE;
                    break;

                case ResizeRegion.NW:
                case ResizeRegion.SE:
                    Cursor = Cursors.SizeNWSE;
                    break;

                default:
                    Cursor = Cursors.SizeNESW;
                    break;
            }
        }

        private ResizeRegion GetResizeRegion(Point clientCursorPos)
        {
            if (clientCursorPos.Y <= _resizeBorderWidth)
            {
                if (clientCursorPos.X <= _resizeBorderWidth) return ResizeRegion.NW;
                else if (clientCursorPos.X >= Width - _resizeBorderWidth) return ResizeRegion.NE;
                else return ResizeRegion.N;
            }
            else if (clientCursorPos.Y >= Height - _resizeBorderWidth)
            {
                if (clientCursorPos.X <= _resizeBorderWidth) return ResizeRegion.SW;
                else if (clientCursorPos.X >= Width - _resizeBorderWidth) return ResizeRegion.SE;
                else return ResizeRegion.S;
            }
            else
            {
                if (clientCursorPos.X <= _resizeBorderWidth) return ResizeRegion.W;
                else return ResizeRegion.E;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;

            int height = Height;
            int width = Width;

            if (IsVertical)
            {
                graphics.RotateTransform(90);
                graphics.TranslateTransform(0, -Width + 1);
                height = Width;
                width = Height;
            }

            DrawRuler(graphics, width, height);

            base.OnPaint(e);
        }

        private void DrawRuler(Graphics g, int formWidth, int formHeight)
        {
            // Border
            g.DrawRectangle(Pens.Black, 0, 0, formWidth - 1, formHeight - 1);

            // Width
            g.DrawString(formWidth + " pixels", Font, Brushes.Black, 10, (formHeight / 2) - (Font.Height / 2));

            // Ticks
            for (int i = 0; i < formWidth; i++)
            {
                if (i % 2 == 0)
                {
                    int tickHeight;
                    if (i % 100 == 0)
                    {
                        tickHeight = 15;
                        DrawTickLabel(g, i.ToString(), i, formHeight, tickHeight);
                    }
                    else if (i % 10 == 0)
                    {
                        tickHeight = 10;
                    }
                    else
                    {
                        tickHeight = 5;
                    }

                    DrawTick(g, i, formHeight, tickHeight);
                }
            }
        }

        private static void DrawTick(Graphics g, int xPos, int formHeight, int tickHeight)
        {
            // Top
            g.DrawLine(Pens.Black, xPos, 0, xPos, tickHeight);

            if (formHeight > 60)
            {
                // Bottom
                g.DrawLine(Pens.Black, xPos, formHeight, xPos, formHeight - tickHeight);
            }
        }

        private void DrawTickLabel(Graphics g, string text, int xPos, int formHeight, int height)
        {
            // Top
            g.DrawString(text, Font, Brushes.Black, xPos, height);
            if (formHeight > 60)
            {
                // Bottom
                g.DrawString(text, Font, Brushes.Black, xPos, formHeight - height - Font.Height);
            }
        }
        
        private void OpacityMenuHandler(object sender, EventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            UncheckMenuItem(mi.Parent);
            mi.Checked = true;
            Opacity = double.Parse(mi.Text.Replace("%", "")) / 100;
        }

        private void UncheckMenuItem(Menu parent)
        {
            if (parent == null || parent.MenuItems == null)
            {
                return;
            }

            for (int i = 0; i < parent.MenuItems.Count; i++)
            {
                if (parent.MenuItems[i].Checked)
                {
                    parent.MenuItems[i].Checked = false;
                }
            }
        }

        private void MenuHandler(object sender, EventArgs e)
        {
            MenuItem mi = (MenuItem)sender;

            switch (mi.Text)
            {
                case "Close":
                    Close();
                    break;

                case "Tool Tip":
                    ShowToolTip = !ShowToolTip;
                    break;

                case "Vertical":
                    ChangeOrientation();
                    break;

                case "Stay On Top":
                    mi.Checked = !mi.Checked;
                    TopMost = mi.Checked;
                    break;
                
                default:
                    MessageBox.Show("Unknown menu item.");
                    break;
            }
        }

        public void ChangeOrientation()
        {
            this.IsVertical = !IsVertical;
            int width = Width;
            this.Width = Height;
            this.Height = width;
        }
    }
}