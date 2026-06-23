namespace CapitalPos.Cpe.Api.Settings;

public class CpeSecuritySettings
{
    public bool ApiKeyEnabled { get; set; } = true;

    public string HeaderName { get; set; } = "X-API-KEY";

    public string ApiKey { get; set; } = string.Empty;

    public List<string> RutasPublicas { get; set; } = new()
    {
        "/api/health"
    };
}