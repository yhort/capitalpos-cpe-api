namespace CapitalPos.Cpe.Api.Settings;

public class CpeSettings
{
    public string Modo { get; set; } = "BETA";
    public string RutaArchivos { get; set; } = string.Empty;

    public bool SimularFirma { get; set; } = true;
    public bool SimularEnvioSunat { get; set; } = true;

    public bool SimularGeneracionXml { get; set; } = true;
    public bool GuardarCdrSimulado { get; set; } = true;

    /// <summary>
    /// Si true y el RUC no está en BD, intenta usar RutaCertificado/UsuarioSol globales (migración).
    /// Desactivar en cuanto los emisores estén onboarded.
    /// </summary>
    public bool PermitirCredencialesLegacy { get; set; } = true;

    /// <summary>Legacy single-tenant. Preferir certificados por emisor en BD.</summary>
    public string RutaCertificado { get; set; } = string.Empty;

    /// <summary>Legacy single-tenant.</summary>
    public string PasswordCertificado { get; set; } = string.Empty;

    /// <summary>Legacy single-tenant.</summary>
    public string UsuarioSol { get; set; } = string.Empty;

    /// <summary>Legacy single-tenant.</summary>
    public string ClaveSol { get; set; } = string.Empty;

    public string UrlSunatBeta { get; set; } = string.Empty;
    public string UrlSunatProduccion { get; set; } = string.Empty;

    /// <summary>Carpeta persistente del key ring de Data Protection (multi-instancia).</summary>
    public string? DataProtectionKeysPath { get; set; }

    public string ObtenerUrlSunat()
    {
        return Modo.ToUpper() == "PRODUCCION"
            ? UrlSunatProduccion
            : UrlSunatBeta;
    }
}