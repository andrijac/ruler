using Ruler.Shared.Services;
using Ruler.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for UpdateScreen.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
#if DEBUG
        private string _owner = "IsaacMorris1980";
#else
    private string _owner = "andrijac";
#endif
        private string _repo = "ruler";
        private UpdatePackageInfo _packageInfo;
        public UpdateWindow()
        {
            InitializeComponent();
        }

        private async void BtnCheck_Click(object sender, RoutedEventArgs e)
        {
            _packageInfo = await UpdateService.GetLatestGitHubAssetUrlsAsync(_owner, _repo);
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            // Format as string (e.g., "1.0.0.0" or fallback to default if null)
            string currentVersion = version?.ToString() ?? "1.0.0.0";
            var updateInfos = await UpdateService.CheckForUpdateAsync(_packageInfo.ManifestUrl, currentVersion);
            if (updateInfos.UpdateAvailable)
            {
                TxtVersionInfo.Text = $"Update available: {updateInfos.LatestVersion}  Current: {currentVersion}";
            }
            else
            {
                TxtVersionInfo.Text = $"No update available. Current version: {currentVersion}";
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            string result = await UpdateService.DownloadAndApplyUpdateAsync(_packageInfo.ManifestUrl, _packageInfo.SigUrl, _packageInfo.ZipUrl, AppDomain.CurrentDomain.BaseDirectory);
            TxtReleaseNotes.Text = result;
        }
    }
}
