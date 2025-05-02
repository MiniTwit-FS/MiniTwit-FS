using Microsoft.AspNetCore.Components;
using MiniTwitClient.Authentication;
using MiniTwitClient.Controllers;

namespace MiniTwitClient.Layout
{
    public partial class MainLayout : LayoutComponentBase
    {
        [Inject] public MinitwitController Controller { get; set; }
        [Inject] public UserState UserState { get; set; }
        [Inject] public NavigationManager Navigation { get; set; }
        [Inject] public HttpClient _client { get; set; }

        protected override void OnInitialized()
        {
            UserState.OnChange += StateHasChanged;
        }

        public void Dispose()
        {
            UserState.OnChange -= StateHasChanged;
        }

        public async Task Logout()
        {
            var logout = await Controller.Logout();

            if (logout.IsSuccessStatusCode)
            {
                UserState.LogOut();
                Navigation.NavigateTo("/login");
            }
        }

        public void MyPage()
        {
            Navigation.NavigateTo("/");
        }
    }
}
