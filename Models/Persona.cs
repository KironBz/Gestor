using System;
using System.Text.Json.Serialization;

namespace Yes_Gestor.Models
{
    public class Persona
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        private string _nombre;
        [JsonPropertyName("nombre")]
        public string Nombre
        {
            get => _nombre;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Nombre no puede estar vacío.");
                _nombre = value.Trim();
            }
        }

        private string _tipo;
        [JsonPropertyName("tipo")]
        public string Tipo
        {
            get => _tipo;
            set
            {
                if (value != "Deudor" && value != "Acreedor" && value != "Ambos")
                    throw new ArgumentException("Tipo debe ser Deudor, Acreedor o Ambos.");
                _tipo = value;
            }
        }

        public Persona() { }
        public Persona(string nombre, string tipo)
        {
            Nombre = nombre;
            Tipo = tipo;
        }

        public override string ToString() => $"{Nombre} ({Tipo})";
    }
}