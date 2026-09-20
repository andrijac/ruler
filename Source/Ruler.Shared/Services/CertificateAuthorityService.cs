using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Services
{
    public class CertificateAuthorityService
    {
        /// <summary>
        /// Creates the Master Certificate (Root Authority) and saves its private/public key (.pfx) and public key (.cer) to disk.
        /// </summary>
        public static Tuple<X509Certificate2, string> CreateAndSaveMasterCertificate(string outputDirectory, string exportPassword)
        {
            Directory.CreateDirectory(outputDirectory);

            using (var rsa = RSA.Create(4096)) // Traditional using block for .NET Framework compatibility
            {
                var request = new CertificateRequest(
                    "CN=Ruler Master Signing Authority",
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                // Set constraints identifying this as a Certificate Authority (CA)
                request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, true, 0, true));
                request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign | X509KeyUsageFlags.DigitalSignature, true));

                using (var cert = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(20)))
                {
                    // Export Private + Public Key (.pfx)
                    byte[] pfxBytes = cert.Export(X509ContentType.Pfx, exportPassword);
                    string pfxPath = Path.Combine(outputDirectory, "master.pfx");
                    File.WriteAllBytes(pfxPath, pfxBytes);

                    // Export Public Key only (.cer)
                    byte[] cerBytes = cert.Export(X509ContentType.Cert);
                    File.WriteAllBytes(Path.Combine(outputDirectory, "master.cer"), cerBytes);

                    return new Tuple<X509Certificate2, string>(cert, pfxPath);
                }
            }
        }

        /// <summary>
        /// Creates the Active Update Certificate, signs it using the Master Certificate, and saves it to disk.
        /// </summary>
        public static Tuple<X509Certificate2, string> CreateAndSaveActiveCertificate(X509Certificate2 masterCert, string outputDirectory, string exportPassword)
        {
            Directory.CreateDirectory(outputDirectory);

            using (var rsa = RSA.Create(2048))
            {
                var request = new CertificateRequest(
                    "CN=Ruler Active Release Signer",
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation, true));
                request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.3") }, true)); // Code Signing EKU

                // Generate serial number and sign the active certificate request using the Master certificate's private key
                byte[] serialNumber = Guid.NewGuid().ToByteArray();
                using (var issuedCert = request.Create(
                    masterCert,
                    DateTimeOffset.UtcNow.AddDays(-1),
                    DateTimeOffset.UtcNow.AddYears(2),
                    serialNumber))
                {
                    // Bind the private key back to the issued certificate
                    using (var certWithPrivateKey = issuedCert.CopyWithPrivateKey(rsa))
                    {
                        // Export Private + Public Key (.pfx)
                        byte[] pfxBytes = certWithPrivateKey.Export(X509ContentType.Pfx, exportPassword);
                        string pfxPath = Path.Combine(outputDirectory, "active.pfx");
                        File.WriteAllBytes(pfxPath, pfxBytes);

                        // Export Public Key only (.cer)
                        byte[] cerBytes = certWithPrivateKey.Export(X509ContentType.Cert);
                        File.WriteAllBytes(Path.Combine(outputDirectory, "active.cer"), cerBytes);

                        return new Tuple<X509Certificate2, string>(certWithPrivateKey, pfxPath);
                    }
                }
            }
        }
    }
}
