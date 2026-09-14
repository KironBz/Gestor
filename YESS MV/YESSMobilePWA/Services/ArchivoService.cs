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

                    // Detectar si necesita migración ANTES de migrar
                    bool necesitaMigracion = datos.Metadata.SchemaVersion < SchemaVersionActual
                                             || datos.Metadata.AppName != "yess";

                    datos = MigrarSiNecesario(datos);

                    // Si hubo migración, persistir inmediatamente para que
                    // la próxima carga ya encuentre el formato nuevo
                    if (necesitaMigracion)
                    {
                        await GuardarAsync(datos);
                        Console.WriteLine("Datos migrados y persistidos en localStorage.");
                    }
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
        // MIGRACIÓN — enrutador de versiones
        // ==========================================
        private DatosApp MigrarSiNecesario(DatosApp datos)
        {
            // Migración acumulativa: si hay varios saltos de versión,
            // se aplican en orden hasta llegar a SchemaVersionActual
            while (datos.Metadata.SchemaVersion < SchemaVersionActual
                   || datos.Metadata.AppName != "yess")
            {
                int versionActual = datos.Metadata.SchemaVersion;

                if (versionActual < 2 || datos.Metadata.AppName != "yess")
                {
                    datos = MigrarV1aV2(datos);
                }
                else
                {
                    // Si llegamos aquí sin alcanzar SchemaVersionActual,
                    // algo está mal — salir para evitar bucle infinito
                    Console.WriteLine($"Versión desconocida: {versionActual}. Abortando migración.");
                    break;
                }
            }

            return datos;
        }

        // ==========================================
        // MIGRACIÓN V1 → V2
        // Convierte el formato plano anterior al nuevo
        // formato con __metadata, __credentials, __syncStatus
        // ==========================================
        private DatosApp MigrarV1aV2(DatosApp datos)
        {
            Console.WriteLine("Migrando datos de V1 a V2...");

            // 1. Migrar credenciales legacy → __credentials
            if (!string.IsNullOrEmpty(datos.LegacyGitHubToken) &&
                string.IsNullOrEmpty(datos.Credentials.Token))
            {
                datos.Credentials.Token = datos.LegacyGitHubToken;
                Console.WriteLine("Token migrado de campo legacy a __credentials.");
            }

            if (!string.IsNullOrEmpty(datos.LegacyGitHubGistId) &&
                string.IsNullOrEmpty(datos.Credentials.GistId))
            {
                datos.Credentials.GistId = datos.LegacyGitHubGistId;
                Console.WriteLine("GistId migrado de campo legacy a __credentials.");
            }

            // 2. Migrar última exportación → __metadata.lastSync
            if (datos.LegacyUltimaExportacion.HasValue &&
                !datos.Metadata.LastSync.HasValue)
            {
                datos.Metadata.LastSync = datos.LegacyUltimaExportacion;
                Console.WriteLine("UltimaExportacion migrada a __metadata.lastSync.");
            }

            // 3. Limpiar campos legacy (ya no se necesitan)
            datos.LegacyGitHubToken = null;
            datos.LegacyGitHubGistId = null;
            datos.LegacyUltimaExportacion = null;
            datos.LegacyVersion = null;

            // 4. Corregir datos inválidos en movimientos
            //    (heredado de la migración V0→V1 anterior)
            foreach (var mov in datos.Movimientos)
            {
                if (mov.Monto <= 0)
                    mov.Monto = Math.Abs(mov.Monto);
                if (mov.Plazos.HasValue && mov.Plazos <= 0)
                    mov.Plazos = null;
                if (mov.MontoFinal.HasValue && mov.MontoFinal <= 0)
                    mov.MontoFinal = null;
            }

            // 5. Actualizar metadata al nuevo formato
            datos.Metadata.SchemaVersion = SchemaVersionActual;
            datos.Metadata.AppName = "yess";
            datos.Metadata.LastModified = DateTime.UtcNow;

            // 6. Marcar como cambiado para que suba al Gist en el próximo sync
            datos.SyncStatus.DataChanged = true;

            Console.WriteLine("Migración V1→V2 completada exitosamente.");
            return datos;
        }
    }
}