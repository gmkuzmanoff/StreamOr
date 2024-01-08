namespace STREAMOR
{
    public interface ILaunchActivity
    {
        void StartNativeIntentOnBackButtonPressed();
        void MakeToastForAddToFavorites(string title);
        void MakeToastForRemoveToFavorites(string title);
    }
}
