using CapitalPos.Cpe.Api.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace CapitalPos.Cpe.Api.Infrastructure.Security;

public class DataProtectionSecretProtector : ISecretProtector
{
    private readonly IDataProtectionProvider _provider;

    public DataProtectionSecretProtector(IDataProtectionProvider provider)
    {
        _provider = provider;
    }

    public byte[] ProtectBytes(byte[] plainBytes, string purpose)
    {
        return _provider.CreateProtector(purpose).Protect(plainBytes);
    }

    public byte[] UnprotectBytes(byte[] cipherBytes, string purpose)
    {
        return _provider.CreateProtector(purpose).Unprotect(cipherBytes);
    }

    public byte[] ProtectString(string plainText, string purpose)
    {
        return _provider.CreateProtector(purpose).Protect(
            System.Text.Encoding.UTF8.GetBytes(plainText));
    }

    public string UnprotectString(byte[] cipherBytes, string purpose)
    {
        var bytes = _provider.CreateProtector(purpose).Unprotect(cipherBytes);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
}
