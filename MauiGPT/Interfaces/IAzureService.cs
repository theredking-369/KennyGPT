using MauiGPT.Models;

namespace MauiGPT.Interfaces
{
    public interface IAzureService
    {
        Task<MChatResponse> SendMessageAsync(MChatRequest request, string apiKey);
        Task<List<MConversation>> GetConversationsAsync(string apiKey);
        Task<List<MChatMessage>> GetConversationMessagesAsync(string conversationId, string apiKey);
        Task<bool> TestConnectionAsync(string apiKey);
        Task<bool> DeleteConversationAsync(string conversationId, string apiKey);
    }
}