using YESSMobilePWA.Models;

namespace YESSMobilePWA.Models.Mappers
{
    /// <summary>
    /// Convierte Meta → MetaViewModel con cálculos de progreso.
    /// Centraliza lógica de transformación (usada en Metas.razor, Resumen.razor, etc).
    /// </summary>
    public class MetaMapper
    {
        /// <summary>
        /// Convertir lista de metas a ViewModels con progreso calculado.
        /// </summary>
        public List<MetaViewModel> MapToViewModels(
            IEnumerable<Meta> metas,
            IEnumerable<Movimiento> movimientos)
        {
            if (metas == null) return new List<MetaViewModel>();
            if (movimientos == null) movimientos = new List<Movimiento>();

            // Precalcular ahorros por MetaId una sola vez (O(n), no O(n*m))
            // Filtramos MetaId != null y usamos null-forgiving operator (!) en GroupBy
            var ahorrosPorMeta = movimientos
                .Where(m => m.MetaId != null && m.Tipo == "Ingreso")
                .GroupBy(m => m.MetaId!)  // ! = null-forgiving operator
                .ToDictionary(g => g.Key, g => g.Sum(m => m.Monto));

            return metas
                .Select(m => MapToViewModel(m, ahorrosPorMeta))
                .ToList();
        }

        /// <summary>
        /// Convertir una meta individual a ViewModel con progreso calculado.
        /// </summary>
        public MetaViewModel MapToViewModel(
            Meta meta,
            Dictionary<string, decimal>? ahorrosPorMeta = null)
        {
            decimal ahorradoActual = 0;
            if (ahorrosPorMeta?.TryGetValue(meta.Id, out var ahorrado) == true)
            {
                ahorradoActual = ahorrado;
            }

            var porcentajeProgreso = CalcularPorcentajeProgreso(meta.MontoObjetivo, ahorradoActual);

            return new MetaViewModel
            {
                Id = meta.Id,
                Nombre = meta.Nombre,
                MontoObjetivo = meta.MontoObjetivo,
                AhorradoActual = ahorradoActual,
                PorcentajeProgreso = porcentajeProgreso,
                Prioridad = meta.Prioridad,
                FechaCreacion = meta.FechaCreacion,
                Completada = meta.Completada,
                Archivada = meta.Archivada
            };
        }

        /// <summary>
        /// Calcular porcentaje de progreso de meta.
        /// </summary>
        private double CalcularPorcentajeProgreso(decimal montoObjetivo, decimal ahorradoActual)
        {
            if (montoObjetivo == 0) return 0;
            return (double)(ahorradoActual / montoObjetivo) * 100;
        }
    }

    /// <summary>
    /// ViewModel para mostrar metas con progreso calculado.
    /// Usado por Metas.razor y Resumen.razor.
    /// </summary>
    public class MetaViewModel
    {
        public string Id { get; set; } = "";
        public string Nombre { get; set; } = "";
        public decimal MontoObjetivo { get; set; }
        public decimal AhorradoActual { get; set; }
        public double PorcentajeProgreso { get; set; }  // 0-100
        public int Prioridad { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Completada { get; set; }
        public bool Archivada { get; set; }
    }
}