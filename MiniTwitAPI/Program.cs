using Microsoft.EntityFrameworkCore;
using MiniTwitAPI;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.AspNetCore.Identity;
using MiniTwitAPI.Extentions;
using Serilog;
using Serilog.Extensions.Logging;
using MiniTwitAPI.Hubs;
using System.Security.Claims;

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

builder.Services.AddSignalR();

logger.LogInformation("Configuring CORS policies");
builder.Services.AddCors(o => o.AddPolicy("DevCorsPolicy", p =>
    p.WithOrigins("https://localhost:7192")
     .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

logger.LogInformation("Building application");
var app = builder.Build();

app.UseRouting();

// apply the CORS policy once (you defined "AllowAll")
app.UseCors("AllowAll");

app.UseSession();

// allow CORS preflight without auth
app.Use(async (context, next) =>
{
    if (HttpMethods.IsOptions(context.Request.Method))
    {
        context.Response.StatusCode = StatusCodes.Status204NoContent;
        return;
    }
    await next();
});

// auth gate: require Authorization; accept special Basic; let others flow to UseAuthentication
app.Use(async (context, next) =>
{
    var authorizationHeader = context.Request.Headers["Authorization"].ToString();
    var allowedBasicAuthValue = builder.Configuration["SpecialApp:AuthorizationHeader"];

    if (string.IsNullOrWhiteSpace(authorizationHeader))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("Unauthorized - Authorization required.");
        return;
    }

    // Special Basic: exact match -> mark user as authenticated
    if (authorizationHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
    {
        if (authorizationHeader == allowedBasicAuthValue)
        {
            var identity = new ClaimsIdentity(authenticationType: "SpecialBasic");
            identity.AddClaim(new Claim(ClaimTypes.Name, "SpecialApp"));
            context.User = new ClaimsPrincipal(identity);

            await next();
            return;
        }

        // invalid Basic -> stop here
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("Unauthorized - Invalid Basic credentials.");
        return;
    }

    // Non-Basic (e.g., Bearer) -> let the normal auth middleware handle it
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

// (your request logging middleware can stay as-is)

app.MapControllers();

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
            user.PasswordHash = user.PasswordHash.Sha256Hash();
            user.IsPasswordHashed = true;
        }

        db.SaveChanges();
    }
}

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
