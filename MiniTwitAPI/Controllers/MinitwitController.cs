using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniTwitAPI.Models;
using System.Text.RegularExpressions;

namespace MiniTwitAPI.Controllers
{
    [ApiController]
    public class MinitwitController : ControllerBase
    {
        private readonly ILogger<MinitwitController> _logger;
        private readonly AppDbContext _context;

        private readonly string filePath = "./latest_processed_sim_action_id.txt";

        public MinitwitController(AppDbContext context, ILogger<MinitwitController> logger)
        {
            _logger = logger;
            _context = context;
        }

        private IActionResult UpdateLatest(int latest)
        {
            if (latest != -1)
            {
                try
                {
                    System.IO.File.WriteAllText(filePath, latest.ToString());
                }
                catch (Exception ex)
                {
                    return NotFound("Failed to update latest value");
                }
            }

            return Ok(latest);
        }

        private string HashPassword(string password) => password;

        private IActionResult NotFromSimulator(string authorization)
        {
            if (authorization != "Basic c2ltdWxhdG9yOnN1cGVyX3NhZmUh")
            {
                return Forbid("You are not authorized to use this resource!");
            }

            return Ok();
        }


        [HttpGet("/latest")]
        public IActionResult GetLatest()
        {
            try
            {
                string content = System.IO.File.ReadAllText(filePath);
                if (int.TryParse(content, out int latestProcessedCommandId))
                {
                    return Ok(new { latest = latestProcessedCommandId });
                }
            }
            catch { }

            return Ok(new { latest = -1 });
        }

        [HttpPost("/register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest data)
        {
            // Update latest processed command ID
            var updateRequest = UpdateLatest(data.Latest);

            // Validate request data
            if (string.IsNullOrWhiteSpace(data.Username))
                return BadRequest("You have to enter a username");

            if (string.IsNullOrWhiteSpace(data.Email) || !Regex.IsMatch(data.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return BadRequest("You have to enter a valid email address");

            if (string.IsNullOrWhiteSpace(data.Password))
                return BadRequest("You have to enter a password");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == data.Username);
            if (user != null) return BadRequest("The username is already taken");

            await _context.Users.AddAsync(new User
            {
                Username = data.Username,
                Email = data.Email,
                PasswordHash = HashPassword(data.Password),
            });
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("/msgs")]
        public async Task<IActionResult> Messages([FromBody] MessagesRequest request)
        {
            UpdateLatest(request.Latest);

            var notFromSim = NotFromSimulator(request.Authorization);
            if (notFromSim is ForbidResult) return notFromSim;

            var messages = _context.Messages.Where(m => !m.Flagged).OrderByDescending(m => m.PublishedDate).Take(request.NumberOfMessages);

            return Ok(messages);
        }

        [HttpGet("/msgs/{username}")]
        public async Task<IActionResult> UserMessages(string username, [FromBody] MessagesRequest request)
        {
            UpdateLatest(request.Latest);

            var notFromSim = NotFromSimulator(request.Authorization);
            if (notFromSim is ForbidResult) return notFromSim;

            Console.WriteLine("User: " + username);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return NotFound("Couldn't find user");

            var messages = _context.Messages.Where(m => !m.Flagged && m.UserId == user.Id).OrderByDescending(m => m.PublishedDate).Take(request.NumberOfMessages);

            return Ok(messages);
        }

        [HttpPost("/msgs/{username}")]
        public async Task<IActionResult> PostMessage(string username, [FromBody] AddMessageRequest request)
        {
            UpdateLatest(request.Latest);

            var notFromSim = NotFromSimulator(request.Authorization);
            if (notFromSim is ForbidResult) return notFromSim;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return NotFound("Couldn't find user");

            await _context.Messages.AddAsync(new Message
            {
                UserId = user.Id,
                Text = request.Content,
                PublishedDate = DateTime.Now,
                Flagged = false,
            });

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("/fllws/{username}")]
        public async Task<IActionResult> Follow(string username, FollowRequest request)
        {
            UpdateLatest(request.Latest);

            var notFromSim = NotFromSimulator(request.Authorization);
            if (notFromSim is ForbidResult) return notFromSim;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return NotFound("Couldn't find user");

            if (request.Follow != null)
            {
                var followUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Follow);
                if (followUser == null) return NotFound("Couldn't find user to follow");

                var alreadyFollows = await _context.Followers.AnyAsync(f => f.UserId == user.Id && f.FollowsUserId == followUser.Id);
                if (!alreadyFollows)
                {
                    await _context.Followers.AddAsync(new Follower
                    {
                        UserId = user.Id,
                        FollowsUserId = followUser.Id,
                    });
                    await _context.SaveChangesAsync();
                    return NoContent();
                }
            }
            else if (request.Unfollow != null)
            {
                var unfollowUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Unfollow);
                if (unfollowUser == null) return NotFound("Couldn't find user to unfollow");

                var followData = await _context.Followers.FirstOrDefaultAsync(f => f.UserId == user.Id && f.FollowsUserId == unfollowUser.Id);
                if (followData != null)
                {
                    _context.Followers.Remove(followData);
                    await _context.SaveChangesAsync();
                    return NoContent();
                }
            }

            return BadRequest();
        }

        [HttpGet("/fllws/{username}")]
        public async Task<IActionResult> Followers(string username, FollowersRequest request)
        {
            UpdateLatest(request.Latest);

            var notFromSim = NotFromSimulator(request.Authorization);
            if (notFromSim is ForbidResult) return notFromSim;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return NotFound("Couldn't find user");

            var follows = _context.Followers.Where(f => f.UserId == user.Id).Take(request.NumberOfFollowers);
            return Ok(follows);
        }
    }
}