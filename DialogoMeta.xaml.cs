using System;
using System.Windows;
using Yes_Gestor.Models;

namespace Yes_Gestor
{
    public partial class DialogoMeta : Window
    {
        public Meta Meta { get; private set; }

        public DialogoMeta()
        {
            InitializeComponent();
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Ingrese un nombre.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!decimal.TryParse(txtMontoObjetivo.Text, out decimal monto) || monto <= 0)
            {
                MessageBox.Show("Monto objetivo inválido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(txtPrioridad.Text, out int prioridad) || prioridad < 1)
            {
                MessageBox.Show("Prioridad debe ser un número entero >= 1.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Meta = new Meta(txtNombre.Text.Trim(), monto, prioridad);
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