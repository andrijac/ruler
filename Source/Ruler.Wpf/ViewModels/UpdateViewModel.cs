using Ruler.Shared.Commands;
using Ruler.Shared.Models;
using Ruler.Shared.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ruler.Wpf.ViewModels
{
    public class UpdateViewModel : ViewModelBase
    {
#if DEBUG
        private readonly string _owner = "IsaacMorris1980";
#else
        private readonly string _owner = "andrijac";
#endif
        private readonly string _repo = "ruler";

        private UpdatePackageInfo _packageInfo;

        private string _versionInfoText = "Click 'Check for Updates' to look for new versions.";
        public string VersionInfoText
        {
            get => _versionInfoText;
            set => SetProperty(ref _versionInfoText, value);
        }

        private string _releaseNotesText = "No update details loaded yet.";
        public string ReleaseNotesText
        {
            get => _releaseNotesText;
            set => SetProperty(ref _releaseNotesText, value);
        }

        private string _statusText = string.Empty;
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        private bool _isBusy = false;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    OnPropertyChanged(nameof(CanExecuteCheck));
                    OnPropertyChanged(nameof(CanExecuteUpdate));
                }
            }
        }

        private bool _isUpdateAvailable = false;
        public bool IsUpdateAvailable
        {
            get => _isUpdateAvailable;
            set
            {
                if (SetProperty(ref _isUpdateAvailable, value))
                {
                    OnPropertyChanged(nameof(CanExecuteUpdate));
                }
            }
        }

        public bool CanExecuteCheck => !IsBusy;
        public bool CanExecuteUpdate => !IsBusy && IsUpdateAvailable;

        public Action RequestClose { get; set; }

        public ICommand CheckForUpdatesCommand { get; private set; }
        public ICommand DownloadAndInstallCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public UpdateViewModel()
        {
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            CheckForUpdatesCommand = new DelegateCommand(async _ => await CheckForUpdatesAsync(), _ => CanExecuteCheck);
            DownloadAndInstallCommand = new DelegateCommand(async _ => await DownloadAndInstallAsync(), _ => CanExecuteUpdate);
            CancelCommand = new DelegateCommand(_ => RequestClose?.Invoke());
        }

        public async Task CheckForUpdatesAsync()
        {
            if (IsBusy) return;

            // Fast offline pre-check
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                IsUpdateAvailable = false;
                VersionInfoText = "No internet connection.";
                ReleaseNotesText = "Please connect to the internet to check for updates.";
                StatusText = "Offline.";
                return;
            }

            IsBusy = true;
            StatusText = "Checking for updates...";
            VersionInfoText = "Checking version details...";

            try
            {
                _packageInfo = await UpdateService.GetLatestGitHubAssetUrlsAsync(_owner, _repo);
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                string currentVersion = version?.ToString() ?? "1.0.0.0";

                var updateInfos = await UpdateService.CheckForUpdateAsync(_packageInfo.ManifestUrl, currentVersion);

                if (updateInfos.UpdateAvailable)
                {
                    IsUpdateAvailable = true;
                    VersionInfoText = $"Update available: {updateInfos.LatestVersion} (Current: {currentVersion})";
                    ReleaseNotesText = "Download and install latest version";
                }
                else
                {
                    IsUpdateAvailable = false;
                    VersionInfoText = $"No update available. Current version: {currentVersion}";
                    ReleaseNotesText = "You are currently running the latest version.";
                }

                StatusText = string.Empty;
            }
            catch (HttpRequestException)
            {
                IsUpdateAvailable = false;
                VersionInfoText = "Network error occurred.";
                ReleaseNotesText = "Unable to connect to the update server. Please check your internet connection.";
                StatusText = "Offline or unreachable.";
            }
            catch (TaskCanceledException)
            {
                IsUpdateAvailable = false;
                VersionInfoText = "Update check timed out.";
                ReleaseNotesText = "The connection took too long to respond.";
                StatusText = "Request timed out.";
            }
            catch (Exception ex)
            {
                IsUpdateAvailable = false;
                VersionInfoText = "Failed to check for updates.";
                ReleaseNotesText = $"Error: {ex.Message}";
                StatusText = "Check failed.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DownloadAndInstallAsync()
        {
            if (_packageInfo == null || IsBusy) return;

            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                ReleaseNotesText = "No internet connection. Cannot download update.";
                StatusText = "Offline.";
                return;
            }

            IsBusy = true;
            StatusText = "Downloading and applying update...";

            try
            {
                string result = await UpdateService.DownloadAndApplyUpdateAsync(
                    _packageInfo.ManifestUrl,
                    _packageInfo.SigUrl,
                    _packageInfo.ZipUrl,
                    AppDomain.CurrentDomain.BaseDirectory);

                ReleaseNotesText = result;
                StatusText = "Update applied successfully.";
            }
            catch (HttpRequestException)
            {
                ReleaseNotesText = "Network error while downloading the update package. Please check your connection.";
                StatusText = "Download failed.";
            }
            catch (TaskCanceledException)
            {
                ReleaseNotesText = "The download request took too long and timed out.";
                StatusText = "Download timed out.";
            }
            catch (Exception ex)
            {
                ReleaseNotesText = $"Update installation failed: {ex.Message}";
                StatusText = "Error during installation.";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
