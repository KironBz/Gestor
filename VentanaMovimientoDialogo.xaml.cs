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
        public Movimiento Movimiento { get; private set; }
        private bool _guardando = false; // Evita doble guardado

        public VentanaMovimientoDialogo()
        {
            InitializeComponent();
            CargarCombos();
            // Suscribir eventos de cambio (ya están en XAML, pero por seguridad)
            cbTipo.SelectionChanged += TipoCategoria_SelectionChanged;
            cbCategoria.SelectionChanged += TipoCategoria_SelectionChanged;
        }

        private void CargarCombos()
        {
            // Cuentas
            cbCuenta.ItemsSource = App.Datos.Cuentas;
            // Categorías
            cbCategoria.ItemsSource = App.Datos.Categorias;

            // Personas: crear una lista que incluya una opción "Ninguna" como objeto Persona
            var listaPersonas = new List<Persona>();
            listaPersonas.Add(new Persona("(Ninguna)", "Ninguno") { Id = "" }); // elemento vacío
            listaPersonas.AddRange(App.Datos.Personas);
            cbPersona.ItemsSource = listaPersonas;
            cbPersona.DisplayMemberPath = "Nombre";
            cbPersona.SelectedValuePath = "Id";
        }

        // Manejador unificado para cambios en Tipo y Categoría (muestra/oculta panel de préstamo)
        private void TipoCategoria_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string tipo = (cbTipo.SelectedItem as ComboBoxItem)?.Tag as string;
            Categoria categoria = cbCategoria.SelectedItem as Categoria;
            string categoriaNombre = categoria?.Nombre ?? "";

            bool mostrarPanel = (tipo == "Ingreso" && categoriaNombre == "Préstamo") ||
                                (tipo == "Egreso" && categoriaNombre == "Cargo");

            pnlPrestamo.Visibility = mostrarPanel ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (_guardando) return; // Evita ejecución múltiple
            _guardando = true;
            try
            {
                // Validaciones comunes
                if (cbTipo.SelectedItem == null) throw new Exception("Seleccione un tipo.");
                if (cbCuenta.SelectedItem == null) throw new Exception("Seleccione una cuenta.");
                if (cbCategoria.SelectedItem == null) throw new Exception("Seleccione una categoría.");
                if (!decimal.TryParse(txtMonto.Text, out decimal monto) || monto <= 0)
                    throw new Exception("Monto inválido.");

                string tipo = (cbTipo.SelectedItem as ComboBoxItem).Tag as string;
                Cuenta cuenta = cbCuenta.SelectedItem as Cuenta;
                Categoria categoria = cbCategoria.SelectedItem as Categoria;
                Persona personaSel = cbPersona.SelectedItem as Persona;
                string personaId = (personaSel != null && personaSel.Id != "") ? personaSel.Id : null;
                DateTime fecha = dpFecha.SelectedDate ?? DateTime.Today;

                // Determinar el tipo de operación
                bool esPrestamoRecibido = (tipo == "Ingreso" && categoria.Nombre == "Préstamo");
                bool esCargo = (tipo == "Egreso" && categoria.Nombre == "Cargo");

                decimal? montoFinal = null;
                int? plazos = null;

                if (esPrestamoRecibido)
                {
                    // Obligatorio: monto final y plazos
                    if (!decimal.TryParse(txtMontoFinal.Text, out decimal mf) || mf <= 0)
                        throw new Exception("Monto final inválido.");
                    if (!int.TryParse(txtPlazos.Text, out int p) || p <= 0)
                        throw new Exception("Plazos inválidos.");
                    montoFinal = mf;
                    plazos = p;
                }
                else if (esCargo)
                {
                    // Opcional: si se llenaron, los tomamos; si no, quedan nulos
                    if (!string.IsNullOrWhiteSpace(txtMontoFinal.Text))
                    {
                        if (!decimal.TryParse(txtMontoFinal.Text, out decimal mf) || mf <= 0)
                            throw new Exception("Monto final inválido (debe ser mayor a cero).");
                        montoFinal = mf;
                    }
                    if (!string.IsNullOrWhiteSpace(txtPlazos.Text))
                    {
                        if (!int.TryParse(txtPlazos.Text, out int p) || p <= 0)
                            throw new Exception("Plazos inválidos (deben ser mayor a cero).");
                        plazos = p;
                    }
                }

                // Crear el movimiento (NO LO AGREGAMOS AQUÍ, solo lo devolvemos)
                var movimiento = new Movimiento(
                    fechaOcurrido: fecha,
                    tipo: tipo,
                    categoria: categoria.Nombre,
                    cuentaId: cuenta.Id,
                    categoriaId: categoria.Id,
                    monto: monto,
                    personaId: personaId,
                    descripcion: txtDescripcion.Text,
                    montoFinal: montoFinal,
                    plazos: plazos
                );

                // 🔥 IMPORTANTE: NO agregar movimiento aquí, ni guardar.
                // Solo asignamos la propiedad y cerramos el diálogo.
                Movimiento = movimiento;
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
                cbPersona.SelectedIndex = 0; // selecciona "(Ninguna)"
                MessageBox.Show("Persona eliminada correctamente.", "Eliminado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MoverVentana_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}