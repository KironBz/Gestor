using System;
using System.Windows;
using Yes_Gestor.Models;

namespace Yes_Gestor
{
    public partial class DialogoAjusteSaldo : Window
    {
        public decimal? NuevoSaldoReal { get; private set; }

        public DialogoAjusteSaldo(Cuenta cuenta, decimal saldoCalculado)
        {
            InitializeComponent();
            txtNombreCuenta.Text = cuenta.Nombre;
            txtSaldoCalculado.Text = saldoCalculado.ToString("C");
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(txtNuevoSaldo.Text, out decimal nuevoSaldo) || nuevoSaldo < 0)
            {
                MessageBox.Show("Ingrese un valor numérico válido (mayor o igual a cero).", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            NuevoSaldoReal = nuevoSaldo;
            DialogResult = true;
            Close();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}