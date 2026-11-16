using CapitalPos.Cpe.Api.Domain.Tenancy;
using CapitalPos.Cpe.Api.Dtos;

namespace CapitalPos.Cpe.Api.Interfaces;

public interface ICpeFirmaService
{
    CpeFirmaResponse FirmarXml(string nombreXml, EmisorRuntimeContext? context = null);
}
