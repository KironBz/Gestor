using System;
using System.Text.Json.Serialization;

namespace Yes_Gestor.Models
{
    public class Categoria
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        private string _nombre;
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

        private string _tipoPermitido;
        [JsonPropertyName("tipoPermitido")]
        public string TipoPermitido
        {
            get => _tipoPermitido;
            set
            {
                if (value != "Ingreso" && value != "Egreso" && value != "Ambos")
                    throw new ArgumentException("TipoPermitido debe ser Ingreso, Egreso o Ambos.");
                _tipoPermitido = value;
            }
        }

        public Categoria() { }

        public Categoria(string nombre, string tipoPermitido)
        {
            Id = Guid.NewGuid().ToString();
            Nombre = nombre;
            TipoPermitido = tipoPermitido;
        }

        public override string ToString() => $"{Nombre} ({TipoPermitido})";
    }
}