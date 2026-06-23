using System.Security.Cryptography;
using System.Text;
using CapitalPos.Cpe.Api.Interfaces;

namespace CapitalPos.Cpe.Api.Services;

public class CpeHashService : ICpeHashService
{
    public string CalcularHashSha256(string contenido)
    {
        if (string.IsNullOrWhiteSpace(contenido))
            return string.Empty;

        var bytes = Encoding.UTF8.GetBytes(contenido);
        var hashBytes = SHA256.HashData(bytes);

        return Convert.ToHexString(hashBytes);
    }
}