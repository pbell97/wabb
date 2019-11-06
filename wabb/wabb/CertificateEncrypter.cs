using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Security.Cert;
using Javax.Crypto;
using System;

namespace wabb
{
    class CertificateEncrypter
    {
        private readonly Certificate _cert;
        private readonly SecureStorageHelper _storageHelper = new SecureStorageHelper();

        // !! Must match the transformation applied to the Asymm key, @AsymmetricKeyHelper
        private const string TRANSFORMATION = "RSA/ECB/PKCS1Padding";

        //public CertificateEncrypter(string certificateAlias)
        //{
        //    var certificate = _storageHelper.GetItem<byte[]>(certificateAlias);

        //    var stream = new System.IO.MemoryStream(certificate, 0, certificate.Length);
        //    _cert = CertificateFactory.GetInstance("X509").GenerateCertificate(stream);
        //}

        public CertificateEncrypter(string certificateAlias, byte[] serializedCertificate)
        {
            _storageHelper.StoreItem<byte[]>(certificateAlias, serializedCertificate);

            var stream = new System.IO.MemoryStream(serializedCertificate, 0, serializedCertificate.Length);
            var certificate = CertificateFactory.GetInstance("X509").GenerateCertificate(stream);
            _cert = certificate;
        }

        public CertificateEncrypter(string certificate)
        {
            byte[] cert = Convert.FromBase64String(certificate);
            var stream = new System.IO.MemoryStream(cert, 0, cert.Length);
            _cert = CertificateFactory.GetInstance("X509").GenerateCertificate(stream);
        }

        public byte[] GetEncodedCertificate()
        {
            return _cert.GetEncoded();
        }

        public byte[] EncryptData(string plaintext)
        {
            var cipher = Cipher.GetInstance(TRANSFORMATION);
            if (_cert == null)
            {
                return null;
            }

            // Set up encryption machine
            cipher.Init(CipherMode.EncryptMode, _cert);
            // Mostly just copied this, convert UTF8 to bytes?
            var encryptedData = cipher.DoFinal(Encoding.UTF8.GetBytes(plaintext));
            return encryptedData;
        }

        public string EncryptDataToString(string plaintext)
        {
            return Convert.ToBase64String(EncryptData(plaintext));
        }
    }
}