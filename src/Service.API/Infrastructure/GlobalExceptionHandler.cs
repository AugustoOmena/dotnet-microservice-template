using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Service.API.Infrastructure;

internal sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is BadHttpRequestException badRequestException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Bad Request",
                    Detail = "O corpo ou os parâmetros da requisição estão malformados.",
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    Extensions =
                    {
                        ["errorCode"] = "Request.Malformed",
                        ["reason"] = badRequestException.Message
                    }
                }
            });
        }

        LogUnhandledException(logger, exception, httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "Ocorreu um erro inesperado ao processar a requisição.",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1"
            }
        });
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Exceção não tratada capturada. TraceId: {TraceId}")]
    private static partial void LogUnhandledException(
        ILogger logger,
        Exception exception,
        string traceId);
}