namespace Service.Application.Abstractions.Messaging;

public delegate Task<TResponse> RequestHandlerCallback<TResponse>();

public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : notnull
{
    Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerCallback<TResponse> next,
        CancellationToken cancellationToken = default);
}