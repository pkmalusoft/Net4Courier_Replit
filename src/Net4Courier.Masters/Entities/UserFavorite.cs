using System.ComponentModel.DataAnnotations;
using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class UserFavorite : BaseEntity
{
    public long UserId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string MenuCode { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string MenuName { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? MenuIcon { get; set; }
    
    [MaxLength(100)]
    public string? Route { get; set; }
    
    [MaxLength(100)]
    public string? Category { get; set; }
    
    public int DisplayOrder { get; set; }
    
    public virtual User? User { get; set; }
}
