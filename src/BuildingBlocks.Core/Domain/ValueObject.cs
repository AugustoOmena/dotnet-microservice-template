namespace BuildingBlocks.Core.Domain;

public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        return ((ValueObject)obj).GetEqualityComponents()
            .SequenceEqual(GetEqualityComponents());
    }

    public bool Equals(ValueObject? other) =>
        Equals((object?)other);

    public static bool operator ==(ValueObject? left, ValueObject? right) =>
        Equals(left, right);

    public static bool operator !=(ValueObject? left, ValueObject? right) =>
        !Equals(left, right);

    public override int GetHashCode() =>
        GetEqualityComponents()
            .Aggregate(default(int), (hash, obj) => HashCode.Combine(hash, obj));
}