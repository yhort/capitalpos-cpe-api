using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CapitalPos.Cpe.Api.Controllers;

[ApiController]
[Route("api/cpe/archivos")]
public class ArchivosController : ControllerBase
{
    private readonly ICpeStorageService _storageService;
    private readonly ICpeXmlValidatorService _xmlValidatorService;
    private readonly ICpeXmlResumenService _xmlResumenService;

    public ArchivosController(ICpeStorageService storageService, ICpeXmlValidatorService xmlValidatorService,
        ICpeXmlResumenService xmlResumenService)
    {
        _storageService = storageService;
        _xmlValidatorService = xmlValidatorService;
        _xmlResumenService = xmlResumenService;
    }

    [HttpGet("resumen")]
    public IActionResult ObtenerResumenArchivos([FromQuery] string? ruc = null)
    {
        var data = _storageService.ObtenerResumenArchivos(ruc);

        var response = ApiResponse<CpeArchivosResumenDto>.Success(
            "Resumen de archivos CPE obtenido correctamente.",
            data
        );

        return Ok(response);
    }

    // =========================
    // XML
    // =========================


    [HttpGet("xml")]
    public IActionResult ListarXml([FromQuery] string? ruc = null)
    {
        var data = _storageService.ListarXml(ruc);

        var response = ApiResponse<List<string>>.Success(
            "Archivos XML obtenidos correctamente.",
            data
        );

        return Ok(response);
    }

    [HttpGet("xml/info")]
    public IActionResult ListarXmlInfo([FromQuery] string? ruc = null)
    {
        var data = _storageService.ListarXmlInfo(ruc);

        var response = ApiResponse<List<CpeArchivoInfoDto>>.Success(
            "Información de archivos XML obtenida correctamente.",
            data
        );

        return Ok(response);
    }

    [HttpGet("xml/{nombreArchivo}")]
    public IActionResult VerXml(string nombreArchivo, [FromQuery] string? ruc = null)
    {
        var contenidoXml = _storageService.LeerXml(nombreArchivo, ruc);

        if (contenidoXml == null)
        {
            var errorResponse = ApiResponse<string>.Fail(
                "No se encontró el archivo XML.",
                $"No existe el archivo XML: {nombreArchivo}"
            );

            return NotFound(errorResponse);
        }

        return Content(contenidoXml, "application/xml");
    }

    [HttpGet("xml/{nombreArchivo}/descargar")]
    public IActionResult DescargarXml(string nombreArchivo, [FromQuery] string? ruc = null)
    {
        var ruta = _storageService.ObtenerRutaXml(nombreArchivo, ruc);

        if (ruta == null)
        {
            var errorResponse = ApiResponse<string>.Fail(
                "No se encontró el archivo XML.",
                $"No existe el archivo XML: {nombreArchivo}"
            );

            return NotFound(errorResponse);
        }

        var bytes = System.IO.File.ReadAllBytes(ruta);

        return File(bytes, "application/xml", nombreArchivo);
    }

    [HttpGet("xml/{nombreArchivo}/validar")]
    public IActionResult ValidarXmlGuardado(string nombreArchivo, [FromQuery] string? ruc = null)
    {
        var contenidoXml = _storageService.LeerXml(nombreArchivo, ruc);

        if (contenidoXml == null)
        {
            var errorResponse = ApiResponse<CpeXmlValidacionResponse>.Fail(
                "No se encontró el archivo XML.",
                $"No existe el archivo XML: {nombreArchivo}"
            );

            return NotFound(errorResponse);
        }

        var data = _xmlValidatorService.ValidarXml(contenidoXml);

        if (!data.Ok)
        {
            var errorResponse = ApiResponse<CpeXmlValidacionResponse>.Fail(
                "El XML tiene errores.",
                data.Errores
            );

            errorResponse.Data = data;

            return BadRequest(errorResponse);
        }

        var response = ApiResponse<CpeXmlValidacionResponse>.Success(
            "El XML está bien formado.",
            data
        );

        return Ok(response);
    }

    [HttpGet("xml/{nombreArchivo}/resumen")]
    public IActionResult ObtenerResumenXmlGuardado(string nombreArchivo, [FromQuery] string? ruc = null)
    {
        var contenidoXml = _storageService.LeerXml(nombreArchivo, ruc);

        if (contenidoXml == null)
        {
            var errorResponse = ApiResponse<CpeXmlResumenResponse>.Fail(
                "No se encontró el archivo XML.",
                $"No existe el archivo XML: {nombreArchivo}"
            );

            return NotFound(errorResponse);
        }

        var data = _xmlResumenService.ObtenerResumen(contenidoXml);

        if (!data.Ok)
        {
            var errorResponse = ApiResponse<CpeXmlResumenResponse>.Fail(
                "No se pudo obtener el resumen del XML.",
                data.Errores
            );

            errorResponse.Data = data;

            return BadRequest(errorResponse);
        }

        var response = ApiResponse<CpeXmlResumenResponse>.Success(
            "Resumen del XML obtenido correctamente.",
            data
        );

        return Ok(response);
    }

    // =========================
    // ZIP
    // =========================

    [HttpGet("zip")]
    public IActionResult ListarZip([FromQuery] string? ruc = null)
    {
        var data = _storageService.ListarZip(ruc);

        var response = ApiResponse<List<string>>.Success(
            "Archivos ZIP obtenidos correctamente.",
            data
        );

        return Ok(response);
    }

    [HttpGet("zip/info")]
    public IActionResult ListarZipInfo([FromQuery] string? ruc = null)
    {
        var data = _storageService.ListarZipInfo(ruc);

        var response = ApiResponse<List<CpeArchivoInfoDto>>.Success(
            "Información de archivos ZIP obtenida correctamente.",
            data
        );

        return Ok(response);
    }

    [HttpGet("zip/{nombreArchivo}/descargar")]
    public IActionResult DescargarZip(string nombreArchivo, [FromQuery] string? ruc = null)
    {
        var ruta = _storageService.ObtenerRutaZip(nombreArchivo, ruc);

        if (ruta == null)
        {
            var errorResponse = ApiResponse<string>.Fail(
                "No se encontró el archivo ZIP.",
                $"No existe el archivo ZIP: {nombreArchivo}"
            );

            return NotFound(errorResponse);
        }

        var bytes = System.IO.File.ReadAllBytes(ruta);

        return File(bytes, "application/zip", nombreArchivo);
    }

    // =========================
    // CDR
    // =========================

    [HttpGet("cdr")]
    public IActionResult ListarCdr([FromQuery] string? ruc = null)
    {
        var data = _storageService.ListarCdr(ruc);

        var response = ApiResponse<List<string>>.Success(
            "Archivos CDR obtenidos correctamente.",
            data
        );

        return Ok(response);
    }

    [HttpGet("cdr/info")]
    public IActionResult ListarCdrInfo([FromQuery] string? ruc = null)
    {
        var data = _storageService.ListarCdrInfo(ruc);

        var response = ApiResponse<List<CpeArchivoInfoDto>>.Success(
            "Información de archivos CDR obtenida correctamente.",
            data
        );

        return Ok(response);
    }

    [HttpGet("cdr/{nombreArchivo}/descargar")]
    public IActionResult DescargarCdr(string nombreArchivo, [FromQuery] string? ruc = null)
    {
        var ruta = _storageService.ObtenerRutaCdr(nombreArchivo, ruc);

        if (ruta == null)
        {
            var errorResponse = ApiResponse<string>.Fail(
                "No se encontró el archivo CDR.",
                $"No existe el archivo CDR: {nombreArchivo}"
            );

            return NotFound(errorResponse);
        }

        var bytes = System.IO.File.ReadAllBytes(ruta);

        return File(bytes, "application/zip", nombreArchivo);
    }

    [HttpPost("limpiar-antiguos")]
    public IActionResult LimpiarArchivosAntiguos(
        [FromQuery] int dias = 7,
        [FromQuery] bool ejecutar = false,
        [FromQuery] string? ruc = null)
    {
        if (dias <= 0)
        {
            var errorResponse = ApiResponse<CpeLimpiezaArchivosResponse>.Fail(
                "Los días de antigüedad no son válidos.",
                "El parámetro dias debe ser mayor a 0."
            );

            return BadRequest(errorResponse);
        }

        var data = _storageService.LimpiarArchivosAntiguos(dias, ejecutar, ruc);

        var mensaje = ejecutar
            ? "Limpieza de archivos antiguos ejecutada correctamente."
            : "Simulación de limpieza realizada correctamente. No se eliminó ningún archivo.";

        var response = ApiResponse<CpeLimpiezaArchivosResponse>.Success(
            mensaje,
            data
        );

        return Ok(response);
    }
}