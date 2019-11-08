using Java.Security;
using Javax.Crypto;
using Javax.Crypto.Spec;
using System;
using System.Text;

namespace wabb
{
    public class SymmetricKeyHelper
    {
        private const int KEY_SIZE = 256;
        private const string TRANSFORMATION = "AES";

        private readonly string _keyAlias;
        private readonly SecureStorageHelper _storageHelper = new SecureStorageHelper();

        public SymmetricKeyHelper(string keyName)
        {
            // May think about adding a prefix to indicate Symm key
            _keyAlias = keyName.ToLowerInvariant();

        }

        public void CreateKey()
        {
            // Removes key if it already exists, no change otherwise
            DeleteKey();

            // Generate AES key
            var keyGenerator = KeyGenerator.GetInstance("AES");
            keyGenerator.Init(KEY_SIZE);
            var secretKey = keyGenerator.GenerateKey();
            // Push into the secureStorage
            _storageHelper.StoreItem<byte[]>(_keyAlias, secretKey.GetEncoded());
        }

        // Normally should be private, may need to be public for our purposes
        public IKey GetKey()
        {
            // Pull key and reform it into a key
            var jsonKey = _storageHelper.GetItem<byte[]>(_keyAlias);
            var key = new SecretKeySpec(jsonKey, 0, jsonKey.Length, TRANSFORMATION);
            return key;
        }

        // If there is a key associated with the alias
        public bool HasKey()
        {
            var jsonKey = _storageHelper.GetItem<byte[]>(_keyAlias);
            return (jsonKey != null);
        }

        // Gets base64encoded string of the key
        public string GetKeyString()
        {
            byte[] key = GetKey().GetEncoded();
            return Convert.ToBase64String(key);
        }

        // Loads a byte[] key 
        public void LoadKey(byte[] symKey)
        {
            var loadedKey = new SecretKeySpec(symKey, 0, symKey.Length, TRANSFORMATION);
            _storageHelper.StoreItem<byte[]>(_keyAlias, loadedKey.GetEncoded());
        }

        // Loads string of a key
        public void LoadKey(string symKey)
        {
            byte[] convertedKey = Convert.FromBase64String(symKey);
            LoadKey(convertedKey);
        }

        public bool DeleteKey()
        {
            return _storageHelper.RemoveItem(_keyAlias);
        }

        public byte[] EncryptDataToBytes(string plaintext)
        {
            // Get encryption cipher
            var cipher = Cipher.GetInstance(TRANSFORMATION);
            var key = GetKey();
            if (key == null)
            {
                return null;
            }

            // Set up encryption machine
            cipher.Init(CipherMode.EncryptMode, key);

            // Cipher on the bytes
            return cipher.DoFinal(Encoding.UTF8.GetBytes(plaintext));
        }

        public string EncryptDataToSring(string plaintext)
        {
            byte[] data = EncryptDataToBytes(plaintext);
            return Convert.ToBase64String(data);
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

        public string DecryptData(string encryptedData)
        {
            byte[] data = Convert.FromBase64String(encryptedData);
            return DecryptData(data);
        }
    }
}