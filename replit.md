# Net4Courier - Blazor Server Migration

## Overview
Net4Courier is a comprehensive courier/logistics management system designed to manage all facets of courier operations, including shipments (AWB), customer relationship management, branch operations, financial transactions, and extensive reporting capabilities. The project's vision is to provide a robust, scalable, and modern platform for logistics companies, enhancing operational efficiency and providing real-time insights, replacing outdated legacy systems with a modular, maintainable, and high-performance solution.

## User Preferences
- MudBlazor for all UI components
- Modular architecture like TrueBookserp pattern
- PostgreSQL partitioning by TransactionDate for performance
- Party/PartyAddress normalization for storage efficiency

## System Architecture
The application is built on .NET 8 Blazor Server, utilizing a modular architecture to ensure maintainability, scalability, and independent development of features.

### Core Architectural Patterns
- **Modular Design**: Structured into independent modules (`Net4Courier.Web`, `Net4Courier.Infrastructure`, `Net4Courier.Kernel`, `Net4Courier.Masters`, `Net4Courier.Operations`, `Net4Courier.Finance`) for separation of concerns.
- **Data Persistence**: Entity Framework Core with PostgreSQL.
- **UI Framework**: MudBlazor for all user interface elements.
- **Authentication**: Custom authentication with BCrypt for password hashing and `AuthenticationStateProvider`.
- **Database Partitioning**: PostgreSQL table partitioning for `InscanMasters` based on `TransactionDate`.

### UI/UX Decisions
- **MudBlazor Components**: Consistent and responsive design using MudBlazor.
- **Layout**: Standard dashboard with `MainLayout`, `NavMenu`, and organized navigation into 16 major groups.
- **Responsive Design**: Key modules are optimized for mobile devices.
- **Modern Login Page**: Split-screen design with courier illustration and compact login card.

### Technical Implementations
- **Entity Management**: Comprehensive CRUD for Company, Branch, User, Role, Financial Year, Parties, AWB, Invoices, Receipts.
- **Party Classification**: Parties categorized by `PartyType` with corresponding `AccountNature` for financial tracking.
- **Operations Workflow**:
    - **AWB Entry & Generation**: Full shipment details with auto-generated, branch-based AWB numbers. Includes conversion from pickup requests.
    - **Pickup to AWB Conversion**: Two-step process: INSCAN (warehouse receiving) then AWB Conversion.
    - **Pickup Management**: End-to-end workflow from customer request to collection and inscan.
    - **Outscan/DRS Management**: Dispatching shipments to couriers via Delivery Run Sheets with barcode scanning.
    - **AWB Tracking**: Detailed history of shipment status updates.
- **Financial Features**:
    - **Invoice/Receipt Management**: Billing and payment collection.
    - **Journaling**: Accounting entries.
    - **Account Head**: Self-referential chart of accounts.
    - **Financial Period Management**: Auto-generated monthly periods with admin control for opening/closing.
    - **Dynamic Other Charges**: Configurable charge types applied during AWB entry.
- **Reporting**: QuestPDF integration for AWB labels, Invoice, and Receipt PDFs.
- **Automatic Movement Type Calculation**: Determines shipment type (Domestic, International) based on origin/destination.
- **Rate Card Management**:
    - **Zone Matrix**: Define zones for geographic rate categorization.
    - **Rate Cards**: Configurable pricing with movement type, payment mode, validity dates.
    - **Slab-Based Pricing**: Rule-based weight slabs.
    - **Slab Rule Templates**: Reusable template library.
    - **Customer Rate Assignments**: Priority-based assignment with effective dating.
    - **Rating Engine Service**: Automated rate calculation with formula trace display.
    - **Rate Simulator**: Test rate calculations with full tracing.
- **Proof of Delivery (POD)**:
    - **Mobile POD Capture**: Touch-friendly interface for delivery agents with photo evidence, digital signature, GPS location, and delivery status options.
    - **Offline Support**: IndexedDB storage for offline capture with sync.
- **Shipment Status Timeline**:
    - **Database-Driven Status Management**: Flexible status groups and statuses configurable via admin UI.
    - **Timeline History**: Chronological record of status changes.
    - **Automatic Status Updates**: Integrated into AWB entry, outscan, POD capture, RTS, and invoicing.
    - **Public Tracking Page**: Customer-facing `/tracking` page for AWB search and timeline history.
- **MAWB Processing (Master Airwaybill)**:
    - **MasterAirwaybill Entity**: Stores MAWB header info.
    - **MAWBBag Entity**: Individual bags within a MAWB.
    - **Shipment-to-Bag Linking**: Tracks shipments within bags.
    - **MAWBService**: Validation logic for route matching, hold checks, and duplicate prevention.
    - **MAWBList/Entry/Bagging Pages**: UI for managing MAWBs and bagging shipments via barcode scanning.
    - **Finalization Workflow**: Validated finalization of MAWBs.
    - **MAWB Manifest PDF**: QuestPDF report with header, bag summary, and shipment details.
- **Printing & Reports**: AWB Print, Manifest Labels, Export/Domestic Manifests with integration from AWB Entry and MAWB Bagging pages.
- **Service Type Management**: Configurable service types via CRUD UI.
- **Return to Shipper (RTS)**: Workflow for return shipments with address swapping, charge modes, and status tracking.
- **Modern Login Page**: Split-screen design with courier illustration on left and compact vertical login card on right. Features Net4Courier logo, username/password fields, Forgot Password link, and responsive mobile layout.
- **Forgot Password Page**: Matching split-screen design at `/forgot-password` for password reset requests.
- **Knowledge Base**: Comprehensive documentation at `/knowledge-base` covering:
    - Complete operational flow (Pickup Request → Collection → Inscan → AWB → MAWB → DRS → POD → Delivery)
    - Reconciliation (DRS Reconciliation, Courier Day-End, Cash Receipt, Expense Approval, Courier Ledger)
    - Accounts & Finance (General Ledger, Chart of Accounts, Financial Periods, AR Invoicing/Receipts, AP Bills/Payments, Financial Statements)
    - Customer Management CRM (Customer Profiles, Contracts & Pricing, SLAs, Complaints)
    - Pricing & Billing (Zone Management, Rate Cards, Rate Simulator, Special/Fuel/Other Charges, Discounts)
    - System Settings (Companies, Branches, Service Types, Status Management, Users, Geography Masters)
    - Compliance & Audit (Audit Logs, Regulatory Reports, Data Export, Documents)
    - Complete 31 Status Codes reference table
    - Searchable keywords throughout for easy text search (Ctrl+F)
    - Uses Markdig for Markdown rendering with XSS protection (DisableHtml)

## External Dependencies
- **Database**: PostgreSQL
- **UI Framework**: MudBlazor
- **Reporting**: QuestPDF