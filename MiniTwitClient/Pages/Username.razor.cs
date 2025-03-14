using Microsoft.AspNetCore.Components;
using MiniTwitClient.Models;

namespace MiniTwitClient.Pages
{
    public partial class Username : ComponentBase
    {
        [Parameter]
        public int userId { get; set; }

        // Sample data. Replace with actual data retrieval from your backend.
        private List<Message> Messages = new List<Message>
    {
        new Message { UserId = 1, Text = "Tweet from User 1", PublishedDate = DateTime.Now.AddMinutes(-10) },
        new Message { UserId = 2, Text = "Tweet from User 2", PublishedDate = DateTime.Now.AddMinutes(-20) },
        new Message { UserId = 1, Text = "Another tweet from User 1", PublishedDate = DateTime.Now.AddMinutes(-30) },
    };
    }
}
