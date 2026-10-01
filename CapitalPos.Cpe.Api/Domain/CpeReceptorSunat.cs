using CapitalPos.Cpe.Api.Dtos;

namespace CapitalPos.Cpe.Api.Domain;

/// <summary>
/// Normaliza el receptor de boletas menores o iguales a S/ 700 cuando no hay documento
/// de identidad (consumidor final / clientes varios).
/// </summary>
public static class CpeReceptorSunat
{
    public static bool EsBoletaSinIdentificacionObligatoria(string? tipoComprobante, decimal total) =>
        tipoComprobante == CatalogosSunat.TipoComprobante.Boleta
        && total <= CatalogosSunat.UmbralIdentificacionBoleta;

    public static bool EsSinIdentificacion(ClienteDto? cliente)
    {
        if (cliente is null)
        {
            return true;
        }

        var numero = (cliente.NumeroDocumento ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(numero)
            || numero == CatalogosSunat.TipoDocumentoIdentidad.NumeroSinIdentificacion
            || numero == "00000000")
        {
            return true;
        }

        var tipo = (cliente.TipoDocumento ?? string.Empty).Trim();
        return string.IsNullOrEmpty(tipo)
               || tipo == CatalogosSunat.TipoDocumentoIdentidad.SinIdentificacion;
    }

    public static void NormalizarSiCorresponde(EmitirCpeRequest request)
    {
        if (request.Cliente is null)
        {
            return;
        }

        if (!EsBoletaSinIdentificacionObligatoria(request.TipoComprobante, request.Total))
        {
            return;
        }

        if (!EsSinIdentificacion(request.Cliente))
        {
            return;
        }

        request.Cliente.TipoDocumento = CatalogosSunat.TipoDocumentoIdentidad.SinIdentificacion;
        request.Cliente.NumeroDocumento = CatalogosSunat.TipoDocumentoIdentidad.NumeroSinIdentificacion;
    }
}
