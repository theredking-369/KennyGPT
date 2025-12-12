using KennyGPT.Data;
using KennyGPT.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace KennyGPT.Services
{
    public interface IAuthService
    {
        Task<(bool success, string? token, MUser? user, string? error)> LoginAsync(string username, string password);
        Task<MUser?> GetUserByIdAsync(string userId);
        Task<MUser?> GetUserByUsernameAsync(string username);
        string GenerateJwtToken(MUser user);
        Task SeedDefaultUsersAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly ChatDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public AuthService(ChatDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public async Task<(bool success, string? token, MUser? user, string? error)> LoginAsync(string username, string password)
        {
            // Find user by username
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

            if (user == null)
            {
                return (false, null, null, "Invalid username or password");
            }

            if (!user.IsActive)
            {
                return (false, null, null, "Account is inactive");
            }

            // Verify password
            if (!VerifyPassword(password, user.PasswordHash))
            {
                return (false, null, null, "Invalid username or password");
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            return (true, token, user, null);
        }

        public async Task<MUser?> GetUserByIdAsync(string userId)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<MUser?> GetUserByUsernameAsync(string username)
        {
            return await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }

        public string GenerateJwtToken(MUser user)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "your-super-secret-key-change-in-production-min-32-chars";
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "KennyGPT";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "KennyGPT-Users";

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim("username", user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(30), // Token valid for 30 days
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ? Seed the two special users
        public async Task SeedDefaultUsersAsync()
        {
            // Check if users already exist
            if (await _dbContext.Users.AnyAsync())
            {
                return; // Users already seeded
            }

            // ? Get passwords from environment variables or configuration
            var roryPassword = _configuration["DefaultUsers:Rory:Password"] ?? "change-me-in-production";
            var leoniquePassword = _configuration["DefaultUsers:Leonique:Password"] ?? "change-me-in-production";

            var users = new List<MUser>
            {
                new MUser
                {
                    Username = "Rory",
                    PasswordHash = HashPassword(roryPassword),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new MUser
                {
                    Username = "Leonique",
                    PasswordHash = HashPassword(leoniquePassword),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            };

            _dbContext.Users.AddRange(users);
            await _dbContext.SaveChangesAsync();

            Console.WriteLine("? Default users seeded successfully");
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }
    }
}
