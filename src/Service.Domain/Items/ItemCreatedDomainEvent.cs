using BuildingBlocks.Core.Domain;

namespace Service.Domain.Items;

public sealed record ItemCreatedDomainEvent(
    Guid EventId,
    DateTime OccurredOnUtc,
    Guid ItemId,
    string Name,
    decimal Price) : IDomainEvent;