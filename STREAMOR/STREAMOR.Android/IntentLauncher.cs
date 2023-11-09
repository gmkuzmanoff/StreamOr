using Android.App;
using Android.Content;
using STREAMOR.Droid;
using System;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(IntentLauncher))]
namespace STREAMOR.Droid
{
    [Activity(Label = "IntentLauncher")]
    public class IntentLauncher : ILaunchActivity
    {
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