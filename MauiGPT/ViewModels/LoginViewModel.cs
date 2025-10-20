using MauiGPT.Interfaces;
using MauiGPT.Helpers;
using System.Windows.Input;

namespace MauiGPT.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IAzureService _azureService;
        private string _apiKey = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        public LoginViewModel(IAzureService azureService)
        {
            _azureService = azureService;
            LoginCommand = new Command(async () => await LoginAsync(), () => !IsLoading);
        }

        public string ApiKey
        {
            get => _apiKey;
            set => SetProperty(ref _apiKey, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    ((Command)LoginCommand).ChangeCanExecute();
                }
            }
        }

        public ICommand LoginCommand { get; }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                ErrorMessage = "Please enter your API key";
                return;
            }

            // Check connectivity first
            if (!await ConnectivityHelper.CheckConnectionAsync())
            {
                ErrorMessage = "No internet connection";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                AppLogger.Log($"Testing connection with API key: {ApiKey.Substring(0, Math.Min(4, ApiKey.Length))}***");
                
                // Test the connection with the API key
                var isValid = await _azureService.TestConnectionAsync(ApiKey);

                if (isValid)
                {
                    AppLogger.Log("API key validated successfully");
                    
                    // Store API key securely
                    await SecureStorage.SetAsync("api_key", ApiKey);

                    // Navigate to chat page using relative navigation
                    await Shell.Current.GoToAsync("ChatPage");
                }
                else
                {
                    ErrorMessage = "Invalid API key. Please try again.";
                    AppLogger.Log("API key validation failed");
                }
            }
            catch (HttpRequestException httpEx)
            {
                ErrorMessage = "Cannot reach server. Check your connection.";
                AppLogger.LogError("HTTP Error during login", httpEx);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Connection error: {ex.Message}";
                AppLogger.LogError("Error during login", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
