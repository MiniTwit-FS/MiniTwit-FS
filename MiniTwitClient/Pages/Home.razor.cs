using Microsoft.AspNetCore.Components;
using MiniTwitClient.Controllers;
using MiniTwitClient.Models;

namespace MiniTwitClient.Pages
{
    public partial class Home : ComponentBase
    {
		[Inject] public MinitwitController controller { get; set; }

        private List<Message> Messages { get; set; } = new List<Message>();

		protected override async Task OnInitializedAsync()
		{
			Messages = await controller.GetPublicTimeline(new MessagesRequest());

			StateHasChanged();
		}

		private async Task PostMessage()
        {
            // Create a new message object
            //await controller.PostMessage(newMessage);
            // Refresh the messages after posting
            Messages = await controller.GetPublicTimeline(new MessagesRequest());
            StateHasChanged();
        }
    }
}

