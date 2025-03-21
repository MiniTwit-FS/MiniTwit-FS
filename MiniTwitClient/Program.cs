using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MiniTwitClient;
using MiniTwitClient.Controllers;
using System;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Fetch the API endpoint from the environment variable or fallback to a default value (localhost in dev)
string apiEndpoint = Environment.GetEnvironmentVariable("API_ENDPOINT") ?? "https://localhost:7297";

// Register HttpClient with the API endpoint
builder.Services.AddScoped(sp => new HttpClient
{
	BaseAddress = new Uri(apiEndpoint)
});

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.HostEnvironment.Environment}.json", optional: false, reloadOnChange: false);

builder.Services.AddScoped<MinitwitController>();

await builder.Build().RunAsync();
