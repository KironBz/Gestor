using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Yes_Gestor.Models
{
    public class DatosApp
    {
        [JsonPropertyName("movimientos")]
        public List<Movimiento> Movimientos { get; set; } = new();

        [JsonPropertyName("cuentas")]
        public List<Cuenta> Cuentas { get; set; } = new();

        [JsonPropertyName("categorias")]
        public List<Categoria> Categorias { get; set; } = new();

        [JsonPropertyName("personas")]
        public List<Persona> Personas { get; set; } = new();

        public override string ToString() => $"Movimientos: {Movimientos.Count}, Cuentas: {Cuentas.Count}, Categorías: {Categorias.Count}, Personas: {Personas.Count}";
    }
}