using YESSMobilePWA.Models;

namespace YESSMobilePWA.Services
{
    /// <summary>
    /// Servicio centralizado de validaciones.
    /// Evita validación duplicada en Pages.
    /// </summary>
    public class ValidationService
    {
        /// <summary>
        /// Validar que un movimiento sea válido antes de guardar.
        /// </summary>
        public List<string> ValidarMovimiento(Movimiento movimiento)
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(movimiento.Categoria))
                errores.Add("La categoría es obligatoria");

            if (movimiento.Monto <= 0)
                errores.Add("El monto debe ser mayor a 0");

            if (string.IsNullOrWhiteSpace(movimiento.CuentaId))
                errores.Add("Debes seleccionar una cuenta");

            if (movimiento.FechaOcurrido > DateTime.UtcNow)
                errores.Add("La fecha no puede ser en el futuro");

            return errores;
        }

        /// <summary>
        /// Validar que una meta sea válida.
        /// </summary>
        public List<string> ValidarMeta(Meta meta)
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(meta.Nombre))
                errores.Add("El nombre de la meta es obligatorio");

            if (meta.MontoObjetivo <= 0)
                errores.Add("El monto objetivo debe ser mayor a 0");

            return errores;
        }

        /// <summary>
        /// Validar que una cuenta sea válida.
        /// </summary>
        public List<string> ValidarCuenta(Cuenta cuenta)
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(cuenta.Nombre))
                errores.Add("El nombre de la cuenta es obligatorio");

            if (string.IsNullOrWhiteSpace(cuenta.Visibilidad))
                errores.Add("Debes seleccionar una visibilidad");

            return errores;
        }
    }
}