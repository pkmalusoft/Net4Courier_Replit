-- Net4Courier Database Schema Sync Script
-- This script safely adds missing columns to production database
-- Safe to run multiple times (idempotent) using ADD COLUMN IF NOT EXISTS
-- Generated for deployment synchronization

-- ============================================
-- BRANCHES TABLE
-- ============================================
ALTER TABLE "Branches" ADD COLUMN IF NOT EXISTS "TimeZoneId" VARCHAR(100) DEFAULT 'Asia/Dubai';
ALTER TABLE "Branches" ADD COLUMN IF NOT EXISTS "UseAwbStockManagement" BOOLEAN DEFAULT FALSE;
ALTER TABLE "Branches" ADD COLUMN IF NOT EXISTS "HideAccountCodes" BOOLEAN DEFAULT FALSE;
ALTER TABLE "Branches" ADD COLUMN IF NOT EXISTS "VatPercentage" DECIMAL(5,2) DEFAULT 5.00;
ALTER TABLE "Branches" ADD COLUMN IF NOT EXISTS "CurrencyId" BIGINT;
ALTER TABLE "Branches" ADD COLUMN IF NOT EXISTS "AWBPrefix" VARCHAR(20);
ALTER TABLE "Branches" ADD COLUMN IF NOT EXISTS "AWBStartingNumber" BIGINT DEFAULT 1;
ALTER TABLE "Branches" ADD COLUMN IF NOT EXISTS "AWBIncrement" INT DEFAULT 1;
ALTER TABLE "Branches" ADD COLUMN IF NOT EXISTS "AWBLastUsedNumber" BIGINT DEFAULT 0;
ALTER TABLE "Branches" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- COMPANIES TABLE (AR Settings)
-- ============================================
ALTER TABLE "Companies" ADD COLUMN IF NOT EXISTS "UseAwbStockManagement" BOOLEAN DEFAULT FALSE;
ALTER TABLE "Companies" ADD COLUMN IF NOT EXISTS "InvoiceTermsAndConditions" TEXT;
ALTER TABLE "Companies" ADD COLUMN IF NOT EXISTS "BankAccountTitle" VARCHAR(255);
ALTER TABLE "Companies" ADD COLUMN IF NOT EXISTS "BankAccountTitleArabic" VARCHAR(255);
ALTER TABLE "Companies" ADD COLUMN IF NOT EXISTS "BankAccountNumber" VARCHAR(100);
ALTER TABLE "Companies" ADD COLUMN IF NOT EXISTS "BankIBANNumber" VARCHAR(100);
ALTER TABLE "Companies" ADD COLUMN IF NOT EXISTS "BankAccountType" VARCHAR(50);
ALTER TABLE "Companies" ADD COLUMN IF NOT EXISTS "BankAccountTypeArabic" VARCHAR(100);
ALTER TABLE "Companies" ADD COLUMN IF NOT EXISTS "BankCurrency" VARCHAR(20);
ALTER TABLE "Companies" ADD COLUMN IF NOT EXISTS "BankSWIFTCode" VARCHAR(50);
ALTER TABLE "Companies" ADD COLUMN IF NOT EXISTS "BankName" VARCHAR(255);
ALTER TABLE "Companies" ADD COLUMN IF NOT EXISTS "BankNameArabic" VARCHAR(255);
ALTER TABLE "Companies" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- IMPORT MASTERS TABLE
-- ============================================
ALTER TABLE "ImportMasters" ADD COLUMN IF NOT EXISTS "InscannedAt" TIMESTAMP;
ALTER TABLE "ImportMasters" ADD COLUMN IF NOT EXISTS "InscannedByUserId" BIGINT;
ALTER TABLE "ImportMasters" ADD COLUMN IF NOT EXISTS "InscannedByUserName" VARCHAR(255);
ALTER TABLE "ImportMasters" ADD COLUMN IF NOT EXISTS "ClosedAt" TIMESTAMP;
ALTER TABLE "ImportMasters" ADD COLUMN IF NOT EXISTS "ClosedByUserId" BIGINT;
ALTER TABLE "ImportMasters" ADD COLUMN IF NOT EXISTS "ClosedByUserName" VARCHAR(255);
ALTER TABLE "ImportMasters" ADD COLUMN IF NOT EXISTS "CreatedByName" VARCHAR(255);
ALTER TABLE "ImportMasters" ADD COLUMN IF NOT EXISTS "ModifiedByName" VARCHAR(255);
ALTER TABLE "ImportMasters" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- IMPORT SHIPMENTS TABLE
-- ============================================
ALTER TABLE "ImportShipments" ADD COLUMN IF NOT EXISTS "InscannedAt" TIMESTAMP;
ALTER TABLE "ImportShipments" ADD COLUMN IF NOT EXISTS "InscannedByUserId" BIGINT;
ALTER TABLE "ImportShipments" ADD COLUMN IF NOT EXISTS "InscannedByUserName" VARCHAR(255);
ALTER TABLE "ImportShipments" ADD COLUMN IF NOT EXISTS "CustomsValue" DECIMAL(18,4) DEFAULT 0;
ALTER TABLE "ImportShipments" ADD COLUMN IF NOT EXISTS "DutyAmount" DECIMAL(18,4) DEFAULT 0;
ALTER TABLE "ImportShipments" ADD COLUMN IF NOT EXISTS "DutyVatAmount" DECIMAL(18,4) DEFAULT 0;
ALTER TABLE "ImportShipments" ADD COLUMN IF NOT EXISTS "CalculatedImportVAT" DECIMAL(18,4) DEFAULT 0;
ALTER TABLE "ImportShipments" ADD COLUMN IF NOT EXISTS "NetTotal" DECIMAL(18,4) DEFAULT 0;
ALTER TABLE "ImportShipments" ADD COLUMN IF NOT EXISTS "PaymentMode" VARCHAR(20);
ALTER TABLE "ImportShipments" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- IMPORT BAGS TABLE
-- ============================================
ALTER TABLE "ImportBags" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- AWB TRACKINGS TABLE
-- ============================================
ALTER TABLE "AWBTrackings" ADD COLUMN IF NOT EXISTS "ShipmentType" VARCHAR(50) DEFAULT 'Domestic';
ALTER TABLE "AWBTrackings" ADD COLUMN IF NOT EXISTS "MovementType" INT DEFAULT 0;
ALTER TABLE "AWBTrackings" ADD COLUMN IF NOT EXISTS "LocationId" BIGINT;
ALTER TABLE "AWBTrackings" ADD COLUMN IF NOT EXISTS "LocationName" VARCHAR(255);
ALTER TABLE "AWBTrackings" ADD COLUMN IF NOT EXISTS "EventCode" VARCHAR(50);
ALTER TABLE "AWBTrackings" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- PARTIES TABLE (Customers/Agents)
-- ============================================
ALTER TABLE "Parties" ADD COLUMN IF NOT EXISTS "AccountTypeId" BIGINT;
ALTER TABLE "Parties" ADD COLUMN IF NOT EXISTS "CreditLimit" DECIMAL(18,4) DEFAULT 0;
ALTER TABLE "Parties" ADD COLUMN IF NOT EXISTS "CreditPeriodDays" INT DEFAULT 0;
ALTER TABLE "Parties" ADD COLUMN IF NOT EXISTS "SLAAgreementId" BIGINT;
ALTER TABLE "Parties" ADD COLUMN IF NOT EXISTS "CustomerZoneId" BIGINT;
ALTER TABLE "Parties" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- INVOICES TABLE
-- ============================================
ALTER TABLE "Invoices" ADD COLUMN IF NOT EXISTS "InvoiceFormat" VARCHAR(50) DEFAULT 'Commercial';
ALTER TABLE "Invoices" ADD COLUMN IF NOT EXISTS "GlJournalId" BIGINT;
ALTER TABLE "Invoices" ADD COLUMN IF NOT EXISTS "PostedAt" TIMESTAMP;
ALTER TABLE "Invoices" ADD COLUMN IF NOT EXISTS "PostedByUserId" BIGINT;
ALTER TABLE "Invoices" ADD COLUMN IF NOT EXISTS "PostedByUserName" VARCHAR(255);
ALTER TABLE "Invoices" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- INVOICE DETAILS TABLE
-- ============================================
ALTER TABLE "InvoiceDetails" ADD COLUMN IF NOT EXISTS "FreightCost" DECIMAL(18,4) DEFAULT 0;
ALTER TABLE "InvoiceDetails" ADD COLUMN IF NOT EXISTS "TotalCost" DECIMAL(18,4) DEFAULT 0;
ALTER TABLE "InvoiceDetails" ADD COLUMN IF NOT EXISTS "ProfitAmount" DECIMAL(18,4) DEFAULT 0;
ALTER TABLE "InvoiceDetails" ADD COLUMN IF NOT EXISTS "ProfitPercentage" DECIMAL(10,4) DEFAULT 0;
ALTER TABLE "InvoiceDetails" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- RECEIPTS TABLE
-- ============================================
ALTER TABLE "Receipts" ADD COLUMN IF NOT EXISTS "GlJournalId" BIGINT;
ALTER TABLE "Receipts" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- COD REMITTANCES TABLE
-- ============================================
ALTER TABLE "CODRemittances" ADD COLUMN IF NOT EXISTS "GlJournalId" BIGINT;
ALTER TABLE "CODRemittances" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- JOURNALS TABLE
-- ============================================
ALTER TABLE "Journals" ADD COLUMN IF NOT EXISTS "SourceType" VARCHAR(50);
ALTER TABLE "Journals" ADD COLUMN IF NOT EXISTS "SourceId" BIGINT;
ALTER TABLE "Journals" ADD COLUMN IF NOT EXISTS "SourceRefNo" VARCHAR(100);
ALTER TABLE "Journals" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- JOURNAL ENTRIES TABLE
-- ============================================
ALTER TABLE "JournalEntries" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- RATE CARDS TABLE
-- ============================================
ALTER TABLE "RateCards" ADD COLUMN IF NOT EXISTS "ServiceTypeId" BIGINT;
ALTER TABLE "RateCards" ADD COLUMN IF NOT EXISTS "ShipmentModeId" BIGINT;
ALTER TABLE "RateCards" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- RATE CARD ZONES TABLE
-- ============================================
ALTER TABLE "RateCardZones" ADD COLUMN IF NOT EXISTS "ZoneCategoryId" BIGINT;
ALTER TABLE "RateCardZones" ADD COLUMN IF NOT EXISTS "BaseRateCost" DECIMAL(18,4) DEFAULT 0;
ALTER TABLE "RateCardZones" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- RATE CARD SLAB RULES TABLE
-- ============================================
ALTER TABLE "RateCardSlabRules" ADD COLUMN IF NOT EXISTS "RateCost" DECIMAL(18,4) DEFAULT 0;
ALTER TABLE "RateCardSlabRules" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- SHIPMENT STATUSES TABLE
-- ============================================
ALTER TABLE "ShipmentStatuses" ADD COLUMN IF NOT EXISTS "StatusGroupId" BIGINT;
ALTER TABLE "ShipmentStatuses" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- SHIPMENT STATUS GROUPS TABLE
-- ============================================
ALTER TABLE "ShipmentStatusGroups" ADD COLUMN IF NOT EXISTS "SequenceNo" INT DEFAULT 0;
ALTER TABLE "ShipmentStatusGroups" ADD COLUMN IF NOT EXISTS "IconName" VARCHAR(100);
ALTER TABLE "ShipmentStatusGroups" ADD COLUMN IF NOT EXISTS "ColorCode" VARCHAR(20);
ALTER TABLE "ShipmentStatusGroups" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- STATUS EVENT MAPPINGS TABLE (Create if not exists)
-- ============================================
CREATE TABLE IF NOT EXISTS "StatusEventMappings" (
    "Id" BIGSERIAL PRIMARY KEY,
    "CompanyId" BIGINT,
    "BranchId" BIGINT,
    "EventCode" VARCHAR(50) NOT NULL,
    "EventName" VARCHAR(100),
    "StatusId" BIGINT NOT NULL,
    "IsActive" BOOLEAN DEFAULT TRUE,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "ModifiedAt" TIMESTAMP,
    "CreatedBy" VARCHAR(255),
    "ModifiedBy" VARCHAR(255),
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    "IsDemo" BOOLEAN DEFAULT FALSE
);

-- ============================================
-- CUSTOMER ZONES TABLE
-- ============================================
ALTER TABLE "CustomerZones" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- CUSTOMER ZONE CITIES TABLE
-- ============================================
ALTER TABLE "CustomerZoneCities" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- CUSTOMER ZONE COURIERS TABLE
-- ============================================
ALTER TABLE "CustomerZoneCouriers" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- USER FAVORITES TABLE (Create if not exists)
-- ============================================
CREATE TABLE IF NOT EXISTS "UserFavorites" (
    "Id" BIGSERIAL PRIMARY KEY,
    "UserId" BIGINT NOT NULL,
    "MenuPath" VARCHAR(255) NOT NULL,
    "MenuTitle" VARCHAR(255),
    "MenuIcon" VARCHAR(100),
    "DisplayOrder" INT DEFAULT 0,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "ModifiedAt" TIMESTAMP,
    "CreatedBy" VARCHAR(255),
    "ModifiedBy" VARCHAR(255),
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    "IsDemo" BOOLEAN DEFAULT FALSE
);

-- ============================================
-- BRANCH AWB CONFIGS TABLE (Create if not exists)
-- ============================================
CREATE TABLE IF NOT EXISTS "BranchAWBConfigs" (
    "Id" BIGSERIAL PRIMARY KEY,
    "BranchId" BIGINT NOT NULL,
    "MovementType" INT NOT NULL,
    "AWBPrefix" VARCHAR(20),
    "AWBStartingNumber" BIGINT DEFAULT 1,
    "AWBIncrement" INT DEFAULT 1,
    "AWBLastUsedNumber" BIGINT DEFAULT 0,
    "IsActive" BOOLEAN DEFAULT TRUE,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "ModifiedAt" TIMESTAMP,
    "CreatedBy" VARCHAR(255),
    "ModifiedBy" VARCHAR(255),
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    "IsDemo" BOOLEAN DEFAULT FALSE
);

-- ============================================
-- ZONE CATEGORIES TABLE (Create if not exists)
-- ============================================
CREATE TABLE IF NOT EXISTS "ZoneCategories" (
    "Id" BIGSERIAL PRIMARY KEY,
    "CompanyId" BIGINT,
    "Name" VARCHAR(100) NOT NULL,
    "Code" VARCHAR(20),
    "Description" TEXT,
    "AllowCrossBranchMovement" BOOLEAN DEFAULT TRUE,
    "IsActive" BOOLEAN DEFAULT TRUE,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "ModifiedAt" TIMESTAMP,
    "CreatedBy" VARCHAR(255),
    "ModifiedBy" VARCHAR(255),
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    "IsDemo" BOOLEAN DEFAULT FALSE
);

-- ============================================
-- GL CHART OF ACCOUNTS TABLE
-- ============================================
ALTER TABLE "GLChartOfAccounts" ADD COLUMN IF NOT EXISTS "ClassificationId" BIGINT;
ALTER TABLE "GLChartOfAccounts" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- GL JOURNALS (Create if not exists)
-- ============================================
CREATE TABLE IF NOT EXISTS "GLJournals" (
    "Id" BIGSERIAL PRIMARY KEY,
    "CompanyId" BIGINT,
    "BranchId" BIGINT,
    "FinancialPeriodId" BIGINT,
    "JournalNo" VARCHAR(50),
    "TransactionDate" DATE NOT NULL,
    "Description" TEXT,
    "SourceType" VARCHAR(50),
    "SourceId" BIGINT,
    "SourceRefNo" VARCHAR(100),
    "TotalDebit" DECIMAL(18,4) DEFAULT 0,
    "TotalCredit" DECIMAL(18,4) DEFAULT 0,
    "Status" VARCHAR(20) DEFAULT 'Draft',
    "PostedAt" TIMESTAMP,
    "PostedByUserId" BIGINT,
    "PostedByUserName" VARCHAR(255),
    "IsActive" BOOLEAN DEFAULT TRUE,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "ModifiedAt" TIMESTAMP,
    "CreatedBy" VARCHAR(255),
    "ModifiedBy" VARCHAR(255),
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    "IsDemo" BOOLEAN DEFAULT FALSE
);

-- ============================================
-- GL JOURNAL ENTRIES (Create if not exists)
-- ============================================
CREATE TABLE IF NOT EXISTS "GLJournalEntries" (
    "Id" BIGSERIAL PRIMARY KEY,
    "JournalId" BIGINT NOT NULL,
    "AccountId" BIGINT NOT NULL,
    "Description" TEXT,
    "DebitAmount" DECIMAL(18,4) DEFAULT 0,
    "CreditAmount" DECIMAL(18,4) DEFAULT 0,
    "PartyId" BIGINT,
    "Reference" VARCHAR(100),
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "ModifiedAt" TIMESTAMP,
    "CreatedBy" VARCHAR(255),
    "ModifiedBy" VARCHAR(255),
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    "IsDemo" BOOLEAN DEFAULT FALSE
);

-- ============================================
-- FINANCIAL PERIODS TABLE
-- ============================================
ALTER TABLE "FinancialPeriods" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- FINANCIAL YEARS TABLE
-- ============================================
ALTER TABLE "FinancialYears" ADD COLUMN IF NOT EXISTS "OpeningBalanceJournalId" BIGINT;
ALTER TABLE "FinancialYears" ADD COLUMN IF NOT EXISTS "ClosingBalanceJournalId" BIGINT;
ALTER TABLE "FinancialYears" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- AUDIT LOGS TABLE
-- ============================================
ALTER TABLE "AuditLogs" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- USERS TABLE
-- ============================================
ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "LastLoginAt" TIMESTAMP;
ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- ROLES TABLE
-- ============================================
ALTER TABLE "Roles" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- SERVICE TYPES TABLE
-- ============================================
ALTER TABLE "ServiceTypes" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- SHIPMENT MODES TABLE
-- ============================================
ALTER TABLE "ShipmentModes" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- CURRENCIES TABLE
-- ============================================
ALTER TABLE "Currencies" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- COUNTRIES TABLE
-- ============================================
ALTER TABLE "Countries" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- STATES TABLE
-- ============================================
ALTER TABLE "States" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- CITIES TABLE
-- ============================================
ALTER TABLE "Cities" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- LOCATIONS TABLE
-- ============================================
ALTER TABLE "Locations" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- PORTS TABLE
-- ============================================
ALTER TABLE "Ports" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- VEHICLES TABLE
-- ============================================
ALTER TABLE "Vehicles" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- WAREHOUSES TABLE
-- ============================================
ALTER TABLE "Warehouses" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- MANIFESTS TABLE
-- ============================================
ALTER TABLE "Manifests" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- DRS TABLE
-- ============================================
ALTER TABLE "DRS" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- DRS DETAILS TABLE
-- ============================================
ALTER TABLE "DRSDetails" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- PICKUP REQUESTS TABLE
-- ============================================
ALTER TABLE "PickupRequests" ADD COLUMN IF NOT EXISTS "CustomerZoneId" BIGINT;
ALTER TABLE "PickupRequests" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- PREPAID AWBS TABLE
-- ============================================
ALTER TABLE "PrepaidAWBs" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- AWB STOCKS TABLE
-- ============================================
ALTER TABLE "AWBStocks" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- ACCOUNT TYPES TABLE
-- ============================================
ALTER TABLE "AccountTypes" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- OTHER CHARGE TYPES TABLE
-- ============================================
ALTER TABLE "OtherChargeTypes" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- TAX CODES TABLE
-- ============================================
ALTER TABLE "TaxCodes" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- BANK ACCOUNTS TABLE
-- ============================================
ALTER TABLE "BankAccounts" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- CASH BANK TRANSACTIONS TABLE
-- ============================================
ALTER TABLE "CashBankTransactions" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- TICKETS TABLE (CRM)
-- ============================================
ALTER TABLE "Tickets" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- TICKET COMMENTS TABLE
-- ============================================
ALTER TABLE "TicketComments" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- SLA AGREEMENTS TABLE
-- ============================================
ALTER TABLE "SLAAgreements" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- EMPOST LICENSES TABLE
-- ============================================
ALTER TABLE "EmpostLicenses" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- IMPORT SHIPMENT OTHER CHARGES TABLE
-- ============================================
ALTER TABLE "ImportShipmentOtherCharge" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- IMPORT DOCUMENTS TABLE
-- ============================================
ALTER TABLE "ImportDocuments" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- API SETTINGS TABLE
-- ============================================
ALTER TABLE "ApiSettings" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- TRANSFER ORDERS TABLE
-- ============================================
ALTER TABLE "TransferOrders" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- TRANSFER ORDER ITEMS TABLE
-- ============================================
ALTER TABLE "TransferOrderItems" ADD COLUMN IF NOT EXISTS "IsDemo" BOOLEAN DEFAULT FALSE;

-- ============================================
-- VERIFY SCHEMA SYNC
-- ============================================
-- Run this query after sync to verify column counts per table
-- SELECT table_name, COUNT(*) as column_count 
-- FROM information_schema.columns 
-- WHERE table_schema = 'public' 
-- GROUP BY table_name 
-- ORDER BY table_name;

-- ============================================
-- END OF SCHEMA SYNC SCRIPT
-- ============================================
