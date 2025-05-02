using System.ComponentModel.DataAnnotations;

namespace MiniTwitAPI.Entities
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Text { get; set; }

        public DateTime? PublishedDate { get; set; }

        public bool Flagged { get; set; }
    }
}
