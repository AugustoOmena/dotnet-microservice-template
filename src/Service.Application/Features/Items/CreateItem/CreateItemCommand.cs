using Service.Application.Abstractions.Messaging;

namespace Service.Application.Features.Items.CreateItem;

public sealed record CreateItemCommand(string Name, decimal Price) : ICommand<Guid>;