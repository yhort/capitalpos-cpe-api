namespace CapitalPos.Cpe.Api.Dtos;

public class CpeArchivoInfoDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public decimal TamanoKb { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaModificacion { get; set; }
}