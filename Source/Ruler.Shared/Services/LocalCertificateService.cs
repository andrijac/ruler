using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Services
{
    public class LocalCertificateService
    {
        private const string DefaultLocalCertSubject = "RulerLocalDataSigner";

        /// <summary>
        /// Ensures the local machine data persistence certificate exists. 
        /// Installs it into the LocalMachine store if missing, and returns the Subject Name to save in application settings.
        /// </summary>
        public static string GetOrInstallLocalCertificate()
        {
            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadWrite);

                var existingCerts = store.Certificates.Find(X509FindType.FindBySubjectName, DefaultLocalCertSubject, false);

                if (existingCerts.Count > 0)
                {
                    // Certificate already installed; return its settings key reference
                    return DefaultLocalCertSubject;
                }

                // Create a new self-signed persistence certificate if not found
                using (var rsa = RSA.Create(2048))
                {
                    var request = new CertificateRequest(
                        "CN=" + DefaultLocalCertSubject,
                        rsa,
                        HashAlgorithmName.SHA256,
                        RSASignaturePadding.Pkcs1);

                    request.CertificateExtensions.Add(
                        new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.DataEncipherment, true));

                    using (var cert = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(10)))
                    {
                        // Export to PFX and re-import with MachineKeySet flag to securely persist private keys in the LocalMachine store
                        byte[] pfxBytes = cert.Export(X509ContentType.Pfx, string.Empty);
                        using (var persistentCert = new X509Certificate2(pfxBytes, string.Empty, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet))
                        {
                            store.Add(persistentCert);
                        }
                    }
                }

                return DefaultLocalCertSubject;
            }
        }
    }
}
