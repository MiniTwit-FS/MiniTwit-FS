using Microsoft.EntityFrameworkCore;
using MiniTwitAPI;

var builder = WebApplication.CreateBuilder(args);

// Load the correct appsettings based on the environment
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "prod"; // Default to "Production" if null

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Base settings
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"[DEBUG] Using Connection String: {connectionString}");

// Add services to the container.
builder.Services.AddControllers(); // Add controllers for your API endpoints

// Configure DbContext with retry logic for SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Middleware for handling HTTP requests
app.UseSession();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers(); // Map controllers to handle requests

// Gracefully shutdown (optional, helps when using Docker to shut down cleanly)
app.Lifetime.ApplicationStopping.Register(() =>
{
    // Your cleanup logic if any
    Console.WriteLine("Application is shutting down...");
});

// Run the application
app.Run();
