namespace CapitalPos.Cpe.Api.Dtos;

public class CpeFirmaResponse
{
    public bool Ok { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string? NombreXml { get; set; }
    public bool XmlFirmado { get; set; }
    public List<string> Errores { get; set; } = new();
}