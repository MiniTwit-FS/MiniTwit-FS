using System.Text.Json.Serialization;

namespace MiniTwitAPI.Models
{
    public class FollowRequest
    {
        public string? Follow { get; set; }
        public string? Unfollow { get; set; }
    }
}
