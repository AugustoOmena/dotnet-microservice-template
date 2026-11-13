namespace BuildingBlocks.Core.Domain;

public interface IAuditableEntity
{
    DateTime CreatedAtUtc { get; }
    DateTime? UpdatedAtUtc { get; }
}