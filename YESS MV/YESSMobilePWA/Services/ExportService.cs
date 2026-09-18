using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YESSMobilePWA.Models;

namespace YESSMobilePWA.Services
{
    public static class ExportService
    {
        // ==========================================
        // CSV — todos los movimientos
        // ==========================================
        public static string GenerarCSVCompleto(DatosApp datos)
        {
            var mapaCuentas = datos.Cuentas.ToDictionary(c => c.Id, c => c.Nombre);
            var mapaPersonas = datos.Personas.ToDictionary(p => p.Id, p => p.Nombre);
            return GenerarCSV(datos.Movimientos, mapaCuentas, mapaPersonas);
        }

        // ==========================================
        // CSV — subconjunto filtrado por IDs
        // ==========================================
        public static string GenerarCSVFiltrado(DatosApp datos, IEnumerable<string> ids)
        {
            var mapaCuentas = datos.Cuentas.ToDictionary(c => c.Id, c => c.Nombre);
            var mapaPersonas = datos.Personas.ToDictionary(p => p.Id, p => p.Nombre);
            var idSet = ids.ToHashSet();
            var movimientosFiltrados = datos.Movimientos.Where(m => idSet.Contains(m.Id));
            return GenerarCSV(movimientosFiltrados, mapaCuentas, mapaPersonas);
        }

        // ==========================================
        // CORE — generador de CSV
        // ==========================================
        private static string GenerarCSV(
            IEnumerable<Movimiento> movimientos,
            Dictionary<string, string> mapaCuentas,
            Dictionary<string, string> mapaPersonas)
        {
            var sb = new StringBuilder();

            // Encabezados
            sb.AppendLine(
                "Id,FechaOcurrido,FechaRegistro,Tipo,Categoria,Cuenta,Persona," +
                "Descripcion,Monto,MontoFinal,Plazos,ReferenciaAuto,MetaId");

            foreach (var m in movimientos)
            {
                var cuenta = mapaCuentas.GetValueOrDefault(m.CuentaId, m.CuentaId);
                var persona = m.PersonaId != null
                    ? mapaPersonas.GetValueOrDefault(m.PersonaId, m.PersonaId)
                    : "";

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
        // HELPER — escapa campos CSV
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
        public static string NombreArchivoCompleto() =>
            $"yess_movimientos_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

        public static string NombreArchivoFiltrado() =>
            $"yess_movimientos_filtrado_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
    }
}