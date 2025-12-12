using KennyGPT.Services;
using KennyGPT.Data;
using KennyGPT.Models;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using KennyGPT.Interfaces;

namespace KennyGPT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var (success, token, user, error) = await _authService.LoginAsync(
                request.Username, 
                request.Password
            );

            if (!success)
            {
                return Unauthorized(new { error });
            }

            return Ok(new 
            { 
                token, 
                user = new 
                { 
                    user!.Id, 
                    user.Username 
                },
                message = "Login successful" 
            });
        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyToken()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { error = "No token provided" });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            try
            {
                var jwtKey = _configuration["Jwt:Key"] ?? "your-super-secret-key-change-in-production-min-32-chars";
                var jwtIssuer = _configuration["Jwt:Issuer"] ?? "KennyGPT";
                var jwtAudience = _configuration["Jwt:Audience"] ?? "KennyGPT-Users";

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(jwtKey);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value;
                var username = jwtToken.Claims.First(x => x.Type == "username").Value;

                var user = await _authService.GetUserByIdAsync(userId);
                if (user == null || !user.IsActive)
                {
                    return Unauthorized(new { error = "User not found or inactive" });
                }

                return Ok(new
                {
                    user = new
                    {
                        user.Id,
                        user.Username
                    },
                    message = "Token valid"
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { error = "Invalid token", details = ex.Message });
            }
        }
    }

    public record LoginRequest(string Username, string Password);
}
