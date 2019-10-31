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
using Java.Security;
using Javax.Crypto;
using Javax.Crypto.Spec;

namespace wabb
{
    class PasswordBasedKeyHelper : KeyHelper
    {
        private const int KEY_SIZE = 1024;
        private const string TRANSFORMATION = "AES";
        private const int ITERATIONS = 1000;
        private readonly byte[] SALT = Encoding.UTF8.GetBytes("Team Patrick, Kohler, Jake, and Spencer!"); // May change this to be non-static

        public PasswordBasedKeyHelper(string keyName)
        {
            _keyAlias = keyName.ToLowerInvariant();
            _androidKeyStore = KeyStore.GetInstance(KEYSTORE_NAME);
            _androidKeyStore.Load(null);
        }

        // Do not use this. Use CreateKey(string).
        public override void CreateKey()
        {
            // Umm so this can't be done? I guess inheriting from the abstract class kind of breaksdown here
        }

        public void CreateKey(string password)
        {
            DeleteKey();

            var spec = new PBEKeySpec(password.ToCharArray(), SALT, ITERATIONS, KEY_SIZE);
            var passwordBasedKey = SecretKeyFactory.GetInstance("PBKDF2WithHmacSHA1").GenerateSecret(spec);

            _androidKeyStore.SetKeyEntry(_keyAlias, passwordBasedKey, null, null);
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