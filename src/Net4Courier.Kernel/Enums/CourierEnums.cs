namespace Net4Courier.Kernel.Enums;

public enum CourierStatus
{
    Pending = 0,
    PickedUp = 1,
    InTransit = 2,
    OutForDelivery = 3,
    Delivered = 4,
    Returned = 5,
    OnHold = 6,
    Cancelled = 7
}

public enum PaymentMode
{
    Prepaid = 1,
    ToPay = 2,
    COD = 3,
    Credit = 4
}

public enum MovementType
{
    Domestic = 1,
    International = 2,
    Import = 3,
    Export = 4
}

public enum ShipmentMode
{
    Air = 1,
    Surface = 2,
    Sea = 3,
    Rail = 4
}

public enum DocumentType
{
    Document = 1,
    Parcel = 2,
    Cargo = 3
}

public enum InvoiceStatus
{
    Draft = 0,
    Generated = 1,
    Sent = 2,
    PartiallyPaid = 3,
    Paid = 4,
    Cancelled = 5
}
