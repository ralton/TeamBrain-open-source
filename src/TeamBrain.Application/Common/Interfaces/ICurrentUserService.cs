namespace TeamBrain.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string Email { get; }
    string DisplayName { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
