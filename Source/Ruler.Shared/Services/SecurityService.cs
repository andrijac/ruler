using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Ruler.Shared.Models;

namespace Ruler.Shared.Services
{
    public static class SecurityService
    {
        private const string ActivePublicKeyXml = "<RSAKeyValue><Modulus>wY2zbyvfNbH3kY0EH0arMuveaPuQst4Kta5otX2UZFTAbeILwEumMSM6vJ7Tpa8uC1BB4Vh5zdIF4MsPZfVWFgoyWEa/NHPobcgUw8CUBCj1aQ/UDoumIgP0VHHgcwfy0NmiS7ANSpU0xxqQlIm5d6JeR769FZ1kG881GwosITk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        private const string MasterPublicKeyXml = "<RSAKeyValue><Modulus>3xkaPvlFt7UlsQzCWV5KfrGf+KtaxGaeJt+pINSMtfnPFmZ0r1LJJxK8kegGXTJB3ebR0Ppg5ssZ42e9oWGUNzk6UFsKHD1SnIDVIu/3G4MLqRMwoTPJ4dmVtd7KzvKYrw5AkTquKzOzDgqAjtd3CbmHlimJdxFyiPsbMZ0JQHk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        public static bool VerifyData(byte[] dataBytes, string base64Signature)
        {
            if (string.IsNullOrEmpty(base64Signature)) return false;

            try
            {
                byte[] signatureBytes = Convert.FromBase64String(base64Signature);
                using (var rsa = RSA.Create())
                {
                    try
                    {
                        rsa.FromXmlString(ActivePublicKeyXml);
                        if (rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        // Fall through
                    }

                    try
                    {
                        rsa.FromXmlString(MasterPublicKeyXml);
                        return rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool VerifyFileHash(string filePath, string expectedHash)
        {

            if (!File.Exists(filePath)) return false;

            try
            {
                using (var sha256 = SHA256.Create())
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = sha256.ComputeHash(stream);
                    string actualHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                    return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool VerifyFileSignature(string filePath, FileSignature fileSig)
        {
            if (fileSig == null || !File.Exists(filePath)) return false;


            if (!VerifyFileHash(filePath, fileSig.Hash))
            {
                return false;
            }

            byte[] fileBytes = File.ReadAllBytes(filePath);
            return VerifyData(fileBytes, fileSig.Signature);
        }

        /// <summary>
        /// Verifies the detached manifest signature, package-level zip signature, and all individual inner file signatures.
        /// If valid, deploys the new updater, launches it to update the running WPF application, and exits[cite: 6].
        /// </summary>
        public static bool VerifyAndApplyUpdatePackage(string packageDirectory, string targetInstallDirectory)
        {
            string manifestPath = Path.Combine(packageDirectory, UpdateConstants.ManifestFileName);
            string sigPath = Path.Combine(packageDirectory, UpdateConstants.ManifestSigFileName);
            string zipPath = Path.Combine(packageDirectory, UpdateConstants.ZipFileName);

            void CleanupFailedPackage()
            {
                DeleteFileIfExists(manifestPath);
                DeleteFileIfExists(sigPath);
                DeleteFileIfExists(zipPath);
            }

            if (!File.Exists(manifestPath) || !File.Exists(sigPath) || !File.Exists(zipPath))
            {

                return false;
            }

            // 1. Read raw manifest bytes directly to preserve exact line-ending layout for signature verification
            byte[] manifestBytes = File.ReadAllBytes(manifestPath);
            string manifestSig = File.ReadAllText(sigPath).Trim();

            if (!VerifyData(manifestBytes, manifestSig))
            {
                CleanupFailedPackage();
                return false;
            }

            // 2. Deserialize UpdateManifest
            string manifestJson = Encoding.UTF8.GetString(manifestBytes);
            UpdateManifest manifest = JsonConvert.DeserializeObject<UpdateManifest>(manifestJson);
            if (manifest == null) return false;

            // 3. Verify overall Zip package signature
            byte[] zipBytes = File.ReadAllBytes(zipPath);
            if (!VerifyData(zipBytes, manifest.ZipSignature))
            {
                CleanupFailedPackage();
                PurgeTempUpdateZips();
                return false;
            }

            // 4. Extract zip to temporary directory for inner file checks
            string tempExtractPath = Path.Combine(packageDirectory, "temp_extracted_" + Guid.NewGuid().ToString());

            try
            {
                Directory.CreateDirectory(tempExtractPath);
                ZipFile.ExtractToDirectory(zipPath, tempExtractPath);

                string fullExePath = Process.GetCurrentProcess().MainModule.FileName;
                string currentExeName = Path.GetFileName(fullExePath);

                string sourceUpdaterPath = string.Empty;
                string sourceUpdaterConfigPath = string.Empty;
                string sourceWpfPath = string.Empty;
                string sourceWpfConfigPath = string.Empty;
                // 5. Verify individual file signatures
                foreach (var fileSig in manifest.Files)
                {
                    string targetFilePath = Path.Combine(tempExtractPath, fileSig.FileName);
                    if (!VerifyFileSignature(targetFilePath, fileSig))
                    {
                        CleanupFailedPackage();
                        DeleteFolderIfExists(tempExtractPath);
                        PurgeTempUpdateZips();
                        return false;
                    }

                    if (fileSig.FileName.Equals("ruler.updater.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        sourceUpdaterPath = targetFilePath;
                    }
                    if (fileSig.FileName.Equals("ruler.updater.exe.config", StringComparison.OrdinalIgnoreCase))
                    {
                        sourceUpdaterConfigPath = targetFilePath;
                    }
                    if (fileSig.FileName.Equals(currentExeName, StringComparison.OrdinalIgnoreCase))
                    {
                        sourceWpfPath = targetFilePath;
                    }
                    if (fileSig.FileName.Equals(currentExeName + ".config", StringComparison.OrdinalIgnoreCase))
                    {

                        sourceWpfConfigPath = targetFilePath;
                    }
                }


                if (string.IsNullOrEmpty(sourceUpdaterPath) || string.IsNullOrEmpty(sourceWpfPath) || string.IsNullOrEmpty(sourceUpdaterConfigPath) || string.IsNullOrEmpty(sourceWpfConfigPath))
                {
                    return false;
                }

                // 6. Stage the new updater
                Directory.CreateDirectory(targetInstallDirectory);
                string targetUpdaterPath = Path.Combine(targetInstallDirectory, "ruler.updater.exe");
                string targetUpdaterConfigPath = Path.Combine(targetInstallDirectory, "ruler.updater.exe.config");

                File.Copy(sourceUpdaterPath, targetUpdaterPath, overwrite: true);
                File.Copy(sourceUpdaterConfigPath, targetUpdaterConfigPath, overwrite: true);

                if (!File.Exists(targetUpdaterPath))
                {
                    return false;
                }


                FileInfo sourceInfo = new FileInfo(sourceUpdaterPath);
                FileInfo targetInfo = new FileInfo(targetUpdaterPath);
                if (sourceInfo.Length != targetInfo.Length)
                {
                    return false;
                }

                // 7. Launch updater and exit
                string arguments = $"\"{sourceWpfPath}\" \"{sourceWpfConfigPath}\" \"{targetInstallDirectory}\" \"{currentExeName}\"";

                Process.Start(new ProcessStartInfo
                {
                    FileName = targetUpdaterPath,
                    Arguments = arguments,
                    UseShellExecute = true
                });

                Environment.Exit(0);
                return true;

            }
            catch
            {
                return false;
            }
            finally
            {
                CleanupFailedPackage();
                DeleteFolderIfExists(tempExtractPath);
                PurgeTempUpdateZips();
            }

        }

        private static void DeleteFileIfExists(string path)
        {
            if (File.Exists(path))
            {
                try { File.Delete(path); } catch { }
            }
        }

        private static void DeleteFolderIfExists(string path)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch { }
            }
        }
        private static void PurgeTempUpdateZips()
        {
            try
            {
                string tempPath = Path.GetTempPath();
                string searchPattern = $"*{UpdateConstants.ZipFileName}";
                string[] leftoverZips = Directory.GetFiles(tempPath, searchPattern);
                foreach (string zip in leftoverZips)
                {

                    try { File.Delete(zip); } catch { }
                }
            }
            catch { }
        }
    }
}
