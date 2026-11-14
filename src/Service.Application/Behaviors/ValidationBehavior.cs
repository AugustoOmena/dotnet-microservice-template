using System.Collections.Concurrent;
using System.Linq.Expressions;
using BuildingBlocks.Core.Results;
using FluentValidation;
using Service.Application.Abstractions.Messaging;

namespace Service.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseCommand
    where TResponse : Result
{
    private static readonly ConcurrentDictionary<Type, Func<ValidationError, TResponse>> FailureFactories = new();

    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerCallback<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var errors = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .Select(failure => Error.Validation(failure.PropertyName, failure.ErrorMessage))
            .Distinct()
            .ToArray();

        if (errors.Length == 0)
        {
            return await next();
        }

        return CreateValidationFailure(new ValidationError(errors));
    }

    private static TResponse CreateValidationFailure(ValidationError validationError)
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)Result.Failure(validationError);
        }

        var factory = FailureFactories.GetOrAdd(typeof(TResponse), static responseType =>
        {
            var resultValueType = responseType.GetGenericArguments()[0];
            var failureMethod = typeof(Result)
                .GetMethods()
                .First(m => m.Name == nameof(Result.Failure) && m.IsGenericMethod)
                .MakeGenericMethod(resultValueType);

            var errorParam = Expression.Parameter(typeof(ValidationError), "error");
            var callExpr = Expression.Call(failureMethod, errorParam);
            return Expression.Lambda<Func<ValidationError, TResponse>>(callExpr, errorParam).Compile();
        });

        return factory(validationError);
    }
}