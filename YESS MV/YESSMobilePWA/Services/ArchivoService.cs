using Microsoft.JSInterop;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using YESSMobilePWA.Models;

namespace YESSMobilePWA.Services
{
    public class ArchivoService : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private const string DatosKey = "yes_gestor_data";
        private const int SchemaVersionActual = 2;

        private Timer? _debounceTimer;
        private const int DebounceMs = 60_000;
        private static readonly int[] RetryDelaysMs = { 7_000, 15_000, 20_000 };

        public event Action<SyncState>? OnSyncStateChanged;

        public ArchivoService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task GuardarAsync(DatosApp datos)
        {
            datos.Metadata.LastModified = DateTime.UtcNow;
            datos.SyncStatus.DataChanged = true;

            datos.LegacyGitHubToken = null;
            datos.LegacyGitHubGistId = null;
            datos.LegacyUltimaExportacion = null;
            datos.LegacyVersion = null;

            string json = JsonSerializer.Serialize(datos);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", DatosKey, json);

            IniciarDebounce(datos);
        }

        public async Task<DatosApp> CargarAsync()
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>(
                    "localStorage.getItem", DatosKey);

                DatosApp datos;

                if (string.IsNullOrEmpty(json))
                {
                    datos = new DatosApp();
                }
                else
                {
                    datos = JsonSerializer.Deserialize<DatosApp>(json) ?? new DatosApp();

                    bool necesitaMigracion =
                        datos.Metadata.SchemaVersion < SchemaVersionActual
                        || datos.Metadata.AppName != "yess";

                    datos = MigrarSiNecesario(datos);

                    if (necesitaMigracion)
                    {
                        await GuardarAsync(datos);
                        Console.WriteLine("Datos migrados y persistidos.");
                    }
                }

                return datos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando datos: {ex}");
                return new DatosApp();
            }
        }

        private void IniciarDebounce(DatosApp datos)
        {
            _debounceTimer?.Dispose();

            if (string.IsNullOrEmpty(datos.GitHubToken) ||
                string.IsNullOrEmpty(datos.GitHubGistId))
            {
                return;
            }

            NotificarEstado(SyncState.Pendiente);

            _debounceTimer = new Timer(async _ =>
            {
                await SyncConGistAsync(datos);
            }, null, DebounceMs, Timeout.Infinite);
        }

        private async Task SyncConGistAsync(DatosApp datos)
        {
            if (!datos.SyncStatus.DataChanged) return;
            if (string.IsNullOrEmpty(datos.GitHubToken) ||
                string.IsNullOrEmpty(datos.GitHubGistId)) return;

            NotificarEstado(SyncState.Sincronizando);
            datos.SyncStatus.IsSyncing = true;

            int intentos = 0;
            bool exitoso = false;

            while (intentos <= RetryDelaysMs.Length && !exitoso)
            {
                try
                {
                    exitoso = await IntentarSubirGistAsync(datos);

                    if (exitoso)
                    {
                        datos.SyncStatus.DataChanged = false;
                        datos.SyncStatus.IsSyncing = false;
                        datos.SyncStatus.RetryCount = 0;
                        datos.SyncStatus.LastError = null;
                        datos.SyncStatus.NextRetryAt = null;
                        datos.UltimaExportacion = DateTime.Now;
                        await GuardarSinDebounceAsync(datos);
                        NotificarEstado(SyncState.Sincronizado);
                        Console.WriteLine("Sync automático exitoso.");
                    }
                    else if (intentos < RetryDelaysMs.Length)
                    {
                        int delay = RetryDelaysMs[intentos];
                        datos.SyncStatus.RetryCount = intentos + 1;
                        datos.SyncStatus.NextRetryAt =
                            DateTime.Now.AddMilliseconds(delay);
                        NotificarEstado(SyncState.Reintentando);
                        await Task.Delay(delay);
                    }
                }
                catch (Exception ex)
                {
                    datos.SyncStatus.LastError = ex.Message;
                    Console.WriteLine($"Excepción sync intento {intentos + 1}: {ex.Message}");
                    if (intentos < RetryDelaysMs.Length)
                        await Task.Delay(RetryDelaysMs[intentos]);
                }

                intentos++;
            }

            if (!exitoso)
            {
                datos.SyncStatus.IsSyncing = false;
                await GuardarSinDebounceAsync(datos);
                NotificarEstado(SyncState.Error);
                Console.WriteLine("Sync falló después de todos los reintentos.");
            }
        }

        private async Task<bool> IntentarSubirGistAsync(DatosApp datos)
        {
            var datosParaGist = new
            {
                __metadata = new
                {
                    schemaVersion = datos.Metadata.SchemaVersion,
                    lastSync = DateTime.UtcNow,
                    lastModified = datos.Metadata.LastModified,
                    appName = datos.Metadata.AppName
                },
                movimientos = datos.Movimientos,
                cuentas = datos.Cuentas,
                categorias = datos.Categorias,
                personas = datos.Personas,
                metas = datos.Metas
            };

            // En "true" acomoda por renglones y saltos de linea, "false" es una sola línea
            var json = JsonSerializer.Serialize(datosParaGist,
                new JsonSerializerOptions { WriteIndented = true });

            if (json.Contains("ghp_") || json.Contains("gho_"))
            {
                Console.WriteLine("Sync cancelado: credenciales detectadas.");
                return false;
            }

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("Authorization",
                $"token {datos.GitHubToken}");
            http.DefaultRequestHeaders.Add("User-Agent", "YESSMobilePWA");
            http.Timeout = TimeSpan.FromSeconds(15);

            var update = new
            {
                files = new System.Collections.Generic.Dictionary<string, object>
                {
                    ["yes_gestor_data.json"] = new { content = json }
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(update),
                Encoding.UTF8,
                "application/json");

            var response = await http.PatchAsync(
                $"https://api.github.com/gists/{datos.GitHubGistId}", content);

            if (response.IsSuccessStatusCode) return true;

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                datos.GitHubToken = null;
                await GuardarSinDebounceAsync(datos);
                NotificarEstado(SyncState.Error);
            }

            Console.WriteLine($"Sync: error HTTP {response.StatusCode}");
            return false;
        }

        private async Task GuardarSinDebounceAsync(DatosApp datos)
        {
            datos.Metadata.LastModified = DateTime.UtcNow;
            datos.LegacyGitHubToken = null;
            datos.LegacyGitHubGistId = null;
            datos.LegacyUltimaExportacion = null;
            datos.LegacyVersion = null;

            string json = JsonSerializer.Serialize(datos);
            await _jsRuntime.InvokeVoidAsync(
                "localStorage.setItem", DatosKey, json);
        }

        private void NotificarEstado(SyncState estado)
        {
            OnSyncStateChanged?.Invoke(estado);
        }

        private DatosApp MigrarSiNecesario(DatosApp datos)
        {
            while (datos.Metadata.SchemaVersion < SchemaVersionActual
                   || datos.Metadata.AppName != "yess")
            {
                int v = datos.Metadata.SchemaVersion;
                if (v < 2 || datos.Metadata.AppName != "yess")
                    datos = MigrarV1aV2(datos);
                else
                {
                    Console.WriteLine($"Versión desconocida: {v}. Abortando.");
                    break;
                }
            }
            return datos;
        }

        private DatosApp MigrarV1aV2(DatosApp datos)
        {
            Console.WriteLine("Migrando V1 → V2...");

            if (!string.IsNullOrEmpty(datos.LegacyGitHubToken) &&
                string.IsNullOrEmpty(datos.Credentials.Token))
                datos.Credentials.Token = datos.LegacyGitHubToken;

            if (!string.IsNullOrEmpty(datos.LegacyGitHubGistId) &&
                string.IsNullOrEmpty(datos.Credentials.GistId))
                datos.Credentials.GistId = datos.LegacyGitHubGistId;

            if (datos.LegacyUltimaExportacion.HasValue &&
                !datos.Metadata.LastSync.HasValue)
                datos.Metadata.LastSync = datos.LegacyUltimaExportacion;

            datos.LegacyGitHubToken = null;
            datos.LegacyGitHubGistId = null;
            datos.LegacyUltimaExportacion = null;
            datos.LegacyVersion = null;

            foreach (var mov in datos.Movimientos)
            {
                if (mov.Monto <= 0) mov.Monto = Math.Abs(mov.Monto);
                if (mov.Plazos.HasValue && mov.Plazos <= 0) mov.Plazos = null;
                if (mov.MontoFinal.HasValue && mov.MontoFinal <= 0)
                    mov.MontoFinal = null;
            }

            datos.Metadata.SchemaVersion = SchemaVersionActual;
            datos.Metadata.AppName = "yess";
            datos.Metadata.LastModified = DateTime.UtcNow;
            datos.SyncStatus.DataChanged = true;

            Console.WriteLine("Migración V1→V2 completada.");
            return datos;
        }

        public async ValueTask DisposeAsync()
        {
            _debounceTimer?.Dispose();
            await ValueTask.CompletedTask;
        }
    }

    public enum SyncState
    {
        Inactivo,
        Pendiente,
        Sincronizando,
        Reintentando,
        Sincronizado,
        Error
    }
}