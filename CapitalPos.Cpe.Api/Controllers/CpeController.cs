using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Services;
using Microsoft.AspNetCore.Mvc;
using CapitalPos.Cpe.Api.Interfaces;
using CapitalPos.Cpe.Api.Domain;

namespace CapitalPos.Cpe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CpeController : ControllerBase
{
    private readonly CpeEmisionService _cpeEmisionService;
    private readonly ICpeSunatService _sunatService;
    private readonly ICpeFirmaService _firmaService;

    public CpeController(CpeEmisionService cpeEmisionService, ICpeSunatService sunatService, ICpeFirmaService firmaService)
    {
        _cpeEmisionService = cpeEmisionService;
        _sunatService = sunatService;
        _firmaService = firmaService;
    }

    [HttpPost("validar")]
    public IActionResult Validar([FromBody] EmitirCpeRequest request)
    {
        var data = _cpeEmisionService.ValidarComprobante(request);

        if (!data.Ok)
        {
            var errorResponse = ApiResponse<CpeValidacionResponse>.Fail(
                "El comprobante tiene errores de validación.",
                data.Errores
            );

            return BadRequest(errorResponse);
        }

        var response = ApiResponse<CpeValidacionResponse>.Success(
            "Comprobante validado correctamente.",
            data
        );

        return Ok(response);
    }
    
    [HttpPost("generar-xml")]
    public IActionResult GenerarXml([FromBody] EmitirCpeRequest request)
    {
        var data = _cpeEmisionService.GenerarXml(request);

        if (!data.Ok)
        {
            var errorResponse = ApiResponse<CpeXmlResponse>.Fail(
                "No se pudo generar el XML.",
                data.Errores
            );

            return BadRequest(errorResponse);
        }

        var response = ApiResponse<CpeXmlResponse>.Success(
            "XML generado correctamente.",
            data
        );

        return Ok(response);
    }
    
    [HttpPost("firmar-xml")]
    public IActionResult FirmarXml([FromBody] FirmarXmlRequest request)
    {
        var data = _firmaService.FirmarXml(request.NombreXml);

        if (!data.Ok)
        {
            var errores = data.Errores.Count > 0
                ? data.Errores
                : new List<string> { data.Mensaje };

            var errorResponse = ApiResponse<CpeFirmaResponse>.Fail(
                "No se pudo firmar el XML.",
                errores
            );

            return BadRequest(errorResponse);
        }

        var response = ApiResponse<CpeFirmaResponse>.Success(
            "XML firmado correctamente.",
            data
        );

        return Ok(response);
    }
    
    [HttpPost("enviar-sunat")]
    public IActionResult EnviarSunat([FromBody] EnviarSunatRequest request)
    {
        var data = _sunatService.EnviarComprobante(request.nombreZip);

        if (!data.Ok)
        {
            var errores = data.Errores.Count > 0
                ? data.Errores
                : new List<string> { data.MensajeSunat };

            var errorResponse = ApiResponse<CpeSunatResponse>.Fail(
                "No se pudo enviar el comprobante a SUNAT.",
                errores
            );

            return BadRequest(errorResponse);
        }

        var response = ApiResponse<CpeSunatResponse>.Success(
            "Comprobante enviado a SUNAT correctamente.",
            data
        );

        return Ok(response);
    }
    
    [HttpPost("emitir")]
    public IActionResult Emitir([FromBody] EmitirCpeRequest request)
    {
        var data = _cpeEmisionService.EmitirComprobanteSimulado(request);

        if (data.Ok)
        {
            var response = ApiResponse<CpeEmisionResponse>.Success(
                "Comprobante emitido correctamente.",
                data
            );

            return Ok(response);
        }

        if (data.Estado == CpeEstados.ErrorInterno)
        {
            var errorResponse = ApiResponse<CpeEmisionResponse>.Fail(
                "Ocurrió un error interno al emitir el comprobante.",
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