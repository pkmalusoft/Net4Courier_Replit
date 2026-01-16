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
    PickupCash = 1,
    COD = 2,
    Account = 3,
    Prepaid = 4,
    CAD = 5,
    ToPay = 6,
    Credit = 7
}

public enum MovementType
{
    Domestic = 1,
    InternationalExport = 2,
    InternationalImport = 3,
    Transhipment = 4
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

public enum ParcelType
{
    Document = 1,
    Letter = 2,
    ParcelUpto30Kg = 3,
    ParcelAbove30Kg = 4
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

public enum DeliveryStatus
{
    Delivered = 1,
    PartialDelivery = 2,
    Refused = 3,
    NotDelivered = 4
}

public enum NonDeliveryReason
{
    AddressNotFound = 1,
    CustomerNotAvailable = 2,
    Refused = 3,
    PremisesClosed = 4,
    IncorrectAddress = 5,
    CustomerRequestedReschedule = 6,
    WeatherConditions = 7,
    AccessRestricted = 8,
    Other = 99
}

public enum RecipientRelation
{
    Self = 1,
    Family = 2,
    Colleague = 3,
    Security = 4,
    Reception = 5,
    Neighbor = 6,
    Other = 99
}
