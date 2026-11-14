using BuildingBlocks.Core.Domain;
using BuildingBlocks.Core.Results;

namespace Service.Domain.Items;

public sealed class Item : AggregateRoot<Guid>, IAuditableEntity
{
    private Item(Guid id, string name, decimal price)
        : base(id)
    {
        Name = name;
        Price = price;
    }

    private Item() { }

    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public static Result<Item> Create(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ItemErrors.EmptyName;
        }

        if (price <= 0)
        {
            return ItemErrors.InvalidPrice;
        }

        var item = new Item(Guid.CreateVersion7(), name.Trim(), price);

        item.RaiseDomainEvent(new ItemCreatedDomainEvent(
            Guid.CreateVersion7(),
            DateTime.UtcNow,
            item.Id,
            item.Name,
            item.Price));

        return item;
    }

    public Result UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
        {
            return ItemErrors.InvalidPrice;
        }

        Price = newPrice;
        return Result.Success();
    }
}