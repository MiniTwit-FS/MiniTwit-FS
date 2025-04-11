using Microsoft.AspNetCore.Components;
using MiniTwitClient.Authentication;
using MiniTwitClient.Controllers;
using MiniTwitClient.Models;

namespace MiniTwitClient.Pages
{
    public partial class Home : ComponentBase
    {
		[Inject] public MinitwitController Controller { get; set; }
        [Inject] public UserState userState { get; set; }

        private AddMessageRequest AddMessageRequest { get; set; } = new AddMessageRequest();

        private List<Message> Messages { get; set; } = new List<Message>();

		protected override async Task OnInitializedAsync()
		{
			Messages = await Controller.GetPublicTimeline(new MessagesRequest());

			StateHasChanged();
		}

		private async Task PostMessage()
        {
            await Controller.PostMessage(AddMessageRequest);

            // Refresh the messages after posting
            Messages = await Controller.GetPublicTimeline(new MessagesRequest());
            StateHasChanged();
        }
    }
}

