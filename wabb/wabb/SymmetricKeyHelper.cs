using Android.Content;
using Android.OS;
using Android.Security.Keystore;
using Java.Security;
using Javax.Crypto;
using Javax.Crypto.Spec;
using System.Text;

namespace wabb
{
    public class SymmetricKeyHelper
    {
        // Please ignore commented code
        // The commented code is the former Symm keys in the keystore
        // Unfortunately, keystore keys cannot be retrieved and thus
        //  cannot be sent to other users
        private const int KEY_SIZE = 256;
        private const string TRANSFORMATION = "AES";//"AES/ECB/PKCS7Padding";

        private readonly string _keyAlias;
        private SecureStorageHelper _storageHelper = new SecureStorageHelper();

        public SymmetricKeyHelper(string keyName)
        {
            _keyAlias = keyName.ToLowerInvariant();
            //_androidKeyStore = KeyStore.GetInstance(KEYSTORE_NAME);
            //_androidKeyStore.Load(null);
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

            //var keyGenerator = KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmAes);
            //var builder = new KeyGenParameterSpec.Builder(_keyAlias, KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
            //    .SetBlockModes(KeyProperties.BlockModeEcb)
            //    .SetEncryptionPaddings(KeyProperties.EncryptionPaddingPkcs7)
            //    .SetRandomizedEncryptionRequired(false).SetKeySize(KEY_SIZE);

            //keyGenerator.Init(builder.Build());
            //var symmKey = keyGenerator.GenerateKey();

            //_androidKeyStore.SetKeyEntry(_keyAlias, symmKey, null, null);
            //builder.Dispose();
            //keyGenerator.Dispose();
        }

        private IKey GetKey()
        {
            // Pull key and reform it into a key
            var jsonKey = _storageHelper.GetItem<byte[]>(_keyAlias);
            var key = new SecretKeySpec(jsonKey, 0, jsonKey.Length, TRANSFORMATION);
            return key;
            //if (!_androidKeyStore.ContainsAlias(_keyAlias))
            //    return null;
            //return _androidKeyStore.GetKey(_keyAlias, null);
        }

        public bool DeleteKey()
        {
            return _storageHelper.RemoveItem(_keyAlias);
        }

        public byte[] EncryptData(string plaintext)
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