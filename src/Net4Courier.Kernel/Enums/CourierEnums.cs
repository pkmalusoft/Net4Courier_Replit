namespace Net4Courier.Kernel.Enums;

public enum CourierStatus
{
    Pending = 0,
    AssignedToPickup = 1,
    PickedUp = 2,
    InscanAtOrigin = 3,
    InTransit = 4,
    InscanAtDestination = 5,
    OutForDelivery = 6,
    Delivered = 7,
    NotDelivered = 8,
    ReturnToOrigin = 9,
    Returned = 10,
    OnHold = 11,
    Cancelled = 12
}

public enum PaymentMode
{
    PickupCash = 1,
    COD = 2,
    Account = 3,
    Prepaid = 4,
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
    Letter = 1,
    Document = 2,
    ParcelUpto30Kg = 3,
    ParcelAbove30Kg = 4
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

public enum DRSStatus
{
    Open = 1,
    Submitted = 2,
    PartiallyReconciled = 3,
    Reconciled = 4,
    Closed = 5
}

public enum ExpenseType
{
    Fuel = 1,
    Toll = 2,
    Parking = 3,
    Repair = 4,
    Food = 5,
    Other = 99
}

public enum ExpenseStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public enum LedgerEntryType
{
    Shortage = 1,
    Excess = 2,
    ExpenseReimbursement = 3,
    SalaryDeduction = 4,
    Adjustment = 5
}

public enum RTSChargeMode
{
    Free = 0,
    Chargeable = 1
}
