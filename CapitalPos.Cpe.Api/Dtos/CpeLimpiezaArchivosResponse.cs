namespace CapitalPos.Cpe.Api.Dtos;

public class CpeLimpiezaArchivosResponse
{
    public bool Ejecutado { get; set; }
    public int DiasAntiguedad { get; set; }
    public DateTime FechaLimite { get; set; }

    public int TotalArchivosEncontrados { get; set; }
    public int TotalArchivosEliminados { get; set; }

    public List<CpeArchivoLimpiezaDto> Archivos { get; set; } = new();
}

public class CpeArchivoLimpiezaDto
{
    public string Tipo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Ruta { get; set; } = string.Empty;
    public DateTime FechaModificacion { get; set; }
    public bool Eliminado { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}