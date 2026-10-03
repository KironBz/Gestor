using YESSMobilePWA.Models;

namespace YESSMobilePWA.Models.Mappers
{
    /// <summary>
    /// Convierte Movimiento → MovimientoViewModel.
    /// Centraliza lógica de transformación (usada en Resumen.razor, etc).
    /// </summary>
    public class MovementMapper
    {
        /// <summary>
        /// Convertir lista de movimientos a ViewModels con lookup de cuentas.
        /// </summary>
        public List<MovimientoViewModel> MapToViewModels(
            IEnumerable<Movimiento> movimientos,
            Dictionary<string, string> mapaCuentas)
        {
            if (movimientos == null) return new List<MovimientoViewModel>();
            if (mapaCuentas == null) mapaCuentas = new Dictionary<string, string>();

            return movimientos
                .Select(m => MapToViewModel(m, mapaCuentas))
                .ToList();
        }

        /// <summary>
        /// Convertir un movimiento individual a ViewModel.
        /// </summary>
        public MovimientoViewModel MapToViewModel(
            Movimiento movimiento,
            Dictionary<string, string> mapaCuentas)
        {
            var nombreCuenta = mapaCuentas.GetValueOrDefault(movimiento.CuentaId) ?? "Desconocida";

            return new MovimientoViewModel
            {
                FechaOcurrido = movimiento.FechaOcurrido,
                FechaRegistro = movimiento.FechaRegistro,
                NombreCuenta = nombreCuenta,
                Categoria = movimiento.Categoria,
                Descripcion = movimiento.Descripcion ?? "",
                Monto = movimiento.Monto,
                Tipo = movimiento.Tipo
            };
        }
    }

    /// <summary>
    /// ViewModel para mostrar movimientos en tablas/listas.
    /// Usado por Resumen.razor para últimos movimientos.
    /// </summary>
    public class MovimientoViewModel
    {
        public DateTime FechaOcurrido { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string NombreCuenta { get; set; } = "";
        public string Categoria { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public decimal Monto { get; set; }
        public string Tipo { get; set; } = "";
    }
}