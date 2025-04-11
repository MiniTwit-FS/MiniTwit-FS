using Microsoft.AspNetCore.Components;
using MiniTwitClient.Authentication;
using static System.Net.WebRequestMethods;

namespace MiniTwitClient.Layout
{
    public partial class MainLayout : LayoutComponentBase
    {
        [Inject] public UserState UserState { get; set; }
        [Inject] public NavigationManager Navigation { get; set; }
        [Inject] public HttpClient _client { get; set; }

        public void Logout()
        {
            _client.DefaultRequestHeaders.Remove("Username");
            UserState.LogOut();
        }

        public void MyPage()
        {
            Navigation.NavigateTo("/");
        }
    }
}
