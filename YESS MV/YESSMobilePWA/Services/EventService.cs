using YESSMobilePWA.Models;

namespace YESSMobilePWA.Services
{
    /// <summary>
    /// Servicio de eventos para comunicación inter-pages.
    /// Permite que Pages se suscriban y reaccionen a cambios sin reload manual.
    /// </summary>
    public class EventService
    {
        /// <summary>
        /// Se dispara cuando se agrega/edita un movimiento.
        /// </summary>
        public event EventHandler<MovimientoChangedEventArgs>? MovimientoChanged;

        /// <summary>
        /// Se dispara cuando se agrega/edita una meta.
        /// </summary>
        public event EventHandler<MetaChangedEventArgs>? MetaChanged;

        /// <summary>
        /// Se dispara cuando se agrega/edita una deuda.
        /// </summary>
        public event EventHandler<DeudaChangedEventArgs>? DeudaChanged;

        /// <summary>
        /// Se dispara cuando se agrega/edita una cuenta.
        /// </summary>
        public event EventHandler<CuentaChangedEventArgs>? CuentaChanged;

        /// <summary>
        /// Se dispara cuando hay sincronización con GitHub.
        /// </summary>
        public event EventHandler<SyncEventArgs>? SyncCompleted;

        /// <summary>
        /// Notificar que un movimiento cambió.
        /// </summary>
        public void RaiseMovimientoChanged(string accion, Movimiento? movimiento = null)
        {
            MovimientoChanged?.Invoke(this, new MovimientoChangedEventArgs 
            { 
                Accion = accion, 
                Movimiento = movimiento 
            });
        }

        /// <summary>
        /// Notificar que una meta cambió.
        /// </summary>
        public void RaiseMetaChanged(string accion, Meta? meta = null)
        {
            MetaChanged?.Invoke(this, new MetaChangedEventArgs 
            { 
                Accion = accion, 
                Meta = meta 
            });
        }

        /// <summary>
        /// Notificar que una deuda cambió.
        /// </summary>
        public void RaiseDeudaChanged(string accion, DeudaPendiente? deuda = null)
        {
            DeudaChanged?.Invoke(this, new DeudaChangedEventArgs 
            { 
                Accion = accion, 
                Deuda = deuda 
            });
        }

        /// <summary>
        /// Notificar que una cuenta cambió.
        /// </summary>
        public void RaiseCuentaChanged(string accion, Cuenta? cuenta = null)
        {
            CuentaChanged?.Invoke(this, new CuentaChangedEventArgs 
            { 
                Accion = accion, 
                Cuenta = cuenta 
            });
        }

        /// <summary>
        /// Notificar que la sincronización completó.
        /// </summary>
        public void RaiseSyncCompleted(bool exitoso, string mensaje = "")
        {
            SyncCompleted?.Invoke(this, new SyncEventArgs 
            { 
                Exitoso = exitoso, 
                Mensaje = mensaje 
            });
        }
    }

    /// <summary>
    /// Args para evento MovimientoChanged.
    /// </summary>
    public class MovimientoChangedEventArgs : EventArgs
    {
        public string Accion { get; set; } = "";  // "agregado", "editado", "eliminado"
        public Movimiento? Movimiento { get; set; }
    }

    /// <summary>
    /// Args para evento MetaChanged.
    /// </summary>
    public class MetaChangedEventArgs : EventArgs
    {
        public string Accion { get; set; } = "";
        public Meta? Meta { get; set; }
    }

    /// <summary>
    /// Args para evento DeudaChanged.
    /// </summary>
    public class DeudaChangedEventArgs : EventArgs
    {
        public string Accion { get; set; } = "";
        public DeudaPendiente? Deuda { get; set; }
    }

    /// <summary>
    /// Args para evento CuentaChanged.
    /// </summary>
    public class CuentaChangedEventArgs : EventArgs
    {
        public string Accion { get; set; } = "";
        public Cuenta? Cuenta { get; set; }
    }

    /// <summary>
    /// Args para evento SyncCompleted.
    /// </summary>
    public class SyncEventArgs : EventArgs
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = "";
    }
}