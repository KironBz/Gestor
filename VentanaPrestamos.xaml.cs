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
            var (porCobrar, porPagar, completados) = CalcularTodasDeudas(App.Datos);
            dgDeudores.ItemsSource = porCobrar;
            dgAcreedores.ItemsSource = porPagar;
            dgCompletados.ItemsSource = completados;
        }

        private (List<DeudaPendiente> porCobrar, List<DeudaPendiente> porPagar, List<DeudaPendiente> completados) CalcularTodasDeudas(DatosApp datos)
        {
            var porCobrar = new List<DeudaPendiente>();
            var porPagar = new List<DeudaPendiente>();
            var completados = new List<DeudaPendiente>();

            // ===== PRÉSTAMOS RECIBIDOS (debo) – Ingreso + Préstamo con referencia no nula =====
            var prestamosRecibidos = datos.Movimientos
                .Where(m => m.Tipo == "Ingreso" && m.Categoria == "Préstamo" && m.PersonaId != null && !string.IsNullOrEmpty(m.ReferenciaAuto))
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

                var deuda = new DeudaPendiente
                {
                    Contraparte = persona.Nombre,
                    MontoTotal = montoOriginal,
                    Pagado = pagado,
                    SaldoPendiente = pendiente,
                    ReferenciaAuto = prestamo.ReferenciaAuto,
                    PersonaId = prestamo.PersonaId,
                    PagosRealizados = pagosRealizados,
                    PlazosTotales = prestamo.Plazos,
                    CuotaMensual = prestamo.Plazos.HasValue && prestamo.Plazos > 0 ? montoOriginal / prestamo.Plazos.Value : (decimal?)null,
                    Tipo = "Acreedor (debo)"
                };

                if (pendiente > 0)
                    porPagar.Add(deuda);
                else if (pendiente == 0 && pagado > 0)
                    completados.Add(deuda);
            }

            // ===== PRÉSTAMOS OTORGADOS (me deben) – Egreso + Cargo con referencia no nula =====
            var prestamosOtorgados = datos.Movimientos
                .Where(m => m.Tipo == "Egreso" && m.Categoria == "Cargo" && m.PersonaId != null && !string.IsNullOrEmpty(m.ReferenciaAuto))
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

                var deuda = new DeudaPendiente
                {
                    Contraparte = persona.Nombre,
                    MontoTotal = montoOriginal,
                    Pagado = abonado,
                    SaldoPendiente = pendiente,
                    ReferenciaAuto = prestamo.ReferenciaAuto,
                    PersonaId = prestamo.PersonaId,
                    PagosRealizados = abonosRealizados,
                    PlazosTotales = prestamo.Plazos,
                    CuotaMensual = prestamo.Plazos.HasValue && prestamo.Plazos > 0 ? montoOriginal / prestamo.Plazos.Value : (decimal?)null,
                    Tipo = "Deudor (me debe)"
                };

                if (pendiente > 0)
                    porCobrar.Add(deuda);
                else if (pendiente == 0 && abonado > 0)
                    completados.Add(deuda);
            }

            return (porCobrar, porPagar, completados);
        }

        // ================== REGISTRAR PAGOS ==================
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
        private void AbrirBalance_Click(object sender, RoutedEventArgs e) { new VentanaBalance().Show(); this.Close(); }
        private void AbrirMovimientos_Click(object sender, RoutedEventArgs e) { new VentanaMovimientos().Show(); this.Close(); }
        private void AbrirDashboard_Click(object sender, RoutedEventArgs e) { new VentanaDashboard().Show(); this.Close(); }
        private void AbrirMetas_Click(object sender, RoutedEventArgs e) { new VentanaMetas().Show(); this.Close(); }
        private void AbrirConfiguracion_Click(object sender, RoutedEventArgs e) { new VentanaConfiguracion().Show(); this.Close(); }

        private async void AgregarMovimiento_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VentanaMovimientoDialogo();
            if (dialog.ShowDialog() == true && dialog.Movimientos != null)
            {
                foreach (var mov in dialog.Movimientos)
                    App.Datos.Movimientos.Add(mov);
                await App.Servicio.GuardarAsync(App.Datos);
                CargarDeudas();
            }
        }

        private void MoverVentana_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.DragMove();
    }
}