using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TeamBrain.Application.Common.Interfaces;

namespace TeamBrain.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    private string? GetClaim(string type) => Principal?.Claims.FirstOrDefault(c => c.Type == type)?.Value;

    public Guid UserId
    {
        get
        {
            var oid = GetClaim("oid") ?? GetClaim(ClaimTypes.NameIdentifier);
            return oid is not null && Guid.TryParse(oid, out var id) ? id : Guid.Empty;
        }
    }

    public string Email => GetClaim(ClaimTypes.Email) ?? GetClaim("preferred_username") ?? string.Empty;
    public string DisplayName => GetClaim(ClaimTypes.Name) ?? GetClaim("name") ?? string.Empty;
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;
}
