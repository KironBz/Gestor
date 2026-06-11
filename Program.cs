using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using YESS;                           // ← cambiado
using YESS.Services;                  // ← cambiado
using Radzen;
using System.Globalization;

// Configurar cultura
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("es-MX");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("es-MX");

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient base
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Registrar servicios (si tienes interfaz, regístrala)
// builder.Services.AddSingleton<IArchivoService, ArchivoService>();
builder.Services.AddSingleton<ArchivoService>(); // ← mientras no tengas la interfaz

// Radzen
builder.Services.AddRadzenComponents();

await builder.Build().RunAsync();