namespace CapitalPos.Cpe.Api.Middlewares;

public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ObtenerOCrearCorrelationId(context);

        context.Items["CorrelationId"] = correlationId;
        context.TraceIdentifier = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        _logger.LogInformation(
            "Request iniciado. Method: {Method}, Path: {Path}, CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path.Value,
            correlationId
        );

        await _next(context);

        _logger.LogInformation(
            "Request finalizado. Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path.Value,
            context.Response.StatusCode,
            correlationId
        );
    }

    private static string ObtenerOCrearCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var correlationIdRecibido) &&
            !string.IsNullOrWhiteSpace(correlationIdRecibido))
        {
            return correlationIdRecibido.ToString();
        }

        return Guid.NewGuid().ToString("N");
    }
}