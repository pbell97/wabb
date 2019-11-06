using Android.Security.Keystore;
using Java.Security;
using Javax.Crypto;
using System.Text;

namespace wabb
{
    // Class derived from examples here:
    //  https://msicc.net/xamarin-android-asymmetric-encryption-without-any-user-input-or-hardcoded-values/
    public class AsymmetricKeyHelper
    {
        private const string KEYSTORE_NAME = "AndroidKeyStore";
        private const int KEY_SIZE = 2048;
        private const string TRANSFORMATION = "RSA/ECB/PKCS1Padding";


        private readonly string _keyAlias;
        private readonly KeyStore _androidKeyStore;

        public AsymmetricKeyHelper(string keyName)
        {
            _keyAlias = keyName.ToLowerInvariant();
            _androidKeyStore = KeyStore.GetInstance(KEYSTORE_NAME);
            _androidKeyStore.Load(null);
        }

        public void CreateKey()
        {
            // Removes key if it already exists, no change otherwise
            DeleteKey();
            KeyPairGenerator keyGenerator =
                KeyPairGenerator.GetInstance(KeyProperties.KeyAlgorithmRsa, KEYSTORE_NAME);

            // Parameters affiliated with the Transformation settings used when making Cipher
            var builder = new KeyGenParameterSpec.Builder(_keyAlias, KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
                    .SetBlockModes(KeyProperties.BlockModeEcb)
                    .SetEncryptionPaddings(KeyProperties.EncryptionPaddingRsaPkcs1)
                    .SetRandomizedEncryptionRequired(false).SetKeySize(KEY_SIZE);
            keyGenerator.Initialize(builder.Build());
            builder.Dispose();

            // Keys automattically added to KeyStore
            keyGenerator.GenerateKeyPair();
            keyGenerator.Dispose();
        }

        public bool DeleteKey()
        {
            if (!_androidKeyStore.ContainsAlias(_keyAlias))
                return false;
            _androidKeyStore.DeleteEntry(_keyAlias);
            return true;
        }

        // Publicly available certificate
        public Java.Security.Cert.Certificate GetCertificate()
        {
            if (!_androidKeyStore.ContainsAlias(_keyAlias))
                return null;
            return _androidKeyStore.GetCertificate(_keyAlias);
        }

        private IKey GetPublicKey()
        {
            if (!_androidKeyStore.ContainsAlias(_keyAlias))
                return null;
            return _androidKeyStore.GetCertificate(_keyAlias)?.PublicKey;
        }

        private IKey GetPrivateKey()
        {
            if (!_androidKeyStore.ContainsAlias(_keyAlias))
                return null;
            return _androidKeyStore.GetKey(_keyAlias, null);
        }

        public byte[] EncryptData(string plaintext)
        {
            var cipher = Cipher.GetInstance(TRANSFORMATION);
            var publicKey = GetPublicKey();
            if (publicKey == null)
            {
                return null;
            }

            // Set up encryption machine
            cipher.Init(CipherMode.EncryptMode, publicKey);

            // Mostly just copied this, convert UTF8 to bytes?
            return cipher.DoFinal(Encoding.UTF8.GetBytes(plaintext));
        }

        public string DecryptData(byte[] encryptedData)
        {
            var cipher = Cipher.GetInstance(TRANSFORMATION);
            var privateKey = GetPrivateKey();
            if (encryptedData == null || privateKey == null)
            {
                return "";
            }

            // Set up decryption machine
            cipher.Init(CipherMode.DecryptMode, privateKey);

            // go from bytes to string
            var decryptedBytes = cipher.DoFinal(encryptedData);
            var decryptedMessage = Encoding.UTF8.GetString(decryptedBytes);
            return decryptedMessage;
        }
<<<<<<< HEAD
<<<<<<< HEAD
=======

        [System.Obsolete]
        public byte[] EncryptDataWithAnotherPublicKey(byte[] pubKey, string plaintext)
        {
            IKey publicKey = new SecretKeySpec(pubKey, 0, pubKey.Length, TRANSFORMATION);
            IKey actualPublicKey = GetPublicKey();
            //X509EncodedKeySpec spec = new X509EncodedKeySpec(pubKey);
            //KeyFactory keyFactory = KeyFactory.GetInstance("RSA");
            //PublicKey publicKey = (Java.Security.PublicKey)keyFactory.GeneratePublic(spec);

            var cipher = Cipher.GetInstance(TRANSFORMATION);
            // Set up encryption machine
            cipher.Init(CipherMode.EncryptMode, publicKey);

            // Mostly just copied this, convert UTF8 to bytes?
            return cipher.DoFinal(Encoding.UTF8.GetBytes(plaintext));
        }

        public byte[] GetPublicKeyBytes()
        {
            return GetPublicKey().GetEncoded();
        }

        public string GetPublicKeyString()
        {
            return Convert.ToBase64String(GetPublicKeyBytes());
        }

        public void EncryptWithPubKeyString(string pubKey)
        {
            byte[] bytesString = Convert.FromBase64String(pubKey);
            // Do something here
        }

>>>>>>> parent of f77d0ec... Added asym key stuff. Not entirely working but close...
=======
>>>>>>> parent of da3a50e... Added storage support. Added string support for sym keys. Somehwat integrating sym keys.
    }
}