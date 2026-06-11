using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using YESS;
using YESS.Services;
using Radzen;
using System.Globalization;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("es-MX");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("es-MX");

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<ArchivoService>();
builder.Services.AddRadzenComponents();

await builder.Build().RunAsync();