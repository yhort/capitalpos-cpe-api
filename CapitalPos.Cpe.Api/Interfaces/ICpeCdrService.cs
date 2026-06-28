using CapitalPos.Cpe.Api.Dtos;

namespace CapitalPos.Cpe.Api.Interfaces;

public interface ICpeCdrService
{
    CpeCdrResultadoDto LeerCdr(string nombreCdr);
}