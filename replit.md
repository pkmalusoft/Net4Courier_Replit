# Net4Courier - Blazor Server Migration

## Overview
Net4Courier is a comprehensive logistics management system designed to streamline courier operations, including shipment management (AWB), customer relations, branch operations, financial transactions, and reporting. Its primary goal is to replace legacy systems with a modern, modular, and scalable platform that enhances operational efficiency and provides real-time insights for logistics companies.

## User Preferences
- MudBlazor for all UI components
- Modular architecture like TrueBookserp pattern
- PostgreSQL partitioning by TransactionDate for performance
- Party/PartyAddress normalization for storage efficiency

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
- **Financial Reports**: 17 comprehensive reports including Trial Balance (Standard, Grouped, Monthly, Detailed), Profit & Loss (Item Wise, Period Comparison), Customer Reports (Ledger, Aging, Statement), Chart of Accounts, Cash Flow Statement (Indirect Method), and Balance Sheet.
- **Operations Workflow**: Covers AWB entry and generation, pickup management (request to inscan), outscan/DRS management, AWB tracking, Proof of Delivery (POD), Return to Shipper (RTS), Master Airwaybill (MAWB) processing, COD Remittance, Pickup Commitment, Pickup Incentive, and Transfer Order management.
- **Enhanced Pickup Dashboard**: Unified dispatcher view with commitment status integration, showing committed pickups, expiring soon alerts, available pickups, courier performance metrics, countdown timers, and quick actions for commitment confirmation/override with 30-second auto-refresh.
- **Master Data Management**: Includes configurable Rate Card Management, Service Type Management, Shipment Mode Management, Port Master, Currency Management, and extensive Geographic Master Data.
- **Enhanced Customer Master**: Configurable Account Types (Cash, Credit, Prepaid), Customer Branches/Departments management with contact details, Client Address tracking, tabbed Party dialog interface (General Info + Branches tabs), and improved visual layout with logo display and Account Type column.
- **SLA Management**: Customer-specific Service Level Agreements with transit rules by zone/country/service type, credit terms, liability limits, volumetric settings, and status workflow (Draft→PendingApproval→Active→Expired/Terminated/Suspended) with approval tracking.
- **Logo Management**: Company and Party logo upload with 2MB file limit, PNG/JPG/SVG/GIF validation, preview, and storage in /wwwroot/uploads/logos/.
- **Regulatory Compliance**: Empost Regulatory Compliance Module for UAE courier licensing, advance payments, quarterly submissions, and royalty calculations.
- **API Integration**: Provides configuration and webhook endpoints for third-party booking websites.
- **Knowledge Base**: Integrated documentation using Markdig for "How To Guides" and operational flows.
- **Error Handling**: Robust global error handling with `ErrorBoundary`, `PageErrorHandler`, and `MudBlazor Snackbar` integration.

## Entity Property Reference
Key entity property names used in reports:
- **Branch**: `Id`, `Name`, `Code` (not BranchId/BranchName)
- **Party**: `Id`, `Name`, `Code` (not PartyId/PartyCode)
- **AccountHead**: `Code`, `Name`, `Classification` (enum), `ParentId`, `Parent`, `AccountGroup`, `AccountNature`
- **JournalEntry**: `Debit`, `Credit` (nullable decimals), `JournalId`, `AccountHeadId`, `PartyId`
- **Journal**: `VoucherDate`, `VoucherNo`, `BranchId` (date/branch info on parent Journal, not JournalEntry)

## External Dependencies
- **Database**: PostgreSQL
- **UI Framework**: MudBlazor
- **Reporting**: QuestPDF
- **Excel Handling**: ClosedXML