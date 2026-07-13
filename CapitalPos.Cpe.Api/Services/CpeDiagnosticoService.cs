using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Settings;
using Microsoft.Extensions.Options;
using System.Security.Cryptography.X509Certificates;

namespace CapitalPos.Cpe.Api.Services;

public class CpeDiagnosticoService
{
    private readonly CpeSettings _settings;
    private readonly CpeSecuritySettings _securitySettings;

    public CpeDiagnosticoService(
        IOptions<CpeSettings> options,
        IOptions<CpeSecuritySettings> securityOptions)
    {
        _settings = options.Value;
        _securitySettings = securityOptions.Value;
    }

    public CpeDiagnosticoResponse Revisar()
    {
        var checks = new List<CpeDiagnosticoItemResponse>();

        AgregarCheck(
            checks,
            "Modo CPE",
            ModoValido(),
            ModoValido()
                ? $"Modo actual: {NormalizarModo()}."
                : "El modo CPE debe ser BETA o PRODUCCION."
        );

        AgregarCheck(
            checks,
            "Ruta de archivos",
            !string.IsNullOrWhiteSpace(_settings.RutaArchivos),
            string.IsNullOrWhiteSpace(_settings.RutaArchivos)
                ? "La ruta de archivos no está configurada."
                : "La ruta de archivos está configurada."
        );

        var rutaArchivosPreparada = RutaArchivosExisteOPuedePrepararse();

        AgregarCheck(
            checks,
            "Ruta de archivos preparada",
            rutaArchivosPreparada,
            rutaArchivosPreparada
                ? "La ruta de archivos existe o puede prepararse con la configuración actual."
                : "La ruta de archivos no existe y no se pudo confirmar que pueda prepararse."
        );

        RevisarCarpetaModo(checks, "XML");
        RevisarCarpetaModo(checks, "ZIP");
        RevisarCarpetaModo(checks, "CDR");

        AgregarCheck(
            checks,
            "SimularFirma",
            !_settings.SimularFirma,
            _settings.SimularFirma
                ? "Advertencia: la firma está simulada; no hay firma real para SUNAT beta."
                : "Firma real activada."
        );

        AgregarCheck(
            checks,
            "Generación XML",
            !_settings.SimularGeneracionXml,
            _settings.SimularGeneracionXml
                ? "Advertencia: generación XML simulada; no es adecuada para SUNAT beta real."
                : "Generación XML real activada."
        );

        var certificadoConfigurado = !string.IsNullOrWhiteSpace(_settings.RutaCertificado);
        var certificadoExiste = certificadoConfigurado && File.Exists(_settings.RutaCertificado);

        AgregarCheck(
            checks,
            "RutaCertificado configurada",
            certificadoConfigurado,
            certificadoConfigurado
                ? "Ruta de certificado configurada."
                : "Falta configurar RutaCertificado."
        );

        AgregarCheck(
            checks,
            "Archivo certificado existe",
            certificadoExiste,
            certificadoExiste
                ? "Archivo de certificado encontrado."
                : "No se encontró el archivo de certificado configurado."
        );

        AgregarCheck(
            checks,
            "Password certificado",
            !string.IsNullOrWhiteSpace(_settings.PasswordCertificado),
            !string.IsNullOrWhiteSpace(_settings.PasswordCertificado)
                ? "Password de certificado configurado."
                : "Falta configurar PasswordCertificado."
        );

        RevisarLecturaCertificado(checks);

        AgregarCheck(
            checks,
            "SimularEnvioSunat",
            !_settings.SimularEnvioSunat,
            _settings.SimularEnvioSunat
                ? "Advertencia: el envío SUNAT está simulado; no se enviará a SUNAT."
                : "Envío SUNAT real activado."
        );

        AgregarCheck(
            checks,
            "Usuario SOL",
            !string.IsNullOrWhiteSpace(_settings.UsuarioSol),
            !string.IsNullOrWhiteSpace(_settings.UsuarioSol)
                ? $"Usuario SOL configurado ({Enmascarar(_settings.UsuarioSol)})."
                : "Falta configurar UsuarioSol."
        );

        AgregarCheck(
            checks,
            "Clave SOL",
            !string.IsNullOrWhiteSpace(_settings.ClaveSol),
            !string.IsNullOrWhiteSpace(_settings.ClaveSol)
                ? "Clave SOL configurada."
                : "Falta configurar ClaveSol."
        );

        var urlSunat = _settings.ObtenerUrlSunat();

        AgregarCheck(
            checks,
            "URL SUNAT",
            !string.IsNullOrWhiteSpace(urlSunat),
            string.IsNullOrWhiteSpace(urlSunat)
                ? "La URL de SUNAT no está configurada."
                : $"URL SUNAT configurada para {NormalizarModo()}."
        );

        AgregarCheck(
            checks,
            "CDR simulado",
            true,
            _settings.GuardarCdrSimulado
                ? "El CDR simulado se guardará en disco."
                : "El CDR simulado no se guardará por configuración."
        );

        AgregarCheck(
            checks,
            "API key CPE",
            !_securitySettings.ApiKeyEnabled || !string.IsNullOrWhiteSpace(_securitySettings.ApiKey),
            !_securitySettings.ApiKeyEnabled
                ? "Advertencia: API key CPE deshabilitada; solo aceptable en entorno controlado."
                : !string.IsNullOrWhiteSpace(_securitySettings.ApiKey)
                    ? $"API key CPE configurada en header {_securitySettings.HeaderName}."
                    : $"Falta configurar API key CPE para el header {_securitySettings.HeaderName}."
        );

        return new CpeDiagnosticoResponse
        {
            Ok = checks.All(c => c.Ok),
            Modo = NormalizarModo(),
            FechaRevision = DateTime.Now,
            Checks = checks
        };
    }

    private bool ModoValido()
    {
        var modo = NormalizarModo();
        return modo == "BETA" || modo == "PRODUCCION";
    }

    private string NormalizarModo()
    {
        return (_settings.Modo ?? string.Empty).Trim().ToUpperInvariant();
    }

    private bool RutaArchivosExisteOPuedePrepararse()
    {
        if (string.IsNullOrWhiteSpace(_settings.RutaArchivos))
            return false;

        if (Directory.Exists(_settings.RutaArchivos))
            return true;

        try
        {
            var rutaCompleta = Path.GetFullPath(_settings.RutaArchivos);
            var directorioPadre = Directory.GetParent(rutaCompleta);

            return directorioPadre != null && Directory.Exists(directorioPadre.FullName);
        }
        catch
        {
            return false;
        }
    }

    private void RevisarCarpetaModo(List<CpeDiagnosticoItemResponse> checks, string nombreCarpeta)
    {
        if (string.IsNullOrWhiteSpace(_settings.RutaArchivos))
        {
            AgregarCheck(
                checks,
                $"Carpeta {nombreCarpeta}",
                false,
                "No se puede revisar porque RutaArchivos está vacía."
            );

            return;
        }

        var ruta = Path.Combine(_settings.RutaArchivos, _settings.Modo, nombreCarpeta);

        AgregarCheck(
            checks,
            $"Carpeta {nombreCarpeta}",
            Directory.Exists(ruta),
            Directory.Exists(ruta)
                ? $"La carpeta {nombreCarpeta} existe."
                : $"La carpeta {nombreCarpeta} aún no existe; puede prepararse con el endpoint de preparación de carpetas."
        );
    }

    private void RevisarLecturaCertificado(List<CpeDiagnosticoItemResponse> checks)
    {
        if (string.IsNullOrWhiteSpace(_settings.RutaCertificado))
        {
            AgregarCheck(
                checks,
                "Lectura certificado digital",
                false,
                "No se puede leer el certificado porque RutaCertificado está vacío."
            );

            return;
        }

        if (!File.Exists(_settings.RutaCertificado))
        {
            AgregarCheck(
                checks,
                "Lectura certificado digital",
                false,
                "No se encontró el certificado configurado."
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(_settings.PasswordCertificado))
        {
            AgregarCheck(
                checks,
                "Lectura certificado digital",
                false,
                "No se puede leer el certificado porque falta PasswordCertificado."
            );

            return;
        }

        try
        {
            var certificado = new X509Certificate2(
                _settings.RutaCertificado,
                _settings.PasswordCertificado,
                X509KeyStorageFlags.Exportable
            );

            if (!certificado.HasPrivateKey)
            {
                AgregarCheck(
                    checks,
                    "Lectura certificado digital",
                    false,
                    "El certificado se pudo leer, pero no contiene clave privada."
                );

                return;
            }

            if (certificado.NotAfter < DateTime.Now)
            {
                AgregarCheck(
                    checks,
                    "Lectura certificado digital",
                    false,
                    $"El certificado está vencido. Fecha de vencimiento: {certificado.NotAfter:yyyy-MM-dd HH:mm:ss}"
                );

                return;
            }

            AgregarCheck(
                checks,
                "Lectura certificado digital",
                true,
                $"Certificado válido. Vence: {certificado.NotAfter:yyyy-MM-dd HH:mm:ss}"
            );
        }
        catch (Exception ex)
        {
            AgregarCheck(
                checks,
                "Lectura certificado digital",
                false,
                $"No se pudo leer el certificado digital. Error seguro: {TipoErrorSeguro(ex)}."
            );
        }
    }

    private static void AgregarCheck(
        List<CpeDiagnosticoItemResponse> checks,
        string nombre,
        bool ok,
        string mensaje)
    {
        checks.Add(new CpeDiagnosticoItemResponse
        {
            Nombre = nombre,
            Ok = ok,
            Mensaje = mensaje
        });
    }

    private static string Enmascarar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return "no configurado";

        var limpio = valor.Trim();

        if (limpio.Length <= 4)
            return "****";

        return $"{limpio[..2]}***{limpio[^2..]}";
    }

    private static string TipoErrorSeguro(Exception ex)
    {
        return ex.GetType().Name;
    }

    public CpeDiagnosticoResponse PrepararCarpetas()
    {
        var checks = new List<CpeDiagnosticoItemResponse>();

        if (string.IsNullOrWhiteSpace(_settings.RutaArchivos))
        {
            AgregarCheck(
                checks,
                "Ruta de archivos",
                false,
                "No se puede preparar carpetas porque RutaArchivos está vacía."
            );

            return new CpeDiagnosticoResponse
            {
                Ok = false,
                Modo = _settings.Modo,
                FechaRevision = DateTime.Now,
                Checks = checks
            };
        }

        CrearCarpetaSiNoExiste(checks, _settings.RutaArchivos, "Ruta base");

        var rutaModo = Path.Combine(_settings.RutaArchivos, _settings.Modo);
        CrearCarpetaSiNoExiste(checks, rutaModo, $"Carpeta modo {_settings.Modo}");

        CrearCarpetaSiNoExiste(checks, Path.Combine(rutaModo, "XML"), "Carpeta XML");
        CrearCarpetaSiNoExiste(checks, Path.Combine(rutaModo, "ZIP"), "Carpeta ZIP");
        CrearCarpetaSiNoExiste(checks, Path.Combine(rutaModo, "CDR"), "Carpeta CDR");

        return new CpeDiagnosticoResponse
        {
            Ok = checks.All(c => c.Ok),
            Modo = _settings.Modo,
            FechaRevision = DateTime.Now,
            Checks = checks
        };
    }

    private static void CrearCarpetaSiNoExiste(
        List<CpeDiagnosticoItemResponse> checks,
        string ruta,
        string nombre)
    {
        try
        {
            if (!Directory.Exists(ruta))
            {
                Directory.CreateDirectory(ruta);

                AgregarCheck(
                    checks,
                    nombre,
                    true,
                    "Carpeta creada correctamente."
                );

                return;
            }

            AgregarCheck(
            checks,
            nombre,
            true,
            "La carpeta ya existe."
        );
    }
    catch (Exception ex)
    {
        AgregarCheck(
            checks,
            nombre,
            false,
            $"No se pudo crear la carpeta. Error seguro: {TipoErrorSeguro(ex)}."
        );
    }
}
}
