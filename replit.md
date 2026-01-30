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
- **Enhanced Pickup Dashboard**: Unified dispatcher view with commitment status integration, performance metrics, and quick actions.
- **Import Shipment Charges**: Combined Duty/VAT and COD/Collection amount fields, with Excel import/export support.
- **Unified Warehouse Inscan**: Pickup Inscan page handles both domestic AWBs and import shipments, updating status accordingly and providing a combined view.
- **Unified Shipment List**: AWBList page displays both domestic/export AWBs and import shipments in a single view with filtering and search capabilities.
- **Branch AWB Configuration per Movement Type**: Separate AWB number series configuration for each movement type (Domestic, Export, Import, Transhipment) per branch.
- **Master Data Management**: Configurable Rate Card Management (with Excel import), Service Type, Shipment Mode, Port Master, Currency, and extensive Geographic Master Data.
- **Zone Management**: Zones have a Type field (International/Domestic), allowing selection of multiple countries/cities.
- **Enhanced Customer Master**: Configurable Account Types (Cash, Credit, Prepaid), Customer Branches/Departments, Client Address tracking, and a tabbed Party dialog interface.
- **SLA Management**: Customer-specific Service Level Agreements with transit rules, credit terms, liability limits, volumetric settings, and status workflow.
- **Logo Management**: Company and Party logo upload with validation and storage.
- **Regulatory Compliance**: Empost Regulatory Compliance Module for UAE courier licensing.
- **API Integration**: Configuration and webhook endpoints for third-party booking websites.
- **Knowledge Base**: Integrated documentation using Markdig for "How To Guides".
- **Navigation Menu**: Restructured into 9 collapsible main sections.
- **General Ledger Module**: Full GL implementation with Chart of Accounts, Control Accounts, Financial Years, Tax Setup, Vouchers, Bank Reconciliation, Journal Vouchers, and comprehensive Financial Reports.
- **TrueBooks GL Integration**: Migrated GL module to TrueBooks ERP platform, adapted for single-tenant operation with UUIDs.
- **TrueBooks GL Setup Pages**: Integrated GL Setup pages from TrueBooks platform including Chart of Accounts, Currencies, Voucher Numbering, and Control Accounts settings. Pages use fixed tenant ID '11111111-1111-1111-1111-111111111111' via SingleTenantContext. Routes use `/gl/` prefix (e.g., `/gl/chart-of-accounts`, `/gl/currencies`, `/gl/voucher-numbering`, `/gl/control-accounts`).
- **Error Handling**: Robust global error handling with `ErrorBoundary`, `PageErrorHandler`, and `MudBlazor Snackbar` integration.
- **Demo Data Management**: Admin feature to create and delete demo data for training.
- **Initial Setup Wizard**: Secure setup page for platform administrators to configure new client deployments.
- **Barcode Generation**: AWB barcodes generated using ZXing.Net.Bindings.ImageSharp for PNG images.
- **Global Search**: Dashboard features a unified autocomplete search box across AWBs, Customers, and Invoices.
- **Tracking Print Feature**: Tracking page generates professional A4 PDF reports via API.
- **Shipment Invoice**: AWB Entry page generates A4 commercial/customs invoices via API.
- **Customer CRM - Complaints & Tickets**: Full ticket management system with status workflow, priority levels, and customer/AWB linking.
- **Branch Display Settings**: Branch-level setting to hide account codes in financial views.

### Entity Property Reference
Key entity property names used in reports:
- **Branch**: `Id`, `Name`, `Code`
- **Party**: `Id`, `Name`, `Code`
- **AccountHead**: `Code`, `Name`, `Classification`, `ParentId`, `Parent`, `AccountGroup`, `AccountNature`
- **JournalEntry**: `Debit`, `Credit`, `JournalId`, `AccountHeadId`, `PartyId`
- **Journal**: `VoucherDate`, `VoucherNo`, `BranchId`

### Email Integration
- **Gmail API Integration**: Uses Replit Google Workspace connector for OAuth-based email sending.
- **GmailEmailService**: Service class for sending emails with PDF attachments.
- **Report Email Feature**: Reports can be emailed directly to customers/suppliers.
- **Email Pattern**: Branch email as sender, Party email as recipient; validates email addresses.
- **Attachment Support**: Reports generated as PDF attachments with professional HTML email body.

## External Dependencies
- **Database**: PostgreSQL
- **UI Framework**: MudBlazor
- **Reporting**: QuestPDF
- **Excel Handling**: ClosedXML
- **Email**: Google Gmail API via Replit connector