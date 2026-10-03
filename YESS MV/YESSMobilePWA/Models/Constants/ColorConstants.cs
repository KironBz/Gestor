namespace YESSMobilePWA.Models.Constants
{
    /// <summary>
    /// Colores centralizados para temas y visualización.
    /// Elimina hardcoding de colores en CSS y C#.
    /// </summary>
    public static class ColorConstants
    {
        // ============================================
        // TEMA OSCURO (PRINCIPAL)
        // ============================================
        public const string BgDark = "#1a1a1a";
        public const string TextLight = "#f0f0f0";
        public const string TextDim = "#ccc";
        public const string TextDimmer = "#aaa";
        public const string BorderDark = "#444";

        // ============================================
        // COLORES DE ESTADO (Bootstrap)
        // ============================================
        public const string Success = "#28a745";
        public const string Danger = "#dc3545";
        public const string Warning = "#fd7e14";
        public const string Info = "#0d6efd";
        public const string Secondary = "#6c757d";

        // ============================================
        // ALIAS SEMÁNTICOS
        // ============================================
        public const string IngresoBg = Success;      // Verde
        public const string EgresoBg = Danger;        // Rojo
        public const string TransferenciaaBg = Info;  // Azul
        public const string PrestamosBg = Warning;    // Naranja

        // ============================================
        // MAPA DE COLORES POR ESTADO
        // ============================================
        public static readonly Dictionary<string, string> ColorPorTipo = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Ingreso", IngresoBg },
            { "Egreso", EgresoBg },
            { "Transferencia", TransferenciaaBg },
            { "Préstamo", PrestamosBg },
            { "Cargo", PrestamosBg },
            { "Pago", Danger },
            { "Abono", Success }
        };

        /// <summary>
        /// Obtener color por tipo de movimiento.
        /// </summary>
        public static string ObtenerColorPorTipo(string tipo)
        {
            return ColorPorTipo.TryGetValue(tipo, out var color) ? color : Secondary;
        }
    }
}