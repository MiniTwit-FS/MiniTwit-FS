using Microsoft.EntityFrameworkCore;
using MiniTwitAPI;
using MiniTwitAPI.Models;
using System;
using System.Threading.Tasks;

public class TestDbContextFactory
{
    public static async Task<AppDbContext> Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("TestDatabase")  // Use in-memory database for testing
            .Options;

        var context = new AppDbContext(options);

        context.Database.EnsureCreated();

        await PopulateWithDataAsync(context);

        return context;
    }

    // Async method to populate data in the context
    public static async Task PopulateWithDataAsync(AppDbContext context)
    {
        await AddUsersAsync(context);
        await AddFollowersAsync(context);
        await AddMessagesAsync(context);
    }

    private static async Task AddUsersAsync(AppDbContext context)
    {
        var user1 = new User { Username = "testuser1", Email = "testuser1@example.com", PasswordHash = "password1" };
        var user2 = new User { Username = "testuser2", Email = "testuser2@example.com", PasswordHash = "password2" };

        await context.Users.AddAsync(user1);
        await context.Users.AddAsync(user2);

        await context.SaveChangesAsync();
    }

    private static async Task AddFollowersAsync(AppDbContext context)
    {
        var follower1 = new Follower { UserId = 1, FollowsUserId = 2 };
        var follower2 = new Follower { UserId = 2, FollowsUserId = 1 };

        await context.Followers.AddAsync(follower1);
        await context.Followers.AddAsync(follower2);

        await context.SaveChangesAsync();
    }

    private static async Task AddMessagesAsync(AppDbContext context)
    {
        var message1 = new Message { UserId = 1, Text = "Test Message 1", PublishedDate = DateTime.Now };
        var message2 = new Message { UserId = 2, Text = "Test Message 2", PublishedDate = DateTime.Now };

        await context.Messages.AddAsync(message1);
        await context.Messages.AddAsync(message2);

        await context.SaveChangesAsync();
    }
}
