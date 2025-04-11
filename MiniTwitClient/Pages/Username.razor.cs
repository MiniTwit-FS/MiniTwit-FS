using Microsoft.AspNetCore.Components;
using MiniTwitClient.Authentication;
using MiniTwitClient.Controllers;
using MiniTwitClient.Models;

namespace MiniTwitClient.Pages
{
    public partial class Username : ComponentBase
    {
		[Inject] public MinitwitController controller { get; set; }
		[Inject] public UserState UserState { get; set; }
		[Inject] public NavigationManager Navigation { get; set; }

        [Parameter] public string username { get; set; } = "";

		private bool Follows { get; set; } = false;
        private bool MyPage { get; set; } = false;

        // Sample data. Replace with actual data retrieval from your backend.
        private List<Message> Messages = new List<Message>();

		protected override async Task OnInitializedAsync()
		{
			if (UserState.IsLoggedIn)
			{
                MyPage = UserState.Username == username;

                if (MyPage)
                {
                    Navigation.NavigateTo("/");
                    return;
                }

                Follows = await controller.Follows(username);
            }

            Messages = await controller.GetUserTimeline(username, new MessagesRequest());

			StateHasChanged();
		}

		private async Task Follow()
		{
            await controller.FollowChange(new FollowRequest()
            {
                Follow = username
            });

            Follows = await controller.Follows(username);

            StateHasChanged();
        }

        private async Task Unfollow()
        {
            await controller.FollowChange(new FollowRequest()
            {
                Unfollow = username
            });

            Follows = await controller.Follows(username);

            StateHasChanged();
        }
    }
}
