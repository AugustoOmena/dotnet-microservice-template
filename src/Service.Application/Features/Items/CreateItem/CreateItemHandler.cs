using BuildingBlocks.Core.Results;
using Service.Application.Abstractions.Data;
using Service.Application.Abstractions.Messaging;
using Service.Domain.Items;

namespace Service.Application.Features.Items.CreateItem;

internal sealed class CreateItemHandler(IApplicationDbContext dbContext)
    : ICommandHandler<CreateItemCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateItemCommand command,
        CancellationToken cancellationToken = default)
    {
        var itemResult = Item.Create(command.Name, command.Price);
        if (itemResult.IsFailure)
        {
            return itemResult.Error;
        }

        dbContext.Items.Add(itemResult.Value);
        await dbContext.SaveChangesAsync(cancellationToken);

        return itemResult.Value.Id;
    }
}