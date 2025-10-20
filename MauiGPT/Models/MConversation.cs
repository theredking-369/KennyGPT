namespace MauiGPT.Models
{
    public class MConversation
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<MChatMessage> Messages { get; set; } = new();
    }
}
