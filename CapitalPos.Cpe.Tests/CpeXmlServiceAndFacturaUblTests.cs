using System.Xml.Linq;
using CapitalPos.Cpe.Api.Domain.Cpe;
using CapitalPos.Cpe.Api.Infrastructure.Xml;
using CapitalPos.Cpe.Api.Infrastructure.Xml.Generators;
using CapitalPos.Cpe.Api.Settings;
using Microsoft.Extensions.Options;
using Xunit;

namespace CapitalPos.Cpe.Tests;

public class CpeXmlServiceAndFacturaUblTests
{
    [Fact]
    public void GenerarXml_UblFactura_UsaRootInvoiceYTipo01()
    {
        var settings = Options.Create(new CpeSettings { SimularGeneracionXml = false });
        var xml = new CpeXmlService(settings).GenerarXml(CrearFactura());
        var document = XDocument.Parse(xml);

        Assert.Equal("Invoice", document.Root!.Name.LocalName);
        Assert.Equal("01", Valor(document, "InvoiceTypeCode"));
        Assert.Equal("F001-100", Valor(document, "ID"));
        Assert.Equal("PEN", Valor(document, "DocumentCurrencyCode"));
    }

    [Fact]
    public void GenerarXml_UblBoleta_UsaInvoiceTypeCode03()
    {
        var settings = Options.Create(new CpeSettings { SimularGeneracionXml = false });
        var xml = new CpeXmlService(settings).GenerarXml(CrearBoleta());
        var document = XDocument.Parse(xml);

        Assert.Equal("Invoice", document.Root!.Name.LocalName);
        Assert.Equal("03", Valor(document, "InvoiceTypeCode"));
    }

    [Fact]
    public void GenerarXml_Simulado_UsaXmlBasicoSinUblInvoice()
    {
        var settings = Options.Create(new CpeSettings { SimularGeneracionXml = true });
        var xml = new CpeXmlService(settings).GenerarXml(CrearBoleta());
        var document = XDocument.Parse(xml);

        Assert.Equal("Comprobante", document.Root!.Name.LocalName);
        Assert.DoesNotContain(
            document.Descendants(),
            e => e.Name.LocalName == "InvoiceTypeCode");
    }

    [Fact]
    public void Generar_FacturaUbl_MantieneOrdenSunatBasico()
    {
        var xml = new CpeXmlUblGenerator().Generar(CrearFactura());
        var document = XDocument.Parse(xml);
        var nombres = document.Root!.Elements().Select(e => e.Name.LocalName).ToList();

        AssertOrden(nombres, "InvoiceTypeCode", "DocumentCurrencyCode");
        AssertOrden(nombres, "DocumentCurrencyCode", "AccountingSupplierParty");
        AssertOrden(nombres, "AccountingSupplierParty", "AccountingCustomerParty");
        AssertOrden(nombres, "AccountingCustomerParty", "PaymentTerms");
        AssertOrden(nombres, "PaymentTerms", "TaxTotal");
        AssertOrden(nombres, "TaxTotal", "LegalMonetaryTotal");
        AssertOrden(nombres, "LegalMonetaryTotal", "InvoiceLine");
        Assert.Equal("FormaPago", document.Root!.Elements().First(e => e.Name.LocalName == "PaymentTerms")
            .Elements().First(e => e.Name.LocalName == "ID").Value);
        Assert.Equal("Contado", document.Root.Elements().First(e => e.Name.LocalName == "PaymentTerms")
            .Elements().First(e => e.Name.LocalName == "PaymentMeansID").Value);
    }

    [Fact]
    public void ValidarXml_FacturaUblGenerada_EsAceptada()
    {
        var xml = new CpeXmlUblGenerator().Generar(CrearFactura());
        var resultado = new CpeXmlValidatorService().ValidarXml(xml);

        Assert.True(resultado.Ok, string.Join(" | ", resultado.Errores));
    }

    [Fact]
    public void ValidarXml_XmlMalFormado_ErrorClaro()
    {
        var resultado = new CpeXmlValidatorService().ValidarXml("<Invoice><broken>");

        Assert.False(resultado.Ok);
        Assert.NotEmpty(resultado.Errores);
    }

    private static void AssertOrden(List<string> nombres, string primero, string segundo)
    {
        var indicePrimero = nombres.IndexOf(primero);
        var indiceSegundo = nombres.IndexOf(segundo);

        Assert.True(indicePrimero >= 0, $"No se encontro {primero}.");
        Assert.True(indiceSegundo >= 0, $"No se encontro {segundo}.");
        Assert.True(
            indicePrimero < indiceSegundo,
            $"Se esperaba {primero} antes de {segundo}. Orden: {string.Join(", ", nombres)}");
    }

    private static string? Valor(XDocument document, string localName)
    {
        return document.Root!
            .Elements()
            .FirstOrDefault(e => e.Name.LocalName == localName)
            ?.Value;
    }

    private static CpeDocumento CrearFactura()
    {
        var documento = CrearBoleta();
        documento.NumeroComprobante = "20100066603-01-F001-100";
        documento.TipoComprobante = "01";
        documento.Serie = "F001";
        documento.Correlativo = 100;
        documento.Cliente = new CpeCliente
        {
            TipoDocumento = "6",
            NumeroDocumento = "20600000001",
            RazonSocial = "CLIENTE FACTURA SAC"
        };
        return documento;
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
