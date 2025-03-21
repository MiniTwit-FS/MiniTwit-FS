using System.ComponentModel.DataAnnotations;

namespace MiniTwitClient.Models
{
    public class Message
    {
        public string Text { get; set; }

        public DateTime? PublishedDate { get; set; }

        public bool Flagged { get; set; }

        public string Username { get; set; }
    }
}
