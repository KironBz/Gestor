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
        public List<Movimiento> Movimientos { get; private set; }
        private bool _guardando = false;
        private Movimiento _movimientoOriginal; // para edición
        private bool _editando = false;

        // Constructor para NUEVO movimiento
        public VentanaMovimientoDialogo()
        {
            InitializeComponent();
            CargarCombos();
            cbTipo.SelectionChanged += TipoCategoria_SelectionChanged;
            cbCategoria.SelectionChanged += TipoCategoria_SelectionChanged;
            cbCuenta.SelectionChanged += cbCuenta_SelectionChanged;
        }

        // Constructor para EDITAR movimiento existente
        public VentanaMovimientoDialogo(Movimiento movimiento)
        {
            InitializeComponent();
            _movimientoOriginal = movimiento;
            _editando = true;
            CargarCombos();
            CargarDatosDesdeMovimiento(movimiento);
            cbTipo.SelectionChanged += TipoCategoria_SelectionChanged;
            cbCategoria.SelectionChanged += TipoCategoria_SelectionChanged;
            cbCuenta.SelectionChanged += cbCuenta_SelectionChanged;
        }

        private void CargarCombos()
        {
            cbCuenta.ItemsSource = App.Datos.Cuentas;
            cbCategoria.ItemsSource = App.Datos.Categorias;
            cbCuentaOrigen.ItemsSource = App.Datos.Cuentas;
            cbCuentaDestino.ItemsSource = App.Datos.Cuentas;

            var listaPersonas = new List<Persona>();
            listaPersonas.Add(new Persona("(Ninguna)", "Ninguno") { Id = "" });
            listaPersonas.AddRange(App.Datos.Personas);
            cbPersona.ItemsSource = listaPersonas;
            cbPersona.DisplayMemberPath = "Nombre";
            cbPersona.SelectedValuePath = "Id";

            var metasActivas = App.Datos.Metas?.Where(m => !m.Completada && !m.Archivada)
                .OrderBy(m => m.Prioridad).ThenBy(m => m.FechaCreacion).ToList();
            cbMeta.ItemsSource = metasActivas;
            cbMeta.DisplayMemberPath = "Nombre";
            cbMeta.SelectedValuePath = "Id";
        }

        private void CargarDatosDesdeMovimiento(Movimiento mov)
        {
            dpFecha.SelectedDate = mov.FechaOcurrido;
            // Seleccionar tipo
            foreach (ComboBoxItem item in cbTipo.Items)
            {
                if (item.Tag.ToString() == mov.Tipo)
                {
                    cbTipo.SelectedItem = item;
                    break;
                }
            }
            // Seleccionar cuenta
            if (!string.IsNullOrEmpty(mov.CuentaId))
                cbCuenta.SelectedItem = App.Datos.Cuentas.FirstOrDefault(c => c.Id == mov.CuentaId);
            // Seleccionar categoría
            if (!string.IsNullOrEmpty(mov.CategoriaId))
                cbCategoria.SelectedItem = App.Datos.Categorias.FirstOrDefault(c => c.Id == mov.CategoriaId);
            // Seleccionar persona
            if (!string.IsNullOrEmpty(mov.PersonaId))
                cbPersona.SelectedItem = App.Datos.Personas.FirstOrDefault(p => p.Id == mov.PersonaId) ?? cbPersona.Items.OfType<Persona>().FirstOrDefault(p => p.Nombre == "(Ninguna)");
            else
                cbPersona.SelectedItem = cbPersona.Items.OfType<Persona>().FirstOrDefault(p => p.Nombre == "(Ninguna)");
            txtDescripcion.Text = mov.Descripcion;
            txtMonto.Text = mov.Monto.ToString();
            if (mov.MontoFinal.HasValue)
                txtMontoFinal.Text = mov.MontoFinal.Value.ToString();
            if (mov.Plazos.HasValue)
                txtPlazos.Text = mov.Plazos.Value.ToString();
            if (!string.IsNullOrEmpty(mov.MetaId))
                cbMeta.SelectedItem = App.Datos.Metas.FirstOrDefault(m => m.Id == mov.MetaId);
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

        private void cbCuenta_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cuenta = cbCuenta.SelectedItem as Cuenta;
            bool esCuentaAhorro = (cuenta?.Nombre == "Nu");
            if (esCuentaAhorro)
            {
                var categoriaAhorro = App.Datos.Categorias.FirstOrDefault(c => c.Nombre == "Ahorro");
                if (categoriaAhorro != null)
                    cbCategoria.SelectedItem = categoriaAhorro;
                cbCategoria.IsEnabled = false;
                pnlMeta.Visibility = Visibility.Visible;
                CargarCombos(); // refrescar metas
            }
            else
            {
                cbCategoria.IsEnabled = true;
                pnlMeta.Visibility = Visibility.Collapsed;
                cbMeta.SelectedItem = null;
            }
        }

        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (_guardando) return;
            _guardando = true;
            try
            {
                if (cbTipo.SelectedItem == null) throw new Exception("Seleccione un tipo.");
                if (!decimal.TryParse(txtMonto.Text, out decimal monto) || monto <= 0)
                    throw new Exception("Monto inválido.");

                string tipo = (cbTipo.SelectedItem as ComboBoxItem).Tag as string;
                DateTime fecha = dpFecha.SelectedDate ?? DateTime.Today;
                string descripcion = txtDescripcion.Text;

                var movimientosGenerados = new List<Movimiento>();

                if (tipo == "Transferencia")
                {
                    if (cbCuentaOrigen.SelectedItem == null || cbCuentaDestino.SelectedItem == null)
                        throw new Exception("Seleccione cuenta origen y destino.");
                    if (cbCuentaOrigen.SelectedItem == cbCuentaDestino.SelectedItem)
                        throw new Exception("La cuenta origen y destino no pueden ser la misma.");

                    Cuenta cuentaOrigen = cbCuentaOrigen.SelectedItem as Cuenta;
                    Cuenta cuentaDestino = cbCuentaDestino.SelectedItem as Cuenta;

                    var movOrigen = new Movimiento(fecha, "Egreso", "Transferencia", cuentaOrigen.Id, null, monto, null, descripcion);
                    var movDestino = new Movimiento(fecha, "Ingreso", "Transferencia", cuentaDestino.Id, null, monto, null, descripcion);
                    movimientosGenerados.Add(movOrigen);
                    movimientosGenerados.Add(movDestino);
                }
                else
                {
                    if (cbCuenta.SelectedItem == null) throw new Exception("Seleccione una cuenta.");
                    if (cbCategoria.SelectedItem == null) throw new Exception("Seleccione una categoría.");

                    Cuenta cuenta = cbCuenta.SelectedItem as Cuenta;
                    Categoria categoria = cbCategoria.SelectedItem as Categoria;
                    Persona personaSel = cbPersona.SelectedItem as Persona;
                    string personaId = (personaSel != null && personaSel.Id != "") ? personaSel.Id : null;

                    bool esCuentaAhorro = (cuenta?.Nombre == "Nu");
                    string metaId = null;
                    if (esCuentaAhorro)
                    {
                        if (cbMeta.SelectedItem == null)
                            throw new Exception("Debe seleccionar una meta de ahorro para esta cuenta.");
                        metaId = (cbMeta.SelectedItem as Meta)?.Id;
                    }

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

                    var movimiento = new Movimiento(fecha, tipo, categoria.Nombre, cuenta.Id, categoria.Id, monto, personaId, descripcion, montoFinal, plazos, metaId);
                    if (_editando && _movimientoOriginal != null)
                    {
                        movimiento.Id = _movimientoOriginal.Id; // conservar el mismo ID para reemplazar
                    }
                    movimientosGenerados.Add(movimiento);
                }

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

        private void Cancelar_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }
        private void MoverVentana_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.DragMove();

        // ================== AGREGAR / ELIMINAR (CUENTAS, CATEGORÍAS, PERSONAS) ==================
        // ... (mantenga aquí todos los métodos que ya tenía, como AgregarCuenta_Click, EliminarCuenta_Click, etc.)
        // Si no los tiene, los incluyo a continuación:
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
        private async void EliminarCuenta_Click(object sender, RoutedEventArgs e)
        {
            var cuenta = cbCuenta.SelectedItem as Cuenta;
            if (cuenta == null) return;
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
            }
        }
        private async void EliminarCategoria_Click(object sender, RoutedEventArgs e)
        {
            var categoria = cbCategoria.SelectedItem as Categoria;
            if (categoria == null) return;
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
            }
        }
        private async void EliminarPersona_Click(object sender, RoutedEventArgs e)
        {
            var persona = cbPersona.SelectedItem as Persona;
            if (persona == null || string.IsNullOrEmpty(persona.Id) || persona.Nombre == "(Ninguna)") return;
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
            }
        }
    }
}