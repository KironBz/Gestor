using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Yes_Gestor.Models;

namespace Yes_Gestor
{
    public partial class VentanaPrestamos : Window
    {
        public VentanaPrestamos()
        {
            InitializeComponent();
            CargarDeudas();
        }

        private void CargarDeudas()
        {
            var (porCobrar, porPagar) = CalcularDeudas(App.Datos);
            dgDeudores.ItemsSource = porCobrar;
            dgAcreedores.ItemsSource = porPagar;
        }

        private (List<DeudaPendiente> porCobrar, List<DeudaPendiente> porPagar) CalcularDeudas(DatosApp datos)
        {
            var porCobrar = new List<DeudaPendiente>();
            var porPagar = new List<DeudaPendiente>();

            // Préstamos recibidos (debo)
            var prestamosRecibidos = datos.Movimientos
                .Where(m => m.Tipo == "Ingreso" && m.Categoria == "Préstamo" && m.PersonaId != null)
                .ToList();

            foreach (var prestamo in prestamosRecibidos)
            {
                var persona = datos.Personas.FirstOrDefault(p => p.Id == prestamo.PersonaId);
                if (persona == null) continue;

                decimal pagado = datos.Movimientos
                    .Where(m => m.Tipo == "Egreso" && m.Categoria == "Pago" && m.ReferenciaAuto == prestamo.ReferenciaAuto)
                    .Sum(m => m.Monto);

                decimal montoOriginal = prestamo.MontoFinal ?? prestamo.Monto;
                decimal pendiente = montoOriginal - pagado;
                if (pendiente > 0)
                {
                    porPagar.Add(new DeudaPendiente
                    {
                        Contraparte = persona.Nombre,
                        MontoTotal = montoOriginal,
                        Pagado = pagado,
                        SaldoPendiente = pendiente,
                        ReferenciaAuto = prestamo.ReferenciaAuto,
                        PersonaId = prestamo.PersonaId
                    });
                }
            }

            // Préstamos otorgados (me deben)
            var prestamosOtorgados = datos.Movimientos
                .Where(m => m.Tipo == "Egreso" && m.Categoria == "Cargo" && m.PersonaId != null)
                .ToList();

            foreach (var prestamo in prestamosOtorgados)
            {
                var persona = datos.Personas.FirstOrDefault(p => p.Id == prestamo.PersonaId);
                if (persona == null) continue;

                decimal pagado = datos.Movimientos
                    .Where(m => m.Tipo == "Ingreso" && m.Categoria == "Abono" && m.ReferenciaAuto == prestamo.ReferenciaAuto)
                    .Sum(m => m.Monto);

                decimal montoOriginal = prestamo.MontoFinal ?? prestamo.Monto;
                decimal pendiente = montoOriginal - pagado;
                if (pendiente > 0)
                {
                    porCobrar.Add(new DeudaPendiente
                    {
                        Contraparte = persona.Nombre,
                        MontoTotal = montoOriginal,
                        Pagado = pagado,
                        SaldoPendiente = pendiente,
                        ReferenciaAuto = prestamo.ReferenciaAuto,
                        PersonaId = prestamo.PersonaId
                    });
                }
            }

            return (porCobrar, porPagar);
        }

        private async void RegistrarPagoDeudor_Click(object sender, RoutedEventArgs e)
        {
            var deuda = dgDeudores.SelectedItem as DeudaPendiente;
            if (deuda == null)
            {
                MessageBox.Show("Seleccione una deuda de la lista.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Registrar pago para deudor (me debe) -> movimiento de tipo Ingreso con categoría Abono
            var dialog = new VentanaRegistroPagoDialogo(deuda);
            if (dialog.ShowDialog() == true)
            {
                var pago = dialog.PagoMovimiento;
                if (pago != null)
                {
                    App.Datos.Movimientos.Add(pago);
                    await App.Servicio.GuardarAsync(App.Datos);
                    CargarDeudas(); // refrescar
                }
            }
        }

        private async void RegistrarPagoAcreedor_Click(object sender, RoutedEventArgs e)
        {
            var deuda = dgAcreedores.SelectedItem as DeudaPendiente;
            if (deuda == null)
            {
                MessageBox.Show("Seleccione una deuda de la lista.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Registrar pago para acreedor (debo) -> movimiento de tipo Egreso con categoría Pago
            var dialog = new VentanaRegistroPagoDialogo(deuda, esPagoPropio: true);
            if (dialog.ShowDialog() == true)
            {
                var pago = dialog.PagoMovimiento;
                if (pago != null)
                {
                    App.Datos.Movimientos.Add(pago);
                    await App.Servicio.GuardarAsync(App.Datos);
                    CargarDeudas(); // refrescar
                }
            }
        }

        private void MoverVentana_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}