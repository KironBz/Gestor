using Microsoft.JSInterop;
using System;
using System.Text.Json;
using YESSMobilePWA.Models;

namespace YESSMobilePWA.Services
{
    public class ArchivoService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string DatosKey = "yes_gestor_data";
        private const int SchemaVersionActual = 2;

        public ArchivoService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        // ==========================================
        // GUARDAR
        // ==========================================
        public async Task GuardarAsync(DatosApp datos)
        {
            // Actualizar lastModified en cada guardado
            datos.Metadata.LastModified = DateTime.UtcNow;

            // Limpiar campos legacy antes de serializar
            // (ya están en __credentials y __metadata)
            datos.LegacyGitHubToken = null;
            datos.LegacyGitHubGistId = null;
            datos.LegacyUltimaExportacion = null;
            datos.LegacyVersion = null;

            string json = JsonSerializer.Serialize(datos);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", DatosKey, json);
        }

        // ==========================================
        // CARGAR + MIGRACIÓN AUTOMÁTICA
        // ==========================================
        public async Task<DatosApp> CargarAsync()
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", DatosKey);

                DatosApp datos;

                if (string.IsNullOrEmpty(json))
                {
                    // Primera vez — datos vacíos con estructura nueva
                    datos = new DatosApp();
                }
                else
                {
                    datos = JsonSerializer.Deserialize<DatosApp>(json) ?? new DatosApp();
                    datos = MigrarSiNecesario(datos);
                }

                return datos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando datos, devolviendo DatosApp vacío: {ex}");
                return new DatosApp();
            }
        }

        // ==========================================
        // MIGRACIÓN
        // ==========================================
        private DatosApp MigrarSiNecesario(DatosApp datos)
        {
            // Detectar si viene del formato viejo (sin __metadata/__credentials)
            bool esFormatoViejo = datos.Metadata.SchemaVersion < SchemaVersionActual
                                  || (datos.Metadata.AppName != "yess");

            if (esFormatoViejo)
            {
                datos = MigrarV1aV2(datos);
            }

            return datos;
        }

        private DatosApp MigrarV1aV2(DatosApp datos)
        {
            Console.WriteLine("Migrando datos de V1 a V2...");

            // Migrar credenciales legacy → __credentials
            if (!string.IsNullOrEmpty(datos.LegacyGitHubToken) &&
                string.IsNullOrEmpty(datos.Credentials.Token))
            {
                datos.Credentials.Token = datos.LegacyGitHubToken;
            }

            if (!string.IsNullOrEmpty(datos.LegacyGitHubGistId) &&
                string.IsNullOrEmpty(datos.Credentials.GistId))
            {
                datos.Credentials.GistId = datos.LegacyGitHubGistId;
            }

            // Migrar última exportación → __metadata.lastSync
            if (datos.LegacyUltimaExportacion.HasValue &&
                !datos.Metadata.LastSync.HasValue)
            {
                datos.Metadata.LastSync = datos.LegacyUltimaExportacion;
            }

            // Limpiar campos legacy
            datos.LegacyGitHubToken = null;
            datos.LegacyGitHubGistId = null;
            datos.LegacyUltimaExportacion = null;
            datos.LegacyVersion = null;

            // Corregir datos inválidos en movimientos (herencia de MigrarV0aV1)
            foreach (var mov in datos.Movimientos)
            {
                if (mov.Monto <= 0)
                    mov.Monto = Math.Abs(mov.Monto);
                if (mov.Plazos.HasValue && mov.Plazos <= 0)
                    mov.Plazos = null;
                if (mov.MontoFinal.HasValue && mov.MontoFinal <= 0)
                    mov.MontoFinal = null;
            }

            // Actualizar versión y metadata
            datos.Metadata.SchemaVersion = SchemaVersionActual;
            datos.Metadata.AppName = "yess";
            datos.Metadata.LastModified = DateTime.UtcNow;
            datos.SyncStatus.DataChanged = true; // Marcar como cambiado para que suba al Gist

            Console.WriteLine("Migración V1→V2 completada.");
            return datos;
        }
    }
}