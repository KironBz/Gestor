using System;
using System.Text.Json.Serialization;

namespace YESSMobilePWA.Models
{
    public class Cuenta
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
                    throw new ArgumentException("El nombre no puede estar vacío.");
                _nombre = value.Trim();
            }
        }

        private string _visibilidad;
        [JsonPropertyName("visibilidad")]
        public string Visibilidad
        {
            get => _visibilidad;
            set
            {
                if (value != "Corriente" && value != "Oculto" && value != "Ajeno" && value != "Transporte")
                    throw new ArgumentException("Visibilidad debe ser Corriente, Oculto, Ajeno o Transporte.");
                _visibilidad = value;
            }
        }

        [JsonPropertyName("saldoInicial")]
        public decimal SaldoInicial { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; } = "#FFFFFF";

        [JsonIgnore]
        public decimal SaldoActual { get; set; }

        public Cuenta() { }

        public Cuenta(string nombre, string visibilidad, decimal saldoInicial = 0)
        {
            Id = Guid.NewGuid().ToString();
            Nombre = nombre;
            Visibilidad = visibilidad;
            SaldoInicial = saldoInicial;
        }

        public override string ToString() => $"{Nombre} ({Visibilidad}) - Saldo inicial: {SaldoInicial:C}";
    }
}