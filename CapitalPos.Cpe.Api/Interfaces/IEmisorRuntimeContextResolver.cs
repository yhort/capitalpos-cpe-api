using CapitalPos.Cpe.Api.Domain.Tenancy;
using CapitalPos.Cpe.Api.Dtos;

namespace CapitalPos.Cpe.Api.Interfaces;

public interface IEmisorRuntimeContextResolver
{
    Task<EmisorRuntimeResolveResult> ResolveAsync(
        string ruc,
        string? ambienteOverride = null,
        CancellationToken cancellationToken = default);

    Task<EmisorRuntimeResolveResult> ResolveFromRequestAsync(
        EmitirCpeRequest request,
        CancellationToken cancellationToken = default);
}

public class EmisorRuntimeResolveResult
{
    public bool Ok { get; init; }
    public string CodigoError { get; init; } = string.Empty;
    public string Mensaje { get; init; } = string.Empty;
    public List<string> Errores { get; init; } = new();
    public EmisorRuntimeContext? Context { get; init; }

    public static EmisorRuntimeResolveResult Success(EmisorRuntimeContext context) => new()
    {
        Ok = true,
        Context = context
    };

    public static EmisorRuntimeResolveResult Fail(string codigo, string mensaje, params string[] errores) => new()
    {
        Ok = false,
        CodigoError = codigo,
        Mensaje = mensaje,
        Errores = errores.Length > 0 ? errores.ToList() : new List<string> { mensaje }
    };
}
