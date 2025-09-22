using KennyGPT.Models;
namespace KennyGPT.Interfaces
{
    public interface IAzureService
    {
        Task<string> GetChatCompletion(List<MChatMessage> messages, string? systemPrompt = null);
    }
}
