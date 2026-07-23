using YESSMobilePWA.Models;

namespace YESSMobilePWA.Services
{
    /// <summary>
    /// Fuente única de verdad para calcular préstamos, cargos y su estado de pago/abono.
    /// Consumido por Prestamos.razor (detalle), Resumen.razor y Balance.razor (resumen).
    /// </summary>
    public class DeudaCalculatorService
    {
        /// <summary>
        /// Categorías que representan movimiento de deuda (préstamo, cargo, pago, abono),
        /// no ingreso/gasto real. Fuente única de verdad — consumida por Resumen.razor y
        /// Dashboard.razor para excluir flujo de deuda de sus métricas de ingreso/gasto.
        /// </summary>
        public readonly HashSet<string> CategoriasDeuda = new(StringComparer.OrdinalIgnoreCase)
        {
            "Préstamo", "Cargo", "Pago", "Abono"
        };

        /// <summary>
        /// Detalle completo por préstamo/cargo individual: acreedores (debo), deudores
        /// (me deben) y completados. Es el nivel de detalle que necesita Prestamos.razor.
        /// </summary>
        public (List<DeudaPendiente> Acreedores, List<DeudaPendiente> Deudores, List<DeudaPendiente> Completados)
            CalcularDeudasDetalle(DatosApp datos)
        {
            var mapaPersonas = datos.Personas.ToDictionary(p => p.Id, p => p);
            var mapaCuentas = datos.Cuentas.ToDictionary(c => c.Id, c => c);

            var acreedores = new List<DeudaPendiente>();
            var deudores = new List<DeudaPendiente>();
            var completados = new List<DeudaPendiente>();

            var pagosPorReferencia = datos.Movimientos
                .Where(m => m.Tipo == "Egreso" && m.Categoria == "Pago" && !string.IsNullOrEmpty(m.ReferenciaAuto))
                .GroupBy(m => m.ReferenciaAuto!)
                .ToDictionary(g => g.Key, g => new { Total = g.Sum(m => m.Monto), Count = g.Count(), Ultimo = g.Max(m => m.FechaOcurrido) });

            var abonosPorReferencia = datos.Movimientos
                .Where(m => m.Tipo == "Ingreso" && m.Categoria == "Abono" && !string.IsNullOrEmpty(m.ReferenciaAuto))
                .GroupBy(m => m.ReferenciaAuto!)
                .ToDictionary(g => g.Key, g => new { Total = g.Sum(m => m.Monto), Count = g.Count(), Ultimo = g.Max(m => m.FechaOcurrido) });

            var prestamosRecibidos = datos.Movimientos
                .Where(m => m.Tipo == "Ingreso" && m.Categoria == "Préstamo" && m.PersonaId != null && !string.IsNullOrEmpty(m.ReferenciaAuto))
                .ToList();

            foreach (var prestamo in prestamosRecibidos)
            {
                if (!mapaPersonas.TryGetValue(prestamo.PersonaId!, out var persona)) continue;
                if (!mapaCuentas.TryGetValue(prestamo.CuentaId, out var cuenta)) continue;

                var pagoInfo = pagosPorReferencia.GetValueOrDefault(prestamo.ReferenciaAuto!);
                decimal pagado = pagoInfo?.Total ?? 0;
                int pagosRealizados = pagoInfo?.Count ?? 0;
                decimal montoOriginal = prestamo.MontoFinal ?? prestamo.Monto;
                decimal pendiente = montoOriginal - pagado;

                var deuda = new DeudaPendiente
                {
                    Contraparte = persona.Nombre,
                    MontoTotal = montoOriginal,
                    Pagado = pagado,
                    SaldoPendiente = pendiente,
                    ReferenciaAuto = prestamo.ReferenciaAuto!,
                    PersonaId = prestamo.PersonaId!,
                    PagosRealizados = pagosRealizados,
                    PlazosTotales = prestamo.Plazos,
                    ColorCuenta = cuenta.Color,
                    Tipo = "Acreedor",
                    FechaOcurrido = prestamo.FechaOcurrido
                };

                if (pendiente > 0)
                    acreedores.Add(deuda);
                else if (pendiente == 0 && pagado > 0)
                {
                    deuda.FechaCompletado = pagoInfo?.Ultimo;
                    completados.Add(deuda);
                }
            }

            var prestamosOtorgados = datos.Movimientos
                .Where(m => m.Tipo == "Egreso" && m.Categoria == "Cargo" && m.PersonaId != null && !string.IsNullOrEmpty(m.ReferenciaAuto))
                .ToList();

            foreach (var prestamo in prestamosOtorgados)
            {
                if (!mapaPersonas.TryGetValue(prestamo.PersonaId!, out var persona)) continue;
                if (!mapaCuentas.TryGetValue(prestamo.CuentaId, out var cuenta)) continue;

                var abonoInfo = abonosPorReferencia.GetValueOrDefault(prestamo.ReferenciaAuto!);
                decimal abonado = abonoInfo?.Total ?? 0;
                int abonosRealizados = abonoInfo?.Count ?? 0;
                decimal montoOriginal = prestamo.MontoFinal ?? prestamo.Monto;
                decimal pendiente = montoOriginal - abonado;

                var deuda = new DeudaPendiente
                {
                    Contraparte = persona.Nombre,
                    MontoTotal = montoOriginal,
                    Pagado = abonado,
                    SaldoPendiente = pendiente,
                    ReferenciaAuto = prestamo.ReferenciaAuto!,
                    PersonaId = prestamo.PersonaId!,
                    PagosRealizados = abonosRealizados,
                    PlazosTotales = prestamo.Plazos,
                    ColorCuenta = cuenta.Color,
                    Tipo = "Deudor",
                    FechaOcurrido = prestamo.FechaOcurrido
                };

                if (pendiente > 0)
                    deudores.Add(deuda);
                else if (pendiente == 0 && abonado > 0)
                {
                    deuda.FechaCompletado = abonoInfo?.Ultimo;
                    completados.Add(deuda);
                }
            }

            completados = completados.OrderByDescending(c => c.FechaCompletado).ToList();

            return (acreedores, deudores, completados);
        }

        /// <summary>
        /// Igual que CalcularDeudasDetalle, pero agregado por persona: si alguien
        /// tiene dos préstamos activos, aparece como una sola fila con la suma.
        /// Es el nivel de detalle que necesitan Resumen.razor y Balance.razor.
        /// Se deriva directamente del detalle — nunca reimplementa el cálculo.
        /// </summary>
        public (List<ResumenDeuda> Acreedores, List<ResumenDeuda> Deudores) CalcularDeudasResumen(DatosApp datos)
        {
            var (acreedoresDetalle, deudoresDetalle, _) = CalcularDeudasDetalle(datos);

            var acreedoresResumen = acreedoresDetalle
                .GroupBy(d => d.Contraparte)
                .Select(g => new ResumenDeuda { Contraparte = g.Key, SaldoPendiente = g.Sum(d => d.SaldoPendiente) })
                .ToList();

            var deudoresResumen = deudoresDetalle
                .GroupBy(d => d.Contraparte)
                .Select(g => new ResumenDeuda { Contraparte = g.Key, SaldoPendiente = g.Sum(d => d.SaldoPendiente) })
                .ToList();

            return (acreedoresResumen, deudoresResumen);
        }
    }
}