using Microsoft.EntityFrameworkCore;
using ProductService.Application.Services;
using ProductService.Infrastructure;
using ProductService.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/product-service-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

// Infra: EF Core + repos
builder.Services.AddInfrastructure(builder.Configuration);

// Application layer service
builder.Services.AddScoped<IProductService, ProductService.Application.Services.ProductService>();

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ProductDbContext>("ProductDb");

var app = builder.Build();

//var internalSecret = app.Configuration["InternalApi:SharedKey"];

//app.Use(async (context, next) =>
//{
//    var path = context.Request.Path.Value ?? string.Empty;

//    if (path.StartsWith("/health"))
//    {
//        await next();
//        return;
//    }

//    if (!string.IsNullOrEmpty(internalSecret))
//    {
//        if (!context.Request.Headers.TryGetValue("X-Internal-Secret", out var header) ||
//            header != internalSecret)
//        {
//            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
//            await context.Response.WriteAsync("Unauthorized");
//            return;
//        }
//    }

//    await next();
//});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    dbContext.Database.Migrate();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
