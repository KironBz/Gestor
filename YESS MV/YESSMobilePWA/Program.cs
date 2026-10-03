using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Logging;
using YESSMobilePWA;
using YESSMobilePWA.Services;
using Radzen;
using System.Globalization;

// Configurar cultura a México
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("es-MX");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("es-MX");

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// ============================================
// ROOT COMPONENTS
// ============================================
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ============================================
// HTTP CLIENT
// ============================================
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// ============================================
// SERVICIOS CON INTERFACES (INYECTABLE)
// ============================================

builder.Services.AddScoped<IArchivoService, ArchivoService>();
builder.Services.AddSingleton<IDeudaCalculatorService, DeudaCalculatorService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddSingleton<EventService>();
builder.Services.AddScoped<ValidationService>();  // ← AGREGAR ESTA LÍNEA
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Services.AddRadzenComponents();


// ============================================
// BUILD Y RUN
// ============================================
await builder.Build().RunAsync();