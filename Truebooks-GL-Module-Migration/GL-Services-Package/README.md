# GL Services Package

This package contains the complete GL service layer source code extracted from Truebooks-ERP for integration into Net4Courier.

## Package Contents

### Interfaces (5 files)
| File | Purpose |
|------|---------|
| IChartOfAccountsService.cs | Chart of Accounts CRUD operations |
| ICurrencyService.cs | Currency management |
| ITaxCodeService.cs | Tax code management |
| IVoucherNumberingService.cs | Document numbering |
| IAccountClassificationService.cs | Account type classifications |

### Service Implementations (5 files)
| File | Size | Purpose |
|------|------|---------|
| ChartOfAccountsService.cs | 15KB | Full Chart of Accounts logic |
| CurrencyService.cs | 5KB | Currency CRUD operations |
| TaxCodeService.cs | 5KB | Tax code management |
| VoucherNumberingService.cs | 6KB | Auto-numbering generation |
| AccountClassificationService.cs | 4KB | Account classifications |

### API Controllers (5 files)
| File | Purpose |
|------|---------|
| ChartOfAccountController.cs | Chart of Accounts API endpoints |
| CurrencyController.cs | Currency API endpoints |
| TaxCodeController.cs | Tax code API endpoints |
| VoucherNumberingController.cs | Voucher numbering API endpoints |
| AccountClassificationController.cs | Account classification API endpoints |

### DTOs (2 files)
| File | Purpose |
|------|---------|
| GLDtos.cs | GL-related data transfer objects |
| MasterDataDtos.cs | Master data DTOs |

## Integration Steps

### Step 1: Copy Files to Net4Courier

```
Net4Courier/
├── Interfaces/
│   ├── IChartOfAccountsService.cs
│   ├── ICurrencyService.cs
│   ├── ITaxCodeService.cs
│   ├── IVoucherNumberingService.cs
│   └── IAccountClassificationService.cs
├── Services/
│   ├── ChartOfAccountsService.cs
│   ├── CurrencyService.cs
│   ├── TaxCodeService.cs
│   ├── VoucherNumberingService.cs
│   └── AccountClassificationService.cs
├── Controllers/
│   ├── ChartOfAccountController.cs
│   ├── CurrencyController.cs
│   ├── TaxCodeController.cs
│   ├── VoucherNumberingController.cs
│   └── AccountClassificationController.cs
└── DTOs/
    ├── GLDtos.cs
    └── MasterDataDtos.cs
```

### Step 2: Update Namespaces

Replace in all files:
```csharp
// FROM:
namespace Truebooks.Platform.Contracts.Services
namespace Truebooks.Platform.Host.Services
namespace Truebooks.Platform.Host.Controllers

// TO:
namespace Net4Courier.Interfaces
namespace Net4Courier.Services
namespace Net4Courier.Controllers
```

### Step 3: Update DbContext References

The services use `PlatformDbContext`. Update to use your DbContext:
```csharp
// FROM:
private readonly IDbContextFactory<PlatformDbContext> _dbContextFactory;

// TO:
private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
```

### Step 4: Remove Multi-Tenant Logic (For Single-Tenant)

If Net4Courier is single-tenant, remove `TenantId` filtering:
```csharp
// FROM:
.Where(x => x.TenantId == tenantId)

// TO:
// Remove or use a fixed tenant ID
```

### Step 5: Register Services in Program.cs

```csharp
// Add to Program.cs
builder.Services.AddScoped<IChartOfAccountsService, ChartOfAccountsService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<ITaxCodeService, TaxCodeService>();
builder.Services.AddScoped<IVoucherNumberingService, VoucherNumberingService>();
builder.Services.AddScoped<IAccountClassificationService, AccountClassificationService>();
```

### Step 6: Add Required NuGet Packages

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

## Dependencies

These services require:
- Entity Framework Core 8.0
- Npgsql (PostgreSQL provider)
- Your AppDbContext with GL entity configurations

## Entity Requirements

Ensure your DbContext has these entities configured:
- ChartOfAccount
- Currency
- TaxCode
- VoucherNumberSequence
- AccountClassification

Refer to `GL_DATABASE_SCHEMA.sql` from GL-Migration-Package for table structures.

## Notes

1. **IDbContextFactory Pattern**: Services use factory pattern to avoid threading issues in Blazor Server
2. **Async Operations**: All methods are async - maintain this pattern
3. **Validation**: Services include validation logic - review and adapt as needed
