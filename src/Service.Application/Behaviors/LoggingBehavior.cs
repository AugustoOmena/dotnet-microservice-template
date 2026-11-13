using System.Diagnostics;
using BuildingBlocks.Core.Results;
using Microsoft.Extensions.Logging;
using Service.Application.Abstractions.Messaging;

namespace Service.Application.Behaviors;

public sealed partial class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerCallback<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        var requestName = typeof(TRequest).Name;

        LogProcessingRequest(logger, requestName);

        var startTimestamp = Stopwatch.GetTimestamp();
        var result = await next();
        var elapsed = Stopwatch.GetElapsedTime(startTimestamp);

        if (result.IsSuccess)
        {
            LogRequestCompleted(logger, requestName, elapsed.TotalMilliseconds);
        }
        else
        {
            LogRequestFailed(logger, requestName, result.Error.Code, elapsed.TotalMilliseconds);
        }

        return result;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Processando request {RequestName}")]
    private static partial void LogProcessingRequest(ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Request {RequestName} concluído com sucesso em {ElapsedMs:F2}ms")]
    private static partial void LogRequestCompleted(ILogger logger, string requestName, double elapsedMs);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Request {RequestName} falhou com erro {ErrorCode} em {ElapsedMs:F2}ms")]
    private static partial void LogRequestFailed(ILogger logger, string requestName, string errorCode, double elapsedMs);
}