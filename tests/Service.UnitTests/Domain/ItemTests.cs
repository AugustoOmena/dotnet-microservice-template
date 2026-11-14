using FluentAssertions;
using Service.Domain.Items;

namespace Service.UnitTests.Domain;

public sealed class ItemTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceedAndRaiseDomainEvent()
    {
        // Act
        var result = Item.Create("Teclado Mecânico", 450.00m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Teclado Mecânico");
        result.Value.Price.Should().Be(450.00m);
        result.Value.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ItemCreatedDomainEvent>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10.5)]
    public void Create_WithNonPositivePrice_ShouldReturnInvalidPriceError(decimal invalidPrice)
    {
        // Act
        var result = Item.Create("Mouse Gamer", invalidPrice);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ItemErrors.InvalidPrice);
    }
}