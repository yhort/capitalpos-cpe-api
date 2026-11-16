namespace CapitalPos.Cpe.Api.Data.Entities;

public class EmisorCredencialSol
{
    public Guid Id { get; set; }
    public Guid EmisorId { get; set; }

    /// <summary>BETA o PRODUCCION.</summary>
    public string Ambiente { get; set; } = "BETA";

    public byte[] UsuarioSolCiphertext { get; set; } = Array.Empty<byte>();
    public byte[] ClaveSolCiphertext { get; set; } = Array.Empty<byte>();

    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    public Emisor Emisor { get; set; } = null!;
}
