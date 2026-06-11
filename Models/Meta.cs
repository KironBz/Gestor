using System;
using System.Text.Json.Serialization;

namespace YESS.Models
{
    public class Meta
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("montoObjetivo")]
        public decimal MontoObjetivo { get; set; }

        [JsonPropertyName("prioridad")]
        public int Prioridad { get; set; }  // 1 = más urgente

        [JsonPropertyName("fechaCreacion")]
        public DateTime FechaCreacion { get; set; }

        [JsonPropertyName("completada")]
        public bool Completada { get; set; }

        [JsonPropertyName("archivada")]
        public bool Archivada { get; set; }

        [JsonPropertyName("fechaCompletada")]
        public DateTime? FechaCompletada { get; set; }

        [JsonPropertyName("fechaArchivada")]
        public DateTime? FechaArchivada { get; set; }

        public Meta()
        {
            Id = Guid.NewGuid().ToString();
            FechaCreacion = DateTime.Now;
            Completada = false;
            Archivada = false;
        }

        public Meta(string nombre, decimal montoObjetivo, int prioridad) : this()
        {
            Nombre = nombre;
            MontoObjetivo = montoObjetivo;
            Prioridad = prioridad;
        }

        public decimal AhorradoActual { get; set; }
    }
}