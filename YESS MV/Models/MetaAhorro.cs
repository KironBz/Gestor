using System;
using System.Text.Json.Serialization;

namespace Yes_Gestor.Models
{
    public class MetaAhorro
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("referencia")]
        public string Referencia { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("montoObjetivo")]
        public decimal MontoObjetivo { get; set; }

        [JsonPropertyName("ahorradoManual")]
        public decimal AhorradoManual { get; set; }  // Si el usuario no asigna movimientos

        [JsonPropertyName("fechaCreacion")]
        public DateTime FechaCreacion { get; set; }

        public MetaAhorro()
        {
            Id = Guid.NewGuid().ToString();
            FechaCreacion = DateTime.Now;
        }

        public MetaAhorro(string referencia, string nombre, decimal montoObjetivo)
        {
            Id = Guid.NewGuid().ToString();
            Referencia = referencia;
            Nombre = nombre;
            MontoObjetivo = montoObjetivo;
            AhorradoManual = 0;
            FechaCreacion = DateTime.Now;
        }

        public decimal AhorradoTotal => AhorradoManual; // Por ahora solo manual. Luego sumaremos movimientos opcionales
    }
}