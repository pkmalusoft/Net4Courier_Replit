-- Cash & Bank and Journal Entry Module Database Schema
-- For single-tenant ERP migration

-- =============================================
-- BANK ACCOUNTS TABLE
-- =============================================
CREATE TABLE IF NOT EXISTS "BankAccounts" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "AccountNumber" VARCHAR(50) NOT NULL,
    "AccountName" VARCHAR(200) NOT NULL,
    "BankName" VARCHAR(200) NOT NULL,
    "BranchName" VARCHAR(200),
    "SwiftCode" VARCHAR(20),
    "IbanNumber" VARCHAR(50),
    "ChartOfAccountId" UUID NOT NULL,
    "CurrencyId" UUID,
    "OpeningBalance" DECIMAL(18,2) DEFAULT 0,
    "OpeningBalanceDate" TIMESTAMP,
    "IsActive" BOOLEAN DEFAULT TRUE,
    "DeactivatedDate" TIMESTAMP,
    "DeactivatedByUserId" UUID,
    "DeactivationReason" TEXT,
    "Notes" TEXT,
    "TenantId" UUID NOT NULL,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "CreatedBy" VARCHAR(100),
    "UpdatedBy" VARCHAR(100),
    CONSTRAINT "FK_BankAccounts_ChartOfAccounts" FOREIGN KEY ("ChartOfAccountId") 
        REFERENCES "ChartOfAccounts"("Id"),
    CONSTRAINT "FK_BankAccounts_Currencies" FOREIGN KEY ("CurrencyId") 
        REFERENCES "Currencies"("Id")
);

CREATE INDEX IF NOT EXISTS "IX_BankAccounts_TenantId" ON "BankAccounts"("TenantId");
CREATE INDEX IF NOT EXISTS "IX_BankAccounts_ChartOfAccountId" ON "BankAccounts"("ChartOfAccountId");

-- =============================================
-- CASH & BANK TRANSACTIONS TABLE
-- =============================================
CREATE TABLE IF NOT EXISTS "CashBankTransactions" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "VoucherNo" VARCHAR(50) NOT NULL,
    "VoucherDate" TIMESTAMP NOT NULL,
    "TransactionType" INTEGER NOT NULL, -- 0=Receipt, 1=Payment
    "RecPayType" INTEGER NOT NULL, -- 0=Cash, 1=Bank, 2=Cheque
    "TransactionCategory" INTEGER NOT NULL, -- 0=Regular, 1=CustomerReceipt, 2=VendorPayment
    "SourceAccountId" UUID NOT NULL,
    "BankAccountId" UUID,
    "TotalAmount" DECIMAL(18,2) NOT NULL,
    "ChequeNo" VARCHAR(50),
    "ChequeDate" TIMESTAMP,
    "IsPDC" BOOLEAN DEFAULT FALSE,
    "BankName" VARCHAR(200),
    "BranchName" VARCHAR(200),
    "ReferenceNo" VARCHAR(100),
    "Status" INTEGER DEFAULT 0, -- 0=Draft, 1=Posted, 2=Voided
    "ReceiptType" INTEGER DEFAULT 0,
    "CustomerId" UUID,
    "VendorId" UUID,
    "DepositStatus" INTEGER DEFAULT 0,
    "ActualDepositDate" TIMESTAMP,
    "ClearanceStatus" INTEGER DEFAULT 0,
    "ClearanceDate" TIMESTAMP,
    "BouncedReason" TEXT,
    "PostedDate" TIMESTAMP,
    "PostedByUserId" UUID,
    "IsVoided" BOOLEAN DEFAULT FALSE,
    "VoidedDate" TIMESTAMP,
    "VoidedByUserId" UUID,
    "VoidReason" TEXT,
    "TDSAmount" DECIMAL(18,2) DEFAULT 0,
    "TDSPercent" DECIMAL(5,2) DEFAULT 0,
    "TDSCertificateNo" VARCHAR(50),
    "ServiceContractId" UUID,
    "JournalEntryId" UUID,
    "BranchId" UUID,
    "DepartmentId" UUID,
    "FiscalYear" INTEGER NOT NULL,
    "TenantId" UUID NOT NULL,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "CreatedBy" VARCHAR(100),
    "UpdatedBy" VARCHAR(100),
    CONSTRAINT "FK_CashBankTransactions_SourceAccount" FOREIGN KEY ("SourceAccountId") 
        REFERENCES "ChartOfAccounts"("Id"),
    CONSTRAINT "FK_CashBankTransactions_BankAccount" FOREIGN KEY ("BankAccountId") 
        REFERENCES "BankAccounts"("Id"),
    CONSTRAINT "FK_CashBankTransactions_Customer" FOREIGN KEY ("CustomerId") 
        REFERENCES "Customers"("Id"),
    CONSTRAINT "FK_CashBankTransactions_Vendor" FOREIGN KEY ("VendorId") 
        REFERENCES "Suppliers"("Id")
);

CREATE INDEX IF NOT EXISTS "IX_CashBankTransactions_TenantId" ON "CashBankTransactions"("TenantId");
CREATE INDEX IF NOT EXISTS "IX_CashBankTransactions_VoucherDate" ON "CashBankTransactions"("VoucherDate");
CREATE INDEX IF NOT EXISTS "IX_CashBankTransactions_VoucherNo" ON "CashBankTransactions"("VoucherNo");
CREATE INDEX IF NOT EXISTS "IX_CashBankTransactions_Status" ON "CashBankTransactions"("Status");
CREATE INDEX IF NOT EXISTS "IX_CashBankTransactions_CustomerId" ON "CashBankTransactions"("CustomerId");
CREATE INDEX IF NOT EXISTS "IX_CashBankTransactions_VendorId" ON "CashBankTransactions"("VendorId");

-- =============================================
-- CASH & BANK TRANSACTION LINES TABLE
-- =============================================
CREATE TABLE IF NOT EXISTS "CashBankTransactionLines" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "CashBankTransactionId" UUID NOT NULL,
    "AccountId" UUID NOT NULL,
    "Description" TEXT,
    "Amount" DECIMAL(18,2) NOT NULL,
    "TaxCodeId" UUID,
    "TaxAmount" DECIMAL(18,2) DEFAULT 0,
    "TenantId" UUID NOT NULL,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "CreatedBy" VARCHAR(100),
    "UpdatedBy" VARCHAR(100),
    CONSTRAINT "FK_CashBankTransactionLines_Transaction" FOREIGN KEY ("CashBankTransactionId") 
        REFERENCES "CashBankTransactions"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_CashBankTransactionLines_Account" FOREIGN KEY ("AccountId") 
        REFERENCES "ChartOfAccounts"("Id")
);

CREATE INDEX IF NOT EXISTS "IX_CashBankTransactionLines_TransactionId" ON "CashBankTransactionLines"("CashBankTransactionId");
CREATE INDEX IF NOT EXISTS "IX_CashBankTransactionLines_AccountId" ON "CashBankTransactionLines"("AccountId");

-- =============================================
-- INVOICE PAYMENT ALLOCATIONS TABLE
-- =============================================
CREATE TABLE IF NOT EXISTS "InvoicePaymentAllocations" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ReceiptId" UUID NOT NULL,
    "InvoiceId" UUID NOT NULL,
    "AllocatedAmount" DECIMAL(18,2) NOT NULL,
    "AllocationDate" TIMESTAMP NOT NULL,
    "Notes" TEXT,
    "TenantId" UUID NOT NULL,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    CONSTRAINT "FK_InvoicePaymentAllocations_Receipt" FOREIGN KEY ("ReceiptId") 
        REFERENCES "CashBankTransactions"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_InvoicePaymentAllocations_Invoice" FOREIGN KEY ("InvoiceId") 
        REFERENCES "Invoices"("Id")
);

CREATE INDEX IF NOT EXISTS "IX_InvoicePaymentAllocations_ReceiptId" ON "InvoicePaymentAllocations"("ReceiptId");
CREATE INDEX IF NOT EXISTS "IX_InvoicePaymentAllocations_InvoiceId" ON "InvoicePaymentAllocations"("InvoiceId");

-- =============================================
-- BILL PAYMENT ALLOCATIONS TABLE
-- =============================================
CREATE TABLE IF NOT EXISTS "BillPaymentAllocations" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "PaymentId" UUID NOT NULL,
    "BillId" UUID NOT NULL,
    "AllocatedAmount" DECIMAL(18,2) NOT NULL,
    "AllocationDate" TIMESTAMP NOT NULL,
    "Notes" TEXT,
    "TenantId" UUID NOT NULL,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    CONSTRAINT "FK_BillPaymentAllocations_Payment" FOREIGN KEY ("PaymentId") 
        REFERENCES "CashBankTransactions"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_BillPaymentAllocations_Bill" FOREIGN KEY ("BillId") 
        REFERENCES "Bills"("Id")
);

CREATE INDEX IF NOT EXISTS "IX_BillPaymentAllocations_PaymentId" ON "BillPaymentAllocations"("PaymentId");
CREATE INDEX IF NOT EXISTS "IX_BillPaymentAllocations_BillId" ON "BillPaymentAllocations"("BillId");

-- =============================================
-- VOUCHER ATTACHMENTS TABLE
-- =============================================
CREATE TABLE IF NOT EXISTS "VoucherAttachments" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "TransactionId" UUID NOT NULL,
    "FileName" VARCHAR(255) NOT NULL,
    "ContentType" VARCHAR(100),
    "FileSize" BIGINT,
    "FileData" BYTEA,
    "TenantId" UUID NOT NULL,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(100),
    CONSTRAINT "FK_VoucherAttachments_Transaction" FOREIGN KEY ("TransactionId") 
        REFERENCES "CashBankTransactions"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_VoucherAttachments_TransactionId" ON "VoucherAttachments"("TransactionId");

-- =============================================
-- JOURNAL ENTRIES TABLE
-- =============================================
CREATE TABLE IF NOT EXISTS "JournalEntries" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "VoucherNo" VARCHAR(50) NOT NULL,
    "VoucherDate" TIMESTAMP NOT NULL,
    "Reference" VARCHAR(100),
    "Description" TEXT,
    "TotalDebit" DECIMAL(18,2) NOT NULL,
    "TotalCredit" DECIMAL(18,2) NOT NULL,
    "Status" INTEGER DEFAULT 0, -- 0=Draft, 1=Posted, 2=Voided
    "PostedDate" TIMESTAMP,
    "PostedByUserId" UUID,
    "IsVoided" BOOLEAN DEFAULT FALSE,
    "VoidedDate" TIMESTAMP,
    "VoidedByUserId" UUID,
    "VoidReason" TEXT,
    "SourceType" VARCHAR(50), -- CashBank, Invoice, Bill, Manual
    "SourceId" UUID,
    "BranchId" UUID,
    "DepartmentId" UUID,
    "FiscalYear" INTEGER NOT NULL,
    "TenantId" UUID NOT NULL,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "CreatedBy" VARCHAR(100),
    "UpdatedBy" VARCHAR(100)
);

CREATE INDEX IF NOT EXISTS "IX_JournalEntries_TenantId" ON "JournalEntries"("TenantId");
CREATE INDEX IF NOT EXISTS "IX_JournalEntries_VoucherDate" ON "JournalEntries"("VoucherDate");
CREATE INDEX IF NOT EXISTS "IX_JournalEntries_VoucherNo" ON "JournalEntries"("VoucherNo");
CREATE INDEX IF NOT EXISTS "IX_JournalEntries_Status" ON "JournalEntries"("Status");
CREATE INDEX IF NOT EXISTS "IX_JournalEntries_SourceId" ON "JournalEntries"("SourceId");

-- =============================================
-- JOURNAL ENTRY LINES TABLE
-- =============================================
CREATE TABLE IF NOT EXISTS "JournalEntryLines" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "JournalEntryId" UUID NOT NULL,
    "AccountId" UUID NOT NULL,
    "Description" TEXT,
    "DebitAmount" DECIMAL(18,2) DEFAULT 0,
    "CreditAmount" DECIMAL(18,2) DEFAULT 0,
    "ProjectId" UUID,
    "CostCenterId" UUID,
    "TenantId" UUID NOT NULL,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    CONSTRAINT "FK_JournalEntryLines_JournalEntry" FOREIGN KEY ("JournalEntryId") 
        REFERENCES "JournalEntries"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_JournalEntryLines_Account" FOREIGN KEY ("AccountId") 
        REFERENCES "ChartOfAccounts"("Id"),
    CONSTRAINT "FK_JournalEntryLines_Project" FOREIGN KEY ("ProjectId") 
        REFERENCES "Projects"("Id")
);

CREATE INDEX IF NOT EXISTS "IX_JournalEntryLines_JournalEntryId" ON "JournalEntryLines"("JournalEntryId");
CREATE INDEX IF NOT EXISTS "IX_JournalEntryLines_AccountId" ON "JournalEntryLines"("AccountId");

-- =============================================
-- SINGLE-TENANT ADAPTATION NOTES
-- =============================================
-- For single-tenant deployment, you can:
-- 1. Remove TenantId columns if not needed
-- 2. Remove tenant-based indexes
-- 3. Simplify foreign key constraints
-- 
-- To remove TenantId (optional):
-- ALTER TABLE "BankAccounts" DROP COLUMN "TenantId";
-- ALTER TABLE "CashBankTransactions" DROP COLUMN "TenantId";
-- etc.
