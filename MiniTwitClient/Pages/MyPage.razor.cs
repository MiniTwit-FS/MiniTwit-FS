using Microsoft.AspNetCore.Components;
using MiniTwitClient.Authentication;
using MiniTwitClient.Controllers;
using MiniTwitClient.Models;

namespace MiniTwitClient.Pages
{
    public partial class MyPage : ComponentBase
    {
		[Inject] public MinitwitController Controller { get; set; }
        [Inject] public UserState UserState { get; set; }
        [Inject] public NavigationManager Navigation { get; set; }

        private AddMessageRequest AddMessageRequest { get; set; } = new AddMessageRequest();

        private List<Message> Messages { get; set; } = new List<Message>();

		protected override async Task OnInitializedAsync()
		{
            if (!UserState.IsLoggedIn)
            {
                Navigation.NavigateTo("/public");
                return;
            }

            Messages = await Controller.GetMyTimeline(new MessagesRequest());

			StateHasChanged();
		}
    }
}

