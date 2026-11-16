namespace CapitalPos.Cpe.Api.Data.Entities;

public class CdrRegistro
{
    public Guid Id { get; set; }
    public Guid ComprobanteEmitidoId { get; set; }

    public string? CodigoSunat { get; set; }
    public string? MensajeSunat { get; set; }
    public string? ObservacionesJson { get; set; }
    public string? NombreArchivo { get; set; }

    public DateTime RecibidoEn { get; set; } = DateTime.UtcNow;

    public ComprobanteEmitido ComprobanteEmitido { get; set; } = null!;
}
