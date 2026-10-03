using YESSMobilePWA.Models;

namespace YESSMobilePWA.Services
{
    /// <summary>
    /// Interfaz para persistencia y sincronización de datos.
    /// Abstrae localStorage + GitHub Gist sync.
    /// Implementaciones: ArchivoService (local) o futura CloudArchivoService.
    /// </summary>
    public interface IArchivoService : IAsyncDisposable
    {
        /// <summary>
        /// Cargar datos desde persistencia (localStorage).
        /// Ejecuta migraciones automáticas si es necesario.
        /// </summary>
        Task<DatosApp> CargarAsync();

        /// <summary>
        /// Guardar datos en persistencia (localStorage).
        /// Inicia debounce para sync a GitHub Gist (si configurado).
        /// </summary>
        Task GuardarAsync(DatosApp datos);

        /// <summary>
        /// Evento: cambios en el estado de sincronización.
        /// Emitido por SyncIndicator.razor para mostrar estado en UI.
        /// Estados: Inactivo, Pendiente, Sincronizando, Reintentando, Sincronizado, Error.
        /// </summary>
        event Action<SyncState>? OnSyncStateChanged;
    }
}