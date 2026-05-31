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
            // Obtener fecha inicio (si no, tomar mínimo histórico)
            DateTime fechaDesde = dpFechaDesde.SelectedDate ?? DateTime.MinValue;
            DateTime fechaHasta = dpFechaHasta.SelectedDate ?? DateTime.MaxValue;

            string cuentaId = cbCuentaFiltro.SelectedValue as string;
            string categoriaId = cbCategoriaFiltro.SelectedValue as string;

            var lista = from m in datos.Movimientos
                        where m.FechaOcurrido >= fechaDesde && m.FechaOcurrido <= fechaHasta
                        select m;

            if (!string.IsNullOrEmpty(cuentaId))
                lista = lista.Where(m => m.CuentaId == cuentaId);
            if (!string.IsNullOrEmpty(categoriaId))
                lista = lista.Where(m => m.CategoriaId == categoriaId);

            // Convertir a ViewModel para mostrar nombres en lugar de IDs
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

        private async void AgregarMovimiento_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VentanaMovimientoDialogo();
            if (dialog.ShowDialog() == true)
            {
                var nuevoMov = dialog.Movimiento;
                // El constructor ya asigna Id y FechaRegistro. Solo lo agregamos.
                datos.Movimientos.Add(nuevoMov);
                await App.Servicio.GuardarAsync(datos);
                AplicarFiltros();
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
    }
}