-- Missing Tables for Net4Courier Production Database
-- Run this script on production database before publishing

-- CourierLedgers table
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
    "UpdatedBy" INTEGER,
    CONSTRAINT "FK_CourierLedgers_DRS" FOREIGN KEY ("DRSId") REFERENCES "DRS"("Id")
);

-- SlabRuleTemplates table
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

-- SlabRuleTemplateDetails table
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

-- SpecialCharges table
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

-- InvoiceSpecialCharges table
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
    "TotalAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    CONSTRAINT "FK_InvoiceSpecialCharges_Invoice" FOREIGN KEY ("InvoiceId") REFERENCES "Invoices"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_InvoiceSpecialCharges_SpecialCharge" FOREIGN KEY ("SpecialChargeId") REFERENCES "SpecialCharges"("Id")
);

-- CreditNotes table (if missing)
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
    "UpdatedBy" INTEGER,
    CONSTRAINT "FK_CreditNotes_Company" FOREIGN KEY ("CompanyId") REFERENCES "Companies"("Id"),
    CONSTRAINT "FK_CreditNotes_Branch" FOREIGN KEY ("BranchId") REFERENCES "Branches"("Id"),
    CONSTRAINT "FK_CreditNotes_Customer" FOREIGN KEY ("CustomerId") REFERENCES "Parties"("Id"),
    CONSTRAINT "FK_CreditNotes_Invoice" FOREIGN KEY ("InvoiceId") REFERENCES "Invoices"("Id")
);

-- DebitNotes table (if missing)
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
    "UpdatedBy" INTEGER,
    CONSTRAINT "FK_DebitNotes_Company" FOREIGN KEY ("CompanyId") REFERENCES "Companies"("Id"),
    CONSTRAINT "FK_DebitNotes_Branch" FOREIGN KEY ("BranchId") REFERENCES "Branches"("Id"),
    CONSTRAINT "FK_DebitNotes_Vendor" FOREIGN KEY ("VendorId") REFERENCES "Parties"("Id")
);
