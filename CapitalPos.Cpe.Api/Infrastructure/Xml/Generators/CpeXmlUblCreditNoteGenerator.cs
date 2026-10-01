using System.Globalization;
using System.Xml.Linq;
using CapitalPos.Cpe.Api.Domain;
using CapitalPos.Cpe.Api.Domain.Cpe;

namespace CapitalPos.Cpe.Api.Infrastructure.Xml.Generators;

/// <summary>
/// Genera UBL 2.1 CreditNote (tipo comprobante 07) alineado al legacy CPE_NC.
/// </summary>
public class CpeXmlUblCreditNoteGenerator : ICpeXmlGenerator
{
    public string Generar(CpeDocumento documento)
    {
        ArgumentNullException.ThrowIfNull(documento);

        if (documento.DocumentoReferencia is null)
        {
            throw new InvalidOperationException(
                "La nota de crédito requiere DocumentoReferencia para generar el XML UBL.");
        }

        if (string.IsNullOrWhiteSpace(documento.CodigoMotivo))
        {
            throw new InvalidOperationException(
                "La nota de crédito requiere CodigoMotivo (Catálogo 09) para generar el XML UBL.");
        }

        if (string.IsNullOrWhiteSpace(documento.DescripcionMotivo))
        {
            throw new InvalidOperationException(
                "La nota de crédito requiere DescripcionMotivo para generar el XML UBL.");
        }

        var ns = UblNamespaces.CreditNote;
        var cac = UblNamespaces.Cac;
        var cbc = UblNamespaces.Cbc;
        var ext = UblNamespaces.Ext;
        var sac = UblNamespaces.Sac;
        var ds = UblNamespaces.Ds;

        var serieCorrelativo = $"{documento.Serie}-{documento.Correlativo}";

        var xml = new XDocument(
            new XDeclaration("1.0", "utf-8", "no"),
            new XElement(ns + "CreditNote",
                new XAttribute(XNamespace.Xmlns + "cac", cac),
                new XAttribute(XNamespace.Xmlns + "cbc", cbc),
                new XAttribute(XNamespace.Xmlns + "ext", ext),
                new XAttribute(XNamespace.Xmlns + "sac", sac),
                new XAttribute(XNamespace.Xmlns + "ds", ds),
                CrearExtensiones(ext),
                new XElement(cbc + "UBLVersionID", "2.1"),
                new XElement(cbc + "CustomizationID",
                    new XAttribute("schemeAgencyName", "PE:SUNAT"),
                    "2.0"
                ),
                new XElement(cbc + "ID", serieCorrelativo),
                new XElement(cbc + "IssueDate", documento.FechaEmision.ToString("yyyy-MM-dd")),
                new XElement(cbc + "IssueTime", documento.FechaEmision.ToString("HH:mm:ss")),
                CrearDocumentCurrencyCode(documento, cbc),
                CrearDiscrepancyResponse(documento, cac, cbc),
                CrearBillingReference(documento, cac, cbc),
                CrearEmisor(documento, cac, cbc),
                CrearCliente(documento, cac, cbc),
                CrearImpuestos(documento, cac, cbc),
                CrearTotales(documento, cac, cbc),
                CrearItems(documento, cac, cbc)
            )
        );

        return xml.ToString();
    }

    private static XElement CrearExtensiones(XNamespace ext)
    {
        return new XElement(ext + "UBLExtensions",
            new XElement(ext + "UBLExtension",
                new XElement(ext + "ExtensionContent")
            )
        );
    }

    private static XElement CrearDocumentCurrencyCode(CpeDocumento documento, XNamespace cbc)
    {
        return new XElement(cbc + "DocumentCurrencyCode",
            new XAttribute("listID", "ISO 4217 Alpha"),
            new XAttribute("listName", "Currency"),
            new XAttribute("listAgencyName", "United Nations Economic Commission for Europe"),
            documento.Moneda
        );
    }

    private static XElement CrearDiscrepancyResponse(
        CpeDocumento documento,
        XNamespace cac,
        XNamespace cbc)
    {
        var referencia = documento.DocumentoReferencia!;

        return new XElement(cac + "DiscrepancyResponse",
            new XElement(cbc + "ReferenceID", referencia.SerieCorrelativo),
            new XElement(cbc + "ResponseCode",
                new XAttribute("listAgencyName", "PE:SUNAT"),
                new XAttribute("listName", "Tipo de nota de credito"),
                new XAttribute("listURI", "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo09"),
                documento.CodigoMotivo
            ),
            new XElement(cbc + "Description", documento.DescripcionMotivo)
        );
    }

    private static XElement CrearBillingReference(
        CpeDocumento documento,
        XNamespace cac,
        XNamespace cbc)
    {
        var referencia = documento.DocumentoReferencia!;

        return new XElement(cac + "BillingReference",
            new XElement(cac + "InvoiceDocumentReference",
                new XElement(cbc + "ID", referencia.SerieCorrelativo),
                new XElement(cbc + "DocumentTypeCode",
                    new XAttribute("listAgencyName", "PE:SUNAT"),
                    new XAttribute("listName", "Tipo de Documento"),
                    new XAttribute("listURI", "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo01"),
                    referencia.TipoComprobante
                )
            )
        );
    }

    private static XElement CrearEmisor(CpeDocumento documento, XNamespace cac, XNamespace cbc)
    {
        return new XElement(cac + "AccountingSupplierParty",
            new XElement(cac + "Party",
                new XElement(cac + "PartyIdentification",
                    new XElement(cbc + "ID",
                        new XAttribute("schemeID", "6"),
                        new XAttribute("schemeName", "Documento de Identidad"),
                        new XAttribute("schemeAgencyName", "PE:SUNAT"),
                        new XAttribute("schemeURI", "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06"),
                        documento.Emisor.Ruc
                    )
                ),
                new XElement(cac + "PartyName",
                    new XElement(cbc + "Name", documento.Emisor.NombreComercial)
                ),
                new XElement(cac + "PartyLegalEntity",
                    new XElement(cbc + "RegistrationName", documento.Emisor.RazonSocial),
                    new XElement(cac + "RegistrationAddress",
                        new XElement(cbc + "ID", documento.Emisor.Ubigeo),
                        new XElement(cbc + "AddressTypeCode",
                            new XAttribute("listAgencyName", "PE:SUNAT"),
                            new XAttribute("listName", "Establecimientos anexos"),
                            "0000"
                        ),
                        new XElement(cbc + "CityName", documento.Emisor.Provincia),
                        new XElement(cbc + "CountrySubentity", documento.Emisor.Departamento),
                        new XElement(cbc + "District", documento.Emisor.Distrito),
                        new XElement(cac + "AddressLine",
                            new XElement(cbc + "Line", documento.Emisor.Direccion)
                        ),
                        new XElement(cac + "Country",
                            new XElement(cbc + "IdentificationCode", "PE")
                        )
                    )
                )
            )
        );
    }

    private static XElement CrearCliente(CpeDocumento documento, XNamespace cac, XNamespace cbc)
    {
        return new XElement(cac + "AccountingCustomerParty",
            new XElement(cac + "Party",
                new XElement(cac + "PartyIdentification",
                    new XElement(cbc + "ID",
                        new XAttribute("schemeID", documento.Cliente.TipoDocumento),
                        new XAttribute("schemeName", "Documento de Identidad"),
                        new XAttribute("schemeAgencyName", "PE:SUNAT"),
                        new XAttribute("schemeURI", "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06"),
                        documento.Cliente.NumeroDocumento
                    )
                ),
                new XElement(cac + "PartyLegalEntity",
                    new XElement(cbc + "RegistrationName", documento.Cliente.RazonSocial)
                )
            )
        );
    }

    private static XElement CrearImpuestos(CpeDocumento documento, XNamespace cac, XNamespace cbc)
    {
        var taxTotal = new XElement(cac + "TaxTotal",
            new XElement(cbc + "TaxAmount",
                new XAttribute("currencyID", documento.Moneda),
                FormatoMonto(documento.TotalIgv)
            )
        );

        if (documento.TotalGravada > 0)
        {
            taxTotal.Add(
                CrearTaxSubtotal(
                    cac,
                    cbc,
                    documento.Moneda,
                    documento.TotalGravada,
                    documento.TotalIgv,
                    "1000",
                    "IGV",
                    "VAT"
                )
            );
        }

        if (documento.TotalExonerada > 0)
        {
            taxTotal.Add(
                CrearTaxSubtotal(
                    cac,
                    cbc,
                    documento.Moneda,
                    documento.TotalExonerada,
                    0,
                    "9997",
                    "EXO",
                    "VAT"
                )
            );
        }

        if (documento.TotalInafecta > 0)
        {
            taxTotal.Add(
                CrearTaxSubtotal(
                    cac,
                    cbc,
                    documento.Moneda,
                    documento.TotalInafecta,
                    0,
                    "9998",
                    "INA",
                    "FRE"
                )
            );
        }

        return taxTotal;
    }

    private static XElement CrearTaxSubtotal(
        XNamespace cac,
        XNamespace cbc,
        string moneda,
        decimal taxableAmount,
        decimal taxAmount,
        string taxSchemeId,
        string taxSchemeName,
        string taxTypeCode)
    {
        return new XElement(cac + "TaxSubtotal",
            new XElement(cbc + "TaxableAmount",
                new XAttribute("currencyID", moneda),
                FormatoMonto(taxableAmount)
            ),
            new XElement(cbc + "TaxAmount",
                new XAttribute("currencyID", moneda),
                FormatoMonto(taxAmount)
            ),
            new XElement(cac + "TaxCategory",
                CrearTaxScheme(cac, cbc, taxSchemeId, taxSchemeName, taxTypeCode)
            )
        );
    }

    private static XElement CrearTotales(CpeDocumento documento, XNamespace cac, XNamespace cbc)
    {
        return new XElement(cac + "LegalMonetaryTotal",
            new XElement(cbc + "LineExtensionAmount",
                new XAttribute("currencyID", documento.Moneda),
                FormatoMonto(documento.TotalGravada + documento.TotalExonerada + documento.TotalInafecta)
            ),
            new XElement(cbc + "TaxInclusiveAmount",
                new XAttribute("currencyID", documento.Moneda),
                FormatoMonto(documento.Total)
            ),
            new XElement(cbc + "PayableAmount",
                new XAttribute("currencyID", documento.Moneda),
                FormatoMonto(documento.Total)
            )
        );
    }

    private static IEnumerable<XElement> CrearItems(CpeDocumento documento, XNamespace cac, XNamespace cbc)
    {
        var numeroLinea = 1;

        foreach (var item in documento.Items)
        {
            yield return new XElement(cac + "CreditNoteLine",
                new XElement(cbc + "ID", numeroLinea),
                new XElement(cbc + "CreditedQuantity",
                    new XAttribute("unitCode", item.UnidadMedida),
                    new XAttribute("unitCodeListID", "UN/ECE rec 20"),
                    new XAttribute("unitCodeListAgencyName", "United Nations Economic Commission for Europe"),
                    item.Cantidad
                ),
                new XElement(cbc + "LineExtensionAmount",
                    new XAttribute("currencyID", documento.Moneda),
                    FormatoMonto(item.Subtotal)
                ),
                new XElement(cac + "PricingReference",
                    new XElement(cac + "AlternativeConditionPrice",
                        new XElement(cbc + "PriceAmount",
                            new XAttribute("currencyID", documento.Moneda),
                            FormatoMonto(item.PrecioUnitario)
                        ),
                        new XElement(cbc + "PriceTypeCode",
                            new XAttribute("listName", "Tipo de Precio"),
                            new XAttribute("listAgencyName", "PE:SUNAT"),
                            new XAttribute("listURI", "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo16"),
                            "01"
                        )
                    )
                ),
                new XElement(cac + "TaxTotal",
                    new XElement(cbc + "TaxAmount",
                        new XAttribute("currencyID", documento.Moneda),
                        FormatoMonto(item.Igv)
                    ),
                    new XElement(cac + "TaxSubtotal",
                        new XElement(cbc + "TaxableAmount",
                            new XAttribute("currencyID", documento.Moneda),
                            FormatoMonto(item.Subtotal)
                        ),
                        new XElement(cbc + "TaxAmount",
                            new XAttribute("currencyID", documento.Moneda),
                            FormatoMonto(item.Igv)
                        ),
                        new XElement(cac + "TaxCategory",
                            new XElement(cbc + "Percent", ObtenerPorcentajeIgv(item.CodigoAfectacionIgv)),
                            new XElement(cbc + "TaxExemptionReasonCode",
                                new XAttribute("listAgencyName", "PE:SUNAT"),
                                new XAttribute("listName", "Afectacion del IGV"),
                                new XAttribute("listURI", "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07"),
                                item.CodigoAfectacionIgv
                            ),
                            CrearTaxSchemeItem(cac, cbc, item.CodigoAfectacionIgv)
                        )
                    )
                ),
                new XElement(cac + "Item",
                    new XElement(cbc + "Description", item.Descripcion),
                    new XElement(cac + "SellersItemIdentification",
                        new XElement(cbc + "ID", item.Codigo)
                    )
                ),
                new XElement(cac + "Price",
                    new XElement(cbc + "PriceAmount",
                        new XAttribute("currencyID", documento.Moneda),
                        FormatoMonto(item.ValorUnitario)
                    )
                )
            );

            numeroLinea++;
        }
    }

    private static int ObtenerPorcentajeIgv(string codigoAfectacionIgv)
    {
        return codigoAfectacionIgv == CatalogosSunat.AfectacionIgv.GravadoOperacionOnerosa
            ? 18
            : 0;
    }

    private static XElement CrearTaxSchemeItem(
        XNamespace cac,
        XNamespace cbc,
        string codigoAfectacionIgv)
    {
        if (codigoAfectacionIgv == CatalogosSunat.AfectacionIgv.ExoneradoOperacionOnerosa)
        {
            return CrearTaxScheme(cac, cbc, "9997", "EXO", "VAT");
        }

        if (codigoAfectacionIgv == CatalogosSunat.AfectacionIgv.InafectoOperacionOnerosa)
        {
            return CrearTaxScheme(cac, cbc, "9998", "INA", "FRE");
        }

        return CrearTaxScheme(cac, cbc, "1000", "IGV", "VAT");
    }

    private static XElement CrearTaxScheme(
        XNamespace cac,
        XNamespace cbc,
        string id,
        string nombre,
        string taxTypeCode)
    {
        return new XElement(cac + "TaxScheme",
            new XElement(cbc + "ID",
                new XAttribute("schemeName", "Codigo de tributos"),
                new XAttribute("schemeAgencyName", "PE:SUNAT"),
                new XAttribute("schemeURI", "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo05"),
                id
            ),
            new XElement(cbc + "Name", nombre),
            new XElement(cbc + "TaxTypeCode", taxTypeCode)
        );
    }

    private static string FormatoMonto(decimal monto)
    {
        return monto.ToString("0.00", CultureInfo.InvariantCulture);
    }
}
