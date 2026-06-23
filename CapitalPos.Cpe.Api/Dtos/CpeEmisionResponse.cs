namespace CapitalPos.Cpe.Api.Dtos;

public class CpeEmisionResponse
{
    public bool Ok { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? Comprobante { get; set; }
    public string? Hash { get; set; }
    public string? NombreXml { get; set; }
    public string? NombreZip { get; set; }
    public bool XmlFirmado { get; set; }
    public string? NombreCdr { get; set; }
    
    public DateTime FechaProceso { get; set; } = DateTime.Now;
    
    public List<CpeEtapaResponse> Etapas { get; set; } = new();
    public List<string> Errores { get; set; } = new();
    
}