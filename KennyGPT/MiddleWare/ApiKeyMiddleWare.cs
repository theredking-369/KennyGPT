namespace KennyGPT.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string API_KEY_HEADER = "X-API-Key";
        private const string SESSION_ID_HEADER = "X-Session-Id";
        private readonly ILogger<ApiKeyMiddleware> _logger;

        public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            // Allow Swagger endpoints without API key
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }

            // Skip API key check for authentication endpoints
            if (context.Request.Path.StartsWithSegments("/api/auth"))
            {
                await _next(context);
                return;
            }

            // Skip API key check for non-API endpoints
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                await _next(context);
                return;
            }

            context.Request.Headers.TryGetValue(SESSION_ID_HEADER, out var sessionId);

            if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var extractedApiKey))
            {
                _logger.LogWarning("API key missing from request");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key is missing");
                return;
            }

            var apiKey = configuration["ApiKey"];
            var publicDemoKey = configuration["PublicDemoKey"]; // ✅ Add public key

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("API Key not configured");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("API Key not configured");
                return;
            }

            // ✅ Accept BOTH private AND public keys
            if (!apiKey.Equals(extractedApiKey) && !publicDemoKey.Equals(extractedApiKey))
            {
                _logger.LogWarning("Invalid API key attempt");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid API Key");
                return;
            }

            await _next(context);
        }
    }
}