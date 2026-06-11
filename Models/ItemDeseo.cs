using System;
using System.Text.Json.Serialization;

namespace YESS.Models
{
    public class ItemDeseo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("referencia")]
        public string Referencia { get; set; } = string.Empty;

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("precio")]
        public decimal Precio { get; set; }

        [JsonPropertyName("prioridad")]
        public int Prioridad { get; set; }

        [JsonPropertyName("adquirido")]
        public bool Adquirido { get; set; }

        public ItemDeseo()
        {
            Id = Guid.NewGuid().ToString();
            Adquirido = false;
        }

        public ItemDeseo(string referencia, string nombre, decimal precio, int prioridad) : this()
        {
            Referencia = referencia;
            Nombre = nombre;
            Precio = precio;
            Prioridad = prioridad;
        }
    }
}