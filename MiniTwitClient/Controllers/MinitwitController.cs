using Microsoft.AspNetCore.Mvc;
using MiniTwitClient.Models;
using MiniTwitClient.Pages;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MiniTwitClient.Controllers
{
    public class MinitwitController
    {
        private readonly HttpClient _httpClient;
        private static string _auth = "c2ltdWxhdG9yOnN1cGVyX3NhZmUh";

        public Uri address;

        public MinitwitController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", _auth);
            address = httpClient.BaseAddress;
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
            var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}?no={request.NumberOfMessages}");

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
            var jsonContent = JsonSerializer.Serialize(new RegisterRequest {
                Username = data.Username,
                Email = data.Email,
                Password = data.Password
            });
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            return await _httpClient.PostAsync($"{_httpClient.BaseAddress}register", content);
        }

        public async Task<HttpResponseMessage> Login(LoginRequest request)
		{
            var jsonContent = JsonSerializer.Serialize(new LoginRequest { Username = request.Username, Password = request.Password});
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            return await _httpClient.PostAsync($"{_httpClient.BaseAddress}login", content);
        }

        public async Task<HttpResponseMessage> Logout()
        {
            return await _httpClient.GetAsync($"{_httpClient.BaseAddress}logout");
        }

		public async Task<HttpResponseMessage> PostMessage(AddMessageRequest request, string username)
		{
            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            return await _httpClient.PostAsync($"{_httpClient.BaseAddress}msgs/{username}", content);
        }


        public async Task<HttpResponseMessage> FollowChange(FollowRequest request, string username)
		{
            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            return await _httpClient.PostAsync($"{_httpClient.BaseAddress}fllws/{username}", content);
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

        public async Task<List<string>> GetLogs(string date, int currentPage, int pageSize)
        {
            var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}logs?date={date}&page={currentPage}&pageSize={pageSize}");

            if (response.IsSuccessStatusCode)
            {
                var logs = await response.Content.ReadFromJsonAsync<List<string>>();
                return logs;
            }
            else return null;
        }

        public async Task<List<string>> GetMoreLogs(string date, int currentPage, int pageSize)
        {
            var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}more-logs?date={date}&page={currentPage}&pageSize={pageSize}");

            if (response.IsSuccessStatusCode)
            {
                var logs = await response.Content.ReadFromJsonAsync<List<string>>();
                return logs;
            }
            else return null;
        }

        /// <summary>
        /// Fetches the list of all .log file names from the server.
        /// </summary>
        public async Task<List<string>?> GetLogFiles()
        {
            var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}log-files-names");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<string>>();
            }
            else
            {
                Console.WriteLine($"Error fetching log file list: {response.StatusCode}");
                return null;
            }
        }
    }
}
