namespace CapitalPos.Cpe.Api.Dtos;

public class HealthResponse
{
    public string Status { get; set; } = "OK";
    public string Service { get; set; } = "CapitalPOS CPE API";
    public string Version { get; set; } = "1.0.0";
    public string Modo { get; set; } = string.Empty;
    
    public bool SimularGeneracionXml { get; set; }
    public bool SimularFirma { get; set; }
    public bool SimularEnvioSunat { get; set; }
    public bool GuardarCdrSimulado { get; set; }
    public string RutaArchivos { get; set; } = string.Empty;
    public string RutaArchivosAbsoluta { get; set; } = string.Empty;
    public DateTime FechaServidor { get; set; } = DateTime.Now;
}