using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CapitalPos.Cpe.Api.Controllers;

[ApiController]
[Route("api/cpe/config")]
public class ConfigController : ControllerBase
{
    private readonly CpeConfigService _configService;

    public ConfigController(CpeConfigService configService)
    {
        _configService = configService;
    }

    [HttpGet]
    public IActionResult ObtenerConfiguracion()
    {
        var data = _configService.ObtenerConfiguracion();

        var response = ApiResponse<CpeConfigResponse>.Success(
            "Configuración CPE obtenida correctamente.",
            data
        );

        return Ok(response);
    }

    [HttpGet("validar")]
    public IActionResult ValidarConfiguracion()
    {
        var data = _configService.ValidarConfiguracion();

        if (!data.Ok)
        {
            var errorResponse = ApiResponse<CpeConfigValidationResponse>.Fail(
                data.Mensaje,
                data.Errores
            );

            return BadRequest(errorResponse);
        }

        var response = ApiResponse<CpeConfigValidationResponse>.Success(
            "Configuración CPE validada correctamente.",
            data
        );

        return Ok(response);
    }
}