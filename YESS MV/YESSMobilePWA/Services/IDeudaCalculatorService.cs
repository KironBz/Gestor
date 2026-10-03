using YESSMobilePWA.Models;

namespace YESSMobilePWA.Services
{
    /// <summary>
    /// Interfaz para cálculos de deudas (préstamos, cargos, pagos, abonos).
    /// Fuente única de verdad para lógica de deuda.
    /// Consumida por: Prestamos.razor, Resumen.razor, Balance.razor.
    /// </summary>
    public interface IDeudaCalculatorService
    {
        /// <summary>
        /// Conjunto de categorías que representan movimiento de deuda.
        /// Usado para filtrar en métricas de ingreso/gasto.
        /// </summary>
        IReadOnlySet<string> CategoriasDeuda { get; }

        /// <summary>
        /// Cálculo detallado: acreedores (debo), deudores (me deben), completados.
        /// Nivel de detalle requerido por Prestamos.razor.
        /// </summary>
        (List<DeudaPendiente> Acreedores, List<DeudaPendiente> Deudores, List<DeudaPendiente> Completados)
            CalcularDeudasDetalle(DatosApp datos);

        /// <summary>
        /// Cálculo agregado por persona: suma de múltiples préstamos por contraparte.
        /// Nivel de detalle requerido por Resumen.razor y Balance.razor.
        /// Se deriva de CalcularDeudasDetalle (nunca reimplementa).
        /// </summary>
        (List<ResumenDeuda> Acreedores, List<ResumenDeuda> Deudores)
            CalcularDeudasResumen(DatosApp datos);
    }
}