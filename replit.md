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
- **Party/PartyAddress**: Customers, agents, vendors (normalized)
- **FinancialYear**: Fiscal year definitions

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

## Recent Changes (Jan 15, 2026)
- Implemented modular architecture with 6 separate modules
- Created complete InscanMaster entity (139 fields, legacy compatible)
- Added separate AWBTracking table for status history
- Created Party/PartyAddress normalized structure
- Full Finance module with Invoice, Receipt, Journal entities
- Successfully created 22 database tables
- Fixed MudBlazor static assets for production deployment (local files in wwwroot/lib/mudblazor/)

## MudBlazor Deployment Note
MudBlazor static assets are stored locally in `wwwroot/lib/mudblazor/` for Replit compatibility. Do not use `_content/MudBlazor/` paths as they don't work in Replit's production environment.
