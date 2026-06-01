using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Yes_Gestor.Models;

namespace Yes_Gestor
{
    public partial class VentanaMetas : Window
    {
        private Cuenta _cuentaNu;
        private List<Movimiento> _movimientosNu;

        public VentanaMetas()
        {
            InitializeComponent();
            CargarDatos();
        }

        private void CargarDatos()
        {
            // Obtener la cuenta "Nu" (ajuste según su nombre real)
            _cuentaNu = App.Datos.Cuentas.FirstOrDefault(c => c.Nombre == "Nu");
            if (_cuentaNu != null)
            {
                _movimientosNu = App.Datos.Movimientos
                    .Where(m => m.CuentaId == _cuentaNu.Id && m.Categoria == "Ahorro")
                    .ToList();
            }
            else
            {
                _movimientosNu = new List<Movimiento>();
            }

            // Calcular ahorro actual para cada meta
            foreach (var meta in App.Datos.Metas)
            {
                meta.AhorradoActual = _movimientosNu
                    .Where(m => m.MetaId == meta.Id)
                    .Sum(m => m.Tipo == "Ingreso" ? m.Monto : -m.Monto);
            }

            // Separar metas según estado
            var metasActivas = App.Datos.Metas
                .Where(m => !m.Completada && !m.Archivada)
                .OrderBy(m => m.Prioridad) // menor número = mayor prioridad
                .ThenBy(m => m.FechaCreacion)
                .ToList();

            // Meta principal: la primera de la lista (si existe)
            var metaPrincipal = metasActivas.FirstOrDefault();
            if (metaPrincipal != null)
            {
                MostrarMetaPrincipal(metaPrincipal);
                // El resto son las otras activas
                var otrasMetas = metasActivas.Skip(1).ToList();
                dgMetasActivas.ItemsSource = otrasMetas;
            }
            else
            {
                // No hay meta activa
                MostrarMetaPrincipal(null);
                dgMetasActivas.ItemsSource = new List<Meta>();
            }

            // Metas completadas (ordenadas por fecha de completación, asumiendo que usamos FechaCreacion como referencia)
            var completadas = App.Datos.Metas.Where(m => m.Completada).OrderByDescending(m => m.FechaCreacion).ToList();
            dgMetasCompletadas.ItemsSource = completadas;

            // Metas archivadas
            var archivadas = App.Datos.Metas.Where(m => m.Archivada).OrderByDescending(m => m.FechaCreacion).ToList();
            dgMetasArchivadas.ItemsSource = archivadas;
        }

        private void MostrarMetaPrincipal(Meta meta)
        {
            if (meta == null)
            {
                txtMetaNombre.Text = "No hay metas activas";
                txtMetaObjetivo.Text = "";
                txtMetaAhorrado.Text = "";
                pbMetaPrincipal.Value = 0;
                return;
            }

            txtMetaNombre.Text = meta.Nombre;
            txtMetaObjetivo.Text = $"Objetivo: {meta.MontoObjetivo:C}";
            txtMetaAhorrado.Text = $"Ahorrado: {meta.AhorradoActual:C}";
            double porcentaje = meta.MontoObjetivo > 0 ? (double)(meta.AhorradoActual / meta.MontoObjetivo * 100) : 0;
            pbMetaPrincipal.Value = porcentaje > 100 ? 100 : porcentaje;
        }

        private async void GuardarCambios()
        {
            await App.Servicio.GuardarAsync(App.Datos);
            CargarDatos(); // refrescar todo
        }

        // ================== EVENTOS DE METAS ==================
        private void CompletarMetaPrincipal_Click(object sender, RoutedEventArgs e)
        {
            var metasActivas = App.Datos.Metas.Where(m => !m.Completada && !m.Archivada)
                .OrderBy(m => m.Prioridad).ThenBy(m => m.FechaCreacion).ToList();
            var metaPrincipal = metasActivas.FirstOrDefault();
            if (metaPrincipal == null) return;

            metaPrincipal.Completada = true;
            metaPrincipal.FechaCompletada = DateTime.Now;   // ← ASIGNAR FECHA
            GuardarCambios();
        }

        private void ArchivarMetaPrincipal_Click(object sender, RoutedEventArgs e)
        {
            var metasActivas = App.Datos.Metas.Where(m => !m.Completada && !m.Archivada)
                .OrderBy(m => m.Prioridad).ThenBy(m => m.FechaCreacion).ToList();
            var metaPrincipal = metasActivas.FirstOrDefault();
            if (metaPrincipal == null) return;

            metaPrincipal.Archivada = true;
            metaPrincipal.FechaArchivada = DateTime.Now;    // ← ASIGNAR FECHA
            GuardarCambios();
        }

        private void CompletarMeta_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var meta = btn?.Tag as Meta;
            if (meta == null) return;
            meta.Completada = true;
            meta.FechaCompletada = DateTime.Now;   // ← ASIGNAR FECHA
            GuardarCambios();
        }

        private void ArchivarMeta_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var meta = btn?.Tag as Meta;
            if (meta == null) return;
            meta.Archivada = true;
            meta.FechaArchivada = DateTime.Now;    // ← ASIGNAR FECHA
            GuardarCambios();
        }

        private void ArchivarMetaCompletada_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var meta = btn?.Tag as Meta;
            if (meta == null) return;
            meta.Completada = false;
            meta.Archivada = true;
            meta.FechaArchivada = DateTime.Now;    // ← ASIGNAR FECHA (al archivar)
            GuardarCambios();
        }

        private void RestaurarMeta_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var meta = btn?.Tag as Meta;
            if (meta == null) return;
            meta.Archivada = false;
            meta.Completada = false;
            meta.FechaArchivada = null;           // ← LIMPIAR
            meta.FechaCompletada = null;          // ← LIMPIAR
            GuardarCambios();
        }

        private async void NuevaMeta_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new DialogoMeta();
            if (dialog.ShowDialog() == true)
            {
                App.Datos.Metas.Add(dialog.Meta);
                await App.Servicio.GuardarAsync(App.Datos);
                CargarDatos();
            }
        }

        private async void EliminarMeta_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var meta = btn?.Tag as Meta;
            if (meta == null) return;
            if (MessageBox.Show($"¿Eliminar permanentemente la meta '{meta.Nombre}'? Esta acción no se puede deshacer.",
                                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                App.Datos.Metas.Remove(meta);
                await App.Servicio.GuardarAsync(App.Datos);
                CargarDatos();
            }
        }

        private void EditarMeta_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var meta = btn?.Tag as Meta;
            if (meta == null) return;
            var dialog = new DialogoMeta(meta);
            if (dialog.ShowDialog() == true)
            {
                // El diálogo ya modificó las propiedades del objeto meta
                GuardarCambios();
            }
        }

        private void EditarMetaPrincipal_Click(object sender, RoutedEventArgs e)
        {
            var metasActivas = App.Datos.Metas.Where(m => !m.Completada && !m.Archivada)
                .OrderBy(m => m.Prioridad).ThenBy(m => m.FechaCreacion).ToList();
            var metaPrincipal = metasActivas.FirstOrDefault();
            if (metaPrincipal == null) return;
            var dialog = new DialogoMeta(metaPrincipal);
            if (dialog.ShowDialog() == true)
            {
                GuardarCambios();
            }
        }

        // ================== NAVEGACIÓN ==================
        private void AbrirBalance_Click(object sender, RoutedEventArgs e) { new VentanaBalance().Show(); this.Close(); }
        private void AbrirMovimientos_Click(object sender, RoutedEventArgs e) { new VentanaMovimientos().Show(); this.Close(); }
        private void AbrirPrestamos_Click(object sender, RoutedEventArgs e) { new VentanaPrestamos().Show(); this.Close(); }
        private void AbrirDashboard_Click(object sender, RoutedEventArgs e) { new VentanaDashboard().Show(); this.Close(); }

        private void MoverVentana_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.DragMove();
    }
}