using MauiGPT.Interfaces;
using MauiGPT.Helpers;
using MauiGPT.Models;
using System.Windows.Input;
using System.Net.Http.Json;

namespace MauiGPT.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IAzureService _azureService;
        private readonly HttpClient _httpClient;
        private string _statusMessage = "Connecting...";
        private bool _isLoading = true;

        private const string PUBLIC_DEMO_KEY = "public-demo-2024";

        public LoginViewModel(IAzureService azureService, HttpClient httpClient)
        {
            _azureService = azureService;
            _httpClient = httpClient;
            
            // ✅ Start with delay
            Task.Run(async () =>
            {
                await Task.Delay(500);
                await AutoLoginAsync();
            });
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private async Task AutoLoginAsync()
        {
            try
            {
                StatusMessage = "Checking session...";
                AppLogger.Log("🔍 Checking existing session...");
                
                // ✅ NEW: Try to use existing session first
                var existingSessionId = await SecureStorage.GetAsync("session_id");
                var existingExpires = await SecureStorage.GetAsync("session_expires");
                
                if (!string.IsNullOrEmpty(existingSessionId) && !string.IsNullOrEmpty(existingExpires))
                {
                    if (DateTime.TryParse(existingExpires, out var expiresAt))
                    {
                        if (DateTime.UtcNow < expiresAt)
                        {
                            // Session is still valid
                            var remaining = expiresAt - DateTime.UtcNow;
                            AppLogger.Log($"✅ Existing session valid for {remaining.Minutes} more minutes");
                            StatusMessage = $"Resuming session ({remaining.Minutes}m left)...";
                            await Task.Delay(1000);
                            
                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                await Shell.Current.GoToAsync("ChatPage");
                            });
                            return; // ✅ Don't create new session
                        }
                        else
                        {
                            AppLogger.Log("⏰ Session expired, creating new one");
                            StatusMessage = "Session expired, creating new one...";
                            ClearPreviousSession(); // Clear expired session
                        }
                    }
                }
                
                // ✅ Only create new session if none exists or expired
                StatusMessage = "Testing connection...";
                AppLogger.Log("🔍 Testing server connection...");
                
                var isValid = await TestConnectionWithDetailsAsync(PUBLIC_DEMO_KEY);

                if (!isValid)
                {
                    StatusMessage = "Cannot connect to server";
                    AppLogger.LogError("❌ Server connection failed", null);
                    await Task.Delay(2000);
                    
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Shell.Current.DisplayAlert(
                            "Connection Error",
                            "Cannot connect to the server. Please check:\n\n" +
                            "1. Your internet connection\n" +
                            "2. The server is running at:\n" +
                            "   https://kennygpt.azurewebsites.net\n\n" +
                            "Try again in a few moments.",
                            "OK");
                    });
                    return;
                }

                AppLogger.Log("✅ Server connection successful");
                StatusMessage = "Creating your session...";
                AppLogger.Log("📝 Requesting new session from server...");

                var session = await CreateNewSession(PUBLIC_DEMO_KEY);
                
                if (session != null)
                {
                    await SecureStorage.SetAsync("api_key", PUBLIC_DEMO_KEY);
                    await SecureStorage.SetAsync("session_id", session.Id);
                    await SecureStorage.SetAsync("session_expires", session.ExpiresAt.ToString("O"));
                    await SecureStorage.SetAsync("user_id", session.UserId);

                    AppLogger.Log($"✅ Session created successfully");
                    AppLogger.Log($"   Session ID: {session.Id}");
                    AppLogger.Log($"   User ID: {session.UserId}");
                    AppLogger.Log($"   Expires: {session.ExpiresAt.ToLocalTime()}");

                    StatusMessage = "Welcome! Starting chat...";
                    await Task.Delay(500);

                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Shell.Current.GoToAsync("ChatPage");
                    });
                }
                else
                {
                    StatusMessage = "Failed to create session";
                    AppLogger.LogError("❌ Session creation returned null", null);
                    await Task.Delay(2000);
                    
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Shell.Current.DisplayAlert(
                            "Session Error",
                            "Failed to create a session. The server may be unavailable.\n\nPlease restart the app.",
                            "OK");
                    });
                }
            }
            catch (HttpRequestException httpEx)
            {
                StatusMessage = "Network error";
                AppLogger.LogError($"❌ HTTP Error: {httpEx.Message}", httpEx);
                await Task.Delay(2000);
                
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Shell.Current.DisplayAlert(
                        "Network Error",
                        $"Cannot reach the server:\n\n{httpEx.Message}\n\nPlease check your internet connection.",
                        "OK");
                });
            }
            catch (Exception ex)
            {
                StatusMessage = "Error starting session";
                AppLogger.LogError($"❌ Unexpected error: {ex.Message}", ex);
                await Task.Delay(2000);
                
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Shell.Current.DisplayAlert(
                        "Error",
                        $"An error occurred:\n\n{ex.Message}",
                        "OK");
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ClearPreviousSession()
        {
            try
            {
                SecureStorage.Remove("api_key");
                SecureStorage.Remove("session_id");
                SecureStorage.Remove("session_expires");
                SecureStorage.Remove("user_id");
                
                AppLogger.Log("✅ Previous session cleared - starting fresh");
            }
            catch (Exception ex)
            {
                AppLogger.LogError("Error clearing session", ex);
            }
        }

        // ✅ NEW: Test connection with detailed error logging
        private async Task<bool> TestConnectionWithDetailsAsync(string apiKey)
        {
            try
            {
                AppLogger.Log($"   Testing: https://kennygpt.azurewebsites.net/api/chat/test");
                
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "MauiGPT/1.0");

                var response = await _httpClient.GetAsync(
                    "https://kennygpt.azurewebsites.net/api/chat/test",
                    HttpCompletionOption.ResponseHeadersRead
                );

                AppLogger.Log($"   Response: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    AppLogger.Log($"   Content: {content}");
                    return true;
                }
                
                AppLogger.LogError($"   Server returned: {response.StatusCode}", null);
                return false;
            }
            catch (HttpRequestException httpEx)
            {
                AppLogger.LogError($"   HTTP Exception: {httpEx.Message}", httpEx);
                return false;
            }
            catch (TaskCanceledException timeoutEx)
            {
                AppLogger.LogError($"   Timeout: {timeoutEx.Message}", timeoutEx);
                return false;
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"   Unknown error: {ex.Message}", ex);
                return false;
            }
        }

        private async Task<MUserSession?> CreateNewSession(string apiKey)
        {
            try
            {
                AppLogger.Log($"   POST: https://kennygpt.azurewebsites.net/api/chat/session");
                
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "MauiGPT/1.0");

                var response = await _httpClient.PostAsync(
                    "https://kennygpt.azurewebsites.net/api/chat/session",
                    null
                );

                AppLogger.Log($"   Response: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var session = await response.Content.ReadFromJsonAsync<MUserSession>();
                    return session;
                }

                var error = await response.Content.ReadAsStringAsync();
                AppLogger.LogError($"   Session creation failed: {response.StatusCode} - {error}", null);
                return null;
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"   Exception creating session: {ex.Message}", ex);
                return null;
            }
        }
    }
}
