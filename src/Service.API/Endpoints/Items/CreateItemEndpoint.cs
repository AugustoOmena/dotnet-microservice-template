using Service.API.Extensions;
using Service.Application.Abstractions.Messaging;
using Service.Application.Features.Items.CreateItem;

namespace Service.API.Endpoints.Items;

internal sealed class CreateItemEndpoint : IEndpoint
{
    public sealed record CreateItemRequest(string Name, decimal Price);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("items", async (
            CreateItemRequest request,
            IDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateItemCommand(request.Name, request.Price);
            var result = await dispatcher.SendAsync<CreateItemCommand, Guid>(command, cancellationToken);

            return result.Match(
                id => Results.Created($"/api/v1/items/{id}", new { id }),
                failure => failure.ToProblemDetails());
        })
        .WithTags("Items")
        .WithSummary("Cria um novo item")
        .Produces<object>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}