using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using Newtonsoft.Json;
using Ruler.Shared.Models;

namespace Ruler.Shared.Services
{
    public class UpdateService
    {
        private bool _isUpdateAvailable;
        private GitHubRelease _latestRelease;
        private string _executablePath;
        // A delegate that the UI project will subscribe to
        public Action OnRequestRestart { get; set; }
        public bool UpdateAvailable
        {
            get
            {
                return _isUpdateAvailable;
            }
        }
        public bool UpdateDownloaded { get; }
        public async Task CheckForUpdates()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            using (var client = new HttpClient())
            {
                // 2. GitHub API MUST have a User-Agent header
                client.DefaultRequestHeaders.Add("User-Agent", "MyUpdaterApp");
                client.DefaultRequestHeaders.Accept.Add(
    new MediaTypeWithQualityHeaderValue("application/json"));

                string apiUrl = "https://api.github.com/repos/andrijac/ruler/releases";

                try
                {
                    // 3. Fetch the JSON string
                    string responseBody = await client.GetStringAsync(apiUrl);

                    // 4. Parse the JSON
                    var releases = JsonConvert.DeserializeObject<List<GitHubRelease>>(responseBody);

                    //_latestRelease = releases; //.FirstOrDefault(r => !r.TagName.Contains("Beta"));
                    //Version currentVersion = Assembly.GetEntryAssembly().GetName().Version;
                    ////if (_latestRelease != null)
                    ////{
                    //Version latestVersion = new Version(_latestRelease.TagName.TrimStart('v'));
                    //if (latestVersion > currentVersion)
                    //{
                    //    _isUpdateAvailable = true;

                    //}

                    ////}
                    ////// Look for a .zip file in the assets list
                    //GitHubAsset asset = new GitHubAsset();
                    //asset.Name = _latestRelease.Assets.FirstOrDefault(a => a.Name.EndsWith(".zip"))?.Name;
                    //asset.DownloadUrl = _latestRelease.Assets.FirstOrDefault(a => a.Name.EndsWith(".zip"))?.DownloadUrl;
                    //_latestRelease.Assets = new List<GitHubAsset> { asset };



                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        public async Task<bool> DownloadUpdateAsync()
        {
            if (_latestRelease != null)
            {

                string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.zip");
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "MyUpdaterApp");
                    using (Stream streamToReadFrom = await client.GetStreamAsync(_latestRelease.Assets.First<GitHubAsset>().DownloadUrl))
                    {
                        // Open a FileStream to write the data to the destination path
                        using (Stream streamToWriteTo = File.Open(localPath, FileMode.Create))
                        {
                            await streamToReadFrom.CopyToAsync(streamToWriteTo);
                        }
                    }
                }
            }
            return true;
        }

        public void InstallUpdate()
        {
            string destinationPath;

            string exePath = Assembly.GetEntryAssembly().Location;
            FileVersionInfo myFileInfo = FileVersionInfo.GetVersionInfo(exePath);

            using (ZipArchive archive = ZipFile.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.zip")))
            {
                archive.ExtractToDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update"));
                destinationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update");
                _executablePath = FindMainExe(destinationPath);
                FileVersionInfo myFileInfo2 = FileVersionInfo.GetVersionInfo(_executablePath);
                if (myFileInfo.InternalName == myFileInfo2.InternalName)
                {
                    LaunchUpdater(_executablePath, exePath);
                }
            }

        }
        public void LaunchUpdater(string newExePath, string oldExePath)
        {
            string fileToDelete = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.zip");
            string folderToDelete = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update");
            // Create a simple batch commands string
            string batchCommands = $@"
@echo off
taskkill /f /im ""{AppDomain.CurrentDomain.FriendlyName}"" > nul 2>&1
timeout /t 2 /nobreak > nul
del /f /q ""{oldExePath}""
move /y ""{newExePath}"" ""{oldExePath}""
start """" ""{oldExePath}""
timeout /t 1 /nobreak > nul

:: Clean up the zip and the extraction folder
if exist ""{fileToDelete}"" del /f /q ""{fileToDelete}""
if exist ""{folderToDelete}"" rd /s /q ""{folderToDelete}""
del ""%~f0"""; // This last line makes the batch file delete itself

            string batchPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.bat");
            File.WriteAllText(batchPath, batchCommands);

            // Start the batch file
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = batchPath,
                WindowStyle = ProcessWindowStyle.Hidden, // Keep it quiet
                UseShellExecute = true
            };

            Process.Start(psi);
            OnRequestRestart.Invoke();
            // Close the current app immediately

        }
        public string FindMainExe(string extractPath)
        {
            // Get all .exe files in the folder (and subfolders)
            var exeFiles = Directory.GetFiles(extractPath, "*.exe", SearchOption.AllDirectories);

            // Filter out common files that aren't your main app
            var mainExe = exeFiles.FirstOrDefault(f =>
                f.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));


            if (mainExe == null)
            {
                throw new FileNotFoundException("Could not find a valid .exe in the update package.");
            }

            return mainExe;
        }

    }
}
