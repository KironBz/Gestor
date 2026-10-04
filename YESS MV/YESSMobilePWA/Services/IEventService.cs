namespace YESSMobilePWA.Services
{
    /// <summary>
    /// Servicio centralizado de eventos para notificar cambios en toda la app
    /// Sin él, páginas NO se actualizan automáticamente cuando datos cambian
    /// </summary>
    public interface IEventService
    {
        // ========== EVENTOS DE DATOS ==========
        
        /// <summary>Se dispara cuando se crea o actualiza un movimiento</summary>
        event Func<Task>? OnMovimientoChanged;
        
        /// <summary>Se dispara cuando se elimina un movimiento</summary>
        event Func<Task>? OnMovimientoBorrado;
        
        /// <summary>Se dispara cuando se crea/edita/elimina una cuenta</summary>
        event Func<Task>? OnCuentaChanged;
        
        /// <summary>Se dispara cuando cambia el saldo de cualquier cuenta</summary>
        event Func<Task>? OnSaldoActualizado;
        
        /// <summary>Se dispara cuando se crea/edita/elimina una meta</summary>
        event Func<Task>? OnMetaChanged;
        
        /// <summary>Se dispara cuando se registra un pago en un préstamo</summary>
        event Func<Task>? OnPagoRegistrado;
        
        /// <summary>Se dispara cuando se crea/edita/elimina una deuda</summary>
        event Func<Task>? OnDeudaChanged;
        
        /// <summary>Se dispara cuando se agrupa un conjunto de cambios (ej: importación de datos)</summary>
        event Func<Task>? OnDatosRefreshCompleto;

        // ========== MÉTODOS DISPARADORES ==========

        /// <summary>Notifica que hubo cambio en movimientos (crear, editar)</summary>
        Task NotifyMovimientoChangedAsync();

        /// <summary>Notifica que se borró un movimiento</summary>
        Task NotifyMovimientoBorradoAsync();

        /// <summary>Notifica que hubo cambio en cuentas (crear, editar, eliminar)</summary>
        Task NotifyCuentaChangedAsync();

        /// <summary>Notifica que cambió un saldo</summary>
        Task NotifySaldoActualizadoAsync();

        /// <summary>Notifica que hubo cambio en metas</summary>
        Task NotifyMetaChangedAsync();

        /// <summary>Notifica que se registró un pago</summary>
        Task NotifyPagoRegistradoAsync();

        /// <summary>Notifica que hubo cambio en deudas</summary>
        Task NotifyDeudaChangedAsync();

        /// <summary>Notifica refresh completo de datos (ej: after load, sync, import)</summary>
        Task NotifyDatosRefreshCompletoAsync();
    }
}