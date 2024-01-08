using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Support.V4.Media.Session;
using Android.Views;
using Android.Widget;
using System;
using Xamarin.Essentials;

namespace STREAMOR
{
    //Media buttons are hardware buttons found on Android devices and other peripheral devices,
    //for example, the pause/play button on a Bluetooth headset.When a user presses a media button,
    //Android generates a KeyEvent, which contains a key code that identifies the button.
    //The key codes for media button KeyEvents are constants that begin with KEYCODE_MEDIA (for example, KEYCODE_MEDIA_PLAY).
    //https://developer.android.com/guide/topics/media/legacy/media-buttons#java

    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class MediaButtonReceiver : BroadcastReceiver
    {
        bool isHeadsetPlugged = false;
        public override void OnReceive(Context context, Intent intent)
        {
            //Headset plug
            if (intent.Action == Intent.ActionHeadsetPlug)
            {
                if (intent.GetIntExtra("state", 0) == 1)
                {
                    Toast.MakeText(context, "🎧 Headset Connected", ToastLength.Long).Show();
                    isHeadsetPlugged = true;
                }
                else if (isHeadsetPlugged && intent.GetIntExtra("state", 0) == 0)
                {
                    Toast.MakeText(context, "🎧 Headset Disconnected", ToastLength.Long).Show();
                    isHeadsetPlugged = false;
                }
            }
            //Media buttons
            if (intent.Action == Intent.ActionMediaButton)
            {
                int keyCodeFromIntent = intent.GetIntExtra(Intent.ActionMediaButton, 0);
                Keycode keycode = Keycode.Unknown;

                if (keyCodeFromIntent == (int)Keycode.MediaPlay)
                {
                    keycode = (Keycode)keyCodeFromIntent;
                }
                else if (keyCodeFromIntent == (int)Keycode.MediaPause)
                {
                    keycode = (Keycode)keyCodeFromIntent;
                }
                else if (keyCodeFromIntent == (int)Keycode.MediaPlayPause)
                {
                    keycode = (Keycode)keyCodeFromIntent;
                }
                else if (keyCodeFromIntent == (int)Keycode.MediaSkipForward)
                {
                    keycode = (Keycode)keyCodeFromIntent;
                }
                else if (keyCodeFromIntent == (int)Keycode.MediaSkipBackward)
                {
                    keycode = (Keycode)keyCodeFromIntent;
                }
                else if (keyCodeFromIntent == (int)Keycode.MediaNext)
                {
                    keycode = (Keycode)keyCodeFromIntent;
                }
                else if (keyCodeFromIntent == (int)Keycode.MediaPrevious)
                {
                    keycode = (Keycode)keyCodeFromIntent;
                }
                Toast.MakeText(context, $"Keycode: {keycode}", ToastLength.Long).Show();
            }
        }
    }
}