using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
=======
=======
>>>>>>> parent of 8dd232e... Minor testing with external public key use
=======
>>>>>>> parent of 1755f9a... Merged latest enc changes
using Java.Security;
// Used for SecureStorage
using Xamarin.Essentials;
<<<<<<< HEAD
>>>>>>> parent of da3a50e... Added storage support. Added string support for sym keys. Somehwat integrating sym keys.
=======
>>>>>>> parent of da3a50e... Added storage support. Added string support for sym keys. Somehwat integrating sym keys.

namespace wabb
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar")]
    public class MainActivity : AppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
<<<<<<< HEAD
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

<<<<<<< HEAD
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

<<<<<<< HEAD
            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;
=======
=======
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.StoredItems);

<<<<<<< HEAD
<<<<<<< HEAD
=======
            // these are mutually exclusive
            //SetupPasswordBasedTesting();
            SetupKeyCreationTesting();
            //SetupStoredItemTesting();
>>>>>>> parent of 8dd232e... Minor testing with external public key use

<<<<<<< HEAD
>>>>>>> parent of f77d0ec... Added asym key stuff. Not entirely working but close...
=======
>>>>>>> parent of f77d0ec... Added asym key stuff. Not entirely working but close...
            AsymmetricKeyHelper firstKey = new AsymmetricKeyHelper("firstKey");
            firstKey.CreateKey();
            byte[] encodedKey1 = firstKey.GetPublicKeyBytes();

<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> parent of f77d0ec... Added asym key stuff. Not entirely working but close...
            AsymmetricKeyHelper secondKey = new AsymmetricKeyHelper("secondKey");
            //byte[] encryptedText = firstKey.EncryptDataWithAnotherPublicKey(encodedKey1, "TestValueGoesHere");

<<<<<<< HEAD
            //string decryptedText = firstKey.DecryptData(encryptedText);

=======
            // Make key pair
            var asymmHelper = new AsymmetricKeyHelper("DEBUGTEST");
            asymmHelper.CreateKey();
            var storageHelper = new SecureStorageHelper();

            // Store cert in SecureStorage
            storageHelper.StoreItem<byte[]>("DEBUGTEST", asymmHelper.GetCertificate().GetEncoded());
            // Pull cert from storage
            var serializedCert = storageHelper.GetItem<byte[]>("DEBUGTEST");
            // Convert to stream in order to recreate cert
            var stream = new System.IO.MemoryStream(serializedCert, 0, serializedCert.Length);
            var certificate = Java.Security.Cert.CertificateFactory.GetInstance("X509").GenerateCertificate(stream);

            // 
            var cipher = Cipher.GetInstance("RSA/ECB/PKCS1Padding");
            if (certificate == null)
            {
                output += "Certificate is null\n";
                return output;
            }

<<<<<<< HEAD
            // Set up encryption machine
            cipher.Init(CipherMode.EncryptMode, certificate);
=======
            SymmetricKeyHelper skh = new SymmetricKeyHelper("firstKey");
            skh.CreateKey();
            var encryptedText = skh.EncryptDataToBytes("Testing123");
            var keyString = skh.GetKeyString();
>>>>>>> parent of f77d0ec... Added asym key stuff. Not entirely working but close...

            // Mostly just copied this, convert UTF8 to bytes?
            var encryptedData = cipher.DoFinal(Encoding.UTF8.GetBytes("Just a quick little test here\n"));
>>>>>>> parent of 8f0ea2d... Added external cert handler

            SymmetricKeyHelper skh = new SymmetricKeyHelper("firstKey");
            skh.CreateKey();
            var encryptedText = skh.EncryptDataToBytes("Testing123");
            var keyString = skh.GetKeyString();

<<<<<<< HEAD

            SymmetricKeyHelper skh2 = new SymmetricKeyHelper("newKey");
            skh2.LoadKey(keyString);
            var decryptedText = skh2.DecryptData(encryptedText);
=======
        public string DEBUGTEST()
        {
            var output = "DEBUGTEST\n";
            return output;
        }
>>>>>>> parent of 8dd232e... Minor testing with external public key use
=======
            SymmetricKeyHelper skh2 = new SymmetricKeyHelper("newKey");
            skh2.LoadKey(keyString);
            var decryptedText = skh2.DecryptData(encryptedText);
>>>>>>> parent of f77d0ec... Added asym key stuff. Not entirely working but close...



            //SymmetricKeyHelper skh = new SymmetricKeyHelper("firstKey");
            //skh.CreateKey();
            //var data = skh.EncryptData("Yeet");
            //var t = skh.GetKey();

            //string keyString = Convert.ToBase64String(t.GetEncoded());
            //byte[] convertedKey = Convert.FromBase64String(keyString);

            //SymmetricKeyHelper skh2 = new SymmetricKeyHelper("newKey");
            //skh2.LoadKey(convertedKey);
            //var l = skh2.DecryptData(data);


            //string encryptedStr = Convert.ToBase64String(data);
            //byte[] test = Convert.FromBase64String(encryptedStr);

            //var k = skh.DecryptData(test);

=======
>>>>>>> parent of da3a50e... Added storage support. Added string support for sym keys. Somehwat integrating sym keys.
=======
>>>>>>> parent of da3a50e... Added storage support. Added string support for sym keys. Somehwat integrating sym keys.
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
>>>>>>> parent of f77d0ec... Added asym key stuff. Not entirely working but close...
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

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}

