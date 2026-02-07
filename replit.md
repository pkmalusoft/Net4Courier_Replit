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
- **Knowledge Base**: Integrated documentation using Markdig. Version 2.2 includes Cost Management Guides (setup, MAWB cost entry, actualization, profitability reports), updated Pricing & Billing reference sections, and February 2026 feature highlights.
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
- **Company Logo Integration**: All PDF reports, AWB prints, labels, invoices, receipts, manifests, and duty receipts dynamically load company logo from database (stored as base64 data URI). AWBPrintService and ReportingService accept `byte[]? logoData` and `string? companyName` parameters. ResolveLogoBytes helper in Program.cs handles both data URI and legacy file path formats. Fallback to "Net4Courier" text when no logo is configured.
- **Journal Entries on Invoices**: Automatic GL journal entry creation upon invoice posting for proper accounting.
- **Status Management Redesign**: Consolidated 9 status groups into 7 workflow-oriented groups (PRE-SHIPMENT, ORIGIN, TRANSIT, DESTINATION, DELIVERY, EXCEPTIONS, FINANCIAL). Added new statuses: Shipment Created, Weight Verified. Renamed statuses for clarity (Sorted, POD Completed, COD Collected, RTO In Progress). Merged COLLECTION + ORIGIN_WH into ORIGIN, CLOSED into FINANCIAL.
- **Status Event Mapping System**: Configurable mapping between operation events (AWB_ENTRY, PICKUP_CONFIRMATION, ORIGIN_INSCAN, MANIFEST_CREATE, OUTSCAN_DRS, POD_ENTRY, RTS_INITIATE, INVOICE_POST, COD_REMITTANCE, DELIVERY_ATTEMPT) and status codes. Uses StatusEventMappingService.ApplyEventStatus() to automatically apply statuses when operations are processed. Managed via Masters > Operations > Status Event Mappings page.
- **Duty Receipt Printout**: DUTY & TAX INVOICE format for International Import shipments with customs/duty charges. Includes company logo, barcode, customer details, shipment details, billing details (Custom Duty-PT, Admin Fee), payment terms, footnotes, payment slip section, and customer endorsement. Available via Tracking page for shipments with duty/VAT charges. API endpoint: `/api/report/duty-receipt/{id}`.
- **Timezone-Aware Display**: DateTimeService converts UTC timestamps to local timezone for display. Branch.TimeZoneId (default: "Asia/Dubai" for UAE/UTC+4) configures timezone per branch. Uses `DateTimeService.FormatDateTime()`, `FormatDate()`, `FormatTime()` methods in Razor pages.
- **Auto-Posting Configuration**: Configurable automatic journal posting for transaction pages. Transaction Page Types define which system pages (AWB_ENTRY, CUSTOMER_INVOICE, POD_UPDATE, etc.) require auto-posting. Posting Setup page provides field-level account mapping - each amount field (Freight, FuelSurcharge, VAT, COD, etc.) can be mapped to specific debit/credit chart of accounts. Uses TransactionFieldDefinitions helper class to define all amount fields for 12 transaction types. PostingSetupLine entity stores field-to-account mappings per company. Managed via Finance Setup > Transaction Page Types and Posting Setup pages.
- **DRS Reconciliation Workflow**: Complete DRS cash reconciliation with per-AWB charge allocation. Accountant issues cash receipt by selecting DRS (Finance > Delivery > Cash Receipt), then reconciles per-AWB with Material Cost/COD/Other Charges breakdown (Finance > Courier Reconciliation > AWB Reconciliation). DRSReconciliationLine entity stores per-AWB allocations with discount tracking and approval workflow. DRSReconciliationStatement entity stores completed statements with per-field totals (TotalMaterialCost, TotalCODAmount, TotalOtherCharges). Printable reconciliation statement available at `/drs-reconciliation-statement/{id}`. Variance calculation: Collected + Expenses - Receipt must equal zero before completing reconciliation.
- **Schema Auto-Sync System**: Automatic database schema synchronization at startup via `SchemaAutoSyncService`. Compares EF Core model to PostgreSQL database and auto-generates/executes CREATE TABLE and ALTER TABLE statements for missing tables/columns. Falls back to `scripts/schema_sync_full.sql` if auto-sync fails. On-demand schema script generation available via Platform Admin > Schema Utility page (`/admin/schema-utility`). Features: generate full schema script, download as SQL file, copy to clipboard, manual sync trigger. Uses TIMESTAMP WITH TIME ZONE for DateTime fields, proper NOT NULL constraints, and IF NOT EXISTS clauses for safe re-execution. Scope: additive only (new tables/columns); does not modify existing column types or foreign keys.

## External Dependencies
- **Database**: PostgreSQL
- **UI Framework**: MudBlazor
- **Reporting**: QuestPDF
- **Excel Handling**: ClosedXML
- **Email**: Google Gmail API via Replit connector

## Roadmap / Future Enhancements

### Onboarding System
**Current Status**: Planned

**Phase 1 - Core Guides (Next)**:
- Setup Guide page (`/setup-guide`) - Interactive wizard with progress tracking for first-time setup (8 sections: Company & Location, Operations Masters, Pricing, Customers, Finance, Users, System Check, Go-Live)
- Operations Workflow page (`/operations-workflow`) - Daily operations guide covering AWB Entry, Pickup, Inscan, Manifest, DRS, POD, RTS, Invoicing, COD Remittance
- Knowledge Base articles for reference documentation

**Phase 2 - Enhanced Experience**:
- In-app onboarding checklist with persistent progress tracking
- Screenshots per step for visual guidance
- Role-specific mini guides:
  - Courier Guide (Delivery, POD, COD)
  - Operations Guide (AWB, Pickup, Manifest, Tracking)
  - Finance Guide (Invoicing, Payments, Reports)
  - Admin Guide (Masters, Settings, User Management)
- Common Mistakes section with troubleshooting tips

### Menu Toggle Feature
**Status**: Requested
- Toggle between reorganized 12-section menu and legacy layout
- User preference persistence

### Cost Update Module
**Status**: Phase 1-5 Implemented (Feb 2026)

**Entities Added**:
- `AgentRateAssignment` - Links forwarding agents to cost rate cards (mirrors CustomerRateAssignment)
- `RateCardType` enum (Sales/Cost/Both) on RateCard entity
- Cost-specific fields on RateCardZone: FuelSurchargePercent, HandlingCharge, PerShipmentCharge, PeakSurcharge, CostMinCharge, SalesMinCharge (both sales and cost variants)
- Cost tracking fields on InscanMaster: SalesRateCardId, SalesFreightCharge, SalesFuelSurcharge, SalesHandlingCharge, SalesTotalCharge, CostRateCardId, CostAgentId, EstimatedFreightCost/FuelSurchargeCost/HandlingCost/TotalCost, ActualFreightCost/FuelSurchargeCost/HandlingCost/TotalCost, CostVariance, GrossMargin, GrossMarginPercent, IsCostLocked, CostLockedAt, CostInvoiceRef
- MAWB cost fields: ForwardingAgentId/Name, TotalMAWBCost, AirFreightCost, FuelSurchargeCost, HandlingCost, OtherCost, VendorInvoiceNo/Date, AllocationMethod (CostAllocationMethod enum), IsCostAllocated

**Engine**:
- `CalculateCost()` method in RatingEngineService - mirrors CalculateRate() using agent rate cards
- `CalculateRateAndCost()` method for simultaneous sales+cost calculation
- `FindApplicableCostRateCard()` - lookups via AgentRateAssignment or RateCardType=Cost/Both
- `CalculateCostCharges()` - mirrors sales charge calculation using cost rates from zones/slabs

**Pages**:
- `/cost-rate-cards` - Cost Rate Card Management with agent assignments
- `/mawb-cost-entry` - MAWB cost entry and HAWB allocation (pro-rata by weight/pieces/CBM/manual)
- `/cost-actualization` - Actual vs estimated cost entry with variance tracking and cost locking
- `/profitability-report` - Multi-view report (Shipment/Customer/Agent/Zone/MAWB) with margin analysis

**Nav**: Cost Rate Cards under Pricing; MAWB Cost Entry, Cost Actualization, Profitability Report under Finance > Cost Management

### Other Planned Features
- (Add future feature requests here)