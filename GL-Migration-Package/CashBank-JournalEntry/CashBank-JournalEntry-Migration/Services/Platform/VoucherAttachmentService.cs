using Truebooks.Platform.Contracts.DTOs.Finance;
using Truebooks.Platform.Contracts.Services;

namespace Truebooks.Platform.Host.Services;

public class VoucherAttachmentService : IVoucherAttachmentService
{
    public Task<List<VoucherAttachmentDto>> GetByTransactionIdAsync(Guid tenantId, Guid transactionId)
        => Task.FromResult(new List<VoucherAttachmentDto>());

    public Task<VoucherAttachmentDto?> UploadAsync(Guid tenantId, Guid transactionId, string fileName, string contentType, byte[] content)
        => Task.FromResult<VoucherAttachmentDto?>(null);

    public Task<byte[]?> DownloadAsync(Guid tenantId, Guid attachmentId)
        => Task.FromResult<byte[]?>(null);

    public Task<bool> DeleteAsync(Guid tenantId, Guid attachmentId)
        => Task.FromResult(false);
}
