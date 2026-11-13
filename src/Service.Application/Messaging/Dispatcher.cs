using BuildingBlocks.Core.Results;
using Microsoft.Extensions.DependencyInjection;
using Service.Application.Abstractions.Messaging;

namespace Service.Application.Messaging;

public sealed class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    public Task<Result> SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        return ExecutePipelineAsync<TCommand, Result>(
            command,
            () => handler.HandleAsync(command, cancellationToken),
            cancellationToken);
    }

    public Task<Result<TResponse>> SendAsync<TCommand, TResponse>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResponse>
    {
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResponse>>();
        return ExecutePipelineAsync<TCommand, Result<TResponse>>(
            command,
            () => handler.HandleAsync(command, cancellationToken),
            cancellationToken);
    }

    public Task<Result<TResponse>> QueryAsync<TQuery, TResponse>(
        TQuery query,
        CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResponse>
    {
        var handler = serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();
        return ExecutePipelineAsync<TQuery, Result<TResponse>>(
            query,
            () => handler.HandleAsync(query, cancellationToken),
            cancellationToken);
    }

    private Task<TResponse> ExecutePipelineAsync<TRequest, TResponse>(
        TRequest request,
        RequestHandlerCallback<TResponse> handlerCallback,
        CancellationToken cancellationToken)
        where TRequest : notnull
    {
        var behaviors = serviceProvider
            .GetServices<IPipelineBehavior<TRequest, TResponse>>()
            .Reverse()
            .ToArray();

        var pipeline = behaviors.Aggregate(
            handlerCallback,
            (next, behavior) => () => behavior.HandleAsync(request, next, cancellationToken));

        return pipeline();
    }
}