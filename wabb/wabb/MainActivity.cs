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
=======
=======
>>>>>>> parent of 8dd232e... Minor testing with external public key use
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

>>>>>>> parent of f77d0ec... Added asym key stuff. Not entirely working but close...
            AsymmetricKeyHelper firstKey = new AsymmetricKeyHelper("firstKey");
            firstKey.CreateKey();
            byte[] encodedKey1 = firstKey.GetPublicKeyBytes();

<<<<<<< HEAD
            AsymmetricKeyHelper secondKey = new AsymmetricKeyHelper("secondKey");
            //byte[] encryptedText = firstKey.EncryptDataWithAnotherPublicKey(encodedKey1, "TestValueGoesHere");

            //string decryptedText = firstKey.DecryptData(encryptedText);


            SymmetricKeyHelper skh = new SymmetricKeyHelper("firstKey");
            skh.CreateKey();
            var encryptedText = skh.EncryptDataToBytes("Testing123");
            var keyString = skh.GetKeyString();


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
            //SetupKeyCreationTesting();
            //SetupStoredItemTesting();

            Print(DEBUGTEST());
        }

        public string DEBUGTEST()
        {
            // Test the possibility of encrypting using a Public key in the SecureStorage
            var output = "DEBUGTEST\n";

            // Make key pair
            var asymmHelper = new AsymmetricKeyHelper("DEBUGTEST");
            asymmHelper.CreateKey();
            var serializedCertificate = asymmHelper.GetCertificate().GetEncoded();

            var certificateEncrypter = new CertificateEncrypter("DEBUGTEST", serializedCertificate);
            var encryptedData = certificateEncrypter.EncryptData("This is a quick little test here/n");

            // Run the externally encrypted data through internal decrypter
            output += asymmHelper.DecryptData(encryptedData);

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

