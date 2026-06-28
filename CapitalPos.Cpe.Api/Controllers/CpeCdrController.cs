using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CapitalPos.Cpe.Api.Controllers;

[ApiController]
[Route("api/cpe/cdr")]
public class CpeCdrController : ControllerBase
{
    private readonly ICpeCdrService _cdrService;

    public CpeCdrController(ICpeCdrService cdrService)
    {
        _cdrService = cdrService;
    }

    [HttpGet("{nombreCdr}")]
    public IActionResult LeerCdr(string nombreCdr)
    {
        var resultado = _cdrService.LeerCdr(nombreCdr);

        if (!resultado.Ok)
        {
            return BadRequest(ApiResponse<CpeCdrResultadoDto>.Fail(
                "No se pudo leer el CDR.",
                resultado.Errores
            ));
        }

        return Ok(ApiResponse<CpeCdrResultadoDto>.Success(
            "CDR leído correctamente.",
            resultado
        ));
    }
}