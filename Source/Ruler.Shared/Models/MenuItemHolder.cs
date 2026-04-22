using System;
using System.Windows.Forms;
using Ruler.Shared.Enums;
namespace Ruler.Shared.Models
{
	public class MenuItemHolder
	{
		public MenuItemHolder(MenuItemEnum menuItemEnum, string title, EventHandler handler, bool isChecked)
		{
			this.MenuItemEnum = menuItemEnum;
			this.MenuItem = new MenuItem(title, handler, Shortcut.None);
			this.MenuItem.Checked = isChecked;
		}

		public static MenuItemHolder Separator
		{
			get
			{
				return new MenuItemHolder(MenuItemEnum.Separator, "-", null, false);
			}
		}

		public MenuItemEnum MenuItemEnum
		{
			get;
			set;
		}

		public MenuItem MenuItem
		{
			get;
			set;
		}
	}
}