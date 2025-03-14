using Microsoft.AspNetCore.Components;
using MiniTwitClient.Controllers;
using MiniTwitClient.Models;

namespace MiniTwitClient.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject] MinitwitController controller { get; set; }

		List<Message> Messages { get; set; } = null;

		protected override async Task OnInitializedAsync()
		{
			Messages = await controller.GetPublicTimeline(new MessagesRequest()); 
		}
	}
}
