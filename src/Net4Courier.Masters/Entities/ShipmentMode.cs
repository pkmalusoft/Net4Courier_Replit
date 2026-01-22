using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class ShipmentMode : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}
