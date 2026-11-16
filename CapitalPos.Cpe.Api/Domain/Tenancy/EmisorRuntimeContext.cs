namespace CapitalPos.Cpe.Api.Domain.Tenancy;

public class EmisorRuntimeContext
{
    public Guid EmisorId { get; init; }
    public string Ruc { get; init; } = string.Empty;
    public string Ambiente { get; init; } = string.Empty;
    public string ProveedorEnvio { get; init; } = string.Empty;

    public byte[] CertificadoPfx { get; init; } = Array.Empty<byte>();
    public string PasswordCertificado { get; init; } = string.Empty;

    public string UsuarioSol { get; init; } = string.Empty;
    public string ClaveSol { get; init; } = string.Empty;

    public string UrlSunat { get; init; } = string.Empty;

    /// <summary>True cuando el contexto se armó desde CpeSettings legacy.</summary>
    public bool EsLegacy { get; init; }
}
