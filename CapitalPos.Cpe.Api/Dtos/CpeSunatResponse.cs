namespace CapitalPos.Cpe.Api.Dtos;

public class CpeSunatResponse
{
    public bool Ok { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string CodigoSunat { get; set; } = string.Empty;
    public string MensajeSunat { get; set; } = string.Empty;
    public string? NombreCdr { get; set; }
    public List<string> Errores { get; set; } = new();
}