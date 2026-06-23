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
    public IActionResult ListarHistorial([FromQuery] int take = 50)
    {
        if (take <= 0)
            take = 50;

        if (take > 200)
            take = 200;

        var historial = _historialService
            .ListarHistorial()
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
        [FromQuery] int take = 50)
    {
        var historial = _historialService.BuscarHistorial(
            comprobante,
            estado,
            cliente,
            desde,
            hasta,
            take
        );

        var response = ApiResponse<List<CpeHistorialItemDto>>.Success(
            "Búsqueda de historial realizada correctamente.",
            historial
        );

        return Ok(response);
    }

    [HttpGet("resumen")]
    public IActionResult ObtenerResumen()
    {
        var resumen = _historialService.ObtenerResumen();

        var response = ApiResponse<CpeHistorialResumenResponse>.Success(
            "Resumen del historial obtenido correctamente.",
            resumen
        );

        return Ok(response);
    }

    [HttpGet("{nombreArchivo}")]
    public IActionResult ObtenerHistorial(string nombreArchivo)
    {
        var historial = _historialService.ObtenerHistorial(nombreArchivo);

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