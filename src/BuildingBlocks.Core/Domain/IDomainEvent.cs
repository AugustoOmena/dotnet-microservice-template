namespace BuildingBlocks.Core.Domain;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOnUtc { get; }
}