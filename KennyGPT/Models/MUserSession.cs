namespace KennyGPT.Models
{
    public class MUserSession
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = "anonymous";
        public string? ApiKey { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(1);
        public bool IsActive => DateTime.UtcNow < ExpiresAt;
    }
}
