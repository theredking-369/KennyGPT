using Azure;
using Azure.AI.OpenAI;
using Azure.AI;
using Azure.Core;
using KennyGPT.Interfaces;
using KennyGPT.Models;

using OpenAI;
using OpenAI.Chat;

namespace KennyGPT.Services
{
    public class AzureService : IAzureService
    {
        private readonly AzureOpenAIClient _client;
        private readonly string _deploymentName;

        public AzureService(IConfiguration configuration)
        {
            var endpoint = configuration["AzureOpenAI:Endpoint"];
            var apiKey = configuration["AzureOpenAI:ApiKey"];
            _deploymentName = configuration["AzureOpenAI:DeploymentName"];

            _client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        }



        
            

        public async Task<string> GetChatCompletion(List<Models.MChatMessage> messages, string? systemPrompt = null)
        {
            var chatClient = _client.GetChatClient(_deploymentName);

            var chatMessages = new List<ChatMessage>();

            if (!string.IsNullOrEmpty(systemPrompt))
            {
                chatMessages.Add(ChatMessage.CreateSystemMessage(systemPrompt));
            }

            foreach (var message in messages)
            {
                if (message.Role == "user")
                    chatMessages.Add(ChatMessage.CreateUserMessage(message.Content));
                else if (message.Role == "assistant")
                    chatMessages.Add(ChatMessage.CreateAssistantMessage(message.Content));
            }

            var completion = await chatClient.CompleteChatAsync(chatMessages, new ChatCompletionOptions
            {
                Temperature = 0.7f,
                MaxTokens = 1000
            });

            return completion.Value.Content[0].Text;
        }
    }
    
}
