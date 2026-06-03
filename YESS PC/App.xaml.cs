using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Yes_Gestor.Models;
using Yes_Gestor.Services;

namespace Yes_Gestor
{
    public partial class App : Application
    {
        public static DatosApp Datos { get; private set; }
        public static ArchivoService Servicio { get; set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string carpeta = ArchivoService.CarpetaPorDefecto();
            Servicio = new ArchivoService(carpeta);
            Datos = await Servicio.CargarAsync();

            // Datos Predeterminados
            if (Datos.Cuentas.Count == 0)
            {
                // Solo una cuenta por defecto (el usuario agregará más)
                Datos.Cuentas.Add(new Cuenta("Efectivo", "Corriente", 0m));
            }
            if (Datos.Categorias.Count == 0)
            {
                // Solo una categoría genérica
                Datos.Categorias.Add(new Categoria("Abono", "Ingreso"));
                Datos.Categorias.Add(new Categoria("Ahorro", "Ambos"));
                Datos.Categorias.Add(new Categoria("Cargo", "Egreso"));
                Datos.Categorias.Add(new Categoria("Pago", "Egreso"));
                Datos.Categorias.Add(new Categoria("Préstamo", "Ingreso"));
            }
            if (Datos.Personas.Count == 0)
            {
                // No es necesario crear personas, el diálogo ya maneja "(Ninguna)".
                // Pero si quiere una por defecto, puede poner una opción vacía o "Yo".
                // Lo dejamos vacío.
                Datos.Personas.Add(new Persona("Yo", "Ambos"));
            }

            /* DATOS DE EJEMPLO
            // Si no hay datos iniciales, crear unos de ejemplo (solo para que no esté vacío)
            if (Datos.Cuentas.Count == 0)
            {
                Datos.Cuentas.Add(new Cuenta("Principal", "Corriente", 0m));
                Datos.Cuentas.Add(new Cuenta("MP", "Corriente", 0m));
                Datos.Cuentas.Add(new Cuenta("Revolut", "Corriente", 0m));
                Datos.Cuentas.Add(new Cuenta("Ahorro 10", "Oculto", 0m));
                Datos.Cuentas.Add(new Cuenta("Dinero de Mamá", "Ajeno", 0m));
                Datos.Cuentas.Add(new Cuenta("Nu", "Ajeno", 0m));
            }
            if (Datos.Categorias.Count == 0)
            {
                Datos.Categorias.Add(new Categoria("Préstamo", "Ingreso"));
                Datos.Categorias.Add(new Categoria("Cargo", "Egreso"));
                Datos.Categorias.Add(new Categoria("Transporte", "Egreso"));
                Datos.Categorias.Add(new Categoria("Comida", "Egreso"));
                Datos.Categorias.Add(new Categoria("Ahorro", "Ambos"));
            }
            if (Datos.Personas.Count == 0)
            {
                Datos.Personas.Add(new Persona("Mamá", "Acreedor"));
                Datos.Personas.Add(new Persona("Ali", "Deudor"));
            }
            if (Datos.Metas.Count == 0)
            {
                Datos.Metas.Add(new Meta("Nueva PC", 25000m, 1));   // prioridad 1 (más urgente)
                Datos.Metas.Add(new Meta("Vacaciones", 8000m, 2));
                Datos.Metas.Add(new Meta("Fondo de emergencia", 5000m, 3));
            }
            */

            await Servicio.GuardarAsync(Datos);

            /*
            var balance = new VentanaBalance();
            balance.Show();
            */

            var balance = new VentanaBalance();
            balance.Show();
        }
    }
}