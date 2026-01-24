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
- **Cash and Bank Vouchers**: Comprehensive cash and bank transaction management via `/cash-bank` route. Features include:
  - Tabbed interface for Cash Vouchers (CV) and Bank Vouchers (BV)
  - Receipt/Payment direction selection with proper accounting treatment
  - Receipt vouchers: Cash/Bank account debited, counterpart accounts credited
  - Payment vouchers: Cash/Bank account credited, counterpart accounts debited
  - Multi-line journal entries with account selection from Chart of Accounts
  - Automatic voucher number generation (CV-YYYYMM-NNNNN or BV-YYYYMM-NNNNN)
  - Post/Unpost workflow for voucher lifecycle management
  - Date and account filtering with Excel export
  - View/Edit/Delete functionality for draft vouchers
- **AWB Other Charges Management**: Special Charges Management page with tabbed interface for AWB-specific charges and customer-level special charges. Features include:
  - AWB search with autocomplete for selecting shipments
  - Add charges using Other Charge Types master (percentage or flat amount)
  - Automatic percentage calculation based on AWB's Courier Charge
  - Edit/delete charge functionality with confirmation dialogs
  - Automatic update of InscanMaster.OtherCharge total when charges change
  - Seamless integration with Invoice Entry - charges automatically flow to invoices when unbilled AWBs are loaded
- **Reporting**: QuestPDF integration for AWB labels, Invoice, Receipt PDFs, and Excel export via ClosedXML.
- **Automatic Movement Type Calculation**: Determines shipment type based on origin/destination.
- **Rate Card Management**: Zone matrix, configurable rate cards with slab-based pricing, slab rule templates, customer rate assignments, rating engine service with formula trace, and a rate simulator.
- **Proof of Delivery (POD)**: Mobile POD capture with photo, signature, GPS, offline support, bulk POD update, and Excel batch upload.
- **Unified Status Change System**: `UpdateStatusDialog` for shipments and pickup requests, `ShipmentStatusHistory` and `PickupStatusHistory` for audit trails, database-driven status management, timeline history, automatic status updates, and a public tracking page.
- **MAWB Processing (Master Airwaybill)**: Entities for `MasterAirwaybill`, `MAWBBag`, shipment-to-bag linking, `MAWBService` for validation, UI for MAWB management, finalization workflow, and MAWB Manifest PDF generation.
- **Printing & Reports**: AWB Print, Manifest Labels, Export/Domestic Manifests.
- **Service Type Management**: Configurable service types via CRUD UI.
- **Shipment Mode Management**: Configurable shipment modes (Air, Sea, Road/Surface, Rail, Multimodal) with CRUD UI. Mode of Shipment dropdown integrated in AWB entry/new pages after Movement field.
- **Port Master Management**: Comprehensive port database with support for Airports, Seaports, and Land Borders. Includes IATA/ICAO codes for airports, UN/LOCODE for seaports, with filtering by port type. Pre-seeded with UAE ports and major international hubs.
- **Currency Management**: Currency entity with Code, Name, Symbol, and DecimalPlaces. Pre-seeded with 15 currencies (AED, SAR, QAR, KWD, BHD, OMR, JOD, USD, INR, EUR, PKR, BDT, MYR, IDR, PHP).
- **Geographic Master Data**: Comprehensive seed data including:
  - 16 Countries: UAE, Saudi Arabia, Qatar, Kuwait, Bahrain, Oman, Jordan, India, Pakistan, Bangladesh, Malaysia, Indonesia, Philippines, Singapore, UK, USA
  - 73 States/Emirates/Provinces for all countries
  - 72 Major Cities with hub designations for capitals
  - 61 Locations with pincodes for major cities
- **Return to Shipper (RTS)**: Workflow for return shipments with address swapping and status tracking.
- **Forgot Password Page**: Dedicated page for password reset requests.
- **Multi-Branch Management**: Companies can have multiple branches, each with its own currency and warehouses. Supports user-branch assignments, branch-restricted login, and displays branch info in the dashboard header.
- **Import Module (Air/Sea/Land)**: `ImportMaster`, `ImportBag`, `ImportShipment` entities, import dashboard, import entry, customs processing, and Excel import functionality with mode-specific templates and validation.
- **API Integration (Third-Party Booking Websites)**: `ApiSetting` entity for configuration, API settings page, webhook endpoint for receiving booking data, `BookingWebhookService` for validation and `PickupRequest` creation, secure storage of credentials, webhook authentication, and connection testing.
- **Knowledge Base**: Comprehensive documentation including "How To Guides", operational flow, reconciliation, accounts & finance, customer management, pricing & billing, system settings, compliance & audit, and status codes reference, rendered using Markdig.
- **Empost Regulatory Compliance Module**: Complete UAE courier licensing management including:
  - License Management: Track Empost license details, validity periods, and renewal dates
  - Advance Payments: Record and track mandatory AED 100,000 minimum advance payments
  - Quarterly Periods: Manage quarterly submission periods with lock/unlock/submit workflow
  - Fee Calculation: Automatic 10% royalty calculation on taxable shipments with exemption rules (>30kg, import, transhipment)
  - Settlements: Track quarterly settlements with advance utilization and balance due calculations
  - Return Adjustments: Handle fee adjustments for returned/cancelled shipments
  - Audit Reports: Comprehensive audit trail for all Empost-related actions
  - Entities: EmpostLicense, EmpostAdvancePayment, EmpostQuarter, EmpostShipmentFee, EmpostQuarterlySettlement, EmpostReturnAdjustment, EmpostAuditLog
  - SQL Schema: `sql/empost_schema.sql` for manual database deployment
- **Bank Account Management**: Dedicated BankAccount entity for managing bank accounts including:
  - Account Details: AccountNumber, AccountName, BankName, BranchName
  - International Banking Codes: SWIFT code and IBAN for cross-border transactions
  - Financial Tracking: Opening balance, opening balance date, and currency support
  - Chart of Accounts Integration: Link to AccountHead for ledger posting
  - Entities: BankAccount
  - UI: BankAccounts.razor (list), BankAccountEdit.razor (create/edit)
  - Navigation: Accounts & Finance > General Ledger > Settings > Bank Accounts
- **Bank Reconciliation Module**: Complete bank statement reconciliation for bank accounts including:
  - Reconciliation Sessions: Create reconciliation sessions for bank accounts with statement date and balances
  - Statement Import: Import bank statements from CSV with flexible column mapping (date, description, debit, credit, cheque number, reference)
  - Auto-Matching: Automatic matching of statement lines to bank vouchers using multiple algorithms (exact amount/date, cheque number, reference matching with confidence scoring)
  - Manual Matching: Side-by-side interface for manual matching of unmatched transactions
  - Adjustments: Record adjustments (bank fees, interest, charges, unrecorded deposits/withdrawals) with posting capability
  - Status Workflow: Draft → InProgress → Completed → Locked with status enforcement on operations
  - Balance Calculation: Automatic book balance calculation and difference tracking
  - Tabbed Wizard Interface: 5-step workflow (Import Statement → Auto Match → Manual Match → Adjustments → Finalize)
  - Entities: BankReconciliation (links to BankAccount), BankStatementImport, BankStatementLine, ReconciliationMatch, ReconciliationAdjustment
  - Services: BankReconciliationService, BankStatementImportService
  - UI: BankReconciliation.razor (list), ReconciliationWorkspace.razor (tabbed wizard), StartReconciliationDialog, ImportStatementDialog, ReconciliationAdjustmentDialog
  - Navigation: Accounts & Finance > General Ledger > Transactions > Bank Reconciliation
- **Tax Management Module**: Configurable tax rates for VAT, GST, and other taxes including:
  - Tax Rate Configuration: Code, Name, Description, Rate percentage with effective date ranges
  - Default Tax Rate: One tax rate can be marked as default for automatic selection
  - Account Linking: Link tax rates to Chart of Accounts for automatic posting
  - Active/Inactive Management: Enable/disable tax rates without deletion
  - Entities: TaxRate with AccountHead relationship
  - UI: TaxManagement.razor with full CRUD operations
  - Pre-seeded Rates: VAT 5% (default), Zero Rated 0%, Tax Exempt 0%
  - Navigation: Accounts & Finance > General Ledger > Settings > Tax Setup
  - SQL Schema: `sql/bank_tax_schema.sql` for deployment

## External Dependencies
- **Database**: PostgreSQL
- **UI Framework**: MudBlazor
- **Reporting**: QuestPDF
- **Excel Handling**: ClosedXML