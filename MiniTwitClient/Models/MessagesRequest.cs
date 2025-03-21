using System.Text.Json.Serialization;

namespace MiniTwitClient.Models
{
    public class MessagesRequest
    {
        public int Latest { get; set; } = -1;

        [JsonPropertyName("no")]
        public int NumberOfMessages { get; set; } = 100;
    }
}
