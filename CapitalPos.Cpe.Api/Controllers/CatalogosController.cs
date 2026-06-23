using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CapitalPos.Cpe.Api.Controllers;

[ApiController]
[Route("api/cpe/catalogos")]
public class CatalogosController : ControllerBase
{
    private readonly CatalogosService _catalogosService;

    public CatalogosController(CatalogosService catalogosService)
    {
        _catalogosService = catalogosService;
    }

    [HttpGet("tipos-comprobante")]
    public IActionResult ObtenerTiposComprobante()
    {
        var data = _catalogosService.ObtenerTiposComprobante();

        var response = ApiResponse<List<CatalogoItemDto>>.Success(
            "Tipos de comprobante obtenidos correctamente.",
            data
        );

        return Ok(response);
    }

    [HttpGet("tipos-documento-identidad")]
    public IActionResult ObtenerTiposDocumentoIdentidad()
    {
        var data = _catalogosService.ObtenerTiposDocumentoIdentidad();

        var response = ApiResponse<List<CatalogoItemDto>>.Success(
            "Tipos de documento de identidad obtenidos correctamente.",
            data
        );

        return Ok(response);
    }

    [HttpGet("monedas")]
    public IActionResult ObtenerMonedas()
    {
        var data = _catalogosService.ObtenerMonedas();

        var response = ApiResponse<List<CatalogoItemDto>>.Success(
            "Monedas obtenidas correctamente.",
            data
        );

        return Ok(response);
    }

    [HttpGet("tipos-operacion")]
    public IActionResult ObtenerTiposOperacion()
    {
        var data = _catalogosService.ObtenerTiposOperacion();

        var response = ApiResponse<List<CatalogoItemDto>>.Success(
            "Tipos de operación obtenidos correctamente.",
            data
        );

        return Ok(response);
    }

    [HttpGet("afectaciones-igv")]
    public IActionResult ObtenerAfectacionesIgv()
    {
        var data = _catalogosService.ObtenerAfectacionesIgv();

        var response = ApiResponse<List<CatalogoItemDto>>.Success(
            "Afectaciones IGV obtenidas correctamente.",
            data
        );

        return Ok(response);
    }

    [HttpGet("formas-pago")]
    public IActionResult ObtenerFormasPago()
    {
        var data = _catalogosService.ObtenerFormasPago();

        var response = ApiResponse<List<CatalogoItemDto>>.Success(
            "Formas de pago obtenidas correctamente.",
            data
        );

        return Ok(response);
    }
    
    [HttpGet("estados-cpe")]
    public IActionResult ObtenerEstadosCpe()
    {
        var data = _catalogosService.ObtenerEstadosCpe();

        var response = ApiResponse<List<CatalogoItemDto>>.Success(
            "Estados CPE obtenidos correctamente.",
            data
        );

        return Ok(response);
    }

    [HttpGet("etapas-cpe")]
    public IActionResult ObtenerEtapasCpe()
    {
        var data = _catalogosService.ObtenerEtapasCpe();

        var response = ApiResponse<List<CatalogoItemDto>>.Success(
            "Etapas CPE obtenidas correctamente.",
            data
        );

        return Ok(response);
    }
}