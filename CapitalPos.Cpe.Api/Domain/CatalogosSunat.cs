namespace CapitalPos.Cpe.Api.Domain;

public static class CatalogosSunat
{
    public static class TipoComprobante
    {
        public const string Factura = "01";
        public const string Boleta = "03";
        public const string NotaCredito = "07";
    }

    /// <summary>
    /// Catálogo No. 09 SUNAT — Códigos de tipo de nota de crédito electrónica.
    /// </summary>
    public static class MotivoNotaCredito
    {
        public const string AnulacionDeLaOperacion = "01";
        public const string AnulacionPorErrorEnElRuc = "02";
        public const string CorreccionPorErrorEnLaDescripcion = "03";
        public const string DescuentoGlobal = "04";
        public const string DescuentoPorItem = "05";
        public const string DevolucionTotal = "06";
        public const string DevolucionPorItem = "07";
        public const string Bonificacion = "08";
        public const string DisminucionEnElValor = "09";
        public const string OtrosConceptos = "10";
        public const string AjustesDeOperacionesDeExportacion = "11";
        public const string AjustesAfectosAlIvap = "12";
    }

    public static class TipoDocumentoIdentidad
    {
        public const string Dni = "1";
        public const string Ruc = "6";

        /// <summary>
        /// Guía de llenado SUNAT (boleta): adquirente no identificado.
        /// Catálogo 06 no lista este código; el XML usa schemeID="-" y el número "-".
        /// </summary>
        public const string SinIdentificacion = "-";

        public const string NumeroSinIdentificacion = "-";
    }

    /// <summary>
    /// Importe máximo de boleta en el que SUNAT no exige identificar al adquirente.
    /// </summary>
    public const decimal UmbralIdentificacionBoleta = 700m;

    public static class Moneda
    {
        public const string Soles = "PEN";
        public const string Dolares = "USD";
    }

    public static class TipoOperacion
    {
        public const string VentaInterna = "0101";
    }

    public static class AfectacionIgv
    {
        public const string GravadoOperacionOnerosa = "10";
        public const string ExoneradoOperacionOnerosa = "20";
        public const string InafectoOperacionOnerosa = "30";
    }

    public static class FormaPago
    {
        public const string Contado = "CONTADO";
        public const string Credito = "CREDITO";
    }
}