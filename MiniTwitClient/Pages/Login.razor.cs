using Microsoft.AspNetCore.Components;
using MiniTwitClient.Authentication;
using MiniTwitClient.Controllers;
using MiniTwitClient.Models;

namespace MiniTwitClient.Pages
{
    public partial class Login : ComponentBase
    {
        [Inject] public MinitwitController Controller { get; set; }
        [Inject] public UserState UserState { get; set; }
        [Inject] public NavigationManager Navigation { get; set; } = default!;

        public LoginRequest LoginRequest { get; set; } = new LoginRequest();

        private async Task LoginCall()
        {
            var login = await Controller.Login(LoginRequest);

            if (login.IsSuccessStatusCode)
            {
                UserState.LogIn(LoginRequest.Username);
                Navigation.NavigateTo($"/public");
            }
            else
            {
                // Show error to user if failed login.
            }
        }
    }
}
