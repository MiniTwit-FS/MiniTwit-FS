using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MiniTwitClient;
using MiniTwitClient.Controllers;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;
using MiniTwitClient.Authentication;
using System.Net.Http.Headers;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var js = builder.Services.BuildServiceProvider().GetRequiredService<IJSRuntime>();

var appConfig = await js.InvokeAsync<Config>("eval", "window.appConfig");

string apiEndpoint = appConfig?.ApiEndpoint + "/" ?? "https://localhost:7297"; // Fallback if not set

var token = await js.InvokeAsync<string>("sessionStorage.getItem", "authToken");

builder.Services.AddSingleton(sp =>
{
    var client = new HttpClient
    {
        BaseAddress = new Uri(apiEndpoint)
    };

    if (!string.IsNullOrEmpty(token))
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
    else
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "c2ltdWxhdG9yOnN1cGVyX3NhZmUh");
    }

    return client;
});

builder.Services.AddSingleton<UserState>();

builder.Services.AddSingleton<MinitwitController>();

await builder.Build().RunAsync();

public class Config
{
    public string ApiEndpoint { get; set; }
}
