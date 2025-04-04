using Microsoft.AspNetCore.Mvc;
using MiniTwitClient.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MiniTwitClient.Controllers
{
    public class MinitwitController
    {
        private readonly HttpClient _httpClient;
        private static string _auth = "c2ltdWxhdG9yOnN1cGVyX3NhZmUh";

        // Injecting HttpClient into the controller
        public MinitwitController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", _auth);
            Console.WriteLine("Controller endpoint: " + httpClient.BaseAddress);
        }

        public async Task<List<Message>> GetPublicTimeline(MessagesRequest request)
        {
			var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/msgs/?no={request.NumberOfMessages}&latest={request.Latest}");
			
			if (response.IsSuccessStatusCode)
			{
				var messages = await response.Content.ReadFromJsonAsync<List<Message>>();
				return messages;
			}
			else
			{
				Console.WriteLine($"Error: {response.StatusCode}");
				// Handle error based on status code
				return new List<Message>();
			}
		}

		public async Task<List<Message>> GetUserTimeline(string username, MessagesRequest request)
		{
			var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/msgs/{username}?no={request.NumberOfMessages}&latest={request.Latest}");

			if (response.IsSuccessStatusCode)
			{
				var messages = await response.Content.ReadFromJsonAsync<List<Message>>();
				return messages;
			}
			else
			{
				Console.WriteLine($"Error: {response.StatusCode}");
				// Handle error based on status code
				return null;
			}
		}









		//// Used to clear database for testing - might need to do something else
		//[HttpGet("/drop/all")]
		//public async Task<IActionResult> DropUsers()
		//{
		//    var users = _context.Users.ToList();
		//    _context.Users.RemoveRange(users);

		//    var messages = _context.Messages.ToList();
		//    _context.Messages.RemoveRange(messages);

		//    var followers = _context.Followers.ToList();
		//    _context.Followers.RemoveRange(followers);

		//    await _context.SaveChangesAsync();

		//    return Ok("All users have been cleared.");
		//}

		//// Get timeline for user, or public timeline if not logged in
		//[HttpGet("")]
		//public async Task<IActionResult> Timeline()
		//{
		//    var sessionUsername = HttpContext.Session.GetString("Username");

		//    if (sessionUsername == null)
		//    {
		//        return Ok(GetPublicMessages());
		//    }

		//    var userId = _context.Users.FirstOrDefault(u => u.Username == sessionUsername).Id;

		//    var followedUserIds = _context.Followers
		//        .Where(f => f.UserId == userId)
		//        .Select(f => f.FollowsUserId)
		//        .ToList();

		//    var messages = _context.Messages
		//        .Where(m => m.UserId == userId || followedUserIds.Contains(m.UserId))
		//        .Select(m => new Message
		//        {
		//            Id = m.Id,
		//            UserId = m.UserId,
		//            Text = HttpUtility.HtmlEncode(m.Text),
		//            PublishedDate = m.PublishedDate,
		//            Flagged = m.Flagged
		//        });

		//    return Ok(messages);
		//}

		//// Get public timeline
		//[HttpGet("/public")]
		//public async Task<IActionResult> PublicTimeline()
		//{
		//    var messages = GetPublicMessages();

		//    return Ok(messages);
		//}

		//// Do login request
		//[HttpPost("/login")]
		//public async Task<IActionResult> Login([FromBody] LoginRequest model)
		//{
		//    var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);

		//    if (user == null)
		//    {
		//        return NotFound("Invalid username");
		//    }

		//    if (user.PasswordHash == model.Password)
		//    {
		//        HttpContext.Session.SetString("Username", model.Username);
		//        return Ok("You were logged in");
		//    }
		//    else return BadRequest("Invalid password");
		//}

		//// Logout
		//[HttpGet("/logout")]
		//public async Task<IActionResult> Logout()
		//{
		//    // Idk
		//    HttpContext.Session.SetString("Username", "");
		//    return Ok("You were logged out");
		//}

		//// Register new user in DB
		//[HttpPost("/register")]
		//public async Task<IActionResult> RegisterUser([FromBody] RegisterRequest model)
		//{
		//    if (string.IsNullOrEmpty(model.Username))
		//    {
		//        return BadRequest("You have to enter a username");
		//    }

		//    if (string.IsNullOrEmpty(model.Email) || !model.Email.Contains("@"))
		//    {
		//        return BadRequest("You have to enter a valid email address");
		//    }

		//    if (string.IsNullOrEmpty(model.Password))
		//    {
		//        return BadRequest("You have to enter a password");
		//    }

		//    if (model.Password != model.Password2)
		//    {
		//        return BadRequest("The two passwords do not match");
		//    }

		//    // Check if the username already exists
		//    var existingUser = _context.Users.FirstOrDefault(u => u.Username == model.Username);

		//    if (existingUser != null)
		//    {
		//        return BadRequest("The username is already taken");
		//    }

		//    // Create the new user
		//    var newUser = new User
		//    {
		//        Username = model.Username,
		//        Email = model.Email,
		//        PasswordHash = model.Password,
		//    };

		//    _context.Users.Add(newUser);
		//    await _context.SaveChangesAsync();

		//    return Ok("You were successfully registered and can login now");
		//}

		//// Register new message based on logged in user in DB
		//[HttpPost("/add_message")]
		//public async Task<IActionResult> AddMessage([FromBody] AddMessageRequest model)
		//{
		//    var sessionUsername = HttpContext.Session.GetString("Username");
		//    if (sessionUsername == null)
		//    {
		//        return BadRequest("You need to be logge in to create a message");
		//    }

		//    var user = _context.Users.FirstOrDefault(u => u.Username == sessionUsername);

		//    var message = new Message
		//    {
		//        Text = model.Text,
		//        UserId = user.Id,
		//        PublishedDate = DateTime.Now,
		//        Flagged = false,
		//    };

		//    _context.Messages.Add(message);
		//    await _context.SaveChangesAsync();

		//    return Ok("Your message was recorded");
		//}

		//// Get timeline for user
		//[HttpGet("{username}")]
		//public async Task<IActionResult> GetUserTimeline(string username)
		//{
		//    var profileUser = _context.Users.FirstOrDefault(u => u.Username == username);

		//    if (profileUser == null)
		//    {
		//        return NotFound();
		//    }

		//    bool followed = false;
		//    var sessionUsername = HttpContext.Session.GetString("Username");

		//    if (sessionUsername == null)
		//    {
		//        return BadRequest("You aren't logged in");
		//    }

		//    var user = _context.Users.FirstOrDefault(u => u.Username == sessionUsername);
		//    if (user != null) followed = _context.Followers.Any(f => f.UserId == user.Id && f.FollowsUserId == profileUser.Id);

		//    var messages = _context.Messages
		//        .Where(m => m.UserId == profileUser.Id)
		//        .OrderByDescending(m => m.PublishedDate)
		//        .ToList();

		//    return Ok(new
		//    {
		//        ProfileUser = profileUser,
		//        Messages = messages,
		//        Followed = followed
		//    });
		//}

		//[HttpGet("{username}/follow")]
		//public async Task<IActionResult> FollowUser(string username)
		//{
		//    var sessionUsername = HttpContext.Session.GetString("Username");
		//    if (sessionUsername == null)
		//    {
		//        return Unauthorized("You need to be logged in to follow a user");
		//    }

		//    var user = _context.Users.FirstOrDefault(u => u.Username == sessionUsername);

		//    var followUser = _context.Users.FirstOrDefault(u => u.Username == username);

		//    if (followUser == null)
		//    {
		//        return NotFound("Couldn't find user to follow");
		//    }

		//    var followModel = new Follower
		//    {
		//        UserId = user.Id,
		//        FollowsUserId = followUser.Id
		//    };

		//    _context.Followers.Add(followModel);
		//    await _context.SaveChangesAsync();

		//    return Ok($"You are now following {HttpUtility.HtmlEncode($"\'{followUser.Username}\'")}");
		//}

		//[HttpGet("{username}/unfollow")]
		//public async Task<IActionResult> UnFollowUser(string username)
		//{
		//    var sessionUsername = HttpContext.Session.GetString("Username");
		//    if (sessionUsername == null)
		//    {
		//        return Unauthorized("You need to be logged in to unfollow a user");
		//    }

		//    var user = _context.Users.FirstOrDefault(u => u.Username == sessionUsername);

		//    var followUser = _context.Users.FirstOrDefault(u => u.Username == username);

		//    if (followUser == null)
		//    {
		//        return NotFound("Couldn't find user to unfollow");
		//    }

		//    var followerEntity = _context.Followers.Single(f => f.UserId == user.Id && f.FollowsUserId == followUser.Id);
		//    _context.Followers.Remove(followerEntity);
		//    await _context.SaveChangesAsync();

		//    return Ok($"You are no longer following {HttpUtility.HtmlEncode($"\'{followUser.Username}\'")}");
		//}


		// Function used to retrieve all public messages (to avoid code duplication)
		//private IQueryable<Message> GetPublicMessages()
		//{
		//    return _context.Messages
		//        .OrderByDescending(m => m.PublishedDate)
		//        .Select(m => new Message
		//        {
		//            Id = m.Id,
		//            UserId = m.UserId,
		//            Text = HttpUtility.HtmlEncode(m.Text),
		//            PublishedDate = m.PublishedDate,
		//            Flagged = m.Flagged
		//        });
		//}

	}
}
