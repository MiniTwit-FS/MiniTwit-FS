using Microsoft.AspNetCore.Components;
using MiniTwitClient.Controllers;
using MiniTwitClient.Models;

namespace MiniTwitClient.Pages
{
    public partial class Username : ComponentBase
    {
		[Inject] public MinitwitController controller { get; set; }

		[Parameter] public string username { get; set; } = "not set";

        // Sample data. Replace with actual data retrieval from your backend.
        private List<Message> Messages = new List<Message>();

		protected override async Task OnInitializedAsync()
		{
			Messages = await controller.GetUserTimeline(username, new MessagesRequest());

			StateHasChanged();
		}
	}
}
