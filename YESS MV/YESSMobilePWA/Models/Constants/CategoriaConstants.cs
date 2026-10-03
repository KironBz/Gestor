namespace YESSMobilePWA.Models.Constants
{
    /// <summary>
    /// Categorías de movimientos que representan flujo de deuda, no ingreso/gasto real.
    /// Consumida por: Resumen.razor, Dashboard.razor, DeudaCalculatorService.cs
    /// Fuente única de verdad para categorías de deuda.
    /// </summary>
    public static class CategoriaConstants
    {
        // ============================================
        // CATEGORÍAS DE DEUDA (préstamos y pagos)
        // ============================================
        public const string Prestamo = "Préstamo";
        public const string Cargo = "Cargo";
        public const string Pago = "Pago";
        public const string Abono = "Abono";

        // ============================================
        // OTROS (para referencia futura)
        // ============================================
        public const string Transferencia = "Transferencia";

        // ============================================
        // COLECCIONES PREDEFINIDAS
        // ============================================

        /// <summary>
        /// Conjunto de todas las categorías que representan movimiento de deuda.
        /// Usado para filtrar en métricas de ingreso/gasto.
        /// </summary>
        public static readonly IReadOnlySet<string> CategoriasDeuda = 
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Prestamo,
                Cargo,
                Pago,
                Abono
            };

        /// <summary>
        /// Verificar si una categoría es movimiento de deuda.
        /// </summary>
        public static bool EsCategoriaDeuда(string categoria)
        {
            return CategoriasDeuda.Contains(categoria);
        }
    }
}