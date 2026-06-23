using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Interfaces;
using CapitalPos.Cpe.Api.Settings;
using Microsoft.Extensions.Options;
using CapitalPos.Cpe.Api.Helpers;
using CapitalPos.Cpe.Api.Domain;

namespace CapitalPos.Cpe.Api.Infrastructure.Sunat;

public class CpeSunatService : ICpeSunatService
{
    private readonly CpeSettings _settings;
    private readonly ICpeStorageService _storageService;

    public CpeSunatService(IOptions<CpeSettings> options, ICpeStorageService storageService)
    {
        _settings = options.Value;
        _storageService = storageService;
    }

    public CpeSunatResponse EnviarComprobante(string nombreZip)
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

        var rutaZip = _storageService.ObtenerRutaZip(nombreZip);

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
            var urlSunat = _settings.ObtenerUrlSunat();
            var nombreCdr = CpeNombreHelper.ObtenerNombreCdr(nombreZip);

            return new CpeSunatResponse
            {
                Ok = true,
                Estado = CpeEstados.Simulado,
                CodigoSunat = "0",
                MensajeSunat =
                    $"Comprobante aceptado en modo simulación. Modo: {_settings.Modo}. URL SUNAT: {urlSunat}",
                NombreCdr = nombreCdr
            };
        }

        var erroresConfiguracion = ValidarConfiguracionSunat();

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

        return new CpeSunatResponse
        {
            Ok = false,
            Estado = CpeEstados.ErrorSunat,
            CodigoSunat = string.Empty,
            MensajeSunat = "El envío real a SUNAT aún no está implementado.",
            Errores = new List<string>
            {
                "La configuración SUNAT está completa, pero todavía falta implementar el cliente SOAP real."
            }
        };
    }

    private List<string> ValidarConfiguracionSunat()
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(_settings.UsuarioSol))
        {
            errores.Add("El usuario SOL es obligatorio para enviar a SUNAT.");
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
}