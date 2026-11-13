namespace BuildingBlocks.Core.Results;

public sealed record ValidationError(Error[] Errors)
    : Error("Validation.General", "Um ou mais erros de validação ocorreram.", ErrorType.Validation)
{
    public static ValidationError FromResults(IEnumerable<Result> results) =>
        new([.. results.Where(r => r.IsFailure).Select(r => r.Error)]);
}