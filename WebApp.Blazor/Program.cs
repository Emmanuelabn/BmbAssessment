using Microsoft.Extensions.Options;
using WebApp.Blazor.Components;
using WebApp.Blazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ApiGateway config
builder.Services.Configure<ApiGatewayOptions>(
    builder.Configuration.GetSection("ApiGateway"));

// Shared auth state
builder.Services.AddSingleton<AuthState>();

// HttpClient for the gateway
builder.Services.AddHttpClient<ApiGatewayClient>((sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<ApiGatewayOptions>>().Value;
    client.BaseAddress = new Uri(opts.BaseUrl);
});

// HttpClient for auth (login/register)
builder.Services.AddHttpClient<AuthClient>((sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<ApiGatewayOptions>>().Value;
    client.BaseAddress = new Uri(opts.BaseUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
