using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Net4Courier.Infrastructure.Data;

namespace Net4Courier.Web.Services;

public interface IAuditContextProvider
{
    long? UserId { get; }
    string? UserName { get; }
    long? BranchId { get; }
    string? BranchName { get; }
    string? IPAddress { get; }
}

public class AuditContextProvider : IAuditContextProvider
{
    private readonly AppAuthStateProvider _authStateProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditContextProvider(AppAuthStateProvider authStateProvider, IHttpContextAccessor httpContextAccessor)
    {
        _authStateProvider = authStateProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public long? UserId => _authStateProvider.CurrentUser?.Id;
    public string? UserName => _authStateProvider.CurrentUser?.FullName ?? _authStateProvider.CurrentUser?.Username;
    public long? BranchId => _authStateProvider.CurrentBranch?.Id;
    public string? BranchName => _authStateProvider.CurrentBranch?.Name;
    public string? IPAddress
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;
            var ip = context.Connection.RemoteIpAddress?.ToString();
            if (ip == "::1") ip = "127.0.0.1";
            var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwarded))
                ip = forwarded.Split(',').First().Trim();
            return ip;
        }
    }
}

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IAuditContextProvider _auditContext;

    public AuditSaveChangesInterceptor(IAuditContextProvider auditContext)
    {
        _auditContext = auditContext;
    }

    private void PopulateAuditContext(DbContext? context)
    {
        if (context is ApplicationDbContext dbContext && dbContext.CurrentUserId == null && _auditContext.UserId != null)
        {
            dbContext.CurrentUserId = _auditContext.UserId;
            dbContext.CurrentUserName = _auditContext.UserName;
            dbContext.CurrentBranchId = _auditContext.BranchId;
            dbContext.CurrentBranchName = _auditContext.BranchName;
            dbContext.CurrentIPAddress = _auditContext.IPAddress;
        }
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        PopulateAuditContext(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        PopulateAuditContext(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
