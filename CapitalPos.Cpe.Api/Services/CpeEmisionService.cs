using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Helpers;
using CapitalPos.Cpe.Api.Settings;
using Microsoft.Extensions.Options;
using CapitalPos.Cpe.Api.Domain;
using CapitalPos.Cpe.Api.Interfaces;
using CapitalPos.Cpe.Api.Mappers;

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
    private readonly ILogger<CpeEmisionService> _logger;
    private readonly CpeSettings _settings;


    public CpeEmisionService(CpeValidacionService validacionService, ICpeStorageService storageService,
        ICpeXmlService xmlService,
        ICpeXmlValidatorService xmlValidatorService,
        ICpeHashService hashService,
        ICpeHistorialService historialService,
        ICpeFirmaService firmaService,
        ICpeSunatService sunatService,
        ILogger<CpeEmisionService> logger,
        IOptions<CpeSettings> options)
    {
        _validacionService = validacionService;
        _storageService = storageService;
        _xmlService = xmlService;
        _xmlValidatorService = xmlValidatorService;
        _hashService = hashService;
        _historialService = historialService;
        _firmaService = firmaService;
        _sunatService = sunatService;
        _logger = logger;
        _settings = options.Value;
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

        //var rucEmisor = CpeNombreHelper.ObtenerRucEmisor(request);
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

            _storageService.GuardarXml(nombreXml, xmlGenerado);

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

    public CpeEmisionResponse EmitirComprobanteSimulado(EmitirCpeRequest request)
    {
        var etapas = new List<CpeEtapaResponse>();

        try
        {
            var rucEmisor = CpeNombreHelper.ObtenerRucEmisor(request);
            var comprobante = CpeNombreHelper.ObtenerNumeroComprobante(request);

            _logger.LogInformation(
                "Iniciando emisión simulada de CPE. Comprobante: {Comprobante}, RUC: {Ruc}",
                comprobante,
                rucEmisor);

            AgregarEtapa(
                etapas,
                CpeEtapas.Inicio,
                true,
                $"Inicio de emisión del comprobante {comprobante}."
            );

            var validacion = ValidarComprobante(request);

            if (!validacion.Ok)
            {
                AgregarEtapa(
                    etapas,
                    CpeEtapas.Validacion,
                    false,
                    "El comprobante tiene errores de validación."
                );

                _logger.LogWarning("No se pudo emitir CPE por errores de validación. Errores: {Errores}",
                    string.Join(" | ", validacion.Errores));

                return CrearError(
                    CpeEstados.ErrorValidacion,
                    "No se puede emitir el comprobante porque tiene errores de validación.",
                    validacion.Errores,
                    etapas
                );
            }

            AgregarEtapa(
                etapas,
                CpeEtapas.Validacion,
                true,
                "Comprobante validado correctamente."
            );

            string nombreXml;

            try
            {
                nombreXml = GenerarYGuardarXml(request);

                AgregarEtapa(
                    etapas,
                    CpeEtapas.Xml,
                    true,
                    $"XML generado correctamente: {nombreXml}."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo generar el XML.");

                AgregarEtapa(
                    etapas,
                    CpeEtapas.Xml,
                    false,
                    "No se pudo generar el XML."
                );

                return CrearError(
                    CpeEstados.ErrorXml,
                    "No se pudo generar el XML del comprobante.",
                    new List<string> { ex.Message },
                    etapas
                );
            }

            var hashXml = CalcularHashXml(nombreXml);

            var firmaResponse = FirmarXml(nombreXml);

            if (!firmaResponse.Ok)
            {
                AgregarEtapa(
                    etapas,
                    CpeEtapas.Firma,
                    false,
                    "No se pudo firmar el XML."
                );

                return CrearError(
                    CpeEstados.ErrorFirma,
                    "No se pudo firmar el XML.",
                    firmaResponse.Errores,
                    etapas
                );
            }

            AgregarEtapa(
                etapas,
                CpeEtapas.Firma,
                true,
                $"XML firmado correctamente: {firmaResponse.NombreXml}."
            );

            var nombreZip = CrearZipDesdeXmlFirmado(firmaResponse.NombreXml!);

            AgregarEtapa(
                etapas,
                CpeEtapas.Zip,
                true,
                $"ZIP generado correctamente: {nombreZip}."
            );

            var respuestaSunat = EnviarSunat(nombreZip);

            AgregarEtapa(
                etapas,
                CpeEtapas.Sunat,
                respuestaSunat.Ok,
                respuestaSunat.MensajeSunat
            );

            GuardarCdrSiCorresponde(respuestaSunat);

            if (respuestaSunat.Ok && !string.IsNullOrWhiteSpace(respuestaSunat.NombreCdr) &&
                _settings.GuardarCdrSimulado)
            {
                AgregarEtapa(
                    etapas,
                    CpeEtapas.Cdr,
                    true,
                    $"CDR guardado correctamente: {respuestaSunat.NombreCdr}."
                );
            }

            if (respuestaSunat.Ok &&
                !string.IsNullOrWhiteSpace(respuestaSunat.NombreCdr) &&
                !_settings.GuardarCdrSimulado)
            {
                AgregarEtapa(
                    etapas,
                    CpeEtapas.Cdr,
                    true,
                    "CDR simulado no guardado por configuración."
                );
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

            GuardarHistorialSeguro(request, response);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno al emitir CPE.");

            AgregarEtapa(
                etapas,
                CpeEtapas.ErrorInterno,
                false,
                ex.Message
            );

            return CrearError(
                CpeEstados.ErrorInterno,
                "Ocurrió un error al emitir el comprobante.",
                new List<string> { ex.Message },
                etapas
            );
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

    private string GenerarYGuardarXml(EmitirCpeRequest request)
    {
        var documento = CpeDocumentoMapper.FromRequest(request);

        var nombreXml = CpeNombreHelper.ObtenerNombreXml(request);
        var xmlGenerado = _xmlService.GenerarXml(documento);

        var validacionXml = _xmlValidatorService.ValidarXml(xmlGenerado);

        if (!validacionXml.Ok)
        {
            _logger.LogWarning(
                "XML generado con errores. Archivo: {NombreXml}. Errores: {Errores}",
                nombreXml,
                string.Join(" | ", validacionXml.Errores)
            );

            throw new InvalidOperationException(
                $"El XML generado no es válido: {string.Join(" | ", validacionXml.Errores)}"
            );
        }

        _storageService.GuardarXml(nombreXml, xmlGenerado);

        _logger.LogInformation("XML generado y guardado correctamente: {NombreXml}", nombreXml);

        return nombreXml;
    }

    private CpeFirmaResponse FirmarXml(string nombreXml)
    {
        var firmaResponse = _firmaService.FirmarXml(nombreXml);

        if (firmaResponse.Ok)
        {
            _logger.LogInformation("XML firmado correctamente: {NombreXml}", firmaResponse.NombreXml);
        }
        else
        {
            _logger.LogWarning("No se pudo firmar el XML: {NombreXml}. Errores: {Errores}",
                nombreXml,
                string.Join(" | ", firmaResponse.Errores));
        }

        return firmaResponse;
    }

    private string CrearZipDesdeXmlFirmado(string nombreXmlFirmado)
    {
        var rutaZip = _storageService.GuardarZipDesdeXml(nombreXmlFirmado);
        var nombreZip = Path.GetFileName(rutaZip);

        _logger.LogInformation("ZIP generado correctamente: {NombreZip}", nombreZip);

        return nombreZip;
    }

    private CpeSunatResponse EnviarSunat(string nombreZip)
    {
        var respuestaSunat = _sunatService.EnviarComprobante(nombreZip);

        _logger.LogInformation("Respuesta SUNAT. Estado: {Estado}, Mensaje: {Mensaje}",
            respuestaSunat.Estado,
            respuestaSunat.MensajeSunat);

        return respuestaSunat;
    }

    private void GuardarCdrSiCorresponde(CpeSunatResponse respuestaSunat)
    {
        if (!respuestaSunat.Ok || string.IsNullOrWhiteSpace(respuestaSunat.NombreCdr))
            return;

        if (!_settings.GuardarCdrSimulado)
        {
            _logger.LogInformation(
                "No se guardó CDR simulado porque GuardarCdrSimulado está desactivado."
            );

            return;
        }

        var contenidoCdrSimulado = $"""
                                    CDR SIMULADO
                                    Estado: {respuestaSunat.Estado}
                                    Codigo SUNAT: {respuestaSunat.CodigoSunat}
                                    Mensaje SUNAT: {respuestaSunat.MensajeSunat}
                                    Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
                                    """;

        _storageService.GuardarCdr(respuestaSunat.NombreCdr, contenidoCdrSimulado);

        _logger.LogInformation("CDR guardado correctamente: {NombreCdr}", respuestaSunat.NombreCdr);
    }

    private static void AgregarEtapa(
        List<CpeEtapaResponse> etapas,
        string etapa,
        bool ok,
        string mensaje)
    {
        etapas.Add(new CpeEtapaResponse
        {
            Etapa = etapa,
            Ok = ok,
            Mensaje = mensaje,
            Fecha = DateTime.Now
        });
    }

    private string CalcularHashXml(string nombreXml)
    {
        var contenidoXml = _storageService.LeerXml(nombreXml);

        if (string.IsNullOrWhiteSpace(contenidoXml))
        {
            _logger.LogWarning("No se pudo calcular hash porque el XML no existe o está vacío: {NombreXml}", nombreXml);
            return string.Empty;
        }

        var hash = _hashService.CalcularHashSha256(contenidoXml);

        _logger.LogInformation("Hash SHA256 calculado correctamente para XML {NombreXml}: {Hash}", nombreXml, hash);

        return hash;
    }

    private void GuardarHistorialSeguro(
        EmitirCpeRequest request,
        CpeEmisionResponse response)
    {
        try
        {
            var nombreHistorial = _historialService.GuardarHistorial(request, response);

            _logger.LogInformation(
                "Historial de emisión registrado correctamente: {NombreHistorial}",
                nombreHistorial
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "No se pudo guardar el historial de emisión del comprobante {Comprobante}.",
                response.Comprobante
            );
        }
    }
}