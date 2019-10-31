using Android.Content;
using Android.OS;
using Android.Security.Keystore;
using Java.Security;
using Javax.Crypto;
using System.Text;

namespace wabb
{
    // Class derived from examples here:
    //  https://msicc.net/xamarin-android-asymmetric-encryption-without-any-user-input-or-hardcoded-values/
    public class AsymmetricKeyHelper : KeyHelper
    {
        private const int KEY_SIZE = 2048; // I guess? We choose I believe
        private const string TRANSFORMATION = "RSA/ECB/PKCS1Padding";

        public AsymmetricKeyHelper(string keyName)
        {
            _keyAlias = keyName.ToLowerInvariant();
            _androidKeyStore = KeyStore.GetInstance(KEYSTORE_NAME);
            _androidKeyStore.Load(null);
        }

        public override void CreateKey()
        {
            // Removes key if it already exists, no change otherwise
            DeleteKey();
            KeyPairGenerator keyGenerator =
                KeyPairGenerator.GetInstance(KeyProperties.KeyAlgorithmRsa, KEYSTORE_NAME);

            // I believe this is always the case for us
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                // Parameters affiliated with the Transformation settings used when making Cipher (@MainActivity.EncryptData())
                var builder = new KeyGenParameterSpec.Builder(_keyAlias, KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
                        .SetBlockModes(KeyProperties.BlockModeEcb)
                        .SetEncryptionPaddings(KeyProperties.EncryptionPaddingRsaPkcs1)
                        .SetRandomizedEncryptionRequired(false).SetKeySize(KEY_SIZE);
                keyGenerator.Initialize(builder.Build());
                builder.Dispose();
            }
            else 
            {
                // Oh buddy it's wild
            }

            keyGenerator.GenerateKeyPair();
            keyGenerator.Dispose();
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
            // Apparently this second parameter acts as a key-pair password, to make private key 
            //  more difficult to get to
            // I dunno when the password would have been set though
        }

        public override byte[] EncryptData(string plaintext)
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

        public override string DecryptData(byte[] encryptedData)
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
    }
}