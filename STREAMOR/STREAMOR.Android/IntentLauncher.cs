using Android.App;
using Android.Content;
using Android.Widget;
using STREAMOR.Droid;
using System;
using Xamarin.Forms;

[assembly: Dependency(typeof(IntentLauncher))]
namespace STREAMOR.Droid
{
    [Activity(Label = "IntentLauncher")]
    public class IntentLauncher : ILaunchActivity
    {
        Context context = Android.App.Application.Context;

        public void MakeToastForAddToFavorites(string title)
        {
            Toast.MakeText(context, $" 💙 {title} is Added to your Favorites", ToastLength.Long).Show();
        }

        public void MakeToastForRemoveToFavorites(string title)
        {
            Toast.MakeText(context, $" 💔 {title} is Removed from your Favorites", ToastLength.Long).Show();
        }

        [Obsolete]
        public void StartNativeIntentOnBackButtonPressed()
        {
            Intent intent = new Intent();
            intent.SetAction(Intent.ActionMain);
            intent.AddCategory(Intent.CategoryHome);
            Forms.Context.StartActivity(intent);
        }
    }
}