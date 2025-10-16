using KennyGPT.Interfaces;
using System.Text.Json;

namespace KennyGPT.Services
{
    public class SerpAPIService : ISerpAPIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _api;

        public SerpAPIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _api = configuration["SerpApi:ApiKey"];
        }

        public async Task<string> WebSearch(string query)
        {
            try
            {
                var url = $"https://serpapi.com/search.json?engine=google&q={Uri.EscapeDataString(query)}&api_key={_api}&num=5";

                var response = await _httpClient.GetStringAsync(url);
                Console.WriteLine($"SerpApi Response: {response}"); // Debug line

                var searchResult = JsonDocument.Parse(response);

                var results = new List<string>();

                if (searchResult.RootElement.TryGetProperty("organic_results", out var organicResults))
                {
                    foreach (var result in organicResults.EnumerateArray().Take(3))
                    {
                        var title = result.TryGetProperty("title", out var titleProp) ? titleProp.GetString() : "";
                        var snippet = result.TryGetProperty("snippet", out var snippetProp) ? snippetProp.GetString() : "";
                        var link = result.TryGetProperty("link", out var linkProp) ? linkProp.GetString() : "";

                        if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(snippet))
                        {
                            results.Add($"Title: {title}\nSnippet: {snippet}\nURL: {link}\n");
                        }
                    }
                }

                if (searchResult.RootElement.TryGetProperty("answer_box", out var answerBox))
                {
                    if (answerBox.TryGetProperty("answer", out var answer))
                    {
                        results.Insert(0, $"Direct Answer: {answer.GetString()}\n");
                    }
                }
                var finalresult = string.Join("\n", results);
                Console.WriteLine($"Processed Search Results: {finalresult}"); // Debug line
                return finalresult;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Search Error: {ex.Message}");
                return $"Search failed: {ex.Message}";
            }
        }
    }
}
