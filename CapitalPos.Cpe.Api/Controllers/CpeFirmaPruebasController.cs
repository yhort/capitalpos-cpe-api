using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CapitalPos.Cpe.Api.Controllers;

[ApiController]
[Route("api/cpe/pruebas/firma")]
public class CpeFirmaPruebasController : ControllerBase
{
    private readonly ICpeFirmaService _firmaService;

    public CpeFirmaPruebasController(ICpeFirmaService firmaService)
    {
        _firmaService = firmaService;
    }

    [HttpPost("{nombreXml}")]
    public IActionResult FirmarXmlExistente(string nombreXml)
    {
        var resultado = _firmaService.FirmarXml(nombreXml);

        if (!resultado.Ok)
        {
            return BadRequest(ApiResponse<CpeFirmaResponse>.Fail(
                "No se pudo firmar el XML.",
                resultado.Errores
            ));
        }

        return Ok(ApiResponse<CpeFirmaResponse>.Success(
            "XML firmado correctamente.",
            resultado
        ));
    }
}