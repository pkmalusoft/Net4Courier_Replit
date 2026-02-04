# Net4Courier Knowledge Base

## Complete Operations & System Guide

This comprehensive knowledge base covers all aspects of Net4Courier - from pickup to delivery, finance to CRM. Use your browser's search (Ctrl+F / Cmd+F) to find specific topics.

---

# Table of Contents

0. [How To Guides](#how-to-guides) - Step-by-step tutorials organized by category
   
   **Quick Start**
   - [Quick Start Guide](#quick-start-guide)
   
   **System Setup Guides**
   - [Create a New Company](#how-to-create-a-new-company)
   - [Create a New Branch](#how-to-create-a-new-branch)
   - [Configure Branch AWB Numbers by Movement Type](#how-to-configure-branch-awb-numbers-by-movement-type)
   - [Create a New Warehouse](#how-to-create-a-new-warehouse)
   - [Create New Users](#how-to-create-new-users)
   - [Create Users and Assign Roles & Permissions](#how-to-create-users-and-assign-roles-permissions)
   - [Give Menu Access to Users](#how-to-give-access-to-menu-for-users)
   - [Restrict Menu Access](#how-to-restrict-menu-access-to-users)
   - [Give Access for Agents/Customers/Vendors](#how-to-give-access-for-agents-customers-vendors)
   - [Configure Service Types](#how-to-configure-service-types)
   - [Set Up Zones (International/Domestic)](#how-to-set-up-zones)
   - [Manage Currencies](#how-to-manage-currencies)
   - [Manage Departments](#how-to-manage-departments)
   - [Manage Designations](#how-to-manage-designations)
   - [Set Up Initial Administrator (First-Time Setup)](#how-to-set-up-initial-administrator)
   - [Platform Administration (Tenant/Subscription Management)](#platform-administration-platform-admin-only)
   - [Create and Delete Demo Data](#how-to-create-and-delete-demo-data)
   - [Delete All Business Data](#delete-all-business-data-platform-admin-only)
   
   **Operations Guides**
   - [Create a New Shipment (AWB)](#how-to-create-a-new-shipment-awb-entry)
   - [Create Pickup Request (Staff)](#how-to-create-a-pickup-request-staff)
   - [Create Pickup Request (Customer)](#how-to-create-a-pickup-request-customer-self-service)
   - [Add Shipment Lines to Pickup](#how-to-add-shipment-lines-to-a-pickup-request)
   - [Convert Pickup Request to AWB](#how-to-convert-pickup-request-to-awb)
   - [Use City Selection (Geography Dropdown)](#how-to-use-city-selection-geography-dropdown)
   - [Process Inscan](#how-to-process-inscan-warehouse-receiving)
   - [Use Unified Warehouse Inscan (Domestic + Import)](#how-to-use-unified-warehouse-inscan)
   - [Create MAWB and Bag Shipments](#how-to-create-mawb-and-bag-shipments)
   - [Create DRS (Delivery Run Sheet)](#how-to-create-drs-delivery-run-sheet)
   - [Capture POD (Proof of Delivery)](#how-to-capture-pod-proof-of-delivery)
   - [Update Bulk POD via Excel](#how-to-update-bulk-pod-via-excel)
   - [Process Return to Shipper (RTS)](#how-to-process-return-to-shipper-rts)
   - [Track a Shipment](#how-to-track-a-shipment)
   - [Use Global Search](#how-to-use-global-search)
   - [Print Tracking Report (PDF)](#how-to-print-tracking-report)
   - [Generate Shipment Invoice](#how-to-generate-shipment-invoice)
   
   **Import Operations Guides**
   - [Create Import (Air/Sea/Land)](#how-to-create-a-new-import-airsealand)
   - [Process Import Customs Clearance](#how-to-process-import-customs-clearance)
   - [Create Import via Excel Upload](#how-to-create-import-via-excel-upload)
   - [Import Shipment Fields (Shipper, Duty/VAT, COD)](#import-shipment-data-fields)
   - [Receive Import Shipments at Warehouse](#how-to-receive-import-shipments-at-warehouse)
   
   **Prepaid AWB Guides**
   - [Manage AWB Stock](#how-to-manage-awb-stock)
   - [Sell Prepaid AWBs to Customers](#how-to-sell-prepaid-awbs)
   - [Use Prepaid AWB in Shipment Entry](#how-to-use-prepaid-awb-in-shipment)
   - [View Prepaid AWB Register](#how-to-view-prepaid-awb-register)
   
   **Dashboard & Reporting Guides**
   - [Use Customer Dashboard](#how-to-use-customer-dashboard)
   - [Use Pickup Dashboard](#how-to-use-pickup-dashboard)
   - [De-brief Courier (End of Day)](#how-to-de-brief-courier-end-of-day)
   - [Reconcile Courier Cash/COD](#how-to-reconcile-courier-cashcod)
   
   **Finance Guides**
   - [Create an Invoice](#how-to-create-an-invoice)
   - [Record Customer Payment](#how-to-record-customer-payment-receipt)
   - [Set Up Rate Cards](#how-to-set-up-rate-cards)
   - [Use Rate Simulator](#how-to-use-rate-simulator)
   - [Generate Reports](#how-to-generate-reports)
   - [Use Public Tracking Page](#how-to-use-public-tracking-page)
   - [Use General Ledger Module](#how-to-use-general-ledger)
   - [Email Reports to Customers/Suppliers](#how-to-email-reports)
   
   **Help & Feedback**
   - [Suggest a New Topic](#suggest-a-new-how-to-topic)
   - [Quick Reference Table](#how-to-guides---quick-reference)

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

Step-by-step tutorials for common tasks in Net4Courier. These guides help you get started quickly with essential operations. Guides are organized into categories for easy reference.

---

# Quick Start Guide

New to Net4Courier? Follow these steps to set up your system:

1. **Create Company** → Set up your organization profile
2. **Create Branch(es)** → Define your operational locations
3. **Create Warehouses** → Set up sorting hubs and storage
4. **Add Users** → Create staff accounts and assign roles
5. **Configure Rate Cards** → Set up pricing for services
6. **Start Operations** → Begin processing shipments

---

# System Setup Guides

## How to Create a New Company

**Navigation:** System Settings → Companies → Click "New Company"

**When to Use:** When setting up Net4Courier for the first time or adding a new legal entity.

**Prerequisites:** Administrator access

**Detailed Steps:**

1. **Access the Company Setup Page**
   - From the left menu, click **System Settings**
   - Select **Companies**
   - Click the **"+ New Company"** button (top right corner)

2. **Enter Basic Information**
   | Field | Description | Example |
   |-------|-------------|---------|
   | Company Name | Official registered business name | "Express Logistics LLC" |
   | Company Code | Short unique identifier (2-10 chars) | "EXLOG" |
   | Registration Number | Business license/registration ID | "DED-123456" |
   | Tax Number | VAT/GST registration number | "TRN100234567890" |

3. **Enter Contact Information**
   | Field | Description | Example |
   |-------|-------------|---------|
   | Email | Primary business email | "info@expresslogistics.ae" |
   | Phone | Main contact number | "+971 4 123 4567" |
   | Website | Company website (optional) | "www.expresslogistics.ae" |

4. **Enter Address Details**
   - **Street Address** - Building name, street
   - **City** - Select from dropdown
   - **State/Emirate** - Select from dropdown
   - **Country** - Select from dropdown
   - **Postal Code** - ZIP/PIN code

5. **Upload Company Logo** (Optional but recommended)
   - Click **"Upload Logo"**
   - Select image file (PNG, JPG)
   - Recommended size: 200x80 pixels
   - Logo appears on reports, invoices, and AWB labels

6. **Save the Company**
   - Review all entered information
   - Click **"Save"** button
   - Success message confirms creation

**What Happens Next:**
- Company is created with status "Active"
- You can now create branches under this company
- Company appears in company dropdown across the system

**Common Issues:**
| Problem | Solution |
|---------|----------|
| "Company code already exists" | Use a different unique code |
| Logo not uploading | Check file size (<2MB) and format (PNG/JPG) |
| Required field missing | Fill all fields marked with * |

> **Important:** You must create at least one company before creating branches, users, or processing any transactions.

---

## How to Create a New Branch

**Navigation:** System Settings → Branches → Click "New Branch"

**When to Use:** When setting up a new operational location, office, or franchise.

**Prerequisites:** At least one company must exist.

**Detailed Steps:**

1. **Access the Branch Setup Page**
   - From the left menu, click **System Settings**
   - Select **Branches**
   - Click the **"+ New Branch"** button

2. **Select Parent Company**
   - Choose the company this branch belongs to from the dropdown
   - All settings and reports will link to this company

3. **Enter Branch Information**
   | Field | Description | Example |
   |-------|-------------|---------|
   | Branch Name | Descriptive location name | "Dubai Main Office" |
   | Branch Code | Short unique code (3-6 chars) | "DXB" |
   | Manager Name | Branch manager's name | "Ahmed Al Rashid" |
   | Manager Phone | Manager's contact | "+971 50 123 4567" |
   | Manager Email | Manager's email | "ahmed@company.ae" |

4. **Configure Currency Settings**
   | Field | Description | Example |
   |-------|-------------|---------|
   | Currency Code | ISO currency code | "AED" |
   | Currency Symbol | Display symbol | "AED" or "د.إ" |
   | VAT Percentage | Default tax rate | 5.00 |

5. **Configure AWB Numbering**
   | Field | Description | Example |
   |-------|-------------|---------|
   | AWB Prefix | Prefix for all AWB numbers | "DXB" |
   | Starting Number | First AWB number | 100001 |
   | Increment | Number increment (usually 1) | 1 |

   > **Example:** With prefix "DXB", starting 100001, first AWB will be "DXB100001"

6. **Enter Branch Address**
   - Complete street address
   - City, State, Country selection
   - Postal code

7. **Set Branch Options**
   - **Is Head Office** - Toggle ON for main branch
   - **Is Active** - Toggle ON to enable operations

8. **Save the Branch**
   - Click **"Save"** button
   - Branch is now available for user assignments and operations

**After Creating a Branch:**
- Create warehouses under this branch
- Assign users to the branch
- Set up branch-specific rate cards if needed

**Multi-Branch Tips:**
- Each branch maintains its own AWB sequence
- Users can be assigned to multiple branches
- Financial reports can be filtered by branch
- Inventory is tracked per warehouse per branch

---

## How to Create a New Warehouse

**Navigation:** System Settings → Warehouses → Click "New Warehouse"

**When to Use:** When adding a new storage location, sorting hub, or collection point.

**Prerequisites:** At least one branch must exist.

**Detailed Steps:**

1. **Access Warehouse Setup**
   - Navigate to **System Settings → Warehouses**
   - Click **"+ New Warehouse"** button

2. **Select Parent Branch**
   - Choose the branch this warehouse belongs to
   - Warehouses inherit branch currency and settings

3. **Enter Warehouse Details**
   | Field | Description | Example |
   |-------|-------------|---------|
   | Warehouse Name | Descriptive name | "Main Sorting Hub" |
   | Warehouse Code | Short unique code | "WH-DXB-01" |
   | Capacity | Storage capacity (units) | 10000 |
   | Warehouse Type | Select type | Sorting Hub / Storage / Collection Point |

4. **Enter Address & Contact**
   - **Address** - Complete physical location
   - **Contact Person** - Warehouse supervisor name
   - **Contact Phone** - Direct contact number
   - **Contact Email** - Warehouse email

5. **Configure Settings**
   - **Is Active** - Enable/disable warehouse
   - **Operating Hours** - Start and end times (optional)

6. **Save the Warehouse**
   - Click **"Save"**
   - Warehouse appears in inscan location dropdown

**Using Warehouses:**
- Select warehouse during Inscan operations
- Track inventory per warehouse
- Generate warehouse-specific reports
- Manage capacity utilization

---

## How to Create New Users

**Navigation:** System Settings → Users → Click "New User"

**When to Use:** When adding staff members, agents, or external user accounts.

**Prerequisites:** Company and at least one branch must exist.

**Detailed Steps:**

1. **Access User Management**
   - Navigate to **System Settings → Users**
   - Click **"+ New User"** button

2. **Enter Login Credentials**
   | Field | Description | Example |
   |-------|-------------|---------|
   | Username | Unique login ID | "ahmed.rashid" |
   | Password | Secure password (min 8 chars) | "SecurePass123!" |
   | Confirm Password | Re-enter password | "SecurePass123!" |

   > **Password Requirements:** Minimum 8 characters, include uppercase, lowercase, and numbers.

3. **Enter Personal Information**
   | Field | Description | Example |
   |-------|-------------|---------|
   | Full Name | Display name | "Ahmed Al Rashid" |
   | Email | User's email | "ahmed@company.ae" |
   | Phone | Contact number | "+971 50 123 4567" |
   | Employee ID | Internal ID (optional) | "EMP-001" |

4. **Select User Type**
   | Type | Description |
   |------|-------------|
   | Staff | Internal employees |
   | Agent | Delivery agents/couriers |
   | Customer | External customer accounts |
   | Vendor | Supplier/partner accounts |

5. **Assign Role**
   - Select from available roles:
     - **Administrator** - Full system access
     - **Manager** - Operations and reports
     - **Operator** - Day-to-day operations
     - **Finance** - Billing and payments
     - **Courier** - Delivery operations only
     - **Read Only** - View access only

6. **Assign Branches**
   - **Check** all branches this user can access
   - **Set Default Branch** - Primary working location
   - User can switch between assigned branches during login

7. **Set Account Status**
   - **Is Active** - Enable to allow login
   - **Force Password Change** - Require new password on first login

8. **Save the User**
   - Click **"Save"**
   - User can now log in with provided credentials

**User Login Process:**
- User enters username and password
- If assigned to multiple branches, selects branch from dropdown
- Dashboard loads with branch-specific data

---

## How to Give Access to Menu for Users

**Navigation:** System Settings → Roles → Select Role → Edit Permissions

**When to Use:** When customizing what menus and features a user role can access.

**Detailed Steps:**

1. **Access Role Management**
   - Navigate to **System Settings → Roles**
   - Find the role you want to modify
   - Click **"Edit"** or **"Manage Permissions"**

2. **View Permission Categories**
   Permissions are organized by module:
   | Module | Contains |
   |--------|----------|
   | Operations | AWB Entry, Pickup, DRS, POD, MAWB |
   | Masters | Parties, Products, Service Types |
   | Finance | Invoices, Receipts, Journals, Ledger |
   | CRM | Customer Dashboard, Contracts, Tickets |
   | Reports | All report types |
   | Settings | Company, Branch, User, Role setup |

3. **Set Menu Permissions**
   For each menu item, configure:
   | Permission | Description |
   |------------|-------------|
   | **View** | Can see the menu and read data |
   | **Create** | Can add new records |
   | **Edit** | Can modify existing records |
   | **Delete** | Can remove records |

4. **Apply Permissions**
   - Check/uncheck boxes as needed
   - Click **"Save Permissions"**
   - Changes apply immediately to all users with this role

**Example Role Configurations:**

**Operations Manager:**
- Full access to Operations module
- View access to Finance module
- No access to Settings module

**Courier/Delivery Agent:**
- View + Update: DRS, POD
- View only: AWB lookup, Tracking
- No access: Masters, Finance, Settings

**Finance Clerk:**
- Full access: Invoices, Receipts
- View only: Ledger, Reports
- No access: Operations, Settings

---

## How to Restrict Menu Access to Users

**Navigation:** System Settings → Roles → Select Role → Edit Permissions

**When to Use:** When limiting access to sensitive features or data.

**Detailed Steps:**

1. **Decide on Restriction Approach**
   - **Create New Role** - For unique permission set
   - **Modify Existing Role** - For adjusting current access

2. **To Create a Restricted Role:**
   - Go to **System Settings → Roles**
   - Click **"+ New Role"**
   - Enter role name (e.g., "Limited Finance Access")
   - Set permissions to minimum required

3. **To Remove Menu Access:**
   - Edit the role permissions
   - **Uncheck all boxes** for menus to hide completely
   - Menu will not appear in user's navigation

4. **To Allow View-Only Access:**
   - Check only **"View"** permission
   - Uncheck **Create, Edit, Delete**
   - User can see data but cannot modify

5. **Assign Restricted Role to Users:**
   - Go to **System Settings → Users**
   - Edit the user
   - Change **Role** dropdown to the restricted role
   - Save

**Common Restriction Scenarios:**

| Scenario | Configuration |
|----------|---------------|
| Hide all finance menus | Uncheck all Finance permissions |
| Prevent data deletion | Uncheck Delete for all modules |
| Read-only access | Check only View permissions |
| COD collection only | Only POD and DRS permissions |

---

## How to Give Access for Agents, Customers, Vendors

**Navigation:** Masters → Parties + System Settings → Users

**When to Use:** When onboarding external parties who need system access.

**Detailed Steps:**

### Step 1: Create Party Record

1. Go to **Masters → Parties**
2. Click **"+ New Party"**
3. Select **Party Type:**
   | Type | Use For |
   |------|---------|
   | Agent | Delivery agents, couriers, franchisees |
   | Customer | Shipping customers, corporate clients |
   | Vendor | Service providers, suppliers |
4. Fill in party details:
   - Party Name, Code
   - Contact information
   - Address details
   - Tax registration (if applicable)
5. Click **"Save"**

### Step 2: Create User Account

1. Go to **System Settings → Users**
2. Click **"+ New User"**
3. Enter username and password
4. Set **User Type** matching the party type
5. Select appropriate **Role:**
   | Party Type | Recommended Role |
   |------------|------------------|
   | Agent | Courier / Delivery Agent |
   | Customer | Customer Portal |
   | Vendor | Vendor Access |
6. Assign branch access
7. Click **"Save"**

### Step 3: Link User to Party (if required)
- Some reports and dashboards filter by linked party
- Edit user and select related party from dropdown

**Access Levels by Type:**

| Type | Can Access |
|------|------------|
| Agent | DRS, POD capture, route assignments |
| Customer | Track shipments, request pickups, view invoices |
| Vendor | View assigned work, submit invoices |

---

## How to Configure Service Types

**Navigation:** System Settings → Service Types

**When to Use:** When adding new delivery service options.

**Detailed Steps:**

1. Go to **System Settings → Service Types**
2. Click **"+ New Service Type"**
3. Enter service details:
   | Field | Example |
   |-------|---------|
   | Service Name | "Express Next Day" |
   | Service Code | "EXP-ND" |
   | Description | "Guaranteed next business day delivery" |
   | Transit Days | 1 |
   | Is Active | Yes |
4. Click **"Save"**

**Standard Service Types:**
- Express Same Day
- Express Next Day
- Standard (2-3 days)
- Economy (4-7 days)
- International Express
- International Standard

---

## How to Set Up Zones

**Navigation:** Pricing & Billing → Zone Management

**When to Use:** When configuring geographic pricing regions for rate calculation.

**Prerequisites:** Countries and cities should be configured in Geography Masters.

**Detailed Steps:**

1. **Access Zone Management**
   - Go to **Pricing & Billing → Zone Management**
   - Click **"+ New Zone"**

2. **Select Zone Type**
   | Type | Use For | Members |
   |------|---------|---------|
   | International | Cross-border shipments | Select countries |
   | Domestic | Within-country shipments | Select cities |

3. **Enter Zone Details**
   | Field | Description | Example |
   |-------|-------------|---------|
   | Zone Name | Descriptive name | "GCC Countries" or "UAE Metro" |
   | Zone Code | Short identifier | "GCC" or "UAE-M" |
   | Zone Category | Category grouping | "International" or "Local" |
   | Description | Zone coverage details | "Gulf Cooperation Council countries" |

4. **Add Zone Members (based on type)**

   **For International Zones:**
   - Click on countries to add them as chips
   - Multiple countries can be selected
   - Countries appear as removable chips
   - Example: UAE, Saudi Arabia, Qatar, Kuwait, Bahrain, Oman

   **For Domestic Zones:**
   - Click on cities to add them as chips
   - Multiple cities can be selected
   - Cities appear as removable chips
   - Example: Dubai, Abu Dhabi, Sharjah, Ajman

5. **Save Zone**
   - Click **"Save"**
   - Zone details stored in ZoneMatrixDetails
   - Available for rate card configuration

**Sample Zone Structure:**
| Zone Type | Zone Name | Coverage |
|-----------|-----------|----------|
| Domestic | Local | Same city delivery |
| Domestic | Metro | Major cities in country |
| Domestic | Remote | Rural/remote areas |
| International | Gulf | GCC countries |
| International | Middle East | ME region |
| International | Asia | Asian countries |
| International | Europe | European countries |
| International | Americas | North/South America |

**Zone Resolution Priority:**
1. City match (most specific)
2. Country match
3. Default zone (fallback)

---

## How to Manage Currencies

**Navigation:** Masters & Settings → Operations Masters → Currencies

**When to Use:** When adding or modifying currencies for transactions and shipment values.

**Detailed Steps:**

1. **Access Currency Management**
   - Go to **Masters & Settings → Operations Masters → Currencies**
   - View existing currencies in the grid

2. **Add New Currency**
   - Click **"Add Currency"** button
   - Dialog opens for currency entry

3. **Enter Currency Details**
   | Field | Description | Example |
   |-------|-------------|---------|
   | Code | ISO 4217 currency code (3 chars) | "USD", "AED", "INR" |
   | Name | Full currency name | "US Dollar", "UAE Dirham" |
   | Symbol | Display symbol | "$", "AED", "₹" |
   | Decimal Places | Precision (0-4) | 2 |
   | Is Active | Enable/disable currency | Yes |

4. **Save Currency**
   - Click **"Save"**
   - Currency available for selection in branches and transactions

**Edit/Delete Currency:**
- Click **Edit** icon to modify existing currency
- Click **Delete** icon to soft-delete (marks as deleted, not removed)

**Branch Currency as Default:**

The currency assigned to a branch is automatically used as the default throughout the system:

| Feature | Currency Usage |
|---------|----------------|
| New AWB Entry | Default currency for shipment value |
| Import Shipments | Default currency for charges |
| Credit Limit Display | Shows currency code in numeric fields |
| AR Settings | Uses branch currency for default values |
| Rate Enquiry | Results shown in branch currency |
| Tracking Page | Falls back to branch currency if not set |
| GL Profile | Base currency initialized from branch |
| Financial Reports | Currency based on branch settings |

**Setting Branch Currency:**
1. Go to **Masters & Settings → Organization → Branches**
2. Edit the branch
3. Select **Currency** from the dropdown
4. Save changes
5. All new transactions will use this currency

**Setting Company Currency:**
1. Go to **Masters & Settings → Organization → Company**
2. Edit the company
3. Select **Currency** from the dropdown
4. Save changes

> **Tip:** Set the branch currency before creating transactions. Existing transactions retain their original currency.

**Common Currencies:**
| Code | Name | Symbol | Decimals |
|------|------|--------|----------|
| AED | UAE Dirham | AED | 2 |
| USD | US Dollar | $ | 2 |
| EUR | Euro | € | 2 |
| GBP | British Pound | £ | 2 |
| INR | Indian Rupee | ₹ | 2 |
| SAR | Saudi Riyal | SAR | 2 |

---

## How to Manage Departments

**Navigation:** Masters & Settings → Organization → Departments

**When to Use:** When organizing staff into departments for reporting and access control.

**Detailed Steps:**

1. **Access Department Management**
   - Go to **Masters & Settings → Organization → Departments**
   - View existing departments

2. **Add New Department**
   - Click **"Add Department"** button
   - Dialog opens for entry

3. **Enter Department Details**
   | Field | Description | Example |
   |-------|-------------|---------|
   | Code | Short unique code | "OPS", "FIN", "HR" |
   | Name | Full department name | "Operations", "Finance", "Human Resources" |
   | Description | Department purpose | "Handles shipment processing" |
   | Is Active | Enable/disable | Yes |

4. **Save Department**
   - Click **"Save"**
   - Department available for user assignment

**Sample Departments:**
- Operations (OPS) - Shipment handling
- Finance (FIN) - Billing and payments
- Customer Service (CS) - Customer support
- Human Resources (HR) - Employee management
- IT (IT) - Technical support
- Sales (SALES) - Business development

---

## How to Manage Designations

**Navigation:** Masters & Settings → Organization → Designations

**When to Use:** When defining job titles/positions for staff members.

**Detailed Steps:**

1. **Access Designation Management**
   - Go to **Masters & Settings → Organization → Designations**
   - View existing designations

2. **Add New Designation**
   - Click **"Add Designation"** button
   - Dialog opens for entry

3. **Enter Designation Details**
   | Field | Description | Example |
   |-------|-------------|---------|
   | Code | Short unique code | "MGR", "EXE", "DIR" |
   | Name | Full title | "Manager", "Executive", "Director" |
   | Description | Role description | "Team lead responsible for..." |
   | Is Active | Enable/disable | Yes |

4. **Save Designation**
   - Click **"Save"**
   - Designation available for user profiles

**Sample Designations:**
- Director (DIR)
- General Manager (GM)
- Manager (MGR)
- Assistant Manager (AM)
- Senior Executive (SR-EXE)
- Executive (EXE)
- Junior Executive (JR-EXE)
- Courier (COU)
- Driver (DRV)

---

## How to Set Up Initial Administrator

**Navigation:** /setup (First-time access only)

**When to Use:** When deploying Net4Courier for a new client/installation for the first time.

**Prerequisites:** 
- SETUP_KEY environment variable must be configured
- No admin user exists in the database

**Detailed Steps:**

1. **Access Setup Page**
   - Navigate to `/setup` in browser
   - If admin already exists, you'll be redirected to login
   - If no admin exists, setup form appears

2. **Enter Setup Key**
   - Enter the **SETUP_KEY** provided by system administrator
   - This authenticates the setup process
   - Prevents unauthorized admin creation

3. **Create Administrator Account**
   | Field | Description | Example |
   |-------|-------------|---------|
   | Username | Admin login ID | "admin" |
   | Password | Secure password (min 8 chars) | "SecurePass123!" |
   | Confirm Password | Re-enter password | "SecurePass123!" |
   | Full Name | Administrator name | "System Administrator" |
   | Email | Admin email | "admin@company.ae" |

4. **Complete Setup**
   - Click **"Create Administrator"**
   - Admin account created with full system access
   - Redirected to login page
   - Login with new credentials

**Security Notes:**
- Setup page only accessible when no admin exists
- SETUP_KEY should be kept confidential
- Change admin password after first login
- Create individual user accounts for staff (don't share admin)

---

## Platform Administration (Platform Admin Only)

**Navigation:** Platform Administration menu (visible only to PlatformAdmin role)

**When to Use:** For managing tenant settings, subscriptions, and system-wide administrative tasks.

**Prerequisites:** Must be logged in with the PlatformAdmin role.

### Tenant Settings

**Navigation:** Platform Administration → Tenant Settings

Manage client/tenant configuration:

| Setting | Description |
|---------|-------------|
| Company Name | Client's registered business name |
| Contact Person | Primary contact for the tenant |
| Contact Email | Main email for communications |
| Contact Phone | Primary phone number |
| Subscription Plan | Current subscription tier (Basic, Professional, Enterprise) |
| Subscription Start | Date subscription began |
| Subscription End | Date subscription expires |

**Steps:**
1. Navigate to **Platform Administration → Tenant Settings**
2. View current tenant configuration
3. Click **Edit** to modify settings
4. Update required fields
5. Click **Save**

### Subscription Management

**Navigation:** Platform Administration → Subscription Management

Monitor and manage client subscriptions:

| Feature | Description |
|---------|-------------|
| Current Plan | View active subscription tier |
| Expiry Date | Track when subscription ends |
| Usage Metrics | View AWB counts, user counts, storage usage |
| Plan Comparison | Compare features across plans |

**Actions:**
- View subscription details
- Track usage against limits
- Review subscription history

### Manage Demo Data

**Navigation:** Platform Administration → Manage Demo Data

Full demo data lifecycle management including:
- **Create Demo Data** - Generate sample data for training
- **Delete Demo Data** - Remove only demo-flagged records
- **Delete All Business Data** - Complete data reset (preserves configuration)

See [How to Create and Delete Demo Data](#how-to-create-and-delete-demo-data) for detailed instructions.

---

## How to Create and Delete Demo Data

**Navigation:** Masters & Settings → User & Security → Demo Data Management

**When to Use:** When setting up training environments or demonstrating system features.

**Prerequisites:** Administrator access required.

**Detailed Steps:**

### Creating Demo Data

1. **Access Demo Data Page**
   - Go to **Masters & Settings → User & Security → Demo Data Management**

2. **Click "Create Demo Data"**
   - Confirmation dialog appears
   - Review what will be created

3. **Demo Data Created:**
   | Type | Records | Details |
   |------|---------|---------|
   | Customers | 5 | DEMO-CUST-001 to 005 with UAE addresses |
   | AWBs | 5 | DEMO-AWB-001 to 005 with complete workflows |
   | Pickup Requests | 5 | Linked to demo AWBs |
   | Tracking Entries | Multiple | Full tracking history per AWB |
   | Inscans | 5 | Warehouse receiving records |

4. **All Demo Records**
   - Flagged with `IsDemo = true`
   - Easily identifiable in reports
   - Safe to delete without affecting real data

### Deleting Demo Data

1. **Access Demo Data Page**
   - Go to **Masters & Settings → User & Security → Demo Data Management**

2. **Click "Delete Demo Data"**
   - Confirmation dialog with warning
   - Lists count of records to be deleted

3. **Confirm Deletion**
   - Click **"Delete"** to proceed
   - All demo records removed
   - Real data unaffected

### Delete All Business Data (Platform Admin Only)

**Navigation:** Platform Administration → Manage Demo Data

**When to Use:** When resetting the system for a fresh start while preserving configuration settings. This is a destructive operation for Platform Administrators only.

**Prerequisites:** Must be logged in as Platform Administrator (role: PlatformAdmin).

**What Gets Deleted:**

| Category | Deleted Items |
|----------|---------------|
| **Master Data** | Parties (Customers, Agents, Vendors, Co-Loaders), Employees, Vehicles, Bank Accounts, AWB Stocks, Prepaid Documents, Ticket Categories |
| **Transactions** | AWBs (Inscan), Pickup Requests, Import Masters, Import Shipments, DRS, Invoices, Receipts, Journals, COD Remittances, Transfer Orders, Complaints/Tickets, Rate Cards, Zones |

**What Gets Preserved:**

| Category | Preserved Items |
|----------|-----------------|
| **System Configuration** | Company, Branch, Ports, Currency, Country/State/City/Location |
| **Organizational** | Designations, Departments, Financial Years |
| **Accounting Structure** | Chart of Accounts (Account Heads), Account Types, Account Classification |
| **Operations Config** | Service Types, Shipment Modes, Shipment Statuses |
| **Security** | Users, Roles |

**Steps to Delete All Business Data:**

1. Log in as Platform Administrator
2. Navigate to **Platform Administration → Manage Demo Data**
3. Scroll to **Delete All Business Data** section (Red warning panel)
4. Click **"Delete All Business Data"** button
5. Type confirmation text exactly: `DELETE ALL DATA`
6. Click **"Confirm Delete"**
7. Wait for deletion to complete (may take a few seconds)

> **Warning:** This action cannot be undone. All business transactions and master data will be permanently deleted. Only use this for resetting test/training environments.

**Use Cases:**
- Training new staff
- Client demonstrations
- Testing new features
- UAT environments
- Resetting system for new client deployment

---

## How to Configure Branch AWB Numbers by Movement Type

**Navigation:** Masters & Settings → Organization → Branches → Edit Branch

**When to Use:** When different AWB number series are needed for Domestic, Export, Import, and Transhipment shipments.

**Prerequisites:** Branch must exist.

**Detailed Steps:**

1. **Access Branch Edit**
   - Go to **Masters & Settings → Organization → Branches**
   - Click **Edit** on the branch to configure

2. **Navigate to AWB Configuration Section**
   - Scroll to **"AWB Configuration by Movement Type"** section
   - Grid shows 4 movement types

3. **Configure Each Movement Type**

   | Movement Type | Use For | Example Prefix |
   |---------------|---------|----------------|
   | Domestic | Within-country shipments | "DXB-D" |
   | Export | Outbound international | "DXB-E" |
   | Import | Inbound international | "DXB-I" |
   | Transhipment | Pass-through cargo | "DXB-T" |

4. **Set AWB Parameters**
   For each movement type, configure:
   | Field | Description | Example |
   |-------|-------------|---------|
   | AWB Prefix | Unique prefix for this type | "DXB-E" |
   | Starting Number | First AWB number | 100001 |
   | Increment By | Number increment (typically 1) | 1 |
   | Last Used Number | Read-only, shows current | 100045 |

5. **Save Configuration**
   - Click **"Save"**
   - Each movement type now has its own AWB series

**How It Works:**
- When creating an AWB, system checks movement type
- Uses movement-type-specific config if available
- Falls back to branch default if no specific config
- AWB numbers auto-generate based on configuration

**Example AWB Numbers:**
```
Domestic:      DXB-D100001, DXB-D100002, ...
Export:        DXB-E100001, DXB-E100002, ...
Import:        DXB-I100001, DXB-I100002, ...
Transhipment:  DXB-T100001, DXB-T100002, ...
```

**Benefits:**
- Easy identification of shipment type from AWB
- Separate numbering for accounting/reporting
- Clear audit trail by movement type
- Flexible configuration per branch

---

## How to Create Users and Assign Roles & Permissions

**Navigation:** Masters & Settings → User & Security → Users, Roles

**When to Use:** When onboarding new staff and configuring their access levels.

**Prerequisites:** At least one branch must exist.

**Detailed Steps:**

### Step 1: Create or Select a Role

1. **Access Role Management**
   - Go to **Masters & Settings → User & Security → Roles**
   - View existing roles or create new

2. **Create New Role (if needed)**
   - Click **"+ New Role"**
   - Enter role name (e.g., "Operations Manager")
   - Enter description

3. **Configure Permissions**
   For each module, set access levels:
   | Permission | Description |
   |------------|-------------|
   | View | Can see data and menus |
   | Create | Can add new records |
   | Edit | Can modify existing records |
   | Delete | Can remove records |

4. **Save Role**
   - Click **"Save"**
   - Role available for user assignment

### Step 2: Create User Account

1. **Access User Management**
   - Go to **Masters & Settings → User & Security → Users**
   - Click **"+ New User"**

2. **Enter Login Credentials**
   | Field | Description | Example |
   |-------|-------------|---------|
   | Username | Unique login ID | "ahmed.rashid" |
   | Password | Min 8 characters | "SecurePass123!" |
   | Confirm Password | Re-enter password | "SecurePass123!" |

3. **Enter Personal Information**
   | Field | Description | Example |
   |-------|-------------|---------|
   | Full Name | Display name | "Ahmed Al Rashid" |
   | Email | User's email | "ahmed@company.ae" |
   | Phone | Contact number | "+971 50 123 4567" |

4. **Assign User Type**
   | Type | Use For |
   |------|---------|
   | Staff | Internal employees |
   | Agent | Delivery couriers |
   | Customer | External customer portal |
   | Vendor | Supplier access |

5. **Assign Role**
   - Select from dropdown
   - Role determines all permissions

6. **Assign Branches**
   - Check all branches user can access
   - Set one as **Default Branch**
   - User selects branch at login if multiple

7. **Set Status**
   - **Is Active**: Enable to allow login
   - Save user

### Step 3: Verify Access

1. **User Logs In**
   - Enter username and password
   - If multiple branches, select branch

2. **Menu Access**
   - Only sees menus based on role permissions
   - Hidden menus = no permission

3. **Data Access**
   - Only sees data for assigned branches
   - Operations scoped to selected branch

**Common Role Templates:**

| Role | Permissions |
|------|-------------|
| Administrator | Full access to all modules |
| Operations Manager | Full Operations, View Finance |
| Finance Manager | Full Finance, View Operations |
| Operator | Create/Edit Operations, no Settings |
| Courier | Only DRS and POD capture |
| Customer Portal | Track shipments, view invoices |
| Read Only | View all, no create/edit/delete |

---

# Operations Guides

## How to Create a New Shipment (AWB Entry)

**Navigation:** Shipments → AWB Entry → Click "New AWB"

**When to Use:** When booking a new shipment for a customer.

**Prerequisites:** Customer must exist in the system, or enter walk-in details.

**Detailed Steps:**

1. **Access AWB Entry**
   - From left menu, click **Shipments**
   - Select **AWB Entry**
   - Click **"+ New AWB"** button

2. **Enter Shipper (Consignor) Details**
   | Field | Description | Example |
   |-------|-------------|---------|
   | Customer | Select from dropdown or search | "ABC Trading LLC" |
   | Shipper Name | Person sending | "Mohammed Ali" |
   | Phone | Contact number | "+971 50 123 4567" |
   | Email | Email for notifications | "m.ali@abc.ae" |
   | Address | Pickup/origin address | "Building 5, Al Quoz" |
   | City/State/Country | Origin location | Dubai, UAE |

3. **Enter Consignee (Receiver) Details**
   | Field | Description | Example |
   |-------|-------------|---------|
   | Consignee Name | Person receiving | "John Smith" |
   | Phone | Receiver contact (required) | "+44 7700 900123" |
   | Address | Delivery address | "45 High Street" |
   | City/State/Country | Destination | London, UK |
   | Postal Code | ZIP/PIN code | "SW1A 1AA" |

4. **Enter Shipment Details**
   | Field | Description |
   |-------|-------------|
   | Product Type | Document / Parcel / Heavy Cargo |
   | Service Type | Express / Standard / Economy |
   | Pieces | Number of packages |
   | Actual Weight | Weight in KG |
   | Dimensions | L x W x H (cm) for volumetric |
   | Description | Contents description |
   | Declared Value | Value for customs/insurance |

5. **Configure Payment & Charges**
   | Field | Description |
   |-------|-------------|
   | Payment Mode | Prepaid / Collect / Third Party / COD |
   | COD Amount | If Cash on Delivery, enter amount |
   | Apply Rate Card | System calculates based on assigned rate |
   | Other Charges | Fuel, handling, packaging fees |
   | Total Amount | System calculates total |

6. **Special Handling (if needed)**
   - Fragile
   - Temperature controlled
   - Dangerous goods
   - Priority handling

7. **Save and Print**
   - Click **"Save"** to generate AWB number
   - System shows: AWB Number, Barcode
   - Click **"Print AWB Label"** for shipping label
   - Click **"Print Invoice"** if prepaid

**AWB Number Format:** `[BranchPrefix][Sequence]`  
Example: DXB100001, DXB100002

**After Creating AWB:**
- Shipment status: `BOOKED`
- Ready for inscan at warehouse
- Appears in shipment tracking

---

## How to Create a Pickup Request (Staff)

**Navigation:** Pickup Management → Click "New Pickup"

**When to Use:** When a customer calls to schedule a pickup.

**Detailed Steps:**

1. **Access Pickup Management**
   - Click **Pickup Management** in left menu
   - Click **"+ New Pickup"** button

2. **Select or Add Customer**
   | Option | Action |
   |--------|--------|
   | Existing Customer | Search by name, code, or phone |
   | New Customer | Click "Add New" and enter details |

3. **Enter Pickup Details**
   | Field | Description | Example |
   |-------|-------------|---------|
   | Pickup Date | Scheduled date | 2024-01-15 |
   | Time Slot | Morning (9-12) / Afternoon (12-5) / Evening (5-8) | Morning |
   | Pickup Address | Collection location | "Office 301, Business Bay" |
   | Contact Person | Person to meet | "Sarah Ahmed" |
   | Contact Phone | Direct phone | "+971 50 987 6543" |

4. **Enter Shipment Estimate**
   | Field | Description |
   |-------|-------------|
   | Expected Pieces | Approximate number of packages |
   | Estimated Weight | Approximate total weight (KG) |
   | Product Type | Document / Parcel / Cargo |
   | Destination | Where shipment is going |
   | Special Instructions | "Handle with care", "Call before arrival" |

5. **Add Shipment Lines (Consignee Details)**
   - At least one shipment line is required
   - For each destination, enter:
     - Consignee Name (required)
     - City (select from dropdown - required)
     - Country (auto-fills from city)
     - Pieces, Weight, Cargo Description
   - Click **"+ Add Line"** for multiple destinations
   - See [How to Add Shipment Lines](#how-to-add-shipment-lines-to-a-pickup-request) for details

6. **Save the Request**
   - Click **"Create Pickup Request"**
   - System assigns **Pickup Request Number**
   - Status: `PICKUP_REQUESTED`

6. **Assign Courier (from Dashboard)**
   - Go to **Pickup Dashboard**
   - Find the unassigned pickup
   - Click **"Assign"**
   - Select available courier
   - Status changes to: `ASSIGNED_FOR_COLLECTION`

**Pickup Workflow:**
```
Requested → Assigned → In Transit → Collected → At Hub
```

---

## How to Create a Pickup Request (Customer Self-Service)

**Navigation:** Customer Portal → Request Pickup

**When to Use:** When customers with portal access create their own pickup requests.

**Prerequisites:** Customer must have portal login credentials.

**Detailed Steps (Customer Portal):**

1. **Login to Customer Portal**
   - Open the customer portal URL
   - Enter username and password
   - Dashboard shows account summary

2. **Request New Pickup**
   - Click **"Request Pickup"** button on dashboard
   - System pre-fills customer address from profile
   - Enter pickup details:
     | Field | Description |
     |-------|-------------|
     | Pickup Date | Preferred collection date |
     | Time Slot | Morning / Afternoon / Evening |
     | Expected Pieces | Number of packages |
     | Estimated Weight | Approximate total weight |
     | Special Instructions | Handling notes |
   - Click **"Submit Request"**

3. **Track Request Status**
   - View status in **My Pickups** section
   - Receive SMS/Email confirmation
   - Get courier assignment notification
   - Track courier en-route (if enabled)

**Detailed Steps (Staff Assisting Customers):**

1. Log into main system as staff
2. Go to **Pickup Management**
3. Click **"+ New Pickup"**
4. Select the customer from dropdown
5. System pre-fills their registered address
6. Verify/modify pickup details as needed
7. Click **"Create Pickup Request"**
8. Customer receives confirmation notification

---

## How to Add Shipment Lines to a Pickup Request

**Navigation:** Pickup Management → New Pickup or Edit Pickup → Shipment Lines Section

**When to Use:** When recording consignee destinations for packages within a pickup request. Each line represents a separate destination/consignee.

**Prerequisites:** Creating or editing a pickup request.

**Detailed Steps:**

1. **Access Shipment Lines Section**
   - In the Pickup Request form, scroll to **Shipment Lines** section
   - A default line is automatically added for new pickups

2. **Enter Consignee Details for Each Line**
   | Field | Required | Description | Example |
   |-------|----------|-------------|---------|
   | Consignee Name | Yes | Recipient name | "ABC Trading LLC" |
   | Contact Person | No | Contact at destination | "Mohammed Ali" |
   | Phone | No | Contact phone | "+971 50 123 4567" |
   | Address | No | Street address | "Warehouse 5, Industrial Area" |
   | City | Yes | Select from dropdown | "Dubai" |
   | State | Auto-filled | Auto-populates from city | "Dubai" |
   | Country | Yes (Auto-filled) | Auto-populates from city | "United Arab Emirates" |
   | Postal Code | No | ZIP/Postal code | "00000" |
   | Pieces | No | Number of packages | 2 |
   | Weight (KG) | No | Package weight | 5.5 |
   | Cargo Description | No | Contents description | "Documents" |

3. **Add Multiple Lines (Multiple Destinations)**
   - Click **"+ Add Line"** button to add more destinations
   - Each line gets a sequential line number
   - Fill consignee details for each destination

4. **Remove a Line**
   - Click the **Delete** (trash) icon on the line
   - Note: Cannot remove if only 1 line exists (minimum required)
   - Note: Cannot remove lines already booked or linked to AWB

5. **Save Pickup Request**
   - All shipment lines are saved with the pickup request
   - Lines are used when converting pickup to AWB

**Business Rules:**
- Minimum 1 shipment line required per pickup request
- City must be selected from dropdown (type to search)
- State and Country auto-populate when city is selected
- Booked lines cannot be deleted during edit

---

## How to Convert Pickup Request to AWB

**Navigation:** AWB Entry → From Pickup → Select Pickup

**When to Use:** When creating an AWB from a collected/inscanned pickup request.

**Prerequisites:** Pickup request must have status **Collected** or **Inscanned**.

**Detailed Steps:**

1. **Open AWB Entry Page**
   - Go to **AWB Entry** from the menu
   - Click **"From Pickup"** button

2. **Select Pickup Request**
   - Dialog shows eligible pickup requests (Collected/Inscanned status)
   - Search by pickup number, customer name, or date
   - Review shipment lines count for each pickup
   - Click on a pickup to select it

3. **Review Shipment Lines**
   - If pickup has shipment lines, they are displayed
   - Each line shows: Consignee, City, Country, Pieces, Weight
   - Select the line to convert to AWB
   - If no lines exist, a message prompts to add lines first

4. **Complete AWB Entry**
   - Shipper details auto-fill from pickup customer
   - Consignee details auto-fill from selected shipment line
   - Complete remaining AWB fields (service type, payment mode, etc.)
   - Click **Save** to create AWB

5. **After Conversion**
   - Shipment line status updates to **Booked**
   - AWB is linked to the pickup request
   - Original pickup can still be tracked

**Conversion Requirements:**
| Requirement | Description |
|-------------|-------------|
| Status | Must be Collected or Inscanned |
| Shipment Lines | At least 1 line required (conversion blocked without lines) |
| Customer | Must have valid customer assigned |

---

## How to Use City Selection (Geography Dropdown)

**Navigation:** Any form with City field (Pickup Request, AWB Entry, Party Master, etc.)

**When to Use:** When entering addresses that require city selection.

**Why Dropdown Only:** City selection uses a strict dropdown to ensure data integrity and proper geography hierarchy.

**Detailed Steps:**

1. **Start Typing City Name**
   - Click on the City field
   - Begin typing the city name (e.g., "Dub")
   - Dropdown shows matching cities

2. **Select from Dropdown**
   - Choose the correct city from the list
   - Cities show with their state/country for clarity
   - **Important:** You MUST select from dropdown (free text not allowed)

3. **Automatic State/Country Population**
   - When you select a city, State and Country auto-fill
   - These fields become read-only after auto-population
   - Ensures consistent geography hierarchy

4. **Example UAE Cities (Pre-configured):**
   | City | State/Emirate |
   |------|---------------|
   | Dubai | Dubai |
   | Sharjah | Sharjah |
   | Abu Dhabi | Abu Dhabi |
   | Ajman | Ajman |
   | Ras Al Khaimah | Ras Al Khaimah |
   | Fujairah | Fujairah |
   | Umm Al Quwain | Umm Al Quwain |
   | Al Ain | Abu Dhabi |
   
   *Note: Full list available in Geography Masters. Additional cities can be added by administrators.*

5. **If City Not Found**
   - Contact administrator to add new cities
   - New cities are added via Geography Masters
   - Navigation: System Settings → Geography Masters → Cities

**Benefits of Dropdown Selection:**
- Prevents spelling errors and duplicates
- Ensures proper State/Country linkage
- Enables accurate reporting by geography
- Supports rate calculation by zone

---

## How to Process Inscan (Warehouse Receiving)

**Navigation:** Sorting/Hub Operations → Inscan

**When to Use:** When shipments arrive at warehouse from pickups or transfers.

**Detailed Steps:**

1. **Access Inscan Page**
   - Go to **Sorting/Hub Operations → Inscan**
   - Select **Warehouse** (your location)

2. **Scan or Enter AWB**
   | Method | Action |
   |--------|--------|
   | Barcode Scan | Use scanner to read AWB barcode |
   | Manual Entry | Type AWB number and press Enter |
   | Bulk Scan | Continuous scanning mode |

3. **Verify Shipment**
   - System displays shipment details
   - Verify pieces count matches
   - Check weight (optional reweigh)
   - Check for damage

4. **Complete Inscan**
   - Click **"Confirm Inscan"**
   - Status updates to: `RECEIVED_AT_HUB`
   - Shipment location updated to warehouse

5. **Handle Exceptions**
   | Issue | Action |
   |-------|--------|
   | Piece count mismatch | Note discrepancy, create exception |
   | Damaged package | Take photo, mark as damaged |
   | Unknown AWB | Check if AWB exists, create if new |

**After Inscan:**
- Shipment ready for sorting
- Ready for MAWB bagging (if international)
- Ready for DRS assignment (if local delivery)

---

## How to Create MAWB and Bag Shipments

**Navigation:** Sorting/Hub Operations → Process Manifest

**When to Use:** When preparing shipments for air freight dispatch.

**Detailed Steps:**

1. **Create New MAWB**
   - Go to **Sorting/Hub Operations → Process Manifest**
   - Click **"+ New MAWB"**

2. **Enter MAWB Header**
   | Field | Description | Example |
   |-------|-------------|---------|
   | MAWB Number | Master AWB from airline | "176-12345678" |
   | Carrier | Airline name | "Emirates" |
   | Flight Number | Flight details | "EK 501" |
   | Origin | Departure airport | "DXB" |
   | Destination | Arrival airport | "LHR" |
   | ETD | Departure date/time | 2024-01-15 14:00 |
   | ETA | Arrival date/time | 2024-01-15 19:00 |

3. **Create Bags**
   - Click **"+ Add Bag"**
   - Enter bag details:
     - Bag Number (auto or manual)
     - Bag Type (Document / Parcel)
     - Seal Number
   - Create multiple bags as needed

4. **Add Shipments to Bags**
   - Select a bag
   - Scan AWB barcodes OR
   - Search and select shipments
   - System validates:
     - Shipment destination matches MAWB destination
     - Shipment not already in another MAWB
     - Shipment is eligible (inscanned, not on hold)

5. **Complete Bagging**
   - Review bag contents
   - Confirm piece/weight totals
   - Click **"Finalize Bag"**
   - Enter seal number

6. **Finalize MAWB**
   - All bags completed
   - Review manifest summary:
     - Total bags
     - Total shipments
     - Total weight
   - Click **"Finalize MAWB"**
   - Status: `DISPATCHED`

7. **Print Manifest**
   - Click **"Print Manifest"**
   - Generates PDF with:
     - MAWB header
     - Bag list with seal numbers
     - All shipment details

---

## How to Create DRS (Delivery Run Sheet)

**Navigation:** Sorting/Hub Operations → DRS-Outscan

**When to Use:** When assigning shipments to couriers for delivery.

**Detailed Steps:**

1. **Access DRS Page**
   - Go to **Sorting/Hub Operations → DRS-Outscan**
   - Click **"+ New DRS"**

2. **Create DRS Header**
   | Field | Description | Example |
   |-------|-------------|---------|
   | DRS Date | Delivery date | Today's date |
   | Courier | Select delivery agent | "Ahmed - Courier 01" |
   | Vehicle | Assign vehicle (optional) | "VAN-001" |
   | Route/Zone | Delivery area | "Dubai Marina" |

3. **Add Shipments to DRS**
   | Method | Action |
   |--------|--------|
   | Scan AWB | Scan barcodes of shipments |
   | Search | Find by AWB number |
   | Bulk Add | Filter and add multiple |
   | Auto-Assign | System suggests based on zone |

4. **Review DRS**
   - Verify all shipments listed
   - Check delivery sequence
   - View total pieces, COD amounts
   - Reorder if needed (drag-drop)

5. **Dispatch DRS**
   - Click **"Dispatch"**
   - All shipments status: `OUT_FOR_DELIVERY`
   - Courier receives notification
   - Print DRS sheet for courier

**DRS Sheet Includes:**
- All AWBs with consignee details
- Sequence/route order
- COD collection amounts
- Signature fields

---

## How to Capture POD (Proof of Delivery)

**Navigation:** Operations → POD Capture (or Mobile App)

**When to Use:** When courier delivers a shipment.

**Mobile POD Capture:**

1. **Open POD App/Page**
   - Courier accesses mobile POD interface
   - Scan or enter AWB number

2. **Confirm Delivery Status**
   | Status | When to Use |
   |--------|-------------|
   | Delivered | Shipment successfully delivered |
   | Partial | Some pieces delivered |
   | Refused | Customer refused to accept |
   | Not Delivered | Customer unavailable, wrong address |

3. **Capture POD Details**

   **For Delivered:**
   | Field | Required |
   |-------|----------|
   | Received By | Yes - Name of person |
   | Relation | Optional - Self, Relative, Guard, etc. |
   | Signature | Yes - Digital signature capture |
   | Photo | Optional - Package/delivery photo |
   | GPS Location | Auto-captured |
   | Delivery Time | Auto-captured |

   **For Not Delivered:**
   | Field | Required |
   |-------|----------|
   | Reason | Yes - Select from list |
   | Remarks | Optional - Additional notes |
   | Reschedule Date | Optional |

4. **Collect COD (if applicable)**
   - Enter amount collected
   - Select payment method (Cash/Card)
   - Capture payment reference

5. **Submit POD**
   - Click **"Submit"**
   - Status updates to: `DELIVERED` or `NOT_DELIVERED`
   - Tracking history updated
   - COD recorded in system

**Offline Mode:**
- POD can be captured without internet
- Data stored locally in device
- Auto-syncs when connection restored

---

## How to Update Bulk POD via Excel

**Navigation:** Operations → POD → Excel Upload

**When to Use:** When processing multiple POD updates at once.

**Detailed Steps:**

1. **Download Template**
   - Go to **Operations → POD → Excel Upload**
   - Click **"Download Template"** for blank template, OR
   - Click **"Template with AWBs"** to pre-fill pending deliveries

2. **Fill the Template**
   | Column | Required | Valid Values |
   |--------|----------|--------------|
   | AWB No | Yes | Shipment number |
   | Delivery Status | Yes | Delivered, Not Delivered, Partial, Refused |
   | Delivery Date | Yes | YYYY-MM-DD format |
   | Received By | For Delivered | Name of receiver |
   | Relation | Optional | Self, Relative, Guard, Colleague, Other |
   | Non-Delivery Reason | For Not Delivered | Address Wrong, Customer Unavailable, etc. |
   | Remarks | Optional | Additional notes |

3. **Upload File**
   - Click **"Upload"**
   - Select your Excel file
   - Wait for validation

4. **Review Validation Results**
   | Indicator | Meaning |
   |-----------|---------|
   | Green row | Valid, ready to process |
   | Red row | Error - see error message |
   | Yellow row | Warning - review needed |

5. **Fix Errors (if any)**
   - Common errors:
     - AWB not found
     - AWB not eligible for POD (not out for delivery)
     - Missing required field
     - Invalid status value
   - Fix in Excel and re-upload

6. **Process Updates**
   - Click **"Process POD Updates"**
   - System updates all valid records
   - Download **Results Report**

**Results Report Shows:**
- Success count
- Failed count with reasons
- Per-AWB status

---

## How to Process Return to Shipper (RTS)

**Navigation:** Operations → RTS Management

**When to Use:** When shipment cannot be delivered and must return to sender.

**Detailed Steps:**

1. **Initiate RTS**
   - Search for the AWB
   - Click **"Initiate RTS"**

2. **Select RTS Reason**
   | Reason | Description |
   |--------|-------------|
   | Customer Refused | Consignee refused delivery |
   | Undeliverable Address | Address incorrect/incomplete |
   | Customer Unreachable | Multiple delivery attempts failed |
   | Shipment Unclaimed | Held at location, not collected |
   | Customs Rejected | Failed customs clearance |

3. **Configure RTS Options**
   | Option | Description |
   |--------|-------------|
   | Swap Addresses | System swaps shipper ↔ consignee |
   | RTS Charge Mode | Prepaid / Collect from shipper |
   | RTS Amount | Return shipping charge |

4. **Confirm RTS**
   - Review swap details
   - Click **"Create RTS"**
   - New AWB created for return (or same AWB updated)
   - Original AWB status: `RTS_INITIATED`

5. **Process RTS Shipment**
   - RTS shipment follows normal flow:
   - Inscan → MAWB (if international) → DRS → Delivery to shipper

**RTS Tracking:**
- Original AWB shows "Returned to Shipper"
- Timeline shows RTS initiation and delivery

---

## How to Track a Shipment

**Navigation:** Multiple access points (Quick Search, Shipments List, Public Tracking)

**When to Use:** When checking shipment status, location, or delivery history.

**Prerequisites:** AWB number required for tracking.

**Detailed Steps:**

### Method 1: Quick Search (Fastest)
1. Locate **Quick Search** box in top navigation bar
2. Enter AWB number (e.g., "DXB100001")
3. Click search icon or press Enter
4. View complete shipment details and timeline

### Method 2: Shipments List
1. Go to **Shipments → All Shipments**
2. Use available filters:
   | Filter | Options |
   |--------|---------|
   | Date Range | From/To dates |
   | Status | Booked, In Transit, Delivered, etc. |
   | Customer | Select customer |
   | Origin/Destination | City or country |
3. Click on any shipment row to view details

### Method 3: Public Tracking Page
1. Open `/tracking` page (no login required)
2. Enter AWB number in search box
3. Click **"Track"** or press Enter
4. View public tracking timeline

**Tracking Information Display:**

| Section | Details Shown |
|---------|---------------|
| Header | AWB No, Current Status, Service Type |
| Parties | Shipper & Consignee names and addresses |
| Shipment | Pieces, Weight, Dimensions, Description |
| Timeline | Chronological status history with dates |
| Location | Current location / last scan point |
| POD | Delivery proof if delivered (signature, photo) |

**Status Timeline Example:**
```
✓ Booked - 10 Jan 09:00 - Dubai Hub
✓ Picked Up - 10 Jan 11:30 - Courier collected
✓ Received at Hub - 10 Jan 14:00 - Dubai Sorting
✓ In Transit - 10 Jan 18:00 - Dispatched to destination
✓ Arrived at Destination - 11 Jan 08:00 - London Hub
✓ Out for Delivery - 11 Jan 09:30 - Assigned to courier
✓ Delivered - 11 Jan 11:15 - Signed by: J. Smith
```

**Tips:**
- Bookmark `/tracking?awb=AWB123456` for quick access
- Share tracking link with customers
- Set up alerts for status changes

---

## How to Use Global Search

**Navigation:** Dashboard → Top Navigation Bar → Search Box

**When to Use:** When quickly searching across AWBs, customers, and invoices from anywhere in the system.

**Detailed Steps:**

1. **Access Global Search**
   - Located in the top navigation bar on the Dashboard
   - Single search box for all entity types

2. **Enter Search Query**
   - Type AWB number, customer name/code, or invoice number
   - Minimum 2 characters to start search
   - Search is case-insensitive

3. **View Search Results**
   Results are categorized with icons:
   | Icon | Category | Examples |
   |------|----------|----------|
   | 📦 | AWBs | AWB numbers matching query |
   | 👤 | Customers | Customer names or codes |
   | 🧾 | Invoices | Invoice numbers |

4. **Select Result**
   - Click on any result to navigate
   - **AWB** → Opens tracking page with full details
   - **Customer** → Opens customer master with filter applied
   - **Invoice** → Opens invoice view

**Search Examples:**
| Query | Finds |
|-------|-------|
| "DXB100" | AWBs starting with DXB100 |
| "ABC Trading" | Customers with matching name |
| "INV-2026" | Invoices from 2026 |
| "Mohammed" | Customers named Mohammed |

**Features:**
- Autocomplete suggestions as you type
- Status badges shown on AWB results
- Results limited to user's branch access
- PostgreSQL-optimized case-insensitive search

---

## How to Print Tracking Report

**Navigation:** Tracking Page → Print Icon

**When to Use:** When generating a professional PDF tracking report for customers or records.

**Detailed Steps:**

1. **Access Tracking Page**
   - Search for AWB using Global Search, OR
   - Navigate to **Shipments & Operations → Tracking**
   - Enter AWB number

2. **View Shipment Details**
   - Confirm shipment information displayed
   - Review tracking timeline

3. **Generate PDF Report**
   - Click the **Print** icon (🖨️) in the tracking page header
   - PDF generates and downloads automatically

**Report Contents:**

| Section | Information |
|---------|-------------|
| **Header** | Company logo, report title, generation date |
| **AWB Info** | AWB number, booking date, current status (color-coded) |
| **Parties** | Shipper and Consignee details (two-column layout) |
| **Shipment** | Pieces, weight, movement type, payment mode, COD amount |
| **Timeline** | Full tracking history with events, remarks, locations, timestamps |
| **Footer** | Page numbers, generation timestamp |

**Report Format:**
- A4 portrait orientation
- Professional layout suitable for customer sharing
- Status shown with color coding (Green=Delivered, Blue=In Transit, etc.)
- Generated via `/api/report/tracking/{awbNo}` endpoint

**Use Cases:**
- Customer proof of delivery requests
- Claims documentation
- Internal audit records
- Insurance documentation

---

## How to Generate Shipment Invoice

**Navigation:** Shipments & Operations → AWB Entry → Shipment Invoice Button

**When to Use:** When creating commercial/customs invoices for international shipments.

**Detailed Steps:**

1. **Access AWB Entry**
   - Go to **Shipments & Operations → Shipments**
   - Open the shipment (AWB) requiring invoice

2. **Generate Invoice**
   - Click **"Shipment Invoice"** button
   - PDF generates and downloads

**Invoice Contents:**

| Section | Information |
|---------|-------------|
| **Header** | Company logo, "Commercial Invoice" title |
| **Shipper Details** | Name, address, contact information |
| **Consignee Details** | Name, address, contact, tax ID if applicable |
| **Items Table** | Description, quantity, unit value, total value |
| **HS Codes** | Harmonized System codes for customs |
| **Customs Summary** | Total value, currency, country of origin |
| **Declaration** | Customs declaration statement |

**Invoice Format:**
- A4 portrait orientation
- Suitable for customs clearance
- Includes HS code columns for international shipments
- Generated via `/api/report/shipment-invoice/{id}` endpoint

**Use Cases:**
- International shipment documentation
- Customs clearance requirements
- Commercial trade documentation
- Proof of value for insurance

---

## How to Use Unified Warehouse Inscan

**Navigation:** Shipments & Operations → Pickup Inscan

**When to Use:** When receiving both domestic pickups and import shipments at the warehouse.

**Prerequisites:** Pickup requests or import shipments must exist in the system.

**Detailed Steps:**

1. **Access Pickup Inscan Page**
   - Go to **Shipments & Operations → Pickup Inscan**
   - Page opens with AWB mode selected by default

2. **Select Scan Mode**
   | Mode | Use For |
   |------|---------|
   | AWB | Scan domestic AWBs or import shipments |
   | Pickup Request | Scan pickup request numbers |

3. **Scan AWB Barcode**
   - Enter AWB number in scan field
   - Press Enter or click Scan
   - System automatically detects shipment type:
     - **Domestic AWB** → Updates status to "Inscanned at Origin"
     - **Import Shipment** → Updates status to "Received at Warehouse"

4. **View Recently Received**
   - **Combined Grid** shows both domestic and import shipments
   - **Import Chip** indicator shows which are import shipments
   - **Counter** shows total of both types received today

**Grid Columns:**
| Column | Description |
|--------|-------------|
| AWB No | Shipment number |
| Type | "Domestic" or "Import" chip |
| Consignee | Receiver name |
| City | Destination city |
| Time | Scan timestamp |

**Benefits of Unified Inscan:**
- Single scanning interface for all shipment types
- Warehouse staff don't need to switch between pages
- Combined counter for throughput tracking
- Consistent workflow for both domestic and import

**Status Updates:**
| Shipment Type | Before Scan | After Scan |
|---------------|-------------|------------|
| Domestic AWB | Collected | Inscanned at Origin |
| Import Shipment | In Transit | Received at Warehouse |

---

# Import Operations Guides

## How to Create a New Import (Air/Sea/Land)

**Navigation:** Import Operations → Import Dashboard → Click "New Import"

**When to Use:** When receiving shipments from international or domestic sources.

**Detailed Steps:**

1. **Access Import Entry**
   - Go to **Import Operations → Import Dashboard**
   - Click **"+ New Import"**

2. **Select Import Mode**
   | Mode | Use For | Identifier |
   |------|---------|------------|
   | Air | International air freight | MAWB Number |
   | Sea | Ocean cargo | Bill of Lading (BL) |
   | Land | Road transport | Truck/Vehicle Number |

3. **Enter Import Header (Mode-Specific)**

   **For Air Imports:**
   | Field | Description | Example |
   |-------|-------------|---------|
   | MAWB Number | Master Airwaybill | "176-12345678" |
   | Airline | Carrier name | "Emirates" |
   | Flight No | Flight number | "EK 501" |
   | Origin Airport | Departure | "LHR" |
   | Dest. Airport | Arrival | "DXB" |
   | ETA | Expected arrival | 2024-01-15 14:00 |

   **For Sea Imports:**
   | Field | Description | Example |
   |-------|-------------|---------|
   | Bill of Lading | BL number | "MAEU123456789" |
   | Shipping Line | Carrier | "Maersk" |
   | Vessel Name | Ship name | "MSC Oscar" |
   | Port of Loading | Origin port | "Southampton" |
   | Port of Discharge | Dest port | "Jebel Ali" |

   **For Land Imports:**
   | Field | Description | Example |
   |-------|-------------|---------|
   | Truck Number | Vehicle reg | "UAE-12345" |
   | Transport Co | Carrier name | "Gulf Transport" |
   | Origin City | Starting point | "Riyadh" |
   | Dest City | Arrival point | "Dubai" |

4. **Add Shipments**
   - Click **"+ Add Shipment"** or
   - Use bulk Excel import
   - Enter AWB details for each shipment

5. **Save Import**
   - Click **"Save"**
   - Import status: `CREATED`
   - Ready for customs processing

---

## How to Process Import Customs Clearance

**Navigation:** Import Operations → Customs Clearance

**When to Use:** When clearing imported shipments through customs.

**Prerequisites:** Import must exist and have status awaiting clearance.

**Detailed Steps:**

1. **Access Customs Processing**
   - Go to **Import Operations → Customs Clearance**
   - Or from Import Dashboard, click **"Process Customs"** on an import

2. **Select Import to Process**
   - Filter by date, status, or MAWB/BL number
   - Click on import to view shipments

3. **Review Shipment Details**
   For each shipment, verify:
   | Field | Purpose |
   |-------|---------|
   | HS Code | Harmonized System classification |
   | Customs Value | Declared value for duty calculation |
   | Description | Contents match declaration |
   | Origin Country | Country of manufacture |

4. **Process Individual Shipments**
   | Action | When to Use |
   |--------|-------------|
   | Approve | Shipment cleared, no issues |
   | Hold for Inspection | Physical inspection required |
   | Mark Duty Pending | Duty payment awaited |
   | Reject | Cannot clear, return required |

5. **Enter Duty Details (if applicable)**
   | Field | Description |
   |-------|-------------|
   | Duty Amount | Calculated import duty |
   | Tax Amount | VAT/GST if applicable |
   | Payment Reference | Duty payment receipt |
   | Clearance Date | Date of approval |

6. **Bulk Processing**
   - Select multiple shipments with checkboxes
   - Click **"Bulk Customs Update"**
   - Apply same status to all selected
   - Useful for routine clearances

7. **Upload Documents**
   - Attach customs declaration
   - Upload duty receipts
   - Store inspection reports

8. **Complete Processing**
   - Click **"Save Clearance"**
   - Cleared shipments status: `CLEARED`
   - Ready for local delivery

**Clearance Status Reference:**

| Status | Description | Next Action |
|--------|-------------|-------------|
| Pending | Awaiting processing | Review and process |
| Cleared | Approved for release | Ready for delivery |
| Held | Under inspection | Wait for result |
| Duty Pending | Payment required | Pay duty |
| Rejected | Cannot clear | Return to origin |

---

## How to Create Import via Excel Upload

**Navigation:** Import Operations → Excel Upload

**When to Use:** When importing large batches of shipment data.

**Detailed Steps:**

1. **Download the Template**
   - Go to **Import Operations → Excel Upload**
   - Select **Import Mode**: Air, Sea, or Land
   - Click **"Download Template"**
   - Template has mode-specific fields

2. **Template Structure**
   | Sheet | Contents |
   |-------|----------|
   | Sheet 1: Header | Import metadata |
   | Sheet 2: Shipments | Individual AWB details |

3. **Fill Header Sheet**
   | Column | Required | Description |
   |--------|----------|-------------|
   | Reference Number | Yes | MAWB/BL/Truck No |
   | Carrier Name | Yes | Airline/Shipping Line |
   | Origin | Yes | Origin location |
   | Destination | Yes | Destination |
   | ETA Date | Yes | Expected arrival |
   | Remarks | No | Additional notes |

4. **Fill Shipments Sheet**
   | Column | Required | Description |
   |--------|----------|-------------|
   | AWB Number | Yes | Shipment number |
   | Shipper Name | Yes | Sender name |
   | Consignee Name | Yes | Receiver name |
   | Consignee Address | Yes | Delivery address |
   | Pieces | Yes | Package count |
   | Weight | Yes | Total weight (KG) |
   | Description | Yes | Contents |
   | Customs Value | No | Declared value |
   | HS Code | No | Customs code |

5. **Upload and Validate**
   - Click **"Upload Excel"**
   - System validates:
     - Required fields present
     - Positive values for pieces/weight
     - No duplicate AWB numbers
     - Valid format

6. **Review Validation Results**
   | Indicator | Meaning |
   |-----------|---------|
   | Green ✓ | Valid row |
   | Red ✗ | Error - see message |
   | Yellow ⚠ | Warning - review |

7. **Fix Errors**
   - Download error report
   - Fix issues in Excel
   - Re-upload corrected file

8. **Process Import**
   - Click **"Import"** to create records
   - Transaction-safe: All or none
   - View imported records in dashboard

---

## Import Shipment Data Fields

The Import Inscan page displays shipment details in a comprehensive table with the following columns:

| Column | Description | Source |
|--------|-------------|--------|
| AWB | House Airwaybill number | Manual entry or Excel import |
| Shipper | Sender/shipper name | Manual entry or Excel import |
| Consignee | Receiver/consignee name | Manual entry or Excel import |
| Weight | Shipment weight in kg | Manual entry or Excel import |
| Pieces | Number of packages | Manual entry or Excel import |
| Bag | Assigned bag number | Bag management |
| Duty/VAT | Customs duty and VAT amount | Manual entry or Excel import |
| COD/Coll | Cash on Delivery or Collection amount | Manual entry or Excel import |
| Status | Current shipment status | System-managed |
| Inscanned | Time of inscan | Automatic |

**Excel Import Mapping:**
- Column "Duty/VAT Amount" → `DutyAmount` field
- Column "COD/Collection Amount" → `CODAmount` field
- Column "Shipper Name" → `ShipperName` field
- When COD amount is entered, `IsCOD` flag is automatically set to true

---

## How to Receive Import Shipments at Warehouse

**Navigation:** Shipments & Operations → Pickup Inscan (AWB Mode)

**When to Use:** When receiving import shipments at the warehouse after customs clearance.

**Prerequisites:** Import shipment must exist and be cleared for release.

**Detailed Steps:**

1. **Access Pickup Inscan Page**
   - Go to **Shipments & Operations → Pickup Inscan**
   - Ensure **AWB Mode** is selected (default)

2. **Scan Import AWB**
   - Enter or scan the import AWB number
   - Press Enter or click Scan

3. **System Detection**
   - System automatically detects this is an import shipment
   - Different from domestic AWB handling

4. **Status Update**
   - Import shipment status changes to **"Received at Warehouse"**
   - (Different from domestic AWBs which get "Inscanned at Origin")
   - Timestamp recorded automatically

5. **View in Combined Grid**
   - Import shipment appears in "Recently Received Today" grid
   - **"Import" chip** indicator distinguishes it from domestic
   - Counter includes both domestic and import shipments

**Visual Indicators:**
| Type | Chip Color | Status After Scan |
|------|------------|-------------------|
| Domestic AWB | No chip | Inscanned at Origin |
| Import Shipment | "Import" chip | Received at Warehouse |

**Next Steps After Receiving:**
- Import shipment ready for local delivery
- Assign to DRS for final delivery
- Track via unified tracking system

**Benefits:**
- Single interface for all receiving operations
- Warehouse staff don't switch between systems
- Unified throughput counter
- Clear visual distinction between shipment types

---

# Prepaid AWB Guides

## How to Manage AWB Stock

**Navigation:** Masters & Settings → Operations Masters → AWB Stock Management

**When to Use:** When receiving and tracking physical AWB inventory (books, stickers, rolls).

**Detailed Steps:**

1. **Access AWB Stock Management**
   - Go to **Masters & Settings → Operations Masters → AWB Stock Management**
   - View current stock summary

2. **Add New Stock**
   - Click **"+ Add Stock"**
   - Enter stock details:
   
   | Field | Description | Example |
   |-------|-------------|---------|
   | Stock Type | Book / Sticker Roll / Sheet | "Book" |
   | AWB Prefix | Prefix for AWB numbers | "DXB" |
   | Start Number | First AWB in range | 100001 |
   | End Number | Last AWB in range | 100100 |
   | Quantity | Total AWBs in stock | 100 |
   | Rate per AWB | Cost per AWB | 5.00 |
   | Received Date | Date stock received | 2026-01-15 |
   | Supplier | AWB supplier name | "Emirates Post" |

3. **Track Usage**
   - View allocated vs available AWBs
   - Monitor stock levels by type
   - Generate reorder alerts

---

## How to Issue Prepaid AWBs

**Navigation:** Customers & CRM → Customer AWB Issue

**When to Use:** When issuing prepaid AWBs to customers in advance.

**Prerequisites:** Customer must have Account Type set to "Pre-paid" to appear in Prepaid AWB issue mode.

**Detailed Steps:**

1. **Access Customer AWB Issue**
   - Go to **Customers & CRM → Customer AWB Issue**
   - Select **"Prepaid AWB"** option (default)

2. **Fill Issue Details**
   - Select customer (only Pre-paid account type customers shown)
   - Enter AWB issue details:
   
   | Field | Description | Example |
   |-------|-------------|---------|
   | Customer | Pre-paid account customer | "ABC Trading LLC" |
   | Origin | Origin city | "Dubai" |
   | Destination | Destination city (required for prepaid) | "Abu Dhabi" |
   | No. of AWBs | Number of AWBs to issue | 10 |
   | Rate per AWB | Price charged per AWB | 15.00 |
   | Total Amount | Auto-calculated | 150.00 |
   | Payment Mode | Cash / Bank | "Cash" |

3. **Payment Details**
   - Select payment mode (Cash or Bank)
   - Choose the corresponding account
   - Enter bank reference if bank payment

4. **Issue AWBs**
   - Click "Issue AWBs" to complete
   - System generates AWB numbers and updates balance
   - Accounting entries created automatically

---

## How to Use Prepaid AWB in Shipment

**Navigation:** Shipments & Operations → Shipments → New AWB

**When to Use:** When creating a shipment using a prepaid AWB number.

**Detailed Steps:**

1. **Start New Shipment**
   - Go to **Shipments & Operations → Shipments**
   - Click **"+ New AWB"**

2. **Select Prepaid Customer**
   - Choose customer with prepaid AWBs
   - System shows available prepaid balance

3. **Enter Prepaid AWB Number**
   - Enter the prepaid AWB number from customer's allocated range
   - System validates:
     - AWB belongs to selected customer
     - AWB not already used
     - AWB within valid range

4. **Complete Shipment Entry**
   - Fill remaining shipment details
   - Save shipment
   - Prepaid balance updated automatically

**Validation Errors:**
| Error | Meaning | Solution |
|-------|---------|----------|
| "AWB not in prepaid range" | AWB number not allocated to customer | Check customer's prepaid AWB range |
| "AWB already used" | AWB was used in another shipment | Use different AWB number |
| "No prepaid balance" | Customer has no prepaid AWBs remaining | Sell more prepaid AWBs |

---

## How to View Prepaid AWB Register

**Navigation:** Customers & CRM → Prepaid AWB Register

**When to Use:** To view prepaid AWB usage and balance by customer.

**Detailed Steps:**

1. **Access Prepaid Register**
   - Go to **Customers & CRM → Prepaid AWB Register**

2. **Filter Options**
   - Select customer (required)
   - Date range (optional)
   - Status filter (All / Used / Available)

3. **Report Columns**
   | Column | Description |
   |--------|-------------|
   | Customer Name | Customer account |
   | Total Prepaid | Total AWBs purchased |
   | Used | AWBs consumed in shipments |
   | Available | Remaining balance |
   | Prepaid Amount | Total prepaid value |

4. **Export Options**
   - Export to Excel
   - Print report

---

# Dashboard & Reporting Guides

## How to Use Customer Dashboard

**Navigation:** CRM → Customer Dashboard

**When to Use:** For complete customer overview and quick actions.

**Dashboard Components:**

1. **Customer Search**
   - Search by: Name, Code, Phone, Email
   - Recent customers shown
   - Click customer to load profile

2. **Summary Cards**
   | Card | Shows |
   |------|-------|
   | Total Shipments | All-time shipment count |
   | Pending Deliveries | Currently in transit |
   | Outstanding Balance | Unpaid invoice amount |
   | This Month Volume | Current month shipments |

3. **Recent Shipments Table**
   - Last 10 shipments
   - AWB, Date, Status, Destination
   - Click to view full details

4. **Financial Summary**
   - Open invoices with aging
   - Payment history
   - Credit limit and usage
   - Average payment days

5. **Quick Actions**
   | Action | Function |
   |--------|----------|
   | New Pickup | Create pickup request |
   | Shipment History | Full shipment list |
   | Statement | Generate account statement |
   | Edit Profile | Update customer info |

**Tips:**
- Pin frequently accessed customers
- Export data to Excel for analysis
- Set alerts for credit limit breach

---

## How to Use Pickup Dashboard

**Navigation:** Pickup Management → Dashboard

**When to Use:** For daily pickup management and courier assignment.

**Dashboard Layout:**

1. **Summary Cards**
   | Card | Shows |
   |------|-------|
   | Total Today | All pickup requests for today |
   | Pending Assignment | Not yet assigned to courier |
   | In Progress | Courier en route |
   | Completed | Collected and at hub |
   | Failed | Collection failed |

2. **Filters**
   - Date range
   - Status (Pending, Assigned, Collected, Failed)
   - Customer
   - Courier
   - Area/Zone

3. **Pickup List**
   - Request number
   - Customer name & address
   - Time slot
   - Assigned courier
   - Status with color coding

**Daily Workflow:**

1. **Morning: Review & Assign**
   - Check pending assignments
   - Review time slots
   - Assign couriers by zone
   - Send notifications

2. **Midday: Monitor**
   - Track collection progress
   - Handle delays
   - Reassign if needed

3. **Evening: Reconcile**
   - Verify all collected
   - Follow up on failures
   - Reschedule for tomorrow

**Assigning Couriers:**
1. Select unassigned pickup(s)
2. Click **"Assign"**
3. Choose courier from available list
4. Click **"Confirm"**
5. Courier receives notification

---

## How to De-brief Courier (End of Day)

**Navigation:** Reconciliation → DRS Reconciliation

**When to Use:** At end of day to close courier's delivery run.

**Detailed Steps:**

1. **Access DRS Reconciliation**
   - Go to **Reconciliation → DRS Reconciliation**

2. **Select DRS**
   | Filter | Select |
   |--------|--------|
   | Date | DRS date (usually today) |
   | Courier | Delivery agent name |
   | DRS Number | Specific DRS (optional) |
   
   Click **"Load DRS"**

3. **Review Delivery Status**
   For each shipment:
   | Status | Action Required |
   |--------|-----------------|
   | Delivered | Verify POD captured |
   | Not Delivered | Select reason, add notes |
   | Pending | Update with actual status |

4. **Reconcile COD Collection**
   | Field | Value |
   |-------|-------|
   | Expected COD | System calculated total |
   | Collected Amount | Actual amount from courier |
   | Difference | Shortfall or excess |

   If difference exists:
   - Document reason
   - Create adjustment entry

5. **Record Expenses**
   | Expense Type | Example |
   |--------------|---------|
   | Fuel | Daily fuel cost |
   | Toll | Road toll receipts |
   | Parking | Parking fees |
   | Other | Miscellaneous |

   Attach receipts if required.

6. **Review Summary**
   | Metric | Value |
   |--------|-------|
   | Shipments Delivered | X / Total |
   | Delivery Rate | XX% |
   | COD Collected | Amount |
   | Expenses | Amount |
   | Net Collection | Amount |

7. **Submit Day-End**
   - Click **"Submit Day-End"**
   - DRS status: CLOSED
   - Print Day-End Report
   - COD transferred to finance

---

## How to Reconcile Courier Cash/COD

**Navigation:** Accounts & Finance → Courier Receipts

**When to Use:** To match COD collected against expected and record handover.

**Detailed Steps:**

1. **Access Courier Receipts**
   - Go to **Accounts & Finance → Courier Receipts**

2. **Filter Collections**
   | Filter | Purpose |
   |--------|---------|
   | Date Range | Period to reconcile |
   | Courier | Specific courier |
   | Status | Pending / Reconciled |

3. **Load Collection Data**
   - Click **"Load Collections"**
   - System shows all COD shipments

4. **Review Collection Details**
   | Column | Shows |
   |--------|-------|
   | AWB No | Shipment number |
   | Customer | Consignee name |
   | Expected | COD amount on AWB |
   | Collected | Amount received |
   | Difference | Variance |

5. **Record Cash Handover**
   | Field | Enter |
   |-------|-------|
   | Amount Received | Actual cash from courier |
   | Payment Method | Cash / Cheque / Bank Transfer |
   | Reference | Receipt number or bank ref |
   | Received By | Staff name |
   | Date | Handover date |

6. **Handle Discrepancies**
   | Situation | Action |
   |-----------|--------|
   | Short Collection | Create debit note to courier |
   | Excess Collection | Create credit note |
   | Disputed Amount | Mark for investigation |
   | Customer Issue | Follow up with customer |

7. **Confirm Receipt**
   - Click **"Confirm Receipt"**
   - System updates:
     - Courier ledger balance
     - Cash/Bank account
     - Shipment payment status
     - Reconciliation report

---

# Finance Guides

## How to Create an Invoice

**Navigation:** Accounts & Finance → Invoices → Click "New Invoice"

**When to Use:** When billing customers for shipment services.

**Detailed Steps:**

1. **Access Invoice Creation**
   - Go to **Accounts & Finance → Invoices**
   - Click **"+ New Invoice"**

2. **Select Customer**
   - Search and select customer
   - System loads:
     - Customer details
     - Billing address
     - Payment terms
     - Outstanding balance

3. **Select Shipments to Bill**
   | Option | Action |
   |--------|--------|
   | Individual | Select specific AWBs |
   | Date Range | All unbilled in period |
   | Auto-Select | All unbilled for customer |

4. **Review Line Items**
   For each shipment:
   | Column | Value |
   |--------|-------|
   | AWB No | Shipment number |
   | Date | Shipment date |
   | Destination | Delivery location |
   | Weight | Chargeable weight |
   | Rate | Applied rate |
   | Amount | Line total |

5. **Add Other Charges**
   | Charge | Example |
   |--------|---------|
   | Fuel Surcharge | Percentage or fixed |
   | Insurance | Based on value |
   | Packaging | Special packing |
   | Handling | Heavy cargo |

6. **Apply Discounts (if any)**
   - Contract discount
   - Volume discount
   - Promotional discount

7. **Review Totals**
   | Field | Value |
   |-------|-------|
   | Subtotal | Sum of line items |
   | Discount | Applied discounts |
   | Tax (VAT) | Calculated tax |
   | **Total** | Final invoice amount |

8. **Save and Generate**
   - Click **"Save"**
   - Invoice number generated
   - Status: DRAFT or ISSUED
   - Click **"Print Invoice"** for PDF

**Invoice Workflow:**
```
Draft → Issued → Partially Paid → Fully Paid
```

---

## How to Record Customer Payment (Receipt)

**Navigation:** Accounts & Finance → Receipts → Click "New Receipt"

**When to Use:** When receiving payment from customers.

**Detailed Steps:**

1. **Access Receipt Entry**
   - Go to **Accounts & Finance → Receipts**
   - Click **"+ New Receipt"**

2. **Select Customer**
   - Search and select customer
   - System shows:
     - Outstanding invoices
     - Total amount due
     - Credit balance (if any)

3. **Enter Payment Details**
   | Field | Description | Example |
   |-------|-------------|---------|
   | Receipt Date | Payment date | Today |
   | Payment Method | Cash/Cheque/Bank/Card | Bank Transfer |
   | Reference | Check no. or bank ref | "TRF-12345" |
   | Bank Account | If bank transfer | "Emirates NBD" |
   | Amount | Payment amount | 15,000.00 |

4. **Allocate to Invoices**
   | Method | Action |
   |--------|--------|
   | Auto-Allocate | FIFO allocation |
   | Manual | Select invoices to pay |
   | Partial | Pay portion of invoice |

5. **Review Allocation**
   | Invoice | Amount | Allocated | Balance |
   |---------|--------|-----------|---------|
   | INV-001 | 5,000 | 5,000 | 0 |
   | INV-002 | 8,000 | 8,000 | 0 |
   | INV-003 | 10,000 | 2,000 | 8,000 |

6. **Handle Excess Payment**
   - Credit to customer account, OR
   - Record as advance payment

7. **Save Receipt**
   - Click **"Save"**
   - Receipt number generated
   - Invoices updated
   - Print receipt voucher

---

## How to Set Up Rate Cards

**Navigation:** Pricing & Billing → Rate Cards → Click "New Rate Card"

**When to Use:** When configuring pricing for services.

**Detailed Steps:**

1. **Access Rate Card Setup**
   - Go to **Pricing & Billing → Rate Cards**
   - Click **"+ New Rate Card"**

2. **Enter Basic Details**
   | Field | Description | Example |
   |-------|-------------|---------|
   | Rate Card Name | Descriptive name | "UAE Express 2024" |
   | Rate Card Code | Unique identifier | "UAE-EXP-24" |
   | Effective From | Start date | 2024-01-01 |
   | Effective To | End date (optional) | 2024-12-31 |
   | Currency | Pricing currency | AED |

3. **Select Applicability**
   | Field | Options |
   |-------|---------|
   | Movement Type | Domestic / International / Both |
   | Service Type | Express / Standard / Economy |
   | Payment Mode | Prepaid / Collect / Both |
   | Zone | Select applicable zones |

4. **Define Weight Slabs**
   | From (KG) | To (KG) | Rate Type | Rate |
   |-----------|---------|-----------|------|
   | 0.00 | 0.50 | Fixed | 25.00 |
   | 0.51 | 1.00 | Fixed | 35.00 |
   | 1.01 | 5.00 | Per KG | 10.00 |
   | 5.01 | 10.00 | Per KG | 8.00 |
   | 10.01 | 999.00 | Per KG | 6.00 |

5. **Add Additional Charges**
   | Charge | Type | Value |
   |--------|------|-------|
   | Fuel Surcharge | Percentage | 5% |
   | Min. Charge | Fixed | 25.00 |
   | Remote Area | Fixed | 50.00 |

6. **Save Rate Card**
   - Click **"Save"**
   - Rate card available for assignment

7. **Assign to Customers**
   - Go to customer profile
   - Select rate card from dropdown
   - Set priority if multiple cards

---

## How to Use Rate Simulator

**Navigation:** Pricing & Billing → Rate Simulator

**When to Use:** To test rate calculations before quoting customers.

**Detailed Steps:**

1. **Access Rate Simulator**
   - Go to **Pricing & Billing → Rate Simulator**

2. **Enter Shipment Details**
   | Field | Enter |
   |-------|-------|
   | Customer | Select or leave blank for default |
   | Origin | City/Country |
   | Destination | City/Country |
   | Weight | Actual weight (KG) |
   | Dimensions | L x W x H (cm) |
   | Service Type | Express / Standard |
   | Payment Mode | Prepaid / Collect |

3. **Click Calculate**
   - System processes all applicable rate cards
   - Shows calculated rates

4. **Review Results**
   | Column | Shows |
   |--------|-------|
   | Rate Card | Applied rate card name |
   | Zone | Determined zone |
   | Base Rate | Weight-based rate |
   | Fuel Charge | Surcharge amount |
   | Other Charges | Additional fees |
   | **Total** | Final amount |

5. **View Formula Trace**
   - Click **"Show Calculation"**
   - See step-by-step breakdown:
     - Volumetric weight calculation
     - Chargeable weight selection
     - Slab determination
     - Rate application
     - Surcharge addition

**Use Cases:**
- Quote customers accurately
- Verify rate card configuration
- Compare different service levels
- Test special pricing scenarios

---

## How to Generate Reports

**Navigation:** Various report locations

**Common Reports and How to Generate:**

### Financial Reports

**Accounts Receivable Aging**
1. Go to **Reports → AR Aging**
2. Select date
3. Filter by customer (optional)
4. Click **"Generate"**
5. Export to Excel or PDF

**Revenue Report**
1. Go to **Reports → Revenue**
2. Select date range
3. Group by: Branch / Customer / Service
4. Click **"Generate"**

### Operations Reports

**Shipment Status Report**
1. Go to **Reports → Shipment Status**
2. Select date range
3. Filter by status
4. Click **"Generate"**

**Delivery Performance**
1. Go to **Reports → Delivery Performance**
2. Select period
3. View metrics: On-time %, SLA breach, etc.

**Courier Performance**
1. Go to **Reports → Courier Report**
2. Select courier and date range
3. View deliveries, COD, expenses

### Export Options

| Format | Use For |
|--------|---------|
| PDF | Printing, official records |
| Excel | Analysis, manipulation |
| CSV | System imports |

---

## How to Use Public Tracking Page

**Navigation:** `/tracking` (no login required)

**When to Use:** Share with customers for self-service tracking.

**For Customers:**

1. **Access Tracking Page**
   - Open: `https://[your-domain]/tracking`
   - No login required

2. **Enter AWB Number**
   - Type AWB number in search box
   - Click **"Track"** or press Enter

3. **View Tracking Results**
   | Section | Shows |
   |---------|-------|
   | Status | Current shipment status |
   | Timeline | All status updates |
   | Location | Last scanned location |
   | Delivery | Expected/actual delivery |

4. **Timeline Details**
   Each event shows:
   - Date and time
   - Status description
   - Location
   - Remarks (if any)

**For Staff:**
- Share tracking URL with customers
- Format: `/tracking?awb=AWB123456`
- Embed in emails or SMS notifications

---

## How to Use General Ledger

**Navigation:** Finance & Accounting → General Ledger

The General Ledger module provides comprehensive financial management with the following components:

### Masters Setup

**1. Chart of Accounts**
- **Navigation:** Finance & Accounting → General Ledger → Masters → Chart of Accounts
- Create and manage account hierarchy (Assets, Liabilities, Income, Expenses, Equity)
- Self-referential structure for unlimited nesting levels
- Define account types and nature (Debit/Credit)

**2. Control Accounts**
- **Navigation:** Finance & Accounting → General Ledger → Masters → Control Accounts
- Link operational modules to GL accounts
- Map: AR Control, AP Control, Cash, Bank, Revenue, COD Payable

**3. Financial Years**
- **Navigation:** Finance & Accounting → General Ledger → Masters → Financial Years
- Define fiscal periods with auto-generated monthly periods
- Open/Close periods for transaction control

**4. Tax Setup**
- **Navigation:** Finance & Accounting → General Ledger → Masters → Tax Setup
- Configure GST, VAT, Service Tax rates

### Transactions

**Cash & Bank Vouchers**
- **Navigation:** Finance & Accounting → General Ledger → Transactions → Cash & Bank → Cash & Bank Vouchers
- Record cash receipts, cash payments, bank deposits, bank payments, bank transfers

**Bank Accounts**
- **Navigation:** Finance & Accounting → General Ledger → Transactions → Cash & Bank → Bank Accounts
- Manage bank account details

**Bank Reconciliation**
- **Navigation:** Finance & Accounting → General Ledger → Transactions → Cash & Bank → Bank Reconciliation
- Match bank statement with system records

**Journal Voucher**
- **Navigation:** Finance & Accounting → General Ledger → Transactions → Journal Voucher
- Create general journal entries with multi-line debits and credits

### Financial Reports

| Report | Navigation | Purpose |
|--------|------------|---------|
| Account Ledger | Financial Reports → Account Ledger | View account transactions (Standard/Detailed/Summary) |
| Day Book | Financial Reports → Day Book | Daily transaction listing |
| Cash & Bank Book | Financial Reports → Cash & Bank Book | Cash and bank movement summary |
| Trial Balance | Financial Reports → Trial Balance | Account balances (Summary/Standard/Grouped/Monthly/Detailed) |
| Profit & Loss | Financial Reports → Profit & Loss | Income vs expenses (Summary/Item Wise/Period Comparison) |
| Balance Sheet | Financial Reports → Balance Sheet | Assets vs liabilities (Standard/Group-wise/Horizontal/Vertical) |
| Cash Flow | Financial Reports → Cash Flow | Cash movement (Indirect/Direct Method) |

---

## How to Email Reports

**Navigation:** Various report pages (Customer Ledger, Customer Statement, Supplier Ledger, Supplier Statement)

**When to Use:** To send PDF reports directly to customers or suppliers via email.

**Prerequisites:** 
- Google Gmail integration configured
- Customer/Supplier must have valid email address on file
- Branch must have email address configured

**Detailed Steps:**

1. **Generate the Report**
   - Navigate to the desired report:
     - **Customer Ledger:** Finance & Accounting → Accounts Receivable → AR Reports → Customer Ledger
     - **Customer Statement:** Finance & Accounting → Accounts Receivable → AR Reports → Customer Statement
     - **Supplier Ledger:** Finance & Accounting → Accounts Payable → AP Reports → Supplier Ledger
     - **Supplier Statement:** Finance & Accounting → Accounts Payable → AP Reports → Supplier Statement

2. **Select Party and Filters**
   - Choose customer or supplier
   - Set date range
   - Apply any additional filters
   - Click **"Generate Report"**

3. **Click Email Button**
   - Find the **"Email"** button in the toolbar
   - Click to open email dialog

4. **Confirm Email Details**
   | Field | Auto-populated |
   |-------|----------------|
   | To | Party's email address |
   | From | Branch email address |
   | Subject | Report name and date range |
   | Attachment | PDF report |

5. **Send Email**
   - Click **"Send"**
   - Email sent via Gmail API
   - Success message confirms delivery

**Email Content:**
- Professional HTML email body with report summary
- PDF attachment with full report details
- Includes: Party name, date range, opening/closing balances

**Troubleshooting:**
| Issue | Solution |
|-------|----------|
| Email button disabled | Ensure party has email address |
| Send failed | Check Gmail integration status |
| No "From" address | Configure branch email in Branch Settings |

---

# Suggest a New How-To Topic

**Can't find what you're looking for?**

We're constantly improving this knowledge base based on user feedback. If you need help with something not covered in these guides:

### Submit Your Question

1. **Use the Form Below** - Scroll to bottom of Knowledge Base page
2. **Describe Your Task** - What are you trying to accomplish?
3. **Provide Context** - Which module or feature relates to your question?

### Contact Support

- **Email:** support@net4courier.com
- **Subject:** "Knowledge Base Request: [Your Topic]"
- **Include:** Screenshots if you're stuck on a specific screen

### Popular Topics We're Working On

Based on user requests, these guides are coming soon:
- How to set up multi-currency operations
- How to configure automated notifications
- How to create custom reports
- How to integrate with external systems
- How to manage franchisee operations

---

# How To Guides - Quick Reference

| Task | Guide Section |
|------|---------------|
| Set up company | System Setup Guides |
| Create branch/warehouse | System Setup Guides |
| Add users and roles | System Setup Guides |
| Book new shipment | Operations Guides |
| Create pickup request | Operations Guides |
| Receive at warehouse | Operations Guides |
| Create MAWB/manifest | Operations Guides |
| Assign deliveries (DRS) | Operations Guides |
| Capture POD | Operations Guides |
| Process returns (RTS) | Operations Guides |
| Track shipment | Operations Guides |
| Import (Air/Sea/Land) | Import Operations Guides |
| Customs clearance | Import Operations Guides |
| Bulk POD update | Operations Guides |
| Customer dashboard | Dashboard & Reporting Guides |
| Pickup dashboard | Dashboard & Reporting Guides |
| Courier de-briefing | Dashboard & Reporting Guides |
| Create invoice | Finance Guides |
| Record payment | Finance Guides |
| Set up rate cards | Finance Guides |
| Rate simulator | Finance Guides |
| Generate reports | Finance Guides |

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

### Complete Pickup Workflow

```
┌──────────────────┐     ┌───────────────────────┐     ┌─────────────────┐
│  Pickup Request  │────▶│ Assigned for Collection│────▶│    Collected    │
│   (Status = 1)   │     │      (Status = 2)      │     │  (Status = 4)   │
└──────────────────┘     └───────────────────────┘     └─────────────────┘
                                    │                          │
                                    │ Failed                   │
                                    ▼                          ▼
                         ┌──────────────────┐          ┌─────────────────┐
                         │    Attempted     │          │    Inscanned    │
                         │   (Status = 3)   │          │   (Status = 5)   │
                         └──────────────────┘          └─────────────────┘
                                    │                          │
                                    │ Re-assign                │
                                    └──────────────────────────┘
                                              │
                                              ▼
                                      ┌─────────────────┐
                                      │   AWB Created   │
                                      └─────────────────┘
```

### Pickup Status Codes

| Status | Value | Description | Next Actions |
|--------|-------|-------------|--------------|
| **Pickup Request** | 1 | Initial request created | Assign to courier, Cancel |
| **Assigned for Collection** | 2 | Courier assigned to collect | Mark Collected, Mark Attempted |
| **Attempted** | 3 | Collection failed (customer unavailable, etc.) | Re-assign to another courier |
| **Shipment Collected** | 4 | Courier has collected the shipment | Proceed to Inscan |
| **Inscanned** | 5 | Shipment received at warehouse | Convert to AWB |
| **Cancelled** | 6 | Request cancelled | No further action |

### Workflow Steps

1. **Create Pickup Request**
   - Customer calls or submits online request
   - Enter customer details, pickup address, expected pieces
   - **Add shipment lines** with consignee details (required for AWB conversion)
   - System assigns Pickup Request Number (e.g., PKP-2026-0001)
   - Status: `Pickup Request (1)`

2. **Assign Collection Agent**
   - Click the **Assign** button (motorcycle icon) on the pickup
   - Select courier from dropdown and assign
   - Agent receives pickup details with address and special instructions
   - Timestamps: `AssignedAt` recorded
   - Status: `Assigned for Collection (2)`

3. **Collection Outcome** (One of two results)

   **Option A - Successful Collection:**
   - Agent arrives at customer location
   - Verifies package count and condition
   - Collects COD amount if applicable
   - Click **Mark Collected** button
   - Timestamps: `CollectedAt` recorded
   - Status: `Shipment Collected (4)`

   **Option B - Failed Collection (Attempted):**
   - Agent unable to collect (customer not available, address wrong, etc.)
   - Click **Mark Attempted** button
   - Enter reason for failed collection
   - Timestamps: `AttemptedAt`, `AttemptedByUserName` recorded
   - Status: `Attempted (3)`
   - **Can be re-assigned** to another courier for retry

4. **Inscan at Warehouse**
   - Agent brings collected packages to warehouse
   - Scan packages at inscan station
   - Verify weight and piece count
   - Status: `Inscanned (5)`

5. **Convert to AWB**
   - From Pickup Request, click **Convert to AWB**
   - System creates AWB from shipment lines
   - Each shipment line becomes a separate AWB
   - Original pickup marked as converted

### Dashboard Summary Cards

The Pickup Management dashboard displays real-time counts for each status:

| Card | Color | Description |
|------|-------|-------------|
| Pending | Orange | New pickup requests awaiting assignment |
| Assigned | Blue | Pickups assigned to couriers |
| Attempted | Red | Failed collection attempts needing re-assignment |
| Collected | Green | Successfully collected, pending inscan |
| Inscanned | Purple | Received at warehouse, ready for processing |

### Key Fields

| Field | Description |
|-------|-------------|
| Pickup Request No | Auto-generated unique identifier (PKP-YYYY-NNNN) |
| Customer | Party who requested pickup |
| Pickup Address | Collection location with landmark |
| Pickup Schedule | Time slot (Morning, Afternoon, Evening) |
| Expected Pieces | Anticipated number of packages |
| Pickup Date/Time | Scheduled collection date and time |
| Assigned Agent | Courier assigned for collection |
| Attempt Remarks | Reason for failed collection (if Attempted) |
| Shipment Lines | Individual consignee details for AWB creation |

### Business Rules

1. **Shipment Lines Required**: Pickup requests must have at least one shipment line with consignee details before converting to AWB
2. **City Selection**: UAE cities must be selected from dropdown (auto-populates State/Country)
3. **Re-assignment**: Attempted pickups can be re-assigned to the same or different courier
4. **Timestamps**: All status changes are recorded with date/time and user info
5. **Audit Trail**: Complete history of status changes maintained in PickupStatusHistory

### Keywords
pickup request, collection, customer pickup, assign agent, schedule pickup, pickup scheduling, courier assignment, collect shipment, pickup confirmation, attempted pickup, failed collection, re-assign courier, shipment lines, pickup inscan, convert to awb

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

**Navigation:** Finance → Accounts Payable → Vendor Bills

**Purpose:** Record and manage bills from suppliers/vendors for accounts payable tracking.

#### Bill Entry
1. Click **New Vendor Bill** to create a new bill
2. Select the supplier from the dropdown
3. Enter bill details:
   - **Bill No**: Vendor's invoice number
   - **Bill Date**: Invoice date from vendor
   - **Due Date**: Payment due date
   - **Description**: Purpose of the bill
4. Enter amounts:
   - **Sub Total**: Bill amount before tax
   - **Tax %**: Applicable tax percentage
   - **Total Amount**: Automatically calculated
5. Save as **Draft** or **Post** immediately

#### Bill Statuses
| Status | Description |
|--------|-------------|
| Draft | Bill entered but not yet approved |
| Approved | Bill verified and ready for payment |
| Paid | Payment completed |

#### Actions
- **Edit**: Modify draft bills
- **Approve**: Mark bill as verified
- **Pay**: Record payment against the bill
- **Delete**: Remove draft bills only

### Keywords
vendor bill, purchase invoice, AP invoice, supplier bill, expense bill, accounts payable

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

**Navigation:** Finance → Accounts Payable → Expenses

**Purpose:** Track, submit, and approve operational expenses with category-based organization and approval workflow.

#### Creating an Expense
1. Click **New Expense** to create a new expense entry
2. Fill in the details:
   - **Expense No**: Auto-generated or manual entry
   - **Date**: When the expense was incurred
   - **Category**: Select from Travel, Fuel, Maintenance, Office, Communication, Utilities, Insurance, Customs, Freight, Handling, or Other
   - **Description**: Details about the expense
3. Optionally link to:
   - **Supplier**: If expense is from a vendor
   - **Employee**: If claimed by an employee
4. Enter amounts:
   - **Amount**: Base expense amount
   - **Tax %**: Applicable tax percentage
   - **Total**: Automatically calculated
5. Add **Reference No** and **Remarks** as needed
6. Save the expense

#### Expense Categories
| Category | Use For |
|----------|---------|
| Travel | Transport, flights, accommodation |
| Fuel | Vehicle fuel costs |
| Maintenance | Repairs, vehicle servicing |
| Office | Office supplies, equipment |
| Communication | Phone, internet, postage |
| Utilities | Electricity, water, gas |
| Insurance | Vehicle, cargo insurance |
| Customs | Customs duties, clearance fees |
| Freight | Shipping, carrier charges |
| Handling | Warehouse, handling fees |
| Other | Miscellaneous expenses |

#### Approval Workflow
| Status | Description |
|--------|-------------|
| Draft | Initial entry, can be edited or deleted |
| Submitted | Sent for approval, awaiting review |
| Approved | Verified and approved for payment |
| Rejected | Not approved, returned with reason |
| Paid | Payment completed |

#### Actions
- **Submit**: Send draft expense for approval
- **Approve/Reject**: Manager actions on submitted expenses
- **Edit**: Modify draft or rejected expenses
- **Delete**: Remove draft expenses only
- **Export**: Download expense list to Excel

### Keywords
expense management, expense tracking, operational expense, cost management, expense approval, expense category

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

**Navigation:** Pricing → Fuel Surcharge

**Purpose:** Configure fuel surcharge rates that are automatically applied to shipment charges based on fuel price fluctuations.

#### Creating a Fuel Surcharge
1. Click **New Fuel Surcharge** to add a new rate
2. Configure the surcharge:
   - **Name**: Descriptive name (e.g., "Q1 2026 Fuel Surcharge")
   - **Percentage**: Surcharge rate (e.g., 15%)
   - **Effective From**: Start date for this rate
   - **Effective To**: End date (optional, leave blank for ongoing)
   - **Movement Type**: Domestic, International, or All
   - **Is Active**: Enable/disable the surcharge
3. Save the configuration

#### How It Works
- Fuel surcharge is applied as a percentage of the base freight charge
- Only active surcharges within the effective date range are applied
- Multiple surcharges can be configured for different movement types
- System automatically selects the applicable surcharge based on shipment date and type

#### Example Calculation
| Base Freight | Fuel Surcharge % | Fuel Amount |
|--------------|------------------|-------------|
| AED 100 | 15% | AED 15 |
| AED 250 | 12% | AED 30 |

### Keywords
fuel surcharge, FSC, fuel charge, fuel adjustment, fuel rate, surcharge percentage

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

**Navigation:** Pricing → Discounts & Contracts

**Purpose:** Create customer-specific discount agreements with service type filtering, percentage discounts, and validity periods.

### Creating a Discount Contract
1. Click **New Contract** to create a discount agreement
2. Configure the contract:
   - **Contract Name**: Descriptive name (e.g., "ABC Corp Volume Discount")
   - **Customer**: Select the customer for this contract
   - **Discount %**: Percentage discount to apply (e.g., 10%)
   - **Service Types**: Select which service types this discount applies to (or leave blank for all)
   - **Valid From**: Contract start date
   - **Valid To**: Contract end date
   - **Is Active**: Enable/disable the contract
3. Add **Description** and **Remarks** as needed
4. Save the contract

### How It Works
- Discount contracts are customer-specific pricing agreements
- System automatically applies the discount when creating invoices for the customer
- Discounts can be limited to specific service types
- Only active contracts within the validity period are applied
- Multiple contracts can exist for a customer with different service types

### Contract Management
| Action | Description |
|--------|-------------|
| Edit | Modify contract terms and dates |
| Activate/Deactivate | Enable or disable the contract |
| Delete | Remove unused contracts |
| View History | Track contract changes |

### Example
| Customer | Discount | Service Types | Valid Period |
|----------|----------|---------------|--------------|
| ABC Corp | 15% | Express, Same Day | Jan-Dec 2026 |
| XYZ Ltd | 10% | All Services | Feb-Jun 2026 |

### Keywords
discount, volume discount, contract discount, special pricing, promotional rate, customer discount, discount agreement

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
| Receive shipment (domestic) | Shipments & Operations → Pickup Inscan |
| Receive import shipment | Shipments & Operations → Pickup Inscan (AWB mode) |
| Create AWB | Shipments → New |
| Configure branch AWB by movement type | Masters & Settings → Branches → Edit |
| Dispatch for delivery | Sorting/Hub Operations → DRS-Outscan |
| Capture POD | Mobile POD (field) |
| Generate invoice | Accounts & Finance → AR → Generate Invoice |
| Record payment | Accounts & Finance → AR → Customer Payments |
| Add customer | CRM → Customer Profiles |
| Create rate card | Pricing & Billing → Rate Cards |
| Check rate | Pricing & Billing → Rate Simulator |
| Set up zones (Int'l/Domestic) | Pricing & Billing → Zone Management |
| Use global search | Dashboard → Top Search Box |
| Print tracking report | Tracking Page → Print Icon |
| Generate shipment invoice | AWB Entry → Shipment Invoice Button |
| Email report to customer | Financial Reports → Email Button |
| Manage currencies | Masters & Settings → Currencies |
| Set branch currency | Masters & Settings → Branches → Edit → Currency |
| Create demo data | Masters & Settings → Demo Data Management |
| Delete all business data | Platform Administration → Manage Demo Data |
| Manage tenant settings | Platform Administration → Tenant Settings |
| Initial setup | /setup (first-time only) |

## New Features (January 2026)

| Feature | Description |
|---------|-------------|
| Unified Warehouse Inscan | Single scanning interface for domestic and import shipments |
| Movement-Type AWB Config | Separate AWB series for Domestic/Export/Import/Transhipment per branch |
| Zone Types | International zones (countries) and Domestic zones (cities) with chip selection |
| Global Search | Search across AWBs, Customers, and Invoices from dashboard |
| Tracking Print | Generate professional A4 PDF tracking reports |
| Shipment Invoice | Generate commercial/customs invoices for international shipments |
| Email Reports | Send financial reports via Gmail to customers/suppliers |
| Initial Setup Wizard | Secure administrator setup for new deployments |
| Demo Data Management | Create/delete demo records for training purposes |
| **Branch Currency Default** | Branch currency automatically used throughout all transactions and dashboards |
| **Currency Selection** | Company and Branch now have currency dropdown selection (linked to Currency master) |
| **Delete All Business Data** | Platform Admin feature to reset all business data while preserving configuration |
| **Platform Administration** | New admin section: Tenant Settings, Subscription Management, Demo Data Management |
| **Three-Tier Security** | Server-side role authorization for sensitive admin features |

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+F | Search this document |
| Enter | Submit form |
| Esc | Close dialog |
| Tab | Next field |

## Search Keywords Summary

**Operations:** pickup, collection, inscan, AWB, shipment, manifest, MAWB, bag, DRS, outscan, dispatch, POD, delivery, RTS, return, tracking, import, warehouse, unified

**Finance:** invoice, receipt, payment, journal, ledger, GL, AR, AP, tax, GST, aging, credit note, debit note, email report, branch currency, default currency

**CRM:** customer, party, contract, SLA, complaint, ticket, department, designation

**Pricing:** rate card, zone, slab, fuel surcharge, discount, charges, simulator, international zone, domestic zone

**System:** company, branch, user, role, permission, status, service type, currency, movement type, AWB config, setup, demo data, delete all data, platform admin, tenant, subscription

---

*Last Updated: January 30, 2026*
*Version: 2.1*
*Net4Courier - Linked To Deliver*
