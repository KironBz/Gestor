namespace YESSMobilePWA.Models
{
    /// <summary>
    /// Tipos de movimiento financiero.
    /// Reemplaza strings mágicos: "Ingreso", "Egreso", "Transferencia"
    /// </summary>
    public enum TipoMovimiento
    {
        Ingreso,
        Egreso,
        Transferencia
    }

    /// <summary>
    /// Estados de una meta de ahorro.
    /// Reemplaza strings mágicos: "Completada", "Archivada", etc.
    /// </summary>
    public enum EstadoMeta
    {
        Activa,
        Completada,
        Archivada
    }

    /// <summary>
    /// Visibilidad de una cuenta en el cálculo de patrimonio.
    /// Reemplaza strings mágicos: "Corriente", "Oculto", "Ahorro", etc.
    /// </summary>
    public enum VisibilidadCuenta
    {
        Corriente,
        Oculto,
        Ahorro,
        Transporte,
        Ajeno
    }

    /// <summary>
    /// Tipo de deuda en relación al usuario.
    /// Reemplaza strings mágicos: "Acreedor", "Deudor"
    /// </summary>
    public enum TipoDeuda
    {
        Acreedor,  // Debo dinero (me prestaron)
        Deudor     // Me deben dinero (di préstamo)
    }

    /// <summary>
    /// Períodos de análisis en Dashboard.
    /// Reemplaza strings mágicos: "todos", "mes1", "semestre1", etc.
    /// </summary>
    public enum Periodo
    {
        Todos,
        Mes1,
        Mes2,
        Mes3,
        Mes4,
        Mes5,
        Mes6,
        Mes7,
        Mes8,
        Mes9,
        Mes10,
        Mes11,
        Mes12,
        Semestre1,
        Semestre2
    }
}