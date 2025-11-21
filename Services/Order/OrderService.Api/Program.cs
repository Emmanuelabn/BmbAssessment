using OrderService.Application.Services;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/order-service-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();
builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Application
builder.Services.AddScoped<IOrderService, OrderService.Application.Services.OrderService>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<OrderDbContext>("OrderDb");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
