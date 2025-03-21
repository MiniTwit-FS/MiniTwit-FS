using System.Text.Json.Serialization;

namespace MiniTwitClient.Models
{
    public class RegisterRequest
    {
        public int Latest { get; set; } = -1;

        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
