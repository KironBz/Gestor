using System.Collections.Generic;
using System.Threading.Tasks;
using YESS.Models;                    // ← añadido

namespace YESS.Services
{
    public interface IArchivoService
    {
        // Métodos que realmente se usan en la app (coinciden con ArchivoService)
        Task<DatosApp> CargarAsync();
        Task GuardarAsync(DatosApp datos);

        // Opcionales: si luego los necesitas
        Task<List<Movimiento>> CargarMovimientosAsync();   // ← Movimiento (singular)
        Task GuardarMovimientosAsync(List<Movimiento> movimientos);
        Task<List<Cuenta>> CargarCuentasAsync();
        Task<List<Categoria>> CargarCategoriasAsync();
        Task<List<Persona>> CargarPersonasAsync();
    }
}