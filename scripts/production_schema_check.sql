-- ==============================================
-- PRODUCTION DATABASE SCHEMA VERIFICATION SCRIPT
-- Net4Courier - Run against Gateex Production DB
-- ==============================================

-- 1. CHECK FOR MISSING TABLES
-- Compare against expected tables from development
-- If any table is missing, this query will show it

WITH expected_tables AS (
    SELECT unnest(ARRAY[
        'AWBOtherCharges', 'AWBStocks', 'AWBTrackings', 'AccountClassifications', 
        'AccountHeads', 'AccountTypes', 'ApiSettings', 'AuditLogs', 'BankAccounts',
        'BankReconciliations', 'BankStatementImports', 'BankStatementLines',
        'BranchAWBConfigs', 'Branches', 'CODRemittanceDetails', 'CODRemittances',
        'CashBankTransactionLines', 'CashBankTransactions', 'ChartOfAccounts',
        'Cities', 'Companies', 'Complaints', 'Countries', 'Currencies',
        'CustomerContractDetails', 'CustomerContracts', 'CustomerSLAs', 'CustomerZones',
        'Customers', 'DRSDetails', 'DRSMasters', 'DeliveryStatus', 'Employees',
        'EmpostApplications', 'EmpostDocuments', 'EmpostLicenses', 'FavouriteMenuItems',
        'FinancialPeriods', 'FinancialYears', 'FreightPricingDetails', 'FreightPricingMasters',
        'FreightSlabRules', 'GLAccountMappings', 'GoodsDescriptions', 'InscanDetails',
        'InscanMasters', 'InvoiceDetails', 'InvoiceMasters', 'JournalEntries',
        'JournalLines', 'KnowledgeBaseArticles', 'MAWBDetails', 'MAWBMasters',
        'ManifestDetails', 'ManifestMasters', 'MenuItems', 'NonDeliveryReasons',
        'OtherChargeTypes', 'OutscanDetails', 'OutscanMasters', 'PackageTypes',
        'PartyAddresses', 'Parties', 'PaymentModes', 'PickupDetails', 'PickupMasters',
        'Ports', 'PrepaidAWBSaleDetails', 'PrepaidAWBSaleMasters', 'RateCardMasters',
        'ReceiptDetails', 'ReceiptMasters', 'ReconciliationItems', 'ReconciliationMatches',
        'ReconciliationRules', 'Roles', 'ServiceTypes', 'ShipmentModes', 'ShipmentStatuses',
        'ShipmentStatusGroups', 'States', 'StatusEventMappings', 'Subscriptions',
        'TaxRates', 'Tenants', 'TransferOrderDetails', 'TransferOrderMasters',
        'UserRoles', 'Users', 'Zones', 'ZoneCategories', 'ZoneMappings'
    ]) AS table_name
),
existing_tables AS (
    SELECT table_name 
    FROM information_schema.tables 
    WHERE table_schema = 'public' AND table_type = 'BASE TABLE'
)
SELECT e.table_name AS missing_table
FROM expected_tables e
LEFT JOIN existing_tables t ON e.table_name = t.table_name
WHERE t.table_name IS NULL
ORDER BY e.table_name;

-- 2. LIST ALL TABLES IN PRODUCTION (for reference)
SELECT table_name, 
       (SELECT COUNT(*) FROM information_schema.columns c WHERE c.table_name = t.table_name AND c.table_schema = 'public') as column_count
FROM information_schema.tables t
WHERE table_schema = 'public' AND table_type = 'BASE TABLE'
ORDER BY table_name;

-- 3. FULL SCHEMA DUMP FOR COMPARISON
-- Export this and compare with development
SELECT table_name, column_name, data_type, is_nullable, 
       COALESCE(character_maximum_length::text, '') as max_length
FROM information_schema.columns
WHERE table_schema = 'public'
ORDER BY table_name, ordinal_position;
