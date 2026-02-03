# Net4Courier - Blazor Server Migration

## Overview
Net4Courier is a comprehensive logistics management system designed to streamline courier operations. Its primary purpose is to manage shipments (AWB), customer relations, branch operations, financial transactions, and reporting. The project aims to replace legacy systems with a modern, modular, and scalable platform to enhance operational efficiency and provide real-time insights for logistics companies. Key capabilities include financial management, operations workflow, prepaid AWB management, customer relationship management (CRM), and regulatory compliance.

## User Preferences
- MudBlazor for all UI components
- Modular architecture like TrueBookserp pattern
- PostgreSQL partitioning by TransactionDate for performance
- Party/PartyAddress normalization for storage efficiency
- Navigation menu organized into 9 main sections: Dashboards, Shipments & Operations, Import/Export, Customers & CRM, Pricing & Billing, Finance & Accounting, Compliance & Audit, Masters & Settings, Knowledge & Tools

## System Architecture
The application is built on .NET 8 Blazor Server, adopting a modular architecture for maintainability, scalability, and independent feature development.

### Core Architectural Patterns
- **Modular Design**: Organized into independent modules (`Net4Courier.Web`, `Net4Courier.Infrastructure`, `Net4Courier.Kernel`, `Net4Courier.Masters`, `Net4Courier.Operations`, `Net4Courier.Finance`) for clear separation of concerns.
- **Data Persistence**: Utilizes Entity Framework Core with PostgreSQL.
- **UI Framework**: MudBlazor for all user interface elements.
- **Authentication**: Custom authentication system with BCrypt for password hashing.
- **Database Partitioning**: PostgreSQL table partitioning for performance.

### UI/UX Decisions
- **MudBlazor Components**: Ensures a consistent and responsive user experience.
- **Layout**: Standard dashboard with `MainLayout`, `NavMenu`, and organized navigation.
- **Responsive Design**: Key modules are optimized for mobile access.
- **Login Page**: Modern split-screen design.

### Technical Implementations
- **Entity Management**: Comprehensive CRUD operations for core entities (Company, Branch, User, Role, Financial Year, Parties, AWB, Invoices, Receipts).
- **Financial Features**: Includes invoice/receipt management, journaling, self-referential account heads, financial period management, dynamic other charges, AR/AP, Cash/Bank Vouchers, Bank Account Management, and 23 comprehensive financial reports.
- **Operations Workflow**: Covers AWB entry/generation, pickup management, outscan/DRS, AWB tracking, POD, RTS, MAWB processing, COD Remittance, and Transfer Order management.
- **Prepaid AWB Management**: AWB Stock Management and Prepaid AWB Sales module with integrated accounting. AWB stock fields (Origin, Destination, No. of AWBs, Courier Charge, AWB Stock, Start/End AWB Number) can be enabled/disabled per branch via Branch Settings toggle.
- **Enhanced Dashboards**: Unified dispatcher view and global autocomplete search across AWBs, Customers, and Invoices.
- **Unified Shipment Processing**: Single pages for Import Shipment Charges, Warehouse Inscan, and Shipment Lists for both domestic and import AWBs.
- **Master Data Management**: Configurable Rate Card Management, Service Type, Shipment Mode, Port Master, Currency, Geographic Master Data, and enhanced Customer Master with configurable account types and SLA management.
- **Enhanced Rate Card System**: Rate cards support ServiceType and ShipmentMode filtering, zone categories with agent-based movement restrictions (ForwardingAgent for Export/Import/Transhipment, DeliveryAgent for Domestic), and flexible slab pricing with five calculation modes:
  - FlatForSlab: Fixed amount for entire slab range
  - PerKg: Rate multiplied by weight in slab
  - PerStep: Rate multiplied by ceiling of (weight ÷ increment)
  - FlatAfter: Flat rate once weight exceeds threshold
  - FlatPlusAdditional: Flat rate up to max weight + per-kg rate for additional weight (e.g., 1-5kg = AED 20 flat, then AED 2/kg above 5kg)
  - Zone-level TaxPercent and TaxMode (Inclusive/Exclusive) for automatic tax calculation
  - Cost tracking fields (CostFlatRate, CostPerKgRate) for profit margin analysis
- **Regulatory Compliance**: Empost Regulatory Compliance Module for UAE courier licensing.
- **API Integration**: Configuration and webhook endpoints for third-party booking websites.
- **Knowledge Base**: Integrated documentation using Markdig for "How To Guides".
- **Native GL Module**: Full GL functionality implemented natively with long-based IDs and Net4Courier naming conventions, including Financial Periods, Year-End Closing, Opening Balances, and automated GL table creation.
- **Cash & Bank Module**: Independent transaction management and bank account management.
- **Error Handling**: Robust global error handling with `ErrorBoundary`, `PageErrorHandler`, and `MudBlazor Snackbar`.
- **Demo Data & Setup**: Admin feature to create/delete demo data, initial setup dialog for new deployments, and a secure initial setup wizard at `/setup`.
- **Platform Administration**: Dedicated admin section with Tenant Settings, Subscription Management, and user management features.
- **Utility Features**: CLI password reset utility, AWB barcode generation, tracking/shipment invoice PDF generation via API, customer CRM with complaints/tickets, branch display settings, branch currency as default, and a "Delete All Business Data" feature for platform admins.
- **Favourites Dashboard**: User-personalized menu dashboard at `/favourites` allowing users to save frequently-used menu items. Users can add items via the Add button dialog and remove via X button on cards. Stored in UserFavorites table with soft-delete. Replaces the previous Application Launchpad.
- **Audit Log System**: Comprehensive audit logging automatically tracking all entity changes (Create, Update, Delete). Captures user ID, username, branch, timestamp, old/new values as JSON. Implemented via SaveChangesAsync override in ApplicationDbContext. Dashboard at `/audit-logs` with filters by date, entity, action, user. Includes Excel export via API endpoint. Access restricted to Administrator, PlatformAdmin, and Manager roles.
- **Customer Zones**: Geographic zone management for organizing customers and couriers for pickup notifications. Zones contain multiple cities, customers can be assigned to zones, and couriers can be assigned to zones. When pickup requests originate from a zone, all assigned couriers receive notifications in the PWA mobile app. Management page at `/customer-zones` with CRUD operations, customer/courier assignment dialogs. Access restricted to Administrator, PlatformAdmin, and Manager roles.

## External Dependencies
- **Database**: PostgreSQL
- **UI Framework**: MudBlazor
- **Reporting**: QuestPDF
- **Excel Handling**: ClosedXML
- **Email**: Google Gmail API via Replit connector

## Code Patterns

### MudBlazor Z-Index Configuration (CRITICAL for Dialogs)
MudBlazor's default Popover z-index (1200) is lower than Dialog z-index (1400), causing dropdowns and calendar popups to render behind dialogs. Fix this in the theme configuration:

```razor
<!-- In MainLayout.razor, EmptyLayout.razor, CustomerLayout.razor -->
<MudThemeProvider Theme="_theme" />

@code {
    private MudTheme _theme = new MudTheme
    {
        ZIndex = new ZIndex
        {
            Popover = 1500,    // Higher than Dialog
            Dialog = 1400,
            Snackbar = 1600,
            Tooltip = 1700
        }
    };
}
```

### MudSelect Dropdown Pattern (in Dialogs)
**CRITICAL: Always use `DisablePortal="true"` for MudSelect inside MudDialog** - Without this, dropdowns render to `<body>` and get hidden/clipped inside dialogs.

**Pattern 1: Simple dropdown with @bind-Value (inside dialog)**
```razor
<MudSelect T="long" @bind-Value="_selectedCurrencyId" Label="Currency" Variant="Variant.Outlined"
           DisablePortal="true">
    <MudSelectItem T="long" Value="0L">-- Select --</MudSelectItem>
    @foreach (var currency in _currencies)
    {
        <MudSelectItem T="long" Value="@currency.Id">@currency.Code - @currency.Name</MudSelectItem>
    }
</MudSelect>
```

**Pattern 2: Cascading dropdown with Value + ValueChanged (inside dialog)**
```razor
<!-- Country dropdown triggers city reload -->
<MudSelect T="long?" Value="_countryId" Label="Country" Variant="Variant.Outlined"
           Clearable="true" ValueChanged="OnCountryChanged" DisablePortal="true">
    @foreach (var country in _countries)
    {
        <MudSelectItem T="long?" Value="@((long?)country.Id)">@country.Name</MudSelectItem>
    }
</MudSelect>

<!-- City dropdown depends on country selection -->
<MudSelect T="long?" Value="_cityId" Label="City" Variant="Variant.Outlined"
           Clearable="true" Disabled="@(!_countryId.HasValue)" ValueChanged="OnCityChanged" DisablePortal="true">
    @foreach (var city in _cities)
    {
        <MudSelectItem T="long?" Value="@((long?)city.Id)">@city.Name</MudSelectItem>
    }
</MudSelect>

@code {
    private long? _countryId;
    private long? _cityId;
    private List<Country> _countries = new();
    private List<City> _cities = new();

    private async Task OnCountryChanged(long? countryId)
    {
        _countryId = countryId;  // MUST update backing field
        _cityId = null;
        _cities.Clear();
        
        if (countryId.HasValue)
        {
            _cities = await DbContext.Cities
                .Where(c => c.CountryId == countryId && c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.Name).ToListAsync();
        }
        StateHasChanged();
    }

    private async Task OnCityChanged(long? cityId)
    {
        _cityId = cityId;  // MUST update backing field
        // Load dependent data if needed
        StateHasChanged();
    }
}
```

### MudDatePicker Calendar Popup Pattern (in Dialogs)
Calendar popups in dialogs work correctly with the z-index fix above. Use standard pattern:

```razor
<MudDatePicker @bind-Date="_selectedDate" Label="Transaction Date" 
               Variant="Variant.Outlined" DateFormat="dd/MM/yyyy"
               Required="true" RequiredError="Date is required" />

@code {
    private DateTime? _selectedDate = DateTime.Today;
}
```

### Native HTML Select Pattern (for Dialogs with MudBlazor v7+ Issues)
When MudSelect dropdowns fail to display options inside MudDialog despite z-index and DisablePortal fixes, use native HTML selects styled with custom CSS:

**CSS Classes (in app.css)**
```css
.native-select-wrapper { position: relative; width: 100%; }
.native-select-wrapper label { display: block; font-size: 0.75rem; color: rgba(0,0,0,0.6); margin-bottom: 4px; }
.native-select-wrapper label.required::after { content: " *"; color: #f44336; }
.native-select { width: 100%; padding: 8px 12px; font-size: 0.875rem; border: 1px solid rgba(0,0,0,0.23); border-radius: 4px; background-color: transparent; }
.native-select.dense { padding: 6px 10px; font-size: 0.8125rem; }
```

**Simple dropdown:**
```razor
<div class="native-select-wrapper">
    <label class="required">Company</label>
    <select class="native-select dense" @bind="Party.CompanyId">
        @foreach (var company in _companies)
        {
            <option value="@company.Id">@company.Name</option>
        }
    </select>
</div>
```

**Cascading dropdown with event handler:**
```razor
<div class="native-select-wrapper">
    <label>Country</label>
    <select class="native-select dense" value="@_countryId" @onchange="OnCountryChangedNative">
        <option value="">-- Select --</option>
        @foreach (var country in _countries)
        {
            <option value="@country.Id">@country.Name</option>
        }
    </select>
</div>

@code {
    private async Task OnCountryChangedNative(ChangeEventArgs e)
    {
        long? countryId = null;
        if (long.TryParse(e.Value?.ToString(), out var id))
            countryId = id;
        await OnCountryChanged(countryId);
    }
}
```

### Key Troubleshooting Notes
1. **Dropdown not visible in dialog**: Check ZIndex configuration in theme, or use native HTML select as fallback
2. **Cascading dropdown not updating**: Ensure ValueChanged handler updates the backing field
3. **Calendar popup behind dialog**: Same z-index fix applies
4. **Value not binding**: Use correct generic type T (long vs long? vs string)
5. **MudSelect still broken after all fixes**: Use native HTML select with custom CSS styling