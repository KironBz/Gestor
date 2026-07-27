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
            bool seRecuperoParcialmente = false;

            if (string.IsNullOrEmpty(json))
            {
                datos = new DatosApp();
            }
            else
            {
                try
                {
                    datos = JsonSerializer.Deserialize<DatosApp>(json) ?? new DatosApp();
                }
                catch (Exception ex)
                {
                    // Al menos un valor del JSON viola las validaciones del modelo
                    // (ej. un Movimiento con Monto <= 0), lo que hace fallar la
                    // deserialización de TODO el documento de un solo golpe.
                    // Se respalda el JSON crudo y se intenta rescatar elemento por
                    // elemento en vez de perder todos los datos.
                    Console.WriteLine($"ArchivoService: error al deserializar datos guardados: {ex.Message}");
                    await RespaldarJsonCorrupto(json);
                    datos = RecuperarParcialmente(json);
                    seRecuperoParcialmente = true;
                }
            }

            // Migración a versión 1 (si es necesario)
            if (datos.Version == 0)
            {
                await MigrarV0aV1(datos);
                await GuardarAsync(datos); // guarda después de migrar
            }
            else if (seRecuperoParcialmente)
            {
                // Aunque no requiera migración de versión, se guarda la versión
                // ya "limpia" (sin los elementos corruptos descartados) para que
                // el próximo CargarAsync no vuelva a tronar con los mismos datos.
                await GuardarAsync(datos);
            }

            return datos;
        }

        private async Task RespaldarJsonCorrupto(string json)
        {
            try
            {
                string backupKey = $"{DatosKey}_backup_corrupto_{DateTime.Now:yyyyMMdd_HHmmss}";
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", backupKey, json);
            }
            catch
            {
                // Si ni siquiera se puede respaldar, se prioriza que la app siga
                // funcionando sobre preservar el respaldo — no se relanza el error.
            }
        }

        /// <summary>
        /// Parsea el JSON como árbol genérico y reconstruye cada colección elemento
        /// por elemento, descartando solo los registros individuales que fallen su
        /// propia deserialización (ej. por violar una validación del modelo), en vez
        /// de perder el documento completo.
        /// </summary>
        private DatosApp RecuperarParcialmente(string json)
        {
            var datos = new DatosApp();

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("version", out var versionEl) && versionEl.TryGetInt32(out var version))
                    datos.Version = version;

                if (root.TryGetProperty("ultimaExportacion", out var ultExpEl) &&
                    ultExpEl.ValueKind != JsonValueKind.Null &&
                    ultExpEl.TryGetDateTime(out var fecha))
                    datos.UltimaExportacion = fecha;

                if (root.TryGetProperty("gitHubToken", out var tokenEl) && tokenEl.ValueKind == JsonValueKind.String)
                    datos.GitHubToken = tokenEl.GetString();

                if (root.TryGetProperty("gitHubGistId", out var gistEl) && gistEl.ValueKind == JsonValueKind.String)
                    datos.GitHubGistId = gistEl.GetString();

                if (root.TryGetProperty("movimientos", out var movEl))
                    datos.Movimientos = DeserializarListaConTolerancia<Movimiento>(movEl, "movimientos");

                if (root.TryGetProperty("cuentas", out var cuentasEl))
                    datos.Cuentas = DeserializarListaConTolerancia<Cuenta>(cuentasEl, "cuentas");

                if (root.TryGetProperty("categorias", out var catEl))
                    datos.Categorias = DeserializarListaConTolerancia<Categoria>(catEl, "categorias");

                if (root.TryGetProperty("personas", out var persEl))
                    datos.Personas = DeserializarListaConTolerancia<Persona>(persEl, "personas");

                if (root.TryGetProperty("metas", out var metasEl))
                    datos.Metas = DeserializarListaConTolerancia<Meta>(metasEl, "metas");
            }
            catch (Exception ex)
            {
                // El JSON ni siquiera es válido como texto JSON (corrupción total,
                // no solo un valor fuera de rango) — no hay nada que rescatar.
                Console.WriteLine($"ArchivoService: no se pudo recuperar nada, JSON ilegible: {ex.Message}");
            }

            return datos;
        }

        private List<T> DeserializarListaConTolerancia<T>(JsonElement arrayElement, string nombreColeccion)
        {
            var resultado = new List<T>();
            if (arrayElement.ValueKind != JsonValueKind.Array) return resultado;

            int indice = 0;
            foreach (var elemento in arrayElement.EnumerateArray())
            {
                try
                {
                    var item = elemento.Deserialize<T>();
                    if (item != null) resultado.Add(item);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ArchivoService: se descartó un elemento inválido en '{nombreColeccion}' (índice {indice}): {ex.Message}");
                }
                indice++;
            }
            return resultado;
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

            // 2. Asegurar que todas las metas tengan un Id (ya lo tienen por constructor)
            // No hay conversión de MetasAhorro porque ya no existe.

            // 3. Marcar versión migrada
            datos.Version = 1;
        }
    }
}