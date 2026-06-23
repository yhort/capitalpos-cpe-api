using CapitalPos.Cpe.Api.Dtos;

namespace CapitalPos.Cpe.Api.Helpers;

public static class CpeNombreHelper
{
    public static string ObtenerRucEmisor(EmitirCpeRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Emisor?.Ruc))
            return request.Emisor.Ruc;

        return request.RucEmisor;
    }

    public static string ObtenerNumeroComprobante(EmitirCpeRequest request)
    {
        var rucEmisor = ObtenerRucEmisor(request);

        return $"{rucEmisor}-{request.TipoComprobante}-{request.Serie}-{request.Correlativo}";
    }

    public static string ObtenerNombreXml(EmitirCpeRequest request)
    {
        return $"{ObtenerNumeroComprobante(request)}.xml";
    }

    public static string ObtenerNombreZip(EmitirCpeRequest request)
    {
        return $"{ObtenerNumeroComprobante(request)}.zip";
    }

    public static string ObtenerNombreCdr(string nombreZip)
    {
        return $"R-{nombreZip}";
    }
}