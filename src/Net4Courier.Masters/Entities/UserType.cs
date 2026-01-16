using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class UserType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
