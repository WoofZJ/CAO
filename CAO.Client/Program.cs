using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using CAO.Client;
using CAO.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    string apiBaseAddress = config["ApiSettings:BaseAddress"]
        ?? builder.HostEnvironment.BaseAddress;
    return new HttpClient
    {
        BaseAddress = new Uri(apiBaseAddress)
    };
});

builder.Services.AddAuthorizationCore();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped(sp => (CustomAuthStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<BlogService>();
builder.Services.AddScoped<StorageService>();
builder.Services.AddScoped<VisitService>();
builder.Services.AddScoped<TagService>();
builder.Services.AddScoped<ArchiveService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<TimelineService>();
builder.Services.AddScoped<AdminService>();

await builder.Build().RunAsync();
