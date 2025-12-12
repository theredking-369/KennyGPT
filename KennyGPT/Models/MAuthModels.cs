using System.ComponentModel.DataAnnotations;

namespace KennyGPT.Models
{
    public class MLoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
        
        public bool RememberMe { get; set; } = false;
    }

    public class MLoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public MUser User { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}
