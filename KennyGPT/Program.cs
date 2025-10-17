using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore;
using KennyGPT.Data;
using KennyGPT.Services;
using KennyGPT.Interfaces;
using KennyGPT.Middleware;

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

            builder.Services.AddDbContext<ChatDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IAzureService, AzureService>();
            builder.Services.AddHttpClient<ISerpAPIService, SerpAPIService>();

            // UPDATED: Restrict CORS to your frontend domain only
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", builder =>
                {
                    builder.WithOrigins(
                        "https://theredking-369.github.io",  // Your GitHub Pages
                        "http://localhost:7066",              // For local testing
                        "http://127.0.0.1:7066"               // For local testing
                    )
                    .AllowAnyMethod()
                    .AllowAnyHeader();
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

            // ADD API KEY AUTHENTICATION MIDDLEWARE
            app.UseMiddleware<ApiKeyMiddleware>();

            // Enable CORS with restricted policy
            app.UseCors("AllowFrontend");

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
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}