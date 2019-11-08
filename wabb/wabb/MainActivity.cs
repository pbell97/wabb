using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Widget;
using Java.IO;
using Java.Security;
using Javax.Crypto;
using System.IO;
using System.Text;
using System.Threading.Tasks;
// Used for SecureStorage
using Xamarin.Essentials;

namespace wabb
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private ImageView _imageView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.StoredItems);

            //Environment.ExternalStorageDirectory.
            _imageView = FindViewById<ImageView>(Resource.Id.imageView1);
            Button button = FindViewById<Button>(Resource.Id.MyButton);
            button.Click += ButtonOnClick;
        }

        void ButtonOnClick(object sender, System.EventArgs eventArgs)
        {
            Intent = new Intent();
            Intent.SetType("image/*");
            Intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), PickImageId);
        }

        public string DEBUGTEST()
        {
            // Test the possibility of encrypting using a Public key in the SecureStorage
            var output = "DEBUGTEST\n";
            return output;
        }

        public static readonly int PickImageId = 1000;

        public TaskCompletionSource<Stream> PickImageTaskCompletionSource { set; get; }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            PickImageTaskCompletionSource = new TaskCompletionSource<Stream>();
            base.OnActivityResult(requestCode, resultCode, intent);

            if (requestCode == PickImageId)
            {
                if ((resultCode == Result.Ok) && (intent != null))
                {
                    Android.Net.Uri uri = intent.Data;
                    Stream stream = ContentResolver.OpenInputStream(uri);

                    // Set the Stream as the completion of the Task
                    PickImageTaskCompletionSource.SetResult(stream);
                    FindViewById<ImageView>(Resource.Id.imageView1).SetImageURI(uri);
                }
                else
                {
                    PickImageTaskCompletionSource.SetResult(null);
                }
            }
        }

    }
}