using System.Xml.Linq;
using CapitalPos.Cpe.Api.Domain.Cpe;
using CapitalPos.Cpe.Api.Infrastructure.Xml;
using CapitalPos.Cpe.Api.Infrastructure.Xml.Generators;
using Xunit;

namespace CapitalPos.Cpe.Tests;

public class CpeXmlUblCreditNoteGeneratorTests
{
    [Fact]
    public void Generar_NotaCreditoSobreBoleta_UsaRootCreditNoteYOrdenSunat()
    {
        var xml = new CpeXmlUblCreditNoteGenerator().Generar(CrearNotaCreditoSobreBoleta());
        var document = XDocument.Parse(xml);
        var nombres = document.Root!.Elements().Select(e => e.Name.LocalName).ToList();

        Assert.Equal("CreditNote", document.Root!.Name.LocalName);
        AssertOrden(nombres, "DocumentCurrencyCode", "DiscrepancyResponse");
        AssertOrden(nombres, "DiscrepancyResponse", "BillingReference");
        AssertOrden(nombres, "BillingReference", "AccountingSupplierParty");
        AssertOrden(nombres, "AccountingSupplierParty", "AccountingCustomerParty");
        AssertOrden(nombres, "AccountingCustomerParty", "TaxTotal");
        AssertOrden(nombres, "TaxTotal", "LegalMonetaryTotal");
        AssertOrden(nombres, "LegalMonetaryTotal", "CreditNoteLine");
    }

    [Fact]
    public void Generar_NotaCreditoSobreBoleta_IncluyeMotivoCatalogo09YDocumentoReferencia()
    {
        var xml = new CpeXmlUblCreditNoteGenerator().Generar(CrearNotaCreditoSobreBoleta());
        var document = XDocument.Parse(xml);

        Assert.Equal("BC01-1", Valor(document, "ID"));
        Assert.Equal("PEN", Valor(document, "DocumentCurrencyCode"));

        var discrepancy = document.Root!.Elements().First(e => e.Name.LocalName == "DiscrepancyResponse");
        Assert.Equal("B001-3002", ValorHijo(discrepancy, "ReferenceID"));
        Assert.Equal("01", ValorHijo(discrepancy, "ResponseCode"));
        Assert.Equal("Anulacion de la operacion", ValorHijo(discrepancy, "Description"));

        var billing = document.Root.Elements().First(e => e.Name.LocalName == "BillingReference");
        var invoiceRef = billing.Elements().First(e => e.Name.LocalName == "InvoiceDocumentReference");
        Assert.Equal("B001-3002", ValorHijo(invoiceRef, "ID"));
        Assert.Equal("03", ValorHijo(invoiceRef, "DocumentTypeCode"));
    }

    [Fact]
    public void Generar_NotaCreditoSobreBoleta_UsaCreditNoteLineYCreditedQuantity()
    {
        var xml = new CpeXmlUblCreditNoteGenerator().Generar(CrearNotaCreditoSobreBoleta());
        var document = XDocument.Parse(xml);

        Assert.DoesNotContain(
            document.Root!.Elements(),
            e => e.Name.LocalName == "InvoiceLine");
        Assert.NotNull(
            document.Root.Elements().FirstOrDefault(e => e.Name.LocalName == "CreditNoteLine"));
        Assert.NotNull(
            document.Root.Descendants().FirstOrDefault(e => e.Name.LocalName == "CreditedQuantity"));
    }

    [Fact]
    public void Generar_NotaCreditoSobreBoleta_NoReintroduceSignatureNiPaymentTerms()
    {
        var xml = new CpeXmlUblCreditNoteGenerator().Generar(CrearNotaCreditoSobreBoleta());
        var document = XDocument.Parse(xml);
        var nombres = document.Root!.Elements().Select(e => e.Name.LocalName).ToList();

        Assert.DoesNotContain("Note", nombres);
        Assert.DoesNotContain("LineCountNumeric", nombres);
        Assert.DoesNotContain("PaymentTerms", nombres);
        Assert.DoesNotContain("Signature", nombres);
        Assert.DoesNotContain("InvoiceTypeCode", nombres);
        Assert.DoesNotContain("ProfileID", nombres);
    }

    [Fact]
    public void Generar_NotaCreditoSobreBoleta_EstructuraTaxSubtotalUsaTaxCategory()
    {
        var xml = new CpeXmlUblCreditNoteGenerator().Generar(CrearNotaCreditoSobreBoleta());
        var document = XDocument.Parse(xml);
        var taxTotalGlobal = document.Root!.Elements().First(e => e.Name.LocalName == "TaxTotal");

        AssertTaxSubtotalsValidos(taxTotalGlobal);
    }

    [Fact]
    public void Generar_SinDocumentoReferencia_LanzaInvalidOperationException()
    {
        var documento = CrearNotaCreditoSobreBoleta();
        documento.DocumentoReferencia = null;

        var ex = Assert.Throws<InvalidOperationException>(
            () => new CpeXmlUblCreditNoteGenerator().Generar(documento));

        Assert.Contains("DocumentoReferencia", ex.Message);
    }

    [Fact]
    public void ValidarXml_AceptaOrdenUblCreditNoteGenerado()
    {
        var xml = new CpeXmlUblCreditNoteGenerator().Generar(CrearNotaCreditoSobreBoleta());

        var resultado = new CpeXmlValidatorService().ValidarXml(xml);

        Assert.True(resultado.Ok, string.Join(" | ", resultado.Errores));
    }

    [Fact]
    public void ValidarXml_DetectaDiscrepancyResponseFaltante()
    {
        var xml = new CpeXmlUblCreditNoteGenerator().Generar(CrearNotaCreditoSobreBoleta());
        var document = XDocument.Parse(xml);
        var discrepancy = document.Root!.Elements().First(e => e.Name.LocalName == "DiscrepancyResponse");
        discrepancy.Remove();

        var resultado = new CpeXmlValidatorService().ValidarXml(document.ToString());

        Assert.False(resultado.Ok);
        Assert.Contains(
            resultado.Errores,
            error => error.Contains("DiscrepancyResponse"));
    }

    [Fact]
    public void ValidarXml_DetectaBillingReferenceDespuesDeAccountingSupplierParty()
    {
        var xml = new CpeXmlUblCreditNoteGenerator().Generar(CrearNotaCreditoSobreBoleta());
        var document = XDocument.Parse(xml);
        var root = document.Root!;
        var billing = root.Elements().First(e => e.Name.LocalName == "BillingReference");
        var supplier = root.Elements().First(e => e.Name.LocalName == "AccountingSupplierParty");

        billing.Remove();
        supplier.AddAfterSelf(billing);

        var resultado = new CpeXmlValidatorService().ValidarXml(document.ToString());

        Assert.False(resultado.Ok);
        Assert.Contains(
            resultado.Errores,
            error => error.Contains("BillingReference debe aparecer antes de AccountingSupplierParty")
        );
    }

    private static void AssertTaxSubtotalsValidos(XElement taxTotal)
    {
        var taxSubtotals = taxTotal
            .Elements()
            .Where(e => e.Name.LocalName == "TaxSubtotal")
            .ToList();

        Assert.NotEmpty(taxSubtotals);

        foreach (var taxSubtotal in taxSubtotals)
        {
            Assert.DoesNotContain(taxSubtotal.Elements(), e => e.Name.LocalName == "TaxScheme");
            var taxCategory = taxSubtotal.Elements().FirstOrDefault(e => e.Name.LocalName == "TaxCategory");
            Assert.NotNull(taxCategory);
            Assert.NotNull(taxCategory!.Elements().FirstOrDefault(e => e.Name.LocalName == "TaxScheme"));
        }
    }

    private static void AssertOrden(List<string> nombres, string primero, string segundo)
    {
        var indicePrimero = nombres.IndexOf(primero);
        var indiceSegundo = nombres.IndexOf(segundo);

        Assert.True(indicePrimero >= 0, $"No se encontro {primero}.");
        Assert.True(indiceSegundo >= 0, $"No se encontro {segundo}.");
        Assert.True(
            indicePrimero < indiceSegundo,
            $"Se esperaba {primero} antes de {segundo}. Orden: {string.Join(", ", nombres)}"
        );
    }

    private static string? Valor(XDocument document, string localName)
    {
        return document.Root!
            .Elements()
            .FirstOrDefault(e => e.Name.LocalName == localName)
            ?.Value;
    }

    private static string? ValorHijo(XElement parent, string localName)
    {
        return parent.Elements().FirstOrDefault(e => e.Name.LocalName == localName)?.Value;
    }

    private static CpeDocumento CrearNotaCreditoSobreBoleta()
    {
        return new CpeDocumento
        {
            NumeroComprobante = "20100066603-07-BC01-1",
            TipoComprobante = "07",
            Serie = "BC01",
            Correlativo = 1,
            FechaEmision = new DateTime(2026, 8, 6, 18, 0, 0),
            Moneda = "PEN",
            TipoOperacion = "0101",
            FormaPago = "CONTADO",
            CodigoMotivo = "01",
            DescripcionMotivo = "Anulacion de la operacion",
            DocumentoReferencia = new CpeDocumentoReferencia
            {
                TipoComprobante = "03",
                SerieCorrelativo = "B001-3002"
            },
            Emisor = new CpeEmisor
            {
                Ruc = "20100066603",
                RazonSocial = "EMPRESA BETA SUNAT SAC",
                NombreComercial = "EMPRESA BETA",
                Ubigeo = "150101",
                Direccion = "AV. BETA 123",
                Departamento = "LIMA",
                Provincia = "LIMA",
                Distrito = "LIMA"
            },
            Cliente = new CpeCliente
            {
                TipoDocumento = "1",
                NumeroDocumento = "12345678",
                RazonSocial = "CLIENTE BOLETA BETA"
            },
            Items = new List<CpeItem>
            {
                new()
                {
                    Codigo = "P001",
                    Descripcion = "Producto gravado beta",
                    UnidadMedida = "NIU",
                    Cantidad = 1,
                    ValorUnitario = 100.00m,
                    PrecioUnitario = 118.00m,
                    Subtotal = 100.00m,
                    Igv = 18.00m,
                    Total = 118.00m,
                    CodigoAfectacionIgv = "10"
                }
            },
            TotalGravada = 100.00m,
            TotalExonerada = 0.00m,
            TotalInafecta = 0.00m,
            TotalIgv = 18.00m,
            Total = 118.00m
        };
    }
}
