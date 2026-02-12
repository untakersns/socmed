using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using socmed_front.Components;
using socmed_front.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<TokenProvider>();
builder.Services.AddScoped<ProtectedLocalStorage>();

builder.Services.AddScoped<JwtInterceptor>(sp =>
{
    var localStorage = sp.GetRequiredService<ProtectedLocalStorage>();
    var tokenProvider = sp.GetRequiredService<TokenProvider>();
    return new JwtInterceptor(localStorage, tokenProvider);
});

builder.Services.AddHttpClient("MyAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7047/");
})
.AddHttpMessageHandler<JwtInterceptor>();

builder.Services.AddScoped<UserService>(sp => {
    var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var client = clientFactory.CreateClient("MyAPI");
    return new UserService(client);
});

builder.Services.AddScoped<AuthService>(sp => {
    var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var client = clientFactory.CreateClient("MyAPI");
    var localStorage = sp.GetRequiredService<ProtectedLocalStorage>();
    var tokenProvider = sp.GetRequiredService<TokenProvider>();
    var authStateProvider = sp.GetRequiredService<CustomAuthStateProvider>();
    var navManager = sp.GetRequiredService<NavigationManager>();
    return new AuthService(client, localStorage, tokenProvider, authStateProvider, navManager);
});

builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddAuthenticationCore();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthStateProvider>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();