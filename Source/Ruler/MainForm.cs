using System;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace Ruler
{
	sealed public class MainForm : Form
	{
		#region ResizeRegion enum

		private enum ResizeRegion
		{
			None, N, NE, E, SE, S, SW, W, NW
		}

		#endregion

		private ToolTip			_toolTip = new ToolTip();
		private Point			_offset;
		private Rectangle		_mouseDownRect;
		private int				_resizeBorderWidth = 5;
		private Point			_mouseDownPoint;
		private ResizeRegion	_resizeRegion = ResizeRegion.None;
		private ContextMenu		_menu = new ContextMenu();
		private MenuItem		_verticalMenuItem;
		private MenuItem		_toolTipMenuItem;

		public MainForm()
		{
            SetStyle(ControlStyles.ResizeRedraw, true);
            UpdateStyles();

			ResourceManager resources = new ResourceManager(typeof(MainForm));
			Icon = ((Icon)(resources.GetObject("$this.Icon")));

			SetUpMenu();

			Text = "Ruler";
			BackColor = Color.White;
			ClientSize = new Size(400, 75);
			FormBorderStyle = FormBorderStyle.None;
			Opacity = 0.65;
			ContextMenu = _menu;
			Font = new Font("Tahoma", 10);
			
			SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
		}

		private bool IsVertical
		{
			get { return _verticalMenuItem.Checked; }
			set { _verticalMenuItem.Checked = value; }
		}

		private bool ShowToolTip
		{
			get { return _toolTipMenuItem.Checked; }
			set
			{
				_toolTipMenuItem.Checked = value;
				if (value)
				{
					SetToolTip();
				}
			}
		}

		private void SetUpMenu()
		{
			AddMenuItem("Stay On Top");
			_verticalMenuItem = AddMenuItem("Vertical");
			_toolTipMenuItem = AddMenuItem("Tool Tip");
			MenuItem opacityMenuItem = AddMenuItem("Opacity");
			AddMenuItem("-");
			AddMenuItem("About");
			AddMenuItem("-");
			AddMenuItem("Exit");

			for (int i = 10; i <= 100; i += 10)
			{
				MenuItem subMenu = new MenuItem(i + "%");
				subMenu.Click += new EventHandler(OpacityMenuHandler);
				opacityMenuItem.MenuItems.Add(subMenu);
			}
		}

		private MenuItem AddMenuItem(string text)
		{
			return AddMenuItem(text, Shortcut.None);
		}

		private MenuItem AddMenuItem(string text, Shortcut shortcut)
		{
			MenuItem mi = new MenuItem(text);
			mi.Click += new EventHandler(MenuHandler);
			mi.Shortcut = shortcut;
			_menu.MenuItems.Add(mi);
			
			return mi;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			_offset = new Point(MousePosition.X - Location.X, MousePosition.Y - Location.Y);
			_mouseDownPoint = MousePosition;
			_mouseDownRect = ClientRectangle;

			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			_resizeRegion = ResizeRegion.None;
			base.OnMouseUp(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (_resizeRegion != ResizeRegion.None)
			{
				HandleResize();
				return;
			}

			Point clientCursorPos = PointToClient(MousePosition);
			Rectangle resizeInnerRect = ClientRectangle;
			resizeInnerRect.Inflate(-_resizeBorderWidth, -_resizeBorderWidth);

			bool inResizableArea = ClientRectangle.Contains(clientCursorPos) && !resizeInnerRect.Contains(clientCursorPos);

			if (inResizableArea)
			{
				ResizeRegion resizeRegion = GetResizeRegion(clientCursorPos);
				SetResizeCursor(resizeRegion);

				if (e.Button == MouseButtons.Left)
				{
					_resizeRegion = resizeRegion;
					HandleResize();
				}
			}
			else
			{
				Cursor = Cursors.Default;

				if (e.Button == MouseButtons.Left)
				{
					Location = new Point(MousePosition.X - _offset.X, MousePosition.Y - _offset.Y);
				}
			}

			base.OnMouseMove(e);
		}

		protected override void OnResize(EventArgs e)
		{
			if (ShowToolTip)
			{
				SetToolTip();
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
			switch (_resizeRegion)
			{
				case ResizeRegion.E:
				{
					int diff = MousePosition.X - _mouseDownPoint.X;
					Width = _mouseDownRect.Width + diff;
					break;
				}
				case ResizeRegion.S:
				{
					int diff = MousePosition.Y - _mouseDownPoint.Y;
					Height = _mouseDownRect.Height + diff;
					break;
				}
				case ResizeRegion.SE:
				{
					Width = _mouseDownRect.Width + MousePosition.X - _mouseDownPoint.X;
					Height = _mouseDownRect.Height + MousePosition.Y - _mouseDownPoint.Y;
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
				if (clientCursorPos.X <= _resizeBorderWidth)				return ResizeRegion.NW;
				else if (clientCursorPos.X >= Width - _resizeBorderWidth)	return ResizeRegion.NE;
				else														return ResizeRegion.N;
			}
			else if (clientCursorPos.Y >= Height - _resizeBorderWidth)
			{
				if (clientCursorPos.X <= _resizeBorderWidth)				return ResizeRegion.SW;
				else if (clientCursorPos.X >= Width - _resizeBorderWidth)	return ResizeRegion.SE;
				else														return ResizeRegion.S;
			}
			else
			{
				if (clientCursorPos.X <= _resizeBorderWidth)				return ResizeRegion.W;
				else														return ResizeRegion.E;
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

			// Bottom
			g.DrawLine(Pens.Black, xPos, formHeight, xPos, formHeight - tickHeight);
		}

		private void DrawTickLabel(Graphics g, string text, int xPos, int formHeight, int height)
		{
			// Top
			g.DrawString(text, Font, Brushes.Black, xPos, height);
			
			// Bottom
			g.DrawString(text, Font, Brushes.Black, xPos, formHeight - height - Font.Height);
		}

		static void Main() 
		{
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
		}

		private void OpacityMenuHandler(object sender, EventArgs e)
		{
			MenuItem mi = (MenuItem)sender;
			Opacity = double.Parse(mi.Text.Replace("%", "")) / 100;
		}

		private void MenuHandler(object sender, EventArgs e)
		{
			MenuItem mi = (MenuItem)sender;
			
			switch (mi.Text)
			{
				case "Exit":
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

				case "About":
			        string message = string.Format("Ruler v{0} by Jeff Key\nwww.sliver.com\nIcon by Kristen Magee @ www.kbecca.com", Application.ProductVersion);
			        MessageBox.Show(message, "About Ruler", MessageBoxButtons.OK, MessageBoxIcon.Information);
					break;

				default:
					MessageBox.Show("Unknown menu item.");
					break;
			}
		}

		private void ChangeOrientation()
		{
			IsVertical = !IsVertical;
			int width = Width;
			Width = Height;
			Height = width;
		}
	}
}
