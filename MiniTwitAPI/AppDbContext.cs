using Microsoft.EntityFrameworkCore;
using MiniTwitAPI.Models;

namespace MiniTwitAPI
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Follower> Followers { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>()
                .HasOne(m => m.User)  // Message has one User
                .WithMany(u => u.Messages) // User has many Messages
                .HasForeignKey(m => m.UserId); // Foreign key

            base.OnModelCreating(modelBuilder);
        }
    }
}
