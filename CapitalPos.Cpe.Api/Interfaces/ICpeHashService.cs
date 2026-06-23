namespace CapitalPos.Cpe.Api.Interfaces;

public interface ICpeHashService
{
    string CalcularHashSha256(string contenido);
}