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
            DatosApp datos;

            if (string.IsNullOrEmpty(json))
            {
                datos = new DatosApp();
            }
            else
            {
                datos = JsonSerializer.Deserialize<DatosApp>(json) ?? new DatosApp();
            }

            // Migración a versión 1 (si es necesario)
            if (datos.Version == 0)
            {
                await MigrarV0aV1(datos);
                await GuardarAsync(datos); // guarda después de migrar
            }

            return datos;
        }

        private async Task MigrarV0aV1(DatosApp datos)
        {
            // 1. Corregir datos inválidos en movimientos
            foreach (var mov in datos.Movimientos)
            {
                if (mov.Monto <= 0)
                    mov.Monto = Math.Abs(mov.Monto); // si es negativo, lo hacemos positivo

                if (mov.Plazos.HasValue && mov.Plazos <= 0)
                    mov.Plazos = null;

                if (mov.MontoFinal.HasValue && mov.MontoFinal <= 0)
                    mov.MontoFinal = null;
            }

            // 2. Unificar metas (si existiera la lista metasAhorro, la fusionamos)
            // En tu JSON actual metasAhorro está vacía, pero por si acaso:
            if (datos.GetType().GetProperty("MetasAhorro") != null)
            {
                var metasAhorro = datos.GetType().GetProperty("MetasAhorro")?.GetValue(datos) as System.Collections.IList;
                if (metasAhorro != null && metasAhorro.Count > 0)
                {
                    // Convertir cada MetaAhorro a Meta y agregar a datos.Metas
                    foreach (var item in metasAhorro)
                    {
                        dynamic ma = item;
                        var nuevaMeta = new Meta
                        {
                            Id = ma.Id,
                            Nombre = ma.Nombre,
                            MontoObjetivo = ma.MontoObjetivo,
                            Prioridad = 3, // prioridad media por defecto
                            FechaCreacion = ma.FechaCreacion,
                            Referencia = ma.Referencia,
                            Completada = (ma.AhorradoManual >= ma.MontoObjetivo)
                        };
                        datos.Metas.Add(nuevaMeta);
                    }
                    // Eliminar la propiedad MetasAhorro de la instancia (no se guardará)
                    // Nota: Esto es más complejo; como no tienes datos, lo omitimos.
                }
            }

            // 3. Marcar versión migrada
            datos.Version = 1;
        }
    }
}