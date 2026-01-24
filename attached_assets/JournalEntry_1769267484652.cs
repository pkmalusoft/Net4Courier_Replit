using System.ComponentModel.DataAnnotations;
using Server.Core.Common;

namespace Server.Modules.GeneralLedger.Models;

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

public class JournalEntry : BaseEntity
{
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

    // Branch and Department for transaction stamping
    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    // Soft Delete / Void fields
    public bool IsVoided { get; set; } = false;

    public DateTime? VoidedDate { get; set; }

    public Guid? VoidedByUserId { get; set; }

    [MaxLength(500)]
    public string? VoidReason { get; set; }

    public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
}

public class JournalEntryLine : BaseEntity
{
    private JournalEntry? _journalEntry;
    
    public Guid JournalEntryId { get; set; }
    
    public JournalEntry JournalEntry 
    { 
        get => _journalEntry!;
        set
        {
            _journalEntry = value;
            if (value != null)
            {
                FiscalYear = value.FiscalYear;
            }
        }
    }

    public Guid AccountId { get; set; }
    public ChartOfAccount Account { get; set; } = null!;

    public decimal Debit { get; set; }

    public decimal Credit { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public Guid? CurrencyId { get; set; }
    public Currency? Currency { get; set; }

    public decimal? ExchangeRate { get; set; }
    
    public int FiscalYear { get; set; }
}
