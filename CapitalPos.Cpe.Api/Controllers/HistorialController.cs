using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CapitalPos.Cpe.Api.Controllers;

[ApiController]
[Route("api/cpe/historial")]
public class HistorialController : ControllerBase
{
    private readonly ICpeHistorialService _historialService;

    public HistorialController(ICpeHistorialService historialService)
    {
        _historialService = historialService;
    }

    [HttpGet]
    public IActionResult ListarHistorial([FromQuery] int take = 50, [FromQuery] string? ruc = null)
    {
        if (take <= 0)
            take = 50;

        if (take > 200)
            take = 200;

        var historial = _historialService
            .ListarHistorial(ruc)
            .Take(take)
            .ToList();

        var response = ApiResponse<List<CpeHistorialItemDto>>.Success(
            "Historial de emisiones obtenido correctamente.",
            historial
        );

        return Ok(response);
    }

    [HttpGet("buscar")]
    public IActionResult BuscarHistorial(
        [FromQuery] string? comprobante,
        [FromQuery] string? estado,
        [FromQuery] string? cliente,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] int take = 50,
        [FromQuery] string? ruc = null)
    {
        var historial = _historialService.BuscarHistorial(
            comprobante,
            estado,
            cliente,
            desde,
            hasta,
            take,
            ruc
        );

        var response = ApiResponse<List<CpeHistorialItemDto>>.Success(
            "Búsqueda de historial realizada correctamente.",
            historial
        );

        return Ok(response);
    }

    [HttpGet("resumen")]
    public IActionResult ObtenerResumen([FromQuery] string? ruc = null)
    {
        var resumen = _historialService.ObtenerResumen(ruc);

        var response = ApiResponse<CpeHistorialResumenResponse>.Success(
            "Resumen del historial obtenido correctamente.",
            resumen
        );

        return Ok(response);
    }

    [HttpGet("{nombreArchivo}")]
    public IActionResult ObtenerHistorial(string nombreArchivo, [FromQuery] string? ruc = null)
    {
        var historial = _historialService.ObtenerHistorial(nombreArchivo, ruc);

        if (historial == null)
        {
            var errorResponse = ApiResponse<CpeHistorialItemDto>.Fail(
                "No se encontró el historial solicitado.",
                $"No existe el archivo de historial: {nombreArchivo}"
            );

            return NotFound(errorResponse);
        }

        var response = ApiResponse<CpeHistorialItemDto>.Success(
            "Historial de emisión obtenido correctamente.",
            historial
        );

        return Ok(response);
    }
}
