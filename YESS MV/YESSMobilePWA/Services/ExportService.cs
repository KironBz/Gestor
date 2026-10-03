using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using YESSMobilePWA.Models;

namespace YESSMobilePWA.Services
{
    /// <summary>
    /// Servicio de exportación de datos en CSV.
    /// Implementa IExportService (inyectable, testeable).
    /// </summary>
    public class ExportService : IExportService
    {
        private readonly ILogger<ExportService> _logger;

        public ExportService(ILogger<ExportService> logger)
        {
            _logger = logger;
        }

        // ==========================================
        // CSV — todos los movimientos
        // ==========================================
        public string GenerarCSVCompleto(DatosApp datos)
        {
            if (datos == null) throw new ArgumentNullException(nameof(datos));

            var mapaCuentas = datos.Cuentas.ToDictionary(c => c.Id, c => c.Nombre);
            var mapaPersonas = datos.Personas.ToDictionary(p => p.Id, p => p.Nombre);
            return GenerarCSV(datos.Movimientos, mapaCuentas, mapaPersonas);
        }

        // ==========================================
        // CSV — subconjunto filtrado por IDs
        // ==========================================
        public string GenerarCSVFiltrado(DatosApp datos, IEnumerable<string> ids)
        {
            if (datos == null) throw new ArgumentNullException(nameof(datos));
            
            if (ids == null || !ids.Any())
            {
                _logger.LogWarning("GenerarCSVFiltrado: IDs vacío, retornando completo");
                return GenerarCSVCompleto(datos);
            }

            var mapaCuentas = datos.Cuentas.ToDictionary(c => c.Id, c => c.Nombre);
            var mapaPersonas = datos.Personas.ToDictionary(p => p.Id, p => p.Nombre);
            var idSet = ids.ToHashSet();
            var movimientosFiltrados = datos.Movimientos.Where(m => idSet.Contains(m.Id));
            return GenerarCSV(movimientosFiltrados, mapaCuentas, mapaPersonas);
        }

        // ==========================================
        // CORE — generador de CSV
        // ==========================================
        private string GenerarCSV(
            IEnumerable<Movimiento> movimientos,
            Dictionary<string, string> mapaCuentas,
            Dictionary<string, string> mapaPersonas)
        {
            var sb = new StringBuilder();

            // Agregar BOM UTF-8 para evitar corrupción en Excel
            sb.Append(Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble()));

            // Encabezados
            sb.AppendLine(
                "Id,FechaOcurrido,FechaRegistro,Tipo,Categoria,Cuenta,Persona," +
                "Descripcion,Monto,MontoFinal,Plazos,ReferenciaAuto,MetaId");

            foreach (var m in movimientos)
            {
                var cuenta = mapaCuentas.GetValueOrDefault(m.CuentaId, "[Desconocida]");
                var persona = m.PersonaId != null
                    ? mapaPersonas.GetValueOrDefault(m.PersonaId, m.PersonaId)
                    : "";

                if (!mapaCuentas.ContainsKey(m.CuentaId))
                {
                    _logger.LogWarning($"Cuenta no encontrada en export: {m.CuentaId}");
                }

                sb.AppendLine(string.Join(",", new[]
                {
                    Escapar(m.Id),
                    m.FechaOcurrido.ToString("yyyy-MM-dd"),
                    m.FechaRegistro.ToString("yyyy-MM-dd HH:mm:ss"),
                    Escapar(m.Tipo),
                    Escapar(m.Categoria),
                    Escapar(cuenta),
                    Escapar(persona),
                    Escapar(m.Descripcion ?? ""),
                    m.Monto.ToString("F2"),
                    m.MontoFinal?.ToString("F2") ?? "",
                    m.Plazos?.ToString() ?? "",
                    Escapar(m.ReferenciaAuto ?? ""),
                    Escapar(m.MetaId ?? "")
                }));
            }

            return sb.ToString();
        }

        // ==========================================
        // HELPER — escapa campos CSV (RFC 4180)
        // ==========================================
        private static string Escapar(string valor)
        {
            if (valor.Contains(',') || valor.Contains('"') || valor.Contains('\n'))
                return $"\"{valor.Replace("\"", "\"\"")}\"";
            return valor;
        }

        // ==========================================
        // NOMBRE DE ARCHIVO — convención estándar
        // ==========================================
        public string NombreArchivoCompleto() =>
            $"yess_movimientos_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

        public string NombreArchivoFiltrado() =>
            $"yess_movimientos_filtrado_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
    }
}