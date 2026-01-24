-- Bank Accounts and Tax Rates Schema Updates
-- Run this script to add the required tables and columns

-- BankAccounts table with all columns
CREATE TABLE IF NOT EXISTS "BankAccounts" (
    "Id" BIGSERIAL PRIMARY KEY,
    "CompanyId" BIGINT,
    "BranchId" BIGINT,
    "AccountNumber" VARCHAR(50) NOT NULL,
    "AccountName" VARCHAR(200) NOT NULL,
    "BankName" VARCHAR(200) NOT NULL,
    "BranchName" VARCHAR(200),
    "SwiftCode" VARCHAR(20),
    "IbanNumber" VARCHAR(50),
    "AccountHeadId" BIGINT,
    "CurrencyId" BIGINT,
    "OpeningBalance" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "OpeningBalanceDate" TIMESTAMP,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "Notes" TEXT,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(100),
    "CreatedByName" VARCHAR(100),
    "ModifiedAt" TIMESTAMP,
    "ModifiedBy" VARCHAR(100),
    "ModifiedByName" VARCHAR(100),
    CONSTRAINT "FK_BankAccounts_Currencies" FOREIGN KEY ("CurrencyId") REFERENCES "Currencies"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_BankAccounts_AccountHeads" FOREIGN KEY ("AccountHeadId") REFERENCES "AccountHeads"("Id") ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "IX_BankAccounts_CurrencyId" ON "BankAccounts"("CurrencyId");
CREATE INDEX IF NOT EXISTS "IX_BankAccounts_AccountHeadId" ON "BankAccounts"("AccountHeadId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_BankAccounts_AccountNumber" ON "BankAccounts"("AccountNumber");

-- Add missing columns to BankAccounts if they don't exist (for existing installations)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'BankAccounts' AND column_name = 'CompanyId') THEN
        ALTER TABLE "BankAccounts" ADD COLUMN "CompanyId" BIGINT;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'BankAccounts' AND column_name = 'BranchId') THEN
        ALTER TABLE "BankAccounts" ADD COLUMN "BranchId" BIGINT;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'BankAccounts' AND column_name = 'Notes') THEN
        ALTER TABLE "BankAccounts" ADD COLUMN "Notes" TEXT;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'BankAccounts' AND column_name = 'CreatedByName') THEN
        ALTER TABLE "BankAccounts" ADD COLUMN "CreatedByName" VARCHAR(100);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'BankAccounts' AND column_name = 'ModifiedByName') THEN
        ALTER TABLE "BankAccounts" ADD COLUMN "ModifiedByName" VARCHAR(100);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'BankAccounts' AND column_name = 'IbanNumber') THEN
        IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'BankAccounts' AND column_name = 'IBAN') THEN
            ALTER TABLE "BankAccounts" RENAME COLUMN "IBAN" TO "IbanNumber";
        ELSE
            ALTER TABLE "BankAccounts" ADD COLUMN "IbanNumber" VARCHAR(50);
        END IF;
    END IF;
END $$;

-- TaxRates table
CREATE TABLE IF NOT EXISTS "TaxRates" (
    "Id" BIGSERIAL PRIMARY KEY,
    "Code" VARCHAR(50) NOT NULL,
    "Name" VARCHAR(200) NOT NULL,
    "Description" TEXT,
    "Rate" DECIMAL(10,4) NOT NULL DEFAULT 0,
    "IsDefault" BOOLEAN NOT NULL DEFAULT FALSE,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "EffectiveFrom" TIMESTAMP,
    "EffectiveTo" TIMESTAMP,
    "AccountHeadId" BIGINT,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(100),
    "CreatedByName" VARCHAR(100),
    "ModifiedAt" TIMESTAMP,
    "ModifiedBy" VARCHAR(100),
    "ModifiedByName" VARCHAR(100),
    CONSTRAINT "FK_TaxRates_AccountHeads" FOREIGN KEY ("AccountHeadId") REFERENCES "AccountHeads"("Id") ON DELETE SET NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_TaxRates_Code" ON "TaxRates"("Code");
CREATE INDEX IF NOT EXISTS "IX_TaxRates_AccountHeadId" ON "TaxRates"("AccountHeadId");

-- Seed default tax rates
INSERT INTO "TaxRates" ("Code", "Name", "Rate", "IsDefault", "IsActive", "CreatedAt")
SELECT 'VAT5', 'VAT 5%', 5.0, TRUE, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "TaxRates" WHERE "Code" = 'VAT5');

INSERT INTO "TaxRates" ("Code", "Name", "Rate", "IsDefault", "IsActive", "CreatedAt")
SELECT 'VAT0', 'Zero Rated', 0.0, FALSE, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "TaxRates" WHERE "Code" = 'VAT0');

INSERT INTO "TaxRates" ("Code", "Name", "Rate", "IsDefault", "IsActive", "CreatedAt")
SELECT 'EXEMPT', 'Tax Exempt', 0.0, FALSE, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "TaxRates" WHERE "Code" = 'EXEMPT');

-- BankReconciliations table (if not exists)
CREATE TABLE IF NOT EXISTS "BankReconciliations" (
    "Id" BIGSERIAL PRIMARY KEY,
    "ReconciliationNumber" VARCHAR(50) NOT NULL,
    "BankAccountId" BIGINT,
    "StatementDate" DATE NOT NULL,
    "StatementOpeningBalance" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "StatementClosingBalance" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "BookOpeningBalance" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "BookClosingBalance" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "Status" VARCHAR(50) NOT NULL DEFAULT 'Draft',
    "Notes" TEXT,
    "CompletedAt" TIMESTAMP,
    "CompletedBy" VARCHAR(100),
    "LockedAt" TIMESTAMP,
    "LockedBy" VARCHAR(100),
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(100),
    "CreatedByName" VARCHAR(100),
    "ModifiedAt" TIMESTAMP,
    "ModifiedBy" VARCHAR(100),
    "ModifiedByName" VARCHAR(100),
    CONSTRAINT "FK_BankReconciliations_BankAccounts" FOREIGN KEY ("BankAccountId") REFERENCES "BankAccounts"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_BankReconciliations_BankAccountId" ON "BankReconciliations"("BankAccountId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_BankReconciliations_Number" ON "BankReconciliations"("ReconciliationNumber");

-- BankStatementImports table
CREATE TABLE IF NOT EXISTS "BankStatementImports" (
    "Id" BIGSERIAL PRIMARY KEY,
    "ReconciliationId" BIGINT NOT NULL,
    "FileName" VARCHAR(255) NOT NULL,
    "ImportedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ImportedBy" VARCHAR(100),
    "TotalRows" INTEGER NOT NULL DEFAULT 0,
    "ProcessedRows" INTEGER NOT NULL DEFAULT 0,
    "ErrorRows" INTEGER NOT NULL DEFAULT 0,
    "Status" VARCHAR(50) NOT NULL DEFAULT 'Pending',
    CONSTRAINT "FK_BankStatementImports_Reconciliations" FOREIGN KEY ("ReconciliationId") REFERENCES "BankReconciliations"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_BankStatementImports_ReconciliationId" ON "BankStatementImports"("ReconciliationId");

-- BankStatementLines table
CREATE TABLE IF NOT EXISTS "BankStatementLines" (
    "Id" BIGSERIAL PRIMARY KEY,
    "ImportId" BIGINT NOT NULL,
    "TransactionDate" DATE NOT NULL,
    "Description" VARCHAR(500),
    "Reference" VARCHAR(100),
    "ChequeNumber" VARCHAR(50),
    "DebitAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "CreditAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "Balance" DECIMAL(18,2),
    "IsMatched" BOOLEAN NOT NULL DEFAULT FALSE,
    "MatchedAt" TIMESTAMP,
    CONSTRAINT "FK_BankStatementLines_Imports" FOREIGN KEY ("ImportId") REFERENCES "BankStatementImports"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_BankStatementLines_ImportId" ON "BankStatementLines"("ImportId");
CREATE INDEX IF NOT EXISTS "IX_BankStatementLines_TransactionDate" ON "BankStatementLines"("TransactionDate");

-- ReconciliationMatches table
CREATE TABLE IF NOT EXISTS "ReconciliationMatches" (
    "Id" BIGSERIAL PRIMARY KEY,
    "ReconciliationId" BIGINT NOT NULL,
    "StatementLineId" BIGINT NOT NULL,
    "VoucherId" BIGINT,
    "MatchType" VARCHAR(50) NOT NULL,
    "ConfidenceScore" DECIMAL(5,2),
    "MatchedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "MatchedBy" VARCHAR(100),
    "IsManual" BOOLEAN NOT NULL DEFAULT FALSE,
    CONSTRAINT "FK_ReconciliationMatches_Reconciliations" FOREIGN KEY ("ReconciliationId") REFERENCES "BankReconciliations"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ReconciliationMatches_StatementLines" FOREIGN KEY ("StatementLineId") REFERENCES "BankStatementLines"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_ReconciliationMatches_ReconciliationId" ON "ReconciliationMatches"("ReconciliationId");
CREATE INDEX IF NOT EXISTS "IX_ReconciliationMatches_StatementLineId" ON "ReconciliationMatches"("StatementLineId");

-- ReconciliationAdjustments table
CREATE TABLE IF NOT EXISTS "ReconciliationAdjustments" (
    "Id" BIGSERIAL PRIMARY KEY,
    "ReconciliationId" BIGINT NOT NULL,
    "AdjustmentType" VARCHAR(50) NOT NULL,
    "Description" VARCHAR(500),
    "Amount" DECIMAL(18,2) NOT NULL,
    "TransactionDate" DATE NOT NULL,
    "AccountHeadId" BIGINT,
    "IsPosted" BOOLEAN NOT NULL DEFAULT FALSE,
    "PostedAt" TIMESTAMP,
    "PostedBy" VARCHAR(100),
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(100),
    CONSTRAINT "FK_ReconciliationAdjustments_Reconciliations" FOREIGN KEY ("ReconciliationId") REFERENCES "BankReconciliations"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_ReconciliationAdjustments_ReconciliationId" ON "ReconciliationAdjustments"("ReconciliationId");
