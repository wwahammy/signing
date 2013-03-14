using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Security.Cryptography.X509Certificates;

namespace ClrPlus.Signing.Signers
{
    public class OPCSigner 
    {
        public OPCSigner(X509Certificate2 certificate) {
            Certificate = certificate;
        }

        public X509Certificate2 Certificate {get; private set;}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <from>http://msdn.microsoft.com/en-us/library/system.io.packaging.packagedigitalsignaturemanager.sign(v=vs.100).aspx</from>
        public void Sign(string path) {
            var package = Package.Open(path);


            var signatureManager = new PackageDigitalSignatureManager(package);
            signatureManager.CertificateOption = CertificateEmbeddingOption.InSignaturePart;

            var toSign = new List<Uri>();
            foreach (PackagePart packagePart in package.GetParts())
            {
                toSign.Add(packagePart.Uri);
            }

            toSign.Add(PackUriHelper.GetRelationshipPartUri(signatureManager.SignatureOrigin));
            toSign.Add(signatureManager.SignatureOrigin);
            toSign.Add(PackUriHelper.GetRelationshipPartUri(new Uri("/", UriKind.RelativeOrAbsolute)));

           
                signatureManager.Sign(toSign, Certificate);
                package.Close();
           
        }
    }
}
