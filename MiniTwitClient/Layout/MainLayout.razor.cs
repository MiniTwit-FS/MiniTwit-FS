using Microsoft.AspNetCore.Components;
using MiniTwitClient.Authentication;

namespace MiniTwitClient.Layout
{
    public partial class MainLayout : LayoutComponentBase
    {
        [Inject] public UserState UserState { get; set; }
        [Inject] public NavigationManager Navigation { get; set; }


        public void Logout()
        {
            UserState.LogOut();
        }

        public void MyPage()
        {
            Navigation.NavigateTo("/");
        }
    }
}
