using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Yes_Gestor.Models;

namespace Yes_Gestor
{
    public partial class VentanaMovimientos : Window
    {
        private DatosApp datos;
        private List<MovimientoViewModel> movimientosView;

        public VentanaMovimientos()
        {
            InitializeComponent();
            CargarDatos();
            CargarComboBoxFiltros();
            AplicarFiltros();
        }

        private void CargarDatos()
        {
            datos = App.Datos;
            if (datos == null) return;
        }

        private void CargarComboBoxFiltros()
        {
            cbCuentaFiltro.ItemsSource = datos.Cuentas;
            cbCategoriaFiltro.ItemsSource = datos.Categorias;
        }

        private void AplicarFiltros()
        {
            DateTime fechaDesde = dpFechaDesde.SelectedDate ?? DateTime.MinValue;
            DateTime fechaHasta = dpFechaHasta.SelectedDate ?? DateTime.MaxValue;

            string cuentaId = cbCuentaFiltro.SelectedValue as string;
            string categoriaId = cbCategoriaFiltro.SelectedValue as string;

            // Filtrar movimientos: eliminar nulos, aplicar fecha, cuenta y categoría
            var listaFiltrada = datos.Movimientos
                .Where(m => m != null)
                .Where(m => m.FechaOcurrido >= fechaDesde && m.FechaOcurrido <= fechaHasta);

            if (!string.IsNullOrEmpty(cuentaId))
                listaFiltrada = listaFiltrada.Where(m => m.CuentaId == cuentaId);
            if (!string.IsNullOrEmpty(categoriaId))
                listaFiltrada = listaFiltrada.Where(m => m.CategoriaId == categoriaId);

            // Convertir a ViewModel para mostrar nombres en lugar de IDs
            movimientosView = listaFiltrada.Select(m => new MovimientoViewModel
            {
                Id = m.Id,
                FechaOcurrido = m.FechaOcurrido,
                Tipo = m.Tipo,
                Categoria = m.Categoria,
                Descripcion = m.Descripcion,
                Monto = m.Monto,
                CuentaId = m.CuentaId,
                CategoriaId = m.CategoriaId,
                PersonaId = m.PersonaId,
                NombreCuenta = ObtenerNombreCuenta(m.CuentaId),
                NombrePersona = ObtenerNombrePersona(m.PersonaId)
            }).ToList();

            dgMovimientos.ItemsSource = movimientosView;
        }

        private string ObtenerNombreCuenta(string cuentaId)
        {
            var cuenta = datos.Cuentas.FirstOrDefault(c => c.Id == cuentaId);
            return cuenta?.Nombre ?? "Desconocida";
        }

        private string ObtenerNombrePersona(string personaId)
        {
            if (string.IsNullOrEmpty(personaId)) return "";
            var persona = datos.Personas.FirstOrDefault(p => p.Id == personaId);
            return persona?.Nombre ?? "";
        }

        private void Filtrar_Click(object sender, RoutedEventArgs e) => AplicarFiltros();
        private void LimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            dpFechaDesde.SelectedDate = null;
            dpFechaHasta.SelectedDate = null;
            cbCuentaFiltro.SelectedItem = null;
            cbCategoriaFiltro.SelectedItem = null;
            AplicarFiltros();
        }

        private async void AgregarMovimiento_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;
            btn.IsEnabled = false;
            try
            {
                var dialog = new VentanaMovimientoDialogo();
                if (dialog.ShowDialog() == true)
                {
                    if (dialog.Movimientos != null && dialog.Movimientos.Count > 0)
                    {
                        foreach (var mov in dialog.Movimientos)
                            datos.Movimientos.Add(mov);
                        await App.Servicio.GuardarAsync(datos);
                        AplicarFiltros();
                    }
                }
            }
            finally
            {
                btn.IsEnabled = true;
            }
        }



        // Clase auxiliar para el DataGrid
        public class MovimientoViewModel
        {
            public string Id { get; set; }
            public DateTime FechaOcurrido { get; set; }
            public string Tipo { get; set; }
            public string Categoria { get; set; }
            public string Descripcion { get; set; }
            public decimal Monto { get; set; }
            public string CuentaId { get; set; }
            public string CategoriaId { get; set; }
            public string PersonaId { get; set; }
            public string NombreCuenta { get; set; }
            public string NombrePersona { get; set; }
        }

        private void MoverVentana_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void AbrirBalance_Click(object sender, RoutedEventArgs e)
        {
            var balance = new VentanaBalance();
            balance.Show();
            this.Close();
        }
    }
}