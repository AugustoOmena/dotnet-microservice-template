using Service.Application.Abstractions.Messaging;

namespace Service.Application.Features.Items.GetItemById;

public sealed record GetItemByIdQuery(Guid Id) : IQuery<ItemResponse>;