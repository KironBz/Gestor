using System.Collections.Generic;
using System.Threading.Tasks;
using YESS.Models;

namespace YESS.Services
{
    public interface IArchivoService
    {
        Task<List<Movimientos>> CargarMovimientosAsync();
        Task GuardarMovimientosAsync(List<Movimientos> movimientos);
        // Agrega otros métodos que necesites, por ejemplo:
        Task<List<Cuenta>> CargarCuentasAsync();
        Task<List<Categoria>> CargarCategoriasAsync();
        Task<List<Persona>> CargarPersonasAsync();
        // etc.
    }
}
