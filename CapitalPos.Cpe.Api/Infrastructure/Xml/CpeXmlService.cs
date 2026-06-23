using System.Xml.Linq;
using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Interfaces;
using CapitalPos.Cpe.Api.Helpers;
using CapitalPos.Cpe.Api.Settings;
using Microsoft.Extensions.Options;
using CapitalPos.Cpe.Api.Domain.Cpe;
using CapitalPos.Cpe.Api.Infrastructure.Xml.Generators;

namespace CapitalPos.Cpe.Api.Infrastructure.Xml;

public class CpeXmlService : ICpeXmlService
{
    private readonly CpeSettings _settings;

    public CpeXmlService(IOptions<CpeSettings> options)
    {
        _settings = options.Value;
    }

    public string GenerarXml(CpeDocumento documento)
    {
        ICpeXmlGenerator generator = _settings.SimularGeneracionXml
            ? new CpeXmlBasicoGenerator()
            : new CpeXmlUblGenerator();

        return generator.Generar(documento);
        /*
        if (!_settings.SimularGeneracionXml)
        {
            throw new InvalidOperationException(
                "La generación XML UBL real aún no está implementada. Active SimularGeneracionXml para usar el XML básico de prueba."
            );
        }

        //var comprobante = CpeNombreHelper.ObtenerNumeroComprobante(request);

        var comprobante = documento.NumeroComprobante;

        var xml = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement("Comprobante",
                new XElement("Numero", comprobante),
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
                    new XElement("Total", documento.Total)
                )
            )
        );

        return xml.ToString();
        */
    }
}