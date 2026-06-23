using System.Text.Json;
using CapitalPos.Cpe.Api.Dtos;

namespace CapitalPos.Cpe.Api.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var correlationId = ObtenerCorrelationId(context);

            _logger.LogError(
                ex,
                "Error no controlado en la API. Path: {Path}, CorrelationId: {CorrelationId}",
                context.Request.Path.Value,
                correlationId
            );

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var errores = new List<string>
            {
                $"Código de seguimiento: {correlationId}"
            };

            if (_environment.IsDevelopment())
            {
                errores.Add(ex.Message);
            }
            else
            {
                errores.Add("Ocurrió un error interno inesperado.");
            }

            var response = ApiResponse<object>.Fail(
                "Ocurrió un error interno en la API.",
                errores
            );

            response.Data = new
            {
                CorrelationId = correlationId
            };

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

    private static string ObtenerCorrelationId(HttpContext context)
    {
        if (context.Items.TryGetValue("CorrelationId", out var correlationId) &&
            correlationId != null)
        {
            return correlationId.ToString() ?? context.TraceIdentifier;
        }

        return context.TraceIdentifier;
    }
}