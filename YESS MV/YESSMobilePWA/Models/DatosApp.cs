using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace YESSMobilePWA.Models
{
    public class DatosApp
    {
        [JsonPropertyName("version")]
        public int Version { get; set; } = 0;

        // Backup local
        [JsonPropertyName("ultimaExportacion")]
        public System.DateTime? UltimaExportacion { get; set; }

        // Backup en GitHub Gist
        [JsonPropertyName("gitHubToken")]
        public string? GitHubToken { get; set; }

        [JsonPropertyName("gitHubGistId")]
        public string? GitHubGistId { get; set; }

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

        public override string ToString() => $"Movimientos: {Movimientos.Count}, Cuentas: {Cuentas.Count}, Categorías: {Categorias.Count}, Personas: {Personas.Count}";
    }
}