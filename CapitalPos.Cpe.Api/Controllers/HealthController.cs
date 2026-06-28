using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CapitalPos.Cpe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly CpeSettings _settings;

    public HealthController(IOptions<CpeSettings> options)
    {
        _settings = options.Value;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var data = new HealthResponse
        {
            Status = "OK",
            Service = "CapitalPOS CPE API",
            Version = "1.0.0",

            Modo = _settings.Modo,
            SimularGeneracionXml = _settings.SimularGeneracionXml,
            SimularFirma = _settings.SimularFirma,
            SimularEnvioSunat = _settings.SimularEnvioSunat,
            GuardarCdrSimulado = _settings.GuardarCdrSimulado,

            RutaArchivos = _settings.RutaArchivos,
            RutaArchivosAbsoluta = Path.GetFullPath(_settings.RutaArchivos),
            FechaServidor = DateTime.Now
        };

        var response = ApiResponse<HealthResponse>.Success(
            "API CPE funcionando correctamente.",
            data
        );

        return Ok(response);
    }

    
}