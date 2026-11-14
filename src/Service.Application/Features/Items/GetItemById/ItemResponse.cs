namespace Service.Application.Features.Items.GetItemById;

public sealed record ItemResponse(Guid Id, string Name, decimal Price, DateTime CreatedAtUtc);