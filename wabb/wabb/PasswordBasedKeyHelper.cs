using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Java.Security;
using Javax.Crypto;
using Javax.Crypto.Spec;

namespace wabb
{
    class PasswordBasedKeyHelper
    {
        private const int KEY_SIZE = 256;
        private const string TRANSFORMATION = "AES";
        private const int ITERATIONS = 1000;
        private readonly byte[] SALT = Encoding.UTF8.GetBytes("Team Patrick, Kohler, Jake, and Spencer!"); // May change this to be non-static

        private readonly string _keyAlias;
        private SecureStorageHelper _storageHelper = new SecureStorageHelper();

        public PasswordBasedKeyHelper(string keyName)
        {
            _keyAlias = keyName.ToLowerInvariant();
        }

        public void CreateKey(string password, string userEmail)
        {
            DeleteKey();

            var spec = new PBEKeySpec((password+userEmail).ToCharArray(), SALT, ITERATIONS, KEY_SIZE);
            var keyGenerator = SecretKeyFactory.GetInstance("PBEWithHmacSHA256AndAES_256");
            var key = keyGenerator.GenerateSecret(spec);

            _storageHelper.StoreItem<byte[]>(_keyAlias, key.GetEncoded());
        }

        public bool DeleteKey()
        {
            return _storageHelper.RemoveItem(_keyAlias);
        }

        private IKey GetKey()
        {
            var jsonKey = _storageHelper.GetItem<byte[]>(_keyAlias);
            var key = new SecretKeySpec(jsonKey, 0, jsonKey.Length, TRANSFORMATION);
            return key;
        }

        public byte[] EncryptData(string plaintext)
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

        public string DecryptData(byte[] encryptedData)
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