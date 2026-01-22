# Net4Courier - Blazor Server Migration

## Overview
Net4Courier is a comprehensive courier/logistics management system designed to manage all facets of courier operations, including shipments (AWB), customer relationship management, branch operations, financial transactions, and extensive reporting capabilities. Its purpose is to provide a robust, scalable, and modern platform for logistics companies, enhancing operational efficiency and providing real-time insights, replacing outdated legacy systems with a modular, maintainable, and high-performance solution.

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
- **Layout**: Standard dashboard with `MainLayout`, `NavMenu`, and organized navigation.
- **Responsive Design**: Key modules are optimized for mobile devices.
- **Modern Login Page**: Split-screen design with courier illustration and compact login card.

### Technical Implementations
- **Entity Management**: Comprehensive CRUD for core entities like Company, Branch, User, Role, Financial Year, Parties, AWB, Invoices, Receipts.
- **Party Classification**: Parties categorized by `PartyType` with corresponding `AccountNature` for financial tracking.
- **Customer Account Number**: Auto-generation or manual entry with validation.
- **Operations Workflow**: Includes AWB entry and generation, pickup management (request to inscan), pickup schedules, outscan/DRS management, and AWB tracking.
- **Financial Features**: Invoice/receipt management, journaling, self-referential account heads, financial period management, dynamic other charges, and comprehensive Account Receivables (Customer Master, Credit Notes, Aging Reports, AR Reports) and Account Payables (Supplier Master, Debit Notes).
- **Reporting**: QuestPDF integration for AWB labels, Invoice, Receipt PDFs, and Excel export via ClosedXML.
- **Automatic Movement Type Calculation**: Determines shipment type based on origin/destination.
- **Rate Card Management**: Zone matrix, configurable rate cards with slab-based pricing, slab rule templates, customer rate assignments, rating engine service with formula trace, and a rate simulator.
- **Proof of Delivery (POD)**: Mobile POD capture with photo, signature, GPS, offline support, bulk POD update, and Excel batch upload.
- **Unified Status Change System**: `UpdateStatusDialog` for shipments and pickup requests, `ShipmentStatusHistory` and `PickupStatusHistory` for audit trails, database-driven status management, timeline history, automatic status updates, and a public tracking page.
- **MAWB Processing (Master Airwaybill)**: Entities for `MasterAirwaybill`, `MAWBBag`, shipment-to-bag linking, `MAWBService` for validation, UI for MAWB management, finalization workflow, and MAWB Manifest PDF generation.
- **Printing & Reports**: AWB Print, Manifest Labels, Export/Domestic Manifests.
- **Service Type Management**: Configurable service types via CRUD UI.
- **Return to Shipper (RTS)**: Workflow for return shipments with address swapping and status tracking.
- **Forgot Password Page**: Dedicated page for password reset requests.
- **Multi-Branch Management**: Companies can have multiple branches, each with its own currency and warehouses. Supports user-branch assignments, branch-restricted login, and displays branch info in the dashboard header.
- **Import Module (Air/Sea/Land)**: `ImportMaster`, `ImportBag`, `ImportShipment` entities, import dashboard, import entry, customs processing, and Excel import functionality with mode-specific templates and validation.
- **API Integration (Third-Party Booking Websites)**: `ApiSetting` entity for configuration, API settings page, webhook endpoint for receiving booking data, `BookingWebhookService` for validation and `PickupRequest` creation, secure storage of credentials, webhook authentication, and connection testing.
- **Knowledge Base**: Comprehensive documentation including "How To Guides", operational flow, reconciliation, accounts & finance, customer management, pricing & billing, system settings, compliance & audit, and status codes reference, rendered using Markdig.

## External Dependencies
- **Database**: PostgreSQL
- **UI Framework**: MudBlazor
- **Reporting**: QuestPDF
- **Excel Handling**: ClosedXML