using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Yes_Gestor.Models;

namespace Yes_Gestor
{
    public partial class VentanaBalance : Window
    {
        public VentanaBalance()
        {
            InitializeComponent();
            CargarDatosReales();
        }

        private void CargarDatosReales()
        {
            try
            {
                var datos = App.Datos;
                if (datos == null)
                {
                    MessageBox.Show("App.Datos es null. Revise que App.xaml.cs esté inicializando los datos correctamente.", "Error de datos", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 1. Calcular saldos de cuentas
                var saldos = CalcularSaldosCuentas(datos);
                var cuentasCorrientes = saldos.Where(c => c.Visibilidad == "Corriente").ToList();
                var cuentasOcultas = saldos.Where(c => c.Visibilidad == "Oculto").ToList();
                var cuentasAjenas = saldos.Where(c => c.Visibilidad == "Ajeno").ToList();

                decimal totalCorriente = cuentasCorrientes.Sum(c => c.Saldo);
                decimal totalOculto = cuentasOcultas.Sum(c => c.Saldo);
                decimal totalAjeno = cuentasAjenas.Sum(c => c.Saldo);

                // 2. Calcular deudas (si las necesita, pero no se mostrarán en la derecha)
                var (porCobrar, porPagar) = CalcularDeudas(datos);
                decimal totalPorCobrar = porCobrar.Sum(d => d.SaldoPendiente);
                decimal totalPorPagar = porPagar.Sum(d => d.SaldoPendiente);

                // 3. Total global = corriente + oculto + ajeno (ignoro deudas, pero puedo mantener fórmula)
                // Si prefiere incluir deudas, use:
                decimal totalGlobal = totalCorriente + totalOculto + totalPorCobrar - totalPorPagar;
                //decimal totalGlobal = totalCorriente + totalOculto + totalAjeno;

                // 4. Gasto transporte del mes actual
                decimal gastoTransporte = CalcularGastoTransporte(datos);

                // 5. Actualizar tarjetas izquierdas
                txtDineroDisponible.Text = totalCorriente.ToString("C");
                txtDineroOculto.Text = totalOculto.ToString("C");
                txtCuentasPorCobrar.Text = totalPorCobrar.ToString("C");
                txtCuentasPorPagar.Text = totalPorPagar.ToString("C");
                txtTotalGlobal.Text = totalGlobal.ToString("C");
                txtTransporte.Text = gastoTransporte.ToString("C");
                txtAjeno.Text = totalAjeno.ToString("C");

                // 6. Actualizar tarjetas derechas (ItemsControl)
                icCuentasCorrientes.ItemsSource = cuentasCorrientes;
                icCuentasOcultas.ItemsSource = cuentasOcultas;
                icCuentasAjenas.ItemsSource = cuentasAjenas;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en CargarDatosReales: {ex.Message}\n\n{ex.StackTrace}", "Excepción", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<CuentaSaldo> CalcularSaldosCuentas(DatosApp datos)
        {
            // Inicializar con todas las cuentas, usando su SaldoInicial
            var dict = datos.Cuentas.ToDictionary(c => c.Id, c => new CuentaSaldo
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Visibilidad = c.Visibilidad,
                Saldo = c.SaldoInicial
            });

            // Sumar/restar los movimientos
            foreach (var mov in datos.Movimientos)
            {
                if (!dict.ContainsKey(mov.CuentaId)) continue;
                if (mov.Tipo == "Ingreso")
                    dict[mov.CuentaId].Saldo += mov.Monto;
                else if (mov.Tipo == "Egreso")
                    dict[mov.CuentaId].Saldo -= mov.Monto;
            }

            return dict.Values.ToList();
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
                        SaldoPendiente = pendiente
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
                        SaldoPendiente = pendiente
                    });
                }
            }

            return (porCobrar, porPagar);
        }

        private decimal CalcularGastoTransporte(DatosApp datos)
        {
            DateTime hoy = DateTime.Today;
            DateTime inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            DateTime finMes = inicioMes.AddMonths(1).AddDays(-1);

            return datos.Movimientos
                .Where(m => m.Tipo == "Egreso" && m.Categoria == "Transporte" &&
                            m.FechaOcurrido >= inicioMes && m.FechaOcurrido <= finMes)
                .Sum(m => m.Monto);
        }

        private void MoverVentana_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void AbrirMovimientos_Click(object sender, RoutedEventArgs e)
        {
            var movimientos = new VentanaMovimientos();
            movimientos.Show();
            this.Close();
        }

        private async void AgregarMovimiento_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VentanaMovimientoDialogo();
            if (dialog.ShowDialog() == true)
            {
                if (dialog.Movimientos != null && dialog.Movimientos.Count > 0)
                {
                    foreach (var mov in dialog.Movimientos)
                        App.Datos.Movimientos.Add(mov);
                    await App.Servicio.GuardarAsync(App.Datos);
                    CargarDatosReales();
                }
            }
        }

        private void AbrirPrestamos_Click(object sender, RoutedEventArgs e)
        {
            var prestamos = new VentanaPrestamos();
            prestamos.Show();
            this.Close();
        }

        private void AbrirDashboard_Click(object sender, RoutedEventArgs e)
        {
            new VentanaDashboard().Show();
            this.Close();
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

    // Clases auxiliares (mantenerlas como estaban)
    public class CuentaSaldo
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Visibilidad { get; set; }
        public decimal Saldo { get; set; }
    }

}