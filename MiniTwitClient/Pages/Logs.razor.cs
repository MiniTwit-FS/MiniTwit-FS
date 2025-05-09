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
        private List<string> _logFilesList = new List<string>();
        private string? _selectedLogFile;
        private int currentPage = 1;
        private int pageSize = 100;

        private ElementReference logContainer;
        private bool _shouldAutoScroll;

        [JSInvokable]
        public async Task OnReachedTop()
        {
            await LoadNextPage();
            await InvokeAsync(StateHasChanged);
        }

        public string? SelectedLogFile
        {
            get => _selectedLogFile;
            set
            {
                if (_selectedLogFile != value)
                {
                    _selectedLogFile = value;
                    // fire-and-forget is okay here; exceptions will surface in the console
                    _ = LoadInitialLogs();
                }
            }
        }

        protected override async Task OnInitializedAsync()
        {
            // 1) Load list of log files
            await LoadLogFiles();

            // 2) If there's at least one, pick the first by default
            if (_logFilesList.Any())
            {
                SelectedLogFile = _logFilesList.First();
            }

            // 3) Start your SignalR hub and then load logs for selected file
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Controller.address + "logHub", options =>
                    options.AccessTokenProvider = async () => UserState.Token)
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<string>("ReceiveLogUpdate", async message =>
            {
                // 1) check if we’re at bottom _before_ inserting
                var atBottom = await JSRuntime.InvokeAsync<bool>("isScrolledToBottom", logContainer);

                // 2) insert new log and re-render
                var converted = FormatTimestampToLocal(message);
                _logMessages.Add(converted);

                if (atBottom)
                    _shouldAutoScroll = true;

                await InvokeAsync(StateHasChanged);
            });

            await _hubConnection.StartAsync();
            await InitialLogs();
        }

        private async Task LoadLogFiles()
        {
            var files = await Controller.GetLogFiles();
            if (files != null)
            {
                _logFilesList = files;
            }
        }

        private void DownloadSelectedLogFile()
        {
            if (string.IsNullOrEmpty(SelectedLogFile)) return;

            var url = $"log-files/download/{Uri.EscapeDataString(SelectedLogFile)}";
            Navigation.NavigateTo(url, forceLoad: true);
        }

        private async Task LoadInitialLogs()
        {
            if (_isLoading || string.IsNullOrEmpty(SelectedLogFile))
                return;

            _isLoading = true;
            StateHasChanged();

            try
            {
                currentPage = 1;
                var date = Path.GetFileNameWithoutExtension(SelectedLogFile);
                date = date.Remove(0, 16);


                var logs = await Controller.GetLogs(date, currentPage, pageSize, more: false);

                if (logs != null)
                {
                    _logMessages = logs
                        .Select(FormatTimestampToLocal)
                        .Reverse()    // oldest first
                        .ToList();
                }
            }
            finally
            {
                _isLoading = false;
                StateHasChanged();
            }
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
            if (_isLoading || string.IsNullOrEmpty(SelectedLogFile))
                return;

            //_isLoading = true;
            //try
            //{
            //    Console.WriteLine("Reached bottom!");

            try
            {
                _previousScrollHeight = await JSRuntime.InvokeAsync<double>("getScrollHeight", _logContainer);

                currentPage++;
                var date = Path.GetFileNameWithoutExtension(SelectedLogFile!);
                var older = await Controller.GetLogs(date, currentPage, pageSize, more: true);

                if (older?.Any() == true)
                {
                    var converted = older.Select(FormatTimestampToLocal);
                    _logMessages.InsertRange(0, converted);
                }

                await InvokeAsync(StateHasChanged);
            }
            finally
            {
                _isLoading = false;
                StateHasChanged();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("scrollToBottom", _logContainer);
                await JSRuntime.InvokeVoidAsync(
                    "initializeTopSentinel",
                    DotNetObjectReference.Create(this)
                );
            }
            else if (_shouldAutoScroll)
            {
                if (_shouldAutoScroll)
                {
                    _shouldAutoScroll = false;
                    await JSRuntime.InvokeVoidAsync("scrollToBottom", _logContainer);
                }
                else if (_previousScrollHeight > 0)
                {
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
