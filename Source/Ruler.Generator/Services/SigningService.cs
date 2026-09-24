using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Generator.Services
{
    public static class SigningService
    {
        /// <summary>
        /// Signs raw data bytes using an X509Certificate2 instance containing the private key.
        /// </summary>
        public static string SignUpdatePackage(byte[] data, X509Certificate2 signingCert)
        {
            if (data == null || data.Length == 0)
            {
                throw new ArgumentException("Data to sign cannot be null or empty.", nameof(data));
            }

            if (signingCert == null || !signingCert.HasPrivateKey)
            {
                throw new ArgumentException("A valid certificate containing a private key is required for signing.", nameof(signingCert));
            }

            using (var rsa = signingCert.GetRSAPrivateKey())
            {
                if (rsa == null)
                {
                    throw new InvalidOperationException("Could not extract the RSA private key from the specified certificate.");
                }
                byte[] signatureBytes = rsa.SignData(data, HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1);
                return Convert.ToBase64String(signatureBytes);
            }
        }
    }
}
