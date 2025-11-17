using KennyGPT.Models;
using Microsoft.EntityFrameworkCore;

namespace KennyGPT.Data
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

        public DbSet<MConversation> Conversations { get; set; }
        public DbSet<MChatMessage> ChatMessages { get; set; }
        public DbSet<MUserSession> UserSessions { get; set; } // ✅ Add this

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MConversation>()
                .HasMany(c => c.Messages)
                .WithOne()
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Add index for session lookup
            modelBuilder.Entity<MUserSession>()
                .HasIndex(s => s.ApiKey);

            modelBuilder.Entity<MUserSession>()
                .HasIndex(s => s.ExpiresAt);
        }
    }
}
