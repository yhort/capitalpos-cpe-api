using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Settings;
using Microsoft.Extensions.Options;
using System.Security.Cryptography.X509Certificates;

namespace CapitalPos.Cpe.Api.Services;

public class CpeDiagnosticoService
{
    private readonly CpeSettings _settings;

    public CpeDiagnosticoService(IOptions<CpeSettings> options)
    {
        _settings = options.Value;
    }

    public CpeDiagnosticoResponse Revisar()
    {
        var checks = new List<CpeDiagnosticoItemResponse>();

        AgregarCheck(
            checks,
            "Modo CPE",
            _settings.Modo.ToUpper() == "BETA" || _settings.Modo.ToUpper() == "PRODUCCION",
            $"Modo actual: {_settings.Modo}"
        );

        AgregarCheck(
            checks,
            "Ruta de archivos",
            !string.IsNullOrWhiteSpace(_settings.RutaArchivos),
            string.IsNullOrWhiteSpace(_settings.RutaArchivos)
                ? "La ruta de archivos no está configurada."
                : $"Ruta configurada: {_settings.RutaArchivos}"
        );

        AgregarCheck(
            checks,
            "Existe ruta de archivos",
            !string.IsNullOrWhiteSpace(_settings.RutaArchivos) && Directory.Exists(_settings.RutaArchivos),
            Directory.Exists(_settings.RutaArchivos)
                ? "La ruta de archivos existe."
                : "La ruta de archivos no existe."
        );

        RevisarCarpetaModo(checks, "XML");
        RevisarCarpetaModo(checks, "ZIP");
        RevisarCarpetaModo(checks, "CDR");

        AgregarCheck(
            checks,
            "Firma digital",
            _settings.SimularFirma || ExisteCertificadoConfigurado(),
            _settings.SimularFirma
                ? "Firma digital en modo simulación."
                : "Firma digital real activada. Se requiere certificado configurado."
        );

        AgregarCheck(
            checks,
            "Generación XML",
            _settings.SimularGeneracionXml,
            _settings.SimularGeneracionXml
                ? "Generación XML en modo simulación."
                : "Generación XML real activada. Aún falta implementar UBL real."
        );

        AgregarCheck(
            checks,
            "Certificado digital",
            _settings.SimularFirma || ExisteCertificadoConfigurado(),
            _settings.SimularFirma
                ? "No se exige certificado porque la firma está simulada."
                : ExisteCertificadoConfigurado()
                    ? "Certificado digital encontrado."
                    : "No se encontró certificado digital."
        );

        AgregarCheck(
            checks,
            "Password certificado",
            _settings.SimularFirma || !string.IsNullOrWhiteSpace(_settings.PasswordCertificado),
            _settings.SimularFirma
                ? "No se exige password porque la firma está simulada."
                : !string.IsNullOrWhiteSpace(_settings.PasswordCertificado)
                    ? "Password de certificado configurado."
                    : "Falta password del certificado."
        );

        RevisarLecturaCertificado(checks);

        AgregarCheck(
            checks,
            "Envío SUNAT",
            _settings.SimularEnvioSunat || TieneCredencialesSol(),
            _settings.SimularEnvioSunat
                ? "Envío SUNAT en modo simulación."
                : "Envío SUNAT real activado. Se requieren credenciales SOL."
        );

        AgregarCheck(
            checks,
            "Usuario SOL",
            _settings.SimularEnvioSunat || !string.IsNullOrWhiteSpace(_settings.UsuarioSol),
            _settings.SimularEnvioSunat
                ? "No se exige usuario SOL porque el envío SUNAT está simulado."
                : !string.IsNullOrWhiteSpace(_settings.UsuarioSol)
                    ? "Usuario SOL configurado."
                    : "Falta usuario SOL."
        );

        AgregarCheck(
            checks,
            "Clave SOL",
            _settings.SimularEnvioSunat || !string.IsNullOrWhiteSpace(_settings.ClaveSol),
            _settings.SimularEnvioSunat
                ? "No se exige clave SOL porque el envío SUNAT está simulado."
                : !string.IsNullOrWhiteSpace(_settings.ClaveSol)
                    ? "Clave SOL configurada."
                    : "Falta clave SOL."
        );

        AgregarCheck(
            checks,
            "URL SUNAT",
            !string.IsNullOrWhiteSpace(_settings.ObtenerUrlSunat()),
            string.IsNullOrWhiteSpace(_settings.ObtenerUrlSunat())
                ? "La URL de SUNAT no está configurada."
                : $"URL SUNAT: {_settings.ObtenerUrlSunat()}"
        );

        AgregarCheck(
            checks,
            "CDR simulado",
            true,
            _settings.GuardarCdrSimulado
                ? "El CDR simulado se guardará en disco."
                : "El CDR simulado no se guardará por configuración."
        );

        return new CpeDiagnosticoResponse
        {
            Ok = checks.All(c => c.Ok),
            Modo = _settings.Modo,
            FechaRevision = DateTime.Now,
            Checks = checks
        };
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
                ? $"La carpeta existe: {ruta}"
                : $"La carpeta aún no existe: {ruta}"
        );
    }

    private void RevisarLecturaCertificado(List<CpeDiagnosticoItemResponse> checks)
    {
        if (_settings.SimularFirma)
        {
            AgregarCheck(
                checks,
                "Lectura certificado digital",
                true,
                "No se valida lectura del certificado porque la firma está simulada."
            );

            return;
        }

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
                $"No se encontró el certificado en la ruta: {_settings.RutaCertificado}"
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
                $"No se pudo leer el certificado digital. Error: {ex.Message}"
            );
        }
    }

    private bool ExisteCertificadoConfigurado()
    {
        return !string.IsNullOrWhiteSpace(_settings.RutaCertificado)
               && File.Exists(_settings.RutaCertificado);
    }

    private bool TieneCredencialesSol()
    {
        return !string.IsNullOrWhiteSpace(_settings.UsuarioSol)
               && !string.IsNullOrWhiteSpace(_settings.ClaveSol);
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
                    $"Carpeta creada correctamente: {ruta}"
                );

                return;
            }

            AgregarCheck(
                checks,
                nombre,
                true,
                $"La carpeta ya existe: {ruta}"
            );
        }
        catch (Exception ex)
        {
            AgregarCheck(
                checks,
                nombre,
                false,
                $"No se pudo crear la carpeta {ruta}. Error: {ex.Message}"
            );
        }
    }
}