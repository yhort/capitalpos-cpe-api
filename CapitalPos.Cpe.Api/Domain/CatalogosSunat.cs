namespace CapitalPos.Cpe.Api.Domain;

public static class CatalogosSunat
{
    public static class TipoComprobante
    {
        public const string Factura = "01";
        public const string Boleta = "03";
    }

    public static class TipoDocumentoIdentidad
    {
        public const string Dni = "1";
        public const string Ruc = "6";
    }

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