# Net4Courier - Blazor Server Migration

## Overview
Net4Courier is a comprehensive courier/logistics management system migrated to .NET 8 Blazor Server. The system is designed to manage all facets of courier operations, including shipments (AWB), customer relationship management, branch operations, financial transactions, and extensive reporting capabilities.

The project's vision is to provide a robust, scalable, and modern platform for logistics companies, enhancing operational efficiency and providing real-time insights. It aims to replace outdated legacy systems with a modular, maintainable, and high-performance solution, leveraging modern web technologies and architectural best practices.

## User Preferences
- MudBlazor for all UI components
- Modular architecture like TrueBookserp pattern
- PostgreSQL partitioning by TransactionDate for performance
- Party/PartyAddress normalization for storage efficiency

## System Architecture
The application is built on .NET 8 Blazor Server, utilizing a modular architecture to ensure maintainability, scalability, and independent development of features.

### Core Architectural Patterns
- **Modular Design**: The solution is structured into several independent modules (`Net4Courier.Web`, `Net4Courier.Infrastructure`, `Net4Courier.Kernel`, `Net4Courier.Masters`, `Net4Courier.Operations`, `Net4Courier.Finance`), promoting separation of concerns and reducing inter-module dependencies.
- **Data Persistence**: Entity Framework Core is used for data access, interacting with a PostgreSQL database.
- **UI Framework**: MudBlazor is the chosen component library for all user interface elements, ensuring a consistent and modern look and feel.
- **Authentication**: Custom authentication using BCrypt for password hashing and an `AuthenticationStateProvider`.
- **Database Partitioning**: PostgreSQL table partitioning is implemented for the `InscanMasters` table based on `TransactionDate` to optimize query performance for large datasets.

### UI/UX Decisions
- **MudBlazor Components**: All UI elements leverage MudBlazor, providing a consistent and responsive design.
- **Layout**: A standard dashboard layout is employed, featuring a `MainLayout`, `NavMenu`, and organized navigation into 16 major groups following ERP best practices.
- **Responsive Design**: Key modules like pickup management include responsive pages optimized for mobile devices.

### Technical Implementations
- **Entity Management**: Comprehensive CRUD operations for core entities like Company, Branch, User, Role, Financial Year, Parties, AWB, Invoices, and Receipts.
- **Party Classification**: Parties are categorized by `PartyType` (Customer, DeliveryAgent, CoLoader, Supplier, ForwardingAgent) with corresponding `AccountNature` (Receivable/Payable) for accurate financial tracking.
- **Operations Workflow**:
    - **AWB Entry**: Full shipment details with auto-generated AWB numbers.
    - **AWB Tracking**: Detailed history of shipment status updates.
    - **Pickup Management**: End-to-end workflow from customer request to shipment collection and inscan, including multi-shipment requests and conversion to AWB records.
    - **Outscan/DRS Management**: Workflow for dispatching shipments to couriers via Delivery Run Sheets (DRS), including barcode scanning and status updates.
- **Financial Features**:
    - **Invoice/Receipt Management**: Comprehensive billing and payment collection with customer autocomplete and receipt allocation.
    - **Journaling**: Accounting entries management.
    - **Account Head**: Self-referential chart of accounts.
    - **Financial Period Management**: Auto-generated monthly periods with admin control for opening/closing, enforcing transaction dates within open periods.
    - **Dynamic Other Charges**: Configurable charge types applied during AWB entry with detailed breakdown.
- **Reporting**: Integration with QuestPDF for generating AWB labels, Invoice PDFs, and Receipt PDFs.
- **Automatic Movement Type Calculation**: Determines shipment type (Domestic, International-Export, International-Import, Transhipment) based on origin/destination countries relative to the company's country.
- **Rate Card Management**:
    - **Zone Matrix**: Define zones with country/city/postal code mappings for geographic rate categorization.
    - **Rate Cards**: Configurable pricing cards with movement type, payment mode, validity dates, and status tracking.
    - **Slab-Based Pricing**: Rule-based weight slabs that reduce storage from 30 rows to 2-3 rules per zone.
    - **Slab Rule Templates**: Reusable template library for saving/loading common slab configurations across rate cards.
    - **Customer Rate Assignments**: Priority-based rate card assignments with effective date versioning and drag-reorder support.
    - **Rating Engine Service**: Automated rate calculation with zone resolution (city > country > default), chargeable weight (actual vs volumetric), and slab calculations (PerStep/PerKg/FlatAfter modes).
    - **Formula Trace Display**: Detailed step-by-step calculation breakdown showing rate card source, zone resolution path, weight calculations, slab charges, and adjustments.
    - **Rate Simulator**: Test rate calculations with full formula tracing before applying to live shipments.
    - **Clone Rate Card**: Duplicate existing rate cards with all zones and slab rules as Draft status.
    - **Bulk Zone Operations**: Multi-select zones to apply common settings (base weight/rate, charges, margins, tax mode).
    - **Approval Workflow**: Status transitions (Draft → Pending Approval → Active → Expired/Suspended) with color-coded indicators.

## External Dependencies
- **Database**: PostgreSQL (hosted on Replit)
- **UI Framework**: MudBlazor (static assets stored locally in `wwwroot/lib/mudblazor/` for Replit compatibility)
- **Reporting**: QuestPDF (for generating various PDF documents)