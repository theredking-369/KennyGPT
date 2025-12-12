using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KennyGPT.Data
{
    public class ChatDbContextFactory : IDesignTimeDbContextFactory<ChatDbContext>
    {
        public ChatDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ChatDbContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=KennyGPT;Trusted_Connection=true;TrustServerCertificate=true;");

            return new ChatDbContext(optionsBuilder.Options);
        }
    }
}
