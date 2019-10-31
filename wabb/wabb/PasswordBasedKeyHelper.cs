using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Security;
using Javax.Crypto;
using Javax.Crypto.Spec;
using Xamarin.Essentials;

namespace wabb
{
    class PasswordBasedKeyHelper
    {
        private const int KEY_SIZE = 256;
        private const string TRANSFORMATION = "AES";
        private const int ITERATIONS = 1000;
        private readonly byte[] SALT = Encoding.UTF8.GetBytes("Team Patrick, Kohler, Jake, and Spencer!"); // May change this to be non-static

        private readonly string _keyAlias;

        public PasswordBasedKeyHelper(string keyName)
        {
            _keyAlias = keyName.ToLowerInvariant();
        }

        public void CreateKey(string password)
        {
            DeleteKey();

            var spec = new PBEKeySpec(password.ToCharArray(), SALT, ITERATIONS, KEY_SIZE);
            var keyGenerator = SecretKeyFactory.GetInstance("PBEWithHmacSHA256AndAES_256");
            var key = keyGenerator.GenerateSecret(spec);
            var serializedKey = Base64.EncodeToString(key.GetEncoded(), default);

            SecureStorage.SetAsync(_keyAlias, serializedKey);
        }

        public bool DeleteKey()
        {
            return SecureStorage.Remove(_keyAlias);
        }

        private IKey GetKey()
        {
            var serializedKey = SecureStorage.GetAsync(_keyAlias).Result;
            if (String.IsNullOrEmpty(serializedKey))
                return null;

            var deserializedKey = Base64.Decode(serializedKey, default);
            var key = new SecretKeySpec(deserializedKey, 0, deserializedKey.Length,TRANSFORMATION);
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