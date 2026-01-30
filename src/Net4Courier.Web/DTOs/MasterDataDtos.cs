namespace Net4Courier.Web.DTOs;

public record BrandDto(
    Guid Id,
    Guid TenantId,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateBrandRequest(
    string Code,
    string Name,
    string? Description,
    bool IsActive = true
);

public record UpdateBrandRequest(
    string Code,
    string Name,
    string? Description,
    bool IsActive
);

public record ColourDto(
    Guid Id,
    Guid TenantId,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateColourRequest(
    string Code,
    string Name,
    string? Description,
    bool IsActive = true
);

public record UpdateColourRequest(
    string Code,
    string Name,
    string? Description,
    bool IsActive
);

public record SizeDto(
    Guid Id,
    Guid TenantId,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateSizeRequest(
    string Code,
    string Name,
    string? Description,
    bool IsActive = true
);

public record UpdateSizeRequest(
    string Code,
    string Name,
    string? Description,
    bool IsActive
);

public record ItemGroupDto(
    Guid Id,
    Guid TenantId,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateItemGroupRequest(
    string Code,
    string Name,
    string? Description,
    bool IsActive = true
);

public record UpdateItemGroupRequest(
    string Code,
    string Name,
    string? Description,
    bool IsActive
);

public enum TaxType
{
    Simple = 1,
    GST = 2,
    VAT = 3,
    USSalesTax = 4
}

public record TaxCodeDto(
    Guid Id,
    Guid TenantId,
    string Code,
    string Description,
    decimal Rate,
    TaxType TaxType,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
)
{
    public string TaxTypeDisplay => TaxType.ToString();
}

public record CreateTaxCodeRequest(
    string Code,
    string Description,
    decimal Rate,
    TaxType TaxType,
    bool IsActive = true
);

public record UpdateTaxCodeRequest(
    string Code,
    string Description,
    decimal Rate,
    TaxType TaxType,
    bool IsActive
);

public enum VoucherTransactionType
{
    CashBankTransaction,
    JournalEntry,
    SalesInvoice,
    CashInvoice,
    ProformaInvoice,
    PurchaseBill,
    PurchaseOrder,
    PurchaseEnquiry,
    PurchaseQuotation,
    SalesOrder,
    SalesEnquiry,
    SalesQuotation,
    SalesOrderRequest,
    GoodsReceivedNote,
    DeliveryNote,
    SalesReturn,
    StockTransfer,
    YearEndClosing,
    AWB,
    PickupRequest,
    DeliveryRunSheet
}

public record VoucherNumberingDto(
    Guid Id,
    Guid TenantId,
    VoucherTransactionType TransactionType,
    string Prefix,
    int NextNumber,
    int NumberLength,
    string Separator,
    bool IsLocked,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
)
{
    public string TransactionTypeName => TransactionType.ToString().Replace("Transaction", "").Replace("Note", " Note");
    public string StatusText => IsLocked ? "Locked" : "Unlocked";
    public string NextVoucherPreview => $"{Prefix}{Separator}{NextNumber.ToString().PadLeft(NumberLength, '0')}";
}

public record CreateVoucherNumberingRequest(
    VoucherTransactionType TransactionType,
    string Prefix,
    int NextNumber,
    int NumberLength,
    string Separator,
    bool IsActive = true
);

public record UpdateVoucherNumberingRequest(
    string Prefix,
    int NextNumber,
    int NumberLength,
    string Separator,
    bool IsActive
);

public enum AccountType
{
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Revenue = 4,
    Expense = 5
}

public record AccountClassificationDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateAccountClassificationRequest(
    string Name,
    string? Description,
    bool IsActive = true
);

public record UpdateAccountClassificationRequest(
    string Name,
    string? Description,
    bool IsActive
);

public record ChartOfAccountDto(
    Guid Id,
    Guid TenantId,
    string? AccountCode,
    string AccountName,
    AccountType AccountType,
    Guid? ParentAccountId,
    string? ParentAccountName,
    bool IsActive,
    bool AllowPosting,
    Guid? AccountClassificationId,
    string? AccountClassificationName,
    int ControlAccountType,
    bool IsSystemAccount,
    DateTime CreatedAt,
    DateTime? UpdatedAt
)
{
    public string AccountTypeDisplay => AccountType.ToString();
    public string FullAccountDisplay => string.IsNullOrEmpty(AccountCode) ? AccountName : $"{AccountCode} - {AccountName}";
}

public record ChartOfAccountHierarchyDto(
    Guid Id,
    Guid TenantId,
    string? AccountCode,
    string AccountName,
    AccountType AccountType,
    Guid? ParentAccountId,
    string? ParentAccountName,
    bool IsActive,
    bool AllowPosting,
    Guid? AccountClassificationId,
    string? AccountClassificationName,
    int ControlAccountType,
    bool IsSystemAccount,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<ChartOfAccountHierarchyDto> SubAccounts
)
{
    public string AccountTypeDisplay => AccountType.ToString();
    public string FullAccountDisplay => string.IsNullOrEmpty(AccountCode) ? AccountName : $"{AccountCode} - {AccountName}";
}

public record CreateChartOfAccountRequest(
    string? AccountCode,
    string AccountName,
    AccountType AccountType,
    Guid? ParentAccountId,
    bool AllowPosting,
    Guid? AccountClassificationId,
    int ControlAccountType = 0,
    bool IsActive = true
);

public record UpdateChartOfAccountRequest(
    string? AccountCode,
    string AccountName,
    AccountType AccountType,
    Guid? ParentAccountId,
    bool AllowPosting,
    Guid? AccountClassificationId,
    int ControlAccountType,
    bool IsActive
);

public record CurrencyDto(
    Guid Id,
    Guid TenantId,
    string Code,
    string Name,
    string Symbol,
    bool IsBaseCurrency,
    bool IsActive
);

public record CreateCurrencyRequest(
    string Code,
    string Name,
    string Symbol,
    bool IsBaseCurrency,
    bool IsActive
);

public record UpdateCurrencyRequest(
    Guid Id,
    string Code,
    string Name,
    string Symbol,
    bool IsBaseCurrency,
    bool IsActive
);

public record BranchDto(
    Guid Id,
    Guid TenantId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string Code,
    string Name,
    string? Address,
    Guid? CountryId,
    Guid? StateId,
    Guid? CityId,
    string? CountryName,
    string? StateName,
    string? CityName,
    string? PostalCode,
    string? Phone,
    string? Email,
    string? TaxRegistrationNumber,
    string? CurrencyCode,
    bool IsHeadOffice,
    bool EnableLogin,
    bool IsActive,
    int SortOrder
);

public record CreateBranchRequest(
    string Code,
    string Name,
    string? Address,
    Guid? CountryId,
    Guid? StateId,
    Guid? CityId,
    string? PostalCode,
    string? Phone,
    string? Email,
    string? TaxRegistrationNumber,
    string? CurrencyCode,
    bool IsHeadOffice,
    bool EnableLogin,
    bool IsActive,
    int SortOrder
);

public record UpdateBranchRequest(
    Guid Id,
    string Code,
    string Name,
    string? Address,
    Guid? CountryId,
    Guid? StateId,
    Guid? CityId,
    string? PostalCode,
    string? Phone,
    string? Email,
    string? TaxRegistrationNumber,
    string? CurrencyCode,
    bool IsHeadOffice,
    bool EnableLogin,
    bool IsActive,
    int SortOrder
);

public record DepartmentDto(
    Guid Id,
    Guid TenantId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string Code,
    string Name,
    string? Description,
    Guid? BranchId,
    string? BranchName,
    Guid? ParentDepartmentId,
    string? ParentDepartmentName,
    bool IsActive,
    int SortOrder,
    List<DepartmentDto> ChildDepartments
);

public record CreateDepartmentRequest(
    string Code,
    string Name,
    string? Description,
    Guid? BranchId,
    Guid? ParentDepartmentId,
    bool IsActive,
    int SortOrder
);

public record UpdateDepartmentRequest(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    Guid? BranchId,
    Guid? ParentDepartmentId,
    bool IsActive,
    int SortOrder
);

public record ProjectDto(
    Guid Id,
    Guid TenantId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    string DisplayName
);

public record CreateProjectRequest(
    string Code,
    string Name,
    string? Description,
    bool IsActive
);

public record UpdateProjectRequest(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive
);

public record GridPreferenceDto(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    string GridName,
    string ColumnSettings,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record SaveGridPreferenceRequest(
    string GridName,
    string ColumnSettings
);

public record ControlAccountAssignmentDto(
    Guid AccountId,
    string AccountCode,
    string AccountName
);
