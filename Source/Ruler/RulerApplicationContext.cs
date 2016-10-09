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
    public class RulerApplicationContext : ApplicationContext
    {
        private string AutostartRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private NotifyIcon _trayIcon;
        private MenuItem _autoStartMenuItem;
        private Config _config;
        public RulerApplicationContext()
        {
            // Initialize Tray Icon
            _autoStartMenuItem = new MenuItem("Autostart", toggleAutoStart);
            RegistryKey key = Registry.CurrentUser.OpenSubKey(AutostartRegistryPath, true);
            _autoStartMenuItem.Checked = key.GetValue(Application.ProductName) != null;
            key.Close();

            ResourceManager resources = new ResourceManager(typeof(RulerForm));
            var contextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Horizontal Ruler", NewHorizontalRuler),
                new MenuItem("Vertical Ruler", NewVerticalRuler),
                new MenuItem("-"),
                _autoStartMenuItem,
                new MenuItem("-"),
                new MenuItem("About", About),
                new MenuItem("Exit", Exit),
            });

            _trayIcon = new NotifyIcon()
            {
                Icon = ((Icon)(resources.GetObject("$this.Icon"))),
                ContextMenu = contextMenu
            };

            _trayIcon.Click += handleClick;
            _trayIcon.Visible = true;

            // load config
            _config = Config.Load();
        }
        void toggleAutoStart(object sender, EventArgs e)
        {
            _autoStartMenuItem.Checked = !_autoStartMenuItem.Checked;
            RegistryKey key = Registry.CurrentUser.OpenSubKey(AutostartRegistryPath, true);
            if (_autoStartMenuItem.Checked)
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
            var ruler = new RulerForm(this, _config.VerticalRulerInfo);
            ruler.Show();
        }
        void NewHorizontalRuler(object sender, EventArgs e)
        {
            var ruler = new RulerForm(this, _config.HorizontalRulerInfo);
            ruler.Show();
        }
        void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            _trayIcon.Visible = false;

            Application.Exit();
        }
        public void SaveConfig()
        {
            Config.Save(_config);
        }
    }
}
