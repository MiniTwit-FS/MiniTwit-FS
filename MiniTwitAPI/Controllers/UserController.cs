using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniTwitAPI.Models;
using System.Security.Claims;

namespace MiniTwitAPI.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly AppDbContext _context;

        public UserController(AppDbContext context, ILogger<UserController> logger)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("/drop/all")]
        public async Task<IActionResult> DropUsers()
        {
            var users = _context.Users.ToList();
            _context.Users.RemoveRange(users);

            var messages = _context.Messages.ToList();
            _context.Messages.RemoveRange(messages);

            var followers = _context.Followers.ToList();
            _context.Followers.RemoveRange(followers);

            await _context.SaveChangesAsync();

            return Ok("All users have been cleared.");
        }

        [HttpPost("/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);

            if (user == null)
            {
                return NotFound("Invalid username");
            }

            if (user.PasswordHash == model.Password)
            {
                HttpContext.Session.SetString("Username", model.Username);
                Thread.Sleep(200);
                return Ok("You were logged in");
            }
            else return BadRequest("Invalid password");
        }

        [HttpGet("/logout")]
        public IActionResult Logout()
        {
            // Idk
            HttpContext.Session.SetString("Username", "");
            return Ok("You were logged out");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (string.IsNullOrEmpty(model.Username))
            {
                return BadRequest("You have to enter a username");
            }

            if (string.IsNullOrEmpty(model.Email) || !model.Email.Contains("@"))
            {
                return BadRequest("You have to enter a valid email address");
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("You have to enter a password");
            }

            if (model.Password != model.Password2)
            {
                return BadRequest("The two passwords do not match");
            }

            // Check if the username already exists
            var existingUser = _context.Users.FirstOrDefault(u => u.Username == model.Username);

            if (existingUser != null)
            {
                return BadRequest("The username is already taken");
            }

            // Create the new user
            var newUser = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = model.Password,
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok("You were successfully registered and can login now");
        }

        [HttpPost("/add_message")]
        public async Task<IActionResult> AddMessage([FromBody] AddMessageRequest model)
        {
            var sessionUsername = HttpContext.Session.GetString("Username");

            var userId = _context.Users.FirstOrDefault(u => u.Username == sessionUsername).Id;

            var message = new Message 
            {
            Text = model.Text,
            UserId = userId

            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return Ok("Your message was recorded");
        }

        [HttpGet("/")]
        public async Task<IActionResult> Timeline([FromBody] AddMessageRequest model)
        {
            var sessionUsername = HttpContext.Session.GetString("Username");

            var userId = _context.Users.FirstOrDefault(u => u.Username == sessionUsername).Id;

            var messages = _context.Messages.Where(m => m.UserId == userId);

            return Ok(messages);
        }

        //[HttpGet("{username}")]
        //public IActionResult Get(string username)
        //{
        //    var profileUser = _context.Users.FirstOrDefault(u => u.Username == username);

        //    if (profileUser == null)
        //    {
        //        return NotFound();
        //    }

        //    // Check if the current user is following the profile user
        //    bool followed = false;
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value); // Assuming you have user ID as a claim
        //        followed = _context.Followers
        //            .Any(f => f.UserId == currentUserId && f.FollowsUserId == profileUser.Id);
        //    }

        //    // Retrieve messages from the profile user
        //    var messages = _context.Messages
        //        .Where(m => m.UserId == profileUser.Id)
        //        .OrderByDescending(m => m.PublishedDate)
        //        .Take(10) // Limit to PER_PAGE equivalent
        //        .ToList();

        //    // You can return the data in JSON format or HTML
        //    return Ok(new
        //    {
        //        ProfileUser = profileUser,
        //        Messages = messages,
        //        Followed = followed
        //    });
        //}
    }
}
