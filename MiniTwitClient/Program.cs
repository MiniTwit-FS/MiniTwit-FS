using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MiniTwitClient;
using MiniTwitClient.Controllers;
using System;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

// Determine the correct config file based on the host
string configFile = builder.HostEnvironment.IsDevelopment() ? "appsettings.dev.json" : "appsettings.prod.json";

// Load configuration files
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
builder.Configuration.AddJsonFile(configFile, optional: false, reloadOnChange: false);

// Read API base URL from the loaded configuration
string apiEndpoint = builder.Configuration["API_ENDPOINT"] ?? "https://localhost:7297";

// Register HttpClient with the correct API endpoint
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiEndpoint)
});

// Register HttpClient with the API endpoint
builder.Services.AddScoped(sp => new HttpClient
{
	BaseAddress = new Uri(apiEndpoint)
});


builder.Services.AddScoped<MinitwitController>();

await builder.Build().RunAsync();
