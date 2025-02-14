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
    }
}
