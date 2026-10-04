namespace YESSMobilePWA.Services
{
    /// <summary>
    /// Implementación centralizada de eventos
    /// Permite que Resumen, Balance, Metas, Dashboard se actualicen sin refresh
    /// </summary>
    public class EventService : IEventService
    {
        public event Func<Task>? OnMovimientoChanged;
        public event Func<Task>? OnMovimientoBorrado;
        public event Func<Task>? OnCuentaChanged;
        public event Func<Task>? OnSaldoActualizado;
        public event Func<Task>? OnMetaChanged;
        public event Func<Task>? OnPagoRegistrado;
        public event Func<Task>? OnDeudaChanged;
        public event Func<Task>? OnDatosRefreshCompleto;

        /// <summary>Dispara OnMovimientoChanged a todos los suscritos</summary>
        public async Task NotifyMovimientoChangedAsync()
        {
            if (OnMovimientoChanged != null)
            {
                foreach (Func<Task> handler in OnMovimientoChanged.GetInvocationList().Cast<Func<Task>>())
                {
                    try
                    {
                        await handler.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error en OnMovimientoChanged: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>Dispara OnMovimientoBorrado a todos los suscritos</summary>
        public async Task NotifyMovimientoBorradoAsync()
        {
            if (OnMovimientoBorrado != null)
            {
                foreach (Func<Task> handler in OnMovimientoBorrado.GetInvocationList().Cast<Func<Task>>())
                {
                    try
                    {
                        await handler.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error en OnMovimientoBorrado: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>Dispara OnCuentaChanged a todos los suscritos</summary>
        public async Task NotifyCuentaChangedAsync()
        {
            if (OnCuentaChanged != null)
            {
                foreach (Func<Task> handler in OnCuentaChanged.GetInvocationList().Cast<Func<Task>>())
                {
                    try
                    {
                        await handler.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error en OnCuentaChanged: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>Dispara OnSaldoActualizado a todos los suscritos</summary>
        public async Task NotifySaldoActualizadoAsync()
        {
            if (OnSaldoActualizado != null)
            {
                foreach (Func<Task> handler in OnSaldoActualizado.GetInvocationList().Cast<Func<Task>>())
                {
                    try
                    {
                        await handler.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error en OnSaldoActualizado: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>Dispara OnMetaChanged a todos los suscritos</summary>
        public async Task NotifyMetaChangedAsync()
        {
            if (OnMetaChanged != null)
            {
                foreach (Func<Task> handler in OnMetaChanged.GetInvocationList().Cast<Func<Task>>())
                {
                    try
                    {
                        await handler.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error en OnMetaChanged: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>Dispara OnPagoRegistrado a todos los suscritos</summary>
        public async Task NotifyPagoRegistradoAsync()
        {
            if (OnPagoRegistrado != null)
            {
                foreach (Func<Task> handler in OnPagoRegistrado.GetInvocationList().Cast<Func<Task>>())
                {
                    try
                    {
                        await handler.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error en OnPagoRegistrado: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>Dispara OnDeudaChanged a todos los suscritos</summary>
        public async Task NotifyDeudaChangedAsync()
        {
            if (OnDeudaChanged != null)
            {
                foreach (Func<Task> handler in OnDeudaChanged.GetInvocationList().Cast<Func<Task>>())
                {
                    try
                    {
                        await handler.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error en OnDeudaChanged: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>Dispara OnDatosRefreshCompleto a todos los suscritos</summary>
        public async Task NotifyDatosRefreshCompletoAsync()
        {
            if (OnDatosRefreshCompleto != null)
            {
                foreach (Func<Task> handler in OnDatosRefreshCompleto.GetInvocationList().Cast<Func<Task>>())
                {
                    try
                    {
                        await handler.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error en OnDatosRefreshCompleto: {ex.Message}");
                    }
                }
            }
        }
    }
}