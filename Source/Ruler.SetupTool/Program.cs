using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.SetupTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Running Ruler Security Setup...");

            try
            {
                string masterPassword = "SuperSecureMasterPassword123!";
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                // Example usage:
                string masterPfxPath = Path.Combine(desktopPath, "MasterPfx.pfx");
                

                // 1. Create Master Certificate (99 Years) -> Export PFX to Hard Drive
                string masterPublicKeyXml;
                using (RSA rsaMaster = RSA.Create(4096))
                {
                    var masterReq = new CertificateRequest("CN=Ruler Master Root", rsaMaster, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    masterReq.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.KeyEncipherment, true));

                    using (X509Certificate2 masterCert = masterReq.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(99)))
                    {
                        byte[] pfxBytes = masterCert.Export(X509ContentType.Pfx, masterPassword);
                        File.WriteAllBytes(masterPfxPath, pfxBytes);

                        using (var pubRsa = masterCert.GetRSAPublicKey())
                        {
                            masterPublicKeyXml = pubRsa.ToXmlString(false);
                        }
                    }
                }

                // 2. Create Active Certificate (2 Years) -> Windows LocalMachine Store
                string activePublicKeyXml;
                using (RSA rsaActive = RSA.Create(2048))
                {
                    var activeReq = new CertificateRequest("CN=Ruler Active Signer", rsaActive, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    activeReq.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, false));

                    using (X509Certificate2 activeCert = activeReq.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(2)))
                    {
                        using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                        {
                            store.Open(OpenFlags.ReadWrite);
                            store.Add(activeCert);
                        }

                        // Save thumbprint for your build tool
                        File.WriteAllText("ruler_active_thumbprint.dat", activeCert.Thumbprint);

                        using (var pubRsa = activeCert.GetRSAPublicKey())
                        {
                            activePublicKeyXml = pubRsa.ToXmlString(false);
                        }
                    }
                }

                // 3. Create Local App Data Certificate (5 Years) -> Windows CurrentUser Store
                using (RSA rsaLocal = RSA.Create(2048))
                {
                    var localReq = new CertificateRequest("CN=Ruler Local Data Signer", rsaLocal, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    localReq.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));

                    using (X509Certificate2 localCert = localReq.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(5)))
                    {
                        using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                        {
                            store.Open(OpenFlags.ReadWrite);
                            store.Add(localCert);
                        }

                        // Save thumbprint for the portable app folder
                        File.WriteAllText("ruler_local_thumbprint.dat", localCert.Thumbprint);
                    }
                }

                Console.WriteLine("\n--- SETUP COMPLETE ---");
                Console.WriteLine("Master PFX saved to cold storage.");
                Console.WriteLine("Thumbprints written to local dat files.");
                Console.WriteLine("\nHardcode these public keys into your client-side SecurityService.cs:\n");
                Console.WriteLine($"private const string MasterPublicKeyXml = \"{masterPublicKeyXml}\";");
                Console.WriteLine($"private const string ActivePublicKeyXml = \"{activePublicKeyXml}\";");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }
    }
}
