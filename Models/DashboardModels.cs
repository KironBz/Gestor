namespace YESS.Models
{
    public class TotalItem
    {
        public string Categoria { get; set; } = string.Empty;
        public double Monto { get; set; }
        public string? Color { get; set; }
    }

    public class CategoriaItem
    {
        public string Categoria { get; set; } = string.Empty;
        public double Monto { get; set; }
    }

    public class GastoMensual
    {
        public string Mes { get; set; } = string.Empty;
        public double Gasto { get; set; }
    }
}