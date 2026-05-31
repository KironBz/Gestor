using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Yes_Gestor.Models;

namespace Yes_Gestor
{
    public partial class VentanaMovimientoDialogo : Window
    {
        public Movimiento Movimiento { get; private set; }

        public VentanaMovimientoDialogo()
        {
            InitializeComponent();
            CargarCombos();
            cbTipo.SelectionChanged += CbTipo_SelectionChanged;
        }

        private void CargarCombos()
        {
            cbCuenta.ItemsSource = App.Datos.Cuentas;
            cbCategoria.ItemsSource = App.Datos.Categorias;
            var personas = App.Datos.Personas.ToList();
            personas.Insert(0, new Persona { Id = "", Nombre = "(Ninguna)", Tipo = "Ninguno" });
            cbPersona.ItemsSource = personas;
        }

        private void CbTipo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string tipo = (cbTipo.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Tag as string;
            // Mostrar/ocultar campos de préstamo solo cuando tipo = Ingreso y categoría = Préstamo
            // Por simplicidad, aún no lo conectamos con la categoría. Más adelante lo perfeccionamos.
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validaciones básicas
                if (cbTipo.SelectedItem == null) throw new Exception("Seleccione un tipo.");
                if (cbCuenta.SelectedItem == null) throw new Exception("Seleccione una cuenta.");
                if (cbCategoria.SelectedItem == null) throw new Exception("Seleccione una categoría.");
                if (!decimal.TryParse(txtMonto.Text, out decimal monto) || monto <= 0)
                    throw new Exception("Monto inválido.");

                string tipo = (cbTipo.SelectedItem as System.Windows.Controls.ComboBoxItem).Tag as string;
                var cuenta = cbCuenta.SelectedItem as Cuenta;
                var categoria = cbCategoria.SelectedItem as Categoria;
                string personaId = (cbPersona.SelectedItem as Persona)?.Id;
                if (personaId == "") personaId = null;

                var movimiento = new Movimiento(
                    fechaOcurrido: dpFecha.SelectedDate ?? DateTime.Today,
                    tipo: tipo,
                    categoria: categoria.Nombre,
                    cuentaId: cuenta.Id,
                    categoriaId: categoria.Id,
                    monto: monto,
                    personaId: personaId,
                    descripcion: txtDescripcion.Text,
                    montoFinal: null,
                    plazos: null
                );

                // Si es préstamo (Ingreso + categoría Préstamo) o cargo (Egreso + categoría Cargo),
                // leer los campos adicionales.
                if (tipo == "Ingreso" && categoria.Nombre == "Préstamo")
                {
                    if (!decimal.TryParse(txtMontoFinal.Text, out decimal montoFinal) || montoFinal <= 0)
                        throw new Exception("Monto final inválido.");
                    if (!int.TryParse(txtPlazos.Text, out int plazos) || plazos <= 0)
                        throw new Exception("Plazos inválidos.");
                    movimiento.MontoFinal = montoFinal;
                    movimiento.Plazos = plazos;
                    movimiento.GenerarReferenciaAuto(); // necesario agregar este método público o usar el constructor
                }

                Movimiento = movimiento;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void MoverVentana_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}