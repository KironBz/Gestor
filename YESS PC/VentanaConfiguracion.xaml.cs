using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Yes_Gestor.Models;
using Yes_Gestor.Services;

namespace Yes_Gestor
{
    public partial class VentanaConfiguracion : Window
    {
        public VentanaConfiguracion()
        {
            InitializeComponent();
            CargarDatos();
        }

        private void CargarDatos()
        {
            // Mostrar carpeta actual
            txtCarpetaActual.Text = $"Ruta actual: {ArchivoService.CarpetaPorDefecto()}";

            // Calcular saldo actual para cada cuenta (necesitamos el método auxiliar)
            ActualizarListas();
        }

        private void ActualizarListas()
        {
            var cuentas = App.Datos.Cuentas.ToList();
            foreach (var c in cuentas)
            {
                c.SaldoActual = CalcularSaldoCuenta(c.Id);
            }
              dgCuentas.ItemsSource = cuentas;                                                    //  Orden manual
            //  dgCuentas.ItemsSource = cuentas.OrderBy(c => c.Nombre).ToList();                        // Por Nombre
            //  dgCategorias.ItemsSource = App.Datos.Categorias.OrderBy(c => c.Nombre).ToList();    //  Orden manual
            dgCategorias.ItemsSource = App.Datos.Categorias.OrderBy(c => c.Nombre).ToList();        // Por Nombre
            //  dgPersonas.ItemsSource = App.Datos.Personas.ToList();                               //  Orden manual
            dgPersonas.ItemsSource = App.Datos.Personas.OrderBy(p => p.Nombre).ToList();            // Por Nombre
        }

        private decimal CalcularSaldoCuenta(string cuentaId)
        {
            var movimientos = App.Datos.Movimientos.Where(m => m.CuentaId == cuentaId);
            decimal suma = movimientos.Where(m => m.Tipo == "Ingreso").Sum(m => m.Monto)
                         - movimientos.Where(m => m.Tipo == "Egreso").Sum(m => m.Monto);
            // Saldo inicial + suma neta
            var cuenta = App.Datos.Cuentas.FirstOrDefault(c => c.Id == cuentaId);
            return (cuenta?.SaldoInicial ?? 0) + suma;
        }

        private async void GuardarCambios()
        {
            await App.Servicio.GuardarAsync(App.Datos);
            ActualizarListas();
        }

        // ================== CUENTAS ==================
        private void AgregarCuenta_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new DialogoCuenta();
            if (dialog.ShowDialog() == true)
            {
                App.Datos.Cuentas.Add(dialog.Cuenta);
                GuardarCambios();
            }
        }

        private void EditarCuenta_Click(object sender, RoutedEventArgs e)
        {
            var cuenta = dgCuentas.SelectedItem as Cuenta;
            if (cuenta == null)
            {
                MessageBox.Show("Seleccione una cuenta para editar.", "Editar", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var dialog = new DialogoCuenta(cuenta);
            if (dialog.ShowDialog() == true)
            {
                GuardarCambios();
            }
        }

        private void EliminarCuenta_Click(object sender, RoutedEventArgs e)
        {
            var cuenta = dgCuentas.SelectedItem as Cuenta;
            if (cuenta == null) return;
            if (App.Datos.Movimientos.Any(m => m.CuentaId == cuenta.Id))
            {
                MessageBox.Show($"No se puede eliminar la cuenta '{cuenta.Nombre}' porque tiene movimientos asociados.", "Eliminar", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (MessageBox.Show($"¿Eliminar la cuenta '{cuenta.Nombre}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                App.Datos.Cuentas.Remove(cuenta);
                GuardarCambios();
            }
        }

        private void AjustarSaldoCuenta_Click(object sender, RoutedEventArgs e)
        {
            var cuenta = dgCuentas.SelectedItem as Cuenta;
            if (cuenta == null)
            {
                MessageBox.Show("Seleccione una cuenta para ajustar el saldo.", "Ajustar", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            decimal saldoCalculado = CalcularSaldoCuenta(cuenta.Id);
            var dialog = new DialogoAjusteSaldo(cuenta, saldoCalculado);
            if (dialog.ShowDialog() == true && dialog.NuevoSaldoReal.HasValue)
            {
                // Recalcular el nuevo SaldoInicial para que el saldo actual sea el deseado
                decimal sumaMovimientos = saldoCalculado - cuenta.SaldoInicial;
                decimal nuevoSaldoInicial = dialog.NuevoSaldoReal.Value - sumaMovimientos;
                cuenta.SaldoInicial = nuevoSaldoInicial;
                GuardarCambios();
            }
        }

        // ================== CATEGORÍAS ==================
        private void AgregarCategoria_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new DialogoCategoria();
            if (dialog.ShowDialog() == true)
            {
                App.Datos.Categorias.Add(dialog.Categoria);
                GuardarCambios();
            }
        }

        private void EditarCategoria_Click(object sender, RoutedEventArgs e)
        {
            var categoria = dgCategorias.SelectedItem as Categoria;
            if (categoria == null)
            {
                MessageBox.Show("Seleccione una categoría para editar.", "Editar", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var dialog = new DialogoCategoria(categoria);
            if (dialog.ShowDialog() == true)
            {
                GuardarCambios();
            }
        }

        private void EliminarCategoria_Click(object sender, RoutedEventArgs e)
        {
            var categoria = dgCategorias.SelectedItem as Categoria;
            if (categoria == null) return;
            if (App.Datos.Movimientos.Any(m => m.CategoriaId == categoria.Id))
            {
                MessageBox.Show($"No se puede eliminar la categoría '{categoria.Nombre}' porque tiene movimientos asociados.", "Eliminar", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (MessageBox.Show($"¿Eliminar la categoría '{categoria.Nombre}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                App.Datos.Categorias.Remove(categoria);
                GuardarCambios();
            }
        }

        // ================== PERSONAS ==================
        private void AgregarPersona_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new DialogoPersona();
            if (dialog.ShowDialog() == true)
            {
                App.Datos.Personas.Add(dialog.Persona);
                GuardarCambios();
            }
        }

        private void EditarPersona_Click(object sender, RoutedEventArgs e)
        {
            var persona = dgPersonas.SelectedItem as Persona;
            if (persona == null)
            {
                MessageBox.Show("Seleccione una persona para editar.", "Editar", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var dialog = new DialogoPersona(persona);
            if (dialog.ShowDialog() == true)
            {
                GuardarCambios();
            }
        }

        private void EliminarPersona_Click(object sender, RoutedEventArgs e)
        {
            var persona = dgPersonas.SelectedItem as Persona;
            if (persona == null) return;
            if (App.Datos.Movimientos.Any(m => m.PersonaId == persona.Id))
            {
                MessageBox.Show($"No se puede eliminar a '{persona.Nombre}' porque tiene movimientos asociados.", "Eliminar", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (MessageBox.Show($"¿Eliminar a '{persona.Nombre}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                App.Datos.Personas.Remove(persona);
                GuardarCambios();
            }
        }

        // ================== OPCIONES ==================
        private async void CambiarCarpeta_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();
            dialog.Title = "Seleccione la carpeta donde se guardarán los datos (Drive)";
            if (dialog.ShowDialog() == true)
            {
                string nuevaCarpeta = dialog.FolderName;
                var nuevoServicio = new ArchivoService(nuevaCarpeta);
                if (nuevoServicio.ExisteArchivo())
                {
                    if (MessageBox.Show("Ya existe un archivo de datos en esa carpeta. ¿Desea sobrescribirlo con los datos actuales?", "Advertencia", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                        return;
                }
                await nuevoServicio.GuardarAsync(App.Datos);
                App.Servicio = nuevoServicio;
                txtCarpetaActual.Text = $"Ruta actual: {nuevaCarpeta}";
                MessageBox.Show("Carpeta de datos cambiada correctamente.", "Configuración", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // ================== NAVEGACIÓN ==================
        private void AbrirBalance_Click(object sender, RoutedEventArgs e) { new VentanaBalance().Show(); this.Close(); }
        private void AbrirMovimientos_Click(object sender, RoutedEventArgs e) { new VentanaMovimientos().Show(); this.Close(); }
        private void AbrirPrestamos_Click(object sender, RoutedEventArgs e) { new VentanaPrestamos().Show(); this.Close(); }
        private void AbrirDashboard_Click(object sender, RoutedEventArgs e) { new VentanaDashboard().Show(); this.Close(); }
        private void AbrirMetas_Click(object sender, RoutedEventArgs e) { new VentanaMetas().Show(); this.Close(); }
        private void MoverVentana_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.DragMove();
    }
}