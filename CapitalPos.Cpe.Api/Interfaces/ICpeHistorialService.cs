using CapitalPos.Cpe.Api.Dtos;

namespace CapitalPos.Cpe.Api.Interfaces;

public interface ICpeHistorialService
{
    string GuardarHistorial(EmitirCpeRequest request, CpeEmisionResponse response);

    List<CpeHistorialItemDto> ListarHistorial();

    CpeHistorialItemDto? ObtenerHistorial(string nombreArchivo);

    List<CpeHistorialItemDto> BuscarHistorial(
        string? comprobante,
        string? estado,
        string? cliente,
        DateTime? desde,
        DateTime? hasta,
        int take);

    CpeHistorialResumenResponse ObtenerResumen();
}