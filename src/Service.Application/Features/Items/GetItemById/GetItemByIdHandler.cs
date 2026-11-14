using BuildingBlocks.Core.Results;
using Microsoft.EntityFrameworkCore;
using Service.Application.Abstractions.Data;
using Service.Application.Abstractions.Messaging;
using Service.Domain.Items;

namespace Service.Application.Features.Items.GetItemById;

internal sealed class GetItemByIdHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetItemByIdQuery, ItemResponse>
{
    public async Task<Result<ItemResponse>> HandleAsync(
        GetItemByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var item = await dbContext.Items
            .AsNoTracking()
            .Where(x => x.Id == query.Id)
            .Select(x => new ItemResponse(x.Id, x.Name, x.Price, x.CreatedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return ItemErrors.NotFound(query.Id);
        }

        return item;
    }
}