using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class Currency : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Symbol { get; set; }
    public int DecimalPlaces { get; set; } = 2;
    public bool IsBaseCurrency { get; set; } = false;
    public decimal ExchangeRate { get; set; } = 1.0m;
}
