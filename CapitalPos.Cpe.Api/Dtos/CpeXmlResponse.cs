namespace CapitalPos.Cpe.Api.Dtos;

public class CpeXmlResponse
{
    public bool Ok { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string? Comprobante { get; set; }
    public string? NombreXml { get; set; }
    public List<string> Errores { get; set; } = new();
}