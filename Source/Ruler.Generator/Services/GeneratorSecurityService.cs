using Newtonsoft.Json;

using Ruler.Generator.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Formatting = Newtonsoft.Json.Formatting;

namespace Ruler.Generator.Services
{
    public static class GeneratorSecurityService
    {
        private const string ActiveCertSubject = "CN=Ruler Active Signer";
        private const string MasterCertSubject = "RulerMaster";

        private static X509Certificate2 GetSigningCertificate()
        {
            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);
                DateTime now = DateTime.Now;

                // 1. Try finding and validating the Active certificate
                var activeCerts = store.Certificates.Find(X509FindType.FindBySubjectName, ActiveCertSubject, false);
                if (activeCerts.Count > 0)
                {
                    var activeCert = activeCerts.Cast<X509Certificate2>().OrderByDescending(c => c.NotBefore).First();

                    bool isValidTime = now >= activeCert.NotBefore && now <= activeCert.NotAfter;
                    if (isValidTime && activeCert.HasPrivateKey)
                    {
                        return activeCert;
                    }

                    activeCert.Dispose();
                }

                // 2. Fall back to the Master certificate
                var masterCerts = store.Certificates.Find(X509FindType.FindBySubjectName, MasterCertSubject, false);
                if (masterCerts.Count == 0)
                {
                    throw new CryptographicException($"Active certificate '{ActiveCertSubject}' is invalid/expired, and no backup Master certificate ('{MasterCertSubject}') was found.");
                }

                var masterCert = masterCerts.Cast<X509Certificate2>().OrderByDescending(c => c.NotBefore).First();

                if (now < masterCert.NotBefore || now > masterCert.NotAfter)
                {
                    throw new CryptographicException($"The backup Master certificate ('{MasterCertSubject}') is outside its validity period.");
                }

                if (!masterCert.HasPrivateKey)
                {
                    throw new InvalidOperationException($"The backup Master certificate ('{MasterCertSubject}') does not contain a private key.");
                }

                return masterCert;
            }
        }

        public static string SignData(byte[] dataBytes)
        {
            using (var cert = GetSigningCertificate())
            using (var rsa = cert.GetRSAPrivateKey())
            {
                if (rsa == null)
                {
                    throw new InvalidOperationException("Could not extract the RSA private key from the certificate.");
                }

                byte[] signatureBytes = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                return Convert.ToBase64String(signatureBytes);
            }
        }

        private static string GetFileHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hashBytes = sha256.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// Helper method that builds and signs a FileSignature[cite: 1] object for an individual file.
        /// </summary>
        private static FileSignature CreateFileSignature(string filePath, string targetFileName, string fallbackVersion)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found for signature creation: {filePath}");
            }

            string version = FileVersionInfo.GetVersionInfo(filePath).FileVersion ?? fallbackVersion;
            string hash = GetFileHash(filePath);
            byte[] fileBytes = File.ReadAllBytes(filePath);
            string signature = SignData(fileBytes);

            return new FileSignature
            {
                FileName = targetFileName,
                Version = version,
                Hash = hash,
                Signature = signature
            };
        }

        public static void GenerateAndSignPackage(
            string releaseVersion,
            string wpfExePath,
            string updaterPath,
            string outputDirectory,
            string releaseNotes)
        {
            if (!File.Exists(wpfExePath) || !File.Exists(updaterPath))
            {
                throw new FileNotFoundException("One or more executable files could not be found.");
            }

            Directory.CreateDirectory(outputDirectory);

            // 1. Create the ZIP package
            string zipPath = Path.Combine(outputDirectory, "ruler.zip");
            using (FileStream fs = new FileStream(zipPath, FileMode.Create))
            {
                using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Create))
                {
                    var wpfEntry = archive.CreateEntry("ruler.wpf.exe");
                    using (var entryStream = wpfEntry.Open())
                    using (var fileStream = new FileStream(wpfExePath, FileMode.Open, FileAccess.Read))
                    {
                        fileStream.CopyTo(entryStream);
                    }

                    var updaterEntry = archive.CreateEntry("ruler.updater.exe");
                    using (var entryStream = updaterEntry.Open())
                    using (var fileStream = new FileStream(updaterPath, FileMode.Open, FileAccess.Read))
                    {
                        fileStream.CopyTo(entryStream);
                    }
                }
            }

            // 2. Initialize the UpdateManifest model[cite: 2]
            var manifest = new UpdateManifest
            {
                ReleaseVersion = releaseVersion,
                ReleaseNotes = releaseNotes,
                Files = new List<FileSignature>()
            };

            // 3. Create file signatures using the helper method[cite: 1]
            manifest.Files.Add(CreateFileSignature(wpfExePath, "ruler.wpf.exe", releaseVersion));
            manifest.Files.Add(CreateFileSignature(updaterPath, "ruler.updater.exe", releaseVersion));

            // 4. Sign the entire .zip package[cite: 2]
            byte[] zipBytes = File.ReadAllBytes(zipPath);
            manifest.ZipSignature = SignData(zipBytes);

            // 5. Save manifest.json to disk
            string manifestPath = Path.Combine(outputDirectory, "manifest.json");
            string manifestJson = JsonConvert.SerializeObject(manifest, Formatting.Indented);
            File.WriteAllText(manifestPath, manifestJson);

            // 6. Generate and save the detached manifest.json.sig file
            byte[] manifestBytes = Encoding.UTF8.GetBytes(manifestJson);
            string manifestSignature = SignData(manifestBytes);
            string sigPath = Path.Combine(outputDirectory, "manifest.json.sig");
            File.WriteAllText(sigPath, manifestSignature);
        }
    }
}
