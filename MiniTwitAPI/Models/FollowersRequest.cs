using System.Text.Json.Serialization;

namespace MiniTwitAPI.Models
{
    public class FollowersRequest
    {
        public string Authorization { get; set; }
        public int Latest { get; set; } = -1;

        [JsonPropertyName("no")]
        public int NumberOfFollowers { get; set; } = 100;
    }
}
