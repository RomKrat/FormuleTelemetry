using FormulaTelemetry.Web;
using FormulaTelemetry.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("ApiBaseUrl is missing in wwwroot/appsettings.json");

builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/")
});
builder.Services.AddScoped<TelemetryApiClient>();
builder.Services.AddScoped<SeasonState>();

await builder.Build().RunAsync();
