using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CapitalPos.Cpe.Api.Controllers;

[ApiController]
[Route("api/emisores")]
public class EmisoresController : ControllerBase
{
    private readonly IEmisorAdminService _emisorAdminService;

    public EmisoresController(IEmisorAdminService emisorAdminService)
    {
        _emisorAdminService = emisorAdminService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var data = await _emisorAdminService.ListarEmisoresAsync(ct);
        return Ok(ApiResponse<List<EmisorDtoResponse>>.Success(
            "Emisores obtenidos correctamente.",
            data));
    }

    [HttpGet("{ruc}")]
    public async Task<IActionResult> Obtener(string ruc, CancellationToken ct)
    {
        try
        {
            var data = await _emisorAdminService.ObtenerEmisorAsync(ruc, ct);
            if (data == null)
            {
                return NotFound(ApiResponse<EmisorDtoResponse>.Fail(
                    "Emisor no encontrado.",
                    $"No existe emisor con RUC {ruc}."));
            }

            return Ok(ApiResponse<EmisorDtoResponse>.Success("Emisor obtenido correctamente.", data));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<EmisorDtoResponse>.Fail(ex.Message, ex.Message));
        }
    }

    [HttpGet("{ruc}/onboarding")]
    public async Task<IActionResult> Onboarding(string ruc, CancellationToken ct)
    {
        try
        {
            var data = await _emisorAdminService.ObtenerOnboardingAsync(ruc, ct);
            if (data == null)
            {
                return NotFound(ApiResponse<EmisorOnboardingStatusDto>.Fail(
                    "Emisor no encontrado.",
                    $"No existe emisor con RUC {ruc}."));
            }

            return Ok(ApiResponse<EmisorOnboardingStatusDto>.Success(
                "Estado de onboarding obtenido correctamente.",
                data));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<EmisorOnboardingStatusDto>.Fail(ex.Message, ex.Message));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarEmisorRequest request, CancellationToken ct)
    {
        try
        {
            var data = await _emisorAdminService.RegistrarEmisorAsync(request, ct);
            return Ok(ApiResponse<EmisorDtoResponse>.Success("Emisor registrado correctamente.", data));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<EmisorDtoResponse>.Fail(ex.Message, ex.Message));
        }
    }

    [HttpPut("{ruc}")]
    public async Task<IActionResult> Actualizar(
        string ruc,
        [FromBody] ActualizarEmisorRequest request,
        CancellationToken ct)
    {
        try
        {
            var data = await _emisorAdminService.ActualizarEmisorAsync(ruc, request, ct);
            if (data == null)
            {
                return NotFound(ApiResponse<EmisorDtoResponse>.Fail(
                    "Emisor no encontrado.",
                    $"No existe emisor con RUC {ruc}."));
            }

            return Ok(ApiResponse<EmisorDtoResponse>.Success("Emisor actualizado correctamente.", data));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<EmisorDtoResponse>.Fail(ex.Message, ex.Message));
        }
    }

    [HttpPost("{ruc}/certificado")]
    [RequestSizeLimit(5_000_000)]
    public async Task<IActionResult> RegistrarCertificado(
        string ruc,
        IFormFile archivo,
        [FromForm] string password,
        CancellationToken ct)
    {
        try
        {
            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest(ApiResponse<EmisorCertificadoInfoDto>.Fail(
                    "Archivo PFX obligatorio.",
                    "Debe enviar el archivo del certificado (.pfx / .p12)."));
            }

            await using var stream = archivo.OpenReadStream();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, ct);

            var data = await _emisorAdminService.RegistrarCertificadoAsync(
                ruc,
                ms.ToArray(),
                password,
                ct);

            return Ok(ApiResponse<EmisorCertificadoInfoDto>.Success(
                "Certificado registrado correctamente.",
                data));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<EmisorCertificadoInfoDto>.Fail(ex.Message, ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<EmisorCertificadoInfoDto>.Fail(
                "No se pudo registrar el certificado.",
                ex.Message));
        }
    }

    [HttpGet("{ruc}/certificado")]
    public async Task<IActionResult> ObtenerCertificado(string ruc, CancellationToken ct)
    {
        try
        {
            var data = await _emisorAdminService.ObtenerCertificadoActivoAsync(ruc, ct);
            if (data == null)
            {
                return NotFound(ApiResponse<EmisorCertificadoInfoDto>.Fail(
                    "Certificado no encontrado.",
                    $"No hay certificado activo para el RUC {ruc}."));
            }

            return Ok(ApiResponse<EmisorCertificadoInfoDto>.Success(
                "Certificado activo obtenido correctamente.",
                data));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<EmisorCertificadoInfoDto>.Fail(ex.Message, ex.Message));
        }
    }

    [HttpPost("{ruc}/credenciales-sol")]
    public async Task<IActionResult> RegistrarCredencialSol(
        string ruc,
        [FromBody] RegistrarCredencialSolRequest request,
        CancellationToken ct)
    {
        try
        {
            await _emisorAdminService.RegistrarCredencialSolAsync(ruc, request, ct);
            return Ok(ApiResponse<object?>.Success(
                "Credenciales SOL registradas correctamente.",
                null));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object?>.Fail(ex.Message, ex.Message));
        }
    }
}
