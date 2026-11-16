namespace CapitalPos.Cpe.Api.Domain;

public static class CpeAmbientes
{
    public const string Beta = "BETA";
    public const string Produccion = "PRODUCCION";

    public static bool EsValido(string? ambiente)
    {
        if (string.IsNullOrWhiteSpace(ambiente))
            return false;

        var normalizado = ambiente.Trim().ToUpperInvariant();
        return normalizado is Beta or Produccion;
    }

    public static string Normalizar(string ambiente)
    {
        return ambiente.Trim().ToUpperInvariant();
    }
}

public static class CpeProveedoresEnvio
{
    public const string SunatDirecta = "SunatDirecta";
}
