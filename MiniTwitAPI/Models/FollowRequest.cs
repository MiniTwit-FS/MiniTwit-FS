using System.Text.Json.Serialization;

namespace MiniTwitAPI.Models
{
    public class FollowRequest
    {
        public string Authorization { get; set; }
        public int Latest { get; set; } = -1;

        public string? Follow { get; set; }
        public string? Unfollow { get; set; }
    }
}
