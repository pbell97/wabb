﻿using Android.Content;
using Android.OS;
using Android.Security.Keystore;
using Java.Security;
using Javax.Crypto;
using System.Text;

namespace wabb
{
    public class SymmetricKeyHelper : KeyHelper
    {
        private const int KEY_SIZE = 256;
        private const string TRANSFORMATION = "AES/ECB/PKCS7Padding";

        public SymmetricKeyHelper(string keyName)
        {
            _keyAlias = keyName.ToLowerInvariant();
            _androidKeyStore = KeyStore.GetInstance(KEYSTORE_NAME);
            _androidKeyStore.Load(null);
        }

        public override void CreateKey()
        {
            // Removes key if it already exists, no change otherwise
            DeleteKey();
            var keyGenerator = KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmAes, KEYSTORE_NAME);
            var builder = new KeyGenParameterSpec.Builder(_keyAlias, KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
                .SetBlockModes(KeyProperties.BlockModeEcb)
                .SetEncryptionPaddings(KeyProperties.EncryptionPaddingPkcs7)
                .SetRandomizedEncryptionRequired(false).SetKeySize(KEY_SIZE);

            keyGenerator.Init(builder.Build());
            var symmKey = keyGenerator.GenerateKey();

            _androidKeyStore.SetKeyEntry(_keyAlias, symmKey, null, null);
            builder.Dispose();
            keyGenerator.Dispose();
        }

        private IKey GetKey()
        {
            if (!_androidKeyStore.ContainsAlias(_keyAlias))
                return null;
            return _androidKeyStore.GetKey(_keyAlias, null);
        }

        public override byte[] EncryptData(string plaintext)
        {
            var cipher = Cipher.GetInstance(TRANSFORMATION);
            var key = GetKey();
            if (key == null)
            {
                return null;
            }

            // Set up encryption machine
            cipher.Init(CipherMode.EncryptMode, key);

            // Mostly just copied this, convert UTF8 to bytes?
            return cipher.DoFinal(Encoding.UTF8.GetBytes(plaintext));
        }

        public override string DecryptData(byte[] encryptedData)
        {
            var cipher = Cipher.GetInstance(TRANSFORMATION);
            var key = GetKey();
            if (encryptedData == null || key == null)
            {
                return "";
            }

            // Set up decryption machine
            cipher.Init(CipherMode.DecryptMode, key);

            // go from bytes to string
            var decryptedBytes = cipher.DoFinal(encryptedData);
            var decryptedMessage = Encoding.UTF8.GetString(decryptedBytes);
            return decryptedMessage;
        }
    }
}