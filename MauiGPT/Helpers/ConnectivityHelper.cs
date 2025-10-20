namespace MauiGPT.Helpers
{
    public static class ConnectivityHelper
    {
        public static bool IsConnected()
        {
            var current = Connectivity.Current.NetworkAccess;
            return current == NetworkAccess.Internet;
        }

        public static async Task<bool> CheckConnectionAsync()
        {
            if (!IsConnected())
            {
                await Shell.Current.DisplayAlert(
                    "No Internet", 
                    "Please check your internet connection", 
                    "OK");
                return false;
            }
            return true;
        }
    }
}
