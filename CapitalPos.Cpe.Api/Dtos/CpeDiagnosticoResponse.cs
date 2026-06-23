namespace CapitalPos.Cpe.Api.Dtos;

public class CpeDiagnosticoResponse
{
    public bool Ok { get; set; }
    public string Modo { get; set; } = string.Empty;
    public DateTime FechaRevision { get; set; } = DateTime.Now;

    public List<CpeDiagnosticoItemResponse> Checks { get; set; } = new();
}

public class CpeDiagnosticoItemResponse
{
    public string Nombre { get; set; } = string.Empty;
    public bool Ok { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}