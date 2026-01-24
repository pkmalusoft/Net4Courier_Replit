-- User-Friendly Modules Schema for Net4Courier
-- COD Remittance, Pickup Commitments, Pickup Incentives, Transfer Orders
-- Run this script after the main application schema

-- =============================================
-- COD REMITTANCE MODULE
-- =============================================

CREATE TABLE IF NOT EXISTS "CODRemittances" (
    "Id" BIGSERIAL PRIMARY KEY,
    "RemittanceNo" VARCHAR(50) NOT NULL,
    "RemittanceDate" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CompanyId" BIGINT,
    "BranchId" BIGINT,
    "FinancialYearId" BIGINT,
    "CustomerId" BIGINT NOT NULL,
    "CustomerName" VARCHAR(200),
    "CustomerCode" VARCHAR(50),
    "TotalCODAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "ServiceCharge" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "ServiceChargePercent" DECIMAL(5,2) NOT NULL DEFAULT 0,
    "TaxAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "NetPayable" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "PaidAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "BalanceAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "PaymentMode" VARCHAR(50),
    "PaymentReference" VARCHAR(100),
    "BankName" VARCHAR(100),
    "ChequeNo" VARCHAR(50),
    "ChequeDate" TIMESTAMP,
    "TransactionId" VARCHAR(100),
    "Status" INTEGER NOT NULL DEFAULT 1,
    "ApprovedAt" TIMESTAMP,
    "ApprovedByUserId" BIGINT,
    "ApprovedByUserName" VARCHAR(200),
    "PaidAt" TIMESTAMP,
    "PaidByUserId" BIGINT,
    "PaidByUserName" VARCHAR(200),
    "Remarks" VARCHAR(1000),
    "JournalId" BIGINT,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "CreatedById" BIGINT,
    "CreatedByName" VARCHAR(200),
    "UpdatedAt" TIMESTAMP,
    "UpdatedById" BIGINT,
    "UpdatedByName" VARCHAR(200),
    CONSTRAINT "UQ_CODRemittances_RemittanceNo" UNIQUE ("RemittanceNo")
);

CREATE INDEX IF NOT EXISTS "IX_CODRemittances_CustomerId_Date" ON "CODRemittances" ("CustomerId", "RemittanceDate");
CREATE INDEX IF NOT EXISTS "IX_CODRemittances_BranchId_Status" ON "CODRemittances" ("BranchId", "Status");

CREATE TABLE IF NOT EXISTS "CODRemittanceDetails" (
    "Id" BIGSERIAL PRIMARY KEY,
    "CODRemittanceId" BIGINT NOT NULL,
    "InscanMasterId" BIGINT NOT NULL,
    "AWBNo" VARCHAR(50) NOT NULL,
    "DeliveredDate" TIMESTAMP,
    "ConsigneeName" VARCHAR(200),
    "CODAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "CollectedAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "ServiceCharge" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "NetPayable" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "Remarks" VARCHAR(500),
    CONSTRAINT "FK_CODRemittanceDetails_CODRemittance" FOREIGN KEY ("CODRemittanceId") REFERENCES "CODRemittances"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_CODRemittanceDetails_InscanMaster" FOREIGN KEY ("InscanMasterId") REFERENCES "InscanMasters"("Id") ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_CODRemittanceDetails_CODRemittanceId" ON "CODRemittanceDetails" ("CODRemittanceId");
CREATE INDEX IF NOT EXISTS "IX_CODRemittanceDetails_InscanMasterId" ON "CODRemittanceDetails" ("InscanMasterId");

-- =============================================
-- PICKUP COMMITMENT MODULE
-- =============================================

CREATE TABLE IF NOT EXISTS "PickupCommitments" (
    "Id" BIGSERIAL PRIMARY KEY,
    "PickupRequestId" BIGINT NOT NULL,
    "CourierId" BIGINT NOT NULL,
    "CourierName" VARCHAR(200),
    "CommittedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ExpiresAt" TIMESTAMP NOT NULL,
    "Status" INTEGER NOT NULL DEFAULT 1,
    "ConfirmedAt" TIMESTAMP,
    "ReleasedAt" TIMESTAMP,
    "ReleaseReason" VARCHAR(500),
    "ReleasedByUserId" BIGINT,
    "ReleasedByUserName" VARCHAR(200),
    "Remarks" VARCHAR(1000),
    "BranchId" BIGINT,
    CONSTRAINT "FK_PickupCommitments_PickupRequest" FOREIGN KEY ("PickupRequestId") REFERENCES "PickupRequests"("Id") ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_PickupCommitments_PickupRequestId_Status" ON "PickupCommitments" ("PickupRequestId", "Status");
CREATE INDEX IF NOT EXISTS "IX_PickupCommitments_CourierId_CommittedAt" ON "PickupCommitments" ("CourierId", "CommittedAt");
CREATE INDEX IF NOT EXISTS "IX_PickupCommitments_ExpiresAt" ON "PickupCommitments" ("ExpiresAt");

-- =============================================
-- PICKUP INCENTIVE MODULE
-- =============================================

CREATE TABLE IF NOT EXISTS "IncentiveSchedules" (
    "Id" BIGSERIAL PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "Description" VARCHAR(500),
    "CompanyId" BIGINT,
    "BranchId" BIGINT,
    "CustomerId" BIGINT,
    "CustomerName" VARCHAR(200),
    "ZoneId" BIGINT,
    "ZoneName" VARCHAR(100),
    "CalculationType" INTEGER NOT NULL DEFAULT 1,
    "IncentiveRate" DECIMAL(18,4) NOT NULL DEFAULT 0,
    "MinWeight" DECIMAL(18,3),
    "MaxWeight" DECIMAL(18,3),
    "MinPieces" INTEGER,
    "MaxPieces" INTEGER,
    "BonusAmount" DECIMAL(18,2),
    "BonusThreshold" INTEGER,
    "EffectiveFrom" TIMESTAMP NOT NULL,
    "EffectiveTo" TIMESTAMP,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "Remarks" VARCHAR(1000)
);

CREATE INDEX IF NOT EXISTS "IX_IncentiveSchedules_CompanyId_IsActive" ON "IncentiveSchedules" ("CompanyId", "IsActive");
CREATE INDEX IF NOT EXISTS "IX_IncentiveSchedules_EffectiveFrom" ON "IncentiveSchedules" ("EffectiveFrom");

CREATE TABLE IF NOT EXISTS "IncentiveAwards" (
    "Id" BIGSERIAL PRIMARY KEY,
    "IncentiveScheduleId" BIGINT NOT NULL,
    "CourierId" BIGINT NOT NULL,
    "CourierName" VARCHAR(200),
    "PickupRequestId" BIGINT,
    "PickupNo" VARCHAR(50),
    "CustomerId" BIGINT,
    "CustomerName" VARCHAR(200),
    "AwardDate" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "Pieces" INTEGER NOT NULL DEFAULT 0,
    "Weight" DECIMAL(18,3) NOT NULL DEFAULT 0,
    "IncentiveAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "BonusAmount" DECIMAL(18,2),
    "TotalAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "Status" INTEGER NOT NULL DEFAULT 1,
    "PaidAt" TIMESTAMP,
    "PaymentReference" VARCHAR(100),
    "BranchId" BIGINT,
    "Remarks" VARCHAR(1000),
    CONSTRAINT "FK_IncentiveAwards_IncentiveSchedule" FOREIGN KEY ("IncentiveScheduleId") REFERENCES "IncentiveSchedules"("Id") ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_IncentiveAwards_CourierId_AwardDate" ON "IncentiveAwards" ("CourierId", "AwardDate");
CREATE INDEX IF NOT EXISTS "IX_IncentiveAwards_IncentiveScheduleId_Status" ON "IncentiveAwards" ("IncentiveScheduleId", "Status");

-- =============================================
-- TRANSFER ORDER MODULE
-- =============================================

CREATE TABLE IF NOT EXISTS "TransferOrders" (
    "Id" BIGSERIAL PRIMARY KEY,
    "TransferNo" VARCHAR(50) NOT NULL,
    "TransferDate" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CompanyId" BIGINT,
    "FinancialYearId" BIGINT,
    "SourceBranchId" BIGINT NOT NULL,
    "SourceBranchName" VARCHAR(200),
    "DestinationBranchId" BIGINT NOT NULL,
    "DestinationBranchName" VARCHAR(200),
    "SourceWarehouseId" BIGINT,
    "SourceWarehouseName" VARCHAR(200),
    "DestinationWarehouseId" BIGINT,
    "DestinationWarehouseName" VARCHAR(200),
    "TransferType" INTEGER NOT NULL DEFAULT 1,
    "Status" INTEGER NOT NULL DEFAULT 1,
    "TotalItems" INTEGER NOT NULL DEFAULT 0,
    "TotalPieces" INTEGER NOT NULL DEFAULT 0,
    "TotalWeight" DECIMAL(18,3) NOT NULL DEFAULT 0,
    "VehicleId" BIGINT,
    "VehicleNo" VARCHAR(50),
    "DriverName" VARCHAR(200),
    "DriverPhone" VARCHAR(50),
    "SealNo" VARCHAR(50),
    "DispatchedAt" TIMESTAMP,
    "DispatchedByUserId" BIGINT,
    "DispatchedByUserName" VARCHAR(200),
    "ReceivedAt" TIMESTAMP,
    "ReceivedByUserId" BIGINT,
    "ReceivedByUserName" VARCHAR(200),
    "ExpectedArrival" TIMESTAMP,
    "Remarks" VARCHAR(1000),
    "DispatchRemarks" VARCHAR(1000),
    "ReceiptRemarks" VARCHAR(1000),
    "ReceivedCount" INTEGER,
    "ShortCount" INTEGER,
    "DamagedCount" INTEGER,
    "ExcessCount" INTEGER,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "CreatedById" BIGINT,
    "CreatedByName" VARCHAR(200),
    "UpdatedAt" TIMESTAMP,
    "UpdatedById" BIGINT,
    "UpdatedByName" VARCHAR(200),
    CONSTRAINT "UQ_TransferOrders_TransferNo" UNIQUE ("TransferNo")
);

CREATE INDEX IF NOT EXISTS "IX_TransferOrders_SourceBranchId_Date" ON "TransferOrders" ("SourceBranchId", "TransferDate");
CREATE INDEX IF NOT EXISTS "IX_TransferOrders_DestBranchId_Status" ON "TransferOrders" ("DestinationBranchId", "Status");

CREATE TABLE IF NOT EXISTS "TransferOrderItems" (
    "Id" BIGSERIAL PRIMARY KEY,
    "TransferOrderId" BIGINT NOT NULL,
    "InscanMasterId" BIGINT,
    "AWBNo" VARCHAR(50),
    "Description" VARCHAR(500),
    "Pieces" INTEGER NOT NULL DEFAULT 1,
    "Weight" DECIMAL(18,3) NOT NULL DEFAULT 0,
    "Dimensions" VARCHAR(100),
    "Status" INTEGER NOT NULL DEFAULT 1,
    "ScannedAt" TIMESTAMP,
    "ScannedByUserId" BIGINT,
    "ScannedByUserName" VARCHAR(200),
    "LoadedAt" TIMESTAMP,
    "UnloadedAt" TIMESTAMP,
    "ReceivedAt" TIMESTAMP,
    "ReceivedRemarks" VARCHAR(500),
    "IsShort" BOOLEAN NOT NULL DEFAULT FALSE,
    "IsDamaged" BOOLEAN NOT NULL DEFAULT FALSE,
    "IsExcess" BOOLEAN NOT NULL DEFAULT FALSE,
    "DamageDescription" VARCHAR(500),
    "Remarks" VARCHAR(500),
    CONSTRAINT "FK_TransferOrderItems_TransferOrder" FOREIGN KEY ("TransferOrderId") REFERENCES "TransferOrders"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_TransferOrderItems_InscanMaster" FOREIGN KEY ("InscanMasterId") REFERENCES "InscanMasters"("Id") ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_TransferOrderItems_TransferOrderId" ON "TransferOrderItems" ("TransferOrderId");
CREATE INDEX IF NOT EXISTS "IX_TransferOrderItems_InscanMasterId" ON "TransferOrderItems" ("InscanMasterId");

CREATE TABLE IF NOT EXISTS "TransferOrderEvents" (
    "Id" BIGSERIAL PRIMARY KEY,
    "TransferOrderId" BIGINT NOT NULL,
    "EventTime" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "EventType" VARCHAR(50) NOT NULL,
    "Description" VARCHAR(500),
    "Location" VARCHAR(200),
    "BranchId" BIGINT,
    "UserId" BIGINT,
    "UserName" VARCHAR(200),
    "OldValue" VARCHAR(500),
    "NewValue" VARCHAR(500),
    CONSTRAINT "FK_TransferOrderEvents_TransferOrder" FOREIGN KEY ("TransferOrderId") REFERENCES "TransferOrders"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_TransferOrderEvents_TransferOrderId_EventTime" ON "TransferOrderEvents" ("TransferOrderId", "EventTime");

-- =============================================
-- COMMENTS FOR DOCUMENTATION
-- =============================================

COMMENT ON TABLE "CODRemittances" IS 'Cash on Delivery remittance tracking for customer payouts';
COMMENT ON TABLE "CODRemittanceDetails" IS 'Individual shipments included in COD remittance';
COMMENT ON TABLE "PickupCommitments" IS 'Courier commitment/reservation system for pickups';
COMMENT ON TABLE "IncentiveSchedules" IS 'Incentive rate schedules for courier pickup bonuses';
COMMENT ON TABLE "IncentiveAwards" IS 'Awarded incentives to couriers for pickups';
COMMENT ON TABLE "TransferOrders" IS 'Inter-hub/branch transfer order management';
COMMENT ON TABLE "TransferOrderItems" IS 'Individual items/shipments in transfer orders';
COMMENT ON TABLE "TransferOrderEvents" IS 'Event timeline for transfer order tracking';
