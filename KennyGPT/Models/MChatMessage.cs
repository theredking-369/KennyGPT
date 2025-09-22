namespace KennyGPT.Models
{
    public class MChatMessage
    {
        public string Id { get; set; }
        public string ConversationId { get; set; }
        public string Role { get; set; } // "user" or "assistant"
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
