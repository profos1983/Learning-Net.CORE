using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        // Получаем или создаём Correlation ID (для трассировки запроса через несколько сервисов)
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        var request = context.Request;
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = request.Headers.UserAgent.ToString();

        // ВАЖНО: используем структурированное логирование с {плейсхолдерами}, 
        // а НЕ интерполяцию строк $"..."! Это позволяет Serilog/NLog индексировать поля.
        _logger.LogInformation(
            "Request started: {Method} {Path} from {RemoteIp} | UserAgent: {UserAgent} | CorrelationId: {CorrelationId}",
            request.Method,
            request.Path,
            remoteIp,
            userAgent,
            correlationId);

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Request failed: {Method} {Path} | CorrelationId: {CorrelationId}",
                request.Method,
                request.Path,
                correlationId);
            throw; // Пробрасываем дальше, чтобы сработал UseExceptionHandler
        }
        finally
        {
            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;
            var level = statusCode >= 500 ? LogLevel.Error
                      : statusCode >= 400 ? LogLevel.Warning
                      : LogLevel.Information;

            _logger.Log(level,
                "Request finished: {Method} {Path} -> {StatusCode} in {ElapsedMs} ms | CorrelationId: {CorrelationId}",
                request.Method,
                request.Path,
                statusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId);
        }
    }
}
