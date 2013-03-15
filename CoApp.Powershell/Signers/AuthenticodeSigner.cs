using System.Security.Cryptography.X509Certificates;
using ClrPlus.Windows.PeBinary.Utility;

namespace ClrPlus.Signing.Signers
{
    

    public class AuthenticodeSigner
    {
        private
        const BinaryLoadOptions BINARY_LOAD_OPTIONS = BinaryLoadOptions.PEInfo |
            BinaryLoadOptions.VersionInfo |
            BinaryLoadOptions.Managed |
            BinaryLoadOptions.Resources |
            BinaryLoadOptions.Manifest |
            BinaryLoadOptions.UnsignedManagedDependencies |
            BinaryLoadOptions.MD5;

        public AuthenticodeSigner(X509Certificate2 certificate) {
            Certificate = certificate; 
        }
        public X509Certificate2 Certificate {get; private set;}

        public void Sign(string path, bool strongName = false)
        {
            var certRef = new CertificateReference(Certificate);
            var r = BinaryLoad(path);

            r.SigningCertificate = certRef;
            if (strongName)
                r.StrongNameKeyCertificate = certRef;

            r.Save().Wait();
        }

        private Binary BinaryLoad(string path)
        {
            return Binary.Load(path, BINARY_LOAD_OPTIONS).Result;
        }
    }
}
