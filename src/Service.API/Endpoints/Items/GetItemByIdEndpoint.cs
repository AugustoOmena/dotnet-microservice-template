using Service.API.Extensions;
using Service.Application.Abstractions.Messaging;
using Service.Application.Features.Items.GetItemById;

namespace Service.API.Endpoints.Items;

internal sealed class GetItemByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("items/{id:guid}", async (
            Guid id,
            IDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            var query = new GetItemByIdQuery(id);
            var result = await dispatcher.QueryAsync<GetItemByIdQuery, ItemResponse>(query, cancellationToken);

            return result.Match(
                item => Results.Ok(item),
                failure => failure.ToProblemDetails());
        })
        .WithTags("Items")
        .WithSummary("Busca um item pelo ID")
        .Produces<ItemResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}