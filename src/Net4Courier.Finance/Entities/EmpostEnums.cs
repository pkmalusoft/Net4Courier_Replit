namespace Net4Courier.Finance.Entities;

public enum QuarterNumber
{
    Q1 = 1,
    Q2 = 2,
    Q3 = 3,
    Q4 = 4
}

public enum QuarterStatus
{
    Open,
    PendingSubmission,
    Submitted,
    Locked
}

public enum EmpostLicenseStatus
{
    Active,
    PendingRenewal,
    Expired,
    Suspended
}

public enum EmpostClassification
{
    Taxable,
    Exempt,
    FreightOver30Kg,
    LumpSumContract,
    Warehousing,
    PassThrough
}

public enum EmpostTaxabilityStatus
{
    Taxable,
    NonTaxable
}

public enum EmpostFeeStatus
{
    Pending,
    Settled,
    Credited,
    Adjusted
}

public enum EmpostSettlementStatus
{
    Pending,
    PartiallyPaid,
    Paid,
    Waived
}

public enum AdvancePaymentStatus
{
    Pending,
    Paid,
    PartiallyPaid,
    Overdue
}

public enum EmpostAdjustmentType
{
    FullRefund,
    PartialRefund,
    Reversal
}

public enum AdjustmentStatus
{
    Pending,
    Applied,
    Rejected
}

public enum EmpostAuditAction
{
    LicenseCreated,
    LicenseUpdated,
    LicenseRenewed,
    AdvancePaymentRecorded,
    QuarterLocked,
    QuarterUnlocked,
    QuarterSubmitted,
    SettlementCreated,
    SettlementPaid,
    ShipmentFeeCalculated,
    ReturnAdjustmentCreated,
    ReturnAdjustmentApplied,
    ReportGenerated,
    ReconciliationPerformed,
    ClassificationOverride
}
