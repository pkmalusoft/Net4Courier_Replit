-- GL Module Database Schema for PostgreSQL
-- Run this script against your database to create required tables

-- Enable UUID extension if not already enabled
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ============================================
-- TENANT & MODULE TABLES (Multi-Tenant Support)
-- ============================================

-- Tenants table
CREATE TABLE IF NOT EXISTS "Tenants" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Name" varchar(200) NOT NULL,
    "Subdomain" varchar(100),
    "AdminEmail" varchar(256),
    "Address" text,
    "State" text,
    "Country" text,
    "TaxSystem" integer DEFAULT 0,
    "GSTIN" text,
    "TRN" varchar(50),
    "EIN" varchar(50),
    "BaseCurrencyId" uuid,
    "IsActive" boolean DEFAULT true,
    "IsDemo" boolean DEFAULT false,
    "HasConfiguredModules" boolean DEFAULT false,
    "EnableMultipleCalendars" boolean DEFAULT false,
    "SubscriptionStatus" integer DEFAULT 0,
    "SubscriptionExpiresAt" timestamptz,
    "TrialEndDate" timestamptz,
    "EmailVerifiedAt" timestamptz,
    "Notes" text,
    "CreatedAt" timestamptz DEFAULT NOW(),
    "UpdatedAt" timestamptz
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Tenants_Subdomain" ON "Tenants" ("Subdomain") WHERE "Subdomain" IS NOT NULL;

-- Modules table
CREATE TABLE IF NOT EXISTS "Modules" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Code" varchar(100) NOT NULL,
    "Name" varchar(200) NOT NULL,
    "Description" text,
    "IsCore" boolean DEFAULT false,
    "IsActive" boolean DEFAULT true,
    "DisplayOrder" integer DEFAULT 0,
    "CreatedAt" timestamptz DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Modules_Code" ON "Modules" ("Code");

-- TenantModules table
CREATE TABLE IF NOT EXISTS "TenantModules" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "TenantId" uuid NOT NULL REFERENCES "Tenants"("Id") ON DELETE CASCADE,
    "ModuleId" uuid NOT NULL REFERENCES "Modules"("Id") ON DELETE CASCADE,
    "Status" integer DEFAULT 1,
    "LicenseTier" integer DEFAULT 0,
    "EnabledFeaturesJson" varchar(4000),
    "EnabledOn" timestamptz,
    "EnabledByUserId" uuid,
    "DisabledOn" timestamptz,
    "DisabledByUserId" uuid,
    "LicenseExpiresAt" timestamptz,
    "LicenseKey" varchar(200),
    "CreatedAt" timestamptz DEFAULT NOW(),
    "UpdatedAt" timestamptz
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_TenantModules_TenantId_ModuleId" ON "TenantModules" ("TenantId", "ModuleId");

-- ============================================
-- FINANCIAL SETUP TABLES
-- ============================================

-- Currencies table
CREATE TABLE IF NOT EXISTS "Currencies" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "TenantId" uuid NOT NULL REFERENCES "Tenants"("Id") ON DELETE CASCADE,
    "Code" varchar(10) NOT NULL,
    "Name" varchar(100) NOT NULL,
    "Symbol" varchar(10),
    "DecimalPlaces" integer DEFAULT 2,
    "IsBaseCurrency" boolean DEFAULT false,
    "IsActive" boolean DEFAULT true,
    "CreatedAt" timestamptz DEFAULT NOW(),
    "UpdatedAt" timestamptz
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Currencies_TenantId_Code" ON "Currencies" ("TenantId", "Code");

-- Exchange Rates table
CREATE TABLE IF NOT EXISTS "ExchangeRates" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "TenantId" uuid NOT NULL REFERENCES "Tenants"("Id") ON DELETE CASCADE,
    "FromCurrencyId" uuid NOT NULL REFERENCES "Currencies"("Id") ON DELETE CASCADE,
    "ToCurrencyId" uuid NOT NULL REFERENCES "Currencies"("Id") ON DELETE CASCADE,
    "Rate" decimal(18, 6) NOT NULL,
    "EffectiveDate" date NOT NULL,
    "CreatedAt" timestamptz DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS "IX_ExchangeRates_TenantId_EffectiveDate" ON "ExchangeRates" ("TenantId", "EffectiveDate");

-- Financial Calendars table
CREATE TABLE IF NOT EXISTS "FinancialCalendars" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "TenantId" uuid NOT NULL REFERENCES "Tenants"("Id") ON DELETE CASCADE,
    "Code" varchar(50) NOT NULL,
    "Name" varchar(100) NOT NULL,
    "Description" varchar(500),
    "CountryCode" varchar(10),
    "IsDefault" boolean DEFAULT false,
    "IsActive" boolean DEFAULT true,
    "CreatedAt" timestamptz DEFAULT NOW(),
    "UpdatedAt" timestamptz
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_FinancialCalendars_TenantId_Code" ON "FinancialCalendars" ("TenantId", "Code");

-- Financial Periods table
CREATE TABLE IF NOT EXISTS "FinancialPeriods" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "TenantId" uuid NOT NULL REFERENCES "Tenants"("Id") ON DELETE CASCADE,
    "CalendarId" uuid REFERENCES "FinancialCalendars"("Id") ON DELETE SET NULL,
    "FiscalYear" integer NOT NULL,
    "PeriodNumber" integer NOT NULL,
    "PeriodName" varchar(100),
    "StartDate" date NOT NULL,
    "EndDate" date NOT NULL,
    "Status" integer DEFAULT 0,
    "IsClosed" boolean DEFAULT false,
    "ClosedAt" timestamptz,
    "ClosedByUserId" uuid,
    "Notes" varchar(500),
    "CreatedAt" timestamptz DEFAULT NOW(),
    "UpdatedAt" timestamptz
);

CREATE INDEX IF NOT EXISTS "IX_FinancialPeriods_TenantId_FiscalYear_PeriodNumber" ON "FinancialPeriods" ("TenantId", "FiscalYear", "PeriodNumber");

-- ============================================
-- CHART OF ACCOUNTS
-- ============================================

-- Account Classifications table
CREATE TABLE IF NOT EXISTS "AccountClassifications" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "TenantId" uuid NOT NULL REFERENCES "Tenants"("Id") ON DELETE CASCADE,
    "Name" varchar(100) NOT NULL,
    "Description" varchar(500),
    "AccountType" integer NOT NULL,
    "DisplayOrder" integer DEFAULT 0,
    "IsSystem" boolean DEFAULT false,
    "IsActive" boolean DEFAULT true,
    "CreatedAt" timestamptz DEFAULT NOW(),
    "UpdatedAt" timestamptz
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_AccountClassifications_TenantId_Name" ON "AccountClassifications" ("TenantId", "Name");

-- Chart of Accounts table
CREATE TABLE IF NOT EXISTS "ChartOfAccounts" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "TenantId" uuid NOT NULL REFERENCES "Tenants"("Id") ON DELETE CASCADE,
    "AccountCode" varchar(50) NOT NULL,
    "AccountName" varchar(200) NOT NULL,
    "AccountType" integer NOT NULL,
    "AccountClassificationId" uuid REFERENCES "AccountClassifications"("Id") ON DELETE SET NULL,
    "ParentAccountId" uuid REFERENCES "ChartOfAccounts"("Id") ON DELETE SET NULL,
    "CurrencyId" uuid REFERENCES "Currencies"("Id") ON DELETE SET NULL,
    "Description" varchar(500),
    "IsGroup" boolean DEFAULT false,
    "IsSystem" boolean DEFAULT false,
    "IsActive" boolean DEFAULT true,
    "IsBankAccount" boolean DEFAULT false,
    "IsCashAccount" boolean DEFAULT false,
    "AllowPosting" boolean DEFAULT true,
    "OpeningBalance" decimal(18, 2) DEFAULT 0,
    "CurrentBalance" decimal(18, 2) DEFAULT 0,
    "Level" integer DEFAULT 1,
    "CreatedAt" timestamptz DEFAULT NOW(),
    "UpdatedAt" timestamptz
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_ChartOfAccounts_TenantId_AccountCode" ON "ChartOfAccounts" ("TenantId", "AccountCode");

-- ============================================
-- JOURNAL ENTRIES
-- ============================================

-- Journal Entries table
CREATE TABLE IF NOT EXISTS "JournalEntries" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "TenantId" uuid NOT NULL REFERENCES "Tenants"("Id") ON DELETE CASCADE,
    "EntryNumber" varchar(50) NOT NULL,
    "EntryDate" date NOT NULL,
    "PostingDate" date,
    "FiscalYear" integer NOT NULL,
    "PeriodNumber" integer,
    "Description" varchar(500),
    "Reference" varchar(100),
    "SourceModule" varchar(50),
    "SourceDocumentId" uuid,
    "SourceDocumentNumber" varchar(50),
    "EntryType" integer DEFAULT 0,
    "Status" integer DEFAULT 0,
    "IsPosted" boolean DEFAULT false,
    "IsVoided" boolean DEFAULT false,
    "IsAdjustment" boolean DEFAULT false,
    "AdjustmentReason" varchar(500),
    "VoidReason" varchar(500),
    "VoidedAt" timestamptz,
    "VoidedByUserId" uuid,
    "PostedAt" timestamptz,
    "PostedByUserId" uuid,
    "CreatedByUserId" uuid,
    "CreatedAt" timestamptz DEFAULT NOW(),
    "UpdatedAt" timestamptz
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_JournalEntries_TenantId_EntryNumber" ON "JournalEntries" ("TenantId", "EntryNumber");
CREATE INDEX IF NOT EXISTS "IX_JournalEntries_TenantId_FiscalYear" ON "JournalEntries" ("TenantId", "FiscalYear");

-- Journal Entry Lines table
CREATE TABLE IF NOT EXISTS "JournalEntryLines" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "TenantId" uuid NOT NULL REFERENCES "Tenants"("Id") ON DELETE CASCADE,
    "JournalEntryId" uuid NOT NULL REFERENCES "JournalEntries"("Id") ON DELETE CASCADE,
    "AccountId" uuid NOT NULL REFERENCES "ChartOfAccounts"("Id") ON DELETE RESTRICT,
    "LineNumber" integer NOT NULL,
    "Description" varchar(500),
    "Debit" decimal(18, 2) DEFAULT 0,
    "Credit" decimal(18, 2) DEFAULT 0,
    "CurrencyId" uuid REFERENCES "Currencies"("Id") ON DELETE SET NULL,
    "ExchangeRate" decimal(18, 6) DEFAULT 1,
    "HomeCurrencyDebit" decimal(18, 2) DEFAULT 0,
    "HomeCurrencyCredit" decimal(18, 2) DEFAULT 0,
    "Reference" varchar(100),
    "CostCenterId" uuid,
    "ProjectId" uuid,
    "CreatedAt" timestamptz DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS "IX_JournalEntryLines_TenantId_JournalEntryId" ON "JournalEntryLines" ("TenantId", "JournalEntryId");
CREATE INDEX IF NOT EXISTS "IX_JournalEntryLines_TenantId_AccountId" ON "JournalEntryLines" ("TenantId", "AccountId");

-- ============================================
-- OPENING BALANCES
-- ============================================

-- Opening Balance Batches table
CREATE TABLE IF NOT EXISTS "OpeningBalanceBatches" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "TenantId" uuid NOT NULL REFERENCES "Tenants"("Id") ON DELETE CASCADE,
    "BatchNumber" varchar(50) NOT NULL,
    "BatchDate" date NOT NULL,
    "FiscalYear" integer NOT NULL,
    "Description" varchar(500),
    "Notes" varchar(500),
    "Status" integer DEFAULT 0,
    "IsPosted" boolean DEFAULT false,
    "PostedAt" timestamptz,
    "PostedByUserId" uuid,
    "PostedJournalId" uuid,
    "ClosingJournalId" uuid,
    "CreatedByUserId" uuid,
    "CreatedAt" timestamptz DEFAULT NOW(),
    "UpdatedAt" timestamptz
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_OpeningBalanceBatches_TenantId_BatchNumber" ON "OpeningBalanceBatches" ("TenantId", "BatchNumber");
CREATE INDEX IF NOT EXISTS "IX_OpeningBalanceBatches_TenantId_FiscalYear" ON "OpeningBalanceBatches" ("TenantId", "FiscalYear");
CREATE INDEX IF NOT EXISTS "IX_OpeningBalanceBatches_TenantId_Status" ON "OpeningBalanceBatches" ("TenantId", "Status");

-- Opening Balance Lines table
CREATE TABLE IF NOT EXISTS "OpeningBalanceLines" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "TenantId" uuid NOT NULL REFERENCES "Tenants"("Id") ON DELETE CASCADE,
    "BatchId" uuid NOT NULL REFERENCES "OpeningBalanceBatches"("Id") ON DELETE CASCADE,
    "ReferenceType" integer NOT NULL,
    "ReferenceId" uuid NOT NULL,
    "ReferenceName" varchar(200),
    "ReferenceCode" varchar(50),
    "Debit" decimal(18, 2) DEFAULT 0,
    "Credit" decimal(18, 2) DEFAULT 0,
    "CurrencyId" uuid REFERENCES "Currencies"("Id") ON DELETE SET NULL,
    "ExchangeRate" decimal(18, 6) DEFAULT 1,
    "HomeCurrencyDebit" decimal(18, 2) DEFAULT 0,
    "HomeCurrencyCredit" decimal(18, 2) DEFAULT 0,
    "Notes" varchar(500),
    "CreatedAt" timestamptz DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS "IX_OpeningBalanceLines_TenantId_BatchId" ON "OpeningBalanceLines" ("TenantId", "BatchId");
CREATE INDEX IF NOT EXISTS "IX_OpeningBalanceLines_TenantId_ReferenceType_ReferenceId" ON "OpeningBalanceLines" ("TenantId", "ReferenceType", "ReferenceId");

-- ============================================
-- MASTER DATA (Customers/Suppliers for AR/AP)
-- ============================================

-- Customer Categories table
CREATE TABLE IF NOT EXISTS "CustomerCategories" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "TenantId" uuid NOT NULL REFERENCES "Tenants"("Id") ON DELETE CASCADE,
    "Code" varchar(20) NOT NULL,
    "Name" varchar(100) NOT NULL,
    "Description" varchar(500),
    "IsActive" boolean DEFAULT true,
    "CreatedAt" timestamptz DEFAULT NOW(),
    "UpdatedAt" timestamptz
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_CustomerCategories_TenantId_Code" ON "CustomerCategories" ("TenantId", "Code");

-- Supplier Categories table
CREATE TABLE IF NOT EXISTS "SupplierCategories" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "TenantId" uuid NOT NULL REFERENCES "Tenants"("Id") ON DELETE CASCADE,
    "Code" varchar(20) NOT NULL,
    "Name" varchar(100) NOT NULL,
    "Description" varchar(500),
    "IsActive" boolean DEFAULT true,
    "CreatedAt" timestamptz DEFAULT NOW(),
    "UpdatedAt" timestamptz
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_SupplierCategories_TenantId_Code" ON "SupplierCategories" ("TenantId", "Code");

-- Customers table
CREATE TABLE IF NOT EXISTS "Customers" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "TenantId" uuid NOT NULL REFERENCES "Tenants"("Id") ON DELETE CASCADE,
    "CustomerCode" varchar(50) NOT NULL,
    "Name" varchar(200) NOT NULL,
    "Email" varchar(200) NOT NULL,
    "Phone" varchar(50) NOT NULL,
    "Address" varchar(500),
    "City" varchar(100),
    "State" varchar(100),
    "Country" varchar(100),
    "PostalCode" varchar(20),
    "TaxIdNumber" varchar(50),
    "CustomerCategoryId" uuid REFERENCES "CustomerCategories"("Id") ON DELETE SET NULL,
    "DefaultCurrencyId" uuid REFERENCES "Currencies"("Id") ON DELETE SET NULL,
    "DefaultARAccountId" uuid REFERENCES "ChartOfAccounts"("Id") ON DELETE SET NULL,
    "CreditLimit" decimal(18, 2) DEFAULT 0,
    "RequestedCreditLimit" decimal(18, 2) DEFAULT 0,
    "PaymentTermsDays" integer DEFAULT 30,
    "IsActive" boolean DEFAULT true,
    "CreatedAt" timestamptz DEFAULT NOW(),
    "UpdatedAt" timestamptz
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Customers_TenantId_CustomerCode" ON "Customers" ("TenantId", "CustomerCode");

-- Suppliers table
CREATE TABLE IF NOT EXISTS "Suppliers" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "TenantId" uuid NOT NULL REFERENCES "Tenants"("Id") ON DELETE CASCADE,
    "SupplierCode" varchar(50) NOT NULL,
    "Name" varchar(200) NOT NULL,
    "Email" varchar(200) NOT NULL,
    "Phone" varchar(50) NOT NULL,
    "Address" varchar(500),
    "City" varchar(100),
    "State" varchar(100),
    "Country" varchar(100),
    "PostalCode" varchar(20),
    "TaxIdNumber" varchar(50),
    "SupplierCategoryId" uuid REFERENCES "SupplierCategories"("Id") ON DELETE SET NULL,
    "DefaultCurrencyId" uuid REFERENCES "Currencies"("Id") ON DELETE SET NULL,
    "DefaultAPAccountId" uuid REFERENCES "ChartOfAccounts"("Id") ON DELETE SET NULL,
    "CreditLimit" decimal(18, 2) DEFAULT 0,
    "RequestedCreditLimit" decimal(18, 2) DEFAULT 0,
    "PaymentTermsDays" integer DEFAULT 30,
    "IsActive" boolean DEFAULT true,
    "CreatedAt" timestamptz DEFAULT NOW(),
    "UpdatedAt" timestamptz
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Suppliers_TenantId_SupplierCode" ON "Suppliers" ("TenantId", "SupplierCode");

-- ============================================
-- SEED DATA
-- ============================================

-- Insert AccountsFinance module
INSERT INTO "Modules" ("Id", "Code", "Name", "Description", "IsCore", "IsActive", "DisplayOrder")
VALUES (uuid_generate_v4(), 'AccountsFinance', 'Accounts & Finance', 'General Ledger, AR, AP, Period Management', true, true, 1)
ON CONFLICT ("Code") DO NOTHING;

-- ============================================
-- SINGLE TENANT SETUP (Optional)
-- ============================================
-- Uncomment and modify for single-tenant deployment

/*
-- Create a single tenant
INSERT INTO "Tenants" ("Id", "Name", "Subdomain", "IsActive", "HasConfiguredModules")
VALUES ('11111111-1111-1111-1111-111111111111', 'My Company', 'mycompany', true, true);

-- Enable AccountsFinance module for the tenant
INSERT INTO "TenantModules" ("Id", "TenantId", "ModuleId", "Status", "EnabledOn")
SELECT uuid_generate_v4(), '11111111-1111-1111-1111-111111111111', "Id", 1, NOW()
FROM "Modules" WHERE "Code" = 'AccountsFinance';

-- Create base currency
INSERT INTO "Currencies" ("Id", "TenantId", "Code", "Name", "Symbol", "IsBaseCurrency", "IsActive")
VALUES (uuid_generate_v4(), '11111111-1111-1111-1111-111111111111', 'USD', 'US Dollar', '$', true, true);

-- Create default account classifications
INSERT INTO "AccountClassifications" ("Id", "TenantId", "Name", "AccountType", "DisplayOrder", "IsSystem")
VALUES 
(uuid_generate_v4(), '11111111-1111-1111-1111-111111111111', 'Assets', 1, 1, true),
(uuid_generate_v4(), '11111111-1111-1111-1111-111111111111', 'Liabilities', 2, 2, true),
(uuid_generate_v4(), '11111111-1111-1111-1111-111111111111', 'Equity', 3, 3, true),
(uuid_generate_v4(), '11111111-1111-1111-1111-111111111111', 'Revenue', 4, 4, true),
(uuid_generate_v4(), '11111111-1111-1111-1111-111111111111', 'Expenses', 5, 5, true);
*/

-- ============================================
-- VERIFICATION
-- ============================================
-- Run this query to verify tables were created:
-- SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' ORDER BY table_name;
