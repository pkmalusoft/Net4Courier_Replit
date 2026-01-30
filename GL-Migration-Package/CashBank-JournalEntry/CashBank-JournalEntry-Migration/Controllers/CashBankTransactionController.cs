using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Truebooks.Platform.Core.Infrastructure;
using Truebooks.Platform.Core.MultiTenancy;

namespace Truebooks.Platform.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CashBankTransactionController : ControllerBase
{
    private readonly PlatformDbContext _context;
    private readonly ITenantContext _tenantContext;

    public CashBankTransactionController(PlatformDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    private static string GetTransactionTypeString(int type) => type switch
    {
        0 => "Receipt",
        1 => "Payment",
        2 => "Transfer",
        _ => "Unknown"
    };

    private static string GetStatusString(int status) => status switch
    {
        0 => "Draft",
        1 => "Posted",
        2 => "Voided",
        _ => "Unknown"
    };

    [HttpGet]
    public async Task<ActionResult<List<CashBankTransactionDto>>> GetAll([FromQuery] Guid? bankAccountId = null)
    {
        var tenantId = _tenantContext.TenantId;
        var query = _context.CashBankTransactions.Where(t => t.TenantId == tenantId);

        if (bankAccountId.HasValue)
        {
            query = query.Where(t => t.BankAccountId == bankAccountId.Value);
        }

        var transactions = await query
            .Include(t => t.BankAccount)
            .Include(t => t.Customer)
            .Include(t => t.Vendor)
            .OrderByDescending(t => t.VoucherDate)
            .ThenByDescending(t => t.CreatedAt)
            .Select(t => new CashBankTransactionDto
            {
                Id = t.Id,
                VoucherNo = t.VoucherNo,
                VoucherDate = t.VoucherDate,
                TransactionType = GetTransactionTypeString(t.TransactionType),
                BankAccountId = t.BankAccountId,
                BankAccountName = t.BankAccount != null ? t.BankAccount.AccountName : "",
                TotalAmount = t.TotalAmount,
                Status = GetStatusString(t.Status),
                CustomerId = t.CustomerId,
                CustomerName = t.Customer != null ? t.Customer.Name ?? "" : "",
                VendorId = t.VendorId,
                VendorName = t.Vendor != null ? t.Vendor.Name : "",
                ReferenceNo = t.ReferenceNo,
                ChequeNo = t.ChequeNo,
                IsVoided = t.IsVoided
            })
            .ToListAsync();

        return Ok(transactions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CashBankTransactionDetailDto>> GetById(Guid id)
    {
        var tenantId = _tenantContext.TenantId;
        var t = await _context.CashBankTransactions
            .Where(x => x.TenantId == tenantId && x.Id == id)
            .Include(x => x.BankAccount)
            .Include(x => x.Customer)
            .Include(x => x.Vendor)
            .Include(x => x.Lines)
            .FirstOrDefaultAsync();

        if (t == null)
            return NotFound();

        return Ok(new CashBankTransactionDetailDto
        {
            Id = t.Id,
            VoucherNo = t.VoucherNo,
            VoucherDate = t.VoucherDate,
            TransactionType = GetTransactionTypeString(t.TransactionType),
            RecPayType = t.RecPayType,
            TransactionCategory = t.TransactionCategory,
            SourceAccountId = t.SourceAccountId,
            BankAccountId = t.BankAccountId,
            BankAccountName = t.BankAccount?.AccountName ?? "",
            TotalAmount = t.TotalAmount,
            ChequeNo = t.ChequeNo,
            ChequeDate = t.ChequeDate,
            IsPDC = t.IsPDC,
            BankName = t.BankName,
            BranchName = t.BranchName,
            ReferenceNo = t.ReferenceNo,
            Status = GetStatusString(t.Status),
            ReceiptType = t.ReceiptType,
            CustomerId = t.CustomerId,
            CustomerName = t.Customer?.Name ?? "",
            VendorId = t.VendorId,
            VendorName = t.Vendor?.Name ?? "",
            IsVoided = t.IsVoided,
            VoidReason = t.VoidReason,
            VoidedDate = t.VoidedDate,
            Lines = t.Lines.Select(l => new CashBankTransactionLineDto
            {
                Id = l.Id,
                AccountId = l.AccountId,
                Description = l.Description,
                Amount = l.Amount,
                TaxCodeId = l.TaxCodeId,
                TaxAmount = l.TaxAmount
            }).ToList()
        });
    }

    [HttpGet("generate-voucher-no")]
    public async Task<ActionResult<string>> GenerateVoucherNo([FromQuery] int transactionType, [FromQuery] int recPayType)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return BadRequest("Tenant context not available");

        var prefix = transactionType == 0 ? (recPayType == 0 ? "BR" : "BP") : (recPayType == 0 ? "CR" : "CP");
        var year = DateTime.Now.Year;
        var month = DateTime.Now.Month;
        
        var lastVoucher = await _context.CashBankTransactions
            .Where(t => t.TenantId == tenantId.Value && t.VoucherNo.StartsWith(prefix))
            .OrderByDescending(t => t.VoucherNo)
            .Select(t => t.VoucherNo)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (!string.IsNullOrEmpty(lastVoucher))
        {
            var parts = lastVoucher.Split('-');
            if (parts.Length > 1 && int.TryParse(parts[^1], out int lastNum))
            {
                nextNumber = lastNum + 1;
            }
        }

        var voucherNo = $"{prefix}-{year}{month:D2}-{nextNumber:D5}";
        return Ok(voucherNo);
    }

    [HttpPost]
    public async Task<ActionResult<CashBankTransactionDetailDto>> Create([FromBody] CreateCashBankTransactionRequest request)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return BadRequest("Tenant context not available");

        var prefix = request.TransactionType == 0 ? (request.RecPayType == 0 ? "BR" : "BP") : (request.RecPayType == 0 ? "CR" : "CP");
        var year = DateTime.Now.Year;
        var month = DateTime.Now.Month;
        
        var lastVoucher = await _context.CashBankTransactions
            .Where(t => t.TenantId == tenantId.Value && t.VoucherNo.StartsWith(prefix))
            .OrderByDescending(t => t.VoucherNo)
            .Select(t => t.VoucherNo)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (!string.IsNullOrEmpty(lastVoucher))
        {
            var parts = lastVoucher.Split('-');
            if (parts.Length > 1 && int.TryParse(parts[^1], out int lastNum))
            {
                nextNumber = lastNum + 1;
            }
        }

        var voucherNo = $"{prefix}-{year}{month:D2}-{nextNumber:D5}";

        var transaction = new CashBankTransaction
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            VoucherNo = voucherNo,
            VoucherDate = request.VoucherDate,
            TransactionType = request.TransactionType,
            RecPayType = request.RecPayType,
            TransactionCategory = request.TransactionCategory,
            SourceAccountId = request.SourceAccountId,
            BankAccountId = request.BankAccountId,
            TotalAmount = request.TotalAmount,
            ChequeNo = request.ChequeNo,
            ChequeDate = request.ChequeDate,
            IsPDC = request.IsPDC,
            BankName = request.BankName,
            BranchName = request.BranchName,
            ReferenceNo = request.ReferenceNo,
            Status = 0,
            ReceiptType = request.ReceiptType,
            CustomerId = request.CustomerId,
            VendorId = request.VendorId,
            BranchId = request.BranchId,
            DepartmentId = request.DepartmentId,
            FiscalYear = request.FiscalYear,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var lineRequest in request.Lines)
        {
            var line = new CashBankTransactionLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId.Value,
                CashBankTransactionId = transaction.Id,
                AccountId = lineRequest.AccountId,
                Description = lineRequest.Description,
                Amount = lineRequest.Amount,
                TaxCodeId = lineRequest.TaxCodeId,
                TaxAmount = lineRequest.TaxAmount,
                CreatedAt = DateTime.UtcNow
            };
            transaction.Lines.Add(line);
        }

        _context.CashBankTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, new CashBankTransactionDetailDto
        {
            Id = transaction.Id,
            VoucherNo = transaction.VoucherNo,
            VoucherDate = transaction.VoucherDate,
            TransactionType = GetTransactionTypeString(transaction.TransactionType),
            RecPayType = transaction.RecPayType,
            TransactionCategory = transaction.TransactionCategory,
            SourceAccountId = transaction.SourceAccountId,
            BankAccountId = transaction.BankAccountId,
            TotalAmount = transaction.TotalAmount,
            Status = GetStatusString(transaction.Status)
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCashBankTransactionRequest request)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return BadRequest("Tenant context not available");

        var transaction = await _context.CashBankTransactions
            .Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId.Value && t.Id == id);

        if (transaction == null)
            return NotFound();

        if (transaction.Status != 0)
            return BadRequest("Only draft transactions can be updated");

        transaction.VoucherDate = request.VoucherDate;
        transaction.TransactionType = request.TransactionType;
        transaction.RecPayType = request.RecPayType;
        transaction.TransactionCategory = request.TransactionCategory;
        transaction.SourceAccountId = request.SourceAccountId;
        transaction.BankAccountId = request.BankAccountId;
        transaction.TotalAmount = request.TotalAmount;
        transaction.ChequeNo = request.ChequeNo;
        transaction.ChequeDate = request.ChequeDate;
        transaction.IsPDC = request.IsPDC;
        transaction.BankName = request.BankName;
        transaction.BranchName = request.BranchName;
        transaction.ReferenceNo = request.ReferenceNo;
        transaction.ReceiptType = request.ReceiptType;
        transaction.CustomerId = request.CustomerId;
        transaction.VendorId = request.VendorId;
        transaction.BranchId = request.BranchId;
        transaction.DepartmentId = request.DepartmentId;
        transaction.UpdatedAt = DateTime.UtcNow;

        _context.CashBankTransactionLines.RemoveRange(transaction.Lines);

        foreach (var lineRequest in request.Lines)
        {
            var line = new CashBankTransactionLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId.Value,
                CashBankTransactionId = transaction.Id,
                AccountId = lineRequest.AccountId,
                Description = lineRequest.Description,
                Amount = lineRequest.Amount,
                TaxCodeId = lineRequest.TaxCodeId,
                TaxAmount = lineRequest.TaxAmount,
                CreatedAt = DateTime.UtcNow
            };
            transaction.Lines.Add(line);
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("{id}/post")]
    public async Task<IActionResult> Post(Guid id)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return BadRequest("Tenant context not available");

        var transaction = await _context.CashBankTransactions
            .FirstOrDefaultAsync(t => t.TenantId == tenantId.Value && t.Id == id);

        if (transaction == null)
            return NotFound();

        if (transaction.Status != 0)
            return BadRequest("Only draft transactions can be posted");

        transaction.Status = 1;
        transaction.PostedDate = DateTime.UtcNow;
        transaction.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return BadRequest("Tenant context not available");

        var transaction = await _context.CashBankTransactions
            .Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId.Value && t.Id == id);

        if (transaction == null)
            return NotFound();

        if (transaction.Status != 0)
            return BadRequest("Only draft transactions can be deleted");

        _context.CashBankTransactionLines.RemoveRange(transaction.Lines);
        _context.CashBankTransactions.Remove(transaction);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/void")]
    public async Task<IActionResult> Void(Guid id, [FromBody] VoidCashBankTransactionRequest request)
    {
        var tenantId = _tenantContext.TenantId;
        var transaction = await _context.CashBankTransactions
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);

        if (transaction == null)
            return NotFound();

        if (transaction.IsVoided)
            return BadRequest("Transaction is already voided");

        transaction.IsVoided = true;
        transaction.VoidedDate = DateTime.UtcNow;
        transaction.VoidReason = request.Reason;
        transaction.Status = 2;
        transaction.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Transaction voided" });
    }
}

public class CashBankTransactionDto
{
    public Guid Id { get; set; }
    public string VoucherNo { get; set; } = "";
    public DateTime VoucherDate { get; set; }
    public string TransactionType { get; set; } = "";
    public Guid? BankAccountId { get; set; }
    public string BankAccountName { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "";
    public Guid? CustomerId { get; set; }
    public string CustomerName { get; set; } = "";
    public Guid? VendorId { get; set; }
    public string VendorName { get; set; } = "";
    public string? ReferenceNo { get; set; }
    public string? ChequeNo { get; set; }
    public bool IsVoided { get; set; }
}

public class CashBankTransactionDetailDto : CashBankTransactionDto
{
    public int RecPayType { get; set; }
    public int TransactionCategory { get; set; }
    public Guid SourceAccountId { get; set; }
    public DateTime? ChequeDate { get; set; }
    public bool IsPDC { get; set; }
    public string? BankName { get; set; }
    public string? BranchName { get; set; }
    public int ReceiptType { get; set; }
    public string? VoidReason { get; set; }
    public DateTime? VoidedDate { get; set; }
    public List<CashBankTransactionLineDto> Lines { get; set; } = new();
}

public class CashBankTransactionLineDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public Guid? TaxCodeId { get; set; }
    public decimal TaxAmount { get; set; }
}

public class VoidCashBankTransactionRequest
{
    public string Reason { get; set; } = "";
}

public class CreateCashBankTransactionRequest
{
    public DateTime VoucherDate { get; set; }
    public int TransactionType { get; set; }
    public int RecPayType { get; set; }
    public int TransactionCategory { get; set; }
    public Guid SourceAccountId { get; set; }
    public Guid? BankAccountId { get; set; }
    public decimal TotalAmount { get; set; }
    public string? ChequeNo { get; set; }
    public DateTime? ChequeDate { get; set; }
    public bool IsPDC { get; set; }
    public string? BankName { get; set; }
    public string? BranchName { get; set; }
    public string? ReferenceNo { get; set; }
    public int ReceiptType { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? VendorId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    public int FiscalYear { get; set; }
    public List<CreateCashBankTransactionLineRequest> Lines { get; set; } = new();
}

public class UpdateCashBankTransactionRequest
{
    public DateTime VoucherDate { get; set; }
    public int TransactionType { get; set; }
    public int RecPayType { get; set; }
    public int TransactionCategory { get; set; }
    public Guid SourceAccountId { get; set; }
    public Guid? BankAccountId { get; set; }
    public decimal TotalAmount { get; set; }
    public string? ChequeNo { get; set; }
    public DateTime? ChequeDate { get; set; }
    public bool IsPDC { get; set; }
    public string? BankName { get; set; }
    public string? BranchName { get; set; }
    public string? ReferenceNo { get; set; }
    public int ReceiptType { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? VendorId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    public List<CreateCashBankTransactionLineRequest> Lines { get; set; } = new();
}

public class CreateCashBankTransactionLineRequest
{
    public Guid AccountId { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public Guid? TaxCodeId { get; set; }
    public decimal TaxAmount { get; set; }
}
