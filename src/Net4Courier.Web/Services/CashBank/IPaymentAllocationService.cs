using Net4Courier.Web.DTOs;

namespace Net4Courier.Web.Services.CashBank;

public interface IPaymentAllocationService
{
    Task<List<InvoiceForAllocationDto>> GetOutstandingInvoicesAsync(Guid tenantId, Guid customerId);
    Task<List<BillForAllocationDto>> GetOutstandingBillsAsync(Guid tenantId, Guid vendorId);
    
    Task<List<CashBankInvoiceAllocationDto>> GetInvoiceAllocationsByReceiptAsync(Guid tenantId, Guid transactionId);
    Task<CashBankInvoiceAllocationDto?> CreateInvoiceAllocationAsync(Guid tenantId, CreateInvoiceAllocationRequest request);
    Task<bool> DeleteInvoiceAllocationAsync(Guid tenantId, Guid allocationId);
    
    Task<List<CashBankBillAllocationDto>> GetBillAllocationsByPaymentAsync(Guid tenantId, Guid transactionId);
    Task<CashBankBillAllocationDto?> CreateBillAllocationAsync(Guid tenantId, CreateBillAllocationRequest request);
    Task<bool> DeleteBillAllocationAsync(Guid tenantId, Guid allocationId);
}
