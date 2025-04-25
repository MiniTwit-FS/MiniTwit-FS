using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniTwitAPI.DTOs;
using MiniTwitAPI.Extentions;
using MiniTwitAPI.Models;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.AspNetCore.SignalR;
using MiniTwitAPI.Hubs;

namespace MiniTwitAPI.Controllers
{
    [ApiController]
    public class MinitwitController : ControllerBase
    {
        private readonly ILogger<MinitwitController> _logger;
        private readonly AppDbContext _context;
        private readonly LogHub _logHub;

        private readonly string filePath = "./latest_processed_sim_action_id.txt";
        private readonly string logFilePath = "./logs/minitwit-api-log";

        public MinitwitController(AppDbContext context, ILogger<MinitwitController> logger, LogHub logHub)
        {
            _logger = logger;
            _context = context;
            _logHub = logHub;
        }

        private IActionResult UpdateLatest(int latest)
        {
            if (latest != -1)
            {
                try
                {
                    _logger.RLogDebug($"Updating latest processed action ID to: {latest}", _logHub);
                    System.IO.File.WriteAllText(filePath, latest.ToString());
                }
                catch (Exception ex)
                {
                    _logger.RLogError(ex, $"Failed to update latest value to {latest}", _logHub);
                    return NotFound("Failed to update latest value");
                }
            }

            return Ok(latest);
        }

        private string HashPassword(string password) => password.Sha256Hash();

        private IActionResult NotFromSimulator(string authorization)
        {
            if (authorization != "Basic c2ltdWxhdG9yOnN1cGVyX3NhZmUh")
            {
                _logger.RLogWarning("Unauthorized access attempt with invalid authorization header", _logHub);
                return Forbid("You are not authorized to use this resource!");
            }

            return Ok();
        }

        [HttpGet("/latest")]
        public IActionResult GetLatest()
        {
            _logger.RLogInformation("GetLatest endpoint called", _logHub);
            try
            {
                string content = System.IO.File.ReadAllText(filePath);
                if (int.TryParse(content, out int latestProcessedCommandId))
                {
                    _logger.RLogDebug($"Retrieved latest processed command ID: {latestProcessedCommandId}", _logHub);
                    return Ok(new { latest = latestProcessedCommandId });
                }
            }
            catch (Exception ex)
            {
                _logger.RLogError(ex, "Error reading latest processed command ID file", _logHub);
            }

            _logger.RLogDebug("No latest ID found, returning default value -1", _logHub);
            return Ok(new { latest = -1 });
        }

        [HttpPost("/register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest data, [FromHeader] string authorization, [FromQuery] int latest = -1)
        {
            _logger.RLogInformation($"Register endpoint called for username: {data?.Username}", _logHub);

            var notFromSim = NotFromSimulator(authorization);
            if (notFromSim is ForbidResult) return notFromSim;

            // Update latest processed command ID
            var updateRequest = UpdateLatest(latest);

            // Validate request data
            if (string.IsNullOrWhiteSpace(data.Username))
            {
                _logger.RLogWarning("Registration attempt with empty username", _logHub);
                return BadRequest("You have to enter a username");
            }

            if (string.IsNullOrWhiteSpace(data.Email) || !Regex.IsMatch(data.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                _logger.RLogWarning($"Registration attempt with invalid email: {data.Email}", _logHub);
                return BadRequest("You have to enter a valid email address");
            }

            if (string.IsNullOrWhiteSpace(data.Password))
            {
                _logger.RLogWarning($"Registration attempt with empty password for user: {data.Username}", _logHub);
                return BadRequest("You have to enter a password");
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == data.Username);
                if (user != null)
                {
                    _logger.RLogWarning($"Registration attempt with existing username: {data.Username}", _logHub);
                    return BadRequest("The username is already taken");
                }

                await _context.Users.AddAsync(new User
                {
                    Username = data.Username,
                    Email = data.Email,
                    PasswordHash = HashPassword(data.Password)
                });
                await _context.SaveChangesAsync();

                _logger.LogInformation("User successfully registered: {Username}", data.Username);
                return Ok("You were successfully registered and can login now");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user: {Username}", data.Username);
                return StatusCode(500, "An error occurred while registering the user");
            }
        }

        [HttpGet("/msgs")]
        public async Task<IActionResult> Messages([FromHeader] string authorization, [FromQuery] int latest = -1, [FromQuery] int no = 100)
        {
            _logger.RLogInformation($"Messages endpoint called requesting {no} messages", _logHub);
            var notFromSim = NotFromSimulator(authorization);
            if (notFromSim is ForbidResult) return notFromSim;

            UpdateLatest(latest);

            try
            {
                var messages = _context.Messages
                .Where(m => !m.Flagged)
                .OrderByDescending(m => m.PublishedDate)
                .Take(no)
                .Select(m => new MessageDTO
                {
                    Id = m.Id,
                    Text = m.Text,
                    PublishedDate = m.PublishedDate,
                    Flagged = m.Flagged,
                    Username = _context.Users.First(u => u.Id == m.UserId).Username
                })
                .ToList();

                _logger.RLogDebug("Retrieved messages for global timeline", _logHub);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages for global timeline");
                return NotFound("An error occurred while retrieving messages");
            }
        }

        [HttpGet("")]
        public async Task<IActionResult> Timeline([FromHeader] string username, [FromHeader] string authorization, [FromQuery] int latest = -1, [FromQuery] int no = 100)
        {
            _logger.LogInformation("Timeline endpoint called requesting {Count} messages", no);
            var notFromSim = NotFromSimulator(authorization);
            if (notFromSim is ForbidResult) return notFromSim;

            UpdateLatest(latest);

            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Username == username);

                if (user == null) return BadRequest("Couldn't find user");

                var followersIds = _context.Followers.Where(f => f.UserId == user.Id).Select(f => f.FollowsUserId);

                var messages = _context.Messages
                    .Where(m => m.UserId == user.Id || followersIds.Contains(m.UserId))
                    .OrderByDescending(m => m.PublishedDate)
                    .Take(no)
                    .Select(m => new MessageDTO
                    {
                        Id = m.Id,
                        Text = m.Text,
                        PublishedDate = m.PublishedDate,
                        Flagged = m.Flagged,
                        Username = _context.Users.First(u => u.Id == m.UserId).Username
                    })
                    .ToList();


                _logger.LogDebug("Retrieved messages for personal timeline");
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages for personal timeline");
                return NotFound("An error occurred while retrieving messages");
            }
        }

        [HttpGet("/msgs/{username}")]
        public async Task<IActionResult> UserMessages(string username,
            [FromHeader] string authorization, [FromQuery] int latest = -1, [FromQuery] int no = 100)
        {
            _logger.LogInformation("UserMessages endpoint called for user: {Username}, requesting {Count} messages",
                username, no);

            var notFromSim = NotFromSimulator(authorization);
            if (notFromSim is ForbidResult) return notFromSim;

            UpdateLatest(latest);

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {Username}", username);
                    return NotFound("Couldn't find user");
                }

                var messages = _context.Messages
                .Where(m => !m.Flagged && m.UserId == user.Id)
                .OrderByDescending(m => m.PublishedDate)
                .Take(no)
                .Select(m => new MessageDTO
                {
                    Id = m.Id,
                    Text = m.Text,
                    PublishedDate = m.PublishedDate,
                    Flagged = m.Flagged,
                    Username = username
                })
                .ToList();

                _logger.LogDebug("Retrieved {Count} messages for user {Username}", no, username);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages for user {Username}", username);
                return StatusCode(500, "An error occurred while retrieving user messages");
            }
        }

        [HttpPost("/msgs")]
        public async Task<IActionResult> PostMessage([FromHeader] string username,
            [FromBody] AddMessageRequest request, [FromHeader] string authorization, [FromQuery] int latest = -1)
        {
            _logger.LogInformation("PostMessage endpoint called for user: {Username}", username);

            var notFromSim = NotFromSimulator(authorization);
            if (notFromSim is ForbidResult) return notFromSim;

            UpdateLatest(latest);

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                {
                    _logger.LogWarning("User not found when posting message: {Username}", username);
                    return NotFound("Couldn't find user");
                }

                var msg = new Message
                {
                    UserId = user.Id,
                    Text = request.Content,
                    PublishedDate = DateTime.UtcNow,
                    Flagged = false,
                };

                await _context.Messages.AddAsync(msg);

                await _context.SaveChangesAsync();
                _logger.LogInformation("Message posted successfully for user: {Username}", username);
                return Ok(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error posting message for user {Username}", username);
                return StatusCode(500, "An error occurred while posting the message");
            }
        }

        [HttpPost("/fllws")]
        public async Task<IActionResult> Follow([FromHeader] string username,
            [FromBody] FollowRequest request, [FromHeader] string authorization, [FromQuery] int latest = -1)
        {
            _logger.LogInformation("Follow endpoint called for user: {Username}", username);
            if (request.Follow != null)
            {
                _logger.LogDebug("User {Username} attempting to follow {TargetUsername}", username, request.Follow);
            }
            else if (request.Unfollow != null)
            {
                _logger.LogDebug("User {Username} attempting to unfollow {TargetUsername}", username, request.Unfollow);
            }

            UpdateLatest(latest);

            var notFromSim = NotFromSimulator(authorization);
            if (notFromSim is ForbidResult) return notFromSim;

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                {
                    _logger.LogWarning("User not found in follow request: {Username}", username);
                    return NotFound("Couldn't find user");
                }

                if (request.Follow != null)
                {
                    var followUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Follow);
                    if (followUser == null)
                    {
                        _logger.LogWarning("Follow target not found: {TargetUsername}", request.Follow);
                        return NotFound("Couldn't find user to follow");
                    }

                    if (user == followUser)
                    {
                        return BadRequest("Can't follow yourself");
                    }

                    var alreadyFollows = await _context.Followers.AnyAsync(f => f.UserId == user.Id && f.FollowsUserId == followUser.Id);
                    if (!alreadyFollows)
                    {
                        await _context.Followers.AddAsync(new Follower
                        {
                            UserId = user.Id,
                            FollowsUserId = followUser.Id,
                        });
                        await _context.SaveChangesAsync();
                        _logger.RLogInformation($"User {username} now follows {request.Follow}", _logHub);
                        return NoContent();
                    }
                    else
                    {
                        _logger.LogDebug("User {Username} already follows {TargetUsername}", username, request.Follow);
                    }
                }
                else if (request.Unfollow != null)
                {
                    var unfollowUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Unfollow);
                    if (unfollowUser == null)
                    {
                        _logger.LogWarning("Unfollow target not found: {TargetUsername}", request.Unfollow);
                        return NotFound("Couldn't find user to unfollow");
                    }

                    var followData = await _context.Followers.FirstOrDefaultAsync(f => f.UserId == user.Id && f.FollowsUserId == unfollowUser.Id);
                    if (followData != null)
                    {
                        _context.Followers.Remove(followData);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("User {Username} unfollowed {TargetUsername}", username, request.Unfollow);
                        return NoContent();
                    }
                    else
                    {
                        _logger.LogDebug("User {Username} was not following {TargetUsername}", username, request.Unfollow);
                    }
                }

                _logger.LogWarning("Invalid follow/unfollow request from user {Username}", username);
                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing follow/unfollow request for user {Username}", username);
                return StatusCode(500, "An error occurred while processing follow request");
            }
        }

        [HttpGet("/fllws/{followUser}")]
        public async Task<IActionResult> Follows([FromHeader] string username, string followUser)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                {
                    _logger.LogWarning("User not found in follows request: {Username}", username);
                    return NotFound("Couldn't find user");
                }

                var userToFollow = await _context.Users.FirstOrDefaultAsync(u => u.Username == followUser);
                if (userToFollow == null)
                {
                    _logger.LogWarning("User not found in follows request: {Username}", username);
                    return NotFound("Couldn't find user");
                }

                var follows = _context.Followers
                    .Any(f => f.UserId == user.Id && f.FollowsUserId == userToFollow.Id);

                return Ok(follows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving followers for user {Username}", username);
                return StatusCode(500, "An error occurred while retrieving followers");
            }
        }

        [HttpPost("/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                return NotFound("Invalid username");
            }
            else if (user.PasswordHash != HashPassword(request.Password))
            {
                return BadRequest("Invalid password");
            }
            else return Ok("You were logged in");
        }

        [HttpGet("/logout")]
        public async Task<IActionResult> Logout([FromHeader] string username)
        {
            if (username != null)
            {
                return Ok("You were logged out");
            }
            else
            {
                return BadRequest("You are not logged in");
            }
        }

        [HttpGet("/logs")]
        public IActionResult GetLogs(string date, int page = 1, int pageSize = 100)
        {
            try
            {
                var logs = GetLogsFromFile(date, page, pageSize);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error reading log file", details = ex.Message });
            }
        }

        private List<string> GetLogsFromFile(string date, int page, int pageSize)
        {
            var lines = new List<string>();
            try
            {
                // Open the file with exclusive read access, using a FileStream to handle concurrency
                using (var fileStream = new FileStream(logFilePath + date + ".log", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var streamReader = new StreamReader(fileStream))
                {
                    var totalLines = new List<string>();
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        totalLines.Add(line);
                    }

                    // Reverse to get the most recent log entries first
                    totalLines.Reverse();

                    // Paginate the results
                    var paginatedLines = totalLines.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                    return paginatedLines;
                }
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error reading log file");
                throw new Exception("Error reading the log file. It might be locked by another process.");
            }
        }


        // Helper method to fetch the next set of logs when the user scrolls up
        [HttpGet("/more-logs")]
        public IActionResult FetchMoreLogs(string date, int currentPage, int pageSize = 100)
        {
            try
            {
                var nextPage = currentPage + 1;
                var moreLogs = GetLogsFromFile(date, nextPage, pageSize);
                return Ok(moreLogs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching more logs", details = ex.Message });
            }
        }
    }
}
