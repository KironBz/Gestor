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
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(txtMonto.Text, out decimal monto) || monto <= 0)
            {
                MessageBox.Show("Monto inválido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (monto > _deuda.SaldoPendiente)
            {
                var result = MessageBox.Show($"El monto ingresado ({monto:C}) es mayor al saldo pendiente ({_deuda.SaldoPendiente:C}). ¿Registrarlo igual?", "Advertencia", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes)
                    return;
            }

            DateTime fecha = dpFecha.SelectedDate ?? DateTime.Today;
            string tipo = _esPagoPropio ? "Egreso" : "Ingreso";
            string categoria = _esPagoPropio ? "Pago" : "Abono";
            string personaId = _deuda.PersonaId;

            var movimiento = new Movimiento(
                fechaOcurrido: fecha,
                tipo: tipo,
                categoria: categoria,
                cuentaId: ObtenerCuentaPorDefecto(),
                categoriaId: null,
                monto: monto,
                personaId: personaId,
                descripcion: $"Pago de {_deuda.Contraparte} - {_deuda.ReferenciaAuto}",
                montoFinal: null,
                plazos: null
            );
            movimiento.ReferenciaAuto = _deuda.ReferenciaAuto;  // para vincular

            PagoMovimiento = movimiento;
            DialogResult = true;
            Close();
        }

        private string ObtenerCuentaPorDefecto()
        {
            var cuenta = App.Datos.Cuentas.FirstOrDefault(c => c.Visibilidad == "Corriente");
            return cuenta?.Id ?? App.Datos.Cuentas.FirstOrDefault()?.Id ?? throw new Exception("No hay cuentas disponibles");
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