namespace CapitalPos.Cpe.Api.Dtos;

public class CpeCdrResultadoDto
{
    public bool Ok { get; set; }

    public string Estado { get; set; } = string.Empty;

    public string CodigoSunat { get; set; } = string.Empty;

    public string MensajeSunat { get; set; } = string.Empty;

    public string NombreCdr { get; set; } = string.Empty;

    public List<string> Observaciones { get; set; } = new();

    public List<string> Errores { get; set; } = new();
}