using MiniTwitClient.Controllers;
using MiniTwitClient.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

public class MiniTwitClientTests
{
    private readonly MinitwitController _controller;

    public MiniTwitClientTests()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7297")
        };

        _controller = new MinitwitController(httpClient);
    }

    private async Task<HttpResponseMessage> Register(string username, string password, string password2 = null, string email = null)
    {
        if (password2 == null) password2 = password;
        if (email == null) email = $"{username}@example.com";

        return await _controller.Register(new RegisterRequest()
        {
            Username = username,
            Password = password,
            Email = email
        });
    }

    private async Task<HttpResponseMessage> Login(string username, string password)
    {
        return await _controller.Login(new LoginRequest()
        {
            Username = username,
            Password = password
        });
    }

    private async Task<HttpResponseMessage> RegisterAndLogin(string username, string password)
    {
        await Register(username, password);
        return await Login(username, password);
    }

    private async Task<HttpResponseMessage> Logout()
    {
        throw new NotImplementedException();
    }

    private async Task<HttpResponseMessage> AddMessage(string username, string text)
    {
        var response = await _controller.PostMessage(username, new AddMessageRequest()
        {
            Content = text
        });
        var content = await response.Content.ReadAsStringAsync();

        if (!string.IsNullOrEmpty(text))
        {
            Assert.Contains("Your message was recorded", content);
        }

        return response;
    }

    private async Task<List<Message>> GetPublic()
    {
        return await _controller.GetPublicTimeline(new MessagesRequest());
    }

    private async Task<List<Message>> GetUserTimeline(string username)
    {
        return await _controller.GetUserTimeline(username, new MessagesRequest());
    }

    private async Task<List<Message>> GetTimeline(string username)
    {
        return await _controller.GetMyTimeline(new MessagesRequest(), username);
    }

    private async Task<HttpResponseMessage> Follow(string myUser, string followUser)
    {
        return await _controller.FollowChange(myUser, new FollowRequest()
        {
            Follow = followUser
        });
    }

    private async Task<HttpResponseMessage> Unfollow(string myUser, string followUser)
    {
        return await _controller.FollowChange(myUser, new FollowRequest()
        {
            Unfollow = followUser
        });
    }

    [Fact]
    public async Task TestRegister()
    {
        var rv = await Register("user1", "default");
        Assert.Contains("You were successfully registered and can login now", await rv.Content.ReadAsStringAsync());

        rv = await Register("user1", "default");
        Assert.Contains("The username is already taken", await rv.Content.ReadAsStringAsync());

        rv = await Register("", "default");
        Assert.Contains("You have to enter a username", await rv.Content.ReadAsStringAsync());

        rv = await Register("meh", "");
        Assert.Contains("You have to enter a password", await rv.Content.ReadAsStringAsync());

        rv = await Register("meh", "x", "y");
        Assert.Contains("The two passwords do not match", await rv.Content.ReadAsStringAsync());

        rv = await Register("meh", "foo", "foo", "broken");
        Assert.Contains("You have to enter a valid email address", await rv.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task TestLoginLogout()
    {
        var rv = await RegisterAndLogin("user1", "default");
        Assert.Contains("You were logged in", await rv.Content.ReadAsStringAsync());

        rv = await Logout();
        Assert.Contains("You were logged out", await rv.Content.ReadAsStringAsync());

        rv = await Login("user1", "wrongpassword");
        Assert.Contains("Invalid password", await rv.Content.ReadAsStringAsync());

        rv = await Login("user2", "wrongpassword");
        Assert.Contains("Invalid username", await rv.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task TestMessageRecording()
    {
        await RegisterAndLogin("foo", "default");
        await AddMessage("foo", "test message 1");
        await AddMessage("foo", "<test message 2>"); 
        var messages = (await GetTimeline("foo")).Select(m => m.Text);
        Assert.Contains("test message 1", messages);
        Assert.Contains("&lt;test message 2&gt;", messages);
    }

    [Fact]
    public async Task TestTimelines()
    {
        await RegisterAndLogin("foo", "default");
        await AddMessage("foo", "the message by foo");
        await Logout();
        await RegisterAndLogin("bar", "default");
        await AddMessage("bar", "the message by bar");

        var messages = (await GetPublic()).Select(m => m.Text);
        Assert.Contains("the message by foo", messages);
        Assert.Contains("the message by bar", messages);

        // bar's timeline should just show bar's message
        messages = (await GetTimeline("bar")).Select(m => m.Text);
        Assert.DoesNotContain("the message by foo", messages);
        Assert.Contains("the message by bar", messages);

        // now let's follow foo
        var rv = await Follow("bar", "foo");
        var content = await rv.Content.ReadAsStringAsync();
        Assert.Contains("You are now following &#34;foo&#34;", content);

        // we should now see foo's message
        messages = (await GetTimeline("bar")).Select(m => m.Text);
        Assert.Contains("the message by foo", messages);
        Assert.Contains("the message by bar", messages);

        // but on the user's page we only want the user's message
        messages = (await GetUserTimeline("bar")).Select(m => m.Text);
        Assert.DoesNotContain("the message by foo", messages);
        Assert.Contains("the message by bar", messages);

        messages = (await GetUserTimeline("foo")).Select(m => m.Text);
        Assert.Contains("the message by foo", messages);
        Assert.DoesNotContain("the message by bar", messages);

        // now unfollow and check if that worked
        rv = await Unfollow("bar", "foo");
        content = await rv.Content.ReadAsStringAsync();
        Assert.Contains("You are no longer following &#34;foo&#34;", content);

        messages = (await GetTimeline("bar")).Select(m => m.Text);
        Assert.DoesNotContain("the message by foo", messages);
        Assert.Contains("the message by bar", messages);
    }
}
