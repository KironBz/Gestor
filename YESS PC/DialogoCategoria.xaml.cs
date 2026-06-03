using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Yes_Gestor.Models;

namespace Yes_Gestor
{
    public partial class DialogoCategoria : Window
    {
        public Categoria Categoria { get; private set; }
        private Categoria _categoriaExistente;

        public DialogoCategoria(Categoria categoria = null)
        {
            InitializeComponent();
            if (categoria != null)
            {
                _categoriaExistente = categoria;
                txtNombre.Text = categoria.Nombre;
                // Seleccionar el tipo permitido en el ComboBox
                cbTipoPermitido.SelectedItem = cbTipoPermitido.Items
                    .OfType<ComboBoxItem>()
                    .FirstOrDefault(i => i.Content.ToString() == categoria.TipoPermitido);
            }
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Ingrese un nombre.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string tipoPermitido = (cbTipoPermitido.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Ambos";

            if (_categoriaExistente != null)
            {
                _categoriaExistente.Nombre = txtNombre.Text.Trim();
                _categoriaExistente.TipoPermitido = tipoPermitido;
                Categoria = _categoriaExistente;
            }
            else
            {
                Categoria = new Categoria(txtNombre.Text.Trim(), tipoPermitido);
            }
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