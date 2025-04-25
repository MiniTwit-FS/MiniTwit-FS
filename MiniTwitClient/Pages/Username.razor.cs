using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MiniTwitClient.Authentication;
using MiniTwitClient.Controllers;
using MiniTwitClient.Models;

namespace MiniTwitClient.Pages
{
    public partial class Username : ComponentBase
    {
		[Inject] public MinitwitController Controller { get; set; }
		[Inject] public UserState UserState { get; set; }
		[Inject] public NavigationManager Navigation { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }

        [Parameter] public string username { get; set; } = "";
        private int messagesCount = 50;
        private int messageIndex = 1;

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

                Follows = await Controller.Follows(username);
            }

            Messages = await Controller.GetUserTimeline(username, new MessagesRequest { NumberOfMessages = messagesCount });

			StateHasChanged();
		}

		private async Task Follow()
		{
            await Controller.FollowChange(new FollowRequest()
            {
                Follow = username
            });

            Follows = await Controller.Follows(username);

            StateHasChanged();
        }

        private async Task Unfollow()
        {
            await Controller.FollowChange(new FollowRequest()
            {
                Unfollow = username
            });

            Follows = await Controller.Follows(username);

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
                Messages = await Controller.GetUserTimeline(username, new MessagesRequest
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
