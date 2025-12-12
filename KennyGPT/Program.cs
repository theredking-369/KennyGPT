using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore;
using KennyGPT.Data;
using KennyGPT.Services;
using KennyGPT.Interfaces;
using KennyGPT.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace KennyGPT
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ✅ JWT Authentication
            var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-super-secret-key-change-in-production-min-32-chars";
            var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "KennyGPT";
            var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "KennyGPT-Users";

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                    };
                });

            builder.Services.AddAuthorization();

            // Database
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ChatDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Services
            builder.Services.AddHttpClient(); // ✅ ADD THIS: Register HttpClient
            builder.Services.AddScoped<IAzureService, AzureService>();
            builder.Services.AddScoped<ISerpAPIService, SerpAPIService>();
            builder.Services.AddScoped<IAuthService, AuthService>(); // ✅ NEW: Auth service

            // ? FIXED: Proper CORS configuration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(
                        "https://gray-ocean-0040c6203.2.azurestaticapps.net",  // ? Your Static Web App
                        "http://localhost:7066",              // Local HTTP
                        "https://localhost:7066",             // Local HTTPS
                        "http://127.0.0.1:7066",              // Local IP HTTP
                        "https://127.0.0.1:7066"              // Local IP HTTPS
                    )
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();  // ? CRITICAL: Required for API key headers
                });
            });

            var app = builder.Build();

            // AUTO-APPLY DATABASE MIGRATIONS ON STARTUP
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var context = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                    logger.LogInformation("Applying database migrations...");
                    context.Database.Migrate();
                    logger.LogInformation("Database migrations applied successfully.");
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }
            }

            // ✅ Seed default users
            using (var scope = app.Services.CreateScope())
            {
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                authService.SeedDefaultUsersAsync().Wait();
            }

            // Configure the HTTP request pipeline.
            // ? CRITICAL FIX: CORS MUST BE BEFORE ApiKeyMiddleware!
            app.UseCors("AllowFrontend");

            // ADD API KEY AUTHENTICATION MIDDLEWARE (after CORS!)
            app.UseMiddleware<ApiKeyMiddleware>();

            // DISABLE Swagger in Production
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "KennyGPT API V1");
                    c.RoutePrefix = string.Empty;
                });
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();
            app.UseAuthentication(); // ✅ Authentication & Authorization middleware
            app.UseAuthorization();
            app.MapControllers();

            // Serve HTML file
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}