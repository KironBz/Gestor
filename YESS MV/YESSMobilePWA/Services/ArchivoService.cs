using Microsoft.JSInterop;
using System.Text.Json;
using YESSMobilePWA.Models;

namespace YESSMobilePWA.Services
{
    public class GuardadoFallidoException : Exception
    {
        public GuardadoFallidoException(string message, Exception inner) : base(message, inner) { }
    }

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
            string json;
            try
            {
                json = JsonSerializer.Serialize(datos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ArchivoService: error al serializar datos para guardar: {ex.Message}");
                throw new GuardadoFallidoException(
                    "No se pudieron preparar los datos para guardar. Ningún cambio se perdió en pantalla, pero no se guardó en el dispositivo.",
                    ex);
            }

            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", DatosKey, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ArchivoService: error al escribir en localStorage: {ex.Message}");
                throw new GuardadoFallidoException(
                    "No se pudo guardar la información en este dispositivo. Es posible que el almacenamiento local esté lleno. Considera exportar un respaldo y liberar espacio.",
                    ex);
            }
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
                    Console.WriteLine($"ArchivoService: error al deserializar datos guardados: {ex.Message}");
                    await RespaldarJsonCorrupto(json);
                    datos = RecuperarParcialmente(json);
                    seRecuperoParcialmente = true;
                }
            }

            if (datos.Version == 0)
            {
                MigrarV0aV1(datos);
                await GuardarAsync(datos);
            }
            else if (seRecuperoParcialmente)
            {
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

        private void MigrarV0aV1(DatosApp datos)
        {
            foreach (var mov in datos.Movimientos)
            {
                if (mov.Monto <= 0)
                    mov.Monto = Math.Abs(mov.Monto);

                if (mov.Plazos.HasValue && mov.Plazos <= 0)
                    mov.Plazos = null;

                if (mov.MontoFinal.HasValue && mov.MontoFinal <= 0)
                    mov.MontoFinal = null;
            }

            datos.Version = 1;
        }
    }
}