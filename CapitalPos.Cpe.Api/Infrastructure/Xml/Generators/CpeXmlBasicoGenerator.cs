using System.Xml.Linq;
using CapitalPos.Cpe.Api.Domain.Cpe;

namespace CapitalPos.Cpe.Api.Infrastructure.Xml.Generators;

public class CpeXmlBasicoGenerator : ICpeXmlGenerator
{
    public string Generar(CpeDocumento documento)
    {
        var xml = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement("Comprobante",
                new XElement("Numero", documento.NumeroComprobante),
                new XElement("Emisor",
                    new XElement("Ruc", documento.Emisor.Ruc),
                    new XElement("RazonSocial", documento.Emisor.RazonSocial),
                    new XElement("NombreComercial", documento.Emisor.NombreComercial),
                    new XElement("Ubigeo", documento.Emisor.Ubigeo),
                    new XElement("Direccion", documento.Emisor.Direccion),
                    new XElement("Departamento", documento.Emisor.Departamento),
                    new XElement("Provincia", documento.Emisor.Provincia),
                    new XElement("Distrito", documento.Emisor.Distrito)
                ),
                new XElement("Documento",
                    new XElement("Tipo", documento.TipoComprobante),
                    new XElement("Serie", documento.Serie),
                    new XElement("Correlativo", documento.Correlativo),
                    new XElement("FechaEmision", documento.FechaEmision.ToString("yyyy-MM-dd")),
                    new XElement("Moneda", documento.Moneda),
                    new XElement("TipoOperacion", documento.TipoOperacion),
                    new XElement("Observacion", documento.Observacion ?? string.Empty),
                    new XElement("FormaPago", documento.FormaPago),
                    new XElement("MontoPendientePago", documento.MontoPendientePago),
                    new XElement("Cuotas",
                        documento.Cuotas.Select(cuota =>
                            new XElement("Cuota",
                                new XElement("Numero", cuota.Numero),
                                new XElement("FechaVencimiento", cuota.FechaVencimiento.ToString("yyyy-MM-dd")),
                                new XElement("Monto", cuota.Monto)
                            )
                        )
                    )
                ),
                new XElement("Cliente",
                    new XElement("TipoDocumento", documento.Cliente.TipoDocumento),
                    new XElement("NumeroDocumento", documento.Cliente.NumeroDocumento),
                    new XElement("RazonSocial", documento.Cliente.RazonSocial)
                ),
                new XElement("Items",
                    documento.Items.Select(item =>
                        new XElement("Item",
                            new XElement("Codigo", item.Codigo),
                            new XElement("Descripcion", item.Descripcion),
                            new XElement("UnidadMedida", item.UnidadMedida),
                            new XElement("Cantidad", item.Cantidad),
                            new XElement("ValorUnitario", item.ValorUnitario),
                            new XElement("PrecioUnitario", item.PrecioUnitario),
                            new XElement("Subtotal", item.Subtotal),
                            new XElement("Igv", item.Igv),
                            new XElement("Total", item.Total),
                            new XElement("CodigoAfectacionIgv", item.CodigoAfectacionIgv)
                        )
                    )
                ),
                new XElement("Totales",
                    new XElement("TotalGravada", documento.TotalGravada),
                    new XElement("TotalExonerada", documento.TotalExonerada),
                    new XElement("TotalInafecta", documento.TotalInafecta),
                    new XElement("TotalIgv", documento.TotalIgv),
                    new XElement("Total", documento.Total),
                    new XElement("MontoEnLetras", documento.MontoEnLetras)
                )
            )
        );

        return xml.ToString();
    }
}