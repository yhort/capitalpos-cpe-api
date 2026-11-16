namespace CapitalPos.Cpe.Api.Data.Entities;

public class EmisorCertificado
{
    public Guid Id { get; set; }
    public Guid EmisorId { get; set; }

    public byte[] PfxCiphertext { get; set; } = Array.Empty<byte>();
    public byte[] PasswordCiphertext { get; set; } = Array.Empty<byte>();

    public string? HuellaThumbprint { get; set; }
    public DateTime? VigenteDesde { get; set; }
    public DateTime? VigenteHasta { get; set; }

    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    public Emisor Emisor { get; set; } = null!;
}
