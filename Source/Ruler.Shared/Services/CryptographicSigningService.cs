using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.Pkcs;

namespace Ruler.Shared.Services
{
    public class CryptographicSigningService
    {
        /// <summary>
        /// Retrieves a certificate from the specified Windows certificate store by its Subject Name.
        /// </summary>
        private static X509Certificate2 GetCertificate(string subjectName, StoreLocation location, StoreName name)
        {
            using (var store = new X509Store(name, location))
            {
                store.Open(OpenFlags.ReadOnly);
                var certs = store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, false);

                if (certs.Count == 0)
                {
                    throw new InvalidOperationException("Certificate with subject name '" + subjectName + "' was not found in " + location + "/" + name + ".");
                }

                // Return a copy so the underlying store can be safely closed/disposed
                return new X509Certificate2(certs[0]);
            }
        }

        /// <summary>
        /// Computes a detached CMS/PKCS#7 digital signature for a byte array using the specified signing certificate.
        /// </summary>
        public static byte[] SignData(byte[] data, string certSubjectName, StoreLocation storeLocation = StoreLocation.LocalMachine, StoreName storeName = StoreName.My)
        {
            using (var cert = GetCertificate(certSubjectName, storeLocation, storeName))
            {
                var contentInfo = new ContentInfo(data);
                var signedCms = new SignedCms(contentInfo, detached: true);
                var signer = new CmsSigner(cert);

                signedCms.ComputeSignature(signer);
                return signedCms.Encode();
            }
        }

        /// <summary>
        /// Verifies a detached CMS/PKCS#7 signature against a byte array and a trusted certificate.
        /// </summary>
        public static bool VerifyData(byte[] data, byte[] signatureBytes, string trustedCertSubjectName, StoreLocation storeLocation = StoreLocation.LocalMachine, StoreName storeName = StoreName.My)
        {
            try
            {
                var contentInfo = new ContentInfo(data);
                var signedCms = new SignedCms(contentInfo, detached: true);
                signedCms.Decode(signatureBytes);

                using (var trustedCert = GetCertificate(trustedCertSubjectName, storeLocation, storeName))
                {
                    // Verify signature only (allows self-signed certificates without forcing public root CA checks)
                    signedCms.CheckSignature(verifySignatureOnly: true);

                    foreach (SignerInfo signer in signedCms.SignerInfos)
                    {
                        if (signer.Certificate.Thumbprint.Equals(trustedCert.Thumbprint, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (CryptographicException)
            {
                return false; // Signature corruption or mismatch
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Signs a physical file and writes a companion .sig file containing the detached signature.
        /// </summary>
        public static void SignFile(string filePath, string certSubjectName, StoreLocation storeLocation = StoreLocation.LocalMachine, StoreName storeName = StoreName.My)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File to sign not found.", filePath);
            }

            byte[] fileBytes = File.ReadAllBytes(filePath);
            byte[] signatureBytes = SignData(fileBytes, certSubjectName, storeLocation, storeName);

            File.WriteAllBytes(filePath + ".sig", signatureBytes);
        }

        /// <summary>
        /// Verifies a physical file using its companion .sig file against a trusted certificate.
        /// </summary>
        public static bool VerifyFile(string filePath, string trustedCertSubjectName, StoreLocation storeLocation = StoreLocation.LocalMachine, StoreName storeName = StoreName.My)
        {
            string sigPath = filePath + ".sig";
            if (!File.Exists(filePath) || !File.Exists(sigPath))
            {
                return false;
            }

            byte[] fileBytes = File.ReadAllBytes(filePath);
            byte[] signatureBytes = File.ReadAllBytes(sigPath);

            return VerifyData(fileBytes, signatureBytes, trustedCertSubjectName, storeLocation, storeName);
        }
    }
}
