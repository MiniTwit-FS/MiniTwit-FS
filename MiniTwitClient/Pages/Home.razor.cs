using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MiniTwitClient.Authentication;
using MiniTwitClient.Controllers;
using MiniTwitClient.Models;

namespace MiniTwitClient.Pages
{
    public partial class Home : ComponentBase
    {
		[Inject] public MinitwitController Controller { get; set; }
        [Inject] public UserState userState { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }

        private AddMessageRequest AddMessageRequest { get; set; } = new AddMessageRequest();

        private List<Message> Messages { get; set; } = new List<Message>();
        private int messagesCount = 50;
        private int messageIndex = 1;

		protected override async Task OnInitializedAsync()
		{
			Messages = await Controller.GetPublicTimeline(new MessagesRequest { NumberOfMessages = messagesCount });

			StateHasChanged();
		}

		private async Task PostMessage()
        {
            await Controller.PostMessage(AddMessageRequest);

            // Refresh the messages after posting
            Messages = await Controller.GetPublicTimeline(new MessagesRequest());
            StateHasChanged();
        }

        private bool _isLoading = false;

        [JSInvokable]
        public async Task OnReachedBottom()
        {
            // if we’re already loading, bail out
            if (_isLoading)
                return;

            _isLoading = true;
            StateHasChanged();
            try
            {
                Console.WriteLine("Reached bottom!");

                messageIndex++;
                Messages = await Controller.GetPublicTimeline(new MessagesRequest
                {
                    NumberOfMessages = messagesCount * messageIndex
                });
                StateHasChanged();
            }
            finally
            {
                _isLoading = false;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("initializeScrollListener",
                    DotNetObjectReference.Create(this));
            }
        }
    }
}

