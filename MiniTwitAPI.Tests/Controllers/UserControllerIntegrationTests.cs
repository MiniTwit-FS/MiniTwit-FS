using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using MiniTwitAPI.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace MiniTwitAPI.Controllers
{
    public class UserControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public UserControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task RegisterUser_ShouldReturnOk()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "testuser",
                Email = "testuser@example.com",
                Password = "password",
                Password2 = "password"
            };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/register", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Equal("You were successfully registered and can login now", responseString);
        }

        [Fact]
        public async Task GetUserMessages_ReturnsOK()
        {
            // Arrange
            var username = "testuser1";

            // Act
            var response = await _client.GetAsync($"/{username}");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Equal("You were successfully registered and can login now", responseString);
        }
    }
}
