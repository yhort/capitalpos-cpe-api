using System.Xml.Linq;
using CapitalPos.Cpe.Api.Domain;
using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Interfaces;

namespace CapitalPos.Cpe.Api.Infrastructure.Xml;

public class CpeXmlValidatorService : ICpeXmlValidatorService
{
    public CpeXmlValidacionResponse ValidarXml(string xml)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(xml))
        {
            errores.Add("El XML está vacío.");
        }
        else
        {
            try
            {
                var documento = XDocument.Parse(xml);
                var raizLocalName = documento.Root?.Name.LocalName;

                if (raizLocalName == "CreditNote")
                    ValidarOrdenCreditNoteUblSunat(documento, errores);
                else
                    ValidarOrdenInvoiceUblSunat(documento, errores);
            }
            catch (Exception ex)
            {
                errores.Add($"El XML no está bien formado: {ex.Message}");
            }
        }

        if (errores.Count > 0)
        {
            return new CpeXmlValidacionResponse
            {
                Ok = false,
                Mensaje = "El XML tiene errores.",
                Errores = errores
            };
        }

        return new CpeXmlValidacionResponse
        {
            Ok = true,
            Mensaje = "El XML está bien formado."
        };
    }

    private static void ValidarOrdenInvoiceUblSunat(XDocument documento, List<string> errores)
    {
        var raiz = documento.Root;

        if (raiz == null || raiz.Name.LocalName != "Invoice")
            return;

        var elementos = raiz.Elements().ToList();

        ValidarAntes(elementos, "InvoiceTypeCode", "DocumentCurrencyCode", errores);
        ValidarAntes(elementos, "DocumentCurrencyCode", "AccountingSupplierParty", errores);
        ValidarAntes(
            elementos,
            "AccountingSupplierParty",
            "AccountingCustomerParty",
            errores
        );

        ValidarAntes(elementos, "AccountingCustomerParty", "TaxTotal", errores);
        ValidarAntes(elementos, "TaxTotal", "LegalMonetaryTotal", errores);
        ValidarAntes(elementos, "LegalMonetaryTotal", "InvoiceLine", errores);
        ValidarNoteNoAntesDeLegalMonetaryTotal(elementos, errores);
        ValidarFormaPagoSegunTipo(elementos, errores);
        ValidarElementosNoReintroducidos(elementos, errores);
        ValidarEstructuraImpuestos(raiz, errores);
    }

    private static void ValidarOrdenCreditNoteUblSunat(XDocument documento, List<string> errores)
    {
        var raiz = documento.Root;

        if (raiz == null || raiz.Name.LocalName != "CreditNote")
            return;

        var elementos = raiz.Elements().ToList();

        ValidarAntes(elementos, "DocumentCurrencyCode", "DiscrepancyResponse", errores);
        ValidarAntes(elementos, "DiscrepancyResponse", "BillingReference", errores);
        ValidarAntes(elementos, "BillingReference", "AccountingSupplierParty", errores);
        ValidarAntes(
            elementos,
            "AccountingSupplierParty",
            "AccountingCustomerParty",
            errores
        );
        ValidarAntes(elementos, "AccountingCustomerParty", "TaxTotal", errores);
        ValidarAntes(elementos, "TaxTotal", "LegalMonetaryTotal", errores);
        ValidarAntes(elementos, "LegalMonetaryTotal", "CreditNoteLine", errores);
        ValidarNoteNoAntesDeLegalMonetaryTotal(elementos, errores);
        ValidarAusente(elementos, "LineCountNumeric", errores);
        ValidarAusente(elementos, "PaymentTerms", errores);
        ValidarAusente(elementos, "Signature", errores);
        ValidarEstructuraImpuestos(raiz, errores);
        ValidarDiscrepancyResponse(raiz, errores);
        ValidarBillingReference(raiz, errores);
    }

    private static void ValidarDiscrepancyResponse(XElement raiz, List<string> errores)
    {
        var discrepancy = raiz
            .Elements()
            .FirstOrDefault(e => e.Name.LocalName == "DiscrepancyResponse");

        if (discrepancy == null)
        {
            errores.Add("Falta el elemento UBL DiscrepancyResponse.");
            return;
        }

        if (!discrepancy.Elements().Any(e => e.Name.LocalName == "ReferenceID"))
            errores.Add("DiscrepancyResponse debe contener ReferenceID.");

        if (!discrepancy.Elements().Any(e => e.Name.LocalName == "ResponseCode"))
            errores.Add("DiscrepancyResponse debe contener ResponseCode.");

        if (!discrepancy.Elements().Any(e => e.Name.LocalName == "Description"))
            errores.Add("DiscrepancyResponse debe contener Description.");
    }

    private static void ValidarBillingReference(XElement raiz, List<string> errores)
    {
        var billing = raiz
            .Elements()
            .FirstOrDefault(e => e.Name.LocalName == "BillingReference");

        if (billing == null)
        {
            errores.Add("Falta el elemento UBL BillingReference.");
            return;
        }

        var invoiceRef = billing
            .Elements()
            .FirstOrDefault(e => e.Name.LocalName == "InvoiceDocumentReference");

        if (invoiceRef == null)
        {
            errores.Add("BillingReference debe contener InvoiceDocumentReference.");
            return;
        }

        if (!invoiceRef.Elements().Any(e => e.Name.LocalName == "ID"))
            errores.Add("InvoiceDocumentReference debe contener ID.");

        if (!invoiceRef.Elements().Any(e => e.Name.LocalName == "DocumentTypeCode"))
            errores.Add("InvoiceDocumentReference debe contener DocumentTypeCode.");
    }

    private static void ValidarAntes(
        List<XElement> elementos,
        string primero,
        string segundo,
        List<string> errores)
    {
        var indicePrimero = elementos.FindIndex(e => e.Name.LocalName == primero);
        var indiceSegundo = elementos.FindIndex(e => e.Name.LocalName == segundo);

        if (indicePrimero < 0)
        {
            errores.Add($"Falta el elemento UBL {primero}.");
            return;
        }

        if (indiceSegundo < 0)
        {
            errores.Add($"Falta el elemento UBL {segundo}.");
            return;
        }

        if (indicePrimero > indiceSegundo)
        {
            errores.Add($"El elemento UBL {primero} debe aparecer antes de {segundo}.");
        }
    }

    private static void ValidarNoteNoAntesDeLegalMonetaryTotal(
        List<XElement> elementos,
        List<string> errores)
    {
        var indiceNote = elementos.FindIndex(e => e.Name.LocalName == "Note");

        if (indiceNote < 0)
            return;

        var indiceLegalMonetaryTotal = elementos.FindIndex(e => e.Name.LocalName == "LegalMonetaryTotal");

        if (indiceLegalMonetaryTotal < 0)
        {
            errores.Add("Falta el elemento UBL LegalMonetaryTotal.");
            return;
        }

        if (indiceNote < indiceLegalMonetaryTotal)
        {
            errores.Add("El elemento UBL Note no debe aparecer antes de LegalMonetaryTotal para SUNAT beta.");
        }
    }

    private static void ValidarFormaPagoSegunTipo(List<XElement> elementos, List<string> errores)
    {
        var tipo = elementos.FirstOrDefault(e => e.Name.LocalName == "InvoiceTypeCode")?.Value;
        if (tipo == CatalogosSunat.TipoComprobante.Factura)
        {
            ValidarAntes(elementos, "AccountingCustomerParty", "PaymentTerms", errores);
            ValidarAntes(elementos, "PaymentTerms", "TaxTotal", errores);
            return;
        }

        ValidarAusente(elementos, "PaymentTerms", errores);
    }

    private static void ValidarElementosNoReintroducidos(
        List<XElement> elementos,
        List<string> errores)
    {
        ValidarAusente(elementos, "LineCountNumeric", errores);
        ValidarAusente(elementos, "Signature", errores);
    }

    private static void ValidarAusente(
        List<XElement> elementos,
        string nombre,
        List<string> errores)
    {
        if (elementos.Any(e => e.Name.LocalName == nombre))
        {
            errores.Add($"El elemento UBL {nombre} no debe aparecer como nodo principal en esta etapa.");
        }
    }

    private static void ValidarEstructuraImpuestos(XElement raiz, List<string> errores)
    {
        var taxSubtotals = raiz
            .Descendants()
            .Where(e => e.Name.LocalName == "TaxSubtotal")
            .ToList();

        foreach (var taxSubtotal in taxSubtotals)
        {
            var tieneTaxSchemeDirecto = taxSubtotal
                .Elements()
                .Any(e => e.Name.LocalName == "TaxScheme");

            if (tieneTaxSchemeDirecto)
            {
                errores.Add("TaxScheme no debe ser hijo directo de TaxSubtotal; debe estar dentro de TaxCategory.");
            }

            var taxCategory = taxSubtotal
                .Elements()
                .FirstOrDefault(e => e.Name.LocalName == "TaxCategory");

            if (taxCategory == null)
            {
                errores.Add("TaxSubtotal debe contener TaxCategory.");
                continue;
            }

            var tieneTaxSchemeEnCategoria = taxCategory
                .Elements()
                .Any(e => e.Name.LocalName == "TaxScheme");

            if (!tieneTaxSchemeEnCategoria)
            {
                errores.Add("TaxCategory debe contener TaxScheme.");
            }
        }
    }
}
