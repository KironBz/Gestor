using System;
using System.Text.Json.Serialization;

namespace YESS.Models
{
    public class MetaAhorro
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("referencia")]
        public string Referencia { get; set; } = string.Empty;

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("montoObjetivo")]
        public decimal MontoObjetivo { get; set; }

        [JsonPropertyName("ahorradoManual")]
        public decimal AhorradoManual { get; set; }

        [JsonPropertyName("fechaCreacion")]
        public DateTime FechaCreacion { get; set; }

        public MetaAhorro()
        {
            Id = Guid.NewGuid().ToString();
            FechaCreacion = DateTime.Now;
        }

        public MetaAhorro(string referencia, string nombre, decimal montoObjetivo) : this()
        {
            Referencia = referencia;
            Nombre = nombre;
            MontoObjetivo = montoObjetivo;
            AhorradoManual = 0;
        }

        public decimal AhorradoTotal => AhorradoManual;
    }
}