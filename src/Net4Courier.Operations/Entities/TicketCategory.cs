using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class TicketCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int SortOrder { get; set; }
}
