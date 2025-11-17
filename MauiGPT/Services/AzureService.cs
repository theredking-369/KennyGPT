using MauiGPT.Interfaces;
using MauiGPT.Models;
using System.Net.Http.Json;

namespace MauiGPT.Services
{
    public class AzureService : IAzureService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public AzureService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(60);
            _apiBaseUrl = "https://kennygpt.azurewebsites.net/api";
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MauiGPT/1.0");
        }

        // ✅ NEW: Add authentication headers including session ID
        private async Task AddAuthHeaders(string apiKey)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MauiGPT/1.0");
            
            // ✅ Add session ID if available
            var sessionId = await SecureStorage.GetAsync("session_id");
            if (!string.IsNullOrEmpty(sessionId))
            {
                _httpClient.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
            }
        }

        public async Task<MChatResponse> SendMessageAsync(MChatRequest request, string apiKey)
        {
            try
            {
                await AddAuthHeaders(apiKey);

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_apiBaseUrl}/chat/send",
                    request
                );

                // ✅ Check for session expiry
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (content.Contains("expired") || content.Contains("Session expired"))
                    {
                        throw new Exception("SESSION_EXPIRED");
                    }
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<MChatResponse>() 
                    ?? throw new Exception("Empty response from server");
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception($"Network error: {httpEx.Message}", httpEx);
            }
            catch (TaskCanceledException)
            {
                throw new Exception("Request timed out. Please check your connection.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error sending message: {ex.Message}", ex);
            }
        }

        public async Task<List<MConversation>> GetConversationsAsync(string apiKey)
        {
            try
            {
                await AddAuthHeaders(apiKey);

                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/chat/conversations");
                
                // ✅ Check for session expiry
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (content.Contains("expired"))
                    {
                        throw new Exception("SESSION_EXPIRED");
                    }
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<MConversation>>() ?? new List<MConversation>();
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception($"Network error: {httpEx.Message}", httpEx);
            }
            catch (TaskCanceledException)
            {
                throw new Exception("Request timed out.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading conversations: {ex.Message}", ex);
            }
        }

        public async Task<List<MChatMessage>> GetConversationMessagesAsync(string conversationId, string apiKey)
        {
            try
            {
                await AddAuthHeaders(apiKey);

                var response = await _httpClient.GetAsync(
                    $"{_apiBaseUrl}/chat/conversations/{conversationId}/messages"
                );

                // ✅ Check for session expiry
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (content.Contains("expired"))
                    {
                        throw new Exception("SESSION_EXPIRED");
                    }
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<MChatMessage>>() ?? new List<MChatMessage>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading messages: {ex.Message}", ex);
            }
        }

        public async Task<bool> TestConnectionAsync(string apiKey)
        {
            try
            {
                await AddAuthHeaders(apiKey);

                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/chat/test");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteConversationAsync(string conversationId, string apiKey)
        {
            try
            {
                await AddAuthHeaders(apiKey);

                var response = await _httpClient.DeleteAsync(
                    $"{_apiBaseUrl}/chat/conversations/{conversationId}"
                );

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting conversation: {ex.Message}", ex);
            }
        }
    }
}