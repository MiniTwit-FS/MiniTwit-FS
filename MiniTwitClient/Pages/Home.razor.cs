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
	}
}

