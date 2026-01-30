# GL Setup Pages Package

This package contains the General Ledger setup pages extracted from Truebooks-ERP for integration into Net4Courier.

## Included Pages

### MasterData Pages (8 files)
| File | Route | Purpose |
|------|-------|---------|
| ChartOfAccountsList.razor | /master-data/chart-of-accounts | View/manage chart of accounts |
| ChartOfAccountsEdit.razor | /master-data/chart-of-accounts/edit/{id} | Create/edit GL accounts |
| Currencies.razor | /master-data/currencies | Currency list |
| CurrencyEdit.razor | /master-data/currencies/edit/{id} | Create/edit currencies |
| TaxCodeList.razor | /master-data/tax-codes | Tax code list |
| TaxCodeEdit.razor | /master-data/tax-codes/edit/{id} | Create/edit tax codes |
| VoucherNumberingList.razor | /master-data/voucher-numbering | Document numbering list |
| VoucherNumberingEdit.razor | /master-data/voucher-numbering/edit/{id} | Edit numbering rules |

### Settings Pages (1 file)
| File | Route | Purpose |
|------|-------|---------|
| ControlAccountsSettings.razor | /settings/control-accounts | Map control accounts (AR, AP, etc.) |

## Integration Steps

### Step 1: Copy Files
Copy the `Pages` folder to your Blazor project:
```
Net4Courier/
├── Components/
│   └── Pages/
│       ├── MasterData/          ← Copy here
│       │   ├── ChartOfAccountsList.razor
│       │   ├── ChartOfAccountsEdit.razor
│       │   ├── Currencies.razor
│       │   ├── CurrencyEdit.razor
│       │   ├── TaxCodeList.razor
│       │   ├── TaxCodeEdit.razor
│       │   ├── VoucherNumberingList.razor
│       │   └── VoucherNumberingEdit.razor
│       └── Settings/            ← Copy here
│           └── ControlAccountsSettings.razor
```

### Step 2: Update Namespaces
Replace namespace references in each file:
- FROM: `Truebooks.CommonModules.UI.Pages.MasterData`
- TO: `Net4Courier.Components.Pages.MasterData`

### Step 3: Update Service References
These pages depend on the following services (from Platform.Core/Platform.Contracts):

| Service Interface | Purpose |
|-------------------|---------|
| IChartOfAccountsService | CRUD for GL accounts |
| ICurrencyService | Currency management |
| ITaxCodeService | Tax code management |
| IVoucherNumberingService | Document numbering |
| IControlAccountService | Control account mapping |

You can either:
1. Use the NuGet packages from GL-Migration-Package (recommended)
2. Create your own service implementations matching these interfaces

### Step 4: Update Route Prefixes (Optional)
The pages use `@page` directives. You may want to change routes to match Net4Courier's URL structure.

Example:
```razor
@page "/master-data/chart-of-accounts"
// Change to:
@page "/gl/chart-of-accounts"
```

### Step 5: Update Navigation
Add menu items in your NavMenu.razor:
```razor
<MudNavLink Href="/master-data/chart-of-accounts" Icon="@Icons.Material.Filled.AccountTree">Chart of Accounts</MudNavLink>
<MudNavLink Href="/master-data/currencies" Icon="@Icons.Material.Filled.CurrencyExchange">Currencies</MudNavLink>
<MudNavLink Href="/master-data/tax-codes" Icon="@Icons.Material.Filled.Receipt">Tax Codes</MudNavLink>
<MudNavLink Href="/master-data/voucher-numbering" Icon="@Icons.Material.Filled.Numbers">Voucher Numbering</MudNavLink>
```

## Dependencies

These pages require:
- MudBlazor 7.16.0
- Microsoft.AspNetCore.Components.Web 8.0.8
- Services from Truebooks.Platform.Core or equivalent implementations

## Notes

1. **Authentication**: Pages use `[Authorize]` attribute - ensure your auth is configured
2. **Tenant Context**: Pages reference `TenantId` - adapt for single-tenant if needed
3. **_Imports.razor**: Add necessary `@using` statements to your _Imports.razor file
