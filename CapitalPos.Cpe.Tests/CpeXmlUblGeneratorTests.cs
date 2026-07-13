using System.Xml.Linq;
using CapitalPos.Cpe.Api.Domain.Cpe;
using CapitalPos.Cpe.Api.Infrastructure.Xml;
using CapitalPos.Cpe.Api.Infrastructure.Xml.Generators;
using Xunit;

namespace CapitalPos.Cpe.Tests;

public class CpeXmlUblGeneratorTests
{
    [Fact]
    public void Generar_BoletaConDni_ColocaProveedorClienteYTotalesEnOrdenSunatBeta()
    {
        var xml = new CpeXmlUblGenerator().Generar(CrearBoleta());
        var document = XDocument.Parse(xml);
        var nombres = document.Root!.Elements().Select(e => e.Name.LocalName).ToList();

        AssertOrden(nombres, "InvoiceTypeCode", "DocumentCurrencyCode");
        AssertOrden(nombres, "DocumentCurrencyCode", "AccountingSupplierParty");
        AssertOrden(nombres, "AccountingSupplierParty", "AccountingCustomerParty");
        AssertOrden(nombres, "AccountingCustomerParty", "TaxTotal");
        AssertOrden(nombres, "TaxTotal", "LegalMonetaryTotal");
        AssertOrden(nombres, "LegalMonetaryTotal", "InvoiceLine");
    }

    [Fact]
    public void Generar_BoletaConDni_MantieneOrdenBasicoDeTotalesYLineas()
    {
        var xml = new CpeXmlUblGenerator().Generar(CrearBoleta());
        var document = XDocument.Parse(xml);
        var nombres = document.Root!.Elements().Select(e => e.Name.LocalName).ToList();

        AssertOrden(nombres, "TaxTotal", "LegalMonetaryTotal");
        AssertOrden(nombres, "LegalMonetaryTotal", "InvoiceLine");
    }

    [Fact]
    public void Generar_BoletaConDni_ContieneInvoiceTypeCodeConValor03()
    {
        var xml = new CpeXmlUblGenerator().Generar(CrearBoleta());
        var document = XDocument.Parse(xml);

        Assert.Equal("03", Valor(document, "InvoiceTypeCode"));
    }

    [Fact]
    public void Generar_BoletaConDni_ColocaDocumentCurrencyCodeDespuesDeInvoiceTypeCode()
    {
        var xml = new CpeXmlUblGenerator().Generar(CrearBoleta());
        var document = XDocument.Parse(xml);
        var nombres = document.Root!.Elements().Select(e => e.Name.LocalName).ToList();

        AssertOrden(nombres, "InvoiceTypeCode", "DocumentCurrencyCode");
    }

    [Fact]
    public void Generar_BoletaConDni_ColocaDocumentCurrencyCodeAntesDeAccountingSupplierParty()
    {
        var xml = new CpeXmlUblGenerator().Generar(CrearBoleta());
        var document = XDocument.Parse(xml);
        var nombres = document.Root!.Elements().Select(e => e.Name.LocalName).ToList();

        AssertOrden(nombres, "DocumentCurrencyCode", "AccountingSupplierParty");
    }

    [Fact]
    public void Generar_BoletaConDni_ContieneDocumentCurrencyCodeConValorPen()
    {
        var xml = new CpeXmlUblGenerator().Generar(CrearBoleta());
        var document = XDocument.Parse(xml);

        Assert.Equal("PEN", Valor(document, "DocumentCurrencyCode"));
    }

    [Fact]
    public void Generar_BoletaConDni_NoReintroduceElementosPrincipalesDiferidos()
    {
        var xml = new CpeXmlUblGenerator().Generar(CrearBoleta());
        var document = XDocument.Parse(xml);
        var nombres = document.Root!.Elements().Select(e => e.Name.LocalName).ToList();

        Assert.DoesNotContain("Note", nombres);
        Assert.DoesNotContain("LineCountNumeric", nombres);
        Assert.DoesNotContain("PaymentTerms", nombres);
        Assert.DoesNotContain("Signature", nombres);
    }

    [Fact]
    public void Generar_BoletaConDni_NoColocaNoteAntesDeLegalMonetaryTotal()
    {
        var xml = new CpeXmlUblGenerator().Generar(CrearBoleta());
        var document = XDocument.Parse(xml);
        var nombres = document.Root!.Elements().Select(e => e.Name.LocalName).ToList();

        var indiceNote = nombres.IndexOf("Note");
        var indiceLegalMonetaryTotal = nombres.IndexOf("LegalMonetaryTotal");

        Assert.True(indiceLegalMonetaryTotal >= 0, "No se encontro LegalMonetaryTotal.");
        Assert.True(
            indiceNote < 0 || indiceNote > indiceLegalMonetaryTotal,
            $"Note no debe aparecer antes de LegalMonetaryTotal. Orden: {string.Join(", ", nombres)}"
        );
    }

    [Fact]
    public void Generar_BoletaConDni_MantieneFirmaYDatosPrincipales()
    {
        var xml = new CpeXmlUblGenerator().Generar(CrearBoleta());
        var document = XDocument.Parse(xml);

        Assert.Equal("Invoice", document.Root!.Name.LocalName);
        Assert.Equal("B001-3002", Valor(document, "ID"));
        Assert.Equal("03", Valor(document, "InvoiceTypeCode"));
        Assert.Equal("PEN", Valor(document, "DocumentCurrencyCode"));
        Assert.NotNull(document.Root.Descendants().FirstOrDefault(e => e.Name.LocalName == "ExtensionContent"));
        Assert.NotNull(document.Root.Elements().FirstOrDefault(e => e.Name.LocalName == "AccountingSupplierParty"));
        Assert.NotNull(document.Root.Elements().FirstOrDefault(e => e.Name.LocalName == "AccountingCustomerParty"));
        Assert.NotNull(document.Root.Elements().FirstOrDefault(e => e.Name.LocalName == "TaxTotal"));
        Assert.NotNull(document.Root.Elements().FirstOrDefault(e => e.Name.LocalName == "LegalMonetaryTotal"));
        Assert.NotNull(document.Root.Descendants().FirstOrDefault(e => e.Name.LocalName == "InvoiceLine"));
    }

    [Fact]
    public void Generar_BoletaConDni_EstructuraTaxSubtotalGlobalUsaTaxCategory()
    {
        var xml = new CpeXmlUblGenerator().Generar(CrearBoleta());
        var document = XDocument.Parse(xml);
        var taxTotalGlobal = document.Root!.Elements().First(e => e.Name.LocalName == "TaxTotal");

        AssertTaxSubtotalsValidos(taxTotalGlobal);
    }

    [Fact]
    public void Generar_BoletaConDni_EstructuraTaxSubtotalDeLineasUsaTaxCategory()
    {
        var xml = new CpeXmlUblGenerator().Generar(CrearBoleta());
        var document = XDocument.Parse(xml);
        var invoiceLineTaxTotals = document.Root!
            .Elements()
            .Where(e => e.Name.LocalName == "InvoiceLine")
            .SelectMany(e => e.Elements().Where(child => child.Name.LocalName == "TaxTotal"))
            .ToList();

        Assert.NotEmpty(invoiceLineTaxTotals);

        foreach (var taxTotal in invoiceLineTaxTotals)
        {
            AssertTaxSubtotalsValidos(taxTotal);
        }
    }

    [Fact]
    public void ValidarXml_DetectaTaxTotalAntesDeAccountingCustomerParty()
    {
        var xml = new CpeXmlUblGenerator().Generar(CrearBoleta());
        var document = XDocument.Parse(xml);
        var root = document.Root!;
        var taxTotal = root.Elements().First(e => e.Name.LocalName == "TaxTotal");
        var customer = root.Elements().First(e => e.Name.LocalName == "AccountingCustomerParty");

        taxTotal.Remove();
        customer.AddBeforeSelf(taxTotal);

        var resultado = new CpeXmlValidatorService().ValidarXml(document.ToString());

        Assert.False(resultado.Ok);
        Assert.Contains(
            resultado.Errores,
            error => error.Contains("AccountingCustomerParty debe aparecer antes de TaxTotal")
        );
    }

    [Fact]
    public void ValidarXml_DetectaInvoiceTypeCodeFaltante()
    {
        var xml = new CpeXmlUblGenerator().Generar(CrearBoleta());
        var document = XDocument.Parse(xml);
        var invoiceTypeCode = document.Root!.Elements().First(e => e.Name.LocalName == "InvoiceTypeCode");

        invoiceTypeCode.Remove();

        var resultado = new CpeXmlValidatorService().ValidarXml(document.ToString());

        Assert.False(resultado.Ok);
        Assert.Contains(
            resultado.Errores,
            error => error.Contains("Falta el elemento UBL InvoiceTypeCode")
        );
    }

    [Fact]
    public void ValidarXml_DetectaDocumentCurrencyCodeFaltante()
    {
        var xml = new CpeXmlUblGenerator().Generar(CrearBoleta());
        var document = XDocument.Parse(xml);
        var documentCurrencyCode = document.Root!.Elements().First(e => e.Name.LocalName == "DocumentCurrencyCode");

        documentCurrencyCode.Remove();

        var resultado = new CpeXmlValidatorService().ValidarXml(document.ToString());

        Assert.False(resultado.Ok);
        Assert.Contains(
            resultado.Errores,
            error => error.Contains("Falta el elemento UBL DocumentCurrencyCode")
        );
    }

    [Fact]
    public void ValidarXml_DetectaDocumentCurrencyCodeDespuesDeAccountingSupplierParty()
    {
        var xml = new CpeXmlUblGenerator().Generar(CrearBoleta());
        var document = XDocument.Parse(xml);
        var root = document.Root!;
        var documentCurrencyCode = root.Elements().First(e => e.Name.LocalName == "DocumentCurrencyCode");
        var supplier = root.Elements().First(e => e.Name.LocalName == "AccountingSupplierParty");

        documentCurrencyCode.Remove();
        supplier.AddAfterSelf(documentCurrencyCode);

        var resultado = new CpeXmlValidatorService().ValidarXml(document.ToString());

        Assert.False(resultado.Ok);
        Assert.Contains(
            resultado.Errores,
            error => error.Contains("DocumentCurrencyCode debe aparecer antes de AccountingSupplierParty")
        );
    }

    [Fact]
    public void ValidarXml_DetectaElementosPrincipalesDiferidos()
    {
        var xml = new CpeXmlUblGenerator().Generar(CrearBoleta());
        var document = XDocument.Parse(xml);
        var root = document.Root!;
        var cbc = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
        var cac = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
        var invoiceLine = root.Elements().First(e => e.Name.LocalName == "InvoiceLine");

        invoiceLine.AddBeforeSelf(
            new XElement(cbc + "LineCountNumeric", "1"),
            new XElement(cac + "PaymentTerms"),
            new XElement(cac + "Signature")
        );

        var resultado = new CpeXmlValidatorService().ValidarXml(document.ToString());

        Assert.False(resultado.Ok);
        Assert.Contains(resultado.Errores, error => error.Contains("LineCountNumeric no debe aparecer"));
        Assert.Contains(resultado.Errores, error => error.Contains("PaymentTerms no debe aparecer"));
        Assert.Contains(resultado.Errores, error => error.Contains("Signature no debe aparecer"));
    }

    [Fact]
    public void ValidarXml_DetectaTaxSchemeComoHijoDirectoDeTaxSubtotal()
    {
        var xml = new CpeXmlUblGenerator().Generar(CrearBoleta());
        var document = XDocument.Parse(xml);
        var taxSubtotal = document
            .Descendants()
            .First(e => e.Name.LocalName == "TaxSubtotal");
        var taxCategory = taxSubtotal.Elements().First(e => e.Name.LocalName == "TaxCategory");
        var taxScheme = taxCategory.Elements().First(e => e.Name.LocalName == "TaxScheme");

        taxScheme.Remove();
        taxCategory.Remove();
        taxSubtotal.Add(taxScheme);

        var resultado = new CpeXmlValidatorService().ValidarXml(document.ToString());

        Assert.False(resultado.Ok);
        Assert.Contains(
            resultado.Errores,
            error => error.Contains("TaxScheme no debe ser hijo directo de TaxSubtotal")
        );
        Assert.Contains(
            resultado.Errores,
            error => error.Contains("TaxSubtotal debe contener TaxCategory")
        );
    }

    [Fact]
    public void ValidarXml_DetectaTaxCategorySinTaxScheme()
    {
        var xml = new CpeXmlUblGenerator().Generar(CrearBoleta());
        var document = XDocument.Parse(xml);
        var taxCategory = document
            .Descendants()
            .First(e => e.Name.LocalName == "TaxCategory");
        var taxScheme = taxCategory.Elements().First(e => e.Name.LocalName == "TaxScheme");

        taxScheme.Remove();

        var resultado = new CpeXmlValidatorService().ValidarXml(document.ToString());

        Assert.False(resultado.Ok);
        Assert.Contains(
            resultado.Errores,
            error => error.Contains("TaxCategory debe contener TaxScheme")
        );
    }

    [Fact]
    public void ValidarXml_DetectaNoteAntesDeLegalMonetaryTotal()
    {
        var xml = new CpeXmlUblGenerator().Generar(CrearBoleta());
        var document = XDocument.Parse(xml);
        var root = document.Root!;
        var legalMonetaryTotal = root.Elements().First(e => e.Name.LocalName == "LegalMonetaryTotal");
        var cbc = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

        legalMonetaryTotal.AddBeforeSelf(new XElement(cbc + "Note", "LEYENDA"));

        var resultado = new CpeXmlValidatorService().ValidarXml(document.ToString());

        Assert.False(resultado.Ok);
        Assert.Contains(
            resultado.Errores,
            error => error.Contains("Note no debe aparecer antes de LegalMonetaryTotal")
        );
    }

    [Fact]
    public void ValidarXml_AceptaOrdenUblBasicoGenerado()
    {
        var xml = new CpeXmlUblGenerator().Generar(CrearBoleta());

        var resultado = new CpeXmlValidatorService().ValidarXml(xml);

        Assert.True(resultado.Ok, string.Join(" | ", resultado.Errores));
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
            Assert.NotNull(taxSubtotal.Elements().FirstOrDefault(e => e.Name.LocalName == "TaxableAmount"));
            Assert.NotNull(taxSubtotal.Elements().FirstOrDefault(e => e.Name.LocalName == "TaxAmount"));
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

    private static CpeDocumento CrearBoleta()
    {
        return new CpeDocumento
        {
            NumeroComprobante = "20100066603-03-B001-3002",
            TipoComprobante = "03",
            Serie = "B001",
            Correlativo = 3002,
            FechaEmision = new DateTime(2026, 7, 12, 11, 0, 0),
            Moneda = "PEN",
            TipoOperacion = "0101",
            FormaPago = "CONTADO",
            MontoEnLetras = "CIENTO DIECIOCHO Y 00/100 SOLES",
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
