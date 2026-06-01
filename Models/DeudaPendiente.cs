namespace Yes_Gestor.Models
{
    public class DeudaPendiente
    {
        public string Contraparte { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal Pagado { get; set; }
        public decimal SaldoPendiente { get; set; }
        public string ReferenciaAuto { get; set; }
        public string PersonaId { get; set; }
        public int PagosRealizados { get; set; }
        public int? PlazosTotales { get; set; }
        public decimal? CuotaMensual { get; set; }
    }
}