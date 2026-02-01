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
- **Prepaid AWB Management**: AWB Stock Management and Prepaid AWB Sales module with integrated accounting.
- **Enhanced Dashboards**: Unified dispatcher view and global autocomplete search across AWBs, Customers, and Invoices.
- **Unified Shipment Processing**: Single pages for Import Shipment Charges, Warehouse Inscan, and Shipment Lists for both domestic and import AWBs.
- **Master Data Management**: Configurable Rate Card Management, Service Type, Shipment Mode, Port Master, Currency, Geographic Master Data, and enhanced Customer Master with configurable account types and SLA management.
- **Regulatory Compliance**: Empost Regulatory Compliance Module for UAE courier licensing.
- **API Integration**: Configuration and webhook endpoints for third-party booking websites.
- **Knowledge Base**: Integrated documentation using Markdig for "How To Guides".
- **Native GL Module**: Full GL functionality implemented natively with long-based IDs and Net4Courier naming conventions, including Financial Periods, Year-End Closing, Opening Balances, and automated GL table creation.
- **Cash & Bank Module**: Independent transaction management and bank account management.
- **Error Handling**: Robust global error handling with `ErrorBoundary`, `PageErrorHandler`, and `MudBlazor Snackbar`.
- **Demo Data & Setup**: Admin feature to create/delete demo data, initial setup dialog for new deployments, and a secure initial setup wizard at `/setup`.
- **Platform Administration**: Dedicated admin section with Tenant Settings, Subscription Management, and user management features.
- **Utility Features**: CLI password reset utility, AWB barcode generation, tracking/shipment invoice PDF generation via API, customer CRM with complaints/tickets, branch display settings, branch currency as default, and a "Delete All Business Data" feature for platform admins.

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
Use Value + ValueChanged pattern for cascading dropdowns, or @bind-Value for simple dropdowns:

**Pattern 1: Simple dropdown with @bind-Value**
```razor
<MudSelect T="long" @bind-Value="_selectedCurrencyId" Label="Currency" Variant="Variant.Outlined">
    <MudSelectItem T="long" Value="0L">-- Select --</MudSelectItem>
    @foreach (var currency in _currencies)
    {
        <MudSelectItem T="long" Value="@currency.Id">@currency.Code - @currency.Name</MudSelectItem>
    }
</MudSelect>
```

**Pattern 2: Cascading dropdown with Value + ValueChanged**
```razor
<!-- Country dropdown triggers city reload -->
<MudSelect T="long?" Value="_countryId" Label="Country" Variant="Variant.Outlined"
           Clearable="true" ValueChanged="OnCountryChanged">
    @foreach (var country in _countries)
    {
        <MudSelectItem T="long?" Value="@((long?)country.Id)">@country.Name</MudSelectItem>
    }
</MudSelect>

<!-- City dropdown depends on country selection -->
<MudSelect T="long?" Value="_cityId" Label="City" Variant="Variant.Outlined"
           Clearable="true" Disabled="@(!_countryId.HasValue)" ValueChanged="OnCityChanged">
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

### Key Troubleshooting Notes
1. **Dropdown not visible in dialog**: Check ZIndex configuration in theme
2. **Cascading dropdown not updating**: Ensure ValueChanged handler updates the backing field
3. **Calendar popup behind dialog**: Same z-index fix applies
4. **Value not binding**: Use correct generic type T (long vs long? vs string)