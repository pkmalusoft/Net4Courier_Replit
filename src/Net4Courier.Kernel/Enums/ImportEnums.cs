namespace Net4Courier.Kernel.Enums;

public enum ShipmentDirection
{
    Import = 1,
    Export = 2
}

public enum ImportMode
{
    Air = 1,
    Sea = 2,
    Land = 3
}

public enum MasterReferenceType
{
    MAWB = 1,
    BL = 2,
    TruckWaybill = 3
}

public enum ImportMasterStatus
{
    Draft = 0,
    Confirmed = 1,
    InTransit = 2,
    Arrived = 3,
    Inscanning = 4,
    Inscanned = 5,
    CustomsProcessing = 6,
    Cleared = 7,
    Closed = 8,
    Cancelled = 9
}

public enum ImportBagStatus
{
    Expected = 0,
    Arrived = 1,
    Inscanned = 2,
    Missing = 3,
    Damaged = 4
}

public enum ImportShipmentStatus
{
    Expected = 0,
    Inscanned = 1,
    PendingCustoms = 2,
    CustomsHold = 3,
    UnderExamination = 4,
    AwaitingDocuments = 5,
    Cleared = 6,
    Released = 7,
    HandedOver = 8,
    OnHold = 9
}

public enum CustomsHoldReason
{
    None = 0,
    Valuation = 1,
    Documentation = 2,
    ProhibitedItems = 3,
    InspectionRequired = 4,
    DutyPaymentPending = 5,
    ImporterVerification = 6,
    HSCodeMismatch = 7,
    WeightDiscrepancy = 8,
    DescriptionMismatch = 9,
    MissingInvoice = 10,
    RestrictedGoods = 11,
    Other = 99
}

public enum CustomsStatus
{
    NotApplicable = 0,
    PendingFiling = 1,
    Filed = 2,
    UnderAssessment = 3,
    DutyAssessed = 4,
    DutyPaid = 5,
    Cleared = 6,
    OnHold = 7,
    Released = 8
}

public enum ImportShipmentType
{
    Document = 1,
    NonDocument = 2
}

public enum ImportType
{
    Courier = 1,
    Commercial = 2,
    PersonalEffects = 3,
    Sample = 4,
    Gift = 5
}

public enum CargoType
{
    Courier = 1,
    Freight = 2,
    Express = 3
}

public enum BagType
{
    CourierBag = 1,
    Pallet = 2,
    Container = 3,
    Loose = 4
}

public enum NoteCategory
{
    General = 0,
    Customs = 1,
    Operations = 2,
    CustomerCommunication = 3,
    HoldReason = 4,
    Resolution = 5,
    Delivery = 6,
    Finance = 7,
    Issue = 8
}

public enum CustomsWorkStatus
{
    NotStarted = 0,
    DocumentReview = 1,
    Inspection = 2,
    DutyCalculation = 3,
    PaymentPending = 4,
    Completed = 5
}

public enum ImportBagType
{
    Bag = 1,
    Container = 2,
    Pallet = 3,
    Carton = 4
}
