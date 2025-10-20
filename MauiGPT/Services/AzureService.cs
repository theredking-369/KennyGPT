using MauiGPT.Interfaces;
using MauiGPT.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MauiGPT.Services
{
    public class AzureService : IAzureService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public AzureService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(60); // Add timeout for mobile networks
            
            // Use your Azure App Service URL
            _apiBaseUrl = "https://kennygpt.azurewebsites.net/api";
            
            // Add default headers for mobile
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MauiGPT/1.0");
        }

        public async Task<MChatResponse> SendMessageAsync(MChatRequest request, string apiKey)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "MauiGPT/1.0");

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_apiBaseUrl}/chat/send",
                    request
                );

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
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "MauiGPT/1.0");

                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/chat/conversations");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<MConversation>>() ?? new List<MConversation>();
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception($"Network error loading conversations: {httpEx.Message}", httpEx);
            }
            catch (TaskCanceledException)
            {
                throw new Exception("Request timed out loading conversations.");
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
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "MauiGPT/1.0");

                var response = await _httpClient.GetAsync(
                    $"{_apiBaseUrl}/chat/conversations/{conversationId}/messages"
                );

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<MChatMessage>>() ?? new List<MChatMessage>();
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception($"Network error loading messages: {httpEx.Message}", httpEx);
            }
            catch (TaskCanceledException)
            {
                throw new Exception("Request timed out loading messages.");
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
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "MauiGPT/1.0");

                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/chat/test");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
            catch (TaskCanceledException)
            {
                return false;
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
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "MauiGPT/1.0");

                var url = $"{_apiBaseUrl}/chat/conversations/{conversationId}";
                Console.WriteLine($"[DELETE] Sending DELETE request to: {url}");

                var response = await _httpClient.DeleteAsync(url);

                Console.WriteLine($"[DELETE] Response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[DELETE] Response content: {content}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[DELETE] Error response: {errorContent}");
                    throw new Exception($"Server returned {response.StatusCode}: {errorContent}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception($"Network error deleting conversation: {httpEx.Message}", httpEx);
            }
            catch (TaskCanceledException)
            {
                throw new Exception("Request timed out while deleting conversation.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting conversation: {ex.Message}", ex);
            }
        }
    }
}