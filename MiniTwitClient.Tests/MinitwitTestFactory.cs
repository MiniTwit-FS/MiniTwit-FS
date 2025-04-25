using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniTwitAPI;
using MiniTwitAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniTwitClient.Tests.ClientTest
{
    public class MinitwitTestFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(async services =>
            {
                // Remove the real DB context
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                // Add a fake/in-memory DB context
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                var sp = services.BuildServiceProvider();

                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<AppDbContext>();

                    // Ensure database is created
                    db.Database.EnsureCreated();

                    //await SeedUsers(db);
                    //await SeedMessages(db);
                    //await SeedFollowers(db);

                    await db.SaveChangesAsync();
                }
            });
        }

        private async Task SeedUsers(AppDbContext db)
        {
            db.Users.AddRange(
                new User { Username = "user1", Email = "user1@test.com", PasswordHash = "hash1" },
                new User { Username = "user2", Email = "user2@test.com", PasswordHash = "hash2" },
                new User { Username = "user3", Email = "user3@test.com", PasswordHash = "hash3" },
                new User { Username = "user4", Email = "user4@test.com", PasswordHash = "hash4" }
            );
            await db.SaveChangesAsync();
        }

        private async Task SeedMessages(AppDbContext db)
        {
            var user1 = db.Users.FirstOrDefault(u => u.Username == "user1");
            var user2 = db.Users.FirstOrDefault(u => u.Username == "user2");
            var user3 = db.Users.FirstOrDefault(u => u.Username == "user3");

            db.Messages.AddRange(
                new Message { UserId = user1.Id, Text = "user1 message" },    
                new Message { UserId = user1.Id, Text = "user1 message" },    
                new Message { UserId = user1.Id, Text = "user1 message" },   
                
                new Message { UserId = user2.Id, Text = "user2 message" },    
                new Message { UserId = user2.Id, Text = "user2 message" },  

                new Message { UserId = user3.Id, Text = "user3 message" }
            );
            await db.SaveChangesAsync();
        }

        private async Task SeedFollowers(AppDbContext db)
        {
            var user1 = db.Users.FirstOrDefault(u => u.Username == "user1");
            var user2 = db.Users.FirstOrDefault(u => u.Username == "user2");
            var user3 = db.Users.FirstOrDefault(u => u.Username == "user3");
            var user4 = db.Users.FirstOrDefault(u => u.Username == "user4");

            db.Followers.AddRange(
                new Follower { UserId = user4.Id, FollowsUserId = user1.Id},
                new Follower { UserId = user4.Id, FollowsUserId = user2.Id},
                new Follower { UserId = user4.Id, FollowsUserId = user3.Id},

                new Follower { UserId = user1.Id, FollowsUserId = user2.Id },
                new Follower { UserId = user1.Id, FollowsUserId = user3.Id },

                new Follower { UserId = user2.Id, FollowsUserId = user1.Id },
                new Follower { UserId = user2.Id, FollowsUserId = user3.Id }
            );
            await db.SaveChangesAsync();
        }
    }
}
