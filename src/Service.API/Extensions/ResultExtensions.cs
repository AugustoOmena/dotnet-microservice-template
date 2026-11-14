using BuildingBlocks.Core.Results;

namespace Service.API.Extensions;

public static class ResultExtensions
{
    public static IResult Match(
        this Result result,
        Func<IResult> onSuccess,
        Func<Result, IResult>? onFailure = null) =>
        result.IsSuccess
            ? onSuccess()
            : (onFailure ?? ToProblemDetails)(result);

    public static IResult Match<TValue>(
        this Result<TValue> result,
        Func<TValue, IResult> onSuccess,
        Func<Result, IResult>? onFailure = null) =>
        result.IsSuccess
            ? onSuccess(result.Value)
            : (onFailure ?? ToProblemDetails)(result);

    public static IResult ToProblemDetails(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Não é possível converter um Result de sucesso em ProblemDetails.");
        }

        return Results.Problem(
            statusCode: GetStatusCode(result.Error.Type),
            title: GetTitle(result.Error.Type),
            detail: result.Error.Description,
            type: GetTypeUri(result.Error.Type),
            extensions: GetExtensions(result));
    }

    private static int GetStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };

    private static string GetTitle(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => "Bad Request",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden => "Forbidden",
        ErrorType.NotFound => "Not Found",
        ErrorType.Conflict => "Conflict",
        _ => "Server Failure"
    };

    private static string GetTypeUri(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        ErrorType.Unauthorized => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
        ErrorType.Forbidden => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
        ErrorType.NotFound => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
        ErrorType.Conflict => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
        _ => "https://tools.ietf.org/html/rfc9110#section-15.6.1"
    };

    private static Dictionary<string, object?> GetExtensions(Result result)
    {
        var extensions = new Dictionary<string, object?>
        {
            ["errorCode"] = result.Error.Code
        };

        if (result.Error is ValidationError validationError)
        {
            extensions["errors"] = validationError.Errors
                .Select(e => new { e.Code, e.Description });
        }

        return extensions;
    }
}