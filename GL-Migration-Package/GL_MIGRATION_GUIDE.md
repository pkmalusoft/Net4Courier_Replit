# GL Module Migration Guide

## Overview

This guide helps you integrate the General Ledger (GL) module from Truebooks-ERP into your Net4Courier project.

**Important**: This is a starting foundation. The GL module was designed for Truebooks-ERP's multi-tenant architecture. When integrated into Net4Courier, you may need to adapt authentication, routing, and some services to match your existing setup.

## Included NuGet Packages

| Package | Description |
|---------|-------------|
| `Truebooks.Platform.Contracts.1.0.0.nupkg` | Interfaces, DTOs, and events |
| `Truebooks.Platform.Core.1.0.0.nupkg` | DbContext, entities, infrastructure |
| `Truebooks.Platform.Finance.1.0.0.nupkg` | Financial services (GL, AR, AP) |
| `Truebooks.Shared.UI.1.0.0.nupkg` | Shared Blazor components |
| `Truebooks.AccountsFinance.GL.UI.1.0.0.nupkg` | GL UI pages (Chart of Accounts, Journal Entry) |
| `Truebooks.Reports.GL.UI.1.0.0.nupkg` | GL Reports (Trial Balance, P&L, Balance Sheet) |

---

## Step 1: Setup NuGet Feed

### Option A: Copy Packages to Project

1. Create folder `NuGet/packages` in Net4Courier root
2. Copy all `.nupkg` files to that folder
3. Create `NuGet.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="Truebooks" value="./NuGet/packages" />
  </packageSources>
</configuration>
```

### Option B: Use Shared Folder

Reference the packages from a shared location using absolute path.

---

## Step 2: Add Package References

Add to your `.csproj` file:

```xml
<ItemGroup>
  <PackageReference Include="Truebooks.Platform.Contracts" Version="1.0.0" />
  <PackageReference Include="Truebooks.Platform.Core" Version="1.0.0" />
  <PackageReference Include="Truebooks.Platform.Finance" Version="1.0.0" />
  <PackageReference Include="Truebooks.Shared.UI" Version="1.0.0" />
  <PackageReference Include="Truebooks.AccountsFinance.GL.UI" Version="1.0.0" />
  <PackageReference Include="Truebooks.Reports.GL.UI" Version="1.0.0" />
</ItemGroup>
```

---

## Step 3: Database Setup

### Option A: Use the SQL Script (Recommended for New Projects)

Run the SQL script `GL_DATABASE_SCHEMA.sql` included in this package against your PostgreSQL database.

### Option B: Use Entity Framework Migrations (If Using EF Core)

If your project uses EF Core, you can let EF create the tables:

```bash
dotnet ef database update
```

The `PlatformDbContext` in `Truebooks.Platform.Core` contains all entity configurations.

### Required Tables

| Table | Purpose |
|-------|---------|
| `Tenants` | Multi-tenant support (can use single tenant) |
| `Modules` | Module definitions |
| `TenantModules` | Modules enabled per tenant |
| `ChartOfAccounts` | Account master data |
| `AccountClassifications` | Account categories |
| `JournalEntries` | Transaction headers |
| `JournalEntryLines` | Transaction details |
| `FinancialPeriods` | Fiscal periods |
| `FinancialCalendars` | Fiscal year definitions |
| `Currencies` | Currency master |
| `ExchangeRates` | Exchange rate history |
| `OpeningBalanceBatches` | Opening balance headers |
| `OpeningBalanceLines` | Opening balance details |
| `Customers` | Customer master (for AR) |
| `Suppliers` | Supplier master (for AP) |

---

## Step 4: Register Services in Program.cs

```csharp
using Truebooks.Platform.Core.Infrastructure;
using Truebooks.Platform.Finance.Services;

// DbContext with Factory pattern (required for Blazor)
builder.Services.AddDbContextFactory<PlatformDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Core services
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<IChartOfAccountsService, ChartOfAccountsService>();
builder.Services.AddScoped<IJournalEntryService, JournalEntryService>();
builder.Services.AddScoped<IFinancialPeriodService, FinancialPeriodService>();

// For single-tenant setup, create a simple TenantContext:
public class SingleTenantContext : ITenantContext
{
    public Guid TenantId => Guid.Parse("YOUR-FIXED-TENANT-GUID");
    public string? TenantName => "MyCompany";
}
```

---

## Step 5: Add Routes in App.razor

```razor
@using Truebooks.AccountsFinance.GL.UI.Pages.GL
@using Truebooks.Reports.GL.UI.Pages.Reports

<Router AppAssembly="@typeof(App).Assembly"
        AdditionalAssemblies="new[] { 
            typeof(Truebooks.AccountsFinance.GL.UI._Imports).Assembly,
            typeof(Truebooks.Reports.GL.UI._Imports).Assembly 
        }">
```

---

## Step 6: Add MudBlazor (Required)

```csharp
// In Program.cs
builder.Services.AddMudServices();

// In _Host.cshtml or index.html
<link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
<script src="_content/MudBlazor/MudBlazor.min.js"></script>
```

---

## GL Module Pages

| Route | Page | Description |
|-------|------|-------------|
| `/gl/chart-of-accounts` | ChartOfAccounts.razor | Manage accounts |
| `/gl/journal-entry` | JournalEntry.razor | Create/edit journals |
| `/gl/journal-entry/{id}` | JournalEntry.razor | Edit specific journal |
| `/gl/journal-list` | JournalList.razor | View all journals |
| `/gl/opening-balance` | OpeningBalance.razor | Set opening balances |
| `/reports/trial-balance` | TrialBalance.razor | Trial Balance report |
| `/reports/profit-loss` | ProfitLoss.razor | P&L Statement |
| `/reports/balance-sheet` | BalanceSheet.razor | Balance Sheet |
| `/reports/ledger` | GeneralLedger.razor | Ledger report |

---

## Single-Tenant Adaptation

For single-tenant use:

1. Create one record in `Tenants` table with a fixed GUID
2. Use the `SingleTenantContext` class shown above
3. All data will be scoped to that tenant ID

---

## Required Seed Data

After creating the database, you must seed the following data:

### 1. Create a Tenant
```sql
INSERT INTO "Tenants" ("Id", "Name", "Subdomain", "IsActive", "HasConfiguredModules", "CreatedAt")
VALUES ('11111111-1111-1111-1111-111111111111', 'My Company', 'mycompany', true, true, NOW());
```

### 2. Create Base Currency
```sql
INSERT INTO "Currencies" ("Id", "TenantId", "Code", "Name", "Symbol", "IsBaseCurrency", "IsActive", "CreatedAt")
VALUES (uuid_generate_v4(), '11111111-1111-1111-1111-111111111111', 'USD', 'US Dollar', '$', true, true, NOW());
```

### 3. Create Account Classifications
```sql
INSERT INTO "AccountClassifications" ("Id", "TenantId", "Name", "AccountType", "DisplayOrder", "IsSystem", "IsActive", "CreatedAt")
VALUES 
(uuid_generate_v4(), '11111111-1111-1111-1111-111111111111', 'Assets', 1, 1, true, true, NOW()),
(uuid_generate_v4(), '11111111-1111-1111-1111-111111111111', 'Liabilities', 2, 2, true, true, NOW()),
(uuid_generate_v4(), '11111111-1111-1111-1111-111111111111', 'Equity', 3, 3, true, true, NOW()),
(uuid_generate_v4(), '11111111-1111-1111-1111-111111111111', 'Revenue', 4, 4, true, true, NOW()),
(uuid_generate_v4(), '11111111-1111-1111-1111-111111111111', 'Expenses', 5, 5, true, true, NOW());
```

### 4. Create Financial Calendar & Periods
```sql
-- Create calendar
INSERT INTO "FinancialCalendars" ("Id", "TenantId", "Code", "Name", "IsDefault", "IsActive", "CreatedAt")
VALUES (uuid_generate_v4(), '11111111-1111-1111-1111-111111111111', 'FY2025', 'Fiscal Year 2025', true, true, NOW());

-- Create 12 periods (one for each month)
INSERT INTO "FinancialPeriods" ("Id", "TenantId", "FiscalYear", "PeriodNumber", "PeriodName", "StartDate", "EndDate", "Status", "CreatedAt")
VALUES 
(uuid_generate_v4(), '11111111-1111-1111-1111-111111111111', 2025, 1, 'January', '2025-01-01', '2025-01-31', 1, NOW()),
(uuid_generate_v4(), '11111111-1111-1111-1111-111111111111', 2025, 2, 'February', '2025-02-01', '2025-02-28', 1, NOW()),
-- ... continue for all 12 months
(uuid_generate_v4(), '11111111-1111-1111-1111-111111111111', 2025, 12, 'December', '2025-12-01', '2025-12-31', 1, NOW());
```

### 5. Enable AccountsFinance Module
```sql
INSERT INTO "TenantModules" ("Id", "TenantId", "ModuleId", "Status", "EnabledOn", "CreatedAt")
SELECT uuid_generate_v4(), '11111111-1111-1111-1111-111111111111', "Id", 1, NOW(), NOW()
FROM "Modules" WHERE "Code" = 'AccountsFinance';
```

---

## Dependencies

| Package | Version |
|---------|---------|
| .NET | 8.0 |
| MudBlazor | 7.16.0 |
| Microsoft.EntityFrameworkCore | 8.0.8 |
| Npgsql.EntityFrameworkCore.PostgreSQL | 8.0.3 |

---

## Files in This Package

- `GL_MIGRATION_GUIDE.md` - This file
- `GL_DATABASE_SCHEMA.sql` - Complete database schema
- `*.nupkg` - NuGet packages

---

## Support

For issues, refer to the original Truebooks-ERP project documentation.
