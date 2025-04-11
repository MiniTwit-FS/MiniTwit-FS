using Microsoft.AspNetCore.Components;
using MiniTwitClient.Authentication;

namespace MiniTwitClient.Layout
{
    public partial class MainLayout : LayoutComponentBase
    {
        [Inject] public UserState UserState { get; set; }

        public void Logout()
        {
            UserState.LogOut();
        }
    }
}
