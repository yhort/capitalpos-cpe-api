namespace CapitalPos.Cpe.Api.Settings;

public class CpeSettings
{
    public string Modo { get; set; } = "BETA";
    public string RutaArchivos { get; set; } = string.Empty;

    public bool SimularFirma { get; set; } = true;
    public bool SimularEnvioSunat { get; set; } = true;
    
    public bool SimularGeneracionXml { get; set; } = true;
    public bool GuardarCdrSimulado { get; set; } = true;
    public string RutaCertificado { get; set; } = string.Empty;
    public string PasswordCertificado { get; set; } = string.Empty;

    public string UsuarioSol { get; set; } = string.Empty;
    public string ClaveSol { get; set; } = string.Empty;

    public string UrlSunatBeta { get; set; } = string.Empty;
    public string UrlSunatProduccion { get; set; } = string.Empty;

    public string ObtenerUrlSunat()
    {
        return Modo.ToUpper() == "PRODUCCION"
            ? UrlSunatProduccion
            : UrlSunatBeta;
    }
}