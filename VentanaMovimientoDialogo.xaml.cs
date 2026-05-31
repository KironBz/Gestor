using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Yes_Gestor.Models;

namespace Yes_Gestor
{
    public partial class VentanaMovimientoDialogo : Window
    {
        public List<Movimiento> Movimientos { get; private set; }  // ← NUEVA PROPIEDAD
        private bool _guardando = false;

        public VentanaMovimientoDialogo()
        {
            InitializeComponent();
            CargarCombos();
            cbTipo.SelectionChanged += TipoCategoria_SelectionChanged;
            cbCategoria.SelectionChanged += TipoCategoria_SelectionChanged;
        }

        private void CargarCombos()
        {
            // Cuentas
            cbCuenta.ItemsSource = App.Datos.Cuentas;
            cbCategoria.ItemsSource = App.Datos.Categorias;

            // Cuentas para transferencia
            cbCuentaOrigen.ItemsSource = App.Datos.Cuentas;
            cbCuentaDestino.ItemsSource = App.Datos.Cuentas;

            // Personas: lista con opción "(Ninguna)"
            var listaPersonas = new List<Persona>();
            listaPersonas.Add(new Persona("(Ninguna)", "Ninguno") { Id = "" });
            listaPersonas.AddRange(App.Datos.Personas);
            cbPersona.ItemsSource = listaPersonas;
            cbPersona.DisplayMemberPath = "Nombre";
            cbPersona.SelectedValuePath = "Id";
        }

        private void TipoCategoria_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string tipo = (cbTipo.SelectedItem as ComboBoxItem)?.Tag as string;
            Categoria categoria = cbCategoria.SelectedItem as Categoria;
            string categoriaNombre = categoria?.Nombre ?? "";

            bool esPrestamoCargo = (tipo == "Ingreso" && categoriaNombre == "Préstamo") ||
                                   (tipo == "Egreso" && categoriaNombre == "Cargo");
            bool esTransferencia = (tipo == "Transferencia");

            pnlPrestamo.Visibility = esPrestamoCargo ? Visibility.Visible : Visibility.Collapsed;
            pnlTransferencia.Visibility = esTransferencia ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (_guardando) return;
            _guardando = true;
            try
            {
                // Validaciones comunes
                if (cbTipo.SelectedItem == null) throw new Exception("Seleccione un tipo.");
                if (!decimal.TryParse(txtMonto.Text, out decimal monto) || monto <= 0)
                    throw new Exception("Monto inválido.");

                string tipo = (cbTipo.SelectedItem as ComboBoxItem).Tag as string;
                DateTime fecha = dpFecha.SelectedDate ?? DateTime.Today;
                string descripcion = txtDescripcion.Text;

                var movimientosGenerados = new List<Movimiento>();

                if (tipo == "Transferencia")
                {
                    // validaciones transferencia...
                    if (cbCuentaOrigen.SelectedItem == null || cbCuentaDestino.SelectedItem == null)
                        throw new Exception("Seleccione cuenta origen y destino.");
                    if (cbCuentaOrigen.SelectedItem == cbCuentaDestino.SelectedItem)
                        throw new Exception("La cuenta origen y destino no pueden ser la misma.");

                    Cuenta cuentaOrigen = cbCuentaOrigen.SelectedItem as Cuenta;
                    Cuenta cuentaDestino = cbCuentaDestino.SelectedItem as Cuenta;

                    var movOrigen = new Movimiento(fecha, "Egreso", "Transferencia", cuentaOrigen.Id, null, monto, null, descripcion, null, null);
                    var movDestino = new Movimiento(fecha, "Ingreso", "Transferencia", cuentaDestino.Id, null, monto, null, descripcion, null, null);
                    movimientosGenerados.Add(movOrigen);
                    movimientosGenerados.Add(movDestino);
                }
                else
                {
                    // movimientos normales o préstamos/cargos
                    if (cbCuenta.SelectedItem == null) throw new Exception("Seleccione una cuenta.");
                    if (cbCategoria.SelectedItem == null) throw new Exception("Seleccione una categoría.");

                    Cuenta cuenta = cbCuenta.SelectedItem as Cuenta;
                    Categoria categoria = cbCategoria.SelectedItem as Categoria;
                    Persona personaSel = cbPersona.SelectedItem as Persona;
                    string personaId = (personaSel != null && personaSel.Id != "") ? personaSel.Id : null;

                    bool esPrestamoRecibido = (tipo == "Ingreso" && categoria.Nombre == "Préstamo");
                    bool esCargo = (tipo == "Egreso" && categoria.Nombre == "Cargo");

                    decimal? montoFinal = null;
                    int? plazos = null;

                    if (esPrestamoRecibido)
                    {
                        if (!decimal.TryParse(txtMontoFinal.Text, out decimal mf) || mf <= 0)
                            throw new Exception("Monto final inválido.");
                        if (!int.TryParse(txtPlazos.Text, out int p) || p <= 0)
                            throw new Exception("Plazos inválidos.");
                        montoFinal = mf;
                        plazos = p;
                    }
                    else if (esCargo)
                    {
                        if (!string.IsNullOrWhiteSpace(txtMontoFinal.Text))
                        {
                            if (!decimal.TryParse(txtMontoFinal.Text, out decimal mf) || mf <= 0)
                                throw new Exception("Monto final inválido.");
                            montoFinal = mf;
                        }
                        if (!string.IsNullOrWhiteSpace(txtPlazos.Text))
                        {
                            if (!int.TryParse(txtPlazos.Text, out int p) || p <= 0)
                                throw new Exception("Plazos inválidos.");
                            plazos = p;
                        }
                    }

                    var movimiento = new Movimiento(fecha, tipo, categoria.Nombre, cuenta.Id, categoria.Id, monto, personaId, descripcion, montoFinal, plazos);
                    movimientosGenerados.Add(movimiento);
                }

                // 🔥 NO agregamos aquí a App.Datos, solo devolvemos la lista
                Movimientos = movimientosGenerados;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                _guardando = false;
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // ================== AGREGAR ==================
        private async void AgregarCuenta_Click(object sender, RoutedEventArgs e)
        {
            var nombre = Interaction.InputBox("Ingrese el nombre de la nueva cuenta:", "Nueva cuenta", "");
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                var nuevaCuenta = new Cuenta(nombre.Trim(), "Corriente", 0);
                App.Datos.Cuentas.Add(nuevaCuenta);
                await App.Servicio.GuardarAsync(App.Datos);
                CargarCombos();
                cbCuenta.SelectedItem = nuevaCuenta;
            }
        }

        private async void AgregarCategoria_Click(object sender, RoutedEventArgs e)
        {
            var nombre = Interaction.InputBox("Ingrese el nombre de la nueva categoría:", "Nueva categoría", "");
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                var nuevaCategoria = new Categoria(nombre.Trim(), "Ambos");
                App.Datos.Categorias.Add(nuevaCategoria);
                await App.Servicio.GuardarAsync(App.Datos);
                CargarCombos();
                cbCategoria.SelectedItem = nuevaCategoria;
            }
        }

        private async void AgregarPersona_Click(object sender, RoutedEventArgs e)
        {
            var nombre = Interaction.InputBox("Ingrese el nombre de la nueva persona:", "Nueva persona", "");
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                var nuevaPersona = new Persona(nombre.Trim(), "Ninguno");
                App.Datos.Personas.Add(nuevaPersona);
                await App.Servicio.GuardarAsync(App.Datos);
                CargarCombos();
                cbPersona.SelectedItem = nuevaPersona;
            }
        }

        // ================== ELIMINAR ==================
        private async void EliminarCuenta_Click(object sender, RoutedEventArgs e)
        {
            var cuenta = cbCuenta.SelectedItem as Cuenta;
            if (cuenta == null)
            {
                MessageBox.Show("Seleccione una cuenta para eliminar.", "Eliminar cuenta", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (App.Datos.Movimientos.Any(m => m.CuentaId == cuenta.Id))
            {
                MessageBox.Show($"No se puede eliminar la cuenta '{cuenta.Nombre}' porque tiene movimientos asociados.", "Eliminar cuenta", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (MessageBox.Show($"¿Eliminar permanentemente la cuenta '{cuenta.Nombre}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                App.Datos.Cuentas.Remove(cuenta);
                await App.Servicio.GuardarAsync(App.Datos);
                CargarCombos();
                cbCuenta.SelectedIndex = -1;
                MessageBox.Show("Cuenta eliminada correctamente.", "Eliminado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void EliminarCategoria_Click(object sender, RoutedEventArgs e)
        {
            var categoria = cbCategoria.SelectedItem as Categoria;
            if (categoria == null)
            {
                MessageBox.Show("Seleccione una categoría para eliminar.", "Eliminar categoría", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (App.Datos.Movimientos.Any(m => m.CategoriaId == categoria.Id))
            {
                MessageBox.Show($"No se puede eliminar la categoría '{categoria.Nombre}' porque tiene movimientos asociados.", "Eliminar categoría", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (MessageBox.Show($"¿Eliminar permanentemente la categoría '{categoria.Nombre}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                App.Datos.Categorias.Remove(categoria);
                await App.Servicio.GuardarAsync(App.Datos);
                CargarCombos();
                cbCategoria.SelectedIndex = -1;
                MessageBox.Show("Categoría eliminada correctamente.", "Eliminado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void EliminarPersona_Click(object sender, RoutedEventArgs e)
        {
            var persona = cbPersona.SelectedItem as Persona;
            if (persona == null || string.IsNullOrEmpty(persona.Id) || persona.Nombre == "(Ninguna)")
            {
                MessageBox.Show("Seleccione una persona válida para eliminar.", "Eliminar persona", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (App.Datos.Movimientos.Any(m => m.PersonaId == persona.Id))
            {
                MessageBox.Show($"No se puede eliminar a '{persona.Nombre}' porque tiene movimientos asociados.", "Eliminar persona", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (MessageBox.Show($"¿Eliminar permanentemente a '{persona.Nombre}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                App.Datos.Personas.Remove(persona);
                await App.Servicio.GuardarAsync(App.Datos);
                CargarCombos();
                cbPersona.SelectedIndex = 0;
                MessageBox.Show("Persona eliminada correctamente.", "Eliminado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MoverVentana_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}