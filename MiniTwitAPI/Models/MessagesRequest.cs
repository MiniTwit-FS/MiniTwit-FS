using System.Text.Json.Serialization;

namespace MiniTwitAPI.Models
{
    public class MessagesRequest
    {
        public string Authorization { get; set; }
        public int Latest { get; set; } = -1;

        [JsonPropertyName("no")]
        public int NumberOfMessages { get; set; } = 100;
    }
}
