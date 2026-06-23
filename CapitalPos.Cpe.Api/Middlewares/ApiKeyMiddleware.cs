using System.Text.Json;
using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Settings;
using Microsoft.Extensions.Options;

namespace CapitalPos.Cpe.Api.Middlewares;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyMiddleware> _logger;
    private readonly CpeSecuritySettings _settings;

    public ApiKeyMiddleware(
        RequestDelegate next,
        ILogger<ApiKeyMiddleware> logger,
        IOptions<CpeSecuritySettings> options)
    {
        _next = next;
        _logger = logger;
        _settings = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_settings.ApiKeyEnabled)
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;

        if (EsRutaPublica(path))
        {
            await _next(context);
            return;
        }

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            _logger.LogError("API Key habilitada, pero no configurada.");

            await EscribirRespuestaJson(
                context,
                StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail(
                    "Error de configuración de seguridad.",
                    "API Key no configurada en el servidor."
                )
            );

            return;
        }

        if (!context.Request.Headers.TryGetValue(_settings.HeaderName, out var apiKeyRecibida))
        {
            _logger.LogWarning("Solicitud rechazada. No se envió API Key. Ruta: {Ruta}", path);

            await EscribirRespuestaJson(
                context,
                StatusCodes.Status401Unauthorized,
                ApiResponse<object>.Fail(
                    "No autorizado.",
                    $"Debe enviar el header {_settings.HeaderName}."
                )
            );

            return;
        }

        if (apiKeyRecibida != _settings.ApiKey)
        {
            _logger.LogWarning("Solicitud rechazada. API Key inválida. Ruta: {Ruta}", path);

            await EscribirRespuestaJson(
                context,
                StatusCodes.Status401Unauthorized,
                ApiResponse<object>.Fail(
                    "No autorizado.",
                    "La API Key enviada no es válida."
                )
            );

            return;
        }

        await _next(context);
    }

    private bool EsRutaPublica(string path)
    {
        return _settings.RutasPublicas.Any(ruta =>
            path.Equals(ruta, StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith(ruta + "/", StringComparison.OrdinalIgnoreCase));
    }

    private static async Task EscribirRespuestaJson(
        HttpContext context,
        int statusCode,
        object response)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

        await context.Response.WriteAsync(json);
    }
}