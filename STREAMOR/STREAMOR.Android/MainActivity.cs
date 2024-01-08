using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Support.V4.Media.Session;
using Android.Bluetooth;

namespace STREAMOR.Droid
{
    [Activity(Label = "STREAMOR", Icon = "@mipmap/icon", Theme = "@style/splashscreen", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize, ScreenOrientation = ScreenOrientation.Portrait)]
    
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        MediaButtonReceiver receiver;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            base.SetTheme(Resource.Style.MainTheme);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            receiver = new MediaButtonReceiver();
            RegisterReceiver(receiver, new IntentFilter(Intent.ActionHeadsetPlug));
            RegisterReceiver(receiver, new IntentFilter(Intent.ActionMediaButton));
            LoadApplication(new App());
            //PublishBroadcast();
        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnDestroy()
        {
            UnregisterReceiver(receiver);
            base.OnDestroy();
        }

        public void PublishBroadcast()
        {
            Intent intent = new Intent();
            intent.SetAction(Intent.ActionMediaButton);
            intent.PutExtra(Intent.ActionMediaButton, 272);
            SendBroadcast(intent);
        }
    }

}