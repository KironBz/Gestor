using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace YESSMobilePWA.Models
{
    public class DatosApp
    {
        // ==========================================
        // METADATA — control de versión y timestamps
        // ==========================================
        [JsonPropertyName("__metadata")]
        public AppMetadata Metadata { get; set; } = new();

        // ==========================================
        // CREDENTIALS — token y gistId de GitHub
        // ==========================================
        [JsonPropertyName("__credentials")]
        public AppCredentials Credentials { get; set; } = new();

        // ==========================================
        // SYNC STATUS — estado interno de sincronización
        // ==========================================
        [JsonPropertyName("__syncStatus")]
        public AppSyncStatus SyncStatus { get; set; } = new();

        // ==========================================
        // CAMPOS LEGACY — mantenidos para migración
        // Se leen del JSON viejo pero NO se escriben
        // en el nuevo formato (ignorados al serializar
        // si ya están en __credentials/__metadata)
        // ==========================================
        [JsonPropertyName("version")]
        public int? LegacyVersion { get; set; }

        [JsonPropertyName("ultimaExportacion")]
        public DateTime? LegacyUltimaExportacion { get; set; }

        [JsonPropertyName("gitHubToken")]
        public string? LegacyGitHubToken { get; set; }

        [JsonPropertyName("gitHubGistId")]
        public string? LegacyGitHubGistId { get; set; }

        // ==========================================
        // DATOS DE LA APP
        // ==========================================
        [JsonPropertyName("movimientos")]
        public List<Movimiento> Movimientos { get; set; } = new();

        [JsonPropertyName("cuentas")]
        public List<Cuenta> Cuentas { get; set; } = new();

        [JsonPropertyName("categorias")]
        public List<Categoria> Categorias { get; set; } = new();

        [JsonPropertyName("personas")]
        public List<Persona> Personas { get; set; } = new();

        [JsonPropertyName("metas")]
        public List<Meta> Metas { get; set; } = new();

        // ==========================================
        // PROPIEDADES DE COMPATIBILIDAD
        // Permiten que el resto del código siga usando
        // GitHubToken, GitHubGistId, UltimaExportacion
        // sin cambiar nada en Configuracion.razor u otras páginas
        // ==========================================
        [JsonIgnore]
        public string? GitHubToken
        {
            get => Credentials.Token;
            set => Credentials.Token = value;
        }

        [JsonIgnore]
        public string? GitHubGistId
        {
            get => Credentials.GistId;
            set => Credentials.GistId = value;
        }

        [JsonIgnore]
        public DateTime? UltimaExportacion
        {
            get => Metadata.LastSync;
            set => Metadata.LastSync = value;
        }

        [JsonIgnore]
        public int Version
        {
            get => Metadata.SchemaVersion;
            set => Metadata.SchemaVersion = value;
        }

        public override string ToString() =>
            $"Movimientos: {Movimientos.Count}, Cuentas: {Cuentas.Count}, " +
            $"Categorías: {Categorias.Count}, Personas: {Personas.Count}";
    }

    // ==========================================
    // CLASES DE SOPORTE
    // ==========================================

    public class AppMetadata
    {
        [JsonPropertyName("schemaVersion")]
        public int SchemaVersion { get; set; } = 2;

        [JsonPropertyName("lastSync")]
        public DateTime? LastSync { get; set; }

        [JsonPropertyName("lastModified")]
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("appName")]
        public string AppName { get; set; } = "yess";
    }

    public class AppCredentials
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("gistId")]
        public string? GistId { get; set; }
    }

    public class AppSyncStatus
    {
        [JsonPropertyName("dataChanged")]
        public bool DataChanged { get; set; } = false;

        [JsonPropertyName("isSyncing")]
        public bool IsSyncing { get; set; } = false;

        [JsonPropertyName("lastError")]
        public string? LastError { get; set; }

        [JsonPropertyName("retryCount")]
        public int RetryCount { get; set; } = 0;

        [JsonPropertyName("nextRetryAt")]
        public DateTime? NextRetryAt { get; set; }
    }
}