using System.ComponentModel.DataAnnotations;

namespace Server.Modules.Courier.Models;

public class CourierSettings : BaseEntity
{
    public Guid? PrepaidControlAccountId { get; set; }
    public ChartOfAccount? PrepaidControlAccount { get; set; }

    public Guid? CODControlAccountId { get; set; }
    public ChartOfAccount? CODControlAccount { get; set; }

    public Guid? FreightRevenueAccountId { get; set; }
    public ChartOfAccount? FreightRevenueAccount { get; set; }

    public Guid? CODPayableAccountId { get; set; }
    public ChartOfAccount? CODPayableAccount { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
