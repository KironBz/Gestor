using System.Collections.Generic;
using System.Windows;

namespace Yes_Gestor
{
    public partial class VentanaBalance : Window
    {
        public class DeudorMock
        {
            public string Persona { get; set; }
            public decimal MontoOriginal { get; set; }
            public decimal Pagado { get; set; }
            public decimal Pendiente => MontoOriginal - Pagado;
        }

        public class AcreedorMock
        {
            public string Persona { get; set; }
            public decimal MontoOriginal { get; set; }
            public decimal Pagado { get; set; }
            public decimal Pendiente => MontoOriginal - Pagado;
        }

        public VentanaBalance()
        {
            InitializeComponent();

            // Datos de ejemplo para deudores
            dgDeudores.ItemsSource = new List<DeudorMock>
            {
                new DeudorMock { Persona = "Ali", MontoOriginal = 500, Pagado = 200 },
                new DeudorMock { Persona = "BeMatzo", MontoOriginal = 300, Pagado = 50 },
                new DeudorMock { Persona = "MABP", MontoOriginal = 450, Pagado = 450 }
            };

            // Datos de ejemplo para acreedores
            dgAcreedores.ItemsSource = new List<AcreedorMock>
            {
                new AcreedorMock { Persona = "Mamá", MontoOriginal = 1200, Pagado = 300 },
                new AcreedorMock { Persona = "Mercado Pago", MontoOriginal = 417, Pagado = 417 },
                new AcreedorMock { Persona = "Ali", MontoOriginal = 200, Pagado = 0 }
            };

            // Valores de ejemplo para las tarjetas
            txtDineroDisponible.Text = "$12,500.00";
            txtDineroOculto.Text = "$3,200.00";
            txtCuentasPorCobrar.Text = "$1,200.00";
            txtCuentasPorPagar.Text = "$4,500.00";
            txtTotalGlobal.Text = "$12,400.00";
            txtTransporte.Text = "$350.00";      // ejemplo
            txtAjeno.Text = "$800.00";           // ejemplo
        }
    }
}