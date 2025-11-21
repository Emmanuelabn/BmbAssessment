using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

// 1) Serilog configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/apigateway-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 2) YARP reverse proxy
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// 3) Rate limiting
var rlConfig = builder.Configuration.GetSection("RateLimiting");
var permitLimit = rlConfig.GetValue<int>("PermitLimit", 100);
var windowSeconds = rlConfig.GetValue<int>("WindowSeconds", 60);

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        // Partition by IP address (or you can use a user id claim later)
        var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: key,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(windowSeconds),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// 4) Health checks (simple self-check)
builder.Services.AddHealthChecks();

var app = builder.Build();

// 5) Middleware pipeline
app.UseSerilogRequestLogging();

// If you add auth later, it will go here:
// app.UseAuthentication();
// app.UseAuthorization();

app.UseRateLimiter();

app.MapHealthChecks("/health");

// Map YARP as the final handler
app.MapReverseProxy();

app.Run();
