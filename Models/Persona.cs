using System;
using System.Text.Json.Serialization;

namespace YESS.Models
{
    public class Persona
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        private string _nombre = string.Empty;
        [JsonPropertyName("nombre")]
        public string Nombre
        {
            get => _nombre;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Nombre no puede estar vacío.");
                _nombre = value.Trim();
            }
        }

        private string _tipo = "Ninguno";
        [JsonPropertyName("tipo")]
        public string Tipo
        {
            get => _tipo;
            set
            {
                if (value != "Deudor" && value != "Acreedor" && value != "Ambos" && value != "Ninguno")
                    throw new ArgumentException("Tipo debe ser Deudor, Acreedor, Ambos o Ninguno.");
                _tipo = value;
            }
        }

        public Persona() { }

        public Persona(string nombre, string tipo)
        {
            Id = Guid.NewGuid().ToString();
            Nombre = nombre;
            Tipo = tipo;
        }

        public override string ToString() => $"{Nombre} ({Tipo})";
    }
}