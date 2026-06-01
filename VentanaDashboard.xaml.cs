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

        public VentanaDashboard()
        {
            InitializeComponent();
            CargarGraficos();
            this.DataContext = this;
        }

        private void CargarGraficos()
        {
            var datos = App.Datos;
            if (datos == null) return;

            // ===== GRÁFICO DE BARRAS: Ingresos vs Egresos por mes (últimos 12 meses) =====
            var hoy = DateTime.Today;
            var meses = Enumerable.Range(0, 12)
                .Select(i => hoy.AddMonths(-i))
                .OrderBy(m => m)
                .ToList();

            List<string> etiquetas = new List<string>();
            List<decimal> ingresos = new List<decimal>();
            List<decimal> egresos = new List<decimal>();

            foreach (var mes in meses)
            {
                etiquetas.Add(mes.ToString("MMM yyyy"));
                var inicioMes = new DateTime(mes.Year, mes.Month, 1);
                var finMes = inicioMes.AddMonths(1).AddDays(-1);

                var movimientosMes = datos.Movimientos
                    .Where(m => m.FechaOcurrido >= inicioMes && m.FechaOcurrido <= finMes);

                ingresos.Add(movimientosMes.Where(m => m.Tipo == "Ingreso").Sum(m => m.Monto));
                egresos.Add(movimientosMes.Where(m => m.Tipo == "Egreso").Sum(m => m.Monto));
            }

            Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Ingresos",
                    Values = new ChartValues<decimal>(ingresos),
                    DataLabels = true
                },
                new ColumnSeries
                {
                    Title = "Egresos",
                    Values = new ChartValues<decimal>(egresos),
                    DataLabels = true
                }
            };

            // ===== GRÁFICO DE PASTEL: Ingresos vs Egresos totales =====
            decimal totalIngresos = datos.Movimientos.Where(m => m.Tipo == "Ingreso").Sum(m => m.Monto);
            decimal totalEgresos = datos.Movimientos.Where(m => m.Tipo == "Egreso").Sum(m => m.Monto);

            PieSeries = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Ingresos",
                    Values = new ChartValues<decimal> { totalIngresos },
                    DataLabels = true
                },
                new PieSeries
                {
                    Title = "Egresos",
                    Values = new ChartValues<decimal> { totalEgresos },
                    DataLabels = true
                }
            };
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