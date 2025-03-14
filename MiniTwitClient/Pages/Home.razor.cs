using Microsoft.AspNetCore.Components;
using MiniTwitClient.Models;

namespace MiniTwitClient.Pages
{
    public partial class Home : ComponentBase
    {
        // Sample messages; replace this with actual database retrieval
        private List<Message> Messages = new()
        {
            new() { UserId = 2, Text = "Hello Blazor Twitter Clone!", PublishedDate = DateTime.Now.AddHours(-20) },
            new() { UserId = 1, Text = "This is my first tweet!", PublishedDate = DateTime.Now.AddMinutes(-15) },
            new() { UserId = 2, Text = "Hello Blazor Twitter Clone!", PublishedDate = DateTime.Now.AddHours(-2) }
        };
    }
}

