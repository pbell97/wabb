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

namespace wabb
{
    public abstract class KeyHelper
    {
        protected const string KEYSTORE_NAME = "AndroidKeyStore"; // I guess? Static I believe

        protected string _keyAlias; // key alias
        protected KeyStore _androidKeyStore;

        abstract public void CreateKey();

        public bool DeleteKey()
        {
            if (!_androidKeyStore.ContainsAlias(_keyAlias))
                return false;
            _androidKeyStore.DeleteEntry(_keyAlias);
            return true;
        }

        abstract public byte[] EncryptData(string plaintext);

        abstract public string DecryptData(byte[] encryptedData);
    }
}