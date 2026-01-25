using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class AccountType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    
    public virtual ICollection<Party> Parties { get; set; } = new List<Party>();
}
