using KennyGPT.Data;
using KennyGPT.Models;
using KennyGPT.Interfaces;
using KennyGPT.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KennyGPT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IAzureService _openAIService;
        private readonly ChatDbContext _dbContext;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IAzureService openAIService, ChatDbContext dbContext, ILogger<ChatController> logger)
        {
            _openAIService = openAIService;
            _dbContext = dbContext;
            _logger = logger;
        }

        // ✅ ENHANCED: Create session endpoint with detailed logging
        [HttpPost("session")]
        public async Task<ActionResult<MUserSession>> CreateSession()
        {
            try
            {
                _logger.LogInformation("📝 Session creation request received");

                // Extract API key from header (already validated by middleware)
                var apiKey = Request.Headers["X-API-Key"].ToString();
                _logger.LogInformation($"   API Key: {(string.IsNullOrEmpty(apiKey) ? "MISSING" : "Present")}");

                // Create new session
                var session = new MUserSession
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = Guid.NewGuid().ToString(), // Unique user ID per session
                    ApiKey = apiKey == "public-demo-2024" ? null : apiKey, // Don't store public key
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(1) // 1 hour session
                };

                _logger.LogInformation($"   Session ID: {session.Id}");
                _logger.LogInformation($"   User ID: {session.UserId}");

                _dbContext.UserSessions.Add(session);

                _logger.LogInformation("   Saving to database...");
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"✅ Session created successfully: {session.Id} for user: {session.UserId}");

                return Ok(session);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError($"❌ Database error creating session: {dbEx.Message}");
                _logger.LogError($"   Inner exception: {dbEx.InnerException?.Message}");
                return StatusCode(500, new
                {
                    error = "Database error creating session",
                    message = dbEx.InnerException?.Message ?? dbEx.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error creating session: {ex.Message}");
                _logger.LogError($"   Stack trace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    error = "Error creating session",
                    message = ex.Message
                });
            }
        }

        // ✅ UPDATED: Get conversations filtered by session
        [HttpGet("conversations")]
        public async Task<ActionResult<List<MConversation>>> GetConversations()
        {
            try
            {
                // Get session ID from header
                var sessionId = Request.Headers["X-Session-Id"].ToString();

                if (string.IsNullOrEmpty(sessionId))
                {
                    return Unauthorized(new { expired = false, message = "Session ID is required" });
                }

                // Validate session
                var session = await _dbContext.UserSessions.FindAsync(sessionId);
                if (session == null)
                {
                    return Unauthorized(new { expired = false, message = "Invalid session" });
                }

                if (DateTime.UtcNow >= session.ExpiresAt)
                {
                    return Unauthorized(new { expired = true, message = "Session expired" });
                }

                // Get conversations for this user only
                var conversations = await _dbContext.Conversations
                    .Where(c => c.UserId == session.UserId)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(50)
                    .ToListAsync();

                return Ok(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading conversations: {ex.Message}");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        // ✅ UPDATED: Send message with session validation
        [HttpPost("send")]
        public async Task<ActionResult<MChatResponse>> SendMessage([FromBody] MChatRequest request)
        {
            try
            {
                // Get session ID from header
                var sessionId = Request.Headers["X-Session-Id"].ToString();

                if (string.IsNullOrEmpty(sessionId))
                {
                    return Unauthorized(new { expired = false, message = "Session ID is required" });
                }

                // Validate session
                var session = await _dbContext.UserSessions.FindAsync(sessionId);
                if (session == null)
                {
                    return Unauthorized(new { expired = false, message = "Invalid session" });
                }

                if (DateTime.UtcNow >= session.ExpiresAt)
                {
                    return Unauthorized(new { expired = true, message = "Session expired" });
                }

                // Get or create conversation with user ID from session
                var conversation = await GetOrCreateConversation(request.ConversationId, session.UserId);
                bool isFirstMessage = conversation.Messages.Count == 0;

                // Add user message
                var userMessage = new MChatMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    ConversationId = conversation.Id,
                    Role = "user",
                    Content = request.Message,
                    Timestamp = DateTime.UtcNow
                };

                conversation.Messages.Add(userMessage);
                _dbContext.ChatMessages.Add(userMessage);

                // Get AI response
                var aiResponse = await _openAIService.GetChatCompletion(
                    conversation.Messages.TakeLast(10).ToList(),
                    request.SystemPrompt
                );

                // Add AI response
                var assistantMessage = new MChatMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    ConversationId = conversation.Id,
                    Role = "assistant",
                    Content = aiResponse,
                    Timestamp = DateTime.UtcNow
                };

                conversation.Messages.Add(assistantMessage);
                _dbContext.ChatMessages.Add(assistantMessage);

                await _dbContext.SaveChangesAsync();

                if (isFirstMessage)
                {
                    await UpdateConversationTitle(conversation.Id, request.Message);
                }

                return Ok(new MChatResponse
                {
                    Response = aiResponse,
                    ConversationId = conversation.Id,
                    Timestamp = assistantMessage.Timestamp
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message: {ex.Message}");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("conversations/{conversationId}/messages")]
        public async Task<ActionResult<List<MChatMessage>>> GetConversationMessages(string conversationId)
        {
            try
            {
                var sessionId = Request.Headers["X-Session-Id"].ToString();

                if (string.IsNullOrEmpty(sessionId))
                {
                    return Unauthorized(new { expired = false, message = "Session ID is required" });
                }

                var session = await _dbContext.UserSessions.FindAsync(sessionId);
                if (session == null || DateTime.UtcNow >= session.ExpiresAt)
                {
                    return Unauthorized(new { expired = true, message = "Session expired" });
                }

                var messages = await _dbContext.ChatMessages
                    .Where(m => m.ConversationId == conversationId)
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();

                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "API is working!", timestamp = DateTime.UtcNow });
        }

        [HttpDelete("conversations/{conversationId}")]
        public async Task<IActionResult> DeleteConversation(string conversationId)
        {
            try
            {
                var conversation = await _dbContext.Conversations
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.Id == conversationId);

                if (conversation == null)
                {
                    return NotFound(new { message = "Conversation not found" });
                }

                _dbContext.ChatMessages.RemoveRange(conversation.Messages);
                _dbContext.Conversations.Remove(conversation);

                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Conversation deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting conversation: {ex.Message}");
            }
        }

        // ✅ UPDATED: Accept userId parameter
        private async Task<MConversation> GetOrCreateConversation(string? conversationId, string userId)
        {
            if (!string.IsNullOrEmpty(conversationId))
            {
                var existing = await _dbContext.Conversations
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId);

                if (existing != null)
                    return existing;
            }

            var newConversation = new MConversation
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId, // Use session's user ID
                Title = "New Conversation",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Conversations.Add(newConversation);
            return newConversation;
        }

        private async Task UpdateConversationTitle(string conversationId, string firstMessage)
        {
            var conversation = await _dbContext.Conversations.FindAsync(conversationId);
            if (conversation != null && conversation.Title == "New Conversation")
            {
                conversation.Title = firstMessage.Length > 50
                    ? firstMessage.Substring(0, 50) + "..."
                    : firstMessage;
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}

// AUTO-APPLY DATABASE MIGRATIONS ON STARTUP

