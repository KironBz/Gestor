using System;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Yes_Gestor.Models
{
    public class Movimiento
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("fechaOcurrido")]
        public DateTime FechaOcurrido { get; set; }

        [JsonPropertyName("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        private string _tipo;
        [JsonPropertyName("tipo")]
        public string Tipo
        {
            get => _tipo;
            set
            {
                if (value != "Ingreso" && value != "Egreso" && value != "Transferencia")
                    throw new ArgumentException("Tipo debe ser Ingreso, Egreso o Transferencia.");
                _tipo = value;
            }
        }

        private string _categoria;
        [JsonPropertyName("categoria")]
        public string Categoria
        {
            get => _categoria;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("La categoría no puede estar vacía.");
                _categoria = value.Trim();
            }
        }

        [JsonPropertyName("cuentaId")]
        public string CuentaId { get; set; }

        [JsonPropertyName("categoriaId")]
        public string CategoriaId { get; set; }

        [JsonPropertyName("personaId")]
        public string PersonaId { get; set; }

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; }

        private decimal _monto;
        [JsonPropertyName("monto")]
        public decimal Monto
        {
            get => _monto;
            set
            {
                if (value <= 0) throw new ArgumentException("El monto debe ser mayor a cero.");
                _monto = value;
            }
        }

        [JsonPropertyName("montoFinal")]
        public decimal? MontoFinal { get; set; }

        [JsonPropertyName("plazos")]
        public int? Plazos { get; set; }

        [JsonPropertyName("referenciaAuto")]
        public string ReferenciaAuto { get; set; }



        // Constructor para deserialización
        public Movimiento() { }

        // Constructor para crear nuevo movimiento
        public Movimiento(
            DateTime? fechaOcurrido,
            string tipo,
            string categoria,
            string cuentaId,
            string categoriaId,
            decimal monto,
            string personaId = null,
            string descripcion = null,
            decimal? montoFinal = null,
            int? plazos = null)
        {
            Id = Guid.NewGuid().ToString();
            FechaOcurrido = fechaOcurrido ?? DateTime.Today;
            FechaRegistro = DateTime.Now;
            Tipo = tipo;
            Categoria = categoria;
            CuentaId = cuentaId;
            CategoriaId = categoriaId;
            Monto = monto;
            PersonaId = personaId;
            Descripcion = descripcion;

            bool esPrestamoRecibido = (tipo == "Ingreso" && categoria == "Préstamo");
            bool esCargoCompleto = (tipo == "Egreso" && categoria == "Cargo" && montoFinal != null && plazos != null);

            if (esPrestamoRecibido)
            {
                if (montoFinal == null || plazos == null)
                    throw new ArgumentException("Para préstamos recibidos, MontoFinal y Plazos son obligatorios.");
                MontoFinal = montoFinal;
                Plazos = plazos;
                GenerarReferenciaAuto();
            }
            else if (esCargoCompleto)
            {
                MontoFinal = montoFinal;
                Plazos = plazos;
                GenerarReferenciaAuto();
            }
            else
            {
                // Para cargos simples o movimientos normales, no se genera referencia
                MontoFinal = null;
                Plazos = null;
            }
        }

        public void GenerarReferenciaAuto()
        {
            string prefijo = (Tipo == "Ingreso" && Categoria == "Préstamo") ? "PRE" : "CAR";
            string fechaStr = FechaOcurrido.ToString("yyyyMMdd");

            string descLimpia = "NA";
            if (!string.IsNullOrEmpty(Descripcion))
            {
                descLimpia = Regex.Replace(Descripcion, "[^a-zA-Z0-9]", "");
                if (string.IsNullOrEmpty(descLimpia))
                    descLimpia = "NA";
                else if (descLimpia.Length > 5)
                    descLimpia = descLimpia.Substring(0, 5);
            }

            ReferenciaAuto = $"{prefijo}-{fechaStr}-{descLimpia}-{Monto}";
        }

        public int Signo() => Tipo == "Ingreso" ? 1 : (Tipo == "Egreso" ? -1 : 0);

        public override string ToString() => $"[{FechaOcurrido:dd-MMM-yyyy}] {Tipo} - {Categoria} - {Monto:C} - CuentaId:{CuentaId?.Substring(0, 5)}...";
    }
}