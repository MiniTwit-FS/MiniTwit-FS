using Microsoft.JSInterop;
using MiniTwitClient.Pages;
using System.Net.Http.Headers;

namespace MiniTwitClient.Authentication
{
    public class UserState
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public string? Username { get; private set; }
        public bool IsLoggedIn => !string.IsNullOrEmpty(Username);
        public bool IsAdmin => Username != null && Username.Equals("helgeandmircea");
        public string? Token { get; private set; }

        public event Action? OnChange;

        public UserState(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;

            if (httpClient.DefaultRequestHeaders.Authorization.ToString().Contains("Bearer"))
            {
                LoginCache();
            }
        }

        private async Task LoginCache()
        {
            var token = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "authToken");
            var username = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "username");

            await LogIn(username, token);
        }

        public async Task LogIn(string username, string token)
        {
            Username = username;
            Token = token;

            _httpClient.DefaultRequestHeaders.Add("Username", username);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "authToken", token);
            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "username", username);

            NotifyStateChanged();
        }

        public async Task LogOut()
        {
            Username = null;
            Token = null;

            _httpClient.DefaultRequestHeaders.Remove("Username");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "c2ltdWxhdG9yOnN1cGVyX3NhZmUh");
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "username");
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "authToken");

            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
