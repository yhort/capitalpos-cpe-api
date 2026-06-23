namespace CapitalPos.Cpe.Api.Dtos;

public class CpeConfigResponse
{
    public string Modo { get; set; } = string.Empty;
    public string RutaArchivos { get; set; } = string.Empty;
    public string UrlSunat { get; set; } = string.Empty;

    public bool SimularGeneracionXml { get; set; }
    public bool SimularFirma { get; set; }
    public bool SimularEnvioSunat { get; set; }
    public bool GuardarCdrSimulado { get; set; }
    public bool TieneCertificado { get; set; }
    public bool TienePasswordCertificado { get; set; }
    public bool TieneUsuarioSol { get; set; }
    public bool TieneClaveSol { get; set; }
}