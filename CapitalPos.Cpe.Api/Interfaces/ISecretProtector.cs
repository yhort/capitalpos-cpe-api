namespace CapitalPos.Cpe.Api.Interfaces;

public interface ISecretProtector
{
    byte[] ProtectBytes(byte[] plainBytes, string purpose);
    byte[] UnprotectBytes(byte[] cipherBytes, string purpose);

    byte[] ProtectString(string plainText, string purpose);
    string UnprotectString(byte[] cipherBytes, string purpose);
}

public static class SecretProtectorPurposes
{
    public const string CertificadoPfx = "Cpe.Emisor.CertificadoPfx.v1";
    public const string CertificadoPassword = "Cpe.Emisor.CertificadoPassword.v1";
    public const string UsuarioSol = "Cpe.Emisor.UsuarioSol.v1";
    public const string ClaveSol = "Cpe.Emisor.ClaveSol.v1";
}
