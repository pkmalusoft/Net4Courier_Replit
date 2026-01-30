# Cash & Bank and Journal Entry Module Integration Guide

## Overview

This package contains all the files needed to migrate the Cash & Bank and Journal Entry modules from Truebooks-ERP to a standalone project.

## Package Contents

```
CashBank-JournalEntry-Migration/
├── UI/
│   ├── CashBank/
│   │   ├── CashBankTransactionEdit.razor    # Transaction entry/edit page
│   │   └── CashBankTransactionList.razor    # Transaction list page
│   ├── BankAccounts/
│   │   ├── BankAccountEdit.razor            # Bank account entry/edit
│   │   └── BankAccounts.razor               # Bank accounts list
│   └── JournalEntry/
│       ├── JournalEntry.razor               # Journal entry page
│       └── JournalVoucherList.razor         # Journal voucher list
├── Services/
│   ├── Platform/
│   │   ├── ICashBankTransactionService.cs   # Interface
│   │   ├── CashBankTransactionService.cs    # Implementation
│   │   ├── IBankAccountService.cs           # Interface
│   │   ├── BankAccountService.cs            # Implementation
│   │   ├── IPaymentAllocationService.cs     # Interface
│   │   ├── PaymentAllocationService.cs      # Implementation
│   │   ├── IVoucherAttachmentService.cs     # Interface
│   │   ├── VoucherAttachmentService.cs      # Implementation
│   │   ├── IJournalEntryService.cs          # Interface
│   │   └── JournalEntryService.cs           # Implementation
│   └── Legacy/
│       ├── CashBankTransactionService.cs    # Legacy HTTP client service
│       └── IJournalEntryService.cs          # Legacy interface
├── Controllers/
│   ├── CashBankTransactionController.cs     # Cash/Bank API
│   ├── CashBankRegisterController.cs        # Register report API
│   ├── BankAccountController.cs             # Bank accounts API
│   └── JournalEntryController.cs            # Journal entry API
├── DTOs/
│   ├── CashBankDtos.cs                      # Cash/Bank DTOs
│   ├── CashBankModels.cs                    # Legacy models
│   ├── CashBankEnums.cs                     # Enums
│   ├── TransactionDtos.cs                   # Transaction DTOs
│   └── TransactionModels.cs                 # Transaction models
├── Entities/
│   ├── CashBankTransaction.cs
│   ├── CashBankTransactionLine.cs
│   ├── CashBankAllocation.cs
│   ├── CashBankInvoiceAllocation.cs
│   ├── CashBankBillAllocation.cs
│   ├── VoucherAttachment.cs
│   ├── BankAccount.cs
│   └── JournalEntry.cs
└── Schema/
    └── database_schema.sql                   # Database tables
```

## Integration Steps

### 1. Database Setup

Run the SQL script in `Schema/database_schema.sql` to create the required tables. The script includes:

- BankAccounts
- CashBankTransactions
- CashBankTransactionLines
- InvoicePaymentAllocations
- BillPaymentAllocations
- VoucherAttachments
- JournalEntries
- JournalEntryLines

**Prerequisites:** The GL module must be installed first as these tables reference:
- ChartOfAccounts
- Currencies
- Customers
- Suppliers (Vendors)
- Projects
- Invoices (for payment allocation)
- Bills (for payment allocation)

### 2. Single-Tenant Adaptation

For single-tenant deployment, update the services to remove tenant filtering:

```csharp
// Before (multi-tenant)
public async Task<IEnumerable<CashBankTransactionDto>> GetAllAsync(Guid tenantId, ...)
{
    var query = context.CashBankTransactions.Where(t => t.TenantId == tenantId);
}

// After (single-tenant)
public async Task<IEnumerable<CashBankTransactionDto>> GetAllAsync(...)
{
    var query = context.CashBankTransactions;
}
```

### 3. DbContext Configuration

Add the following DbSets to your DbContext:

```csharp
public class AppDbContext : DbContext
{
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<CashBankTransaction> CashBankTransactions => Set<CashBankTransaction>();
    public DbSet<CashBankTransactionLine> CashBankTransactionLines => Set<CashBankTransactionLine>();
    public DbSet<InvoicePaymentAllocation> InvoicePaymentAllocations => Set<InvoicePaymentAllocation>();
    public DbSet<BillPaymentAllocation> BillPaymentAllocations => Set<BillPaymentAllocation>();
    public DbSet<VoucherAttachment> VoucherAttachments => Set<VoucherAttachment>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
}
```

### 4. Service Registration

Register services in Program.cs:

```csharp
// Use IDbContextFactory for concurrent access support
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Cash & Bank Services
builder.Services.AddScoped<ICashBankTransactionService, CashBankTransactionService>();
builder.Services.AddScoped<IBankAccountService, BankAccountService>();
builder.Services.AddScoped<IPaymentAllocationService, PaymentAllocationService>();
builder.Services.AddScoped<IVoucherAttachmentService, VoucherAttachmentService>();

// Journal Entry Services
builder.Services.AddScoped<IJournalEntryService, JournalEntryService>();
```

### 5. API Routes

The controllers provide these API endpoints:

**Cash & Bank Transactions:**
- GET `/api/cash-bank-transactions` - List transactions
- GET `/api/cash-bank-transactions/{id}` - Get by ID
- POST `/api/cash-bank-transactions` - Create
- PUT `/api/cash-bank-transactions/{id}` - Update
- POST `/api/cash-bank-transactions/{id}/post` - Post transaction
- POST `/api/cash-bank-transactions/{id}/void` - Void transaction

**Bank Accounts:**
- GET `/api/bank-accounts` - List accounts
- GET `/api/bank-accounts/{id}` - Get by ID
- POST `/api/bank-accounts` - Create
- PUT `/api/bank-accounts/{id}` - Update

**Journal Entries:**
- GET `/api/journal-entries` - List entries
- GET `/api/journal-entries/{id}` - Get by ID
- POST `/api/journal-entries` - Create
- PUT `/api/journal-entries/{id}` - Update
- POST `/api/journal-entries/{id}/post` - Post entry

### 6. UI Page Routes

Configure these routes in your Blazor app:

```csharp
// In _Imports.razor or NavMenu
@page "/cash-bank-transactions"           // List page
@page "/cash-bank-transactions/new"       // Create new
@page "/cash-bank-transactions/{Id:guid}" // Edit existing

@page "/bank-accounts"                    // List page
@page "/bank-accounts/new"                // Create new
@page "/bank-accounts/{Id:guid}"          // Edit existing

@page "/journal-entries"                  // List page
@page "/journal-entries/new"              // Create new
@page "/journal-entries/{Id:guid}"        // Edit existing
```

## Key Features

### Cash & Bank Transactions
- Receipt and Payment processing
- Cash, Bank, and Cheque modes
- Customer receipts with invoice allocation
- Vendor payments with bill allocation
- PDC (Post-Dated Cheque) handling
- TDS deduction support
- File attachments
- Automatic journal entry generation
- Void/reversal capability

### Bank Accounts
- Multiple bank account management
- Link to Chart of Accounts
- Multi-currency support
- Opening balance tracking
- Active/Inactive status

### Journal Entries
- Manual journal voucher creation
- Multi-line entries
- Debit/Credit balancing
- Project/Cost center allocation
- Source tracking (manual, system-generated)

## Dependencies

This module requires these services from the GL module:
- IChartOfAccountsService
- IFiscalPeriodService
- ICustomerService
- ISupplierService
- IProjectService
- IBranchService
- IDepartmentService
- ICurrencyService

## Threading Pattern

**IMPORTANT:** All services use `IDbContextFactory<AppDbContext>` instead of direct `DbContext` injection. This prevents "second operation started on this context" errors when the UI loads multiple dropdowns concurrently.

```csharp
public class CashBankTransactionService : ICashBankTransactionService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public CashBankTransactionService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<CashBankTransactionDto?> GetByIdAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Use context here...
    }
}
```

## Enums Reference

```csharp
public enum TransactionType { Receipt = 0, Payment = 1 }
public enum RecPayType { Cash = 0, Bank = 1, Cheque = 2 }
public enum TransactionCategory { Regular = 0, CustomerReceipt = 1, VendorPayment = 2 }
public enum TransactionStatus { Draft = 0, Posted = 1, Voided = 2 }
public enum DepositStatus { NotDeposited = 0, Deposited = 1 }
public enum ClearanceStatus { NotCleared = 0, Cleared = 1, Bounced = 2 }
```

## Notes

1. **Voucher Number Generation:** Implement your own voucher number sequence generator.

2. **Fiscal Year:** The FiscalYear field is used for partitioning and reporting. Ensure your system has fiscal year management.

3. **Journal Entry Integration:** When transactions are posted, they create corresponding journal entries. Review the posting logic in CashBankTransactionService.

4. **Payment Allocation:** The allocation services handle matching payments to invoices/bills. Adjust for your invoice/bill table structure.
