using System.ComponentModel.DataAnnotations;

namespace Server.Modules.CashBank.Models;

public class VoucherAttachment : BaseEntity
{
    public Guid CashBankTransactionId { get; set; }
    public CashBankTransaction CashBankTransaction { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public byte[] FileData { get; set; } = Array.Empty<byte>();

    public int DisplayOrder { get; set; }

    public Guid? UploadedByUserId { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
