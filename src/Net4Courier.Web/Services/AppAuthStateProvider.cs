using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Net4Courier.Masters.Entities;

namespace Net4Courier.Web.Services;

public class AppAuthStateProvider : AuthenticationStateProvider
{
    private User? _currentUser;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public User? CurrentUser => _currentUser;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_currentUser == null)
            return Task.FromResult(new AuthenticationState(_anonymous));

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, _currentUser.Username),
            new(ClaimTypes.Email, _currentUser.Email ?? ""),
            new("FullName", _currentUser.FullName ?? ""),
            new("UserId", _currentUser.Id.ToString()),
            new(ClaimTypes.Role, _currentUser.Role?.Name ?? "")
        };

        var identity = new ClaimsIdentity(claims, "Custom");
        var principal = new ClaimsPrincipal(identity);

        return Task.FromResult(new AuthenticationState(principal));
    }

    public void Login(User user)
    {
        _currentUser = user;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void Logout()
    {
        _currentUser = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
