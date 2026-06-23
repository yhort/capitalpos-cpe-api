using CapitalPos.Cpe.Api.Dtos;
namespace CapitalPos.Cpe.Api.Interfaces;

public interface ICpeFirmaService
{
    CpeFirmaResponse FirmarXml(string nombreXml);
}