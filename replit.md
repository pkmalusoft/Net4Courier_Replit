# Net4Courier - Blazor Server Migration

## Overview
Net4Courier is a comprehensive logistics management system designed to streamline courier operations. Its primary purpose is to manage shipments, customer relations, branch operations, financial transactions, and reporting. The project aims to replace legacy systems with a modern, modular, and scalable platform to enhance operational efficiency and provide real-time insights for logistics companies. It focuses on managing AWB, customer relations, branch operations, and financial transactions. Key capabilities include comprehensive CRUD for core entities, financial management with GL, operations workflow covering pickup to POD, prepaid AWB management, enhanced dashboards, unified shipment processing, and robust master data management.

## User Preferences
- MudBlazor for all UI components
- Modular architecture like TrueBookserp pattern
- PostgreSQL partitioning by TransactionDate for performance
- Party/PartyAddress normalization for storage efficiency
- Navigation menu reorganized into 12 role-based workflow sections: Dashboards, Operations (workflow-based), Delivery (courier-focused), Customers, Pricing, Finance (AR/AP/GL), Reports (consolidated), Masters, Settings (Organization + User Management), Compliance, Help, My Account

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
- **Role-Based Menu Visibility**: NavMenu uses default-deny security with HasValidRole flag.

### Technical Implementations
- **Entity Management**: Comprehensive CRUD operations for core entities.
- **Financial Features**: Includes invoice/receipt management, journaling, account heads, financial period management, dynamic other charges, AR/AP, Cash/Bank Vouchers, Bank Account Management, and comprehensive financial reports with a native GL module.
- **Operations Workflow**: Covers AWB entry/generation, pickup management, outscan/DRS, AWB tracking, POD, RTS, MAWB processing, COD Remittance, and Transfer Order management.
- **Prepaid AWB Management**: AWB Stock Management and Prepaid AWB Sales module with integrated accounting.
- **Enhanced Dashboards**: Unified dispatcher view, global autocomplete search, and user-personalized Favourites Dashboard.
- **Unified Shipment Processing**: Single pages for Import Shipment Charges, Warehouse Inscan, and Shipment Lists for both domestic and import AWBs.
- **Master Data Management**: Configurable Rate Card Management, Service Type, Shipment Mode, Port Master, Currency, Geographic Master Data, and enhanced Customer Master.
- **Enhanced Rate Card System**: Supports ServiceType and ShipmentMode filtering, zone categories, flexible pricing, tax calculation, and cost tracking.
- **Regulatory Compliance**: Empost Regulatory Compliance Module for UAE courier licensing.
- **Error Handling**: Robust global error handling with `ErrorBoundary`, `PageErrorHandler`, and `MudBlazor Snackbar`.
- **Demo Data & Setup**: Admin feature to create/delete demo data, initial setup dialog, and a secure initial setup wizard.
- **Platform Administration**: Dedicated admin section with Tenant Settings, Subscription Management, user management features, and Branch Data Reassignment tool.
- **Utility Features**: CLI password reset, AWB barcode generation, tracking/shipment invoice PDF generation, customer CRM, and "Delete All Business Data" feature.
- **Audit Log System**: Comprehensive audit logging for all entity changes with dashboard access and Excel export.
- **Customer Zones**: Geographic zone management for customer and courier assignments.
- **AWB Configuration**: Per-branch, per-movement-type AWB numbering system.
- **Email Invoice**: Direct customer invoice emailing with dynamic content.
- **Financial Reporting**: Day Book Register, Customer Statement, and Customer Ledger reports.
- **Invoice PDF Improvements**: Dynamic company logo loading, redesigned formats, and in-window preview.
- **Journal Entries on Invoices**: Automatic GL journal entry creation upon invoice posting.
- **Status Management Redesign**: Consolidated 9 status groups into 7 workflow-oriented groups with new and renamed statuses for clarity.
- **Status Event Mapping System**: Configurable mapping between operation events and status codes for automatic status application.
- **Duty Receipt Printout**: DUTY & TAX INVOICE format for International Import shipments with customs/duty charges.
- **Timezone-Aware Display**: `DateTimeService` converts UTC timestamps to local timezone for display based on `Branch.TimeZoneId`.
- **Auto-Posting Configuration**: Configurable automatic journal posting for transaction pages with field-level account mapping.
- **DRS Reconciliation Workflow**: Complete DRS cash reconciliation with per-AWB charge allocation, variance calculation, and a printable statement.
- **Schema Auto-Sync System**: Automatic database schema synchronization at startup, generating/executing CREATE TABLE and ALTER TABLE statements for missing tables/columns. Provides on-demand schema script generation via Platform Admin.
- **Cost Update Module**: Implemented Cost Management with `AgentRateAssignment`, `RateCardType` enum, cost-specific fields on `RateCardZone`, and cost tracking fields on `InscanMaster`. Includes a `CalculateCost()` method in `RatingEngineService` and dedicated pages for Cost Rate Card Management, MAWB cost entry, cost actualization, and profitability reporting.
- **LinkDel PWA (Courier Mobile App)**: PWA for courier roles with mobile-first layout, courier-specific login, home dashboard, JavaScript interop for device features (GPS, camera, barcode, navigation), Phase 2 pickup workflow with list/detail pages, accept/reject, collect with GPS/actual pieces/weight, attempt recording with status history tracking, and courier-scoped authorization. Phase 3 delivery workflow with DRS-based delivery list, delivery detail with consignee info/address/navigation, POD capture (receiver name, relation, collected amount), delivery attempt with predefined reasons and RTS marking, DRS counter updates on delivery/attempt. Phase 4 expenses & cash: expense list with Today/Week/All tabs and Add Expense dialog (DRS selection, expense type, amount, description), COD cash submission against DRS with server-side balance validation, daily summary with operations stats/collections/expenses/net position. Phase 5 offline & notifications: IndexedDB-based offline data caching wired into Pickups/Deliveries/Expenses pages (cache on load, fallback on offline), offline mutation queue with background sync via service worker, sync processing with authorization-checked server-side mutation handling (accept/reject pickup, add expense, submit cash), online/offline status indicator in layout, push notification support with VAPID-based subscription creation and service worker push/notificationclick handlers.

## External Dependencies
- **Database**: PostgreSQL
- **UI Framework**: MudBlazor
- **Reporting**: QuestPDF
- **Excel Handling**: ClosedXML
- **Email**: Google Gmail API