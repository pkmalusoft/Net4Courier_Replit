using Truebooks.Platform.Contracts.DTOs.Finance;

namespace Truebooks.Platform.Contracts.Services;

public interface IVoucherAttachmentService
{
    Task<List<VoucherAttachmentDto>> GetByTransactionIdAsync(Guid tenantId, Guid transactionId);
    Task<VoucherAttachmentDto?> UploadAsync(Guid tenantId, Guid transactionId, string fileName, string contentType, byte[] content);
    Task<byte[]?> DownloadAsync(Guid tenantId, Guid attachmentId);
    Task<bool> DeleteAsync(Guid tenantId, Guid attachmentId);
}
