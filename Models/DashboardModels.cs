namespace YESSMobilePWA.Models
{
    public class TotalItem
    {
        public string Categoria { get; set; } = "";
        public double Monto { get; set; }
        public string? Color { get; set; }
    }

    public class CategoriaItem
    {
        public string Categoria { get; set; } = "";
        public double Monto { get; set; }
    }

    public class GastoMensual
    {
        public string Mes { get; set; } = "";
        public double Gasto { get; set; }
    }
}