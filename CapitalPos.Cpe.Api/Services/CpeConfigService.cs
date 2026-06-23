using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Settings;
using Microsoft.Extensions.Options;

namespace CapitalPos.Cpe.Api.Services;

public class CpeConfigService
{
    private readonly CpeSettings _settings;

    public CpeConfigService(IOptions<CpeSettings> options)
    {
        _settings = options.Value;
    }

    public CpeConfigResponse ObtenerConfiguracion()
    {
        return new CpeConfigResponse
        {
            Modo = _settings.Modo,
            RutaArchivos = _settings.RutaArchivos,
            UrlSunat = _settings.ObtenerUrlSunat(),

            SimularGeneracionXml = _settings.SimularGeneracionXml,
            SimularFirma = _settings.SimularFirma,
            SimularEnvioSunat = _settings.SimularEnvioSunat,
            GuardarCdrSimulado = _settings.GuardarCdrSimulado,

            TieneCertificado = !string.IsNullOrWhiteSpace(_settings.RutaCertificado),
            TienePasswordCertificado = !string.IsNullOrWhiteSpace(_settings.PasswordCertificado),
            TieneUsuarioSol = !string.IsNullOrWhiteSpace(_settings.UsuarioSol),
            TieneClaveSol = !string.IsNullOrWhiteSpace(_settings.ClaveSol)
        };
    }

    public CpeConfigValidationResponse ValidarConfiguracion()
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(_settings.Modo))
            errores.Add("El modo CPE es obligatorio.");

        if (_settings.Modo.ToUpper() != "BETA" && _settings.Modo.ToUpper() != "PRODUCCION")
            errores.Add("El modo CPE debe ser BETA o PRODUCCION.");

        if (string.IsNullOrWhiteSpace(_settings.RutaArchivos))
            errores.Add("La ruta de archivos CPE es obligatoria.");

        if (!string.IsNullOrWhiteSpace(_settings.RutaArchivos) &&
            !Directory.Exists(_settings.RutaArchivos))
        {
            errores.Add("La ruta de archivos CPE no existe.");
        }

        if (string.IsNullOrWhiteSpace(_settings.ObtenerUrlSunat()))
            errores.Add("La URL de SUNAT es obligatoria.");

        var requiereConfiguracionReal = !_settings.SimularFirma || !_settings.SimularEnvioSunat;

        if (requiereConfiguracionReal)
        {
            if (string.IsNullOrWhiteSpace(_settings.RutaCertificado))
            {
                errores.Add("La ruta del certificado digital es obligatoria.");
            }
            else if (!File.Exists(_settings.RutaCertificado))
            {
                errores.Add("No se encontró el certificado digital.");
            }

            if (string.IsNullOrWhiteSpace(_settings.PasswordCertificado))
                errores.Add("La contraseña del certificado es obligatoria.");

            if (string.IsNullOrWhiteSpace(_settings.UsuarioSol))
                errores.Add("El usuario SOL es obligatorio.");

            if (string.IsNullOrWhiteSpace(_settings.ClaveSol))
                errores.Add("La clave SOL es obligatoria.");
        }

        if (errores.Count > 0)
        {
            return new CpeConfigValidationResponse
            {
                Ok = false,
                Mensaje = "La configuración CPE tiene errores.",
                Errores = errores
            };
        }

        return new CpeConfigValidationResponse
        {
            Ok = true,
            Mensaje = "La configuración CPE es válida."
        };
    }
}