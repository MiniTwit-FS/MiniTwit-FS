namespace MiniTwitClient.Authentication
{
    public class UserState
    {
        public string? Username { get; private set; }
        public bool IsLoggedIn => !string.IsNullOrEmpty(Username);

        public void LogIn(string username)
        {
            Username = username;
        }

        public void LogOut()
        {
            Username = null;
        }
    }
}
