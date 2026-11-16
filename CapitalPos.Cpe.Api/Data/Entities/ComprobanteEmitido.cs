namespace CapitalPos.Cpe.Api.Data.Entities;

public class ComprobanteEmitido
{
    public Guid Id { get; set; }
    public Guid EmisorId { get; set; }

    public string Ruc { get; set; } = string.Empty;
    public string TipoComprobante { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public int Correlativo { get; set; }

    public string Estado { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;

    public string? Hash { get; set; }
    public string? NombreXml { get; set; }
    public string? NombreZip { get; set; }
    public string? NombreCdr { get; set; }

    public string? RazonSocialEmisor { get; set; }
    public string? TipoDocumentoCliente { get; set; }
    public string? NumeroDocumentoCliente { get; set; }
    public string? RazonSocialCliente { get; set; }

    public decimal Total { get; set; }
    public string Moneda { get; set; } = "PEN";

    /// <summary>Solo auditoría; no forma parte de la clave de tenant.</summary>
    public string? SistemaOrigen { get; set; }

    public string? CorrelationId { get; set; }

    public DateTime FechaEmision { get; set; }
    public DateTime FechaProceso { get; set; } = DateTime.UtcNow;

    public string? EtapasJson { get; set; }
    public string? ErroresJson { get; set; }

    public Emisor Emisor { get; set; } = null!;
    public ICollection<CdrRegistro> Cdrs { get; set; } = new List<CdrRegistro>();
}
