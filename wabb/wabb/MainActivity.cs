using System;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Java.Security;
using Javax.Crypto;
// Used for SecureStorage
using Xamarin.Essentials;

namespace wabb
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.StoredItems);

            // these are mutually exclusive
            SetupKeyCreationTesting();
            //SetupStoredItemTesting();
        }

        public void SetupKeyCreationTesting()
        {
            // Grab the buttons
            var saveButton = FindViewById<Button>(Resource.Id.saveButton);
            var getButton = FindViewById<Button>(Resource.Id.getButton);
            var deleteButton = FindViewById<Button>(Resource.Id.deleteButton);
            // Renamed, get all can feed into delete all if desired
            var deleteAllButton = FindViewById<Button>(Resource.Id.deleteAllButton);
            deleteAllButton.Text = "Get All";

            // Janky add listeners to buttons
            saveButton.Click += (o, e) =>
            {
                var key = FindViewById<EditText>(Resource.Id.storedKeyText).Text;
                var data = FindViewById<EditText>(Resource.Id.storedMessageText).Text;
                Print(CreateKey(key, data));
            };
            getButton.Click += (o, e) =>
            {
                var key = FindViewById<EditText>(Resource.Id.storedKeyText).Text;
                var data = FindViewById<EditText>(Resource.Id.storedMessageText).Text;
                var helper = new PlatformEncryptionKeyHelper(key);

                var encryptedData = EncryptMessage(helper, data);
                Print(DecryptMessage(helper, encryptedData));
            };

            deleteButton.Click += (o, e) =>
            {
                var key = FindViewById<EditText>(Resource.Id.storedKeyText).Text;
                Print(DeleteKey(key).ToString());
            };
            deleteAllButton.Click += (o, e) =>
            {
                var keyAliases = GetAllKeys();
                var output = "Key aliases: \n";
                foreach (var alias in keyAliases)
                {
                    output += alias + "\n";
                }
                Print(output);
            };

            // Grab the text inputs
            var nameInput = FindViewById<EditText>(Resource.Id.storedKeyText);
            var messageInput = FindViewById<EditText>(Resource.Id.storedMessageText);
            // Set prompts
            nameInput.Hint = "Key alias";
            messageInput.Hint = "Message to be encrypted";
        }

        public void SetupStoredItemTesting()
        {
            // Grab the buttons
            var saveButton = FindViewById<Button>(Resource.Id.saveButton);
            var getButton = FindViewById<Button>(Resource.Id.getButton);
            var deleteButton = FindViewById<Button>(Resource.Id.deleteButton);
            var deleteAllButton = FindViewById<Button>(Resource.Id.deleteAllButton);

            // Janky add listeners to buttons
            saveButton.Click += (o, e) =>
            {
                var key = FindViewById<EditText>(Resource.Id.storedKeyText).Text;
                var data = FindViewById<EditText>(Resource.Id.storedMessageText).Text;
                CreateStoredItem(key, data);
            };
            getButton.Click += (o, e) =>
            {
                var key = FindViewById<EditText>(Resource.Id.storedKeyText).Text;
                Print(GetStoredItem(key));
            };

            deleteButton.Click += (o, e) =>
            {
                var key = FindViewById<EditText>(Resource.Id.storedKeyText).Text;
                Print(DeleteStoredItem(key).ToString());
            };
            deleteAllButton.Click += (o, e) =>
            {
                DeleteAllStoredItems();
            };

            // Grab the text inputs
            var nameInput = FindViewById<EditText>(Resource.Id.storedKeyText);
            var messageInput = FindViewById<EditText>(Resource.Id.storedMessageText);
            // Set prompts
            nameInput.Hint = "Stored item key name";
            messageInput.Hint = "Stored item contents";
        }

        public void Print(string output)
        {
            var keysText = FindViewById<TextView>(Resource.Id.keysText);
            keysText.Text = output;
        }

        // ----- SecureStorage Interactions -----
        public void CreateStoredItem(string key, string data)
        {
            try
            {
                // This returns as Task<>, can bubble up async instead of lame .Wait()
                // key and data must be strings
                SecureStorage.SetAsync(key, data).Wait();
            }
            catch
            {
                Print("Failed to create");
            }
        }

        public string GetStoredItem(string key)
        {
            try
            {
                // This returns a Task<string>, can bubble up async instead of lame .Result
                return SecureStorage.GetAsync(key).Result;
            }
            catch
            {
                return "Failed to get";
            }
        }

        public bool DeleteStoredItem(string key)
        {
            // My short testing returned false every time, whether the key existed or not
            // it did succeed in deleting if the key did exist though
            return SecureStorage.Remove(key);
        }

        public void DeleteAllStoredItems()
        {
            SecureStorage.RemoveAll();
        }


        // ----- KeyStore Interactions -----
        public string CreateKey(string alias, string message)
        {
            // Make use of that helper bay-bee, yeet
            var helper = new PlatformEncryptionKeyHelper(alias);
            // Used for Asymm keys
            //helper.CreateKeyPair();
            // Used for Symm keys
            var testKeyOutput = helper.CreateSymmetricKey();
            return testKeyOutput;

            // If encrypted data is converted from byte[] to string, then back to byte[]
            // it does not come back the same, will not decrypt
            //var encryptedData = EncryptMessage(helper, message); //TEMP uncomment once done testing
            //return DecryptMessage(helper, encryptedData); //TEMP uncomment once done testing
        }

        public byte[] EncryptMessage(PlatformEncryptionKeyHelper helper, string message)
        {
            // Define the key parameters (I just copied this)
            var transformation = "RSA/ECB/PKCS1Padding";
            var cipher = Cipher.GetInstance(transformation);
            var publicKey = helper.GetPublicKey();
            if (publicKey == null)
            {
                return null;
            }

            // Set up encryption machine
            cipher.Init(CipherMode.EncryptMode, publicKey);

            // Mostly jsut copied this, convert UTF8 to bytes?
            var encryptedData = cipher.DoFinal(Encoding.UTF8.GetBytes(message));
            return encryptedData;
        }

        public string DecryptMessage(PlatformEncryptionKeyHelper helper, byte[] encryptedData)
        {
            // Define the key parameters (I just copied this)
            var transformation = "RSA/ECB/PKCS1Padding";
            var cipher = Cipher.GetInstance(transformation);
            if (encryptedData == null)
            {
                return "Failed to recieve encrypted data";
            }

            var privateKey = helper.GetPrivateKey();
            if (privateKey == null)
            {
                return "Failed to retrieve private key";
            }
            // Set up decryption machine
            cipher.Init(CipherMode.DecryptMode, helper.GetPrivateKey());

            // This was when I tried to pull the encrypted data from the viewer as string
            // the string must add padding/data because it excepts
            //var encryptedMessageView = FindViewById<TextView>(Resource.Id.encrypted);
            //var encryptedMessage = Encoding.UTF8.GetBytes(encryptedMessageView.Text);
            
            // go from bytes to string
            var decryptedBytes = cipher.DoFinal(encryptedData);
            var decryptedMessage = Encoding.UTF8.GetString(decryptedBytes);
            return decryptedMessage;
        }

        public JavaList<string> GetAllKeys()
        {
            var keyStore = KeyStore.GetInstance("AndroidKeyStore");
            keyStore.Load(null);

            var keyAliasEnumerator = keyStore.Aliases();
            var keyAliases = new JavaList<string>();

            while (keyAliasEnumerator.HasMoreElements)
            {
                keyAliases.Add(keyAliasEnumerator.NextElement());
            }
            return keyAliases;
        }

        public bool DeleteAllKeys()
        {
            var output = true;
            var keyAliases = GetAllKeys();

            foreach(var alias in keyAliases)
            {
                // xamarin key is used to build SecureStorage
                if (alias.Contains("xamarin"))
                    continue;
                output = output && DeleteKey(alias);
            }
            return output;
        }

        public bool DeleteKey(string keyAlias)
        {
            var helper = new PlatformEncryptionKeyHelper(keyAlias);
            return helper.DeleteKey();
        }

    }
}

