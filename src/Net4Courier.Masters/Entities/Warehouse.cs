using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class Warehouse : BaseEntity
{
    public long BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public decimal? CapacitySquareMeters { get; set; }
    public int? MaxPalletCapacity { get; set; }
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
    
    public virtual Branch Branch { get; set; } = null!;
}
