using System;
using System.Text;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Javax.Crypto;

namespace wabb
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.list_item);

            //var listview = FindViewById<ListView>(Resource.Id.listView);
            //listview.AddView(FindViewById(Resource.Layout.list_item));
            var button = FindViewById<Button>(Resource.Id.createButton);
            button.Click += (o, e) => {
                var aliasView = FindViewById<EditText>(Resource.Id.keyAlias);
                var messageView = FindViewById<EditText>(Resource.Id.inputText);
                CreateKey(aliasView.Text, messageView.Text); };
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

