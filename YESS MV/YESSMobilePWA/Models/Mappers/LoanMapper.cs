using YESSMobilePWA.Models;

namespace YESSMobilePWA.Models.Mappers
{
    /// <summary>
    /// Convierte DeudaPendiente → LoanViewModel.
    /// Centraliza lógica de transformación (usada en Prestamos.razor, etc).
    /// </summary>
    public class LoanMapper
    {
        /// <summary>
        /// Convertir lista de deudas a ViewModels con cálculos de progreso.
        /// </summary>
        public List<LoanViewModel> MapToViewModels(IEnumerable<DeudaPendiente> deudas)
        {
            if (deudas == null) return new List<LoanViewModel>();

            return deudas
                .Select(d => MapToViewModel(d))
                .ToList();
        }

        /// <summary>
        /// Convertir una deuda individual a ViewModel con progreso calculado.
        /// </summary>
        public LoanViewModel MapToViewModel(DeudaPendiente deuda)
        {
            var porcentajeProgreso = CalcularPorcentajeProgreso(deuda);

            return new LoanViewModel
            {
                Contraparte = deuda.Contraparte,
                MontoTotal = deuda.MontoTotal,
                Pagado = deuda.Pagado,
                SaldoPendiente = deuda.SaldoPendiente,
                ReferenciaAuto = deuda.ReferenciaAuto,
                PersonaId = deuda.PersonaId,
                PagosRealizados = deuda.PagosRealizados,
                PlazosTotales = deuda.PlazosTotales,
                ColorCuenta = deuda.ColorCuenta,
                Tipo = deuda.Tipo,
                FechaOcurrido = deuda.FechaOcurrido,
                FechaCompletado = deuda.FechaCompletado,
                PorcentajeProgreso = porcentajeProgreso
            };
        }

        /// <summary>
        /// Calcular porcentaje de progreso de pago/abono.
        /// Maneja dos casos:
        /// - Con plazos: (PagosRealizados / PlazosTotales) * 100
        /// - Sin plazos: (Pagado / MontoTotal) * 100
        /// </summary>
        private double CalcularPorcentajeProgreso(DeudaPendiente deuda)
        {
            if (deuda.PlazosTotales.HasValue && deuda.PlazosTotales.Value > 0)
            {
                return (double)deuda.PagosRealizados / (double)deuda.PlazosTotales.Value * 100;
            }

            if (deuda.MontoTotal == 0) return 0;
            return (double)(deuda.Pagado / deuda.MontoTotal) * 100;
        }
    }

    /// <summary>
    /// ViewModel para mostrar deudas con progreso calculado.
    /// Usado por Prestamos.razor para listas y tablas.
    /// </summary>
    public class LoanViewModel
    {
        public string Contraparte { get; set; } = "";
        public decimal MontoTotal { get; set; }
        public decimal Pagado { get; set; }
        public decimal SaldoPendiente { get; set; }
        public string ReferenciaAuto { get; set; } = "";
        public string PersonaId { get; set; } = "";
        public int PagosRealizados { get; set; }
        public int? PlazosTotales { get; set; }
        public string ColorCuenta { get; set; } = "";
        public string Tipo { get; set; } = "";  // "Acreedor" o "Deudor"
        public DateTime FechaOcurrido { get; set; }
        public DateTime? FechaCompletado { get; set; }
        public double PorcentajeProgreso { get; set; }  // 0-100
    }
}