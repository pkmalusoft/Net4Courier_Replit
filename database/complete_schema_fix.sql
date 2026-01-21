-- Complete Schema Fix for Net4Courier Production Database
-- Run this script on production database before publishing
-- This script uses IF NOT EXISTS to safely run multiple times

-- 1. CourierLedgers table
CREATE TABLE IF NOT EXISTS "CourierLedgers" (
    "Id" BIGSERIAL PRIMARY KEY,
    "CompanyId" BIGINT,
    "BranchId" BIGINT,
    "CourierId" INTEGER NOT NULL,
    "CourierName" VARCHAR(255),
    "TransactionDate" TIMESTAMP NOT NULL,
    "EntryType" INTEGER NOT NULL DEFAULT 0,
    "DRSId" BIGINT,
    "DRSNo" VARCHAR(50),
    "DebitAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "CreditAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "RunningBalance" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "Narration" VARCHAR(500),
    "Reference" VARCHAR(100),
    "IsSettled" BOOLEAN NOT NULL DEFAULT FALSE,
    "SettledAt" TIMESTAMP,
    "SettlementRef" VARCHAR(100),
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" INTEGER,
    "UpdatedAt" TIMESTAMP,
    "UpdatedBy" INTEGER
);

-- 2. SlabRuleTemplates table
CREATE TABLE IF NOT EXISTS "SlabRuleTemplates" (
    "Id" BIGSERIAL PRIMARY KEY,
    "TemplateName" VARCHAR(255) NOT NULL,
    "Description" TEXT,
    "BaseWeight" DECIMAL(18,4) NOT NULL DEFAULT 0.5,
    "BaseRate" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "CompanyId" BIGINT,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" INTEGER,
    "UpdatedAt" TIMESTAMP,
    "UpdatedBy" INTEGER
);

-- 3. SlabRuleTemplateDetails table
CREATE TABLE IF NOT EXISTS "SlabRuleTemplateDetails" (
    "Id" BIGSERIAL PRIMARY KEY,
    "TemplateId" BIGINT NOT NULL,
    "FromWeight" DECIMAL(18,4) NOT NULL DEFAULT 0,
    "ToWeight" DECIMAL(18,4) NOT NULL DEFAULT 0,
    "IncrementWeight" DECIMAL(18,4) NOT NULL DEFAULT 0.5,
    "IncrementRate" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "CalculationMode" INTEGER NOT NULL DEFAULT 0,
    "SortOrder" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" INTEGER,
    "UpdatedAt" TIMESTAMP,
    "UpdatedBy" INTEGER,
    CONSTRAINT "FK_SlabRuleTemplateDetails_Template" FOREIGN KEY ("TemplateId") REFERENCES "SlabRuleTemplates"("Id") ON DELETE CASCADE
);

-- 4. SpecialCharges table
CREATE TABLE IF NOT EXISTS "SpecialCharges" (
    "Id" BIGSERIAL PRIMARY KEY,
    "CompanyId" BIGINT NOT NULL,
    "CustomerId" BIGINT,
    "CustomerName" VARCHAR(255),
    "ChargeName" VARCHAR(255) NOT NULL,
    "ChargeCode" VARCHAR(50),
    "ChargeType" INTEGER NOT NULL DEFAULT 0,
    "ChargeValue" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "FromDate" TIMESTAMP NOT NULL,
    "ToDate" TIMESTAMP NOT NULL,
    "IsTaxApplicable" BOOLEAN NOT NULL DEFAULT FALSE,
    "TaxPercent" DECIMAL(5,2),
    "Status" INTEGER NOT NULL DEFAULT 0,
    "ApprovedById" INTEGER,
    "ApprovedByName" VARCHAR(255),
    "ApprovedAt" TIMESTAMP,
    "Remarks" TEXT,
    "IsLocked" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" INTEGER,
    "UpdatedAt" TIMESTAMP,
    "UpdatedBy" INTEGER
);

-- 5. InvoiceSpecialCharges table
CREATE TABLE IF NOT EXISTS "InvoiceSpecialCharges" (
    "Id" BIGSERIAL PRIMARY KEY,
    "InvoiceId" BIGINT NOT NULL,
    "SpecialChargeId" BIGINT NOT NULL,
    "ChargeName" VARCHAR(255) NOT NULL,
    "ChargeType" INTEGER NOT NULL DEFAULT 0,
    "ChargeValue" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "CalculatedAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "IsTaxApplicable" BOOLEAN NOT NULL DEFAULT FALSE,
    "TaxPercent" DECIMAL(5,2),
    "TaxAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "TotalAmount" DECIMAL(18,2) NOT NULL DEFAULT 0
);

-- 6. CreditNotes table
CREATE TABLE IF NOT EXISTS "CreditNotes" (
    "Id" BIGSERIAL PRIMARY KEY,
    "CreditNoteNo" VARCHAR(50) NOT NULL,
    "CreditNoteDate" TIMESTAMP NOT NULL,
    "CompanyId" BIGINT NOT NULL,
    "BranchId" BIGINT NOT NULL,
    "CustomerId" BIGINT NOT NULL,
    "CustomerName" VARCHAR(255) NOT NULL,
    "InvoiceId" BIGINT,
    "InvoiceNo" VARCHAR(50),
    "Reason" VARCHAR(500) NOT NULL,
    "Amount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "TaxAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "TotalAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "ApprovedById" INTEGER,
    "ApprovedByName" VARCHAR(255),
    "ApprovedAt" TIMESTAMP,
    "PostedAt" TIMESTAMP,
    "CancelledAt" TIMESTAMP,
    "CancelReason" VARCHAR(500),
    "Remarks" TEXT,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" INTEGER,
    "UpdatedAt" TIMESTAMP,
    "UpdatedBy" INTEGER
);

-- 7. DebitNotes table
CREATE TABLE IF NOT EXISTS "DebitNotes" (
    "Id" BIGSERIAL PRIMARY KEY,
    "DebitNoteNo" VARCHAR(50) NOT NULL,
    "DebitNoteDate" TIMESTAMP NOT NULL,
    "CompanyId" BIGINT NOT NULL,
    "BranchId" BIGINT NOT NULL,
    "VendorId" BIGINT NOT NULL,
    "VendorName" VARCHAR(255) NOT NULL,
    "BillId" BIGINT,
    "BillNo" VARCHAR(50),
    "Reason" VARCHAR(500) NOT NULL,
    "Amount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "TaxAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "TotalAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "ApprovedById" INTEGER,
    "ApprovedByName" VARCHAR(255),
    "ApprovedAt" TIMESTAMP,
    "PostedAt" TIMESTAMP,
    "CancelledAt" TIMESTAMP,
    "CancelReason" VARCHAR(500),
    "Remarks" TEXT,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" INTEGER,
    "UpdatedAt" TIMESTAMP,
    "UpdatedBy" INTEGER
);

-- 8. Add missing columns to existing tables (safe to run multiple times)

-- Add AccountNature to Parties if missing
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Parties' AND column_name = 'AccountNature') THEN
        ALTER TABLE "Parties" ADD COLUMN "AccountNature" INTEGER DEFAULT 0;
    END IF;
END $$;

-- Add AWB auto-generation columns to Branches if missing
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Branches' AND column_name = 'AWBPrefix') THEN
        ALTER TABLE "Branches" ADD COLUMN "AWBPrefix" VARCHAR(50);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Branches' AND column_name = 'AWBStartingNumber') THEN
        ALTER TABLE "Branches" ADD COLUMN "AWBStartingNumber" BIGINT NOT NULL DEFAULT 1;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Branches' AND column_name = 'AWBIncrement') THEN
        ALTER TABLE "Branches" ADD COLUMN "AWBIncrement" INTEGER NOT NULL DEFAULT 1;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Branches' AND column_name = 'AWBLastUsedNumber') THEN
        ALTER TABLE "Branches" ADD COLUMN "AWBLastUsedNumber" BIGINT NOT NULL DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Branches' AND column_name = 'CurrencyCode') THEN
        ALTER TABLE "Branches" ADD COLUMN "CurrencyCode" VARCHAR(10);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Branches' AND column_name = 'CurrencySymbol') THEN
        ALTER TABLE "Branches" ADD COLUMN "CurrencySymbol" VARCHAR(10);
    END IF;
END $$;

-- Add Notes column to AWBOtherCharges if missing
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'AWBOtherCharges' AND column_name = 'Notes') THEN
        ALTER TABLE "AWBOtherCharges" ADD COLUMN "Notes" VARCHAR(500);
    END IF;
END $$;

-- Add RateCard cost/sales columns if missing
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'RateCardZones' AND column_name = 'CostBaseRate') THEN
        ALTER TABLE "RateCardZones" ADD COLUMN "CostBaseRate" DECIMAL(18,2) NOT NULL DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'RateCardZones' AND column_name = 'CostPerKg') THEN
        ALTER TABLE "RateCardZones" ADD COLUMN "CostPerKg" DECIMAL(18,2) NOT NULL DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'RateCardZones' AND column_name = 'SalesBaseRate') THEN
        ALTER TABLE "RateCardZones" ADD COLUMN "SalesBaseRate" DECIMAL(18,2) NOT NULL DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'RateCardZones' AND column_name = 'SalesPerKg') THEN
        ALTER TABLE "RateCardZones" ADD COLUMN "SalesPerKg" DECIMAL(18,2) NOT NULL DEFAULT 0;
    END IF;
END $$;

-- Add DRS reconciliation columns if missing
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'DRS' AND column_name = 'ExpectedTotal') THEN
        ALTER TABLE "DRS" ADD COLUMN "ExpectedTotal" DECIMAL(18,2) DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'DRS' AND column_name = 'ActualReceived') THEN
        ALTER TABLE "DRS" ADD COLUMN "ActualReceived" DECIMAL(18,2) DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'DRS' AND column_name = 'Variance') THEN
        ALTER TABLE "DRS" ADD COLUMN "Variance" DECIMAL(18,2) DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'DRS' AND column_name = 'Status') THEN
        ALTER TABLE "DRS" ADD COLUMN "Status" INTEGER DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'DRS' AND column_name = 'TotalCourierCharges') THEN
        ALTER TABLE "DRS" ADD COLUMN "TotalCourierCharges" DECIMAL(18,2) DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'DRS' AND column_name = 'TotalMaterialCost') THEN
        ALTER TABLE "DRS" ADD COLUMN "TotalMaterialCost" DECIMAL(18,2) DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'DRS' AND column_name = 'PickupCash') THEN
        ALTER TABLE "DRS" ADD COLUMN "PickupCash" DECIMAL(18,2) DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'DRS' AND column_name = 'OutstandingCollected') THEN
        ALTER TABLE "DRS" ADD COLUMN "OutstandingCollected" DECIMAL(18,2) DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'DRS' AND column_name = 'ApprovedExpenses') THEN
        ALTER TABLE "DRS" ADD COLUMN "ApprovedExpenses" DECIMAL(18,2) DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'DRS' AND column_name = 'SubmittedAt') THEN
        ALTER TABLE "DRS" ADD COLUMN "SubmittedAt" TIMESTAMP;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'DRS' AND column_name = 'ReconciledAt') THEN
        ALTER TABLE "DRS" ADD COLUMN "ReconciledAt" TIMESTAMP;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'DRS' AND column_name = 'ReconciledById') THEN
        ALTER TABLE "DRS" ADD COLUMN "ReconciledById" INTEGER;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'DRS' AND column_name = 'ReconciledByName') THEN
        ALTER TABLE "DRS" ADD COLUMN "ReconciledByName" VARCHAR(255);
    END IF;
END $$;

-- Add RTS columns to InscanMasters if missing
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'IsRTS') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "IsRTS" BOOLEAN DEFAULT FALSE;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'OriginalShipmentId') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "OriginalShipmentId" BIGINT;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'RTSChargeMode') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "RTSChargeMode" INTEGER;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'RTSCreatedAt') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "RTSCreatedAt" TIMESTAMP;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'RTSCreatedByUserId') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "RTSCreatedByUserId" BIGINT;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'RTSCreatedByUserName') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "RTSCreatedByUserName" VARCHAR(255);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'RTSReason') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "RTSReason" TEXT;
    END IF;
END $$;

-- Add MAWB columns to InscanMasters if missing
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'MAWBId') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "MAWBId" BIGINT;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'MAWBBagId') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "MAWBBagId" BIGINT;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'MAWBNo') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "MAWBNo" VARCHAR(50);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'BagNo') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "BagNo" VARCHAR(50);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'BaggedAt') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "BaggedAt" TIMESTAMP;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'BaggedByUserId') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "BaggedByUserId" BIGINT;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'BaggedByUserName') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "BaggedByUserName" VARCHAR(255);
    END IF;
END $$;

-- Add Hold columns to InscanMasters if missing
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'IsOnHold') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "IsOnHold" BOOLEAN DEFAULT FALSE;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'HoldReason') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "HoldReason" TEXT;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'HoldDate') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "HoldDate" TIMESTAMP;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'HoldByUserId') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "HoldByUserId" BIGINT;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'HoldByUserName') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "HoldByUserName" VARCHAR(255);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'HoldReleasedDate') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "HoldReleasedDate" TIMESTAMP;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'HoldReleasedByUserId') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "HoldReleasedByUserId" BIGINT;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'HoldReleasedByUserName') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "HoldReleasedByUserName" VARCHAR(255);
    END IF;
END $$;

-- Add Pickup Request columns to InscanMasters if missing
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'PickupRequestId') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "PickupRequestId" BIGINT;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'PickupRequestShipmentId') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "PickupRequestShipmentId" BIGINT;
    END IF;
END $$;

-- Add additional InscanMasters columns if missing
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'Currency') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "Currency" VARCHAR(10);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'IsActive') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "IsActive" BOOLEAN DEFAULT TRUE;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'SpotRate') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "SpotRate" DECIMAL(18,2);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'MarginPercent') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "MarginPercent" DECIMAL(18,2);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'ManifestWeight') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "ManifestWeight" DECIMAL(18,3);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'IsCashOnly') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "IsCashOnly" BOOLEAN DEFAULT FALSE;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'IsChequeOnly') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "IsChequeOnly" BOOLEAN DEFAULT FALSE;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'IsCollectMaterial') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "IsCollectMaterial" BOOLEAN DEFAULT FALSE;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'IsDOCopyBack') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "IsDOCopyBack" BOOLEAN DEFAULT FALSE;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'IsNCND') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "IsNCND" BOOLEAN DEFAULT FALSE;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'PickedBy') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "PickedBy" VARCHAR(255);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'ReceivedBy') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "ReceivedBy" VARCHAR(255);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'InscanMasters' AND column_name = 'ReceivedByEmployeeId') THEN
        ALTER TABLE "InscanMasters" ADD COLUMN "ReceivedByEmployeeId" INTEGER;
    END IF;
END $$;

-- Verify completion
SELECT 'Schema fix completed successfully!' as status;
