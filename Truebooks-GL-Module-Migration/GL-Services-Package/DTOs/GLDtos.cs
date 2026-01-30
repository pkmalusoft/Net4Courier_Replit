namespace Truebooks.Platform.Contracts.DTOs.Finance;

public enum JournalEntryStatus
{
    Draft,
    Posted,
    Cancelled
}

public enum JournalEntrySource
{
    Manual,
    SalesInvoice,
    PurchaseBill,
    CashBank,
    YearEndClosing,
    OpeningBalance,
    PriorPeriodAdjustment
}

public record JournalEntryListDto(
    Guid Id,
    string EntryNumber,
    DateTime EntryDate,
    string Description,
    JournalEntryStatus Status,
    JournalEntrySource Source,
    bool IsVoided,
    string? VoidReason,
    decimal TotalDebit,
    decimal TotalCredit
);

public enum FinancialPeriodStatus
{
    NotOpened,
    Open,
    SoftClosed,
    HardClosed,
    Archived
}

public enum CalendarType
{
    Statutory,
    ClientFacing,
    Internal
}

public record FinancialPeriodDto(
    Guid Id,
    int FiscalYear,
    int PeriodNumber,
    string PeriodName,
    DateTime StartDate,
    DateTime EndDate,
    FinancialPeriodStatus Status,
    bool IsClosed,
    bool IsYearEnd,
    DateTime? ClosedDate,
    string? ClosedBy
);

public record CreateFinancialPeriodRequest(
    int FiscalYear,
    int PeriodNumber,
    string PeriodName,
    DateTime StartDate,
    DateTime EndDate
);

public record UpdateFinancialPeriodRequest(
    string PeriodName,
    DateTime StartDate,
    DateTime EndDate
);

public record FinancialCalendarDto(
    Guid Id,
    string Name,
    string Code,
    CalendarType Type,
    int StartMonth,
    string? CountryCode,
    bool IsPrimary,
    bool IsActive,
    string? Description
);

public record CreateFinancialCalendarRequest(
    string Name,
    string Code,
    string Type,
    int StartMonth,
    bool IsActive,
    string? Description,
    string? CountryCode
);

public record UpdateFinancialCalendarRequest(
    string Name,
    string Code,
    string Type,
    int StartMonth,
    bool IsActive,
    string? Description,
    string? CountryCode
);

public record TransactionCountsDto(
    int FiscalYear,
    int JournalEntries,
    int SalesInvoices,
    int PurchaseBills,
    int CashBankTransactions,
    int SalesOrders,
    int PurchaseOrders,
    int DeliveryNotes,
    int FinancialPeriods,
    int TotalTransactions
);

public record OpeningBalanceDto(
    Guid Id,
    int FiscalYear,
    string Status,
    DateTime? EffectiveDate,
    decimal TotalDebit,
    decimal TotalCredit,
    string? Notes,
    DateTime CreatedAt,
    string? CreatedBy,
    List<OpeningBalanceLineDto> Lines
);

public record OpeningBalanceLineDto(
    Guid Id,
    Guid AccountId,
    string AccountCode,
    string AccountName,
    decimal DebitAmount,
    decimal CreditAmount,
    string? Description
);

public record CreateOpeningBalanceRequest(
    int FiscalYear,
    DateTime EffectiveDate,
    string? Notes
);

public record UpdateOpeningBalanceRequest(
    DateTime EffectiveDate,
    string? Notes
);


public record DeferredRevenueDashboardDto(
    decimal TotalDeferredRevenue,
    decimal RecognizedThisMonth,
    decimal PendingRecognition,
    int ActiveSchedules,
    int PendingAlerts
);

public record RevenueScheduleDto(
    Guid Id,
    string ScheduleNumber,
    string SourceDocumentNumber,
    Guid? ContractId,
    string? ContractNumber,
    Guid? CustomerId,
    string? CustomerName,
    decimal TotalAmount,
    decimal RecognizedAmount,
    decimal RemainingAmount,
    DateTime StartDate,
    DateTime EndDate,
    DateTime ServiceStartDate,
    DateTime ServiceEndDate,
    string Status,
    int TotalPeriods,
    int CompletedPeriods
);

public record RecognitionLineDto(
    Guid Id,
    int PeriodNumber,
    DateTime RecognitionDate,
    decimal Amount,
    string Status,
    bool IsProcessed,
    DateTime? ProcessedDate,
    Guid? JournalEntryId
);

public record DeferredRevenueAlertDto(
    Guid Id,
    Guid ScheduleId,
    string ScheduleNumber,
    string AlertType,
    string Message,
    DateTime DueDate,
    decimal Amount,
    bool IsAcknowledged
);

public record YearEndStatusDto(
    int FiscalYear,
    bool IsClosed,
    DateTime? ClosedDate,
    string? ClosedBy,
    bool HasOpenTransactions,
    bool HasUnpostedEntries,
    int OpenPeriods,
    decimal RetainedEarnings
);

public record YearEndPreviewDto(
    int FiscalYear,
    decimal TotalRevenue,
    decimal TotalExpenses,
    decimal NetIncome,
    decimal RetainedEarningsBefore,
    decimal RetainedEarningsAfter,
    Guid? RetainedEarningsAccountId,
    string? RetainedEarningsAccountName,
    List<YearEndAccountBalanceDto> AccountBalances
);

public record YearEndAccountBalanceDto(
    Guid AccountId,
    string AccountCode,
    string AccountName,
    string AccountType,
    decimal Balance,
    bool WillClose
);

public record YearEndClosingRequest(
    Guid RetainedEarningsAccountId,
    DateTime ClosingDate,
    string? Notes
);

public record OpeningBalanceBatchDto
{
    public Guid Id { get; init; }
    public string BatchNumber { get; init; } = "";
    public DateTime EffectiveDate { get; init; }
    public string SourceModule { get; init; } = "";
    public string Status { get; init; } = "";
    public int FiscalYear { get; init; }
    public int LineCount { get; init; }
    public decimal TotalDebit { get; init; }
    public decimal TotalCredit { get; init; }
    public string? Notes { get; init; }
    public List<OpeningBalanceBatchLineDto>? Lines { get; init; }
}

public record OpeningBalanceBatchLineDto(
    Guid Id,
    Guid ReferenceId,
    string? ReferenceName,
    string ReferenceType,
    Guid? CurrencyId,
    decimal ExchangeRate,
    decimal HomeCurrencyDebit,
    decimal HomeCurrencyCredit
);

public record CreateOpeningBalanceBatchRequest(
    string SourceModule,
    DateTime EffectiveDate,
    int FiscalYear,
    string? Notes
);

public record UpdateOpeningBalanceBatchRequest(
    DateTime? EffectiveDate,
    string? Notes
);

public record CreateOpeningBalanceLineRequest(
    string ReferenceType,
    Guid ReferenceId,
    Guid CurrencyId,
    decimal ExchangeRate,
    decimal OriginalDebit,
    decimal OriginalCredit
);

public record DashboardSummaryDto(
    decimal TotalDeferredAmount,
    decimal RecognizedThisMonth,
    decimal PendingRecognition,
    int ActiveScheduleCount
);

public enum YearEndClosingStatus
{
    Draft,
    Validated,
    IncomeExpenseClosed,
    SubledgersSnapshotted,
    InventorySnapshotted,
    OpeningJEGenerated,
    Completed,
    RolledBack
}

public record YearEndClosingDto(
    Guid Id,
    string ClosingNumber,
    Guid FinancialPeriodId,
    int FiscalYear,
    string PeriodName,
    YearEndClosingStatus Status,
    decimal NetIncomeOrLoss,
    decimal ARSubledgerBalance,
    decimal APSubledgerBalance,
    decimal InventorySubledgerBalance,
    DateTime? ClosingDate,
    string? Notes
);

public record YearEndValidationResultDto(
    bool IsValid,
    bool RetainedEarningsConfigured,
    bool ARControlConfigured,
    int UnpostedJournals,
    decimal TotalRevenue,
    decimal TotalExpense,
    decimal NetIncomeOrLoss,
    List<string> Errors
);

public record DeferredRevenueRecognitionLineDto(
    DateTime PeriodStartDate,
    decimal Amount,
    string Status,
    DateTime? PostedDate
);
