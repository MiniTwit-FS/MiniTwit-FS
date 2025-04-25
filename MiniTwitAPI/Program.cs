using Microsoft.EntityFrameworkCore;
using MiniTwitAPI;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.AspNetCore.Identity;
using MiniTwitAPI.Extentions;
using Serilog;
using Serilog.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Load the correct appsettings based on the environment
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "prod"; // Default to "Production" if null

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Base settings
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Configure logging with providers
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Setup file logging if configured
if (builder.Configuration.GetSection("Serilog").Exists())
{
    var logPath = builder.Configuration["Serilog:WriteTo:1:Args:path"] ?? "logs/minitwit-api-log.log";
    var logDir = Path.GetDirectoryName(logPath);
    if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
    {
        Directory.CreateDirectory(logDir);
    }
}

var logger = new SerilogLoggerFactory(Log.Logger).CreateLogger("Program");

logger.LogInformation("Application starting. Environment: {Environment}", environment);


// Add services to the container.
builder.Services.AddControllers();

var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")?.Replace("{DB_PASSWORD}", dbPassword);

// Don't log full connection string in production for security
if (environment == "dev")
{
    logger.LogDebug("Using Connection String: {ConnectionString}", connectionString);
}
else
{
    var sanitizedConnectionString = new StringBuilder(connectionString ?? "");
    if (connectionString?.Contains("password=") == true)
    {
        var startIndex = connectionString.IndexOf("password=");
        var endIndex = connectionString.IndexOf(";", startIndex);
        if (endIndex == -1) endIndex = connectionString.Length;
        sanitizedConnectionString.Remove(startIndex, endIndex - startIndex).Insert(startIndex, "password=***");
    }
    logger.LogDebug("Using Database Connection with masked credentials");
}

// Configure DbContext with retry logic for SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

    // Add logging to EF Core operations in development
    if (environment == "dev")
    {
        options.EnableSensitiveDataLogging()
               .LogTo(message => logger.LogDebug(message),
                      LogLevel.Information);
    }
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});
builder.Services.AddHttpContextAccessor();

logger.LogInformation("Configuring CORS policies");
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // This effectively allows any origin.
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();

        logger.LogDebug("CORS policy 'AllowAll' configured");
    });
});

logger.LogInformation("Building application");
var app = builder.Build();

// Create an application lifetime logger
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
var appLogger = app.Services.GetRequiredService<ILogger<Program>>();

// Middleware for handling HTTP requests
app.UseCors("AllowAll"); // Apply the CORS policy
app.UseSession();
app.UseHttpsRedirection();

// Add a basic request logger middleware
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var start = DateTime.UtcNow;

    // Capture the request details
    logger.LogDebug("Request {Method} {Url} started",
        context.Request.Method,
        context.Request.Path);

    try
    {
        await next();

        var elapsedMs = (DateTime.UtcNow - start).TotalMilliseconds;
        logger.LogInformation("Request {Method} {Url} completed in {ElapsedTime}ms with status {StatusCode}",
            context.Request.Method,
            context.Request.Path,
            elapsedMs,
            context.Response?.StatusCode);
    }
    catch (Exception ex)
    {
        var elapsedMs = (DateTime.UtcNow - start).TotalMilliseconds;
        logger.LogError(ex, "Request {Method} {Url} failed after {ElapsedTime}ms",
            context.Request.Method,
            context.Request.Path,
            elapsedMs);
        throw;
    }
});

app.MapControllers(); // Map controllers to handle requests

// Register application lifecycle logging
lifetime.ApplicationStarted.Register(() =>
    appLogger.LogInformation("Application started successfully"));

lifetime.ApplicationStopping.Register(() =>
    appLogger.LogInformation("Application is shutting down..."));

lifetime.ApplicationStopped.Register(() =>
    appLogger.LogInformation("Application has been shut down"));

// Apply migrations
if (environment == "prod")
{
    try
    {
        logger.LogInformation("Applying database migrations");
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();
        }
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations");
    }
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var unhashedUsers = db.Users.Where(u => !u.IsPasswordHashed).ToList();
    if (unhashedUsers.Any())
    {
        foreach (var user in unhashedUsers)
        {
            user.PasswordHash = user.PasswordHash.Sha256Hash(); // ← Your hashing method
            user.IsPasswordHashed = true;
        }

        db.SaveChanges();
    }
}
Console.WriteLine();
// Run the application
try
{
    logger.LogInformation("Starting web host");
    app.Run();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Web host terminated unexpectedly");
}
