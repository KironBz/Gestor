using Microsoft.JSInterop;
using System.Text.Json;
using YESSMobilePWA.Models;

namespace YESSMobilePWA.Services
{
    public class ArchivoService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string DatosKey = "yes_gestor_data";

        public ArchivoService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task GuardarAsync(DatosApp datos)
        {
            string json = JsonSerializer.Serialize(datos);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", DatosKey, json);
        }

        public async Task<DatosApp> CargarAsync()
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", DatosKey);
            if (string.IsNullOrEmpty(json))
                return new DatosApp();
            return JsonSerializer.Deserialize<DatosApp>(json) ?? new DatosApp();
        }
    }
}