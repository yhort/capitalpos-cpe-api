using CapitalPos.Cpe.Api.Dtos;

namespace CapitalPos.Cpe.Api.Interfaces;

public interface IEmisorAdminService
{
    Task<EmisorDtoResponse> RegistrarEmisorAsync(RegistrarEmisorRequest request, CancellationToken ct = default);
    Task<EmisorDtoResponse?> ActualizarEmisorAsync(string ruc, ActualizarEmisorRequest request, CancellationToken ct = default);
    Task<EmisorDtoResponse?> ObtenerEmisorAsync(string ruc, CancellationToken ct = default);
    Task<List<EmisorDtoResponse>> ListarEmisoresAsync(CancellationToken ct = default);
    Task<EmisorOnboardingStatusDto?> ObtenerOnboardingAsync(string ruc, CancellationToken ct = default);

    Task<EmisorCertificadoInfoDto> RegistrarCertificadoAsync(
        string ruc,
        byte[] pfxBytes,
        string password,
        CancellationToken ct = default);

    Task<EmisorCertificadoInfoDto?> ObtenerCertificadoActivoAsync(string ruc, CancellationToken ct = default);

    Task RegistrarCredencialSolAsync(string ruc, RegistrarCredencialSolRequest request, CancellationToken ct = default);
}
