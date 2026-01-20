# Net4Courier Knowledge Base

## Complete Operations & System Guide

This comprehensive knowledge base covers all aspects of Net4Courier - from pickup to delivery, finance to CRM. Use your browser's search (Ctrl+F / Cmd+F) to find specific topics.

---

# Table of Contents

0. [How To Guides](#how-to-guides) - Quick step-by-step tutorials
   - [Create a New Company](#how-to-create-a-new-company)
   - [Create a New Branch](#how-to-create-a-new-branch)
   - [Create a New Warehouse](#how-to-create-a-new-warehouse)
   - [Create a New Shipment](#how-to-create-a-new-shipment)
   - [Create Pickup Request (Staff)](#how-to-create-a-new-pickup-request-by-staff)
   - [Create Pickup Request (Customer)](#how-to-create-a-new-pickup-request-by-customers)
   - [Give Access for Agents/Customers/Vendors](#how-to-give-access-to-the-system-for-agentscustomersvendors)
   - [Create New Users](#how-to-create-new-users)
   - [Give Menu Access to Users](#how-to-give-access-to-menu-for-users)
   - [Restrict Menu Access](#how-to-restrict-menu-access-to-users)
   - [Process Import Customs Clearance](#how-to-process-import-customs-clearance)
   - [Create Import via Excel Upload](#how-to-create-import-by-uploading-excel-file)
   - [Update Bulk POD](#how-to-update-bulk-pod)
   - [Use Customer Dashboard](#how-to-use-customer-dashboard)
   - [Use Pickup Dashboard](#how-to-use-pickup-dashboard)
   - [De-brief Courier (End of Day)](#how-to-de-brief-courier-end-of-day)
   - [Reconcile Courier Receipts](#how-to-reconcile-courier-receipts)

1. [Operations Flow](#operations-flow)
   - [Pickup Management](#pickup-management)
   - [Inscan (Warehouse Receiving)](#inscan-warehouse-receiving)
   - [AWB Entry & Shipments](#awb-entry--shipments)
   - [Process Manifest (MAWB)](#process-manifest-mawb)
   - [Import Module (Air/Sea/Land)](#import-module-airsealand)
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

# How To Guides

Step-by-step tutorials for common tasks in Net4Courier. These guides help you get started quickly with essential operations.

---

## How to Create a New Company

**Navigation:** System Settings > Companies

**Steps:**
1. Click **"New Company"** button in the top right
2. Fill in the required company details:
   - **Company Name** - Official registered name
   - **Company Code** - Short code (e.g., "NET4")
   - **Registration Number** - Business registration ID
   - **Tax Number** - VAT/Tax registration number
   - **Address** - Complete business address
   - **Country/State/City** - Select from dropdowns
   - **Contact Details** - Phone, Email, Website
3. Optionally upload **Company Logo** (recommended size: 200x80px)
4. Click **"Save"** to create the company

> **Note:** You need at least one company before creating branches and users.

---

## How to Create a New Branch

**Navigation:** System Settings > Branches

**Steps:**
1. Click **"New Branch"** button
2. Select the **Parent Company** from dropdown
3. Fill in branch details:
   - **Branch Name** - E.g., "Dubai Main Office"
   - **Branch Code** - Short code (e.g., "DXB")
   - **Currency Code** - E.g., "AED"
   - **Currency Symbol** - E.g., "AED"
   - **Address Details** - Complete branch address
   - **Manager Name** and contact information
4. Configure **AWB Settings**:
   - **AWB Prefix** - Unique prefix for shipment numbers (e.g., "DXB")
   - **Starting Number** - First AWB number to use
   - **Increment** - Usually 1
5. Set **VAT Percentage** if applicable
6. Toggle **Is Head Office** if this is the main branch
7. Click **"Save"**

> **Tip:** Each branch can have its own AWB numbering sequence.

---

## How to Create a New Warehouse

**Navigation:** System Settings > Warehouses

**Steps:**
1. Click **"New Warehouse"** button
2. Select the **Branch** this warehouse belongs to
3. Enter warehouse details:
   - **Warehouse Name** - E.g., "Main Sorting Hub"
   - **Warehouse Code** - Short code (e.g., "WH01")
   - **Capacity** - Storage capacity in units
   - **Address** - Physical location
   - **Contact Person** and contact details
4. Set the warehouse status (Active/Inactive)
5. Click **"Save"**

> **Note:** Warehouses are used for inscan and sorting operations.

---

## How to Create a New Shipment

**Navigation:** Operations > AWB Entry

**Steps:**
1. Click **"New AWB"** or use the AWB Entry page
2. **Shipper Details** (Consignor):
   - Select existing customer or enter new shipper details
   - Fill in name, address, phone, and email
3. **Receiver Details** (Consignee):
   - Enter complete receiver name and address
   - Include mobile number for delivery notifications
4. **Shipment Information**:
   - **Product Type** - Document, Parcel, Heavy Cargo
   - **Service Type** - Express, Standard, Economy
   - **Pieces** - Number of packages
   - **Weight** - Actual weight in KG
   - **Dimensions** - L x W x H for volumetric weight
5. **Payment & Charges**:
   - **Payment Mode** - Prepaid, Collect, Third Party
   - System calculates charges based on rate card
   - Add any special charges if applicable
6. Click **"Save"** to generate AWB number
7. Print the **AWB Label** for the shipment

> **Important:** For COD shipments, enter the collection amount in the COD field.

---

## How to Create a New Pickup Request by Staff

**Navigation:** Operations > Pickup Management

**Steps:**
1. Click **"New Pickup"** button
2. **Customer Selection**:
   - Search and select existing customer, OR
   - Enter new customer details manually
3. **Pickup Details**:
   - **Pickup Date** - When to collect
   - **Time Slot** - Morning/Afternoon/Evening
   - **Pickup Address** - Verify address is correct
   - **Contact Person** - Name and phone number
4. **Shipment Estimate**:
   - **Expected Pieces** - Approximate count
   - **Estimated Weight** - Approximate total weight
   - **Special Instructions** - Handling notes
5. Click **"Create Pickup Request"**
6. System assigns a **Pickup Request Number**
7. Assign to a courier for collection from the dispatch screen

---

## How to Create a New Pickup Request by Customers

**Navigation:** Customer Portal (for logged-in customers)

**For Staff Assisting Customers:**
1. Navigate to **Pickup Management**
2. Click **"New Pickup"**
3. Select **Customer** from the customer dropdown
4. The system pre-fills customer address
5. Fill in pickup date, time slot, and expected pieces
6. Submit the request
7. Customer receives confirmation via SMS/Email

**Self-Service Portal Features:**
- Customers can log in to their portal
- Click "Request Pickup" on their dashboard
- Fill in shipment details and preferred time
- Track request status in real-time
- View pickup history

---

## How to Give Access to the System for Agents, Customers, Vendors

**Navigation:** Masters > Parties + System Settings > Users

**Step 1 - Create the Party Record:**
1. Go to **Masters > Parties**
2. Click **"New Party"**
3. Select **Party Type**:
   - **Agent** - For delivery partners
   - **Customer** - For shipping customers
   - **Vendor** - For service providers
4. Fill in party details (name, contact, address)
5. Save the party record

**Step 2 - Create User Account:**
1. Go to **System Settings > Users**
2. Click **"New User"**
3. Fill in:
   - **Username** - Login ID
   - **Password** - Secure password
   - **Full Name** - Display name
   - **Email** and **Phone**
   - **User Type** - Agent/Customer/Vendor
   - **Role** - Assign appropriate role
4. **Assign Branches** - Select which branches they can access
5. Click **"Save"**

> **Security Tip:** Each user type has role-based menu restrictions by default.

---

## How to Create New Users

**Navigation:** System Settings > Users

**Steps:**
1. Click **"New User"** button
2. Enter **Account Details**:
   - **Username** - Unique login name
   - **Password** - Strong password (min 8 characters)
   - **Confirm Password**
3. Enter **Personal Information**:
   - **Full Name**
   - **Email Address**
   - **Phone Number**
4. **Access Configuration**:
   - **User Type** - Staff, Agent, Customer, Vendor
   - **Role** - Select from available roles (Admin, Manager, Operator, etc.)
   - **Default Branch** - Primary working branch
5. **Branch Assignments**:
   - Select all branches this user can access
   - Set one branch as default
6. Toggle **Is Active** to enable/disable the account
7. Click **"Save"**

---

## How to Give Access to Menu for Users

**Navigation:** System Settings > Roles

**Steps:**
1. Go to **Roles Management**
2. Select the **Role** you want to modify (e.g., "Operations Manager")
3. Click **"Edit Permissions"**
4. The system displays all menu items grouped by module:
   - **Operations** - AWB, Pickup, DRS, POD
   - **Masters** - Parties, Products, Services
   - **Finance** - Invoices, Receipts, Journals
   - **Reports** - All report types
   - **Settings** - System configuration
5. **Check** the boxes for menus this role should access
6. For each menu, set permission level:
   - **View** - Read-only access
   - **Create** - Can add new records
   - **Edit** - Can modify records
   - **Delete** - Can remove records
7. Click **"Save Permissions"**

> **Tip:** Create role templates for common job functions.

---

## How to Restrict Menu Access to Users

**Navigation:** System Settings > Roles

**Steps:**
1. Go to **Roles Management**
2. Either create a **new restricted role** or modify existing role
3. Click **"Edit Permissions"**
4. **Uncheck** menus that should NOT be accessible
5. For partial access:
   - Keep **View** checked but uncheck **Create/Edit/Delete**
   - User can see data but cannot modify
6. Save the role permissions
7. **Assign users to this role** in User Management:
   - Go to Users > Edit User
   - Change Role dropdown to the restricted role
   - Save

**Common Restrictions:**
- **Courier Role:** Only DRS, POD, and tracking access
- **Customer Role:** Only shipment tracking and pickup requests
- **Finance Clerk:** Only invoice viewing, no approval

---

## How to Process Import Customs Clearance

**Navigation:** Operations > Import > Customs Clearance

**Steps:**
1. Go to **Import Dashboard** and find the import awaiting clearance
2. Click on the import to open details
3. Navigate to **Customs Clearance** tab or click **"Process Customs"**
4. For each shipment in the import:
   - Verify **Customs Value** and **HS Code**
   - Enter **Duty Amount** if applicable
   - Select **Clearance Status**:
     - Cleared
     - Held for Inspection
     - Duty Pending
     - Rejected
5. For bulk processing:
   - Select multiple shipments using checkboxes
   - Click **"Bulk Customs Update"**
   - Apply same status to all selected items
6. Upload any **customs documents** if required
7. Click **"Save Clearance Status"**
8. Cleared shipments move to the next stage for delivery

---

## How to Create Import by Uploading Excel File

**Navigation:** Operations > Import > Excel Upload

**Steps:**
1. Click **"Download Template"** button to get the Excel template
2. Fill in the template with import data:
   - **Sheet 1: Header** - MAWB/BL number, origin, carrier, etc.
   - **Sheet 2: Shipments** - Individual AWB details
3. Select the **Import Mode**:
   - **Air** - For air freight
   - **Sea** - For sea cargo
   - **Land** - For road transport
4. Click **"Upload Excel"** and select your file
5. System **validates** the data:
   - Checks required fields
   - Validates AWB format
   - Detects duplicates
6. Review the **Preview** showing:
   - Valid rows in green
   - Errors in red with explanations
7. Fix any errors by editing the Excel and re-uploading
8. Click **"Import"** to process valid records
9. View imported records in the Import Dashboard

> **Template Columns:** AWB No, Shipper, Consignee, Pieces, Weight, Description, etc.

---

## How to Update Bulk POD

**Navigation:** Operations > POD > Excel Upload

**Steps:**
1. Click **"Download Template"** to get the POD update template
   - Or click **"Template with AWBs"** to pre-fill pending deliveries
2. Fill in the Excel template:
   - **AWB No** - Shipment number
   - **Delivery Status** - Delivered, Not Delivered, Partial
   - **Delivery Date** - Date of delivery
   - **Received By** - Name of person who received
   - **Relation** - Relation to consignee (Self, Relative, Guard, etc.)
   - **Non-Delivery Reason** - If not delivered
   - **Remarks** - Additional notes
3. Save the Excel file
4. Click **"Upload"** and select your file
5. System validates each row:
   - AWB exists and is out for delivery
   - Required fields are filled
   - Status is valid
6. Review validation results
7. Click **"Process POD Updates"**
8. Download the **Results Report** showing success/failure for each AWB

> **Alternative:** Use **Bulk POD Update** at `/pod-bulk` for grid-based updates.

---

## How to Use Customer Dashboard

**Navigation:** CRM > Customer Dashboard

**Features Overview:**
1. **Summary Cards** - Quick stats for selected customer:
   - Total Shipments
   - Pending Deliveries
   - Outstanding Balance
   - This Month's Volume
2. **Search Customer** - Find by name, code, or phone
3. **Recent Shipments** - Latest 10 shipments with status
4. **Financial Summary**:
   - Invoices pending payment
   - Payment history
   - Credit limit usage
5. **Quick Actions**:
   - Create new pickup request
   - View shipment history
   - Generate statement

**How to Use:**
1. Enter customer name or code in search box
2. Dashboard loads customer's complete profile
3. Click on any shipment to track details
4. Use action buttons for common operations
5. Export data for analysis if needed

---

## How to Use Pickup Dashboard

**Navigation:** Operations > Pickup Dashboard

**Dashboard Components:**
1. **Summary Cards**:
   - Total Requests Today
   - Pending Assignment
   - In Progress
   - Completed Today
2. **Filters**:
   - Date range
   - Status filter
   - Customer filter
   - Courier filter
3. **Request List**:
   - Pickup number, customer, time slot
   - Assigned courier
   - Current status with color coding

**How to Use:**
1. Review pending pickups at the start of day
2. **Assign Couriers**:
   - Select unassigned pickups
   - Click "Assign"
   - Choose available courier
3. **Track Progress**:
   - Monitor status updates in real-time
   - View collected vs pending count
4. **Handle Issues**:
   - Click on a pickup to see details
   - Reschedule if needed
   - Add notes or special instructions

---

## How to De-brief Courier (End of Day)

**Navigation:** Operations > DRS Reconciliation

**Purpose:** Close out a courier's day by reconciling all assigned shipments.

**Steps:**
1. Go to **DRS Reconciliation** page
2. Select the **Courier** from dropdown
3. Select the **DRS Date** (usually today)
4. Click **"Load DRS"** to see all assigned shipments
5. **Reconcile Each Shipment**:
   - **Delivered** - Confirm POD is captured
   - **Not Delivered** - Select reason, schedule retry
   - **Return to Hub** - Mark for reattempt
6. **COD Collection**:
   - View total COD to be collected
   - Enter actual amount collected
   - Note any discrepancies
7. **Expenses** (if any):
   - Enter fuel, tolls, or other expenses
   - Attach receipts if required
8. Review the **Summary**:
   - Deliveries: Completed / Total
   - COD: Collected / Expected
9. Click **"Submit Day-End"** to close the DRS
10. Print or save the **Day-End Report** for records

---

## How to Reconcile Courier Receipts

**Navigation:** Finance > Courier Receipts

**Purpose:** Match COD collected by couriers against expected amounts.

**Steps:**
1. Go to **Courier Receipts** page
2. Select **Date Range** and **Courier**
3. Click **"Load Collections"**
4. The system displays:
   - All COD shipments delivered by this courier
   - Expected collection amount
   - Actual collected (from POD/DRS)
5. **Verify Amounts**:
   - Check if collected matches expected
   - Note any shortfalls or excess
6. **Record Cash Handover**:
   - Enter amount received from courier
   - Select payment method (Cash/Cheque/Transfer)
   - Add reference number if applicable
7. **Handle Discrepancies**:
   - Short collection: Create debit note to courier
   - Excess collection: Create credit note
   - Mark disputed items for investigation
8. Click **"Confirm Receipt"** to finalize
9. System updates:
   - Courier ledger
   - Cash/Bank account
   - Shipment payment status

> **Report:** Generate Courier Collection Report for audit purposes.

---

## Suggest a New How-To Topic

**Have a question that's not covered here?**

We're constantly improving this knowledge base. If you need help with something not listed above, please:

1. **Contact Support** - Email support@net4courier.com with your question
2. **Describe the Task** - Tell us what you're trying to accomplish
3. **Include Screenshots** - If you're stuck on a specific screen

Your suggestions help us improve the system for everyone. Common requests will be added to this guide in future updates.

**Frequently Requested Topics Coming Soon:**
- How to configure rate cards for specific customers
- How to set up automated notifications
- How to generate financial reports
- How to manage multi-currency transactions

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

## Import Module (Air/Sea/Land)

**Navigation:** Import Operations → Import Dashboard

**Purpose:** Manage incoming shipments from international or domestic sources via air, sea, or land transport.

### Import Modes

| Mode | Identifier | Use Case |
|------|------------|----------|
| **Air** | MAWB (Master Airwaybill) | International air freight |
| **Sea** | BL (Bill of Lading) | Ocean cargo |
| **Land** | Truck/Vehicle Number | Road transport |

### Import Dashboard (`/import-dashboard`)

The dashboard provides:
- **Summary Cards**: Total imports, pending customs, cleared, in transit
- **Filters**: Date range, mode, status, origin
- **Import List**: All imports with key details and actions

### Create New Import (`/import-entry`)

**Step 1: Select Import Mode**
- Choose Air, Sea, or Land

**Step 2: Enter Header Details**

| Field | Air Mode | Sea Mode | Land Mode |
|-------|----------|----------|-----------|
| Reference | MAWB Number | Bill of Lading | Truck Number |
| Carrier | Airline Name | Shipping Line | Transport Company |
| Origin | Origin Airport | Port of Loading | Origin City |
| Destination | Dest. Airport | Port of Discharge | Destination City |
| ETA | Flight ETA | Vessel ETA | Truck ETA |

**Step 3: Add Shipments**
- Enter individual AWB/shipment details
- Or use Excel Import for bulk entry

### Customs Processing (`/import-customs`)

**Purpose:** Process customs clearance for import shipments.

**Workflow:**
1. Select import awaiting clearance
2. Review shipment details (HS Code, Value)
3. For each shipment, set:
   - **Customs Status**: Cleared, Held, Duty Pending, Rejected
   - **Duty Amount**: If applicable
   - **Clearance Date**: When cleared
4. Upload supporting documents if required
5. Bulk update available for multiple shipments

**Clearance Statuses:**

| Status | Description |
|--------|-------------|
| Pending | Awaiting customs processing |
| Cleared | Approved and released |
| Held for Inspection | Physical inspection required |
| Duty Pending | Payment awaited |
| Rejected | Not cleared, return required |

### Excel Import (`/import-excel-upload`)

**Purpose:** Bulk import creation via Excel upload.

**Template Structure:**
- **Sheet 1: Header** - Import metadata (MAWB/BL, carrier, dates)
- **Sheet 2: Shipments** - Individual AWB details

**Workflow:**
1. Click **"Download Template"** (mode-specific)
2. Fill in header and shipment details
3. Click **"Upload"** and select file
4. Review validation:
   - Required fields check
   - Positive values validation
   - Duplicate AWB detection
5. Preview valid/error rows
6. Click **"Import"** to process
7. System creates import with all shipments

**Template Columns (Shipments):**
- AWB Number, Shipper Name, Consignee Name
- Pieces, Weight, Description
- Customs Value, HS Code
- Origin, Destination

### Import Statuses

| Status | Description |
|--------|-------------|
| Created | Import header created |
| Arrived | Shipment arrived at destination |
| In Customs | Undergoing customs processing |
| Cleared | All customs formalities complete |
| Released | Ready for local delivery |
| Completed | All shipments delivered |

### Keywords
import, air import, sea import, land import, MAWB, bill of lading, customs, clearance, duty, HS code, excel import, bulk import, carrier, freight, cargo, customs processing, import dashboard

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

### Bulk POD Update

**Navigation:** Operations → POD → Bulk Update (`/pod-bulk`)

For updating multiple PODs at once using a grid interface:
1. Filter by date range, DRS number, or courier
2. Select multiple AWBs using checkboxes
3. Apply status update to all selected
4. Confirm and submit

### POD Excel Batch Upload

**Navigation:** Operations → POD → Excel Upload (`/pod-excel-upload`)

For processing large batches of POD updates via Excel:

**Step 1: Download Template**
- Click **"Download Template"** for blank template, OR
- Click **"Template with AWBs"** to pre-populate with pending deliveries

**Step 2: Fill Template**
The Excel template includes these columns:

| Column | Required | Description |
|--------|----------|-------------|
| AWB No | Yes | Shipment number |
| Delivery Status | Yes | Delivered, Not Delivered, Partial, Refused |
| Delivery Date | Yes | Date of delivery (YYYY-MM-DD) |
| Received By | For Delivered | Name of person who received |
| Relation | Optional | Self, Relative, Guard, Colleague, etc. |
| Non-Delivery Reason | For Not Delivered | Address Wrong, Customer Unavailable, etc. |
| Remarks | Optional | Additional notes |

**Step 3: Upload and Validate**
- Click **"Upload"** and select your filled Excel file
- System validates each row:
  - AWB exists in system
  - Shipment is eligible for POD (out for delivery)
  - Required fields are filled
  - Status value is valid
- Preview shows valid rows (green) and errors (red)

**Step 4: Process Updates**
- Click **"Process POD Updates"** to apply changes
- System updates:
  - POD status in InscanMaster
  - AWB Tracking history
  - Shipment timeline
- Download **Results Report** showing success/failure per AWB

### Keywords
POD, proof of delivery, delivery confirmation, signature, photo evidence, GPS, location, COD collection, delivered, refused, not delivered, mobile capture, delivery status, bulk POD, excel upload, batch update, POD template

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

**Navigation:** System Settings → Branches

**Purpose:** Manage branch/location master data with multi-branch support.

### Branch Details
- **Branch Code** - Used in AWB prefix for unique numbering
- **Branch Name** - Display name (e.g., "Dubai Main Office")
- **Currency Code/Symbol** - Branch-specific currency (e.g., AED, USD)
- **Address** - Complete physical address with city, state, country
- **Manager Name** - Branch manager contact
- **Linked Company** - Parent company relationship
- **VAT Percentage** - Tax rate for this branch
- **Is Head Office** - Flag for main branch

### AWB Number Configuration
Each branch can have its own AWB numbering sequence:
- **AWB Prefix** - Unique prefix (e.g., "DXB", "SHJ")
- **Starting Number** - First AWB number to use
- **Increment** - Step value (typically 1)
- **Last Used Number** - Tracks current sequence

### Multi-Branch User Access
- Users can be assigned to **multiple branches** via User-Branch assignments
- Each assignment has an **IsDefault** flag for primary branch
- **Branch-Restricted Login**: Users with multiple branches see a branch selection dropdown after entering credentials
- Dashboard header displays: **Company Name | Branch Name | User Name**
- All operations are scoped to the user's selected branch

### Warehouse Management
Each branch can have multiple warehouses at `/warehouses`:
- **Warehouse Code/Name** - Identification
- **Capacity** - Storage capacity in units
- **Address** - Physical location
- **Contact Person** - Warehouse manager
- **Status** - Active/Inactive

### Keywords
branch, location, office, hub, warehouse, branch master, currency, multi-branch, user branch, branch assignment, AWB prefix

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
