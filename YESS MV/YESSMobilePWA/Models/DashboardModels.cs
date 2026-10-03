namespace YESSMobilePWA.Models
{
    /// <summary>
    /// Models para Dashboard.
    /// Usa decimal para precisión monetaria (no double).
    /// </summary>

    public class TotalItem
    {
        public string Categoria { get; set; } = "";
        public decimal Monto { get; set; }  // ← Cambié de double
        public string? Color { get; set; }
    }

    public class CategoriaItem
    {
        public string Categoria { get; set; } = "";
        public decimal Monto { get; set; }  // ← Cambié de double
    }

    public class GastoMensual
    {
        public string Mes { get; set; } = "";
        public decimal Gasto { get; set; }  // ← Cambié de double
    }

    // ═══════════════════════════════════════════════════════
    // NUEVOS MODELOS (Fase 6)
    // ═══════════════════════════════════════════════════════

    public class MetricasMensuales
    {
        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal SaldoNeto { get; set; }
        public decimal PatrimonioNeto { get; set; }
        public int TotalMovimientos { get; set; }
        public DateTime MesConsultado { get; set; }
    }

    public class ProgresionMetaSummary
    {
        public string MetaId { get; set; } = "";
        public string NombreMeta { get; set; } = "";
        public decimal MontoObjetivo { get; set; }
        public decimal AhorradoActual { get; set; }
        public double PorcentajeProgreso { get; set; }  // 0-100 (double está ok para porcentajes)
        public bool Completada { get; set; }
    }

    public class AnalisisTendencias
    {
        public List<MesTendencia> Meses { get; set; } = new();
        public decimal PromedioMensualIngresos { get; set; }
        public decimal PromedioMensualEgresos { get; set; }
        public string TendenciaGeneral { get; set; } = "";  // "Positiva", "Negativa", "Estable"
    }

    public class MesTendencia
    {
        public int Mes { get; set; }  // 1-12
        public int Año { get; set; }
        public decimal Ingresos { get; set; }
        public decimal Egresos { get; set; }
        public decimal Neto { get; set; }
    }

    public class ResumenCuentas
    {
        public string CuentaId { get; set; } = "";
        public string NombreCuenta { get; set; } = "";
        public decimal Saldo { get; set; }
        public string Visibilidad { get; set; } = "";
        public int TotalMovimientos { get; set; }
    }

    public class ResumenDeudas
    {
        public decimal TotalAcreencias { get; set; }  // Lo que me deben
        public decimal TotalDeudas { get; set; }      // Lo que debo
        public decimal NetoDeudas { get; set; }       // Diferencia
        public int TotalPrestamos { get; set; }
        public int PrestamosActivos { get; set; }
    }
}