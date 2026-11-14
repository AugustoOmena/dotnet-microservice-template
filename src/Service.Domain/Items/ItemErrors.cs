using BuildingBlocks.Core.Results;

namespace Service.Domain.Items;

public static class ItemErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Items.NotFound", $"O item com identificador '{id}' não foi encontrado.");

    public static readonly Error InvalidPrice =
        Error.Validation("Items.InvalidPrice", "O preço do item deve ser maior que zero.");

    public static readonly Error EmptyName =
        Error.Validation("Items.EmptyName", "O nome do item não pode ser vazio.");
}