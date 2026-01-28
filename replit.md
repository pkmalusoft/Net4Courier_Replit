# Net4Courier - Blazor Server Migration

## Overview
Net4Courier is a comprehensive logistics management system designed to streamline courier operations, including shipment management (AWB), customer relations, branch operations, financial transactions, and reporting. Its primary goal is to replace legacy systems with a modern, modular, and scalable platform that enhances operational efficiency and provides real-time insights for logistics companies.

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
- **Database Partitioning**: PostgreSQL table partitioning implemented for `InscanMasters` based on `TransactionDate`.

### UI/UX Decisions
- **MudBlazor Components**: Ensures a consistent and responsive user experience.
- **Layout**: Standard dashboard with `MainLayout`, `NavMenu`, and organized navigation.
- **Responsive Design**: Key modules are optimized for mobile access.
- **Login Page**: Modern split-screen design featuring a courier illustration and a compact login card.

### Technical Implementations
- **Entity Management**: Comprehensive CRUD operations for core entities such as Company, Branch, User, Role, Financial Year, Parties, AWB, Invoices, Receipts.
- **Financial Features**: Includes invoice/receipt management, journaling, self-referential account heads, financial period management, dynamic other charges, Account Receivables, Account Payables, Cash and Bank Vouchers, and Bank Account Management.
- **Financial Reports**: 23 comprehensive reports including Trial Balance (Standard, Grouped, Monthly, Detailed), Profit & Loss (Item Wise, Period Comparison), Customer Reports (Ledger, Aging, Statement), Chart of Accounts, Cash Flow Statement (Indirect and Direct Methods), Balance Sheet (Standard, GroupWise with collapsible hierarchy, Horizontal side-by-side, Vertical stacked), and Account Ledger (Standard, Detailed with running balance, Summary with opening/closing).
- **Operations Workflow**: Covers AWB entry and generation, pickup management (request to inscan), outscan/DRS management, AWB tracking, Proof of Delivery (POD), Return to Shipper (RTS), Master Airwaybill (MAWB) processing, COD Remittance, Pickup Commitment, Pickup Incentive, and Transfer Order management.
- **Prepaid AWB Management**: AWB Stock Management for tracking physical AWB inventory (books, stickers, rolls) with quantity, rate, and AWB number ranges. Prepaid AWB Sales module for selling prepaid AWBs to customers with automatic allocation from stock, payment mode selection (Cash/Bank/Cheque), and integrated accounting (Dr Cash/Bank, Cr Prepaid Control at sale; Dr Prepaid Control, Cr Revenue at usage).
- **Enhanced Pickup Dashboard**: Unified dispatcher view with commitment status integration, showing committed pickups, expiring soon alerts, available pickups, courier performance metrics, countdown timers, and quick actions for commitment confirmation/override with 30-second auto-refresh.
- **Import Shipment Charges**: Combined Duty/VAT Amount field, COD/Collection Amount field, and Shipper Name column in import shipment dialog, ImportEntry, and ImportInscan data grids. Excel import/export supports these columns with automatic mapping (DutyVatAmount → DutyAmount, CodCollectionAmount → CODAmount, ShipperName → ShipperName) and IsCOD flag auto-set when COD amount is entered. Import Bag dialog uses DialogService pattern for improved reliability.
- **Master Data Management**: Includes configurable Rate Card Management with Excel import capability, Service Type Management, Shipment Mode Management, Port Master, Currency Management, and extensive Geographic Master Data.
- **Zone Management**: Zones now have a Type field (International/Domestic). International zones allow selecting multiple countries via chips interface. Domestic zones allow selecting multiple cities. Zone details are stored in ZoneMatrixDetails with atomic transaction saves.
- **Rate Card Excel Import**: Import rate cards from client Excel templates with dynamic header detection, zone category/zone/country parsing, weight-based slab rules extraction, preview with validation warnings, and batch processing.
- **Enhanced Customer Master**: Configurable Account Types (Cash, Credit, Prepaid), Customer Branches/Departments management with contact details, Client Address tracking, tabbed Party dialog interface (General Info + Branches tabs), and improved visual layout with logo display and Account Type column.
- **SLA Management**: Customer-specific Service Level Agreements with transit rules by zone/country/service type, credit terms, liability limits, volumetric settings, and status workflow (Draft→PendingApproval→Active→Expired/Terminated/Suspended) with approval tracking.
- **Logo Management**: Company and Party logo upload with 2MB file limit, PNG/JPG/SVG/GIF validation, preview, and storage in /wwwroot/uploads/logos/.
- **Regulatory Compliance**: Empost Regulatory Compliance Module for UAE courier licensing, advance payments, quarterly submissions, and royalty calculations.
- **API Integration**: Provides configuration and webhook endpoints for third-party booking websites.
- **Knowledge Base**: Integrated documentation using Markdig for "How To Guides" and operational flows, including Prepaid AWB guides, General Ledger documentation, Email Reports guide, and Import module field documentation.
- **Navigation Menu**: Restructured into 9 collapsible main sections with bold headers: Dashboards, Shipments & Operations, Import/Export, Customers & CRM, Pricing & Billing, Finance & Accounting (with GL Masters/Transactions/Reports, AR, AP, Courier Reconciliation), Compliance & Audit (with Empost Regulatory), Masters & Settings (with Organization, Operations Masters, User & Security, Geography, Customer Settings), Knowledge & Tools.
- **General Ledger Module**: Full GL implementation with Chart of Accounts, Control Accounts, Financial Years, Tax Setup, Cash & Bank Vouchers, Bank Accounts, Bank Reconciliation, Journal Vouchers, and comprehensive Financial Reports (Account Ledger, Day Book, Cash & Bank Book, Trial Balance, Profit & Loss, Balance Sheet, Cash Flow).
- **Error Handling**: Robust global error handling with `ErrorBoundary`, `PageErrorHandler`, and `MudBlazor Snackbar` integration.
- **Demo Data Management**: Admin feature to create and delete demo data for training purposes. Creates 5 demo customers (DEMO-CUST-001 to 005) with UAE addresses and 5 complete AWB workflows (DEMO-AWB-001 to 005) including pickup requests, inscans, tracking entries, and delivery completion. All demo records are flagged with IsDemo=true for easy identification and safe deletion. Located under Masters & Settings > User & Security menu.
- **Initial Setup Wizard**: Platform administrators can configure new client deployments through a secure setup page (/setup). When no admin user exists in the database, users are automatically redirected to the setup page. The setup requires a SETUP_KEY environment variable for authentication before allowing administrator account creation. This replaces hardcoded admin creation for secure multi-client deployments.
- **Barcode Generation**: AWB barcodes generated using ZXing.Net.Bindings.ImageSharp with SixLabors.ImageSharp for horizontal (300x80) and vertical (40x250, rotated 90°) PNG barcodes embedded in AWB labels.

## Entity Property Reference
Key entity property names used in reports:
- **Branch**: `Id`, `Name`, `Code` (not BranchId/BranchName)
- **Party**: `Id`, `Name`, `Code` (not PartyId/PartyCode)
- **AccountHead**: `Code`, `Name`, `Classification` (enum), `ParentId`, `Parent`, `AccountGroup`, `AccountNature`
- **JournalEntry**: `Debit`, `Credit` (nullable decimals), `JournalId`, `AccountHeadId`, `PartyId`
- **Journal**: `VoucherDate`, `VoucherNo`, `BranchId` (date/branch info on parent Journal, not JournalEntry)

## Email Integration
- **Gmail API Integration**: Uses Replit Google Workspace connector for OAuth-based email sending via Gmail API
- **GmailEmailService**: Service class implementing IGmailEmailService for sending emails with PDF attachments
- **Report Email Feature**: Customer Ledger, Customer Statement, Supplier Ledger, and Supplier Statement reports have Email buttons to send PDF reports directly to customers/suppliers
- **Email Pattern**: Branch email used as sender, Party email used as recipient; validates email addresses before sending
- **Attachment Support**: Reports are generated as PDF attachments with professional HTML email body containing summary information

## External Dependencies
- **Database**: PostgreSQL
- **UI Framework**: MudBlazor
- **Reporting**: QuestPDF
- **Excel Handling**: ClosedXML
- **Email**: Google Gmail API via Replit connector