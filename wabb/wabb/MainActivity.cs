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

            SetupKeyCreationTesting();
            //SetupStoredItemTesting();
        }

        public void SetupKeyCreationTesting()
        {
            // Grab the buttons
            var saveButton = FindViewById<Button>(Resource.Id.saveButton);
            var getButton = FindViewById<Button>(Resource.Id.getButton);
            var deleteButton = FindViewById<Button>(Resource.Id.deleteButton);

            var deleteAllButton = FindViewById<Button>(Resource.Id.deleteAllButton);
            var parentView = FindViewById<LinearLayout>(Resource.Id.linearLayout2);

            // Remove since it is not possible
            parentView.RemoveView(deleteAllButton);

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
                var helper = new PlatformEncryptionKeyHelper(key);
                Print(helper.DeleteKey().ToString());
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

        public void Print(string output)
        {
            var keysText = FindViewById<TextView>(Resource.Id.keysText);
            keysText.Text = output;
        }

        public string CreateKey(string alias, string message)
        {
            // Make use of that helper bay-bee, yeet
            var helper = new PlatformEncryptionKeyHelper(alias);
            helper.CreateKeyPair();

            // If encrypted data is converted from byte[] to string, then back to byte[]
            // it does not come back the same, will not decrypt
            var encryptedData = EncryptMessage(helper, message);
            return DecryptMessage(helper, encryptedData);
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

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}

