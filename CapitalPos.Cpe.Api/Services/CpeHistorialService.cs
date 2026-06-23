using System.Text.Json;
using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Helpers;
using CapitalPos.Cpe.Api.Interfaces;
using CapitalPos.Cpe.Api.Settings;
using Microsoft.Extensions.Options;

namespace CapitalPos.Cpe.Api.Services;

public class CpeHistorialService : ICpeHistorialService
{
    private readonly CpeSettings _settings;
    private readonly ILogger<CpeHistorialService> _logger;

    public CpeHistorialService(
        IOptions<CpeSettings> options,
        ILogger<CpeHistorialService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public string GuardarHistorial(EmitirCpeRequest request, CpeEmisionResponse response)
    {
        var carpetaHistorial = ObtenerCarpetaHistorial();

        Directory.CreateDirectory(carpetaHistorial);

        var comprobante = CpeNombreHelper.ObtenerNumeroComprobante(request);
        var idHistorial = $"{comprobante}-{DateTime.Now:yyyyMMddHHmmss}";

        var item = new CpeHistorialItemDto
        {
            IdHistorial = idHistorial,

            Comprobante = comprobante,
            Estado = response.Estado,
            Mensaje = response.Mensaje,

            Hash = response.Hash,
            NombreXml = response.NombreXml,
            NombreZip = response.NombreZip,
            NombreCdr = response.NombreCdr,

            TipoComprobante = request.TipoComprobante,
            Serie = request.Serie,
            Correlativo = request.Correlativo,

            RucEmisor = CpeNombreHelper.ObtenerRucEmisor(request),
            RazonSocialEmisor = request.Emisor.RazonSocial,

            TipoDocumentoCliente = request.Cliente.TipoDocumento,
            NumeroDocumentoCliente = request.Cliente.NumeroDocumento,
            RazonSocialCliente = request.Cliente.RazonSocial,

            Total = request.Total,
            Moneda = request.Moneda,

            FechaEmision = request.FechaEmision,
            FechaProceso = response.FechaProceso,

            Etapas = response.Etapas,
            Errores = response.Errores
        };

        var opcionesJson = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(item, opcionesJson);

        var nombreArchivo = $"{idHistorial}.json";
        var rutaArchivo = Path.Combine(carpetaHistorial, nombreArchivo);

        File.WriteAllText(rutaArchivo, json);

        _logger.LogInformation(
            "Historial de emisión guardado correctamente: {RutaArchivo}",
            rutaArchivo
        );

        return nombreArchivo;
    }

    public List<CpeHistorialItemDto> ListarHistorial()
    {
        var carpetaHistorial = ObtenerCarpetaHistorial();

        if (!Directory.Exists(carpetaHistorial))
            return new List<CpeHistorialItemDto>();

        var archivos = Directory
            .GetFiles(carpetaHistorial, "*.json")
            .OrderByDescending(File.GetLastWriteTime)
            .ToList();

        var resultado = new List<CpeHistorialItemDto>();

        foreach (var archivo in archivos)
        {
            try
            {
                var json = File.ReadAllText(archivo);

                var item = JsonSerializer.Deserialize<CpeHistorialItemDto>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (item != null)
                    resultado.Add(item);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "No se pudo leer el archivo de historial: {Archivo}",
                    archivo
                );
            }
        }

        return resultado;
    }

    public CpeHistorialItemDto? ObtenerHistorial(string nombreArchivo)
    {
        if (!NombreHistorialValido(nombreArchivo))
            return null;

        var carpetaHistorial = ObtenerCarpetaHistorial();
        var rutaArchivo = Path.Combine(carpetaHistorial, nombreArchivo);

        if (!File.Exists(rutaArchivo))
            return null;

        var json = File.ReadAllText(rutaArchivo);

        return JsonSerializer.Deserialize<CpeHistorialItemDto>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }

    public List<CpeHistorialItemDto> BuscarHistorial(
        string? comprobante,
        string? estado,
        string? cliente,
        DateTime? desde,
        DateTime? hasta,
        int take)
    {
        if (take <= 0)
            take = 50;

        if (take > 200)
            take = 200;

        var historial = ListarHistorial().AsQueryable();

        if (!string.IsNullOrWhiteSpace(comprobante))
        {
            historial = historial.Where(x =>
                x.Comprobante.Contains(comprobante, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            historial = historial.Where(x =>
                x.Estado.Equals(estado, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(cliente))
        {
            historial = historial.Where(x =>
                x.RazonSocialCliente.Contains(cliente, StringComparison.OrdinalIgnoreCase) ||
                x.NumeroDocumentoCliente.Contains(cliente, StringComparison.OrdinalIgnoreCase));
        }

        if (desde.HasValue)
        {
            historial = historial.Where(x =>
                x.FechaProceso.Date >= desde.Value.Date);
        }

        if (hasta.HasValue)
        {
            historial = historial.Where(x =>
                x.FechaProceso.Date <= hasta.Value.Date);
        }

        return historial
            .OrderByDescending(x => x.FechaProceso)
            .Take(take)
            .ToList();
    }

    public CpeHistorialResumenResponse ObtenerResumen()
    {
        var historial = ListarHistorial();

        var resumen = new CpeHistorialResumenResponse
        {
            TotalRegistros = historial.Count,
            TotalOk = historial.Count(x => x.Errores.Count == 0),
            TotalErrores = historial.Count(x => x.Errores.Count > 0),
            MontoTotal = historial.Sum(x => x.Total),

            PrimeraEmision = historial.Count > 0
                ? historial.Min(x => x.FechaProceso)
                : null,

            UltimaEmision = historial.Count > 0
                ? historial.Max(x => x.FechaProceso)
                : null,

            PorEstado = historial
                .GroupBy(x => x.Estado)
                .ToDictionary(g => g.Key, g => g.Count()),

            PorTipoComprobante = historial
                .GroupBy(x => x.TipoComprobante)
                .ToDictionary(g => g.Key, g => g.Count()),

            MontoPorMoneda = historial
                .GroupBy(x => x.Moneda)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Total))
        };

        return resumen;
    }

    private string ObtenerCarpetaHistorial()
    {
        return Path.Combine(
            _settings.RutaArchivos,
            _settings.Modo.ToUpper(),
            "HISTORIAL"
        );
    }

    private static bool NombreHistorialValido(string nombreArchivo)
    {
        if (string.IsNullOrWhiteSpace(nombreArchivo))
            return false;

        if (nombreArchivo.Contains(".."))
            return false;

        if (nombreArchivo.Contains("/") || nombreArchivo.Contains("\\"))
            return false;

        if (!nombreArchivo.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }
}