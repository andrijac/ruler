
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Ruler.Shared.Models;

using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;
using System.Security;
using System.Security.Cryptography;
using System.Windows;

namespace Ruler.Shared.Services
{
    public static class UpdateService
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        /// <summary>
        /// Queries the GitHub Releases API to dynamically retrieve download URLs 
        /// for the manifest, signature file, and zip package using GitHubRelease and GitHubAsset models.
        /// </summary>
        public static async Task<UpdatePackageInfo> GetLatestGitHubAssetUrlsAsync(string repoOwner, string repoName)
        {
            string apiUrl = $"https://api.github.com/repos/{repoOwner}/{repoName}/releases/latest";r
            // GitHub API requires a custom User-Agent header or it will return a 403 Forbidden response
            if (!HttpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("RulerUpdaterClient");
            }

            string jsonResponse = await HttpClient.GetStringAsync(apiUrl);
            var release = JsonConvert.DeserializeObject<GitHubRelease>(jsonResponse);

            if (release == null || release.Assets == null)
            {
                throw new InvalidOperationException("Could not retrieve GitHub release assets.");
            }

            UpdatePackageInfo pack = new UpdatePackageInfo();
            foreach (var asset in release.Assets)
            {
                if (asset.Name.Equals("manifest.json", StringComparison.OrdinalIgnoreCase))
                {
                    pack.ManifestUrl = asset.DownloadUrl;
                }
                else if (asset.Name.Equals("manifest.json.sig", StringComparison.OrdinalIgnoreCase))
                {
                    pack.SigUrl = asset.DownloadUrl;
                }
                else if (asset.Name.Equals("ruler.zip", StringComparison.OrdinalIgnoreCase))
                {
                    pack.ZipUrl = asset.DownloadUrl;
                }
            }

            if (string.IsNullOrEmpty(pack.ManifestUrl) || string.IsNullOrEmpty(pack.SigUrl) || string.IsNullOrEmpty(pack.ZipUrl))
            {
                throw new FileNotFoundException("One or more required update assets (manifest.json, sig, zip) were missing from the GitHub release.");
            }

            return pack;
        }
        /// <summary>
        /// Checks the remote manifest to see if a newer version is available compared to the current app version.
        /// </summary>
        public static async Task<(bool UpdateAvailable, string LatestVersion)> CheckForUpdateAsync(string manifestUrl, string currentVersion)
        {
            try
            {
                string jsonResponse = await HttpClient.GetStringAsync(manifestUrl);
                var manifest = JsonConvert.DeserializeObject<UpdateManifest>(jsonResponse);

                if (manifest == null || string.IsNullOrEmpty(manifest.ReleaseVersion))
                {
                    return (false, null);
                }

                Version currentVer = new Version(currentVersion);
                Version remoteVer = new Version(manifest.ReleaseVersion);

                if (remoteVer > currentVer)
                {
                    return (true, manifest.ReleaseVersion);
                }
            }
            catch
            {
                // Suppress network errors during background checks
            }

            return (false, null);
        }
        /// <summary>
        /// Downloads the update artifacts (manifest, detached signature, and zip package) 
        /// and hands them off to SecurityService for cryptographic verification and execution.
        /// </summary>

        public static async Task<string> DownloadAndApplyUpdateAsync(string manifestUrl, string sigUrl, string zipUrl, string targetAppDirectory)

        {
            try
            {
                Directory.CreateDirectory(targetAppDirectory);
                string manifestPath = Path.Combine(targetAppDirectory, UpdateConstants.ManifestFileName);
                string sigPath = Path.Combine(targetAppDirectory, UpdateConstants.ManifestSigFileName);
                string zipPath = Path.Combine(targetAppDirectory, UpdateConstants.ZipFileName);
                // 1. Download manifest.json as raw bytes to prevent text translation line-ending shifts[cite: 2]
                byte[] manifestBytes = await HttpClient.GetByteArrayAsync(manifestUrl);
                await Task.Run(() => File.WriteAllBytes(manifestPath, manifestBytes));

                // 2. Download manifest.json.sig (detached signature)[cite: 2]
                string manifestSig = await HttpClient.GetStringAsync(sigUrl);
                await Task.Run(() => File.WriteAllText(sigPath, manifestSig));

                // 3. Download ruler.zip package bytes[cite: 2]
                byte[] zipBytes = await HttpClient.GetByteArrayAsync(zipUrl);
                await Task.Run(() => File.WriteAllBytes(zipPath, zipBytes));

                // 4. Delegate verification, staging, and updater handoff to SecurityService[cite: 2]
                var success = SecurityService.VerifyAndApplyUpdatePackage(targetAppDirectory, targetAppDirectory);

                if (!success)
                {
                    return $"Failed to apply update: {success}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                CleanupFailedDownloads(targetAppDirectory);
                return $"Error during update process: {ex.Message}";
            }
            return "Update applied successfully";
        }

        private static void CleanupFailedDownloads(string targetDir)
        {
            try
            {
                string manifestPath = Path.Combine(targetDir, "manifest.json");
                string sigPath = Path.Combine(targetDir, "manifest.json.sig");
                string zipPath = Path.Combine(targetDir, "ruler.zip");

                if (File.Exists(manifestPath)) File.Delete(manifestPath);
                if (File.Exists(sigPath)) File.Delete(sigPath);
                if (File.Exists(zipPath)) File.Delete(zipPath);
            }
            catch { }
        }
    }
}
