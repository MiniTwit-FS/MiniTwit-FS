namespace MiniTwitAPI.DTOs
{
    public class MessageDTO
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime? PublishedDate { get; set; }
        public bool Flagged { get; set; }
        public string Username { get; set; } = string.Empty;
    }

}
