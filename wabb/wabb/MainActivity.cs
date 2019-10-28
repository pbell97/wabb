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
using Xamarin.Essentials;

namespace wabb
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            var saveButton = FindViewById<Button>(Resource.Id.saveButton);
            var getButton = FindViewById<Button>(Resource.Id.getButton);
            var deleteButton = FindViewById<Button>(Resource.Id.deleteButton);
            var deleteAllButton = FindViewById<Button>(Resource.Id.deleteAllButton);

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

            //View header = (View)getLayoutInflater().inflate(Resource.Layout.header_view, null);
            //listView.AddHeaderView(header);

                //var listview = FindViewById<ListView>(Resource.Id.listView);
                //listview.AddView(FindViewById(Resource.Layout.list_item));
                //var button = FindViewById<Button>(Resource.Id.createButton);
                //button.Click += (o, e) => {
                //    var aliasView = FindViewById<EditText>(Resource.Id.keyAlias);
                //    var messageView = FindViewById<EditText>(Resource.Id.inputText);
                //    CreateKey(aliasView.Text, messageView.Text); };
        }

        public void CreateStoredItem(string key, string data)
        {
            try
            {
                Xamarin.Essentials.SecureStorage.SetAsync(key, data).Wait();
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
                return SecureStorage.GetAsync(key).Result;
            }
            catch
            {
                return "Failed to get";
            }
        }

        public bool DeleteStoredItem(string key)
        {
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

        public void CreateKey(string alias, string message)
        {
            var helper = new PlatformEncryptionKeyHelper(Application.Context, alias);
            helper.CreateKeyPair();

            var encryptedData = EncryptMessage(helper, message);
            DecryptMessage(helper, encryptedData);
        }

        public byte[] EncryptMessage(PlatformEncryptionKeyHelper helper, string message)
        {
            var transformation = "RSA/ECB/PKCS1Padding";
            var cipher = Cipher.GetInstance(transformation);
            cipher.Init(CipherMode.EncryptMode, helper.GetPublicKey());

            var encryptedData = cipher.DoFinal(Encoding.UTF8.GetBytes(message));
            var textbox = FindViewById<TextView>(Resource.Id.encrypted);
            textbox.Text = Encoding.UTF8.GetString(encryptedData);
            return encryptedData;
        }

        public void DecryptMessage(PlatformEncryptionKeyHelper helper, byte[] encryptedData)
        {
            var transformation = "RSA/ECB/PKCS1Padding";
            var cipher = Cipher.GetInstance(transformation);
            cipher.Init(CipherMode.DecryptMode, helper.GetPrivateKey());

            //var encryptedMessageView = FindViewById<TextView>(Resource.Id.encrypted);
            //var encryptedMessage = Encoding.UTF8.GetBytes(encryptedMessageView.Text);
            
            var decryptedBytes = cipher.DoFinal(encryptedData);
            var decryptedMessage = Encoding.UTF8.GetString(decryptedBytes);

            var textbox = FindViewById<TextView>(Resource.Id.decrypted);
            textbox.Text = decryptedMessage;
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

