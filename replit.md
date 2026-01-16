# Net4Courier - Blazor Server Migration

## Overview
Net4Courier is a comprehensive courier/logistics management system migrated from ASP.NET MVC 5 (.NET Framework 4.7.2) to .NET 8 Blazor Server. The application manages courier operations including shipments (AWB), customers, branches, financial transactions, and reporting.

## Current State
- **Framework**: .NET 8 Blazor Server with MudBlazor UI components
- **Database**: PostgreSQL (Replit hosted) with 22 tables
- **Status**: Modular architecture complete, database schema created

## Project Structure (Modular)
```
src/
├── Net4Courier.Web/              # Blazor Server frontend (Port 5000)
│   ├── Components/
│   │   ├── Layout/               # MainLayout, NavMenu
│   │   └── Pages/                # Dashboard, Companies, Branches, etc.
│   ├── Services/                 # AuthService, MenuService
│   └── Program.cs                # Application entry point
├── Net4Courier.Infrastructure/   # DbContext, Services, Migrations
│   ├── Data/ApplicationDbContext.cs
│   └── Services/AuthService.cs
├── Net4Courier.Kernel/           # Base entities, enums
│   └── Entities/BaseEntity.cs
├── Net4Courier.Masters/          # Master data entities
│   └── Entities/                 # Company, Branch, User, Role, Party
├── Net4Courier.Operations/       # Operations entities
│   └── Entities/                 # InscanMaster, AWBTracking, DRS, Manifest
├── Net4Courier.Finance/          # Finance entities
│   └── Entities/                 # Invoice, Receipt, Journal, AccountHead
└── Net4Courier.Shared/           # Legacy (being phased out)
```

## Module Dependencies
- Web → Infrastructure → Kernel + Masters + Operations + Finance
- Each module can be built independently to reduce memory usage

## Key Entity Groups

### Masters Module
- **Company**: Multi-tenant company management
- **Branch**: Company branches with fiscal year support  
- **User**: System users with role-based access
- **UserType**: User classification (Employee, Agent, Customer, Vendor)
- **Role/RolePermission**: User roles and permissions
- **Party/PartyAddress**: Party management with clear accounting classifications
- **FinancialYear**: Fiscal year definitions
- **FinancialPeriod**: Monthly accounting periods (auto-generated, open/close control)

## Party Types and Accounting Classification
Parties are classified by PartyType with corresponding AccountNature for proper financial tracking:

| Party Type | Account Nature | Description |
|------------|---------------|-------------|
| **Customer** | Receivable | Pays company for courier services |
| **DeliveryAgent** | Receivable | Delivers shipments on company's behalf |
| **CoLoader** | Receivable | Provides shipment loads to company for delivery |
| **Supplier** | Payable | General suppliers |
| **ForwardingAgent** | Payable | External carriers (DHL, FedEx, Aramex) that company pays to forward shipments |

Seed data includes: DHL Express, FedEx, Aramex, UPS, TNT (all as Forwarding Agents with Account Payable)

Separate management pages exist for each party type:
- `/parties` - Customers only
- `/delivery-agents` - Delivery Agents
- `/co-loaders` - Co-Loaders  
- `/forwarding-agents` - Forwarding Agents (DHL, FedEx, etc.)

### Operations Module
- **InscanMaster**: Main shipment table (139 fields, legacy compatible)
- **InscanMasterItem**: Package items within shipment
- **AWBTracking**: Shipment status history events
- **DRS/DRSDetails**: Delivery Run Sheet for last-mile
- **Manifest**: Grouping shipments for dispatch
- **QuickInscanMaster**: Bulk inscan sheet

### Finance Module
- **Invoice/InvoiceDetail**: Customer billing
- **Receipt/ReceiptAllocation**: Payment collection
- **Journal/JournalEntry**: Accounting entries
- **AccountHead**: Chart of accounts (self-referential)

## Database Configuration
- Connection: `DATABASE_URL` environment variable
- PostgreSQL with EF Core
- Tables auto-created on startup via EnsureCreatedAsync

## Authentication
- Admin user seeded: `admin` / `Admin@123`
- Password hashing using BCrypt

## Running the Application
```bash
cd src/Net4Courier.Web && dotnet run --urls http://0.0.0.0:5000
```

## Migration Progress
1. [x] Modular solution structure (6 modules)
2. [x] EF Core DbContext with all entities (22 tables)
3. [x] MudBlazor dashboard layout
4. [x] Company management CRUD
5. [x] Branch management CRUD
6. [x] User management UI (with role assignment, branch selection, BCrypt password hashing)
7. [x] Role management UI (with granular permissions matrix for 14 modules)
8. [x] Financial Year management (with company association, current year enforcement)
9. [x] Party/Customer management (normalized addresses, multiple address support)
10. [x] AWB Entry form (full shipment details, auto-generated AWB numbers)
11. [x] AWB List (with advanced filtering by date, status, search)
12. [x] AWB Tracking (timeline history with status updates)
13. [x] Authentication/Login UI (custom AuthenticationStateProvider, redirect to login)
14. [x] PostgreSQL table partitioning (PartitioningService created, SQL templates ready)
15. [x] Finance modules - Invoices and Receipts CRUD (with customer autocomplete, AWB billing, receipt allocation)
16. [x] Reporting with QuestPDF (AWB labels, Invoice PDFs, Receipt PDFs)
17. [x] UserType management (Employee, Agent, Customer, Vendor classification)
18. [x] Geography Masters - Country, State, City, Location with hierarchical relationships
19. [x] Financial Period Management - Auto-generated monthly periods with open/close control
20. [x] Dynamic Other Charges - Configurable charge types with dialog selection in AWB entry
21. [x] Pickup Management - Complete workflow from request to INSCAN with responsive mobile pages

## Pickup Management Workflow
The pickup management system handles the complete flow from customer request to hub receipt:

### Status Flow
1. **Pickup Request** - Customer/Staff creates a pickup request
2. **Assigned for Collection** - Dispatcher assigns a courier (Delivery Agent)
3. **Shipment Collected** - Courier picks up the shipment from customer location
4. **Inscanned** - Storekeeper scans and receives the shipment at the hub

### Pages
- `/pickup-management` - Main dashboard with status filters, summary cards, and quick actions
- `/pickup-request-new` - Create new pickup request (staff view)
- `/pickup-inscan` - INSCAN page with barcode scanner input for storekeeper
- `/courier-pickups` - Mobile-friendly courier view with assigned/collected tabs
- `/customer/pickup-request` - Customer portal to request pickups
- `/customer/my-pickups` - Customer portal to track their pickup requests

### Entity: PickupRequest
Fields: PickupNo, RequestDate, ScheduledDate, CustomerId, CourierId, customer/contact details, pickup address, EstimatedPieces/Weight, ActualPieces/Weight, Status, timestamps (AssignedAt, CollectedAt, InscannedAt), CollectionRemarks

### Key Features
- Auto-generated Pickup Numbers (PU + date + sequence)
- Customer address autocomplete from saved addresses
- Courier assignment dialog with delivery agent list
- Collection confirmation dialog with actual pieces and remarks
- Barcode scanner input for INSCAN (works with USB/Bluetooth scanners)
- Real-time status count cards (pending/assigned/collected/inscanned)
- Responsive design for mobile devices

## Dynamic Other Charges System
- **OtherChargeType**: Master table for charge types (Handling, Insurance, Packaging, etc.)
- **AWBOtherCharge**: Junction table linking charges to AWBs with amounts
- AWB entry has an "Other Charges" field with edit button that opens a dialog
- Dialog shows all active charge types with checkboxes and amount fields
- Hover tooltip shows breakdown of selected charges with individual amounts
- Charges saved in a database transaction with AWB to ensure consistency

## Financial Period System
- When a Financial Year is created, 12 monthly periods are auto-generated
- Periods can be individually closed/reopened by admin via Financial Periods page
- AWB entry validates that the transaction date falls in an open period
- Closed periods prevent any new transactions from being posted
- Financial Year close also blocks all transactions in that year

## PostgreSQL Partitioning Notes
- InscanMasters table supports partitioning by TransactionDate
- PartitioningService.cs contains SQL templates for converting to partitioned tables
- For new installations: Run partition migration SQL before adding data
- For existing data: Backup first, then run migration with data copy
- Partitioning improves query performance for date-range queries by 10-100x

## User Preferences
- MudBlazor for all UI components
- Modular architecture like TrueBookserp pattern
- PostgreSQL partitioning by TransactionDate for performance
- Party/PartyAddress normalization for storage efficiency

## Automatic Movement Type Calculation
Movement type is now calculated automatically based on origin/destination countries vs company country:
- **Domestic**: Origin = Company Country AND Destination = Company Country
- **International-Export**: Origin = Company Country AND Destination ≠ Company Country
- **International-Import**: Origin ≠ Company Country AND Destination = Company Country
- **Transhipment**: Origin ≠ Company Country AND Destination ≠ Company Country

The Movement Type dropdown has been removed from AWBNew.razor and replaced with a color-coded chip that updates automatically when origin/destination countries change.

## Recent Changes (Jan 16, 2026)
- **Pickup Management Workflow**: Complete pickup-to-INSCAN workflow with 6 new pages
- Added PickupRequest entity with PickupStatus enum (PickupRequest, AssignedForCollection, ShipmentCollected, Inscanned, Cancelled)
- Created responsive Courier Pickups page for mobile field operations
- Created INSCAN page with barcode scanner input support
- Added Customer Portal layout with simplified navigation
- Reorganized navigation menu into 16 major groups following ERP best practices
- Party Type Classification: Added AccountNature (Receivable/Payable) to parties
- Separate management pages for Customers, Delivery Agents, Co-Loaders, Forwarding Agents
- Seeded forwarding agents: DHL Express, FedEx, Aramex, UPS, TNT
- Added period validation to AWB entry form to prevent transactions in closed periods

## Changes (Jan 15, 2026)
- Implemented modular architecture with 6 separate modules
- Created complete InscanMaster entity (139 fields, legacy compatible)
- Added separate AWBTracking table for status history
- Created Party/PartyAddress normalized structure
- Full Finance module with Invoice, Receipt, Journal entities
- Successfully created 22 database tables
- Fixed MudBlazor static assets for production deployment (local files in wwwroot/lib/mudblazor/)

## MudBlazor Deployment Note
MudBlazor static assets are stored locally in `wwwroot/lib/mudblazor/` for Replit compatibility. Do not use `_content/MudBlazor/` paths as they don't work in Replit's production environment.

## MudBlazor Popover/Dropdown Fix (Critical for Replit)
**Problem**: MudSelect/MudAutocomplete dropdowns may be invisible or appear in wrong positions in Replit's iframe environment.

**Solution**: Place `<MudPopoverProvider />` as the **first child inside `<MudLayout>`** in MainLayout.razor:
```razor
<MudLayout>
    <MudPopoverProvider />
    <MudAppBar>...</MudAppBar>
    <MudDrawer>...</MudDrawer>
    <MudMainContent>...</MudMainContent>
</MudLayout>
```

**DO NOT**:
- Place MudPopoverProvider outside MudLayout
- Add CSS overrides for .mud-popover-provider position
- Use PopoverClass="mud-popover-fixed" on individual components

**MudSelect Pattern** (for cascading dropdowns):
```razor
<MudSelect T="long?" Value="_selectedCountryId" ValueChanged="OnCountryChanged" 
           Variant="Variant.Outlined" Clearable="true">
    @foreach (var country in _countries)
    {
        <MudSelectItem T="long?" Value="@((long?)country.Id)">@country.Name</MudSelectItem>
    }
</MudSelect>
```
- Use `Value`/`ValueChanged` for parent dropdowns (enables cascading)
- Use `@bind-Value` for final dropdown in cascade
- Always cast to nullable: `Value="@((long?)entity.Id)"`
