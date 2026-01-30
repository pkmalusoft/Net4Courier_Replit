using System.ComponentModel.DataAnnotations;
using Server.Modules.CashBank.Models;

namespace Server.Modules.BankReconciliation.Models;

public class BankAccount : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string AccountNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string AccountName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string BankName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? BranchName { get; set; }

    [MaxLength(20)]
    public string? SwiftCode { get; set; }

    [MaxLength(50)]
    public string? IbanNumber { get; set; }

    // Link to Chart of Account for GL posting
    public Guid ChartOfAccountId { get; set; }
    public ChartOfAccount? ChartOfAccount { get; set; }

    public Guid? CurrencyId { get; set; }
    public Currency? Currency { get; set; }

    public decimal OpeningBalance { get; set; }
    public DateTime OpeningBalanceDate { get; set; } = DateTime.UtcNow.Date;

    public bool IsActive { get; set; } = true;

    // Deactivation / Void Metadata
    public DateTime? DeactivatedDate { get; set; }

    public Guid? DeactivatedByUserId { get; set; }

    [MaxLength(500)]
    public string? DeactivationReason { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation
    public ICollection<BankReconciliation> Reconciliations { get; set; } = new List<BankReconciliation>();
    public ICollection<CashBankTransaction> Transactions { get; set; } = new List<CashBankTransaction>();
}
