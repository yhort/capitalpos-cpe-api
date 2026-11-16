using CapitalPos.Cpe.Api.Domain;
using CapitalPos.Cpe.Api.Domain.Tenancy;
using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Helpers;
using CapitalPos.Cpe.Api.Interfaces;
using CapitalPos.Cpe.Api.Settings;
using Microsoft.Extensions.Options;
using System.Security;
using System.Text;
using System.Xml.Linq;

namespace CapitalPos.Cpe.Api.Infrastructure.Sunat;

public class CpeSunatService : ICpeSunatService
{
    private readonly CpeSettings _settings;
    private readonly ICpeStorageService _storageService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICpeCdrService _cdrService;
    private readonly IEmisorContextAccessor _emisorContextAccessor;

    public CpeSunatService(
        IOptions<CpeSettings> options,
        ICpeStorageService storageService,
        IHttpClientFactory httpClientFactory,
        ICpeCdrService cdrService,
        IEmisorContextAccessor emisorContextAccessor)
    {
        _settings = options.Value;
        _storageService = storageService;
        _httpClientFactory = httpClientFactory;
        _cdrService = cdrService;
        _emisorContextAccessor = emisorContextAccessor;
    }

    public CpeSunatResponse EnviarComprobante(string nombreZip, EmisorRuntimeContext? context = null)
    {
        if (string.IsNullOrWhiteSpace(nombreZip))
        {
            return new CpeSunatResponse
            {
                Ok = false,
                Estado = CpeEstados.ErrorSunat,
                CodigoSunat = string.Empty,
                MensajeSunat = "El nombre del ZIP  es obligatorio.",
                Errores = new List<string>
                {
                    "Debe indicar el nombre del archivo ZIP."
                }
            };
        }

        context ??= _emisorContextAccessor.Current;
        var ruc = context?.Ruc ?? ExtraerRucDesdeNombreZip(nombreZip);

        var rutaZip = _storageService.ObtenerRutaZip(nombreZip, ruc);

        if (string.IsNullOrWhiteSpace(rutaZip))
        {
            return new CpeSunatResponse
            {
                Ok = false,
                Estado = CpeEstados.ErrorSunat,
                CodigoSunat = string.Empty,
                MensajeSunat = "No se encontró el ZIP para enviar a SUNAT.",
                Errores = new List<string>
                {
                    $"No existe el archivo ZIP: {nombreZip}"
                }
            };
        }

        if (_settings.SimularEnvioSunat)
        {
            var urlSunat = context?.UrlSunat ?? _settings.ObtenerUrlSunat();
            var nombreCdr = CpeNombreHelper.ObtenerNombreCdr(nombreZip);
            var modo = context?.Ambiente ?? _settings.Modo;

            return new CpeSunatResponse
            {
                Ok = true,
                Estado = CpeEstados.Simulado,
                CodigoSunat = "0",
                MensajeSunat =
                    $"Comprobante aceptado en modo simulación. Modo: {modo}. URL SUNAT: {urlSunat}",
                NombreCdr = nombreCdr
            };
        }

        string usuarioSol;
        string claveSol;
        string urlEnvio;

        if (context != null)
        {
            usuarioSol = context.UsuarioSol;
            claveSol = context.ClaveSol;
            urlEnvio = context.UrlSunat;
        }
        else if (_settings.PermitirCredencialesLegacy)
        {
            var erroresConfiguracion = ValidarConfiguracionSunatLegacy(ruc);
            if (erroresConfiguracion.Count > 0)
            {
                return new CpeSunatResponse
                {
                    Ok = false,
                    Estado = CpeEstados.ErrorSunat,
                    CodigoSunat = string.Empty,
                    MensajeSunat = "No se puede enviar a SUNAT porque la configuración no está completa.",
                    Errores = erroresConfiguracion
                };
            }

            usuarioSol = _settings.UsuarioSol;
            claveSol = _settings.ClaveSol;
            urlEnvio = _settings.ObtenerUrlSunat();
        }
        else
        {
            return new CpeSunatResponse
            {
                Ok = false,
                Estado = CpeEstados.ErrorSunat,
                CodigoSunat = string.Empty,
                MensajeSunat = "No se puede enviar a SUNAT porque no hay contexto de emisor.",
                Errores = new List<string>
                {
                    "Debe resolver el emisor (RUC) antes de enviar o habilitar credenciales legacy."
                }
            };
        }

        if (string.IsNullOrWhiteSpace(usuarioSol) || string.IsNullOrWhiteSpace(claveSol) ||
            string.IsNullOrWhiteSpace(urlEnvio))
        {
            return new CpeSunatResponse
            {
                Ok = false,
                Estado = CpeEstados.ErrorSunat,
                CodigoSunat = string.Empty,
                MensajeSunat = "No se puede enviar a SUNAT porque la configuración no está completa.",
                Errores = new List<string> { "Usuario SOL, Clave SOL y URL SUNAT son obligatorios." }
            };
        }

        if (!string.IsNullOrWhiteSpace(ruc)
            && !usuarioSol.Trim().StartsWith(ruc.Trim(), StringComparison.Ordinal))
        {
            return new CpeSunatResponse
            {
                Ok = false,
                Estado = CpeEstados.ErrorSunat,
                CodigoSunat = string.Empty,
                MensajeSunat = "No se puede enviar a SUNAT porque la configuración no está completa.",
                Errores = new List<string>
                {
                    $"UsuarioSol no corresponde al RUC del comprobante ({ruc}). " +
                    "El usuario SOL debe iniciar con el mismo RUC del emisor (ej. {RUC}{USUARIO})."
                }
            };
        }

        try
        {
            var soapResponse = EnviarSoapSunatAsync(nombreZip, rutaZip, urlEnvio, usuarioSol, claveSol)
                .GetAwaiter()
                .GetResult();

            var applicationResponseBase64 = ExtraerApplicationResponse(soapResponse);

            var nombreCdr = GuardarCdrReal(nombreZip, applicationResponseBase64, ruc);

            var resultadoCdr = _cdrService.LeerCdr(nombreCdr, ruc);

            return new CpeSunatResponse
            {
                Ok = resultadoCdr.Ok,
                Estado = resultadoCdr.Estado,
                CodigoSunat = resultadoCdr.CodigoSunat,
                MensajeSunat = string.IsNullOrWhiteSpace(resultadoCdr.MensajeSunat)
                    ? "SUNAT devolvió el CDR, pero no se pudo interpretar el mensaje."
                    : resultadoCdr.MensajeSunat,
                NombreCdr = nombreCdr,
                Errores = resultadoCdr.Errores
            };
        }
        catch (Exception ex)
        {
            return new CpeSunatResponse
            {
                Ok = false,
                Estado = CpeEstados.ErrorSunat,
                CodigoSunat = string.Empty,
                MensajeSunat = "No se pudo enviar el comprobante a SUNAT.",
                Errores = new List<string>
                {
                    ex.Message
                }
            };
        }
    }

    private List<string> ValidarConfiguracionSunatLegacy(string? rucEmisor = null)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(_settings.UsuarioSol))
        {
            errores.Add("El usuario SOL es obligatorio para enviar a SUNAT.");
        }
        else if (!string.IsNullOrWhiteSpace(rucEmisor)
                 && !_settings.UsuarioSol.Trim().StartsWith(rucEmisor.Trim(), StringComparison.Ordinal))
        {
            errores.Add(
                $"UsuarioSol no corresponde al RUC del comprobante ({rucEmisor}). " +
                "El usuario SOL debe iniciar con el mismo RUC del emisor (ej. {RUC}{USUARIO}).");
        }

        if (string.IsNullOrWhiteSpace(_settings.ClaveSol))
        {
            errores.Add("La clave SOL es obligatoria para enviar a SUNAT.");
        }

        var urlSunat = _settings.ObtenerUrlSunat();

        if (string.IsNullOrWhiteSpace(urlSunat))
        {
            errores.Add("La URL de SUNAT es obligatoria.");
        }

        return errores;
    }

    private static string? ExtraerRucDesdeNombreZip(string nombreZip)
    {
        var nombre = Path.GetFileNameWithoutExtension(nombreZip.Trim());
        if (nombre.StartsWith("R-", StringComparison.OrdinalIgnoreCase))
        {
            nombre = nombre[2..];
        }

        var partes = nombre.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (partes.Length == 0)
        {
            return null;
        }

        var ruc = partes[0];
        return ruc.Length == 11 && ruc.All(char.IsDigit) ? ruc : null;
    }

    private static string CrearSoapEnvelopeSendBill(
        string nombreZip,
        string rutaZip,
        string usuarioSol,
        string claveSol)
    {
        var zipBytes = File.ReadAllBytes(rutaZip);
        var zipBase64 = Convert.ToBase64String(zipBytes);

        var nombreZipSeguro = SecurityElement.Escape(nombreZip);
        var usuarioSolSeguro = SecurityElement.Escape(usuarioSol);
        var claveSolSeguro = SecurityElement.Escape(claveSol);

        return $"""
                <?xml version="1.0" encoding="UTF-8"?>
                <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                                  xmlns:ser="http://service.sunat.gob.pe"
                                  xmlns:wsse="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd">
                    <soapenv:Header>
                        <wsse:Security>
                            <wsse:UsernameToken>
                                <wsse:Username>{usuarioSolSeguro}</wsse:Username>
                                <wsse:Password>{claveSolSeguro}</wsse:Password>
                            </wsse:UsernameToken>
                        </wsse:Security>
                    </soapenv:Header>
                    <soapenv:Body>
                        <ser:sendBill>
                            <fileName>{nombreZipSeguro}</fileName>
                            <contentFile>{zipBase64}</contentFile>
                        </ser:sendBill>
                    </soapenv:Body>
                </soapenv:Envelope>
                """;
    }

    private async Task<string> EnviarSoapSunatAsync(
        string nombreZip,
        string rutaZip,
        string urlSunat,
        string usuarioSol,
        string claveSol)
    {
        var soapEnvelope = CrearSoapEnvelopeSendBill(
            nombreZip,
            rutaZip,
            usuarioSol,
            claveSol
        );

        var client = _httpClientFactory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, urlSunat);

        request.Content = new StringContent(
            soapEnvelope,
            Encoding.UTF8,
            "text/xml"
        );

        request.Headers.Add("SOAPAction", "");

        using var response = await client.SendAsync(request);

        var contenidoRespuesta = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"SUNAT respondió HTTP {(int)response.StatusCode}. Respuesta: {contenidoRespuesta}"
            );
        }

        return contenidoRespuesta;
    }

    private static string ExtraerApplicationResponse(string soapResponse)
    {
        if (string.IsNullOrWhiteSpace(soapResponse))
        {
            throw new InvalidOperationException("SUNAT devolvió una respuesta vacía.");
        }

        var xml = XDocument.Parse(soapResponse);

        var fault = xml
            .Descendants()
            .FirstOrDefault(e => e.Name.LocalName == "Fault");

        if (fault != null)
        {
            var faultString = fault
                .Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "faultstring")
                ?.Value;

            throw new InvalidOperationException(
                $"SUNAT devolvió un error SOAP: {faultString ?? "Sin detalle"}"
            );
        }

        var applicationResponse = xml
            .Descendants()
            .FirstOrDefault(e => e.Name.LocalName == "applicationResponse")
            ?.Value;

        if (string.IsNullOrWhiteSpace(applicationResponse))
        {
            throw new InvalidOperationException(
                "No se encontró applicationResponse en la respuesta de SUNAT."
            );
        }

        return applicationResponse;
    }

    private string GuardarCdrReal(string nombreZip, string applicationResponseBase64, string? ruc)
    {
        if (string.IsNullOrWhiteSpace(applicationResponseBase64))
        {
            throw new InvalidOperationException(
                "El applicationResponse de SUNAT está vacío."
            );
        }

        byte[] cdrBytes;

        try
        {
            cdrBytes = Convert.FromBase64String(applicationResponseBase64);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                "El applicationResponse de SUNAT no tiene formato Base64 válido.",
                ex
            );
        }

        var nombreCdr = CpeNombreHelper.ObtenerNombreCdr(nombreZip);

        _storageService.GuardarCdrBytes(nombreCdr, cdrBytes, ruc);

        return nombreCdr;
    }
}
