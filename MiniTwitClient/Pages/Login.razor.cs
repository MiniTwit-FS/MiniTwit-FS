using Microsoft.AspNetCore.Components;
using MiniTwitClient.Authentication;
using MiniTwitClient.Controllers;
using MiniTwitClient.Models;
using System.Text.Json;

namespace MiniTwitClient.Pages
{
    public partial class Login : ComponentBase
    {
        [Inject] public MinitwitController Controller { get; set; }
        [Inject] public UserState UserState { get; set; }
        [Inject] public NavigationManager Navigation { get; set; } = default!;
        [Inject] public HttpClient _client { get; set; }

        public LoginRequest LoginRequest { get; set; } = new LoginRequest();

        private async Task LoginCall()
        {
            var loginResponse = await Controller.Login(LoginRequest);

            var token = loginResponse?.Token;

            if (string.IsNullOrEmpty(token))
            {
                // Do an error...
                return;
            }

            UserState.LogIn(LoginRequest.Username, token);
            Navigation.NavigateTo($"/public");
        }
    }
}
