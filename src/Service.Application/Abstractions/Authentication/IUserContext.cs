namespace Service.Application.Abstractions.Authentication;

public interface IUserContext
{
    Guid UserId { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}