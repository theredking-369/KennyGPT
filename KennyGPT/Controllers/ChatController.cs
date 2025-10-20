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
                bool isFirstMessage = conversation.Messages.Count == 0;

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

                // ✅ Update conversation title after first message
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
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("conversations")]
        public async Task<ActionResult<List<MConversation>>> GetConversations()
        {
            try
            {
                var conversations = await _dbContext.Conversations
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(50)
                    .ToListAsync();

                return Ok(conversations);
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("conversations/{conversationId}/messages")]
        public async Task<ActionResult<List<MChatMessage>>> GetConversationMessages(string conversationId)
        {
            try
            {
                var messages = await _dbContext.ChatMessages
                    .Where(m => m.ConversationId == conversationId)
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();

                return Ok(messages);
            }
            catch(Exception ex)
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

                // Delete all messages in the conversation
                _dbContext.ChatMessages.RemoveRange(conversation.Messages);

                // Delete the conversation
                _dbContext.Conversations.Remove(conversation);

                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Conversation deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting conversation: {ex.Message}");
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
                UserId = "anonymous",
                Title = "New Conversation",  // Will be updated after first message
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Conversations.Add(newConversation);
            return newConversation;
        }

        // ✅ Add this new method to update conversation title
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
