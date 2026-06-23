using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CapitalPos.Cpe.Api.Controllers;

[ApiController]
[Route("api/cpe/diagnostico")]
public class DiagnosticoController : ControllerBase
{
    private readonly CpeDiagnosticoService _diagnosticoService;

    public DiagnosticoController(CpeDiagnosticoService diagnosticoService)
    {
        _diagnosticoService = diagnosticoService;
    }

    [HttpGet]
    public IActionResult Revisar()
    {
        var data = _diagnosticoService.Revisar();

        if (!data.Ok)
        {
            var errores = data.Checks
                .Where(c => !c.Ok)
                .Select(c => $"{c.Nombre}: {c.Mensaje}")
                .ToList();

            var errorResponse = ApiResponse<CpeDiagnosticoResponse>.Fail(
                "El diagnóstico CPE encontró observaciones.",
                errores
            );

            errorResponse.Data = data;

            return BadRequest(errorResponse);
        }

        var response = ApiResponse<CpeDiagnosticoResponse>.Success(
            "Diagnóstico CPE correcto.",
            data
        );

        return Ok(response);
    }

    [HttpPost("preparar-carpetas")]
    public IActionResult PrepararCarpetas()
    {
        var data = _diagnosticoService.PrepararCarpetas();

        if (!data.Ok)
        {
            var errores = data.Checks
                .Where(c => !c.Ok)
                .Select(c => $"{c.Nombre}: {c.Mensaje}")
                .ToList();

            var errorResponse = ApiResponse<CpeDiagnosticoResponse>.Fail(
                "No se pudieron preparar todas las carpetas CPE.",
                errores
            );

            errorResponse.Data = data;

            return BadRequest(errorResponse);
        }

        var response = ApiResponse<CpeDiagnosticoResponse>.Success(
            "Carpetas CPE preparadas correctamente.",
            data
        );

        return Ok(response);
    }
}