# Net4Courier - Blazor Server Migration

## Overview
Net4Courier is a comprehensive logistics management system designed to streamline courier operations. Its primary purpose is to manage shipments (AWB), customer relations, branch operations, financial transactions, and reporting. The project aims to replace legacy systems with a modern, modular, and scalable platform to enhance operational efficiency and provide real-time insights for logistics companies.

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
- **Role-Based Menu Visibility**: NavMenu uses default-deny security with HasValidRole flag. Courier role sees only Dashboards (limited), Delivery, Help, My Account. Administrator sees all except Platform Admin. PlatformAdmin sees everything including tenant management.

### Technical Implementations
- **Entity Management**: Comprehensive CRUD operations for core entities.
- **Financial Features**: Includes invoice/receipt management, journaling, account heads, financial period management, dynamic other charges, AR/AP, Cash/Bank Vouchers, Bank Account Management, and comprehensive financial reports. Native GL module with long-based IDs, financial periods, year-end closing, and opening balances.
- **Operations Workflow**: Covers AWB entry/generation, pickup management, outscan/DRS, AWB tracking, POD, RTS, MAWB processing, COD Remittance, and Transfer Order management.
- **Prepaid AWB Management**: AWB Stock Management and Prepaid AWB Sales module with integrated accounting and configurable stock fields per branch.
- **Enhanced Dashboards**: Unified dispatcher view and global autocomplete search across AWBs, Customers, and Invoices. User-personalized Favourites Dashboard.
- **Unified Shipment Processing**: Single pages for Import Shipment Charges, Warehouse Inscan, and Shipment Lists for both domestic and import AWBs.
- **Master Data Management**: Configurable Rate Card Management, Service Type, Shipment Mode, Port Master, Currency, Geographic Master Data, and enhanced Customer Master with configurable account types and SLA management.
- **Enhanced Rate Card System**: Supports ServiceType and ShipmentMode filtering, zone categories with agent-based movement restrictions, flexible pricing (zone-level pricing, slab rules), tax calculation, and cost tracking for profit analysis. Automatic freight charge calculation.
- **Regulatory Compliance**: Empost Regulatory Compliance Module for UAE courier licensing.
- **API Integration**: Configuration and webhook endpoints for third-party booking websites.
- **Knowledge Base**: Integrated documentation using Markdig.
- **Cash & Bank Module**: Independent transaction management and bank account management.
- **Error Handling**: Robust global error handling with `ErrorBoundary`, `PageErrorHandler`, and `MudBlazor Snackbar`.
- **Demo Data & Setup**: Admin feature to create/delete demo data, initial setup dialog, and a secure initial setup wizard.
- **Platform Administration**: Dedicated admin section with Tenant Settings, Subscription Management, and user management features.
- **Utility Features**: CLI password reset utility, AWB barcode generation, tracking/shipment invoice PDF generation, customer CRM with complaints/tickets, branch display settings, branch currency as default, and "Delete All Business Data" feature.
- **Audit Log System**: Comprehensive audit logging for all entity changes, accessible via a dashboard with filters and Excel export.
- **Customer Zones**: Geographic zone management for customer and courier assignments, facilitating pickup notifications in the PWA mobile app.
- **AWB Configuration**: Per-branch, per-movement-type AWB numbering system for Domestic, InternationalExport, InternationalImport, and Transhipment.
- **Email Invoice**: Direct customer invoice emailing with company logo, commercial/customs format, and PDF attachment.
- **Financial Reporting**: Day Book Register with detailed filtering and Excel export. Customer Statement and Customer Ledger reports querying source tables for accuracy.
- **Invoice PDF Improvements**: Dynamic company logo loading, redesigned commercial/customs format, and in-window preview functionality.
- **Journal Entries on Invoices**: Automatic GL journal entry creation upon invoice posting for proper accounting.
- **Status Management Redesign**: Consolidated 9 status groups into 7 workflow-oriented groups (PRE-SHIPMENT, ORIGIN, TRANSIT, DESTINATION, DELIVERY, EXCEPTIONS, FINANCIAL). Added new statuses: Shipment Created, Weight Verified. Renamed statuses for clarity (Sorted, POD Completed, COD Collected, RTO In Progress). Merged COLLECTION + ORIGIN_WH into ORIGIN, CLOSED into FINANCIAL.

## External Dependencies
- **Database**: PostgreSQL
- **UI Framework**: MudBlazor
- **Reporting**: QuestPDF
- **Excel Handling**: ClosedXML
- **Email**: Google Gmail API via Replit connector