using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Yes_Gestor.Models;

namespace Yes_Gestor
{
    public partial class VentanaRegistroPagoDialogo : Window
    {
        public Movimiento PagoMovimiento { get; private set; }
        private readonly DeudaPendiente _deuda;
        private readonly bool _esPagoPropio;

        public VentanaRegistroPagoDialogo(DeudaPendiente deuda, bool esPagoPropio = false)
        {
            InitializeComponent();
            _deuda = deuda;
            _esPagoPropio = esPagoPropio;
            dpFecha.SelectedDate = DateTime.Today;

            cbCuenta.ItemsSource = App.Datos.Cuentas.Where(c => c.Visibilidad == "Corriente" || c.Visibilidad == "Oculto").ToList();
            cbCuenta.DisplayMemberPath = "Nombre";
            cbCuenta.SelectedValuePath = "Id";
            if (cbCuenta.Items.Count > 0)
                cbCuenta.SelectedIndex = 0;
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cbCuenta.SelectedItem == null) throw new Exception("Seleccione una cuenta.");
                if (!decimal.TryParse(txtMonto.Text, out decimal monto) || monto <= 0)
                    throw new Exception("Monto inválido.");
                if (monto > _deuda.SaldoPendiente)
                {
                    if (MessageBox.Show($"El monto ({monto:C}) supera el saldo pendiente ({_deuda.SaldoPendiente:C}). ¿Continuar?", "Advertencia", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                        return;
                }

                Cuenta cuenta = cbCuenta.SelectedItem as Cuenta;
                DateTime fecha = dpFecha.SelectedDate ?? DateTime.Today;
                string tipo = _esPagoPropio ? "Egreso" : "Ingreso";
                string categoria = _esPagoPropio ? "Pago" : "Abono";

                var movimiento = new Movimiento(
                    fechaOcurrido: fecha,
                    tipo: tipo,
                    categoria: categoria,
                    cuentaId: cuenta.Id,
                    categoriaId: null,
                    monto: monto,
                    personaId: _deuda.PersonaId,
                    descripcion: $"{categoria} de {_deuda.Contraparte} - {_deuda.ReferenciaAuto}",
                    montoFinal: null,
                    plazos: null,
                    metaId: null
                );
                movimiento.ReferenciaAuto = _deuda.ReferenciaAuto;

                PagoMovimiento = movimiento;
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

        // Método para arrastrar la ventana (necesario porque no tiene borde)
        private void MoverVentana_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}