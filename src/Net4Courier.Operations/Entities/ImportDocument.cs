using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public enum ImportDocumentType
{
    CustomsDeclaration = 1,
    CommercialInvoice = 2,
    PackingList = 3,
    BillOfLading = 4,
    AirwayBill = 5,
    CertificateOfOrigin = 6,
    InsuranceCertificate = 7,
    ImportPermit = 8,
    DeliveryOrder = 9,
    Other = 99
}

public class ImportDocument : BaseEntity
{
    public long ImportMasterId { get; set; }
    public ImportDocumentType DocumentType { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Description { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public long? UploadedByUserId { get; set; }
    public string? UploadedByUserName { get; set; }
    
    public virtual ImportMaster? ImportMaster { get; set; }
}
