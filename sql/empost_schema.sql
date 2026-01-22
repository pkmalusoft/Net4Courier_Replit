-- Empost Module Schema for Net4Courier
-- Run this script in your PostgreSQL database to create Empost tables

-- EmpostLicenses table
CREATE TABLE IF NOT EXISTS "EmpostLicenses" (
    "Id" BIGSERIAL PRIMARY KEY,
    "CompanyId" BIGINT,
    "BranchId" BIGINT,
    "LicenseNumber" VARCHAR(50) NOT NULL,
    "LicenseeName" VARCHAR(200),
    "LicenseDate" TIMESTAMP NOT NULL,
    "LicensePeriodStart" TIMESTAMP NOT NULL,
    "LicensePeriodEnd" TIMESTAMP NOT NULL,
    "AdvancePaymentDueDate" TIMESTAMP NOT NULL,
    "MinimumAdvanceAmount" DECIMAL(18,2) NOT NULL DEFAULT 100000.00,
    "RoyaltyPercentage" DECIMAL(5,2) NOT NULL DEFAULT 10.00,
    "WeightThresholdKg" DECIMAL(10,2) NOT NULL DEFAULT 30.00,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "RenewalDate" TIMESTAMP,
    "Notes" VARCHAR(1000),
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ModifiedAt" TIMESTAMP,
    "CreatedBy" INTEGER,
    "ModifiedBy" INTEGER
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_EmpostLicenses_LicenseNumber" ON "EmpostLicenses" ("LicenseNumber");
CREATE INDEX IF NOT EXISTS "IX_EmpostLicenses_CompanyId_IsActive" ON "EmpostLicenses" ("CompanyId", "IsActive");

-- EmpostAdvancePayments table
CREATE TABLE IF NOT EXISTS "EmpostAdvancePayments" (
    "Id" BIGSERIAL PRIMARY KEY,
    "EmpostLicenseId" BIGINT NOT NULL,
    "PaymentReference" VARCHAR(50) NOT NULL,
    "DueDate" TIMESTAMP NOT NULL,
    "PaymentDate" TIMESTAMP,
    "AmountDue" DECIMAL(18,2) NOT NULL DEFAULT 100000.00,
    "AmountPaid" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "ForLicenseYear" INTEGER NOT NULL,
    "LicensePeriodStart" TIMESTAMP NOT NULL,
    "LicensePeriodEnd" TIMESTAMP NOT NULL,
    "PaymentMethod" VARCHAR(100),
    "BankReference" VARCHAR(100),
    "Notes" VARCHAR(500),
    "RecordedBy" BIGINT,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ModifiedAt" TIMESTAMP,
    "CreatedBy" INTEGER,
    "ModifiedBy" INTEGER,
    CONSTRAINT "FK_EmpostAdvancePayments_License" FOREIGN KEY ("EmpostLicenseId") 
        REFERENCES "EmpostLicenses" ("Id") ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_EmpostAdvancePayments_LicenseId_Year" ON "EmpostAdvancePayments" ("EmpostLicenseId", "ForLicenseYear");

-- EmpostQuarters table
CREATE TABLE IF NOT EXISTS "EmpostQuarters" (
    "Id" BIGSERIAL PRIMARY KEY,
    "EmpostLicenseId" BIGINT NOT NULL,
    "Year" INTEGER NOT NULL,
    "Quarter" INTEGER NOT NULL,
    "QuarterName" VARCHAR(10) NOT NULL,
    "PeriodStart" TIMESTAMP NOT NULL,
    "PeriodEnd" TIMESTAMP NOT NULL,
    "SubmissionDeadline" TIMESTAMP NOT NULL,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "IsLocked" BOOLEAN NOT NULL DEFAULT FALSE,
    "LockedDate" TIMESTAMP,
    "LockedBy" BIGINT,
    "LockedByName" VARCHAR(200),
    "SubmittedDate" TIMESTAMP,
    "SubmittedBy" BIGINT,
    "SubmittedByName" VARCHAR(200),
    "TotalGrossRevenue" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "TotalTaxableRevenue" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "TotalExemptRevenue" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "TotalEmpostFee" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "TotalReturnAdjustments" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "NetEmpostFee" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "TotalShipments" INTEGER NOT NULL DEFAULT 0,
    "TaxableShipments" INTEGER NOT NULL DEFAULT 0,
    "ExemptShipments" INTEGER NOT NULL DEFAULT 0,
    "ReturnedShipments" INTEGER NOT NULL DEFAULT 0,
    "Notes" VARCHAR(1000),
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ModifiedAt" TIMESTAMP,
    "CreatedBy" INTEGER,
    "ModifiedBy" INTEGER,
    CONSTRAINT "FK_EmpostQuarters_License" FOREIGN KEY ("EmpostLicenseId") 
        REFERENCES "EmpostLicenses" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_EmpostQuarters_License_Year_Quarter" ON "EmpostQuarters" ("EmpostLicenseId", "Year", "Quarter");
CREATE INDEX IF NOT EXISTS "IX_EmpostQuarters_Status" ON "EmpostQuarters" ("Status");

-- EmpostShipmentFees table
CREATE TABLE IF NOT EXISTS "EmpostShipmentFees" (
    "Id" BIGSERIAL PRIMARY KEY,
    "InscanMasterId" BIGINT NOT NULL,
    "EmpostQuarterId" BIGINT NOT NULL,
    "AWBNumber" VARCHAR(50) NOT NULL,
    "ShipmentDate" TIMESTAMP NOT NULL,
    "ActualWeight" DECIMAL(10,3) NOT NULL DEFAULT 0,
    "ChargeableWeight" DECIMAL(10,3) NOT NULL DEFAULT 0,
    "Classification" INTEGER NOT NULL DEFAULT 0,
    "TaxabilityStatus" INTEGER NOT NULL DEFAULT 0,
    "FreightCharge" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "FuelSurcharge" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "InsuranceCharge" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "CODCharge" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "OtherCharges" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "DiscountAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "GrossAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "RoyaltyPercentage" DECIMAL(5,2) NOT NULL DEFAULT 10.00,
    "EmpostFeeAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "FeeStatus" INTEGER NOT NULL DEFAULT 0,
    "IsReturnAdjusted" BOOLEAN NOT NULL DEFAULT FALSE,
    "AdjustedDate" TIMESTAMP,
    "AdjustmentReason" VARCHAR(500),
    "Notes" VARCHAR(1000),
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ModifiedAt" TIMESTAMP,
    "CreatedBy" INTEGER,
    "ModifiedBy" INTEGER,
    CONSTRAINT "FK_EmpostShipmentFees_Quarter" FOREIGN KEY ("EmpostQuarterId") 
        REFERENCES "EmpostQuarters" ("Id") ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_EmpostShipmentFees_Quarter_Inscan" ON "EmpostShipmentFees" ("EmpostQuarterId", "InscanMasterId");
CREATE INDEX IF NOT EXISTS "IX_EmpostShipmentFees_AWBNumber" ON "EmpostShipmentFees" ("AWBNumber");
CREATE INDEX IF NOT EXISTS "IX_EmpostShipmentFees_ShipmentDate" ON "EmpostShipmentFees" ("ShipmentDate");

-- EmpostQuarterlySettlements table
CREATE TABLE IF NOT EXISTS "EmpostQuarterlySettlements" (
    "Id" BIGSERIAL PRIMARY KEY,
    "EmpostQuarterId" BIGINT NOT NULL,
    "EmpostLicenseId" BIGINT NOT NULL,
    "SettlementReference" VARCHAR(50) NOT NULL,
    "Year" INTEGER NOT NULL,
    "Quarter" INTEGER NOT NULL,
    "CumulativeFeeToDate" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "AdvancePaymentAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "PreviousSettlements" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "QuarterFeeAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "ReturnAdjustments" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "NetQuarterFee" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "ExcessOverAdvance" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "AmountPayable" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "VATOnFee" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "TotalPayable" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "AmountPaid" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "BalanceDue" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "PaymentDate" TIMESTAMP,
    "PaymentMethod" VARCHAR(100),
    "PaymentReference" VARCHAR(100),
    "SettlementDueDate" TIMESTAMP NOT NULL,
    "Notes" VARCHAR(1000),
    "RecordedBy" BIGINT,
    "RecordedDate" TIMESTAMP,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ModifiedAt" TIMESTAMP,
    "CreatedBy" INTEGER,
    "ModifiedBy" INTEGER,
    CONSTRAINT "FK_EmpostQuarterlySettlements_Quarter" FOREIGN KEY ("EmpostQuarterId") 
        REFERENCES "EmpostQuarters" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_EmpostQuarterlySettlements_License" FOREIGN KEY ("EmpostLicenseId") 
        REFERENCES "EmpostLicenses" ("Id") ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_EmpostQuarterlySettlements_License_Year_Quarter" ON "EmpostQuarterlySettlements" ("EmpostLicenseId", "Year", "Quarter");
CREATE INDEX IF NOT EXISTS "IX_EmpostQuarterlySettlements_Status" ON "EmpostQuarterlySettlements" ("Status");

-- EmpostReturnAdjustments table
CREATE TABLE IF NOT EXISTS "EmpostReturnAdjustments" (
    "Id" BIGSERIAL PRIMARY KEY,
    "EmpostShipmentFeeId" BIGINT NOT NULL,
    "EmpostQuarterId" BIGINT NOT NULL,
    "InscanMasterId" BIGINT NOT NULL,
    "AWBNumber" VARCHAR(50) NOT NULL,
    "OriginalShipmentDate" TIMESTAMP NOT NULL,
    "ReturnDate" TIMESTAMP NOT NULL,
    "AdjustmentType" INTEGER NOT NULL DEFAULT 0,
    "OriginalGrossAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "OriginalFeeAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "AdjustmentAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "Reason" VARCHAR(500) NOT NULL,
    "AppliedDate" TIMESTAMP,
    "AppliedBy" BIGINT,
    "AppliedByName" VARCHAR(200),
    "Notes" VARCHAR(1000),
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ModifiedAt" TIMESTAMP,
    "CreatedBy" INTEGER,
    "ModifiedBy" INTEGER,
    CONSTRAINT "FK_EmpostReturnAdjustments_ShipmentFee" FOREIGN KEY ("EmpostShipmentFeeId") 
        REFERENCES "EmpostShipmentFees" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_EmpostReturnAdjustments_Quarter" FOREIGN KEY ("EmpostQuarterId") 
        REFERENCES "EmpostQuarters" ("Id") ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_EmpostReturnAdjustments_Quarter_Status" ON "EmpostReturnAdjustments" ("EmpostQuarterId", "Status");
CREATE INDEX IF NOT EXISTS "IX_EmpostReturnAdjustments_AWBNumber" ON "EmpostReturnAdjustments" ("AWBNumber");

-- EmpostAuditLogs table
CREATE TABLE IF NOT EXISTS "EmpostAuditLogs" (
    "Id" BIGSERIAL PRIMARY KEY,
    "Action" INTEGER NOT NULL,
    "ActionDescription" VARCHAR(200) NOT NULL,
    "EntityType" VARCHAR(100),
    "EntityId" BIGINT,
    "EmpostLicenseId" BIGINT,
    "EmpostQuarterId" BIGINT,
    "Year" INTEGER,
    "Quarter" INTEGER,
    "AWBNumber" VARCHAR(50),
    "OldValue" DECIMAL(18,2),
    "NewValue" DECIMAL(18,2),
    "OldData" VARCHAR(500),
    "NewData" VARCHAR(500),
    "PerformedBy" BIGINT,
    "PerformedByName" VARCHAR(200),
    "PerformedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IpAddress" VARCHAR(100),
    "Notes" VARCHAR(1000),
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ModifiedAt" TIMESTAMP,
    "CreatedBy" INTEGER,
    "ModifiedBy" INTEGER
);

CREATE INDEX IF NOT EXISTS "IX_EmpostAuditLogs_License_PerformedAt" ON "EmpostAuditLogs" ("EmpostLicenseId", "PerformedAt");
CREATE INDEX IF NOT EXISTS "IX_EmpostAuditLogs_Quarter_Action" ON "EmpostAuditLogs" ("EmpostQuarterId", "Action");

-- Enum Reference (for documentation):
-- EmpostLicenseStatus: 0=Active, 1=PendingRenewal, 2=Expired, 3=Suspended
-- QuarterStatus: 0=Open, 1=PendingSubmission, 2=Submitted, 3=Locked
-- AdvancePaymentStatus: 0=Pending, 1=Paid, 2=PartiallyPaid, 3=Overdue
-- EmpostClassification: 0=Taxable, 1=Exempt, 2=FreightOver30Kg, 3=LumpSumContract, 4=Warehousing, 5=PassThrough
-- EmpostTaxabilityStatus: 0=Taxable, 1=NonTaxable
-- EmpostFeeStatus: 0=Pending, 1=Settled, 2=Credited, 3=Adjusted
-- EmpostSettlementStatus: 0=Pending, 1=PartiallyPaid, 2=Paid, 3=Waived
-- AdjustmentStatus: 0=Pending, 1=Applied, 2=Rejected
-- EmpostAdjustmentType: 0=FullRefund, 1=PartialRefund, 2=Reversal
-- EmpostAuditAction: 0=LicenseCreated, 1=LicenseUpdated, 2=LicenseRenewed, 3=AdvancePaymentRecorded, 
--                    4=QuarterLocked, 5=QuarterUnlocked, 6=QuarterSubmitted, 7=SettlementCreated,
--                    8=SettlementPaid, 9=ShipmentFeeCalculated, 10=ReturnAdjustmentCreated,
--                    11=ReturnAdjustmentApplied, 12=ReportGenerated, 13=ReconciliationPerformed,
--                    14=ClassificationOverride
