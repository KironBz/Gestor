using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LiveCharts;
using LiveCharts.Wpf;
using Yes_Gestor.Models;

namespace Yes_Gestor
{
    public partial class VentanaDashboard : Window
    {
        public SeriesCollection Series { get; set; }
        public SeriesCollection PieSeries { get; set; }
        public SeriesCollection LineSeries { get; set; }
        public List<string> EtiquetasMeses { get; set; }
        public List<string> EtiquetasGastosMeses { get; set; }
        public Func<double, string> FormatterMoneda { get; set; }

        public VentanaDashboard()
        {
            InitializeComponent();
            FormatterMoneda = value => value.ToString("C");
            CargarGraficos();
            this.DataContext = this;
        }

        private void CargarGraficos()
        {
            var datos = App.Datos;
            if (datos == null) return;

            // Excluir transferencias
            var movimientosReales = datos.Movimientos
                .Where(m => m.Categoria != "Transferencia")
                .ToList();

            // ========== 1. GRÁFICO DE BARRAS (Ingresos vs Egresos por mes) ==========
            // Obtener todos los meses con datos (desde el primer movimiento hasta el último)
            if (!movimientosReales.Any())
            {
                Series = new SeriesCollection();
                PieSeries = new SeriesCollection();
                LineSeries = new SeriesCollection();
                EtiquetasMeses = new List<string>();
                EtiquetasGastosMeses = new List<string>();
                return;
            }

            var primeraFecha = movimientosReales.Min(m => m.FechaOcurrido);
            var ultimaFecha = movimientosReales.Max(m => m.FechaOcurrido);
            var listaMeses = new List<DateTime>();
            var fechaActual = new DateTime(primeraFecha.Year, primeraFecha.Month, 1);
            while (fechaActual <= ultimaFecha)
            {
                listaMeses.Add(fechaActual);
                fechaActual = fechaActual.AddMonths(1);
            }

            EtiquetasMeses = listaMeses.Select(m => m.ToString("MMM yyyy")).ToList();
            List<decimal> ingresosMensuales = new List<decimal>();
            List<decimal> egresosMensuales = new List<decimal>();

            foreach (var mes in listaMeses)
            {
                var inicioMes = mes;
                var finMes = inicioMes.AddMonths(1).AddDays(-1);
                var movimientosMes = movimientosReales
                    .Where(m => m.FechaOcurrido >= inicioMes && m.FechaOcurrido <= finMes);

                ingresosMensuales.Add(movimientosMes.Where(m => m.Tipo == "Ingreso").Sum(m => m.Monto));
                egresosMensuales.Add(movimientosMes.Where(m => m.Tipo == "Egreso").Sum(m => m.Monto));
            }

            var barraIngresos = new ColumnSeries
            {
                Title = "Ingresos",
                Values = new ChartValues<decimal>(ingresosMensuales),
                DataLabels = true,
                LabelPoint = point => point.Y.ToString("C")
            };
            var barraEgresos = new ColumnSeries
            {
                Title = "Egresos",
                Values = new ChartValues<decimal>(egresosMensuales),
                DataLabels = true,
                LabelPoint = point => point.Y.ToString("C")
            };
            Series = new SeriesCollection { barraIngresos, barraEgresos };

            // ========== 2. GRÁFICO DE PASTEL (Ingresos vs Egresos totales) ==========
            decimal totalIngresos = movimientosReales.Where(m => m.Tipo == "Ingreso").Sum(m => m.Monto);
            decimal totalEgresos = movimientosReales.Where(m => m.Tipo == "Egreso").Sum(m => m.Monto);

            var pieIngresos = new PieSeries
            {
                Title = "Ingresos",
                Values = new ChartValues<decimal> { totalIngresos },
                DataLabels = true,
                LabelPoint = point => point.Y.ToString("C")
            };
            var pieEgresos = new PieSeries
            {
                Title = "Egresos",
                Values = new ChartValues<decimal> { totalEgresos },
                DataLabels = true,
                LabelPoint = point => point.Y.ToString("C")
            };
            PieSeries = new SeriesCollection { pieIngresos, pieEgresos };

            // ========== 3. GRÁFICO DE LÍNEAS (Evolución de gastos mensuales) ==========
            List<decimal> gastosMensuales = new List<decimal>();
            foreach (var mes in listaMeses)
            {
                var inicioMes = mes;
                var finMes = inicioMes.AddMonths(1).AddDays(-1);
                var egresosMes = movimientosReales
                    .Where(m => m.Tipo == "Egreso" && m.FechaOcurrido >= inicioMes && m.FechaOcurrido <= finMes)
                    .Sum(m => m.Monto);
                gastosMensuales.Add(egresosMes);
            }

            var lineSeries = new LineSeries
            {
                Title = "Gastos mensuales",
                Values = new ChartValues<decimal>(gastosMensuales),
                DataLabels = true,
                LabelPoint = point => point.Y.ToString("C"),
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 10
            };
            LineSeries = new SeriesCollection { lineSeries };
            EtiquetasGastosMeses = listaMeses.Select(m => m.ToString("MMM yyyy")).ToList();
        }

        // ================== NAVEGACIÓN ==================
        private void AbrirBalance_Click(object sender, RoutedEventArgs e)
        {
            new VentanaBalance().Show();
            this.Close();
        }

        private void AbrirMovimientos_Click(object sender, RoutedEventArgs e)
        {
            new VentanaMovimientos().Show();
            this.Close();
        }

        private void AbrirPrestamos_Click(object sender, RoutedEventArgs e)
        {
            new VentanaPrestamos().Show();
            this.Close();
        }

        private void MoverVentana_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}