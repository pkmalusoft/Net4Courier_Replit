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
- **Proof of Delivery (POD)**:
    - **Mobile POD Capture**: Touch-friendly interface for delivery agents with AWB search/barcode scan.
    - **Photo Evidence**: Camera integration for capturing up to 3 delivery photos.
    - **Digital Signature**: Canvas-based signature capture with touch support.
    - **GPS Location**: Automatic geolocation capture for delivery verification.
    - **Delivery Status**: Delivered, Partial Delivery, Refused, Not Delivered options with reason codes.
    - **Bulk POD Update**: Desktop interface for updating multiple AWBs simultaneously.
    - **Barcode Scanner**: Device camera-based AWB barcode/QR scanning for quick lookup.
    - **Offline Support**: IndexedDB storage for offline POD capture with sync when online.
- **Shipment Status Timeline**:
    - **Database-Driven Status Management**: Flexible status groups and statuses configurable via admin UI instead of hardcoded enums.
    - **9 Status Groups**: Pre-Pickup, Collection, Origin Warehouse, Transit, Destination Warehouse, Delivery, Exception/Return, Billing, Closed with 27+ baseline statuses.
    - **Timeline History**: Complete chronological record of every status change with user, location, timestamp, and remarks.
    - **Automatic Status Updates**: Integrated into PickupInscan (SHIPMENT_COLLECTED, INSCAN_ORIGIN), Outscan/DRS (OUT_FOR_DELIVERY), POD capture (DELIVERED, POD_CAPTURED, DELIVERY_FAILED), and Invoice generation (INVOICED).
    - **AWB Timeline Component**: Visual timeline display on AWB details page with color-coded status groups.
    - **Status Management Admin Page**: CRUD operations for status groups and statuses with sequence ordering.
    - **CourierStatus Mapping**: Optional mapping from database statuses to legacy CourierStatus enum for backward compatibility.
- **MAWB Processing (Master Airwaybill)**:
    - **MasterAirwaybill Entity**: Stores MAWB header info including origin/destination cities, carrier details, flight info, departure/arrival times.
    - **MAWBBag Entity**: Individual bags within a MAWB with bag numbers, seal numbers, weights, and piece counts.
    - **Shipment-to-Bag Linking**: InscanMaster extended with MAWBId, MAWBBagId, BaggedAt fields to track which shipments are in which bags.
    - **Hold Tracking**: IsOnHold, HoldReason, HoldDate fields on InscanMaster prevent held shipments from being bagged.
    - **MAWBService**: Comprehensive validation logic including route matching (origin/destination city), hold checks, duplicate prevention, and status eligibility filtering.
    - **MAWBList Page**: View all MAWBs with status filters (Draft, Finalized, Dispatched), date range filtering.
    - **MAWBEntry Page**: Create/edit MAWB headers with origin/destination city autocomplete, carrier info, flight details, co-loader assignment.
    - **MAWBBagging Page**: Manage bags and add shipments via barcode scanning or manual selection grid with real-time validation.
    - **Barcode Scanning**: Live scan input with auto-validation, error messages (on-hold, already bagged, route mismatch, not found), and scan log.
    - **Finalization Workflow**: Pre-finalize validation blocks finalization if any shipment is on-hold; status updates to MANIFESTED.
    - **Status Integration**: BAGGED, UNBAGGED, MANIFESTED, IN_TRANSIT statuses added to timeline on MAWB actions.
    - **MAWB Manifest PDF**: QuestPDF report with MAWB header, bag summary, and shipment details including weights and dimensions.
- **Printing & Reports**:
    - **Air Waybill Print (A5)**: Full AWB document in A5 portrait format with shipper/consignee details, dimensions, charges, and signature areas.
    - **Manifest Labels (4x6)**: Standard shipping labels (4x6 inch) with bag/MAWB info, destination, weight, and COD indicators.
    - **Export Manifest**: Landscape A4 report for international shipments grouped by destination country with customs values.
    - **Domestic Manifest**: A4 report for local shipments grouped by destination region/state.
    - **Print Integration**: Print buttons on AWB Entry page and MAWB Bagging page for quick access to all report types.
- **Service Type Management**:
    - **ServiceType Entity**: Configurable service types with code, name, description, transit days, express flag, and sort order.
    - **Service Types Page**: Full CRUD UI at `/service-types` for managing service offerings.
    - **Seed Data**: 8 default service types (Standard, Express, Overnight, Same Day, Economy, Document Express, Freight, COD).
- **Party Masters Seed Data**:
    - **Co-Loaders**: 5 sample co-loader companies (FastTrack Logistics, Global Freight Partners, Swift Cargo Solutions, TransWorld Shipping, Pacific Logistics) with Account Receivable nature.
    - **Forwarding Agents**: 5 major carriers (DHL, FedEx, Aramex, UPS, TNT) with Account Payable nature.

## External Dependencies
- **Database**: PostgreSQL (hosted on Replit)
- **UI Framework**: MudBlazor (static assets stored locally in `wwwroot/lib/mudblazor/` for Replit compatibility)
- **Reporting**: QuestPDF (for generating various PDF documents)