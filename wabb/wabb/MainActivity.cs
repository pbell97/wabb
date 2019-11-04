﻿using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Widget;
using Java.Security;
// Used for SecureStorage
using Xamarin.Essentials;

namespace wabb
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar")]
    public class MainActivity : AppCompatActivity
    {
        private string keyStyle = "symmetric";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.StoredItems);

            // these are mutually exclusive
            //SetupPasswordBasedTesting();
            SetupKeyCreationTesting();
            //SetupStoredItemTesting();

            Print(DEBUGTEST());
        }

        public string DEBUGTEST()
        {
            var output = "DEBUGTEST\n";
            return output;
        }

        // Show how to use Password Based Helper
        public void SetupPasswordBasedTesting()
        {
            // Grab the buttons
            var saveButton = FindViewById<Button>(Resource.Id.saveButton);
            var getButton = FindViewById<Button>(Resource.Id.getButton);
            var deleteButton = FindViewById<Button>(Resource.Id.deleteButton);
            var deleteAllButton = FindViewById<Button>(Resource.Id.deleteAllButton);

            // Remove unused inputs
            var radioGroup = FindViewById<RadioGroup>(Resource.Id.radioGroup1);
            var parent = FindViewById<LinearLayout>(Resource.Id.linearLayout1);
            parent.RemoveView(radioGroup);

            // Janky add listeners to buttons
            saveButton.Click += (o, e) =>
            {
                var key = FindViewById<EditText>(Resource.Id.storedKeyText).Text;
                var password = FindViewById<EditText>(Resource.Id.storedMessageText).Text;
                var helper = new PasswordBasedKeyHelper(key);

                helper.CreateKey(password, "dummy@email.code");
                var encryptedData = helper.EncryptData("Password based key creation success");
                Print(helper.DecryptData(encryptedData));
            };
            getButton.Click += (o, e) =>
            {
                var key = FindViewById<EditText>(Resource.Id.storedKeyText).Text;
                var helper = new PasswordBasedKeyHelper(key);

                var encryptedData = helper.EncryptData("Password based key retrieved success");
                Print(helper.DecryptData(encryptedData));
            };

            deleteButton.Click += (o, e) =>
            {
                var key = FindViewById<EditText>(Resource.Id.storedKeyText).Text;
                var helper = new PasswordBasedKeyHelper(key);
                Print(helper.DeleteKey().ToString());
            };
            deleteAllButton.Click += (o, e) =>
            {
                var storageHelper = new SecureStorageHelper();
                storageHelper.RemoveAllItems();
            };

            // Grab the text inputs
            var nameInput = FindViewById<EditText>(Resource.Id.storedKeyText);
            var messageInput = FindViewById<EditText>(Resource.Id.storedMessageText);
            // Set prompts
            nameInput.Hint = "Password based key alias";
            messageInput.Hint = "Password";
        }

        // Show how to use Symm and Asymm Helpers (no longer very similar)
        //  Symm keys are in app-level SecureStorage now, Asymm keys are in os-level keystore
        public void SetupKeyCreationTesting()
        {
            // Grab the buttons
            var saveButton = FindViewById<Button>(Resource.Id.saveButton);
            var getButton = FindViewById<Button>(Resource.Id.getButton);
            var deleteButton = FindViewById<Button>(Resource.Id.deleteButton);
            // Renamed, get all can feed into delete all if desired
            var deleteAllButton = FindViewById<Button>(Resource.Id.deleteAllButton);
            deleteAllButton.Text = "Get All";

            // !!!
            // Put private key in symm cipher => No output
            // Put symm key in asymm cipher => Exception
            var symmButton = FindViewById<RadioButton>(Resource.Id.SymmRadioButton);
            var asymmButton = FindViewById<RadioButton>(Resource.Id.AsymmRadioButton);

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

                if (keyStyle == "asymmetric")
                {
                    var helper = new AsymmetricKeyHelper(key);

                    var encryptedData = helper.EncryptData(data);
                    Print(helper.DecryptData(encryptedData));
                }
                else
                {
                    var helper = new SymmetricKeyHelper(key);

                    var encryptedData = helper.EncryptData(data);
                    Print(helper.DecryptData(encryptedData));
                }

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

            symmButton.Click += (o, e) =>
            {
                keyStyle = "symmetric";
            };
            asymmButton.Click += (o, e) =>
            {
                keyStyle = "asymmetric";
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

            // Remove unused inputs
            var radioGroup = FindViewById<RadioGroup>(Resource.Id.radioGroup1);
            var parent = FindViewById<LinearLayout>(Resource.Id.linearLayout1);
            parent.RemoveView(radioGroup);

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
            if (keyStyle == "asymmetric")
            {
                var helper = new AsymmetricKeyHelper(alias); helper.CreateKey();

                // If encrypted data is converted from byte[] to string, then back to byte[]
                // it does not come back the same, will not decrypt
                var encryptedData = helper.EncryptData(message);
                return helper.DecryptData(encryptedData);
            }
            else
            {
                var helper = new SymmetricKeyHelper(alias); helper.CreateKey();

                // If encrypted data is converted from byte[] to string, then back to byte[]
                // it does not come back the same, will not decrypt
                var encryptedData = helper.EncryptData(message);
                return helper.DecryptData(encryptedData);
            }


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

            foreach (var alias in keyAliases)
            {
                // xamarin key is used to build SecureStorage
                if (alias.Contains("xamarin"))
                    continue;
                output = output && DeleteKey(alias);
            }
            keyAliases.Dispose();
            return output;
        }

        public bool DeleteKey(string keyAlias)
        {
            if (keyStyle == "asymmetric")
            {
                var helper = new AsymmetricKeyHelper(keyAlias);
                return helper.DeleteKey();
            }
            else
            {
                var helper = new SymmetricKeyHelper(keyAlias);
                return helper.DeleteKey();
            }
        }

    }
}

