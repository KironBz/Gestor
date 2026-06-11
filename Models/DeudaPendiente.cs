using System;

namespace YESS.Models
{
    public class DeudaPendiente
    {
        public string Contraparte { get; set; } = string.Empty;
        public decimal MontoTotal { get; set; }
        public decimal Pagado { get; set; }
        public decimal SaldoPendiente { get; set; }
        public string ReferenciaAuto { get; set; } = string.Empty;
        public string PersonaId { get; set; } = string.Empty;
        public int PagosRealizados { get; set; }
        public int? PlazosTotales { get; set; }
        public decimal? CuotaMensual { get; set; }
        public string ColorCuenta { get; set; } = "#FFFFFF";
        public DateTime? FechaCompletado { get; set; }
        public string Tipo { get; set; } = string.Empty; // "Acreedor" o "Deudor" para completados
    }
}