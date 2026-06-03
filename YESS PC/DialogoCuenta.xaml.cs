using System;
using System.Windows;
using System.Windows.Controls;
using Yes_Gestor.Models;

namespace Yes_Gestor
{
    public partial class DialogoCuenta : Window
    {
        public Cuenta Cuenta { get; private set; }
        private Cuenta _cuentaExistente;

        public DialogoCuenta(Cuenta cuenta = null)
        {
            InitializeComponent();
            if (cuenta != null)
            {
                _cuentaExistente = cuenta;
                txtNombre.Text = cuenta.Nombre;
                cbVisibilidad.SelectedItem = cbVisibilidad.Items.OfType<ComboBoxItem>().FirstOrDefault(i => i.Content.ToString() == cuenta.Visibilidad);
                txtSaldoInicial.Text = cuenta.SaldoInicial.ToString();
            }
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Ingrese un nombre.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!decimal.TryParse(txtSaldoInicial.Text, out decimal saldoInicial))
                saldoInicial = 0;
            string visibilidad = (cbVisibilidad.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Corriente";

            if (_cuentaExistente != null)
            {
                _cuentaExistente.Nombre = txtNombre.Text.Trim();
                _cuentaExistente.Visibilidad = visibilidad;
                _cuentaExistente.SaldoInicial = saldoInicial;
                Cuenta = _cuentaExistente;
            }
            else
            {
                Cuenta = new Cuenta(txtNombre.Text.Trim(), visibilidad, saldoInicial);
            }
            DialogResult = true;
            Close();
        }
        private void Cancelar_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }
    }
}