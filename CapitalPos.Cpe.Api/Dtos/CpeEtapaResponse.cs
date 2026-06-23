namespace CapitalPos.Cpe.Api.Dtos;

public class CpeEtapaResponse
{
    public string Etapa { get; set; } = string.Empty;
    public bool Ok { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.Now;
}