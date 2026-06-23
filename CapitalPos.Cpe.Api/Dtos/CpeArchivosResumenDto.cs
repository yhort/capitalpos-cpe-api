namespace CapitalPos.Cpe.Api.Dtos;

public class CpeArchivosResumenDto
{
    public CpeArchivosGrupoResumenDto Xml { get; set; } = new();
    public CpeArchivosGrupoResumenDto Zip { get; set; } = new();
    public CpeArchivosGrupoResumenDto Cdr { get; set; } = new();

    public int TotalArchivos => Xml.Cantidad + Zip.Cantidad + Cdr.Cantidad;
    public decimal TamanoTotalKb => Xml.TamanoTotalKb + Zip.TamanoTotalKb + Cdr.TamanoTotalKb;

    public DateTime FechaConsulta { get; set; } = DateTime.Now;
}

public class CpeArchivosGrupoResumenDto
{
    public int Cantidad { get; set; }
    public long TamanoTotalBytes { get; set; }
    public decimal TamanoTotalKb { get; set; }
    public string? UltimoArchivo { get; set; }
    public DateTime? FechaUltimoArchivo { get; set; }
}