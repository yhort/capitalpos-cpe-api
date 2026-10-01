namespace CapitalPos.Cpe.Api.Dtos;

public class EmitirCpeRequest
{
    public string RucEmisor { get; set; } = string.Empty;

    public EmisorDto Emisor { get; set; } = new();
    public string TipoComprobante { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public int Correlativo { get; set; }

    public DateTime FechaEmision { get; set; } = DateTime.Now;
    public string Moneda { get; set; } = "PEN";
    public string TipoOperacion { get; set; } = "0101";
    public string? Observacion { get; set; }

    public string FormaPago { get; set; } = "CONTADO";
    public decimal MontoPendientePago { get; set; }

    public List<CuotaPagoDto> Cuotas { get; set; } = new();
    public ClienteDto Cliente { get; set; } = new();
    public List<ItemCpeDto> Items { get; set; } = new();

    public decimal TotalGravada { get; set; }
    public decimal TotalExonerada { get; set; }
    public decimal TotalInafecta { get; set; }
    public decimal TotalIgv { get; set; }
    public decimal Total { get; set; }

    public string MontoEnLetras { get; set; } = string.Empty;

    /// <summary>
    /// Catálogo 09 SUNAT. Obligatorio cuando <see cref="TipoComprobante"/> es 07.
    /// </summary>
    public string? CodigoMotivo { get; set; }

    /// <summary>
    /// Sustento textual del motivo de la nota de crédito. Obligatorio para tipo 07.
    /// </summary>
    public string? DescripcionMotivo { get; set; }

    /// <summary>
    /// Documento modificado (boleta/factura). Obligatorio cuando <see cref="TipoComprobante"/> es 07.
    /// </summary>
    public DocumentoReferenciaDto? DocumentoReferencia { get; set; }
}

public class DocumentoReferenciaDto
{
    /// <summary>Tipo del comprobante afectado (Catálogo 01): 01 Factura o 03 Boleta.</summary>
    public string TipoComprobante { get; set; } = string.Empty;

    /// <summary>Serie-correlativo del documento afectado, p. ej. B001-41458.</summary>
    public string SerieCorrelativo { get; set; } = string.Empty;
}

public class ClienteDto
{
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
}

public class CuotaPagoDto
{
    public int Numero { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public decimal Monto { get; set; }
}

public class EmisorDto
{
    public string Ruc { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string NombreComercial { get; set; } = string.Empty;

    public string Ubigeo { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public string Provincia { get; set; } = string.Empty;
    public string Distrito { get; set; } = string.Empty;
}

public class ItemCpeDto
{
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;

    public string UnidadMedida { get; set; } = "NIU";
    public decimal Cantidad { get; set; }

    public decimal ValorUnitario { get; set; }
    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal { get; set; }
    public decimal Igv { get; set; }
    public decimal Total { get; set; }
    public string CodigoAfectacionIgv { get; set; } = "10";
}