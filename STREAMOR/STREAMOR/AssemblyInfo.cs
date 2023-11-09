using Android.App;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: 
    XamlCompilation(XamlCompilationOptions.Compile), 
    Application(UsesCleartextTraffic = true),
    ExportFont("PYTHIA.ttf"),
    ExportFont("ARLRDBD.ttf"),
    ExportFont("RussoOne-Regular.ttf"),
    ExportFont("RobotoCondensed-Regular.ttf"), 
    ExportFont("RobotoCondensed-Bold.ttf"),
    ExportFont("RobotoCondensed-Light.ttf"),
    ExportFont("RobotoCondensed-LightItalic.ttf"),
    ExportFont("RobotoCondensed-BoldItalic.ttf"),
    ExportFont("RobotoCondensed-Italic.ttf")]
