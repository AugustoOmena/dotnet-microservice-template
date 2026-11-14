using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Service.Application.Features.Items.GetItemById;
using Service.Infrastructure.Persistence;
using Service.IntegrationTests.Infrastructure;

namespace Service.IntegrationTests.Items;

public sealed class ItemsEndpointsTests(IntegrationTestWebAppFactory factory)
    : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private sealed record CreatedIdResponse(Guid Id);

    [Fact]
    public async Task CreateAndGetItem_WithValidPayload_ShouldPersistItemAndOutboxMessage()
    {
        // Arrange
        var payload = new { Name = "Monitor Ultrawide 34", Price = 2899.90m };

        // Act - POST
        var postResponse = await _client.PostAsJsonAsync("/api/v1/items", payload);

        // Assert - HTTP 201
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await postResponse.Content.ReadFromJsonAsync<CreatedIdResponse>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();

        // Act - GET
        var getResponse = await _client.GetAsync($"/api/v1/items/{created.Id}");

        // Assert - HTTP 200 & Dados
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var item = await getResponse.Content.ReadFromJsonAsync<ItemResponse>();
        item.Should().NotBeNull();
        item!.Name.Should().Be("Monitor Ultrawide 34");
        item.Price.Should().Be(2899.90m);

        // Assert - Outbox Message gravada na mesma transação!
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var outboxMessages = await dbContext.OutboxMessages.ToListAsync();

        outboxMessages.Should().ContainSingle(m => m.Content.Contains(created.Id.ToString()));
    }

    [Fact]
    public async Task CreateItem_WithInvalidPayload_ShouldReturnBadRequestProblemDetails()
    {
        // Arrange
        var invalidPayload = new { Name = "", Price = -50m };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/items", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}