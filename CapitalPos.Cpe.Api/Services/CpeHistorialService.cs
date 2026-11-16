using System.Text.Json;
using CapitalPos.Cpe.Api.Data;
using CapitalPos.Cpe.Api.Data.Entities;
using CapitalPos.Cpe.Api.Domain.Tenancy;
using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Helpers;
using CapitalPos.Cpe.Api.Interfaces;
using CapitalPos.Cpe.Api.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CapitalPos.Cpe.Api.Services;

public class CpeHistorialService : ICpeHistorialService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly CpeDbContext _db;
    private readonly CpeSettings _settings;
    private readonly ILogger<CpeHistorialService> _logger;

    public CpeHistorialService(
        CpeDbContext db,
        IOptions<CpeSettings> options,
        ILogger<CpeHistorialService> logger)
    {
        _db = db;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<CpeEmisionResponse?> ObtenerEmisionExistenteAsync(
        string ruc,
        string tipoComprobante,
        string serie,
        int correlativo,
        CancellationToken cancellationToken = default)
    {
        var existente = await _db.ComprobantesEmitidos
            .AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.Ruc == ruc
                     && c.TipoComprobante == tipoComprobante
                     && c.Serie == serie
                     && c.Correlativo == correlativo,
                cancellationToken);

        if (existente == null)
            return null;

        return MapToEmisionResponse(existente);
    }

    public string GuardarHistorial(EmitirCpeRequest request, CpeEmisionResponse response)
    {
        return GuardarHistorialAsync(request, response, context: null)
            .GetAwaiter()
            .GetResult();
    }

    public async Task<string> GuardarHistorialAsync(
        EmitirCpeRequest request,
        CpeEmisionResponse response,
        EmisorRuntimeContext? context,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var ruc = CpeNombreHelper.ObtenerRucEmisor(request);
        var comprobante = CpeNombreHelper.ObtenerNumeroComprobante(request);
        var idHistorial = $"{comprobante}-{DateTime.Now:yyyyMMddHHmmss}";

        var item = CrearItemDto(request, response, idHistorial, ruc);

        // Transición: también escribir JSON en disco (deprecado).
        try
        {
            GuardarJsonLegacy(item, context?.Ambiente ?? _settings.Modo, ruc);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo guardar historial JSON legacy para {Comprobante}", comprobante);
        }

        if (context == null || context.EsLegacy || context.EmisorId == Guid.Empty)
        {
            _logger.LogInformation(
                "Historial BD omitido (sin EmisorId). IdHistorial={IdHistorial}",
                idHistorial);
            return idHistorial;
        }

        var existente = await _db.ComprobantesEmitidos
            .Include(c => c.Cdrs)
            .FirstOrDefaultAsync(
                c => c.Ruc == ruc
                     && c.TipoComprobante == request.TipoComprobante
                     && c.Serie == request.Serie
                     && c.Correlativo == request.Correlativo,
                cancellationToken);

        if (existente != null)
        {
            ActualizarComprobante(existente, request, response, correlationId);
            if (!string.IsNullOrWhiteSpace(response.NombreCdr) &&
                existente.Cdrs.All(c => c.NombreArchivo != response.NombreCdr))
            {
                existente.Cdrs.Add(CrearCdr(existente.Id, response));
            }

            await _db.SaveChangesAsync(cancellationToken);
            return existente.Id.ToString();
        }

        var entity = new ComprobanteEmitido
        {
            Id = Guid.NewGuid(),
            EmisorId = context.EmisorId,
            Ruc = ruc,
            TipoComprobante = request.TipoComprobante,
            Serie = request.Serie,
            Correlativo = request.Correlativo,
            Estado = response.Estado,
            Mensaje = response.Mensaje,
            Hash = response.Hash,
            NombreXml = response.NombreXml,
            NombreZip = response.NombreZip,
            NombreCdr = response.NombreCdr,
            RazonSocialEmisor = request.Emisor.RazonSocial,
            TipoDocumentoCliente = request.Cliente.TipoDocumento,
            NumeroDocumentoCliente = request.Cliente.NumeroDocumento,
            RazonSocialCliente = request.Cliente.RazonSocial,
            Total = request.Total,
            Moneda = request.Moneda,
            SistemaOrigen = request.SistemaOrigen,
            CorrelationId = correlationId,
            FechaEmision = ToUtc(request.FechaEmision),
            FechaProceso = ToUtc(response.FechaProceso),
            EtapasJson = JsonSerializer.Serialize(response.Etapas),
            ErroresJson = JsonSerializer.Serialize(response.Errores)
        };

        if (!string.IsNullOrWhiteSpace(response.NombreCdr))
        {
            entity.Cdrs.Add(CrearCdr(entity.Id, response));
        }

        _db.ComprobantesEmitidos.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Historial BD guardado. Comprobante={Comprobante}, Id={Id}",
            comprobante,
            entity.Id);

        return entity.Id.ToString();
    }

    public List<CpeHistorialItemDto> ListarHistorial(string? ruc = null)
    {
        var query = _db.ComprobantesEmitidos.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(ruc))
            query = query.Where(c => c.Ruc == ruc.Trim());

        var items = query
            .OrderByDescending(c => c.FechaProceso)
            .Take(500)
            .ToList()
            .Select(MapToHistorialItem)
            .ToList();

        if (items.Count > 0)
            return items;

        // Fallback a JSON legacy si BD vacía.
        return ListarHistorialJsonLegacy()
            .Where(x => string.IsNullOrWhiteSpace(ruc)
                        || x.RucEmisor.Equals(ruc.Trim(), StringComparison.Ordinal))
            .ToList();
    }

    public CpeHistorialItemDto? ObtenerHistorial(string idONombre, string? ruc = null)
    {
        if (Guid.TryParse(idONombre, out var id))
        {
            var entity = _db.ComprobantesEmitidos.AsNoTracking()
                .FirstOrDefault(c => c.Id == id);
            if (entity != null &&
                (string.IsNullOrWhiteSpace(ruc) || entity.Ruc == ruc.Trim()))
            {
                return MapToHistorialItem(entity);
            }
        }

        if (!NombreHistorialValido(idONombre))
            return null;

        var desdeJson = ListarHistorialJsonLegacy()
            .FirstOrDefault(x =>
                x.IdHistorial.Equals(Path.GetFileNameWithoutExtension(idONombre), StringComparison.OrdinalIgnoreCase)
                || $"{x.IdHistorial}.json".Equals(idONombre, StringComparison.OrdinalIgnoreCase));

        if (desdeJson != null &&
            (string.IsNullOrWhiteSpace(ruc) || desdeJson.RucEmisor == ruc.Trim()))
        {
            return desdeJson;
        }

        return null;
    }

    public List<CpeHistorialItemDto> BuscarHistorial(
        string? comprobante,
        string? estado,
        string? cliente,
        DateTime? desde,
        DateTime? hasta,
        int take,
        string? ruc = null)
    {
        if (take <= 0) take = 50;
        if (take > 200) take = 200;

        var historial = ListarHistorial(ruc).AsQueryable();

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
            historial = historial.Where(x => x.FechaProceso.Date >= desde.Value.Date);

        if (hasta.HasValue)
            historial = historial.Where(x => x.FechaProceso.Date <= hasta.Value.Date);

        return historial
            .OrderByDescending(x => x.FechaProceso)
            .Take(take)
            .ToList();
    }

    public CpeHistorialResumenResponse ObtenerResumen(string? ruc = null)
    {
        var historial = ListarHistorial(ruc);

        return new CpeHistorialResumenResponse
        {
            TotalRegistros = historial.Count,
            TotalOk = historial.Count(x => x.Errores.Count == 0),
            TotalErrores = historial.Count(x => x.Errores.Count > 0),
            MontoTotal = historial.Sum(x => x.Total),
            PrimeraEmision = historial.Count > 0 ? historial.Min(x => x.FechaProceso) : null,
            UltimaEmision = historial.Count > 0 ? historial.Max(x => x.FechaProceso) : null,
            PorEstado = historial.GroupBy(x => x.Estado).ToDictionary(g => g.Key, g => g.Count()),
            PorTipoComprobante = historial.GroupBy(x => x.TipoComprobante)
                .ToDictionary(g => g.Key, g => g.Count()),
            MontoPorMoneda = historial.GroupBy(x => x.Moneda)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Total))
        };
    }

    private void GuardarJsonLegacy(CpeHistorialItemDto item, string modo, string ruc)
    {
        var carpeta = Path.Combine(
            _settings.RutaArchivos,
            modo.ToUpperInvariant(),
            ruc,
            "HISTORIAL");

        Directory.CreateDirectory(carpeta);

        var rutaArchivo = Path.Combine(carpeta, $"{item.IdHistorial}.json");
        File.WriteAllText(rutaArchivo, JsonSerializer.Serialize(item, JsonOptions));
    }

    private List<CpeHistorialItemDto> ListarHistorialJsonLegacy()
    {
        var resultado = new List<CpeHistorialItemDto>();
        if (string.IsNullOrWhiteSpace(_settings.RutaArchivos) || !Directory.Exists(_settings.RutaArchivos))
            return resultado;

        foreach (var archivo in Directory.EnumerateFiles(
                     _settings.RutaArchivos,
                     "*.json",
                     SearchOption.AllDirectories)
                     .Where(p => p.Contains($"{Path.DirectorySeparatorChar}HISTORIAL{Path.DirectorySeparatorChar}",
                         StringComparison.OrdinalIgnoreCase))
                     .OrderByDescending(File.GetLastWriteTime)
                     .Take(500))
        {
            try
            {
                var item = JsonSerializer.Deserialize<CpeHistorialItemDto>(
                    File.ReadAllText(archivo),
                    JsonOptions);
                if (item != null)
                    resultado.Add(item);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo leer historial JSON {Archivo}", archivo);
            }
        }

        return resultado;
    }

    private static void ActualizarComprobante(
        ComprobanteEmitido entity,
        EmitirCpeRequest request,
        CpeEmisionResponse response,
        string? correlationId)
    {
        entity.Estado = response.Estado;
        entity.Mensaje = response.Mensaje;
        entity.Hash = response.Hash;
        entity.NombreXml = response.NombreXml;
        entity.NombreZip = response.NombreZip;
        entity.NombreCdr = response.NombreCdr;
        entity.Total = request.Total;
        entity.Moneda = request.Moneda;
        entity.SistemaOrigen = request.SistemaOrigen ?? entity.SistemaOrigen;
        entity.CorrelationId = correlationId ?? entity.CorrelationId;
        entity.FechaProceso = DateTime.UtcNow;
        entity.EtapasJson = JsonSerializer.Serialize(response.Etapas);
        entity.ErroresJson = JsonSerializer.Serialize(response.Errores);
    }

    private static CdrRegistro CrearCdr(Guid comprobanteId, CpeEmisionResponse response) => new()
    {
        Id = Guid.NewGuid(),
        ComprobanteEmitidoId = comprobanteId,
        CodigoSunat = response.Ok ? "0" : null,
        MensajeSunat = response.Mensaje,
        NombreArchivo = response.NombreCdr,
        RecibidoEn = DateTime.UtcNow
    };

    /// <summary>
    /// Npgsql exige DateTime UTC para columnas timestamp with time zone.
    /// </summary>
    private static DateTime ToUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime()
        };

    private static CpeHistorialItemDto CrearItemDto(
        EmitirCpeRequest request,
        CpeEmisionResponse response,
        string idHistorial,
        string ruc) => new()
    {
        IdHistorial = idHistorial,
        Comprobante = CpeNombreHelper.ObtenerNumeroComprobante(request),
        Estado = response.Estado,
        Mensaje = response.Mensaje,
        Hash = response.Hash,
        NombreXml = response.NombreXml,
        NombreZip = response.NombreZip,
        NombreCdr = response.NombreCdr,
        TipoComprobante = request.TipoComprobante,
        Serie = request.Serie,
        Correlativo = request.Correlativo,
        RucEmisor = ruc,
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

    private static CpeHistorialItemDto MapToHistorialItem(ComprobanteEmitido c) => new()
    {
        IdHistorial = c.Id.ToString(),
        Comprobante = $"{c.Ruc}-{c.TipoComprobante}-{c.Serie}-{c.Correlativo}",
        Estado = c.Estado,
        Mensaje = c.Mensaje,
        Hash = c.Hash,
        NombreXml = c.NombreXml,
        NombreZip = c.NombreZip,
        NombreCdr = c.NombreCdr,
        TipoComprobante = c.TipoComprobante,
        Serie = c.Serie,
        Correlativo = c.Correlativo,
        RucEmisor = c.Ruc,
        RazonSocialEmisor = c.RazonSocialEmisor ?? string.Empty,
        TipoDocumentoCliente = c.TipoDocumentoCliente ?? string.Empty,
        NumeroDocumentoCliente = c.NumeroDocumentoCliente ?? string.Empty,
        RazonSocialCliente = c.RazonSocialCliente ?? string.Empty,
        Total = c.Total,
        Moneda = c.Moneda,
        FechaEmision = c.FechaEmision,
        FechaProceso = c.FechaProceso,
        Etapas = DeserializeList<CpeEtapaResponse>(c.EtapasJson),
        Errores = DeserializeList<string>(c.ErroresJson)
    };

    private static CpeEmisionResponse MapToEmisionResponse(ComprobanteEmitido c) => new()
    {
        Ok = c.Estado is "ACEPTADO" or "SIMULADO" or "YA_EMITIDO",
        Estado = string.IsNullOrWhiteSpace(c.Estado) ? "YA_EMITIDO" : c.Estado,
        Mensaje = string.IsNullOrWhiteSpace(c.Mensaje)
            ? "Comprobante ya emitido previamente (idempotencia)."
            : c.Mensaje,
        Comprobante = $"{c.Ruc}-{c.TipoComprobante}-{c.Serie}-{c.Correlativo}",
        Hash = c.Hash,
        NombreXml = c.NombreXml,
        NombreZip = c.NombreZip,
        NombreCdr = c.NombreCdr,
        XmlFirmado = !string.IsNullOrWhiteSpace(c.NombreXml),
        FechaProceso = c.FechaProceso,
        Etapas = DeserializeList<CpeEtapaResponse>(c.EtapasJson),
        Errores = DeserializeList<string>(c.ErroresJson)
    };

    private static List<T> DeserializeList<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<T>();

        try
        {
            return JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? new List<T>();
        }
        catch
        {
            return new List<T>();
        }
    }

    private static bool NombreHistorialValido(string nombreArchivo)
    {
        if (string.IsNullOrWhiteSpace(nombreArchivo))
            return false;
        if (nombreArchivo.Contains("..") || nombreArchivo.Contains('/') || nombreArchivo.Contains('\\'))
            return false;
        return true;
    }
}
