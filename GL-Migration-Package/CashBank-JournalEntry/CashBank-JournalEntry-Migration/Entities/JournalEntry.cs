using System.ComponentModel.DataAnnotations;
using Truebooks.Platform.Contracts.Legacy.DTOs;

namespace Truebooks.Platform.Contracts.Legacy.Models;

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

public class JournalEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    
    private DateTime _entryDate = DateTime.UtcNow;

    [Required]
    [MaxLength(50)]
    public string EntryNumber { get; set; } = string.Empty;

    public DateTime EntryDate
    {
        get => _entryDate;
        set
        {
            _entryDate = value;
            FiscalYear = value.Year;
        }
    }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public JournalEntryStatus Status { get; set; } = JournalEntryStatus.Draft;

    public DateTime? PostedDate { get; set; }

    public Guid? PostedByUserId { get; set; }

    public JournalEntrySource Source { get; set; } = JournalEntrySource.Manual;

    public bool IsPriorPeriodAdjustment { get; set; } = false;

    public Guid? OriginalPeriodId { get; set; }

    public int FiscalYear { get; set; } = DateTime.UtcNow.Year;

    [MaxLength(500)]
    public string? AdjustmentReason { get; set; }

    public Guid? BranchId { get; set; }

    public Guid? DepartmentId { get; set; }

    public bool IsVoided { get; set; } = false;

    public DateTime? VoidedDate { get; set; }

    public Guid? VoidedByUserId { get; set; }

    [MaxLength(500)]
    public string? VoidReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
}

public class JournalEntryLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }

    public Guid JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }

    public Guid AccountId { get; set; }
    public ChartOfAccount? Account { get; set; }

    public decimal Debit { get; set; }

    public decimal Credit { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public Guid? CurrencyId { get; set; }

    public decimal? ExchangeRate { get; set; }

    public int FiscalYear { get; set; }
}
