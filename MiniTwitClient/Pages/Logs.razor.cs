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
        private const int pageSize = 100;

        private ElementReference _logContainer;
        private bool _shouldAutoScroll = false;

        private double _previousScrollHeight = 0;

        protected override async Task OnInitializedAsync()
        {
            _hubConnection = new HubConnectionBuilder()
            .WithUrl(Controller.address + "logHub")
            .WithAutomaticReconnect()
            .Build();

            _hubConnection.On<string>("ReceiveLogUpdate", async message =>
            {
                var atBottom = await JSRuntime.InvokeAsync<bool>("isScrolledToBottom", _logContainer);
                var converted = FormatTimestampToLocal(message);
                _logMessages.Add(converted);

                if (atBottom)
                    _shouldAutoScroll = true;

                await InvokeAsync(StateHasChanged);
            });

            await _hubConnection.StartAsync();
            await LoadInitialLogs();
        }

        private async Task LoadInitialLogs()
        {
            currentPage = 1;
            var date = DateTime.UtcNow.ToString("yyyyMMdd");

            // more: false is default, so you can omit it if you like
            var logs = await Controller.GetLogs(date, currentPage, pageSize, more: false);

            if (logs != null)
            {
                _logMessages = logs
                    .Select(FormatTimestampToLocal)
                    .Reverse()    // oldest at top, newest at bottom
                    .ToList();
            }
        }
        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
                await _hubConnection.DisposeAsync();
        }

        private bool _isLoading = false;

        /// <summary>
        /// When the user scrolls up to the top sentinel—
        /// load the next (older) page and prepend.
        /// </summary>
        [JSInvokable]
        public async Task OnReachedTop()
        {
            // if we’re already loading, bail out
            if (_isLoading)
                return;

            _isLoading = true;
            StateHasChanged();

            try
            {
                // 1) remember how tall we were
                _previousScrollHeight = await JSRuntime.InvokeAsync<double>("getScrollHeight", _logContainer);

                // 2) bump page, fetch older logs
                currentPage++;
                var date = DateTime.UtcNow.ToString("yyyyMMdd");
                var older = await Controller.GetLogs(date, currentPage, pageSize, more: true);

                if (older != null && older.Any())
                {
                    var converted = older.Select(FormatTimestampToLocal).ToList();
                    _logMessages.InsertRange(0, converted);
                }

                await InvokeAsync(StateHasChanged);
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
                // First time: scroll to bottom & start watching top
                await JSRuntime.InvokeVoidAsync("scrollToBottom", _logContainer);
                await JSRuntime.InvokeVoidAsync(
                    "initializeTopSentinel",
                    DotNetObjectReference.Create(this)
                );
            }
            else
            {
                if (_shouldAutoScroll)
                {
                    _shouldAutoScroll = false;
                    await JSRuntime.InvokeVoidAsync("scrollToBottom", _logContainer);
                }
                else if (_previousScrollHeight > 0)
                {
                    // we prepended older logs → restore scroll offset
                    var newHeight = await JSRuntime.InvokeAsync<double>("getScrollHeight", _logContainer);
                    var delta = newHeight - _previousScrollHeight;
                    await JSRuntime.InvokeVoidAsync("setScrollTop", _logContainer, delta);
                    _previousScrollHeight = 0;
                }
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
