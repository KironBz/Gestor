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
            var porCobrar = new List<DeudaPendiente>(); // me deben (préstamos otorgados)
            var porPagar = new List<DeudaPendiente>();  // debo (préstamos recibidos)

            // ===== PRÉSTAMOS RECIBIDOS (DEBO) =====
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
                int pagosRealizados = datos.Movimientos
                    .Count(m => m.Tipo == "Egreso" && m.Categoria == "Pago" && m.ReferenciaAuto == prestamo.ReferenciaAuto);

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
                        PersonaId = prestamo.PersonaId,
                        PagosRealizados = pagosRealizados,
                        PlazosTotales = prestamo.Plazos,
                        CuotaMensual = prestamo.Plazos.HasValue && prestamo.Plazos > 0 ? montoOriginal / prestamo.Plazos.Value : (decimal?)null
                    });
                }
            }

            // ===== PRÉSTAMOS OTORGADOS (ME DEBEN) =====
            var prestamosOtorgados = datos.Movimientos
                .Where(m => m.Tipo == "Egreso" && m.Categoria == "Cargo" && m.PersonaId != null)
                .ToList();

            foreach (var prestamo in prestamosOtorgados)
            {
                var persona = datos.Personas.FirstOrDefault(p => p.Id == prestamo.PersonaId);
                if (persona == null) continue;

                decimal abonado = datos.Movimientos
                    .Where(m => m.Tipo == "Ingreso" && m.Categoria == "Abono" && m.ReferenciaAuto == prestamo.ReferenciaAuto)
                    .Sum(m => m.Monto);
                int abonosRealizados = datos.Movimientos
                    .Count(m => m.Tipo == "Ingreso" && m.Categoria == "Abono" && m.ReferenciaAuto == prestamo.ReferenciaAuto);

                decimal montoOriginal = prestamo.MontoFinal ?? prestamo.Monto;
                decimal pendiente = montoOriginal - abonado;
                if (pendiente > 0)
                {
                    porCobrar.Add(new DeudaPendiente
                    {
                        Contraparte = persona.Nombre,
                        MontoTotal = montoOriginal,
                        Pagado = abonado,
                        SaldoPendiente = pendiente,
                        ReferenciaAuto = prestamo.ReferenciaAuto,
                        PersonaId = prestamo.PersonaId,
                        PagosRealizados = abonosRealizados,
                        PlazosTotales = prestamo.Plazos,
                        CuotaMensual = prestamo.Plazos.HasValue && prestamo.Plazos > 0 ? montoOriginal / prestamo.Plazos.Value : (decimal?)null
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

            var dialog = new VentanaRegistroPagoDialogo(deuda);
            if (dialog.ShowDialog() == true)
            {
                var pago = dialog.PagoMovimiento;
                if (pago != null)
                {
                    App.Datos.Movimientos.Add(pago);
                    await App.Servicio.GuardarAsync(App.Datos);
                    CargarDeudas();
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

            var dialog = new VentanaRegistroPagoDialogo(deuda, esPagoPropio: true);
            if (dialog.ShowDialog() == true)
            {
                var pago = dialog.PagoMovimiento;
                if (pago != null)
                {
                    App.Datos.Movimientos.Add(pago);
                    await App.Servicio.GuardarAsync(App.Datos);
                    CargarDeudas();
                }
            }
        }

        // ================== NAVEGACIÓN ==================
        private void AbrirBalance_Click(object sender, RoutedEventArgs e)
        {
            var balance = new VentanaBalance();
            balance.Show();
            this.Close();
        }

        private void AbrirMovimientos_Click(object sender, RoutedEventArgs e)
        {
            var movimientos = new VentanaMovimientos();
            movimientos.Show();
            this.Close();
        }

        private void MoverVentana_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void AgregarMovimiento_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VentanaMovimientoDialogo();
            if (dialog.ShowDialog() == true && dialog.Movimientos != null)
            {
                foreach (var mov in dialog.Movimientos)
                    App.Datos.Movimientos.Add(mov);
                App.Servicio.GuardarAsync(App.Datos);
                CargarDeudas(); // refrescar
            }
        }
    }
}