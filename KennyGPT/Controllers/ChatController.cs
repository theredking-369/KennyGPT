using KennyGPT.Data;
using KennyGPT.Models;
using KennyGPT.Interfaces;
using KennyGPT.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KennyGPT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IAzureService _openAIService;
        private readonly ChatDbContext _dbContext;

        public ChatController(IAzureService openAIService, ChatDbContext dbContext)
        {
            _openAIService = openAIService;
            _dbContext = dbContext;
        }

        [HttpPost("send")]
        public async Task<ActionResult<MChatResponse>> SendMessage([FromBody] MChatRequest request)
        {
            try
            {
                // Get or create conversation
                var conversation = await GetOrCreateConversation(request.ConversationId);

                // Add user message to conversation
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
                    conversation.Messages.TakeLast(10).ToList(), // Keep last 10 messages for context
                    request.SystemPrompt
                );

                // Add AI response to conversation
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

                return Ok(new MChatResponse
                {
                    Response = aiResponse,
                    ConversationId = conversation.Id,
                    Timestamp = assistantMessage.Timestamp
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        private async Task<MConversation> GetOrCreateConversation(string? conversationId)
        {
            if (!string.IsNullOrEmpty(conversationId))
            {
                var existing = await _dbContext.Conversations
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.Id == conversationId);

                if (existing != null)
                    return existing;
            }

            var newConversation = new MConversation
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "anonymous", // Replace with actual user ID from authentication
                Title = "New Conversation",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Conversations.Add(newConversation);
            return newConversation;
        }
    }
}
