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

            var lista = datos.Movimientos
                .Where(m => m != null)
                .Where(m => m.FechaOcurrido >= fechaDesde && m.FechaOcurrido <= fechaHasta);

            if (!string.IsNullOrEmpty(cuentaId))
                lista = lista.Where(m => m.CuentaId == cuentaId);
            if (!string.IsNullOrEmpty(categoriaId))
                lista = lista.Where(m => m.CategoriaId == categoriaId);

            movimientosView = lista.Select(m => new MovimientoViewModel
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

        // ================== AGREGAR ==================
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

        // ================== EDITAR ==================
        private MovimientoViewModel ObtenerMovimientoSeleccionado()
        {
            return dgMovimientos.SelectedItem as MovimientoViewModel;
        }

        private async void EditarMovimiento_Click(object sender, RoutedEventArgs e)
        {
            var vm = ObtenerMovimientoSeleccionado();
            if (vm == null)
            {
                MessageBox.Show("Seleccione un movimiento para editar.", "Editar", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var movimientoOriginal = datos.Movimientos.FirstOrDefault(m => m.Id == vm.Id);
            if (movimientoOriginal == null) return;

            // No soportamos edición de transferencias (por simplicidad)
            if (movimientoOriginal.Tipo == "Transferencia")
            {
                MessageBox.Show("La edición de transferencias no está soportada. Puede eliminarla y crearla de nuevo.", "No soportado", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new VentanaMovimientoDialogo(movimientoOriginal);
            if (dialog.ShowDialog() == true && dialog.Movimientos != null && dialog.Movimientos.Count > 0)
            {
                var movimientoEditado = dialog.Movimientos[0];
                var index = datos.Movimientos.IndexOf(movimientoOriginal);
                if (index >= 0)
                    datos.Movimientos[index] = movimientoEditado;
                await App.Servicio.GuardarAsync(datos);
                AplicarFiltros();
            }
        }

        private void dgMovimientos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditarMovimiento_Click(sender, null);
        }

        // ================== ELIMINAR ==================
        private async void EliminarMovimiento_Click(object sender, RoutedEventArgs e)
        {
            var vm = ObtenerMovimientoSeleccionado();
            if (vm == null)
            {
                MessageBox.Show("Seleccione un movimiento para eliminar.", "Eliminar", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var movimiento = datos.Movimientos.FirstOrDefault(m => m.Id == vm.Id);
            if (movimiento == null) return;

            var result = MessageBox.Show($"¿Eliminar el movimiento del {movimiento.FechaOcurrido:dd/MM/yyyy} - {movimiento.Descripcion} por {movimiento.Monto:C}?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                datos.Movimientos.Remove(movimiento);
                await App.Servicio.GuardarAsync(datos);
                AplicarFiltros();
            }
        }

        // ================== NAVEGACIÓN ==================
        private void AbrirBalance_Click(object sender, RoutedEventArgs e) { new VentanaBalance().Show(); this.Close(); }
        private void AbrirPrestamos_Click(object sender, RoutedEventArgs e) { new VentanaPrestamos().Show(); this.Close(); }
        private void AbrirDashboard_Click(object sender, RoutedEventArgs e) { new VentanaDashboard().Show(); this.Close(); }
        private void AbrirMetas_Click(object sender, RoutedEventArgs e) { new VentanaMetas().Show(); this.Close(); }
        private void MoverVentana_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.DragMove();

        private void AbrirConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            new VentanaConfiguracion().Show();
            this.Close();
        }

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
    }
}