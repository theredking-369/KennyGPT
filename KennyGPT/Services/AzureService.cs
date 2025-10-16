using Azure;
using Azure.AI.OpenAI;
using Azure.Core;
using KennyGPT.Interfaces;
using KennyGPT.Models;
using OpenAI.Chat;
using System.Text.Json;
using System.ClientModel;

namespace KennyGPT.Services
{
    public class AzureService : IAzureService
    {
        private readonly AzureOpenAIClient _client;
        private readonly string _deploymentName;
        private readonly ISerpAPIService _searchService;
        
        public AzureService(IConfiguration configuration, ISerpAPIService searchService)
        {
            var endpoint = configuration["AzureOpenAI:Endpoint"];
            var apiKey = configuration["AzureOpenAI:ApiKey"];
            _deploymentName = configuration["AzureOpenAI:DeploymentName"];
            _searchService = searchService;

            _client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        }

        public async Task<string> GetChatCompletion(List<Models.MChatMessage> messages, string? systemPrompt = null)
        {
            var chatClient = _client.GetChatClient(_deploymentName);
            
            // Add current date/time to system prompt
            var currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var timeZone = TimeZoneInfo.Local.DisplayName;

            var enhancedSystemPrompt = $@"{systemPrompt}
CURRENT DATE & TIME: {currentDateTime} ({timeZone})

You have access to a web search tool. Use it when users ask about:
- Recent events, news, or current information
- Real-time data (weather, stock prices, sports scores, etc.)
- Current status of people, companies, or ongoing situations
- Breaking news or trending topics
- Any information that requires up-to-date knowledge beyond your training data

Always use the search tool for current information requests.";

            var chatMessages = new List<ChatMessage>();

            if (!string.IsNullOrEmpty(enhancedSystemPrompt))
            {
                chatMessages.Add(new SystemChatMessage(enhancedSystemPrompt));
            }

            foreach (var message in messages)
            {
                if (message.Role == "user")
                    chatMessages.Add(new UserChatMessage(message.Content));
                else if (message.Role == "assistant")
                    chatMessages.Add(new AssistantChatMessage(message.Content));
            }

            // Define the web search function/tool
            var searchTool = ChatTool.CreateFunctionTool(
                functionName: "web_search",
                functionDescription: "Search the web for current information, news, real-time data, or any information that requires up-to-date knowledge. Use this when the user asks about recent events, current status, breaking news, or anything happening after your knowledge cutoff.",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "query": {
                            "type": "string",
                            "description": "The search query to look up on the web. Be specific and include relevant keywords."
                        }
                    },
                    "required": ["query"],
                    "additionalProperties": false
                }
                """)
            );

            var options = new ChatCompletionOptions
            {
                Temperature = 0.7f,
                MaxOutputTokenCount = 1000,
                Tools = { searchTool }
            };

            Console.WriteLine("Sending initial request to Azure OpenAI...");
            ChatCompletion completion = await chatClient.CompleteChatAsync(chatMessages, options);

            // Check if the model wants to call the search function
            if (completion.FinishReason == ChatFinishReason.ToolCalls)
            {
                Console.WriteLine($"Model requested tool call(s)");
                
                // Add the assistant's response with tool calls to the conversation
                chatMessages.Add(new AssistantChatMessage(completion));

                foreach (var toolCall in completion.ToolCalls)
                {
                    if (toolCall.FunctionName == "web_search")
                    {
                        // Parse the function arguments
                        var functionArgs = JsonDocument.Parse(toolCall.FunctionArguments);
                        var searchQuery = functionArgs.RootElement.GetProperty("query").GetString();
                        
                        Console.WriteLine($"Performing web search for: {searchQuery}");
                        
                        // Execute the search
                        var searchResults = await _searchService.WebSearch(searchQuery);
                        Console.WriteLine($"Search completed, results length: {searchResults?.Length ?? 0}");

                        // Add the tool result back to the conversation
                        chatMessages.Add(new ToolChatMessage(toolCall.Id, searchResults));
                    }
                }

                // Get the final response from the model with the search results
                Console.WriteLine("Getting final response with search results...");
                ChatCompletion finalCompletion = await chatClient.CompleteChatAsync(chatMessages, options);
                var finalResponse = finalCompletion.Content[0].Text;
                
                Console.WriteLine($"Final response: {finalResponse}");
                return finalResponse;
            }

            // No tool call needed, return the direct response
            return completion.Content[0].Text;
        }

        public async Task<string> GetBasicChatCompletion(List<MChatMessage> messages, string? systemPrompt = null)
        {
            var chatClient = _client.GetChatClient(_deploymentName);
            var chatMessages = new List<ChatMessage>();

            if (!string.IsNullOrEmpty(systemPrompt))
            {
                chatMessages.Add(new SystemChatMessage(systemPrompt));
            }

            foreach (var message in messages)
            {
                if (message.Role == "user")
                    chatMessages.Add(new UserChatMessage(message.Content));
                else if (message.Role == "assistant")
                    chatMessages.Add(new AssistantChatMessage(message.Content));
            }

            var options = new ChatCompletionOptions
            {
                Temperature = 0.7f,
                MaxOutputTokenCount = 1000
            };

            ChatCompletion completion = await chatClient.CompleteChatAsync(chatMessages, options);

            return completion.Content[0].Text;
        }
    }
}
