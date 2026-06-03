using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LiveCharts;
using LiveCharts.Wpf;
using Yes_Gestor.Models;

namespace Yes_Gestor
{
    public partial class VentanaDashboard : Window
    {
        // Propiedades de los gráficos
        public SeriesCollection AbsSeries { get; set; }
        public SeriesCollection PieSeries { get; set; }
        public SeriesCollection LineSeries { get; set; }
        public List<string> EtiquetasGastosMeses { get; set; }
        public List<string> EtiquetasAbs { get; set; }
        public Func<double, string> FormatterMoneda { get; set; }
        public SeriesCollection IngresosPorCategoriaSeries { get; set; }
        public SeriesCollection EgresosPorCategoriaSeries { get; set; }

        private List<Movimiento> _todosMovimientosReales;
        private List<Movimiento> _movimientosFiltrados;

        public VentanaDashboard()
        {
            InitializeComponent();
            FormatterMoneda = value => value.ToString("C");
            CargarDatosBase();
            CargarAniosDisponibles();
            ActualizarFiltro();  // esto llama a ActualizarGraficosSuperiores y refresca
            this.DataContext = this;
        }

        private void CargarDatosBase()
        {
            var datos = App.Datos;
            if (datos == null) return;

            _todosMovimientosReales = datos.Movimientos
                .Where(m => m.Categoria != "Transferencia")
                .ToList();

            // Gráfico de líneas (evolución de gastos mensuales - sin filtrar)
            if (_todosMovimientosReales.Any())
            {
                var primeraFecha = _todosMovimientosReales.Min(m => m.FechaOcurrido);
                var ultimaFecha = _todosMovimientosReales.Max(m => m.FechaOcurrido);
                var listaMeses = new List<DateTime>();
                var fechaActual = new DateTime(primeraFecha.Year, primeraFecha.Month, 1);
                while (fechaActual <= ultimaFecha)
                {
                    listaMeses.Add(fechaActual);
                    fechaActual = fechaActual.AddMonths(1);
                }

                List<decimal> gastosMensuales = new List<decimal>();
                foreach (var mes in listaMeses)
                {
                    var inicioMes = mes;
                    var finMes = inicioMes.AddMonths(1).AddDays(-1);
                    var egresosMes = _todosMovimientosReales
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
                    PointGeometrySize = 10,
                    LineSmoothness = 0
                };
                LineSeries = new SeriesCollection { lineSeries };
                EtiquetasGastosMeses = listaMeses.Select(m => m.ToString("MMM yyyy")).ToList();
            }
            else
            {
                LineSeries = new SeriesCollection();
                EtiquetasGastosMeses = new List<string>();
            }
        }

        private void CargarAniosDisponibles()
        {
            var anios = _todosMovimientosReales
                .Select(m => m.FechaOcurrido.Year)
                .Distinct()
                .OrderBy(a => a)
                .ToList();

            cbAnio.Items.Clear();
            cbAnio.Items.Add(new ComboBoxItem { Tag = "todos", Content = "Todos" });
            foreach (var anio in anios)
            {
                cbAnio.Items.Add(new ComboBoxItem { Tag = anio.ToString(), Content = anio.ToString() });
            }
            cbAnio.SelectedIndex = 0; // "Todos"
        }

        private void ActualizarFiltro()
        {
            if (_todosMovimientosReales == null) return;

            // Obtener año seleccionado
            string anioSeleccionado = "todos";
            if (cbAnio.SelectedItem is ComboBoxItem itemAnio && itemAnio.Tag.ToString() != "todos")
                anioSeleccionado = itemAnio.Tag.ToString();

            // Obtener período seleccionado
            string periodo = "total";
            if (cbPeriodo.SelectedItem is ComboBoxItem itemPeriodo)
                periodo = itemPeriodo.Tag.ToString();

            // Aplicar filtros
            var query = _todosMovimientosReales.AsEnumerable();

            if (anioSeleccionado != "todos")
            {
                int anio = int.Parse(anioSeleccionado);
                query = query.Where(m => m.FechaOcurrido.Year == anio);
            }

            DateTime hoy = DateTime.Today;
            // Interpretar periodo
            if (periodo == "semestre1")
            {
                query = query.Where(m => m.FechaOcurrido.Month <= 6);
            }
            else if (periodo == "semestre2")
            {
                query = query.Where(m => m.FechaOcurrido.Month >= 7);
            }
            else if (periodo.StartsWith("mes"))
            {
                int mes = int.Parse(periodo.Substring(3));
                // Si año seleccionado es "todos", entonces filtramos por mes pero de todos los años.
                // Si hay año específico, ya lo tenemos.
                query = query.Where(m => m.FechaOcurrido.Month == mes);
                // Nota: si el año es "todos", se mostrarán los datos de ese mes en todos los años. 
                // Eso puede ser extraño, pero así lo pediste. Alternativamente podríamos forzar año actual, pero lo dejo así.
            }

            _movimientosFiltrados = query.ToList();
            ActualizarGraficosSuperiores();
        }

        private void ActualizarGraficosSuperiores()
        {
            if (!_movimientosFiltrados.Any())
            {
                AbsSeries = new SeriesCollection();
                EtiquetasAbs = new List<string> { "Ingresos", "Egresos" };
                PieSeries = new SeriesCollection();
                IngresosPorCategoriaSeries = new SeriesCollection();
                EgresosPorCategoriaSeries = new SeriesCollection();
                // Forzar actualización visual
                ForzarActualizacion();
                return;
            }

            decimal totalIngresos = _movimientosFiltrados.Where(m => m.Tipo == "Ingreso").Sum(m => m.Monto);
            decimal totalEgresos = _movimientosFiltrados.Where(m => m.Tipo == "Egreso").Sum(m => m.Monto);

            // Barras absolutas
            // Crear dos series separadas: una para ingresos, otra para egresos
            var ingresosSeries = new ColumnSeries
            {
                Title = "Ingresos",
                Values = new ChartValues<decimal> { totalIngresos },
                DataLabels = true,
                LabelPoint = point => point.Y.ToString("C"),
                Fill = System.Windows.Media.Brushes.Green
            };

            var egresosSeries = new ColumnSeries
            {
                Title = "Egresos",
                Values = new ChartValues<decimal> { totalEgresos },
                DataLabels = true,
                LabelPoint = point => point.Y.ToString("C"),
                Fill = System.Windows.Media.Brushes.DarkRed
            };

            AbsSeries = new SeriesCollection { ingresosSeries, egresosSeries };

            // Pastel de ingresos vs egresos
            PieSeries = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Ingresos",
                    Values = new ChartValues<decimal> { totalIngresos },
                    DataLabels = true,
                    LabelPoint = point => point.Y.ToString("C"),
                    Fill = System.Windows.Media.Brushes.Green   // ← Color verde
                },
                new PieSeries
                {
                    Title = "Egresos",
                    Values = new ChartValues<decimal> { totalEgresos },
                    DataLabels = true,
                    LabelPoint = point => point.Y.ToString("C"),
                    Fill = System.Windows.Media.Brushes.DarkRed     // ← Color rojo
                }
            };

            // Ingresos por categoría
            var ingresosPorCategoria = _movimientosFiltrados
                .Where(m => m.Tipo == "Ingreso")
                .GroupBy(m => m.Categoria)
                .Select(g => new { Categoria = g.Key, Total = g.Sum(m => m.Monto) })
                .OrderByDescending(x => x.Total)
                .ToList();
            var ingresosPie = new SeriesCollection();
            foreach (var item in ingresosPorCategoria)
            {
                ingresosPie.Add(new PieSeries
                {
                    Title = item.Categoria,
                    Values = new ChartValues<decimal> { item.Total },
                    DataLabels = true,
                    LabelPoint = point => $"{item.Categoria}: {point.Y:C} ({point.Participation:P})"
                });
            }
            IngresosPorCategoriaSeries = ingresosPie;

            // Egresos por categoría
            var egresosPorCategoria = _movimientosFiltrados
                .Where(m => m.Tipo == "Egreso")
                .GroupBy(m => m.Categoria)
                .Select(g => new { Categoria = g.Key, Total = g.Sum(m => m.Monto) })
                .OrderByDescending(x => x.Total)
                .ToList();
            var egresosPie = new SeriesCollection();
            foreach (var item in egresosPorCategoria)
            {
                egresosPie.Add(new PieSeries
                {
                    Title = item.Categoria,
                    Values = new ChartValues<decimal> { item.Total },
                    DataLabels = true,
                    LabelPoint = point => $"{item.Categoria}: {point.Y:C} ({point.Participation:P})"
                });
            }
            EgresosPorCategoriaSeries = egresosPie;

            // Forzar actualización visual
            ForzarActualizacion();
        }

        private void ForzarActualizacion()
        {
            // Pequeño truco para forzar la actualización de los binding (ya que no usamos INotifyPropertyChanged)
            this.DataContext = null;
            this.DataContext = this;
        }

        private void Filtro_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarFiltro();
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

        private void AbrirMetas_Click(object sender, RoutedEventArgs e)
        {
            new VentanaMetas().Show();
            this.Close();
        }

        private void AbrirConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            new VentanaConfiguracion().Show();
            this.Close();
        }
    }
}