namespace PaymentIntegration.API.Middlewares;

public class HttpLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpLoggingMiddleware> _logger;

    public HttpLoggingMiddleware(RequestDelegate next, ILogger<HttpLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationId);
        
        _logger.LogInformation($"Handling request: {context.Request.Method} {context.Request.Path} {correlationId}");

        await _next(context);

        _logger.LogInformation("Finished handling request.");
    }
}