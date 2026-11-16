using CapitalPos.Cpe.Api.Domain.Tenancy;

namespace CapitalPos.Cpe.Api.Interfaces;

public interface IEmisorContextAccessor
{
    EmisorRuntimeContext? Current { get; set; }
}
