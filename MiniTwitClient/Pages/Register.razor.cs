using Microsoft.AspNetCore.Components;
using MiniTwitClient.Authentication;
using MiniTwitClient.Controllers;
using MiniTwitClient.Models;

namespace MiniTwitClient.Pages
{
    public partial class Register : ComponentBase
    {
        [Inject] public MinitwitController Controller { get; set; }
        [Inject] public UserState UserState { get; set; }
        [Inject] public NavigationManager Navigation { get; set; } = default!;

        public RegisterRequest RegisterRequest { get; set; } = new RegisterRequest();

        private async Task CreateAccount()
        {
            var register = await Controller.Register(RegisterRequest);

            if (register.IsSuccessStatusCode)
            {
                //Redirect
                Navigation.NavigateTo($"/login");
            }
            else
            {
                // Show error to user if username is taken, etc.
            }
        }
    }
}
