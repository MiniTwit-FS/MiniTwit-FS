using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MiniTwitClient.Authentication;
using MiniTwitClient.Controllers;
using MiniTwitClient.Models;
using Microsoft.AspNetCore.SignalR.Client;
using static System.Net.WebRequestMethods;
using System.Text.Json;

namespace MiniTwitClient.Pages
{
    public partial class Logs : ComponentBase
    {
		[Inject] public MinitwitController Controller { get; set; }
		[Inject] public NavigationManager Navigation { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }

        private HubConnection? _hubConnection;
        private List<string> _logMessages = new List<string>();
        private int currentPage = 1;
        private int pageSize = 100;

        protected override async Task OnInitializedAsync()
        {
            _hubConnection = new HubConnectionBuilder()
            .WithUrl(Controller.address + "logHub")
            .Build();

            _hubConnection.On<string>("ReceiveLogUpdate", (message) =>
            {
                _logMessages.Insert(_logMessages.Count(), message);
                StateHasChanged();
            });

            await _hubConnection.StartAsync();
            await InitialLogs();
        }

        private async Task InitialLogs()
        {
            // Call your API to load logs
            var time = DateTime.UtcNow;
            var logs = await Controller.GetLogs(time.Year.ToString() + time.Month.ToString("D2") + time.Day.ToString("D2"), currentPage, pageSize);
            if (logs != null) _logMessages.InsertRange(0, logs);
        }

        private async Task ScrollLogs()
        {
            // Call your API to load logs
            var time = DateTime.UtcNow;
            var logs = await Controller.GetMoreLogs(time.Year.ToString() + time.Month.ToString("D2") + time.Day.ToString("D2"), currentPage, pageSize);
            if (logs != null) _logMessages.InsertRange(0, logs);
        }

        private async Task NewLogs()
        {
            // Call your API to load logs
            var time = DateTime.UtcNow;
            var logs = await Controller.GetMoreLogs(time.Year.ToString() + time.Month.ToString("D2") + time.Day.ToString("D2"), currentPage, pageSize);
            if (logs != null) _logMessages.InsertRange(_logMessages.Count(), logs);
        }

        private async Task LoadNextPage()
        {
            currentPage++;
            await ScrollLogs();
        }

        private async Task LoadPreviousPage()
        {
            if (currentPage > 1)
            {
                currentPage--;
                await InitialLogs();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }

        [JSInvokable]
        public async Task OnReachedBottom()
        {
            //// if we’re already loading, bail out
            //if (_isLoading)
            //    return;

            //_isLoading = true;
            //try
            //{
            //    Console.WriteLine("Reached bottom!");

            //    messageIndex++;
            //    Messages = await Controller.GetUserTimeline(username, new MessagesRequest
            //    {
            //        NumberOfMessages = messagesCount * messageIndex
            //    });
            //    StateHasChanged();
            //}
            //finally
            //{
            //    _isLoading = false;
            //}
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
