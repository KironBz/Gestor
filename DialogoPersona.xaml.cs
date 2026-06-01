using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Yes_Gestor.Models;

namespace Yes_Gestor
{
    public partial class DialogoPersona : Window
    {
        public Persona Persona { get; private set; }
        private Persona _personaExistente;

        public DialogoPersona(Persona persona = null)
        {
            InitializeComponent();
            if (persona != null)
            {
                _personaExistente = persona;
                txtNombre.Text = persona.Nombre;
                cbTipo.SelectedItem = cbTipo.Items
                    .OfType<ComboBoxItem>()
                    .FirstOrDefault(i => i.Content.ToString() == persona.Tipo);
            }
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Ingrese un nombre.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string tipo = (cbTipo.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Ninguno";

            if (_personaExistente != null)
            {
                _personaExistente.Nombre = txtNombre.Text.Trim();
                _personaExistente.Tipo = tipo;
                Persona = _personaExistente;
            }
            else
            {
                Persona = new Persona(txtNombre.Text.Trim(), tipo);
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