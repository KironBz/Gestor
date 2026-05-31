using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Yes_Gestor.Models;

namespace Yes_Gestor.Services
{
    public class ArchivoService
    {
        private readonly string _carpetaDatos;
        private readonly string _nombreArchivo = "yes_gestor_data.json";
        private string RutaCompleta => Path.Combine(_carpetaDatos, _nombreArchivo);

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true   // CLAVE: ignora mayúsculas/minúsculas
        };

        public ArchivoService(string carpetaDatos)
        {
            if (string.IsNullOrWhiteSpace(carpetaDatos))
                throw new ArgumentException("Carpeta inválida");
            _carpetaDatos = carpetaDatos;
            Directory.CreateDirectory(_carpetaDatos);
        }

        public async Task GuardarAsync(DatosApp datos)
        {
            string json = JsonSerializer.Serialize(datos, _jsonOptions);
            await File.WriteAllTextAsync(RutaCompleta, json);
        }

        public async Task<DatosApp> CargarAsync()
        {
            if (!File.Exists(RutaCompleta))
                return new DatosApp();
            string json = await File.ReadAllTextAsync(RutaCompleta);
            var datos = JsonSerializer.Deserialize<DatosApp>(json, _jsonOptions);
            return datos ?? new DatosApp();
        }

        public static string CarpetaPorDefecto() =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "YessGestor");

        public bool ExisteArchivo() => File.Exists(RutaCompleta);
    }
}