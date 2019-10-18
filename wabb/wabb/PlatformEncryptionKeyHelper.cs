using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Security.Keystore;
using Android.Views;
using Android.Widget;
using Java.Security;
using Javax.Crypto;

namespace wabb
{
    // Class derived from examples here:
    //  https://msicc.net/xamarin-android-asymmetric-encryption-without-any-user-input-or-hardcoded-values/
    public class PlatformEncryptionKeyHelper
    {
        private readonly int KeySize = 2048; // I guess?
        private readonly string KEYSTORE_NAME = "AndroidKeyStore"; // I guess?
        private Context _context;
        private string _keyName;
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
            DeleteKey();
            KeyPairGenerator keyGenerator =
                KeyPairGenerator.GetInstance(KeyProperties.KeyAlgorithmRsa, KEYSTORE_NAME);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
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
            // Apparently this null acts as a key-pair password, to make private key 
            //  more difficult to get to
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