using Truebooks.Platform.Core.MultiTenancy;

namespace Net4Courier.Web.Services;

public class SingleTenantContext : ITenantContext
{
    private static readonly Guid SingleTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    
    public Guid? TenantId => SingleTenantId;
    public bool HasTenant => true;
    public string? TenantName => "Net4Courier";
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
}
