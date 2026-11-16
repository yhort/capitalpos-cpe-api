using CapitalPos.Cpe.Api.Domain.Tenancy;
using CapitalPos.Cpe.Api.Interfaces;

namespace CapitalPos.Cpe.Api.Infrastructure.Tenancy;

public class EmisorContextAccessor : IEmisorContextAccessor
{
    public EmisorRuntimeContext? Current { get; set; }
}
