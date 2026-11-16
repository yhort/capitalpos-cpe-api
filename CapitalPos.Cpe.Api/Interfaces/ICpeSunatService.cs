using CapitalPos.Cpe.Api.Domain.Tenancy;
using CapitalPos.Cpe.Api.Dtos;

namespace CapitalPos.Cpe.Api.Interfaces;

public interface ICpeSunatService
{
    CpeSunatResponse EnviarComprobante(string nombreZip, EmisorRuntimeContext? context = null);
}
