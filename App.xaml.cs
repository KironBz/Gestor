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
        // Propiedad estática para acceder a los datos desde cualquier ventana
        public static DatosApp Datos { get; private set; }

        // Servicio de persistencia (se inicializará al abrir)
        public static ArchivoService Servicio { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Usar la carpeta por defecto (Documentos/YessGestor)
            string carpeta = ArchivoService.CarpetaPorDefecto();
            Servicio = new ArchivoService(carpeta);

            // Cargar datos existentes o crear nuevos
            Datos = await Servicio.CargarAsync();

            // Si no hay cuentas, categorías o personas, crear algunas de ejemplo (opcional)
            if (Datos.Cuentas.Count == 0)
            {
                Datos.Cuentas.Add(new Cuenta("Principal", "Corriente", 1000m));
                Datos.Cuentas.Add(new Cuenta("Ahorro 10", "Oculto", 500m));
                Datos.Cuentas.Add(new Cuenta("MP", "Corriente", 200m));
                Datos.Cuentas.Add(new Cuenta("Revolut", "Corriente", 0m));
                Datos.Cuentas.Add(new Cuenta("Dinero de Mamá", "Ajeno", 300m));
            }
            if (Datos.Categorias.Count == 0)
            {
                Datos.Categorias.Add(new Categoria("Préstamo", "Ingreso"));
                Datos.Categorias.Add(new Categoria("Cargo", "Egreso"));
                Datos.Categorias.Add(new Categoria("Ahorro", "Ambos"));
                Datos.Categorias.Add(new Categoria("Comida", "Egreso"));
                Datos.Categorias.Add(new Categoria("Transporte", "Egreso"));
            }
            if (Datos.Personas.Count == 0)
            {
                Datos.Personas.Add(new Persona("Mamá", "Acreedor"));
                Datos.Personas.Add(new Persona("Ali", "Deudor"));
                Datos.Personas.Add(new Persona("Mercado Pago", "Ninguno"));
            }

            // Guardar los datos iniciales para que persistan
            await Servicio.GuardarAsync(Datos);
        }
    }
}