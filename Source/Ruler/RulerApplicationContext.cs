using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Resources;
using System.Text;
using System.Windows.Forms;

namespace Ruler
{
    class RulerApplicationContext : ApplicationContext
    {
        private NotifyIcon trayIcon;
        private MenuItem autoStartMenuItem;
        private string AutostartRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        public RulerApplicationContext()
        {
            // Initialize Tray Icon
            autoStartMenuItem = new MenuItem("Autostart", toggleAutoStart);
            RegistryKey key = Registry.CurrentUser.OpenSubKey(AutostartRegistryPath, true);
            autoStartMenuItem.Checked = key.GetValue(Application.ProductName) != null;
            key.Close();

            ResourceManager resources = new ResourceManager(typeof(RulerForm));
            var contextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Horizontal Ruler", NewHorizontalRuler),
                new MenuItem("Vertical Ruler", NewVerticalRuler),
                new MenuItem("-"),
                autoStartMenuItem,
                new MenuItem("-"),
                new MenuItem("About", About),
                new MenuItem("Exit", Exit),
            });

            trayIcon = new NotifyIcon()
            {
                Icon = ((Icon)(resources.GetObject("$this.Icon"))),
                ContextMenu = contextMenu
            };

            trayIcon.Click += handleClick;
            trayIcon.Visible = true;
        }
        void toggleAutoStart(object sender, EventArgs e)
        {
            autoStartMenuItem.Checked = !autoStartMenuItem.Checked;
            RegistryKey key = Registry.CurrentUser.OpenSubKey(AutostartRegistryPath, true);
            if (autoStartMenuItem.Checked)
            {
                key.SetValue(Application.ProductName, "\"" + Application.ExecutablePath + "\"");
            }
            else
            {
                key.DeleteValue(Application.ProductName);
            }
            key.Close();
        }
        void About(object sender, EventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo("https://github.com/andrijac/ruler");
            Process.Start(sInfo);
        }
        void handleClick(object sender, EventArgs e)
        {
            var mouseEvent = (MouseEventArgs)e;
            if (mouseEvent.Button == MouseButtons.Left)
            {
                NewHorizontalRuler(sender, e);
            }
            else if (mouseEvent.Button == MouseButtons.Middle)
            {
                NewVerticalRuler(sender, e);
            }
        }
        void NewVerticalRuler(object sender, EventArgs e)
        {
            var info = RulerInfo.GetDefaultRulerInfo();
            info.IsVertical = true;
            var w = info.Width;
            info.Width = 45;
            info.Height = w;
            var ruler = new RulerForm(info);
            ruler.Show();
        }
        void NewHorizontalRuler(object sender, EventArgs e)
        {
            var info = RulerInfo.GetDefaultRulerInfo();
            var ruler = new RulerForm(info);
            ruler.Show();
        }
        void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;

            Application.Exit();
        }
    }
}
