using Microsoft.Extensions.Options;
using WebApp.Blazor.Components;
using WebApp.Blazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add ApiGatewayOptions configuration
builder.Services.Configure<ApiGatewayOptions>(
    builder.Configuration.GetSection("ApiGateway"));

// Add Http Client for ApiGatewayService
builder.Services.AddHttpClient<ApiGatewayClient>((sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<ApiGatewayOptions>>().Value;

    if (string.IsNullOrWhiteSpace(opts.BaseUrl))
    {
        throw new InvalidOperationException("ApiGateway:BaseUrl is not configured.");
    }

    client.BaseAddress = new Uri(opts.BaseUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
