namespace CapitalPos.Cpe.Api.Dtos;

public class CpeXmlResumenResponse
{
    public bool Ok { get; set; }
    public string Mensaje { get; set; } = string.Empty;

    public string TipoXml { get; set; } = string.Empty;

    public string NumeroComprobante { get; set; } = string.Empty;
    public string TipoComprobante { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string Correlativo { get; set; } = string.Empty;

    public string FechaEmision { get; set; } = string.Empty;
    public string Moneda { get; set; } = string.Empty;

    public string RucEmisor { get; set; } = string.Empty;
    public string RazonSocialEmisor { get; set; } = string.Empty;

    public string TipoDocumentoCliente { get; set; } = string.Empty;
    public string NumeroDocumentoCliente { get; set; } = string.Empty;
    public string RazonSocialCliente { get; set; } = string.Empty;

    public decimal TotalGravada { get; set; }
    public decimal TotalExonerada { get; set; }
    public decimal TotalInafecta { get; set; }
    public decimal TotalIgv { get; set; }
    public decimal Total { get; set; }

    public int CantidadItems { get; set; }

    public List<string> Errores { get; set; } = new();
}