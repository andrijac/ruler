using System;
using System.Diagnostics;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace Ruler
{
	sealed public class MainForm : Form, IRulerInfo
	{
		#region ResizeRegion enum

		private enum ResizeRegion
		{
			None, N, NE, E, SE, S, SW, W, NW
		}

		#endregion ResizeRegion enum

		private ToolTip toolTip;
		private Point offset;
		private Rectangle mouseDownRect;
		private int resizeBorderWidth;
		private Point mouseDownPoint;
		private ResizeRegion resizeRegion;
		private ContextMenu contextMenu;
		private MenuItem verticalMenuItem;
		private MenuItem toolTipMenuItem;
		private MenuItem lockedMenuItem;

		public MainForm()
		{
			this.InitLocalVars();

			RulerInfo rulerInfo = RulerInfo.GetDefaultRulerInfo();

			this.Init(rulerInfo);
		}

		public MainForm(RulerInfo rulerInfo)
		{
			this.InitLocalVars();

			this.Init(rulerInfo);
		}

		public bool IsVertical
		{
			get { return this.verticalMenuItem.Checked; }
			set { this.verticalMenuItem.Checked = value; }
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
				return this.toolTipMenuItem.Checked;
			}
			set
			{
				this.toolTipMenuItem.Checked = value;

				if (value)
				{
					this.SetToolTip();
				}
				else
				{
					this.RemoveToolTip();
				}
			}
		}

		protected override void OnMouseDoubleClick(MouseEventArgs e)
		{
			base.OnMouseDoubleClick(e);

			if (e.Button == MouseButtons.Left)
			{
				this.ChangeOrientation();
			}
		}

		private void InitLocalVars()
		{
			this.toolTip = new ToolTip();
			this.toolTip.AutoPopDelay = 10000;
			this.toolTip.InitialDelay = 1;

			this.resizeRegion = ResizeRegion.None;
			this.contextMenu = new ContextMenu();
			this.resizeBorderWidth = 5;
		}

		private void Init(RulerInfo rulerInfo)
		{
			this.SetStyle(ControlStyles.ResizeRedraw, true);
			this.UpdateStyles();

			ResourceManager resources = new ResourceManager(typeof(MainForm));
			this.Icon = ((Icon)(resources.GetObject("$this.Icon")));
			this.Opacity = rulerInfo.Opacity;

			this.SetUpMenu();

			this.Text = "Ruler";
			this.BackColor = Color.White;

			RulerInfo.CopyInto(rulerInfo, this);

			this.FormBorderStyle = FormBorderStyle.None;

			this.ContextMenu = contextMenu;
			this.Font = new Font("Tahoma", 10);

			this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
		}

		private RulerInfo GetRulerInfo()
		{
			RulerInfo rulerInfo = new RulerInfo();

			RulerInfo.CopyInto(this, rulerInfo);

			return rulerInfo;
		}

		private void SetUpMenu()
		{
			this.AddMenuItem("Stay On Top");
			this.verticalMenuItem = this.AddMenuItem("Vertical");
			this.toolTipMenuItem = this.AddMenuItem("Tool Tip");
			MenuItem opacityMenuItem = this.AddMenuItem("Opacity");
			this.lockedMenuItem = this.AddMenuItem("Lock resizing", Shortcut.None, this.LockHandler);
			this.AddMenuItem("Set size...", Shortcut.None, this.SetWidthHeightHandler);
			this.AddMenuItem("Duplicate", Shortcut.None, this.DuplicateHandler);
			this.AddMenuItem("-");
			this.AddMenuItem("About...");
			this.AddMenuItem("-");
			this.AddMenuItem("Exit");

			for (int i = 10; i <= 100; i += 10)
			{
				MenuItem subMenu = new MenuItem(i + "%");
				subMenu.Checked = i == Opacity * 100;
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

		private void LockHandler(object sender, EventArgs e)
		{
			this.IsLocked = !this.IsLocked;
			this.lockedMenuItem.Checked = this.IsLocked;
		}

		private void DuplicateHandler(object sender, EventArgs e)
		{
			string exe = System.Reflection.Assembly.GetExecutingAssembly().Location;

			RulerInfo rulerInfo = this.GetRulerInfo();

			ProcessStartInfo startInfo = new ProcessStartInfo(exe, rulerInfo.ConvertToParameters());

			Process process = new Process
			{
				StartInfo = startInfo
			};

			process.Start();
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
			this.contextMenu.MenuItems.Add(mi);

			return mi;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			this.offset = new Point(MousePosition.X - Location.X, MousePosition.Y - Location.Y);
			this.mouseDownPoint = MousePosition;
			this.mouseDownRect = ClientRectangle;

			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			this.resizeRegion = ResizeRegion.None;
			base.OnMouseUp(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (this.resizeRegion != ResizeRegion.None)
			{
				this.HandleResize();
				return;
			}

			Point clientCursorPos = PointToClient(MousePosition);
			Rectangle resizeInnerRect = ClientRectangle;
			resizeInnerRect.Inflate(-resizeBorderWidth, -resizeBorderWidth);

			bool inResizableArea = ClientRectangle.Contains(clientCursorPos) && !resizeInnerRect.Contains(clientCursorPos);

			if (inResizableArea)
			{
				ResizeRegion resizeRegion = this.GetResizeRegion(clientCursorPos);
				this.SetResizeCursor(resizeRegion);

				if (e.Button == MouseButtons.Left)
				{
					this.resizeRegion = resizeRegion;
					this.HandleResize();
				}
			}
			else
			{
				Cursor = Cursors.Default;

				if (e.Button == MouseButtons.Left)
				{
					Location = new Point(MousePosition.X - offset.X, MousePosition.Y - offset.Y);
				}
			}

			base.OnMouseMove(e);
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
			toolTip.SetToolTip(this, string.Format("Width: {0} pixels\nHeight: {1} pixels", Width, Height));
		}

		private void RemoveToolTip()
		{
			toolTip.RemoveAll();
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Right:
				case Keys.Left:
				case Keys.Up:
				case Keys.Down:
					this.HandleMoveResizeKeystroke(e);
					break;

				case Keys.Space:
					this.ChangeOrientation();
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
						this.Width += 1;
					}
					else
					{
						this.Left += 1;
					}
				}
				else
				{
					this.Left += 5;
				}
			}
			else if (e.KeyCode == Keys.Left)
			{
				if (e.Control)
				{
					if (e.Shift)
					{
						this.Width -= 1;
					}
					else
					{
						this.Left -= 1;
					}
				}
				else
				{
					this.Left -= 5;
				}
			}
			else if (e.KeyCode == Keys.Up)
			{
				if (e.Control)
				{
					if (e.Shift)
					{
						this.Height -= 1;
					}
					else
					{
						this.Top -= 1;
					}
				}
				else
				{
					this.Top -= 5;
				}
			}
			else if (e.KeyCode == Keys.Down)
			{
				if (e.Control)
				{
					if (e.Shift)
					{
						this.Height += 1;
					}
					else
					{
						this.Top += 1;
					}
				}
				else
				{
					this.Top += 5;
				}
			}
		}

		private void HandleResize()
		{
			if (this.IsLocked)
			{
				return;
			}

			switch (this.resizeRegion)
			{
				case ResizeRegion.E:
					{
						int diff = MousePosition.X - this.mouseDownPoint.X;
						Width = this.mouseDownRect.Width + diff;
						break;
					}
				case ResizeRegion.S:
					{
						int diff = MousePosition.Y - this.mouseDownPoint.Y;
						Height = this.mouseDownRect.Height + diff;
						break;
					}
				case ResizeRegion.SE:
					{
						Width = this.mouseDownRect.Width + MousePosition.X - this.mouseDownPoint.X;
						Height = this.mouseDownRect.Height + MousePosition.Y - this.mouseDownPoint.Y;
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
					this.Cursor = Cursors.SizeNS;
					break;

				case ResizeRegion.E:
				case ResizeRegion.W:
					this.Cursor = Cursors.SizeWE;
					break;

				case ResizeRegion.NW:
				case ResizeRegion.SE:
					this.Cursor = Cursors.SizeNWSE;
					break;

				default:
					this.Cursor = Cursors.SizeNESW;
					break;
			}
		}

		private ResizeRegion GetResizeRegion(Point clientCursorPos)
		{
			if (clientCursorPos.Y <= this.resizeBorderWidth)
			{
				if (clientCursorPos.X <= this.resizeBorderWidth) return ResizeRegion.NW;
				else if (clientCursorPos.X >= Width - this.resizeBorderWidth) return ResizeRegion.NE;
				else return ResizeRegion.N;
			}
			else if (clientCursorPos.Y >= Height - this.resizeBorderWidth)
			{
				if (clientCursorPos.X <= this.resizeBorderWidth) return ResizeRegion.SW;
				else if (clientCursorPos.X >= Width - this.resizeBorderWidth) return ResizeRegion.SE;
				else return ResizeRegion.S;
			}
			else
			{
				if (clientCursorPos.X <= this.resizeBorderWidth) return ResizeRegion.W;
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
						this.DrawTickLabel(g, i.ToString(), i, formHeight, tickHeight);
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
			g.DrawString(text, this.Font, Brushes.Black, xPos, height);

			// Bottom
			g.DrawString(text, this.Font, Brushes.Black, xPos, formHeight - height - this.Font.Height);
		}

		private static void Main(params string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			MainForm mainForm;

			if (args.Length == 0)
			{
				mainForm = new MainForm();
			}
			else
			{
				mainForm = new MainForm(RulerInfo.CovertToRulerInfo(args));
			}

			Application.Run(mainForm);
		}

		private void OpacityMenuHandler(object sender, EventArgs e)
		{
			MenuItem mi = (MenuItem)sender;
			this.UncheckMenuItem(mi.Parent);
			mi.Checked = true;
			this.Opacity = double.Parse(mi.Text.Replace("%", "")) / 100;
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
				case "Exit":
					this.Close();
					break;

				case "Tool Tip":
					this.ShowToolTip = !ShowToolTip;
					break;

				case "Vertical":
					this.ChangeOrientation();
					break;

				case "Stay On Top":
					mi.Checked = !mi.Checked;
					this.TopMost = mi.Checked;
					break;

				case "About...":
					string message = string.Format("Original Ruler implemented by Jeff Key\nwww.sliver.com\nruler.codeplex.com\nIcon by Kristen Magee @ www.kbecca.com.\nMaintained by Andrija Cacanovic\nHosted on \nhttps://github.com/andrijac/ruler", Application.ProductVersion);
					MessageBox.Show(message, "About Ruler", MessageBoxButtons.OK, MessageBoxIcon.Information);
					break;

				default:
					MessageBox.Show("Unknown menu item.");
					break;
			}
		}

		private void ChangeOrientation()
		{
			this.IsVertical = !this.IsVertical;
			int width = Width;
			this.Width = Height;
			this.Height = width;
		}
	}
}