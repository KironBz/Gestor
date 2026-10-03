using YESSMobilePWA.Models;

namespace YESSMobilePWA.Services
{
    /// <summary>
    /// Interfaz para exportación de datos en múltiples formatos.
    /// Facilita testing e inyección de dependencias.
    /// Implementaciones: ExportService (CSV), ExportServiceJSON (futura).
    /// </summary>
    public interface IExportService
    {
        /// <summary>
        /// Generar CSV con TODOS los movimientos.
        /// </summary>
        string GenerarCSVCompleto(DatosApp datos);

        /// <summary>
        /// Generar CSV con movimientos filtrados por IDs.
        /// </summary>
        string GenerarCSVFiltrado(DatosApp datos, IEnumerable<string> ids);

        /// <summary>
        /// Nombre estándar para archivo CSV descargado (con timestamp).
        /// </summary>
        string NombreArchivoCompleto();

        /// <summary>
        /// Nombre estándar para archivo CSV filtrado descargado.
        /// </summary>
        string NombreArchivoFiltrado();
    }
}