namespace CapitalPos.Cpe.Api.Dtos;

public class CpeHistorialItemDto
{
    public string IdHistorial { get; set; } = string.Empty;

    public string Comprobante { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;

    public string? Hash { get; set; }
    public string? NombreXml { get; set; }
    public string? NombreZip { get; set; }
    public string? NombreCdr { get; set; }

    public string TipoComprobante { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public int Correlativo { get; set; }

    public string RucEmisor { get; set; } = string.Empty;
    public string RazonSocialEmisor { get; set; } = string.Empty;

    public string TipoDocumentoCliente { get; set; } = string.Empty;
    public string NumeroDocumentoCliente { get; set; } = string.Empty;
    public string RazonSocialCliente { get; set; } = string.Empty;

    public decimal Total { get; set; }
    public string Moneda { get; set; } = string.Empty;

    public DateTime FechaEmision { get; set; }
    public DateTime FechaProceso { get; set; }

    public List<CpeEtapaResponse> Etapas { get; set; } = new();
    public List<string> Errores { get; set; } = new();
}