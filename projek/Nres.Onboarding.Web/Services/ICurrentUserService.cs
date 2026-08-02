using System.Security.Claims;

namespace Nres.Onboarding.Web.Services;

/// <summary>
/// Read-only view of the signed-in user. Controllers and services depend on this
/// instead of touching <c>HttpContext</c> directly, which keeps them testable.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>Identity user id, or <c>null</c> when nobody is signed in.</summary>
    string? UserId { get; }

    /// <summary>User name (the login), or <c>null</c> when nobody is signed in.</summary>
    string? UserName { get; }

    bool IsAuthenticated { get; }

    IReadOnlyList<string> Roles { get; }

    bool IsInRole(string role);
}

/// <inheritdoc cref="ICurrentUserService" />
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public string? UserId => Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName => Principal?.Identity?.Name;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public IReadOnlyList<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? [];

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;
}
