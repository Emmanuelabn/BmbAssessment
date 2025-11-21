using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using ApiGateway.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Yarp.ReverseProxy;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// ---------- Serilog ----------
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/apigateway-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ---------- Identity + EF Core ----------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EmployeeDatabase")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ---------- JWT authentication ----------
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),

            // These now MUST match the token we issue
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("JWT auth failed: " + context.Exception.Message);
                return Task.CompletedTask;
            }
        };
    });

// ---------- Rate limiting ----------
var rlConfig = builder.Configuration.GetSection("RateLimiting");
var permitLimit = rlConfig.GetValue<int>("PermitLimit", 100);
var windowSeconds = rlConfig.GetValue<int>("WindowSeconds", 60);

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
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

// ---------- YARP reverse proxy + transforms ----------
var sharedSecret = builder.Configuration["Downstreams:SharedSecret"];

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        builderContext.AddRequestTransform(transformContext =>
        {
            var httpContext = transformContext.HttpContext;

            // Forward employee id (Identity user id) to downstream
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                transformContext.ProxyRequest.Headers.Remove("X-Employee-Id");
                transformContext.ProxyRequest.Headers.Add("X-Employee-Id", userId);
            }

            // Add internal secret header so only gateway can call Product/Order
            if (!string.IsNullOrEmpty(sharedSecret))
            {
                transformContext.ProxyRequest.Headers.Remove("X-Internal-Secret");
                transformContext.ProxyRequest.Headers.Add("X-Internal-Secret", sharedSecret);
            }

            return ValueTask.CompletedTask;
        });
    });

// ---------- Health checks ----------
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapControllers();

app.UseSerilogRequestLogging();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

// Require auth for everything going through the proxy
app.MapReverseProxy().RequireAuthorization();


app.Run();
