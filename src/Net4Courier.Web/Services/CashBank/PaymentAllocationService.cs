using Net4Courier.Web.DTOs;

namespace Net4Courier.Web.Services.CashBank;

public class PaymentAllocationService : IPaymentAllocationService
{
    public Task<List<InvoiceForAllocationDto>> GetOutstandingInvoicesAsync(Guid tenantId, Guid customerId)
        => Task.FromResult(new List<InvoiceForAllocationDto>());

    public Task<List<BillForAllocationDto>> GetOutstandingBillsAsync(Guid tenantId, Guid vendorId)
        => Task.FromResult(new List<BillForAllocationDto>());

    public Task<List<CashBankInvoiceAllocationDto>> GetInvoiceAllocationsByReceiptAsync(Guid tenantId, Guid transactionId)
        => Task.FromResult(new List<CashBankInvoiceAllocationDto>());

    public Task<CashBankInvoiceAllocationDto?> CreateInvoiceAllocationAsync(Guid tenantId, CreateInvoiceAllocationRequest request)
        => Task.FromResult<CashBankInvoiceAllocationDto?>(null);

    public Task<bool> DeleteInvoiceAllocationAsync(Guid tenantId, Guid allocationId)
        => Task.FromResult(false);

    public Task<List<CashBankBillAllocationDto>> GetBillAllocationsByPaymentAsync(Guid tenantId, Guid transactionId)
        => Task.FromResult(new List<CashBankBillAllocationDto>());

    public Task<CashBankBillAllocationDto?> CreateBillAllocationAsync(Guid tenantId, CreateBillAllocationRequest request)
        => Task.FromResult<CashBankBillAllocationDto?>(null);

    public Task<bool> DeleteBillAllocationAsync(Guid tenantId, Guid allocationId)
        => Task.FromResult(false);
}
