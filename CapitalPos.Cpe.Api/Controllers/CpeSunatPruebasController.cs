using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CapitalPos.Cpe.Api.Controllers;

[ApiController]
[Route("api/cpe/pruebas/sunat")]
public class CpeSunatPruebasController : ControllerBase
{
    private readonly ICpeSunatService _sunatService;

    public CpeSunatPruebasController(ICpeSunatService sunatService)
    {
        _sunatService = sunatService;
    }

    [HttpPost("{nombreZip}")]
    public IActionResult EnviarZipExistente(string nombreZip)
    {
        var resultado = _sunatService.EnviarComprobante(nombreZip);

        if (!resultado.Ok)
        {
            return BadRequest(ApiResponse<CpeSunatResponse>.Fail(
                "No se pudo enviar el ZIP a SUNAT.",
                resultado.Errores
            ));
        }

        return Ok(ApiResponse<CpeSunatResponse>.Success(
            "ZIP enviado a SUNAT correctamente.",
            resultado
        ));
    }
}