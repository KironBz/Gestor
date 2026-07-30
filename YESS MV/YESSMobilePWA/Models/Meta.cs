using System;
using System.Text.Json.Serialization;

namespace YESSMobilePWA.Models
{
    public class Meta
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        private string _nombre = "";
        [JsonPropertyName("nombre")]
        public string Nombre
        {
            get => _nombre;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El nombre no puede estar vacío.");
                _nombre = value.Trim();
            }
        }

        private decimal _montoObjetivo;
        [JsonPropertyName("montoObjetivo")]
        public decimal MontoObjetivo
        {
            get => _montoObjetivo;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("El monto objetivo debe ser mayor a cero.");
                _montoObjetivo = value;
            }
        }

        [JsonPropertyName("prioridad")]
        public int Prioridad { get; set; } // 1 = más urgente

        [JsonPropertyName("icono")]
        public string Icono { get; set; } = "🎯";

        [JsonPropertyName("fechaCreacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [JsonPropertyName("completada")]
        public bool Completada { get; set; } = false;

        [JsonPropertyName("archivada")]
        public bool Archivada { get; set; } = false;

        [JsonPropertyName("fechaCompletada")]
        public DateTime? FechaCompletada { get; set; }

        [JsonPropertyName("fechaArchivada")]
        public DateTime? FechaArchivada { get; set; }

        // Este campo no se guarda, se calcula
        [JsonIgnore]
        public decimal AhorradoActual { get; set; }

        public Meta() { }

        public Meta(string nombre, decimal montoObjetivo, int prioridad)
        {
            Id = Guid.NewGuid().ToString();
            Nombre = nombre;
            MontoObjetivo = montoObjetivo;
            Prioridad = prioridad;
            FechaCreacion = DateTime.Now;
            Completada = false;
            Archivada = false;
        }
    }
}