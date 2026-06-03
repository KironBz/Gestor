using System;
using System.Text.Json.Serialization;

namespace Yes_Gestor.Models
{
    public class ItemDeseo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("referencia")]
        public string Referencia { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("precio")]
        public decimal Precio { get; set; }

        [JsonPropertyName("prioridad")]
        public int Prioridad { get; set; }  // 1 (baja) a 5 (alta)

        [JsonPropertyName("adquirido")]
        public bool Adquirido { get; set; }

        public ItemDeseo()
        {
            Id = Guid.NewGuid().ToString();
            Adquirido = false;
        }

        public ItemDeseo(string referencia, string nombre, decimal precio, int prioridad)
        {
            Id = Guid.NewGuid().ToString();
            Referencia = referencia;
            Nombre = nombre;
            Precio = precio;
            Prioridad = prioridad;
            Adquirido = false;
        }
    }
}