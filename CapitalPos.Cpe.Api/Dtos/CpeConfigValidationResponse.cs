namespace CapitalPos.Cpe.Api.Dtos;

public class CpeConfigValidationResponse
{
    public bool Ok { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public List<string> Errores { get; set; } = new();
}