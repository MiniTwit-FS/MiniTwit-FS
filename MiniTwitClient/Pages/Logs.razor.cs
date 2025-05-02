using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MiniTwitClient.Authentication;
using MiniTwitClient.Controllers;
using MiniTwitClient.Models;
using Microsoft.AspNetCore.SignalR.Client;
using static System.Net.WebRequestMethods;
using System.Text.Json;
using System.Globalization;

namespace MiniTwitClient.Pages
{
    public partial class Logs : ComponentBase
    {
		[Inject] public MinitwitController Controller { get; set; }
		[Inject] public NavigationManager Navigation { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public UserState UserState { get; set; }

        private HubConnection? _hubConnection;
        private List<string> _logMessages = new List<string>();
        private int currentPage = 1;
        private int pageSize = 100;

        private ElementReference logContainer;

        protected override async Task OnInitializedAsync()
        {
            _hubConnection = new HubConnectionBuilder()
            .WithUrl(Controller.address + "logHub")
            .WithAutomaticReconnect()
            .Build();

            _hubConnection.On<string>("ReceiveLogUpdate", (message) =>
            {
                var converted = FormatTimestampToLocal(message);
                _logMessages.Insert(_logMessages.Count(), converted);
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
            var converted = logs.Select(FormatTimestampToLocal);
            if (logs != null) _logMessages.InsertRange(0, converted);
        }

        private async Task ScrollLogs()
        {
            // Call your API to load logs
            var time = DateTime.UtcNow;
            var logs = await Controller.GetMoreLogs(time.Year.ToString() + time.Month.ToString("D2") + time.Day.ToString("D2"), currentPage, pageSize);
            var converted = logs.Select(FormatTimestampToLocal);
            if (logs != null) _logMessages.InsertRange(0, converted);
        }

        private async Task NewLogs()
        {
            // Call your API to load logs
            var time = DateTime.UtcNow;
            var logs = await Controller.GetMoreLogs(time.Year.ToString() + time.Month.ToString("D2") + time.Day.ToString("D2"), currentPage, pageSize);
            var converted = logs.Select(FormatTimestampToLocal);
            if (logs != null) _logMessages.InsertRange(_logMessages.Count(), converted);
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
                await JSRuntime.InvokeVoidAsync("scrollToBottom", logContainer);
            }
        }

        private string FormatTimestampToLocal(string logLine)
        {
            // Split into [date] [time] [offset] [the rest…]
            var parts = logLine.Split(' ', 4);
            if (parts.Length < 4)
                return logLine;    // something unexpected—just return as-is

            // e.g. "2025-04-25 09:44:44.756 +02:00"
            var tsRaw = $"{parts[0]} {parts[1]} {parts[2]}";

            if (!DateTimeOffset.TryParseExact(
                    tsRaw,
                    "yyyy-MM-dd HH:mm:ss.fff zzz",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var dto))
            {
                return logLine;    // parse failed → leave original
            }

            // Convert to local time and reformat
            var local = dto.ToLocalTime();
            var rest = parts[3];  // "[INF] Application starting..."
            return $"{local:yyyy-MM-dd HH:mm:ss.fff zzz} {rest}";
        }
    }
}
