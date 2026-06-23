using CapitalPos.Cpe.Api.Dtos;
namespace CapitalPos.Cpe.Api.Interfaces;

public interface ICpeSunatService
{
    CpeSunatResponse EnviarComprobante(string nombreZip);
}