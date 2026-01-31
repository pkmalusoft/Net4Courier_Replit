using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Net4Courier.Masters.Entities;

namespace Net4Courier.Web.Services;

public class AppAuthStateProvider : AuthenticationStateProvider
{
    private User? _currentUser;
    private Branch? _currentBranch;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public User? CurrentUser => _currentUser;
    public Branch? CurrentBranch => _currentBranch;
    public string CompanyName => _currentBranch?.Company?.Name ?? "";
    public string BranchName => _currentBranch?.Name ?? "";
    public string UserFullName => _currentUser?.FullName ?? _currentUser?.Username ?? "";
    public string CurrencyCode => _currentBranch?.Currency?.Code ?? "";
    public string CurrencySymbol => _currentBranch?.Currency?.Symbol ?? "";

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
            new(ClaimTypes.Role, _currentUser.Role?.Name ?? ""),
            new("BranchId", _currentBranch?.Id.ToString() ?? ""),
            new("BranchName", _currentBranch?.Name ?? ""),
            new("CompanyId", _currentBranch?.CompanyId.ToString() ?? ""),
            new("CompanyName", _currentBranch?.Company?.Name ?? ""),
            new("CurrencyCode", _currentBranch?.Currency?.Code ?? ""),
            new("CurrencySymbol", _currentBranch?.Currency?.Symbol ?? "")
        };

        var identity = new ClaimsIdentity(claims, "Custom");
        var principal = new ClaimsPrincipal(identity);

        return Task.FromResult(new AuthenticationState(principal));
    }

    public void Login(User user, Branch? branch = null)
    {
        _currentUser = user;
        _currentBranch = branch ?? user.Branch;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
    
    public void SetBranch(Branch branch)
    {
        _currentBranch = branch;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void Logout()
    {
        _currentUser = null;
        _currentBranch = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
