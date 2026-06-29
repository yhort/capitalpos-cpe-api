using CapitalPos.Cpe.Api.Domain;
using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Services;
using CapitalPos.Cpe.Api.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CapitalPos.Cpe.Api.Controllers;

[ApiController]
[Route("api/cpe/pruebas")]
public class PruebasController : ControllerBase
{
    private readonly CpeDemoService _demoService;
    private readonly CpeEmisionService _emisionService;
    private readonly CpeSettings _settings;

    public PruebasController(
        CpeDemoService demoService,
        CpeEmisionService emisionService,
        IOptions<CpeSettings> options)
    {
        _demoService = demoService;
        _emisionService = emisionService;
        _settings = options.Value;
    }

    [HttpGet("request-demo")]
    public IActionResult ObtenerRequestDemo()
    {
        var data = _demoService.CrearBoletaDemo();

        var response = ApiResponse<EmitirCpeRequest>.Success(
            "Request demo generado correctamente.",
            data
        );

        return Ok(response);
    }

    [HttpPost("emitir-demo")]
    public IActionResult EmitirDemo()
    {
        if (!_settings.SimularEnvioSunat)
        {
            var errorResponse = ApiResponse<CpeEmisionResponse>.Fail(
                "No se puede emitir demo porque el envío a SUNAT real está activado.",
                new List<string>
                {
                    "Para usar este endpoint, SimularEnvioSunat debe estar en true. La firma puede ser simulada o real."
                }
            );

            return BadRequest(errorResponse);
        }

        var request = _demoService.CrearBoletaDemo();
        var data = _emisionService.EmitirComprobanteSimulado(request);

        if (data.Ok)
        {
            var response = ApiResponse<CpeEmisionResponse>.Success(
                "Comprobante demo emitido correctamente.",
                data
            );

            return Ok(response);
        }

        if (data.Estado == CpeEstados.ErrorInterno)
        {
            var errorResponse = ApiResponse<CpeEmisionResponse>.Fail(
                "Ocurrió un error interno al emitir el comprobante demo.",
                data.Errores
            );

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }

        var badResponse = ApiResponse<CpeEmisionResponse>.Fail(
            data.Mensaje,
            data.Errores
        );

        return BadRequest(badResponse);
    }
}