namespace KennyGPT.Models
{
    public class MChatRequest
    {
        public string Message { get; set; }
        public string? ConversationId { get; set; }
        public string? SystemPrompt { get; set; }
    }
}
