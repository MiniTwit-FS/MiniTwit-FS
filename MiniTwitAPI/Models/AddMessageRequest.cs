namespace MiniTwitAPI.Models
{
    public class AddMessageRequest
    {
        public string Authorization { get; set; }
        public int Latest { get; set; } = -1;

        public string Content { get; set; }
    }
}
