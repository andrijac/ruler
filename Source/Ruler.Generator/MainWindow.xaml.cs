using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell;

using Newtonsoft.Json;

using Ruler.Generator.Models;
using Ruler.Generator.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Forms;

using MessageBox = System.Windows.Forms.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using Path = System.IO.Path;

namespace Ruler.Generator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string CertSubjectName = "CN=Ruler Active Signer";

        public MainWindow()
        {
            InitializeComponent(); 
            TxtOutputPath.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "RulerRelease"); 
        }

        private void BrowseRuler_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*" }; 
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) 
            {
                TxtRulerPath.Text = dlg.FileName;
            }
        }

        private void BrowseUpdater_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*" }; 
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) 
            {
                TxtUpdaterPath.Text = dlg.FileName; 
            }
        }

        private void BrowseOutput_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new FolderBrowserDialog()) 
            {
                dlg.Description = "Select output directory for release package"; 
                dlg.SelectedPath = TxtOutputPath.Text; 
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) 
                {
                    TxtOutputPath.Text = dlg.SelectedPath; 
                }
            }
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string version = TxtVersion.Text.Trim(); 
                string rulerPath = TxtRulerPath.Text.Trim(); 
                string updaterPath = TxtUpdaterPath.Text.Trim(); 
                string outputDir = TxtOutputPath.Text.Trim(); 

                if (string.IsNullOrEmpty(version) || !File.Exists(rulerPath) || !File.Exists(updaterPath) || string.IsNullOrEmpty(outputDir))
                {
                    MessageBox.Show("Please provide a valid version, ensure all executable paths exist, and specify an output directory.", "Validation Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning); 
                    return;
                }

                Directory.CreateDirectory(outputDir); 
                Log("Starting package generation and signing..."); 

                // 1. Create the ZIP archive containing the binaries
                string zipPath = Path.Combine(outputDir, "ruler.zip"); 
                using (FileStream fs = new FileStream(zipPath, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Create))
                    {
                        var rulerEntry = archive.CreateEntry("Ruler.exe");
                        using (var entryStream = rulerEntry.Open())
                        using (var fileStream = new FileStream(rulerPath, FileMode.Open, FileAccess.Read))
                        {
                            fileStream.CopyTo(entryStream);
                        }

                        var updaterEntry = archive.CreateEntry("Ruler.Updater.exe");
                        using (var entryStream = updaterEntry.Open())
                        using (var fileStream = new FileStream(updaterPath, FileMode.Open, FileAccess.Read))
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                }
                Log($"Created ZIP package at: {zipPath}"); 

                // 2. Build Manifest & Individual File Signatures
                string manifestPath = Path.Combine(outputDir, "manifest.json");
                string signedManifestPath = Path.Combine(outputDir, "manifest.json.sig");

                UpdateManifest upman = new UpdateManifest
                {
                    ReleaseNotes = "Standard or test package release.",
                    ReleaseVersion = version,
                    Files = new List<FileSignature>()
                };

                // Populate file signatures for inner files
                upman.Files.Add(CreateFileSignature("Ruler.exe", rulerPath));
                upman.Files.Add(CreateFileSignature("Ruler.Updater.exe", updaterPath));

                // Sign overall ZIP package
                byte[] zipBytes = File.ReadAllBytes(zipPath);
                upman.ZipSignature = GeneratorSecurityService.SignData(zipBytes); 

                // Save initial valid manifest
                string manifestJson = JsonConvert.SerializeObject(upman, Formatting.Indented);
                File.WriteAllText(manifestPath, manifestJson);

                // Sign manifest
                byte[] manifestBytes = Encoding.UTF8.GetBytes(manifestJson);
                File.WriteAllText(signedManifestPath, GeneratorSecurityService.SignData(manifestBytes));
                Log($"Successfully generated and signed manifest at: {manifestPath}");

                // 3. Apply Fault Injection if selected via RadioButtons
                ApplyFaultInjection(outputDir, zipPath, manifestPath, signedManifestPath);

                MessageBox.Show("Release package generated successfully!", "Success", MessageBoxButtons.OKCancel, MessageBoxIcon.Information); 
            }
            catch (Exception ex)
            {
                Log($"ERROR: {ex.Message}"); 
                MessageBox.Show($"An error occurred during generation: {ex.Message}", "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error); 
            }
        }

        private FileSignature CreateFileSignature(string fileName, string filePath)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            string fileHash;
            using (var sha256 = SHA256.Create())
            {
                fileHash = BitConverter.ToString(sha256.ComputeHash(fileBytes)).Replace("-", "").ToLowerInvariant();
            }

            FileVersionInfo fileInfo = FileVersionInfo.GetVersionInfo(filePath);

            return new FileSignature
            {
                FileName = fileName,
                Version = fileInfo.FileVersion ?? "1.0.0.0",
                Hash = fileHash,
                Signature = GeneratorSecurityService.SignData(fileBytes)
            };
        }

        private void ApplyFaultInjection(string outputDir, string zipPath, string manifestPath, string sigPath)
        {
            if (RbValidPackage.IsChecked == true) return;

            Log("Applying selected test fault injection...");

            if (RbFailManifest.IsChecked == true)
            {
                // Tamper JSON content without updating signature
                string json = File.ReadAllText(manifestPath);
                File.WriteAllText(manifestPath, json + "\n// Tampered line");
                Log("Fault applied: Tampered manifest.json.");
            }
            else if (RbFailZip.IsChecked == true)
            {
                // Corrupt zip binary payload
                using (var fs = new FileStream(zipPath, FileMode.Append, FileAccess.Write))
                {
                    byte[] junk = Encoding.UTF8.GetBytes("CORRUPT_ZIP_DATA");
                    fs.Write(junk, 0, junk.Length);
                }
                Log("Fault applied: Corrupted ruler.zip binary.");
            }
            else if (RbFailWpfExe.IsChecked == true || RbFailUpdaterExe.IsChecked == true)
            {
                string targetInnerFile = RbFailWpfExe.IsChecked == true ? "Ruler.exe" : "Ruler.Updater.exe";

                string tempExtract = Path.Combine(outputDir, "temp_fault_extract");
                Directory.CreateDirectory(tempExtract);
                ZipFile.ExtractToDirectory(zipPath, tempExtract);

                string innerFilePath = Path.Combine(tempExtract, targetInnerFile);
                if (File.Exists(innerFilePath))
                {
                    File.AppendAllText(innerFilePath, "MALICIOUS_INJECTION_DATA");
                }

                // Re-pack zip with modified inner file
                File.Delete(zipPath);
                ZipFile.CreateFromDirectory(tempExtract, zipPath);
                if (Directory.Exists(tempExtract)) Directory.Delete(tempExtract, true);

                // Re-sign the outer zip so it passes steps 1-3, ensuring failure hits step 5 (inner file check)
                byte[] newZipBytes = File.ReadAllBytes(zipPath);
                string manifestJson = File.ReadAllText(manifestPath);
                UpdateManifest manifest = JsonConvert.DeserializeObject<UpdateManifest>(manifestJson);
                manifest.ZipSignature = GeneratorSecurityService.SignData(newZipBytes);

                string updatedJson = JsonConvert.SerializeObject(manifest, Formatting.Indented);
                File.WriteAllText(manifestPath, updatedJson);
                File.WriteAllText(sigPath, GeneratorSecurityService.SignData(Encoding.UTF8.GetBytes(updatedJson)));

                Log($"Fault applied: Tampered inner file ({targetInnerFile}).");
            }
        }

        private void Log(string message)
        {
            TxtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}"); 
            TxtLog.ScrollToEnd();
        }
    }
}
