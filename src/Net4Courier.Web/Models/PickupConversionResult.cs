using Net4Courier.Operations.Entities;

namespace Net4Courier.Web.Models;

public class PickupConversionResult
{
    public PickupRequest PickupRequest { get; set; } = null!;
    public PickupRequestShipment Shipment { get; set; } = null!;
}
