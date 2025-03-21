namespace MiniTwitAPI.Models
{
    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
<<<<<<< Updated upstream
        public string Password { get; set; }
        public string Password2 { get; set; }
=======
        [JsonPropertyName("pwd")] public string Password { get; set; }
>>>>>>> Stashed changes
    }
}
