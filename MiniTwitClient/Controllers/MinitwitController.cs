using Microsoft.AspNetCore.Mvc;
using MiniTwitClient.Models;
using MiniTwitClient.Pages;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MiniTwitClient.Controllers
{
    public class MinitwitController
    {
        private readonly HttpClient _httpClient;
        private static string _auth = "c2ltdWxhdG9yOnN1cGVyX3NhZmUh";

        public MinitwitController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", _auth);
            Console.WriteLine("Controller endpoint: " + httpClient.BaseAddress);
        }

        public async Task<List<Message>> GetPublicTimeline(MessagesRequest request)
        {
			var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}msgs/?no={request.NumberOfMessages}");
			
			if (response.IsSuccessStatusCode)
			{
				var messages = await response.Content.ReadFromJsonAsync<List<Message>>();
                return messages;
			}
			else
			{
				Console.WriteLine($"Error: {response.StatusCode}");
                // Handle error based on status code
                return [];
			}
		}

        public async Task<List<Message>> GetMyTimeline(MessagesRequest request)
        {
            var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}");

            if (response.IsSuccessStatusCode)
            {
                var messages = await response.Content.ReadFromJsonAsync<List<Message>>();
                return messages;
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                // Handle error based on status code
                return [];
            }
        }

        public async Task<List<Message>> GetUserTimeline(string username, MessagesRequest request)
		{
			var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}msgs/{username}?no={request.NumberOfMessages}");

			if (response.IsSuccessStatusCode)
			{
				var messages = await response.Content.ReadFromJsonAsync<List<Message>>();
				return messages;
			}
			else
			{
				Console.WriteLine($"Error: {response.StatusCode}");
				// Handle error based on status code
				return [];
			}
		}

        public async Task<HttpResponseMessage> Register(RegisterRequest data)
        {
            var jsonContent = JsonSerializer.Serialize(data);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            return await _httpClient.PostAsync($"{_httpClient.BaseAddress}register", content);
        }

        public async Task<HttpResponseMessage> Login(LoginRequest request)
		{
            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            return await _httpClient.PostAsync($"{_httpClient.BaseAddress}login", content);
        }

		public async Task<HttpResponseMessage> PostMessage(AddMessageRequest request)
		{
            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            return await _httpClient.PostAsync($"{_httpClient.BaseAddress}msgs", content);
        }

		public async Task<HttpResponseMessage> FollowChange(FollowRequest request)
		{
            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            return await _httpClient.PostAsync($"{_httpClient.BaseAddress}fllws", content);
        }

        public async Task<bool> Follows(string toFollow)
        {
            var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}fllws/{toFollow}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<bool>();
            }
            else return false;
        }
    }
}
