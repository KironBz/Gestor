/*
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Yes_Gestor.Models;

namespace Yes_Gestor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // ========== PRUEBA DE MOVIMIENTO ==========
            try
            {
                var prestamo = new Movimiento(
                    fechaOcurrido: new DateTime(2026, 5, 20),
                    tipo: "Ingreso",
                    categoria: "Préstamo",
                    cuentaId: 1,
                    categoriaId: 5,
                    monto: 1000m,
                    personaId: 3,
                    descripcion: "Préstamo para PC",
                    montoFinal: 1200m,
                    plazos: 6
                );

                Debug.WriteLine("=== PRUEBA MOVIMIENTO ===");
                Debug.WriteLine(prestamo.ToString());
                Debug.WriteLine($"ReferenciaAuto: {prestamo.ReferenciaAuto}");
                Debug.WriteLine($"Signo: {prestamo.Signo()}");
                Debug.WriteLine($"FechaRegistro: {prestamo.FechaRegistro:yyyy-MM-dd HH:mm:ss}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en Movimiento: {ex.Message}");
            }


            // ========== PRUEBA DE CUENTA ==========
            try
            {
                var cuenta1 = new Cuenta("Principal", "Corriente", 1000m);
                var cuenta2 = new Cuenta("Ahorro 10", "Oculto", 500m);
                var cuenta3 = new Cuenta("Dinero de Mamá", "Ajeno", 300m);

                Debug.WriteLine("\n=== PRUEBA CUENTA ===");
                Debug.WriteLine(cuenta1.ToString());
                Debug.WriteLine(cuenta2.ToString());
                Debug.WriteLine(cuenta3.ToString());

                // Probar validación de visibilidad incorrecta
                try
                {
                    var cuentaInvalida = new Cuenta("Mala", "Invisible");
                }
                catch (ArgumentException ex)
                {
                    Debug.WriteLine($"Error esperado (visibilidad inválida): {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en Cuenta: {ex.Message}");
            }


            // ========== PRUEBA DE CATEGORIA (cuando la tenga) ==========
            try
            {
                var cat1 = new Categoria("Préstamo", "Ingreso");
                var cat2 = new Categoria("Cargo", "Egreso");
                var cat3 = new Categoria("Ahorro", "Ambos");

                Debug.WriteLine("\n=== PRUEBA CATEGORIA ===");
                Debug.WriteLine(cat1.ToString());
                Debug.WriteLine(cat2.ToString());
                Debug.WriteLine(cat3.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en Categoria: {ex.Message}");
            }


            // ========== PRUEBA DE PERSONA ==========
            try
            {
                var p1 = new Persona("Mamá", "Acreedor");
                var p2 = new Persona("Ali", "Deudor");
                var p3 = new Persona("UNAM", "Ambos");

                Debug.WriteLine("\n=== PRUEBA PERSONA ===");
                Debug.WriteLine(p1.ToString());
                Debug.WriteLine(p2.ToString());
                Debug.WriteLine(p3.ToString());

                // Probar tipo inválido
                try
                {
                    var pInvalida = new Persona("Invitado", "Inocente");
                }
                catch (ArgumentException ex)
                {
                    Debug.WriteLine($"Error esperado (tipo inválido): {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en Persona: {ex.Message}");
            }


            // ========== PRUEBA DE DATOSAPP ==========
            try
            {
                var datos = new DatosApp();

                // Agregar algunas cuentas de ejemplo
                datos.Cuentas.Add(new Cuenta("Principal", "Corriente", 1000m));
                datos.Cuentas.Add(new Cuenta("Ahorro 10", "Oculto", 500m));
                datos.Cuentas.Add(new Cuenta("MP", "Corriente", 200m));

                // Agregar algunas categorías
                datos.Categorias.Add(new Categoria("Préstamo", "Ingreso"));
                datos.Categorias.Add(new Categoria("Cargo", "Egreso"));
                datos.Categorias.Add(new Categoria("Ahorro", "Ambos"));

                // Agregar algunas personas
                datos.Personas.Add(new Persona("Mamá", "Acreedor"));
                datos.Personas.Add(new Persona("Ali", "Deudor"));

                // Agregar un movimiento de ejemplo (usando Ids que aún no asignamos, pero para prueba está bien)
                var mov = new Movimiento(
                    fechaOcurrido: DateTime.Today,
                    tipo: "Ingreso",
                    categoria: "Préstamo",
                    cuentaId: 1,
                    categoriaId: 1,
                    monto: 500,
                    personaId: 1,
                    descripcion: "Préstamo de mamá",
                    montoFinal: 550,
                    plazos: 3
                );
                datos.Movimientos.Add(mov);

                Debug.WriteLine("\n=== PRUEBA DATOSAPP ===");
                Debug.WriteLine(datos.ToString());
                Debug.WriteLine($"Primer movimiento: {datos.Movimientos[0]}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en DatosApp: {ex.Message}");
            }
        }
    }
}
*/


using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Yes_Gestor.Models;
using Yes_Gestor.Services;

namespace Yes_Gestor
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            _ = ProbarAsync(); // Llamada fire-and-forget para pruebas
        }

        private async Task ProbarAsync()
        {
            try
            {
                string carpetaPrueba = Path.Combine(Path.GetTempPath(), "YessGestorTest");
                Debug.WriteLine($"Usando carpeta: {carpetaPrueba}");
                var service = new ArchivoService(carpetaPrueba);

                // Crear datos de prueba con GUIDs automáticos
                var datos = new DatosApp();

                var cuenta1 = new Cuenta("Principal", "Corriente", 1000m);
                var cuenta2 = new Cuenta("Ahorro 10", "Oculto", 500m);
                datos.Cuentas.Add(cuenta1);
                datos.Cuentas.Add(cuenta2);

                var cat1 = new Categoria("Préstamo", "Ingreso");
                var cat2 = new Categoria("Cargo", "Egreso");
                datos.Categorias.Add(cat1);
                datos.Categorias.Add(cat2);

                var persona1 = new Persona("Mamá", "Acreedor");
                datos.Personas.Add(persona1);

                var mov = new Movimiento(
                    fechaOcurrido: DateTime.Today,
                    tipo: "Ingreso",
                    categoria: "Préstamo",
                    cuentaId: cuenta1.Id,
                    categoriaId: cat1.Id,
                    monto: 500m,
                    personaId: persona1.Id,
                    descripcion: "Préstamo de prueba",
                    montoFinal: 550m,
                    plazos: 3
                );
                datos.Movimientos.Add(mov);

                await service.GuardarAsync(datos);
                Debug.WriteLine($"Guardado en: {Path.Combine(carpetaPrueba, "yes_gestor_data.json")}");

                // Mostrar JSON
                string json = await File.ReadAllTextAsync(Path.Combine(carpetaPrueba, "yes_gestor_data.json"));
                Debug.WriteLine("Contenido del JSON:");
                Debug.WriteLine(json);

                var datosCargados = await service.CargarAsync();
                Debug.WriteLine("\n=== DATOS CARGADOS ===");
                Debug.WriteLine(datosCargados.ToString());
                Debug.WriteLine($"Primera cuenta: {datosCargados.Cuentas[0]}");
                Debug.WriteLine($"Primera categoría: {datosCargados.Categorias[0]}");
                Debug.WriteLine($"Primera persona: {datosCargados.Personas[0]}");
                Debug.WriteLine($"Primer movimiento: {datosCargados.Movimientos[0]}");
                Debug.WriteLine($"IDs: Cuenta={datosCargados.Cuentas[0].Id}, Categoría={datosCargados.Categorias[0].Id}, Persona={datosCargados.Personas[0].Id}, Movimiento={datosCargados.Movimientos[0].Id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                    Debug.WriteLine($"Detalle: {ex.InnerException.Message}");
            }
        }
    }
}