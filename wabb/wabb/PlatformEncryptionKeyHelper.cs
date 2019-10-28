using Android.Content;
using Android.OS;
using Android.Security.Keystore;
using Java.Security;

namespace wabb
{
    // Class derived from examples here:
    //  https://msicc.net/xamarin-android-asymmetric-encryption-without-any-user-input-or-hardcoded-values/
    public class PlatformEncryptionKeyHelper
    {
        private readonly int KeySize = 2048; // I guess? We choose I believe
        private readonly string KEYSTORE_NAME = "AndroidKeyStore"; // I guess? Static I believe
        private Context _context;
        private string _keyName; // key alias
        private KeyStore _androidKeyStore;

        public PlatformEncryptionKeyHelper(Context context, string keyName)
        {
            _context = context;
            _keyName = keyName.ToLowerInvariant();
            _androidKeyStore = KeyStore.GetInstance(KEYSTORE_NAME);
            _androidKeyStore.Load(null);
        }

        public void CreateKeyPair()
        {
            // Removes key if it already exists, no change otherwise
            DeleteKey();
            KeyPairGenerator keyGenerator =
                KeyPairGenerator.GetInstance(KeyProperties.KeyAlgorithmRsa, KEYSTORE_NAME);

            // I believe this is always the case for us
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                // Parameters affiliated with the Transformation settings used when making Cipher (@MainActivity.EncryptData())
                var builder =
                    new KeyGenParameterSpec.Builder(_keyName, KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
                        .SetBlockModes(KeyProperties.BlockModeEcb)
                        .SetEncryptionPaddings(KeyProperties.EncryptionPaddingRsaPkcs1)
                        .SetRandomizedEncryptionRequired(false).SetKeySize(KeySize);
                keyGenerator.Initialize(builder.Build());
            }
            else 
            {
                // Oh buddy it's wild
            }

            keyGenerator.GenerateKeyPair();
        }

        //TODO: Check that key pairs persist
        public IKey GetPublicKey()
        {
            if (!_androidKeyStore.ContainsAlias(_keyName))
                return null;
            return _androidKeyStore.GetCertificate(_keyName)?.PublicKey;
        }

        public IKey GetPrivateKey()
        {
            if (!_androidKeyStore.ContainsAlias(_keyName))
                return null;
            return _androidKeyStore.GetKey(_keyName, null);
            // Apparently this second parameter acts as a key-pair password, to make private key 
            //  more difficult to get to
            // I dunno when the password would have been set though
        }

        public bool DeleteKey()
        {
            if (!_androidKeyStore.ContainsAlias(_keyName))
                return false;
            _androidKeyStore.DeleteEntry(_keyName);
            return true;
        }
    }
}