# Net4Courier Knowledge Base

## Complete Operations & System Guide

This comprehensive knowledge base covers all aspects of Net4Courier - from pickup to delivery, finance to CRM. Use your browser's search (Ctrl+F / Cmd+F) to find specific topics.

---

# Table of Contents

1. [Operations Flow](#operations-flow)
   - [Pickup Management](#pickup-management)
   - [Inscan (Warehouse Receiving)](#inscan-warehouse-receiving)
   - [AWB Entry & Shipments](#awb-entry--shipments)
   - [Process Manifest (MAWB)](#process-manifest-mawb)
   - [DRS-Outscan (Dispatch)](#drs-outscan-dispatch)
   - [Proof of Delivery (POD)](#proof-of-delivery-pod)
   - [Return to Shipper (RTS)](#return-to-shipper-rts)
   - [Returns/RTO](#returnsrto)

2. [Reconciliation](#reconciliation)
   - [DRS Reconciliation](#drs-reconciliation)
   - [Courier Day-End Submission](#courier-day-end-submission)
   - [Courier Receipt](#courier-receipt)
   - [Expense Approval](#expense-approval)
   - [Courier Ledger](#courier-ledger)

3. [Accounts & Finance](#accounts--finance)
   - [General Ledger](#general-ledger)
   - [Account Receivables (AR)](#account-receivables-ar)
   - [Account Payables (AP)](#account-payables-ap)

4. [Customer Management (CRM)](#customer-management-crm)
   - [Customer Dashboard](#customer-dashboard)
   - [Customer Profiles](#customer-profiles)
   - [Contracts & Pricing](#contracts--pricing)
   - [SLA Management](#sla-management)
   - [Complaints & Tickets](#complaints--tickets)

5. [Pricing & Billing](#pricing--billing)
   - [Zone Management](#zone-management)
   - [Rate Cards](#rate-cards)
   - [Rate Simulator](#rate-simulator)
   - [Charges Configuration](#charges-configuration)
   - [Discounts & Contracts](#discounts--contracts)

6. [System Settings](#system-settings)
   - [Company Setup](#company-setup)
   - [Branch Management](#branch-management)
   - [Service Types](#service-types)
   - [Status Management](#status-management)
   - [User Management](#user-management)
   - [Geography Masters](#geography-masters)

7. [Compliance & Audit](#compliance--audit)

8. [Status Codes Reference](#status-codes-reference)

9. [Quick Reference Index](#quick-reference-index)

---

# Operations Flow

## Complete Shipment Lifecycle

```
Pickup Request → Collection → Inscan → AWB Entry → QC Complete
       ↓
MAWB Bagging → Manifest → Dispatch → In Transit
       ↓
Destination Arrival → Sorting → DRS/Outscan → Out for Delivery
       ↓
POD Capture → Delivered (or) RTS/Return
       ↓
Invoice → Payment → Closed
```

---

## Pickup Management

**Navigation:** Pickup Management

**Purpose:** Manage customer pickup requests from creation to collection.

### Workflow Steps

1. **Create Pickup Request**
   - Customer calls or submits online request
   - Enter customer details, pickup address, expected pieces
   - System assigns Pickup Request Number
   - Status: `PICKUP_REQUESTED`

2. **Assign Collection Agent**
   - Dispatcher assigns courier/agent to pickup
   - Agent receives notification with pickup details
   - Status: `ASSIGNED_FOR_COLLECTION`

3. **Collect Shipment**
   - Agent arrives at customer location
   - Verifies package count and condition
   - Collects COD amount if applicable
   - Status: `SHIPMENT_COLLECTED`

4. **Return to Hub**
   - Agent brings collected packages to warehouse
   - Ready for inscan process
   - Status changes upon inscan

### Key Fields

| Field | Description |
|-------|-------------|
| Pickup Request No | Auto-generated unique identifier |
| Customer | Party who requested pickup |
| Pickup Address | Collection location |
| Expected Pieces | Anticipated number of packages |
| Pickup Date/Time | Scheduled collection time |
| Assigned Agent | Courier assigned for collection |
| COD Amount | Cash to collect (if applicable) |
| Remarks | Special instructions |

### Keywords
pickup request, collection, customer pickup, assign agent, schedule pickup, pickup scheduling, courier assignment, collect shipment, pickup confirmation

---

## Inscan (Warehouse Receiving)

**Navigation:** Sorting/Hub Operations → Inscan

**Purpose:** Receive shipments into the warehouse and create AWB entries.

### Workflow Steps

1. **Scan Package**
   - Scan barcode or enter AWB/Reference number
   - If from Pickup Request, system auto-populates details
   - Status: `INSCANNED`

2. **Verify Details**
   - Check weight and dimensions
   - Verify piece count matches
   - Inspect for damage

3. **Quality Check**
   - Validate packaging
   - Confirm address accuracy
   - Check prohibited items
   - Status: `QC_COMPLETED`

4. **Convert to AWB**
   - If from Pickup Request, convert to full AWB
   - Assign AWB number based on branch prefix
   - Ready for bagging/dispatch

### Two-Step Pickup Conversion

| Step | Action | Result |
|------|--------|--------|
| Step 1 | Inscan | Creates initial warehouse entry |
| Step 2 | AWB Conversion | Generates full shipment record |

### Keywords
inscan, warehouse receiving, barcode scan, QC check, quality control, receive shipment, warehouse entry, package verification, inscan origin

---

## AWB Entry & Shipments

**Navigation:** Shipments

**Purpose:** Create and manage Air Waybills (AWB) for all shipments.

### Creating New AWB

1. **Shipper Details**
   - Select or create consignor (sender)
   - Enter origin address
   - Contact information

2. **Receiver Details**
   - Select or create consignee (receiver)
   - Enter destination address
   - Contact information

3. **Shipment Information**
   - Service type (Express, Standard, Economy, etc.)
   - Number of pieces
   - Actual weight
   - Dimensions (L x W x H) for volumetric weight
   - Declared value
   - Payment mode (Prepaid, COD, Credit, To Pay)

4. **Additional Charges**
   - Other charges (handling, packaging, insurance)
   - Fuel surcharge (auto-calculated)
   - Tax calculations

5. **AWB Generation**
   - System generates AWB number: `[BranchCode]-[Type]-[Sequence]`
   - Example: `AWB-DOM-001234` (Domestic), `AWB-INT-001234` (International)

### AWB Number Format

| Component | Description |
|-----------|-------------|
| Prefix | AWB |
| Type | DOM (Domestic), INT (International) |
| Sequence | Auto-increment per branch |

### Movement Types

| Type | Description |
|------|-------------|
| Domestic | Within same country |
| International Export | Outgoing to foreign country |
| International Import | Incoming from foreign country |
| Transhipment | Through transit only |

### Payment Modes

| Mode | Description |
|------|-------------|
| Prepaid | Sender pays in advance |
| COD | Cash on Delivery - Receiver pays |
| Credit | Billed to customer account |
| To Pay | Receiver pays (non-COD) |

### Key Actions

- **Print AWB**: Generate A5 AWB document
- **Print Label**: Generate 4x6 shipping label
- **View Timeline**: See complete status history
- **Update Status**: Manual status update
- **Create RTS**: Initiate return shipment

### Keywords
AWB entry, air waybill, shipment creation, consignor, consignee, sender, receiver, booking, create shipment, weight, dimensions, volumetric, COD, prepaid, credit, to pay, domestic, international, export, import

---

## Process Manifest (MAWB)

**Navigation:** Sorting/Hub Operations → Process Manifest

**Purpose:** Group shipments into bags and create Master Air Waybills for transit.

### MAWB Workflow

1. **Create MAWB**
   - Enter origin and destination cities
   - Specify carrier/airline details
   - Flight number and departure time
   - Select co-loader if applicable

2. **Create Bags**
   - Add bags to MAWB
   - Assign bag numbers and seal numbers
   - Each bag has weight and piece limits

3. **Add Shipments to Bags**
   - Scan AWB barcodes
   - System validates:
     - Route matches MAWB destination
     - Shipment not on hold
     - Not already bagged
   - Status: `BAGGED`

4. **Finalize MAWB**
   - System validates all bags
   - Blocks if any shipment on hold
   - Generates manifest documents
   - Status: `MANIFESTED`

5. **Dispatch**
   - Hand over to carrier
   - Status: `IN_TRANSIT`

### Bag Validation Rules

| Validation | Description |
|------------|-------------|
| Route Match | Shipment destination must match MAWB destination |
| Not On Hold | Shipments with hold status cannot be bagged |
| Not Duplicate | Same AWB cannot be in multiple bags |
| Status Check | Only eligible statuses can be bagged |

### MAWB Statuses

| Status | Description |
|--------|-------------|
| Draft | Being prepared |
| Finalized | Ready for dispatch |
| Dispatched | Handed to carrier |

### Keywords
MAWB, master airwaybill, manifest, bagging, bag shipment, flight manifest, carrier, co-loader, dispatch, finalize manifest, seal number, bag number, transit, air cargo

---

## DRS-Outscan (Dispatch)

**Navigation:** Sorting/Hub Operations → DRS-Outscan

**Purpose:** Create Delivery Run Sheets and dispatch shipments for last-mile delivery.

### DRS Workflow

1. **Create DRS**
   - Select delivery agent (courier)
   - Set delivery date
   - System generates DRS number

2. **Add Shipments**
   - Scan AWB barcodes
   - Only shipments with status `INSCAN_ORIGIN` or `RETURN_COMPLETED` eligible
   - System validates eligibility

3. **Finalize DRS**
   - Confirm all shipments added
   - Print DRS sheet for courier
   - Status: `OUT_FOR_DELIVERY`

4. **Handover**
   - Physical handover of packages to courier
   - Courier signs DRS acknowledgment

### DRS Contents

| Information | Description |
|-------------|-------------|
| DRS Number | Unique identifier for the run sheet |
| Courier | Assigned delivery agent |
| Date | Delivery date |
| Shipment List | AWBs included in this DRS |
| Total Pieces | Sum of all packages |
| COD Total | Cash to collect |
| Route | Delivery area/route |

### Keywords
DRS, delivery run sheet, outscan, dispatch, last mile, delivery agent, courier dispatch, out for delivery, delivery handover, route assignment

---

## Proof of Delivery (POD)

**Navigation:** Mobile POD Capture (field agent)

**Purpose:** Capture delivery confirmation with evidence.

### POD Capture Process

1. **Search Shipment**
   - Scan AWB barcode or enter manually
   - View shipment and recipient details

2. **Capture Delivery Status**
   - **Delivered**: Successful delivery
   - **Partial Delivery**: Some items delivered
   - **Refused**: Recipient refused delivery
   - **Not Delivered**: Unable to deliver

3. **Collect Evidence**
   - **Photo**: Capture up to 3 photos
   - **Signature**: Digital signature capture
   - **GPS Location**: Auto-captured coordinates
   - **Receiver Name**: Who received the package
   - **Relation**: Relationship to consignee

4. **COD Collection**
   - If COD shipment, collect cash
   - Record amount collected
   - Payment mode (Cash, Card, UPI)

5. **Submit POD**
   - System updates shipment status
   - Status: `DELIVERED` or `POD_CAPTURED`

### Delivery Status Options

| Status | Use Case |
|--------|----------|
| Delivered | Full delivery completed |
| Partial | Only some pieces delivered |
| Refused | Customer refused to accept |
| Not Delivered | Customer not available, wrong address, etc. |

### Offline Support
- POD can be captured without internet
- Data stored locally (IndexedDB)
- Auto-syncs when connection restored

### Keywords
POD, proof of delivery, delivery confirmation, signature, photo evidence, GPS, location, COD collection, delivered, refused, not delivered, mobile capture, delivery status

---

## Return to Shipper (RTS)

**Navigation:** Sorting/Hub Operations → Return to Shipper (RTS)

**Purpose:** Process return shipments back to original sender.

### RTS Workflow

1. **Initiate RTS**
   - From POD capture: Click "Create RTS"
   - From RTS page: Search original AWB

2. **RTS Details**
   - Original shipment auto-linked
   - Addresses automatically swapped
   - Specify RTS reason
   - Select charge mode

3. **RTS Charge Modes**

| Mode | Description |
|------|-------------|
| Free | No charge for return |
| Chargeable | Apply rate card to return shipment |

4. **RTS Statuses**
   - `RTS_REQUESTED`: Return initiated
   - `RTS_COLLECTED`: Picked up for return
   - `RTS_INSCANNED`: Received at hub
   - `RTS_IN_TRANSIT`: Being shipped back
   - `RTS_DELIVERED`: Returned to shipper

### Keywords
RTS, return to shipper, return shipment, failed delivery, return, address swap, reverse logistics, return reason, return pickup

---

## Returns/RTO

**Navigation:** Sorting/Hub Operations → Returns/RTO

**Purpose:** Manage Return to Origin (RTO) shipments.

### RTO vs RTS

| Type | Description |
|------|-------------|
| RTO | Return to Origin - Failed delivery returns |
| RTS | Return to Shipper - Customer-initiated returns |

### RTO Reasons
- Recipient not available
- Wrong address
- Refused by recipient
- Address incomplete
- Customer moved

### Keywords
RTO, return to origin, failed delivery, undelivered, return management, delivery failure

---

# Reconciliation

## DRS Reconciliation

**Navigation:** Reconciliation → DRS Reconciliation

**Purpose:** Match DRS dispatches with actual deliveries and returns.

### Reconciliation Process

1. **Select DRS**
   - Choose courier and date range
   - View all DRS for the period

2. **Update Status**
   - Mark each shipment as Delivered, RTS, or Pending
   - Verify COD amounts collected

3. **Balance Verification**
   - Total shipments dispatched
   - Total delivered
   - Total returned
   - COD collected vs expected

### Keywords
DRS reconciliation, delivery reconciliation, match delivery, verify dispatch, COD verification, delivery status update

---

## Courier Day-End Submission

**Navigation:** Reconciliation → Courier Day-End

**Purpose:** End-of-day submission by delivery agents.

### Day-End Process

1. **Courier Login**
   - Courier accesses submission page

2. **Submit Deliveries**
   - List of all assigned shipments
   - Update final status for each
   - Submit pending PODs

3. **Cash Submission**
   - Total COD collected
   - Expenses incurred
   - Net amount to deposit

### Keywords
day end, courier submission, daily closing, cash submission, delivery closing, end of day

---

## Courier Receipt

**Navigation:** Reconciliation → Courier Receipt

**Purpose:** Record cash received from delivery agents.

### Receipt Process

1. **Select Courier**
   - Choose delivery agent
   - View pending cash balance

2. **Record Receipt**
   - Date and amount received
   - Payment mode
   - Reference number

3. **Balance Update**
   - Courier ledger updated
   - COD account credited

### Keywords
courier receipt, cash receipt, COD receipt, agent payment, cash collection, deposit

---

## Expense Approval

**Navigation:** Reconciliation → Expense Approval

**Purpose:** Approve expenses claimed by delivery agents.

### Expense Types
- Fuel/Petrol
- Vehicle maintenance
- Mobile recharge
- Parking
- Toll charges
- Miscellaneous

### Approval Workflow

1. **Courier Submits Expense**
   - Enter expense type and amount
   - Attach receipt/proof

2. **Manager Reviews**
   - View pending expenses
   - Approve, reject, or request clarification

3. **Settlement**
   - Approved expenses adjusted against collections
   - Or reimbursed separately

### Keywords
expense approval, courier expense, reimbursement, fuel expense, delivery expense, expense claim

---

## Courier Ledger

**Navigation:** Reconciliation → Courier Ledger

**Purpose:** View complete transaction history for delivery agents.

### Ledger Contents

| Transaction Type | Description |
|-----------------|-------------|
| COD Collection | Amount collected from customers |
| Cash Deposit | Amount deposited to company |
| Expense | Approved expenses |
| Advance | Money given to courier |
| Adjustment | Manual adjustments |

### Keywords
courier ledger, agent ledger, transaction history, courier balance, COD ledger, agent account

---

# Accounts & Finance

## General Ledger

### Chart of Accounts

**Navigation:** Accounts & Finance → General Ledger → Settings → Chart-of-Accounts

**Purpose:** Define account structure for financial transactions.

#### Account Types

| Type | Nature | Examples |
|------|--------|----------|
| Assets | Debit | Cash, Bank, Receivables, Equipment |
| Liabilities | Credit | Payables, Loans, Advances |
| Income | Credit | Freight Revenue, Service Charges |
| Expenses | Debit | Salaries, Rent, Utilities |
| Equity | Credit | Capital, Retained Earnings |

#### Account Hierarchy
- Parent accounts contain sub-accounts
- Self-referential structure for unlimited nesting
- Example: Assets → Current Assets → Cash → Petty Cash

### Keywords
chart of accounts, COA, account head, ledger account, account type, account nature, account hierarchy

---

### Financial Period

**Navigation:** Accounts & Finance → General Ledger → Settings → Financial Period

**Purpose:** Manage financial years and monthly periods.

#### Features
- Auto-generate monthly periods for financial year
- Open/Close periods for transactions
- Only open periods accept transactions
- Admin controls period status

### Keywords
financial period, financial year, fiscal year, period closing, open period, close period, month end

---

### Control Accounts

**Navigation:** Accounts & Finance → General Ledger → Settings → Control Accounts

**Purpose:** Link operational modules to general ledger accounts.

#### Key Mappings

| Control Account | Purpose |
|----------------|---------|
| Accounts Receivable | Customer invoice postings |
| Accounts Payable | Vendor bill postings |
| Cash Account | Cash transactions |
| Bank Account | Bank transactions |
| Freight Revenue | Shipment revenue |
| COD Payable | COD amounts due to customers |

### Keywords
control accounts, account mapping, GL mapping, integration accounts, posting accounts

---

### Tax Setup

**Navigation:** Accounts & Finance → General Ledger → Settings → Tax Setup

**Purpose:** Configure tax rates and rules.

#### Tax Types
- GST (Goods & Services Tax)
- VAT (Value Added Tax)
- Service Tax
- Customs Duty

### Keywords
tax setup, GST, VAT, tax rate, tax configuration, tax calculation

---

### Cash and Bank

**Navigation:** Accounts & Finance → General Ledger → Transactions → Cash and Bank

**Purpose:** Record cash and bank transactions.

#### Transaction Types
- Cash Receipt
- Cash Payment
- Bank Deposit
- Bank Payment
- Bank Transfer

### Keywords
cash voucher, bank voucher, cash payment, bank payment, cash receipt, bank deposit, fund transfer

---

### Journal Voucher

**Navigation:** Accounts & Finance → General Ledger → Transactions → Journal Voucher

**Purpose:** Record adjusting and non-cash entries.

#### Use Cases
- Opening balances
- Year-end adjustments
- Error corrections
- Provisions
- Accruals

### Keywords
journal entry, journal voucher, adjusting entry, correction entry, provision, accrual

---

### Financial Statements

**Navigation:** Accounts & Finance → General Ledger → Reports → Financial Statements

#### Trial Balance
- List all accounts with balances
- Verify debits equal credits
- Filter by date range

#### Profit & Loss Account
- Revenue minus expenses
- Operating profit calculation
- Net profit/loss

#### Balance Sheet
- Assets, Liabilities, Equity
- Point-in-time financial position
- Verify accounting equation

### Keywords
trial balance, profit and loss, P&L, income statement, balance sheet, financial statement, financial report

---

## Account Receivables (AR)

### Invoices

**Navigation:** Accounts & Finance → Account Receivables → Transactions → Invoices

**Purpose:** View and manage customer invoices.

#### Invoice Types
- Shipment Invoice (based on AWBs)
- Service Invoice
- Credit Note (reduces balance)
- Debit Note (increases balance)

### Keywords
invoice, customer invoice, AR invoice, billing, invoice list, invoice history

---

### Generate Invoice

**Navigation:** Accounts & Finance → Account Receivables → Transactions → Generate Invoice

**Purpose:** Create invoices for unbilled shipments.

#### Invoice Generation Process

1. **Select Customer**
   - Choose customer to bill
   - View credit terms and limits

2. **Select Shipments**
   - Filter by date range
   - Show only uninvoiced AWBs
   - Multi-select shipments

3. **Review Charges**
   - Freight charges
   - Additional charges
   - Taxes

4. **Generate**
   - Create invoice
   - Print/Email to customer
   - Status: `INVOICED`

### Keywords
generate invoice, create invoice, billing, shipment billing, unbilled shipments, invoice generation

---

### Credit / Debit Notes

**Navigation:** Accounts & Finance → Account Receivables → Transactions → Credit / Debit Notes

**Purpose:** Adjust customer balances.

| Type | Effect |
|------|--------|
| Credit Note | Reduces customer balance (refund, discount) |
| Debit Note | Increases customer balance (additional charge) |

### Keywords
credit note, debit note, CN, DN, adjustment, refund, discount

---

### Customer Payments

**Navigation:** Accounts & Finance → Account Receivables → Transactions → Customer Payments

**Purpose:** Record payments received from customers.

#### Payment Process

1. **Select Customer**
   - View outstanding invoices
   - See account balance

2. **Record Payment**
   - Amount received
   - Payment mode (Cash, Cheque, Bank Transfer, Card)
   - Reference number

3. **Allocate to Invoices**
   - Apply payment to specific invoices
   - Or auto-allocate to oldest first

### Keywords
customer payment, receipt, payment collection, payment allocation, invoice payment, AR receipt

---

### Aging Reports

**Navigation:** Accounts & Finance → Account Receivables → Reports → Aging Reports

**Purpose:** Analyze overdue customer balances.

#### Aging Buckets
- Current (not due)
- 1-30 days overdue
- 31-60 days overdue
- 61-90 days overdue
- Over 90 days overdue

### Keywords
aging report, AR aging, overdue, outstanding, collection, receivables aging

---

## Account Payables (AP)

### Suppliers

**Navigation:** Accounts & Finance → Account Payables → Settings → Suppliers

**Purpose:** Manage vendor/supplier master data.

#### Supplier Types
- Co-loaders
- Forwarding agents
- Fuel vendors
- Service providers
- Landlords

### Keywords
supplier, vendor, AP vendor, supplier master, vendor list

---

### Vendor Bills

**Navigation:** Accounts & Finance → Account Payables → Transactions → Vendor Bills

**Purpose:** Record bills from vendors.

#### Bill Entry
- Select vendor
- Enter bill details (date, number, amount)
- Line items with account allocation
- Tax calculations

### Keywords
vendor bill, purchase invoice, AP invoice, supplier bill, expense bill

---

### Vendor Payments

**Navigation:** Accounts & Finance → Account Payables → Transactions → Vendor Payments

**Purpose:** Record payments made to vendors.

#### Payment Process
1. Select vendor
2. View outstanding bills
3. Record payment amount and mode
4. Allocate to bills

### Keywords
vendor payment, supplier payment, bill payment, AP payment, pay vendor

---

### Expense Management

**Navigation:** Accounts & Finance → Account Payables → Transactions → Expense Management

**Purpose:** Track and manage operational expenses.

### Keywords
expense management, expense tracking, operational expense, cost management

---

### TDS / Withholding

**Navigation:** Accounts & Finance → Account Payables → Transactions → TDS / Withholding

**Purpose:** Manage tax deducted at source on vendor payments.

### Keywords
TDS, withholding tax, tax deduction, TDS payment, TDS challan

---

### Vendor Aging

**Navigation:** Accounts & Finance → Account Payables → Reports → Vendor Aging

**Purpose:** Analyze payable dues by age.

### Keywords
vendor aging, AP aging, payable aging, outstanding payables

---

# Customer Management (CRM)

## Customer Dashboard

**Navigation:** Customer Management (CRM) → Customer Dashboard

**Purpose:** Overview of customer metrics and activities.

### Dashboard Widgets
- Total active customers
- Monthly shipment volume
- Revenue by customer
- Overdue balances
- Recent activities

### Keywords
customer dashboard, CRM dashboard, customer overview, customer metrics

---

## Customer Profiles

**Navigation:** Customer Management (CRM) → Customer Profiles

**Purpose:** Manage customer master data.

### Customer Information

| Field | Description |
|-------|-------------|
| Customer Code | Unique identifier |
| Company Name | Business name |
| Contact Person | Primary contact |
| Email/Phone | Communication details |
| Address | Billing and operational addresses |
| Party Type | Customer classification |
| Credit Terms | Payment terms (days) |
| Credit Limit | Maximum outstanding allowed |
| Account Nature | Receivable/Payable |

### Party Types

| Type | Description |
|------|-------------|
| Consignor | Sender/Shipper |
| Consignee | Receiver |
| Customer | Regular booking customer |
| Co-loader | Partner carrier |
| Forwarding Agent | Freight forwarder |
| Vendor | Service provider |

### Keywords
customer profile, customer master, party master, customer data, CRM, contact management, customer information

---

## Contracts & Pricing

**Navigation:** Customer Management (CRM) → Contracts & Pricing

**Purpose:** Manage customer-specific pricing agreements.

### Contract Features
- Customer-specific rate cards
- Volume discounts
- Validity periods
- Priority-based rate selection
- Effective date versioning

### Keywords
customer contract, pricing agreement, rate agreement, customer pricing, special rates, contract pricing

---

## SLA Management

**Navigation:** Customer Management (CRM) → SLAs

**Purpose:** Define and track Service Level Agreements.

### SLA Parameters
- Delivery time targets
- First attempt success rate
- POD upload time
- RTS processing time
- Complaint resolution time

### Keywords
SLA, service level agreement, delivery target, performance metrics, service commitment

---

## Complaints & Tickets

**Navigation:** Customer Management (CRM) → Complaints / Tickets

**Purpose:** Track and resolve customer issues.

### Complaint Types
- Delivery delay
- Damaged package
- Wrong delivery
- Missing items
- Billing dispute
- Staff behavior

### Ticket Workflow
1. Ticket created (customer/agent)
2. Assigned to handler
3. Investigation
4. Resolution
5. Customer feedback
6. Closed

### Keywords
complaint, ticket, customer issue, support ticket, grievance, resolution, customer service, dispute

---

# Pricing & Billing

## Zone Management

### Zone Categories

**Navigation:** Pricing & Billing → Zone Management → Zone Categories

**Purpose:** Group zones into categories for rate structuring.

### Keywords
zone category, zone group, geographic category

---

### Zones

**Navigation:** Pricing & Billing → Zone Management → Zones

**Purpose:** Define geographic zones for rate calculation.

### Zone Configuration

| Field | Description |
|-------|-------------|
| Zone Code | Unique identifier |
| Zone Name | Descriptive name |
| Category | Zone category |
| Countries | Countries in zone |
| Cities | Cities in zone |
| Postal Codes | PIN codes in zone |

### Zone Resolution Priority
1. City match
2. Country match
3. Default zone

### Keywords
zone matrix, zone definition, geographic zone, delivery zone, rate zone, postal zone

---

## Rate Cards

**Navigation:** Pricing & Billing → Rate Cards

**Purpose:** Configure pricing for shipments.

### Rate Card Structure

| Component | Description |
|-----------|-------------|
| Rate Card Name | Identifier |
| Movement Type | Domestic/International |
| Payment Mode | Prepaid/COD/Credit |
| Valid From/To | Validity period |
| Status | Draft/Pending/Active/Expired |

### Zone-Based Pricing

Each zone in a rate card has:
- Base weight and rate
- Additional per-kg rate
- Minimum charge
- Fuel surcharge %
- Tax mode

### Slab-Based Pricing

Weight slabs with different rules:
- **PerStep**: Charge per weight step
- **PerKg**: Charge per kilogram
- **FlatAfter**: Flat rate after base

### Rate Card Statuses

| Status | Description |
|--------|-------------|
| Draft | Being prepared |
| Pending Approval | Submitted for review |
| Active | Currently in use |
| Expired | Past validity date |
| Suspended | Temporarily disabled |

### Keywords
rate card, pricing, freight rate, shipping rate, weight slab, zone rate, pricing setup, rate configuration

---

## Rate Simulator

**Navigation:** Pricing & Billing → Rate Simulator

**Purpose:** Test rate calculations before applying.

### Simulator Inputs
- Origin and destination
- Weight and dimensions
- Service type
- Payment mode
- Customer (for contract rates)

### Output
- Applicable rate card
- Zone resolution path
- Weight calculation (actual vs volumetric)
- Slab charges breakdown
- Total with taxes

### Keywords
rate simulator, rate calculator, quote, pricing test, rate check, shipping cost calculator

---

## Charges Configuration

### Special Charges

**Navigation:** Pricing & Billing → Special Charges

**Purpose:** Configure special handling charges.

Examples:
- Hazardous material handling
- Temperature-controlled
- Oversized package
- Weekend delivery

### Keywords
special charges, handling charges, special handling, surcharge

---

### Fuel Surcharge

**Navigation:** Pricing & Billing → Fuel Surcharge

**Purpose:** Configure fuel surcharge percentages.

### Configuration
- Percentage based on fuel index
- Valid date ranges
- Movement type specific

### Keywords
fuel surcharge, FSC, fuel charge, fuel adjustment

---

### Other Charge Types

**Navigation:** Pricing & Billing → Other Charge Types

**Purpose:** Define additional charge types.

Examples:
- Packaging charge
- Insurance
- Address correction
- Redelivery charge
- Documentation fee

### Keywords
other charges, additional charges, miscellaneous charges, charge types

---

## Discounts & Contracts

**Navigation:** Pricing & Billing → Discounts & Contracts

**Purpose:** Manage volume discounts and special contracts.

### Discount Types
- Volume-based discount
- Customer-specific discount
- Promotional discount
- Loyalty discount

### Keywords
discount, volume discount, contract discount, special pricing, promotional rate

---

# System Settings

## Company Setup

**Navigation:** System Settings → Companies

**Purpose:** Configure company information.

### Company Details
- Company name and code
- Registration numbers (GST, PAN)
- Address
- Logo
- Contact information
- Country (for movement type calculation)

### Keywords
company setup, company master, company profile, organization setup

---

## Branch Management

**Navigation:** System Settings → Branches / Locations

**Purpose:** Manage branch/location master data.

### Branch Details
- Branch code (used in AWB prefix)
- Branch name
- Address
- Contact person
- Linked company

### Keywords
branch, location, office, hub, warehouse, branch master

---

## Service Types

**Navigation:** System Settings → Service Types

**Purpose:** Configure available service offerings.

### Default Service Types

| Service | Transit Days | Express |
|---------|--------------|---------|
| Standard | 3-5 days | No |
| Express | 1-2 days | Yes |
| Overnight | Next day | Yes |
| Same Day | Same day | Yes |
| Economy | 5-7 days | No |
| Document Express | 1 day | Yes |
| Freight | 5-10 days | No |
| COD | 2-3 days | No |

### Keywords
service type, service offering, delivery type, shipping service, express service, standard service

---

## Status Management

**Navigation:** System Settings → Status Management

**Purpose:** Configure shipment statuses and status groups.

### Status Groups

| Group | Description |
|-------|-------------|
| Pre-Pickup | Before collection |
| Collection | Pickup process |
| Origin Warehouse | At origin hub |
| Transit | Moving between locations |
| Destination Warehouse | At destination hub |
| Delivery | Last mile process |
| Exception/Return | Problems and returns |
| Billing | Financial processing |
| Closed | Completed shipments |

### Keywords
status management, shipment status, status group, status configuration, tracking status

---

## User Management

**Navigation:** System Settings → User Management

**Purpose:** Manage system users.

### User Details
- Username and password
- Full name and email
- Role assignment
- Branch assignment
- Active/Inactive status

### Keywords
user management, user creation, user access, login credentials, employee access

---

## User Types

**Navigation:** System Settings → User Types

**Purpose:** Define user classifications.

Examples:
- Admin
- Manager
- Operator
- Courier
- Customer

### Keywords
user type, user category, user classification

---

## Roles & Permissions

**Navigation:** System Settings → Roles & Permissions

**Purpose:** Configure access control.

### Role Configuration
- Role name
- Permissions per module
- Create/Read/Update/Delete access

### Keywords
roles, permissions, access control, user rights, security, authorization

---

## Geography Masters

### Countries

**Navigation:** System Settings → Geography Masters → Countries

**Purpose:** Maintain country master data.

### Keywords
country, country master, country list

---

### States

**Navigation:** System Settings → Geography Masters → States

**Purpose:** Maintain state/province data.

### Keywords
state, province, region, state master

---

### Cities

**Navigation:** System Settings → Geography Masters → Cities

**Purpose:** Maintain city master data.

### Keywords
city, city master, town, urban area

---

### Locations / Pincodes

**Navigation:** System Settings → Geography Masters → Locations / Pincodes

**Purpose:** Maintain postal code data.

### Keywords
pincode, postal code, ZIP code, location, area code, serviceability

---

# Compliance & Audit

## Audit Logs

**Navigation:** Compliance & Audit → Audit Logs

**Purpose:** Track all system activities.

### Logged Information
- User actions (create, update, delete)
- Login/logout events
- System changes
- Timestamp and IP address

### Keywords
audit log, activity log, user activity, system log, tracking log

---

## Regulatory Reports

**Navigation:** Compliance & Audit → Regulatory Reports

**Purpose:** Generate compliance reports.

### Keywords
regulatory report, compliance report, government report, statutory report

---

## Data Export

**Navigation:** Compliance & Audit → Data Export

**Purpose:** Export data for analysis or migration.

### Export Formats
- Excel (XLSX)
- CSV
- PDF

### Keywords
data export, export data, download data, data download

---

## Document Management

**Navigation:** Compliance & Audit → Document Management

**Purpose:** Store and manage documents.

### Keywords
document management, file storage, document upload, attachments

---

# Status Codes Reference

## Complete Status List

| ID | Code | Status Name | Group |
|----|------|-------------|-------|
| 1 | PICKUP_REQUESTED | Pickup Requested | Pre-Pickup |
| 2 | PICKUP_SCHEDULED | Pickup Scheduled | Pre-Pickup |
| 3 | ASSIGNED_FOR_COLLECTION | Assigned for Collection | Collection |
| 4 | PICKUP_ATTEMPTED | Pickup Attempted | Collection |
| 5 | SHIPMENT_COLLECTED | Shipment Collected | Collection |
| 6 | PICKUP_CANCELLED | Pickup Cancelled | Collection |
| 7 | INSCAN_ORIGIN | Inscanned at Origin | Origin Warehouse |
| 8 | QC_COMPLETED | QC Completed | Origin Warehouse |
| 9 | BAGGED | Bagged | Origin Warehouse |
| 10 | MANIFESTED | Manifested | Origin Warehouse |
| 11 | IN_TRANSIT | In Transit | Transit |
| 12 | ARRIVED_DESTINATION | Arrived at Destination | Destination Warehouse |
| 13 | INSCAN_DESTINATION | Inscanned at Destination | Destination Warehouse |
| 14 | SORTED | Sorted | Destination Warehouse |
| 15 | OUT_FOR_DELIVERY | Out for Delivery | Delivery |
| 16 | DELIVERY_ATTEMPTED | Delivery Attempted | Delivery |
| 17 | DELIVERED | Delivered | Delivery |
| 18 | POD_CAPTURED | POD Captured | Delivery |
| 19 | DELIVERY_FAILED | Delivery Failed | Exception/Return |
| 20 | RETURN_TO_ORIGIN | Return to Origin | Exception/Return |
| 21 | RETURN_COMPLETED | Return Completed | Exception/Return |
| 22 | ON_HOLD | On Hold | Exception/Return |
| 23 | LOST | Lost | Exception/Return |
| 24 | DAMAGED | Damaged | Exception/Return |
| 25 | RTS_REQUESTED | RTS Requested | Exception/Return |
| 26 | RTS_COLLECTED | RTS Collected | Exception/Return |
| 27 | RTS_INSCANNED | RTS Inscanned | Exception/Return |
| 28 | RTS_IN_TRANSIT | RTS In Transit | Exception/Return |
| 29 | RTS_DELIVERED | RTS Delivered | Exception/Return |
| 30 | INVOICED | Invoiced | Billing |
| 31 | CLOSED | Closed | Closed |

### Keywords
status code, status ID, shipment status, tracking status, courier status, AWB status

---

# Quick Reference Index

## Common Tasks

| Task | Navigation |
|------|------------|
| Create pickup request | Pickup Management |
| Receive shipment | Sorting/Hub Operations → Inscan |
| Create AWB | Shipments → New |
| Dispatch for delivery | Sorting/Hub Operations → DRS-Outscan |
| Capture POD | Mobile POD (field) |
| Generate invoice | Accounts & Finance → AR → Generate Invoice |
| Record payment | Accounts & Finance → AR → Customer Payments |
| Add customer | CRM → Customer Profiles |
| Create rate card | Pricing & Billing → Rate Cards |
| Check rate | Pricing & Billing → Rate Simulator |

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+F | Search this document |
| Enter | Submit form |
| Esc | Close dialog |
| Tab | Next field |

## Search Keywords Summary

**Operations:** pickup, collection, inscan, AWB, shipment, manifest, MAWB, bag, DRS, outscan, dispatch, POD, delivery, RTS, return, tracking

**Finance:** invoice, receipt, payment, journal, ledger, GL, AR, AP, tax, GST, aging, credit note, debit note

**CRM:** customer, party, contract, SLA, complaint, ticket

**Pricing:** rate card, zone, slab, fuel surcharge, discount, charges, simulator

**System:** company, branch, user, role, permission, status, service type

---

*Last Updated: January 2026*
*Version: 1.0*
*Net4Courier - Linked To Deliver*
