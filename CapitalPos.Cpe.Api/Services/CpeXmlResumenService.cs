using System.Globalization;
using System.Xml.Linq;
using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Interfaces;

namespace CapitalPos.Cpe.Api.Infrastructure.Xml;

public class CpeXmlResumenService : ICpeXmlResumenService
{
    public CpeXmlResumenResponse ObtenerResumen(string xml)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                return CrearError("El XML está vacío.");
            }

            var document = XDocument.Parse(xml);
            var root = document.Root;

            if (root == null)
            {
                return CrearError("El XML no tiene nodo raíz.");
            }

            if (root.Name.LocalName == "Comprobante")
            {
                return ObtenerResumenXmlBasico(root);
            }

            if (root.Name.LocalName == "Invoice")
            {
                return ObtenerResumenXmlUbl(root);
            }

            return CrearError($"Tipo de XML no reconocido. Nodo raíz: {root.Name.LocalName}");
        }
        catch (Exception ex)
        {
            return CrearError($"No se pudo leer el resumen del XML: {ex.Message}");
        }
    }

    private static CpeXmlResumenResponse ObtenerResumenXmlBasico(XElement root)
    {
        var numero = ObtenerValor(root, "Numero");

        var documento = root.Element("Documento");
        var emisor = root.Element("Emisor");
        var cliente = root.Element("Cliente");
        var totales = root.Element("Totales");
        var items = root.Element("Items");

        var serie = ObtenerValor(documento, "Serie");
        var correlativo = ObtenerValor(documento, "Correlativo");

        return new CpeXmlResumenResponse
        {
            Ok = true,
            Mensaje = "Resumen XML básico leído correctamente.",
            TipoXml = "BASICO",

            NumeroComprobante = numero,
            TipoComprobante = ObtenerValor(documento, "Tipo"),
            Serie = serie,
            Correlativo = correlativo,

            FechaEmision = ObtenerValor(documento, "FechaEmision"),
            Moneda = ObtenerValor(documento, "Moneda"),

            RucEmisor = ObtenerValor(emisor, "Ruc"),
            RazonSocialEmisor = ObtenerValor(emisor, "RazonSocial"),

            TipoDocumentoCliente = ObtenerValor(cliente, "TipoDocumento"),
            NumeroDocumentoCliente = ObtenerValor(cliente, "NumeroDocumento"),
            RazonSocialCliente = ObtenerValor(cliente, "RazonSocial"),

            TotalGravada = ObtenerDecimal(totales, "TotalGravada"),
            TotalExonerada = ObtenerDecimal(totales, "TotalExonerada"),
            TotalInafecta = ObtenerDecimal(totales, "TotalInafecta"),
            TotalIgv = ObtenerDecimal(totales, "TotalIgv"),
            Total = ObtenerDecimal(totales, "Total"),

            CantidadItems = items?.Elements("Item").Count() ?? 0
        };
    }

    private static CpeXmlResumenResponse ObtenerResumenXmlUbl(XElement root)
    {
        var cac = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
        var cbc = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

        var serieCorrelativo = root.Element(cbc + "ID")?.Value ?? string.Empty;
        var partes = serieCorrelativo.Split("-");

        var serie = partes.Length > 0 ? partes[0] : string.Empty;
        var correlativo = partes.Length > 1 ? partes[1] : string.Empty;

        var emisor = root.Element(cac + "AccountingSupplierParty")
            ?.Element(cac + "Party");

        var cliente = root.Element(cac + "AccountingCustomerParty")
            ?.Element(cac + "Party");

        var taxTotal = root.Element(cac + "TaxTotal");

        var monetaryTotal = root.Element(cac + "LegalMonetaryTotal");

        var rucEmisor = emisor
            ?.Element(cac + "PartyIdentification")
            ?.Element(cbc + "ID")
            ?.Value ?? string.Empty;

        var razonSocialEmisor = emisor
            ?.Element(cac + "PartyLegalEntity")
            ?.Element(cbc + "RegistrationName")
            ?.Value ?? string.Empty;

        var clienteId = cliente
            ?.Element(cac + "PartyIdentification")
            ?.Element(cbc + "ID");

        var tipoDocumentoCliente = clienteId
            ?.Attribute("schemeID")
            ?.Value ?? string.Empty;

        var numeroDocumentoCliente = clienteId?.Value ?? string.Empty;

        var razonSocialCliente = cliente
            ?.Element(cac + "PartyLegalEntity")
            ?.Element(cbc + "RegistrationName")
            ?.Value ?? string.Empty;

        return new CpeXmlResumenResponse
        {
            Ok = true,
            Mensaje = "Resumen XML UBL leído correctamente.",
            TipoXml = "UBL",

            NumeroComprobante = $"{rucEmisor}-{ObtenerValor(root, cbc + "InvoiceTypeCode")}-{serie}-{correlativo}",
            TipoComprobante = ObtenerValor(root, cbc + "InvoiceTypeCode"),
            Serie = serie,
            Correlativo = correlativo,

            FechaEmision = ObtenerValor(root, cbc + "IssueDate"),
            Moneda = ObtenerValor(root, cbc + "DocumentCurrencyCode"),

            RucEmisor = rucEmisor,
            RazonSocialEmisor = razonSocialEmisor,

            TipoDocumentoCliente = tipoDocumentoCliente,
            NumeroDocumentoCliente = numeroDocumentoCliente,
            RazonSocialCliente = razonSocialCliente,

            TotalIgv = ObtenerDecimal(taxTotal, cbc + "TaxAmount"),
            Total = ObtenerDecimal(monetaryTotal, cbc + "PayableAmount"),

            CantidadItems = root.Elements(cac + "InvoiceLine").Count()
        };
    }

    private static CpeXmlResumenResponse CrearError(string error)
    {
        return new CpeXmlResumenResponse
        {
            Ok = false,
            Mensaje = "No se pudo obtener el resumen del XML.",
            Errores = new List<string> { error }
        };
    }

    private static string ObtenerValor(XElement? elemento, string nombre)
    {
        return elemento?.Element(nombre)?.Value ?? string.Empty;
    }

    private static string ObtenerValor(XElement? elemento, XName nombre)
    {
        return elemento?.Element(nombre)?.Value ?? string.Empty;
    }

    private static decimal ObtenerDecimal(XElement? elemento, string nombre)
    {
        var valor = ObtenerValor(elemento, nombre);
        return ConvertirDecimal(valor);
    }

    private static decimal ObtenerDecimal(XElement? elemento, XName nombre)
    {
        var valor = ObtenerValor(elemento, nombre);
        return ConvertirDecimal(valor);
    }

    private static decimal ConvertirDecimal(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return 0;

        valor = valor.Replace(",", ".");

        return decimal.TryParse(
            valor,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out var resultado)
            ? resultado
            : 0;
    }
}