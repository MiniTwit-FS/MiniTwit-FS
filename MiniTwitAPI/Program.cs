using Microsoft.EntityFrameworkCore;
using MiniTwitAPI;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Load the correct appsettings based on the environment
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers(); // Add controllers for your API endpoints

// Configure DbContext with retry logic for SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
        maxRetryCount: 5,           // Max number of retry attempts
        maxRetryDelay: TimeSpan.FromSeconds(30), // Delay between retries
        errorNumbersToAdd: null)));    // Additional SQL Server error numbers to include in retries (default is transient error codes)

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
