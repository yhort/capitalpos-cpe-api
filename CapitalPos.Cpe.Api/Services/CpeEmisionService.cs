using System.IO.Compression;
using System.Xml.Linq;
using CapitalPos.Cpe.Api.Domain;
using CapitalPos.Cpe.Api.Domain.Tenancy;
using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Helpers;
using CapitalPos.Cpe.Api.Interfaces;
using CapitalPos.Cpe.Api.Mappers;
using CapitalPos.Cpe.Api.Settings;
using Microsoft.Extensions.Options;

namespace CapitalPos.Cpe.Api.Services;

public class CpeEmisionService
{
    private readonly CpeValidacionService _validacionService;
    private readonly ICpeStorageService _storageService;
    private readonly ICpeXmlService _xmlService;
    private readonly ICpeXmlValidatorService _xmlValidatorService;
    private readonly ICpeHashService _hashService;
    private readonly ICpeHistorialService _historialService;
    private readonly ICpeFirmaService _firmaService;
    private readonly ICpeSunatService _sunatService;
    private readonly IEmisorRuntimeContextResolver _emisorResolver;
    private readonly IEmisorContextAccessor _emisorContextAccessor;
    private readonly ILogger<CpeEmisionService> _logger;
    private readonly CpeSettings _settings;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CpeEmisionService(
        CpeValidacionService validacionService,
        ICpeStorageService storageService,
        ICpeXmlService xmlService,
        ICpeXmlValidatorService xmlValidatorService,
        ICpeHashService hashService,
        ICpeHistorialService historialService,
        ICpeFirmaService firmaService,
        ICpeSunatService sunatService,
        IEmisorRuntimeContextResolver emisorResolver,
        IEmisorContextAccessor emisorContextAccessor,
        ILogger<CpeEmisionService> logger,
        IOptions<CpeSettings> options,
        IHttpContextAccessor httpContextAccessor)
    {
        _validacionService = validacionService;
        _storageService = storageService;
        _xmlService = xmlService;
        _xmlValidatorService = xmlValidatorService;
        _hashService = hashService;
        _historialService = historialService;
        _firmaService = firmaService;
        _sunatService = sunatService;
        _emisorResolver = emisorResolver;
        _emisorContextAccessor = emisorContextAccessor;
        _logger = logger;
        _settings = options.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    public CpeValidacionResponse ValidarComprobante(EmitirCpeRequest request)
    {
        var errores = _validacionService.Validar(request);

        if (errores.Count > 0)
        {
            return new CpeValidacionResponse
            {
                Ok = false,
                Mensaje = "El comprobante tiene errores de validación.",
                Errores = errores
            };
        }

        return new CpeValidacionResponse
        {
            Ok = true,
            Mensaje = "Comprobante válido para generar XML.",
            Comprobante = CpeNombreHelper.ObtenerNumeroComprobante(request)
        };
    }

    public CpeXmlResponse GenerarXml(EmitirCpeRequest request)
    {
        var validacion = ValidarComprobante(request);

        if (!validacion.Ok)
        {
            return new CpeXmlResponse
            {
                Ok = false,
                Mensaje = "No se puede generar el XML porque el comprobante tiene errores.",
                Errores = validacion.Errores
            };
        }

        try
        {
            var ruc = CpeNombreHelper.ObtenerRucEmisor(request);
            var documento = CpeDocumentoMapper.FromRequest(request);
            var comprobante = CpeNombreHelper.ObtenerNumeroComprobante(request);
            var nombreXml = CpeNombreHelper.ObtenerNombreXml(request);

            var xmlGenerado = _xmlService.GenerarXml(documento);
            var validacionXml = _xmlValidatorService.ValidarXml(xmlGenerado);

            if (!validacionXml.Ok)
            {
                return new CpeXmlResponse
                {
                    Ok = false,
                    Mensaje = "El XML generado tiene errores.",
                    Errores = validacionXml.Errores
                };
            }

            _storageService.GuardarXml(nombreXml, xmlGenerado, ruc);

            return new CpeXmlResponse
            {
                Ok = true,
                Mensaje = "XML generado correctamente.",
                Comprobante = comprobante,
                NombreXml = nombreXml
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar XML desde endpoint generar-xml.");

            return new CpeXmlResponse
            {
                Ok = false,
                Mensaje = "No se pudo generar el XML del comprobante.",
                Errores = new List<string> { ex.Message }
            };
        }
    }

    public CpeEmisionResponse EmitirComprobante(EmitirCpeRequest request)
    {
        return EmitirComprobanteAsync(request).GetAwaiter().GetResult();
    }

    public async Task<CpeEmisionResponse> EmitirComprobanteAsync(EmitirCpeRequest request)
    {
        var etapas = new List<CpeEtapaResponse>();
        EmisorRuntimeContext? context = null;

        try
        {
            var rucEmisor = CpeNombreHelper.ObtenerRucEmisor(request);
            var comprobante = CpeNombreHelper.ObtenerNumeroComprobante(request);

            var resolve = await _emisorResolver.ResolveFromRequestAsync(request);
            if (!resolve.Ok || resolve.Context == null)
            {
                AgregarEtapa(etapas, CpeEtapas.Inicio, false, resolve.Mensaje);
                return CrearError(
                    CpeEstados.ErrorEmisor,
                    resolve.Mensaje,
                    resolve.Errores,
                    etapas);
            }

            context = resolve.Context;
            _emisorContextAccessor.Current = context;

            _logger.LogInformation(
                "Iniciando emisión CPE. Comprobante: {Comprobante}, RUC: {Ruc}, Ambiente: {Ambiente}, Legacy: {Legacy}",
                comprobante,
                rucEmisor,
                context.Ambiente,
                context.EsLegacy);

            AgregarEtapa(
                etapas,
                CpeEtapas.Inicio,
                true,
                $"Inicio de emisión del comprobante {comprobante}.");

            var existente = await _historialService.ObtenerEmisionExistenteAsync(
                rucEmisor,
                request.TipoComprobante,
                request.Serie,
                request.Correlativo);

            if (existente != null &&
                (existente.Estado == CpeEstados.Aceptado ||
                 existente.Estado == CpeEstados.Simulado ||
                 existente.Estado == CpeEstados.YaEmitido))
            {
                existente.Estado = CpeEstados.YaEmitido;
                existente.Mensaje =
                    "Comprobante ya emitido previamente. Se devolvió el registro existente (idempotencia).";
                existente.Etapas = etapas;
                AgregarEtapa(etapas, "Idempotencia", true, "Se reutilizó la emisión existente.");
                return existente;
            }

            var validacion = ValidarComprobante(request);

            if (!validacion.Ok)
            {
                AgregarEtapa(etapas, CpeEtapas.Validacion, false, "El comprobante tiene errores de validación.");
                return CrearError(
                    CpeEstados.ErrorValidacion,
                    "No se puede emitir el comprobante porque tiene errores de validación.",
                    validacion.Errores,
                    etapas);
            }

            AgregarEtapa(etapas, CpeEtapas.Validacion, true, "Comprobante validado correctamente.");

            string nombreXml;
            try
            {
                nombreXml = GenerarYGuardarXml(request, rucEmisor);
                AgregarEtapa(etapas, CpeEtapas.Xml, true, $"XML generado correctamente: {nombreXml}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo generar el XML.");
                AgregarEtapa(etapas, CpeEtapas.Xml, false, "No se pudo generar el XML.");
                return CrearError(
                    CpeEstados.ErrorXml,
                    "No se pudo generar el XML del comprobante.",
                    new List<string> { ex.Message },
                    etapas);
            }

            var hashXml = CalcularHashXml(nombreXml, rucEmisor);
            var firmaResponse = _firmaService.FirmarXml(nombreXml, context);

            if (!firmaResponse.Ok)
            {
                AgregarEtapa(etapas, CpeEtapas.Firma, false, "No se pudo firmar el XML.");
                return CrearError(
                    CpeEstados.ErrorFirma,
                    "No se pudo firmar el XML.",
                    firmaResponse.Errores,
                    etapas);
            }

            AgregarEtapa(etapas, CpeEtapas.Firma, true, $"XML firmado correctamente: {firmaResponse.NombreXml}.");

            var nombreZip = CrearZipDesdeXmlFirmado(firmaResponse.NombreXml!, rucEmisor);
            AgregarEtapa(etapas, CpeEtapas.Zip, true, $"ZIP generado correctamente: {nombreZip}.");

            var respuestaSunat = _sunatService.EnviarComprobante(nombreZip, context);
            AgregarEtapa(etapas, CpeEtapas.Sunat, respuestaSunat.Ok, respuestaSunat.MensajeSunat);

            GuardarCdrSiCorresponde(respuestaSunat, rucEmisor);

            if (respuestaSunat.Ok && !string.IsNullOrWhiteSpace(respuestaSunat.NombreCdr) &&
                _settings.GuardarCdrSimulado)
            {
                AgregarEtapa(etapas, CpeEtapas.Cdr, true, $"CDR guardado correctamente: {respuestaSunat.NombreCdr}.");
            }

            if (respuestaSunat.Ok &&
                !string.IsNullOrWhiteSpace(respuestaSunat.NombreCdr) &&
                !_settings.GuardarCdrSimulado)
            {
                AgregarEtapa(etapas, CpeEtapas.Cdr, true, "CDR simulado no guardado por configuración.");
            }

            var response = new CpeEmisionResponse
            {
                Ok = respuestaSunat.Ok,
                Estado = respuestaSunat.Estado,
                Mensaje = respuestaSunat.MensajeSunat,
                Comprobante = comprobante,
                Hash = hashXml,
                NombreXml = firmaResponse.NombreXml,
                NombreZip = nombreZip,
                XmlFirmado = firmaResponse.XmlFirmado,
                NombreCdr = respuestaSunat.NombreCdr,
                FechaProceso = DateTime.Now,
                Etapas = etapas,
                Errores = respuestaSunat.Errores
            };

            await GuardarHistorialSeguroAsync(request, response, context);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno al emitir CPE.");
            AgregarEtapa(etapas, CpeEtapas.ErrorInterno, false, ex.Message);
            return CrearError(
                CpeEstados.ErrorInterno,
                "Ocurrió un error al emitir el comprobante.",
                new List<string> { ex.Message },
                etapas);
        }
        finally
        {
            if (_emisorContextAccessor.Current == context)
                _emisorContextAccessor.Current = null;
        }
    }

    private static CpeEmisionResponse CrearError(
        string estado,
        string mensaje,
        List<string> errores,
        List<CpeEtapaResponse>? etapas = null)
    {
        return new CpeEmisionResponse
        {
            Ok = false,
            Estado = estado,
            Mensaje = mensaje,
            FechaProceso = DateTime.Now,
            Etapas = etapas ?? new List<CpeEtapaResponse>(),
            Errores = errores
        };
    }

    private string GenerarYGuardarXml(EmitirCpeRequest request, string ruc)
    {
        var documento = CpeDocumentoMapper.FromRequest(request);
        var nombreXml = CpeNombreHelper.ObtenerNombreXml(request);
        var xmlGenerado = _xmlService.GenerarXml(documento);
        var validacionXml = _xmlValidatorService.ValidarXml(xmlGenerado);

        if (!validacionXml.Ok)
        {
            throw new InvalidOperationException(
                $"El XML generado no es válido: {string.Join(" | ", validacionXml.Errores)}");
        }

        _storageService.GuardarXml(nombreXml, xmlGenerado, ruc);
        return nombreXml;
    }

    private string CrearZipDesdeXmlFirmado(string nombreXmlFirmado, string ruc)
    {
        var rutaZip = _storageService.GuardarZipDesdeXml(nombreXmlFirmado, ruc);
        return Path.GetFileName(rutaZip);
    }

    private void GuardarCdrSiCorresponde(CpeSunatResponse respuestaSunat, string ruc)
    {
        if (!respuestaSunat.Ok || string.IsNullOrWhiteSpace(respuestaSunat.NombreCdr))
            return;

        if (!_settings.GuardarCdrSimulado)
            return;

        if (!_settings.SimularEnvioSunat)
            return;

        var cdrZipBytes = CrearCdrSimuladoZipBytes(respuestaSunat);
        _storageService.GuardarCdrBytes(respuestaSunat.NombreCdr, cdrZipBytes, ruc);
    }

    private static byte[] CrearCdrSimuladoZipBytes(CpeSunatResponse respuestaSunat)
    {
        var nombreCdr = respuestaSunat.NombreCdr
                        ?? throw new InvalidOperationException("El nombre del CDR es obligatorio.");

        var nombreXmlInterno = Path.ChangeExtension(nombreCdr, ".xml")
                               ?? throw new InvalidOperationException(
                                   "No se pudo generar el nombre XML interno del CDR simulado.");

        using var memoryStream = new MemoryStream();
        using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entrada = zip.CreateEntry(nombreXmlInterno);
            using var entradaStream = entrada.Open();
            CrearXmlCdrSimulado(respuestaSunat).Save(entradaStream);
        }

        return memoryStream.ToArray();
    }

    private static XDocument CrearXmlCdrSimulado(CpeSunatResponse respuestaSunat)
    {
        XNamespace app = "urn:oasis:names:specification:ubl:schema:xsd:ApplicationResponse-2";
        XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
        XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

        return new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(app + "ApplicationResponse",
                new XAttribute(XNamespace.Xmlns + "cbc", cbc),
                new XAttribute(XNamespace.Xmlns + "cac", cac),
                new XElement(cbc + "UBLVersionID", "2.0"),
                new XElement(cbc + "CustomizationID", "1.0"),
                new XElement(cbc + "ID", respuestaSunat.NombreCdr),
                new XElement(cbc + "IssueDate", DateTime.Now.ToString("yyyy-MM-dd")),
                new XElement(cbc + "IssueTime", DateTime.Now.ToString("HH:mm:ss")),
                new XElement(cac + "DocumentResponse",
                    new XElement(cac + "Response",
                        new XElement(cbc + "ResponseCode", respuestaSunat.CodigoSunat),
                        new XElement(cbc + "Description", respuestaSunat.MensajeSunat)
                    )
                )
            )
        );
    }

    private static void AgregarEtapa(List<CpeEtapaResponse> etapas, string etapa, bool ok, string mensaje)
    {
        etapas.Add(new CpeEtapaResponse
        {
            Etapa = etapa,
            Ok = ok,
            Mensaje = mensaje,
            Fecha = DateTime.Now
        });
    }

    private string CalcularHashXml(string nombreXml, string ruc)
    {
        var contenidoXml = _storageService.LeerXml(nombreXml, ruc);
        if (string.IsNullOrWhiteSpace(contenidoXml))
            return string.Empty;

        return _hashService.CalcularHashSha256(contenidoXml);
    }

    private async Task GuardarHistorialSeguroAsync(
        EmitirCpeRequest request,
        CpeEmisionResponse response,
        EmisorRuntimeContext context)
    {
        try
        {
            var correlationId = _httpContextAccessor.HttpContext?.TraceIdentifier
                                ?? _httpContextAccessor.HttpContext?.Request.Headers["X-Correlation-ID"]
                                    .FirstOrDefault();

            var id = await _historialService.GuardarHistorialAsync(
                request,
                response,
                context,
                correlationId);

            _logger.LogInformation("Historial de emisión registrado correctamente: {IdHistorial}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "No se pudo guardar el historial de emisión del comprobante {Comprobante}.",
                response.Comprobante);
        }
    }
}
