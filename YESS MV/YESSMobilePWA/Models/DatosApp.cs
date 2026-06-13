using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace YESSMobilePWA.Models
{
    public class DatosApp
    {
        [JsonPropertyName("version")]
        public int Version { get; set; } = 0;   // ← NUEVO: control de migraciones

        [JsonPropertyName("movimientos")]
        public List<Movimiento> Movimientos { get; set; } = new();

        [JsonPropertyName("cuentas")]
        public List<Cuenta> Cuentas { get; set; } = new();

        [JsonPropertyName("categorias")]
        public List<Categoria> Categorias { get; set; } = new();

        [JsonPropertyName("personas")]
        public List<Persona> Personas { get; set; } = new();

        // Elimina o comenta metasAhorro – ya no existe
        // [JsonPropertyName("metasAhorro")]
        // public List<MetaAhorro> MetasAhorro { get; set; } = new();

        [JsonPropertyName("itemsDeseo")]
        public List<ItemDeseo> ItemsDeseo { get; set; } = new();

        [JsonPropertyName("metas")]
        public List<Meta> Metas { get; set; } = new();

        public override string ToString() => $"Movimientos: {Movimientos.Count}, Cuentas: {Cuentas.Count}, Categorías: {Categorias.Count}, Personas: {Personas.Count}";
    }
}