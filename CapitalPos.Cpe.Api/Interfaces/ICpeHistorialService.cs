using CapitalPos.Cpe.Api.Domain.Tenancy;
using CapitalPos.Cpe.Api.Dtos;

namespace CapitalPos.Cpe.Api.Interfaces;

public interface ICpeHistorialService
{
    Task<CpeEmisionResponse?> ObtenerEmisionExistenteAsync(
        string ruc,
        string tipoComprobante,
        string serie,
        int correlativo,
        CancellationToken cancellationToken = default);

    Task<string> GuardarHistorialAsync(
        EmitirCpeRequest request,
        CpeEmisionResponse response,
        EmisorRuntimeContext? context,
        string? correlationId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Compatibilidad con callers síncronos existentes.</summary>
    string GuardarHistorial(EmitirCpeRequest request, CpeEmisionResponse response);

    List<CpeHistorialItemDto> ListarHistorial(string? ruc = null);

    CpeHistorialItemDto? ObtenerHistorial(string idONombre, string? ruc = null);

    List<CpeHistorialItemDto> BuscarHistorial(
        string? comprobante,
        string? estado,
        string? cliente,
        DateTime? desde,
        DateTime? hasta,
        int take,
        string? ruc = null);

    CpeHistorialResumenResponse ObtenerResumen(string? ruc = null);
}
