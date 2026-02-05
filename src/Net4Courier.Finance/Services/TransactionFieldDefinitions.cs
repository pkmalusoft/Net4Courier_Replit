namespace Net4Courier.Finance.Services;

public static class TransactionFieldDefinitions
{
    public class FieldDefinition
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

    private static readonly Dictionary<string, List<FieldDefinition>> _fieldsByTransactionType = new()
    {
        ["AWB_ENTRY"] = new()
        {
            new() { Code = "FREIGHT", Name = "Freight Charges", Description = "Base freight charges for shipment", SortOrder = 1 },
            new() { Code = "FUEL_SURCHARGE", Name = "Fuel Surcharge", Description = "Fuel surcharge amount", SortOrder = 2 },
            new() { Code = "OTHER_CHARGES", Name = "Other Charges", Description = "Additional service charges", SortOrder = 3 },
            new() { Code = "VAT_AMOUNT", Name = "VAT Amount", Description = "Value Added Tax", SortOrder = 4 },
            new() { Code = "COD_AMOUNT", Name = "COD Amount", Description = "Cash on Delivery collection amount", SortOrder = 5 },
            new() { Code = "TOTAL_RECEIVABLE", Name = "Total Receivable", Description = "Total amount receivable from customer", SortOrder = 6 }
        },
        
        ["CUSTOMER_INVOICE"] = new()
        {
            new() { Code = "FREIGHT", Name = "Freight Charges", Description = "Base freight charges", SortOrder = 1 },
            new() { Code = "FUEL_SURCHARGE", Name = "Fuel Surcharge", Description = "Fuel surcharge amount", SortOrder = 2 },
            new() { Code = "OTHER_CHARGES", Name = "Other Charges", Description = "Additional charges", SortOrder = 3 },
            new() { Code = "DISCOUNT", Name = "Discount", Description = "Discount amount", SortOrder = 4 },
            new() { Code = "TAX_AMOUNT", Name = "Tax Amount", Description = "Tax/VAT amount", SortOrder = 5 },
            new() { Code = "TOTAL_AMOUNT", Name = "Total Invoice Amount", Description = "Total invoiced amount", SortOrder = 6 },
            new() { Code = "ACCOUNTS_RECEIVABLE", Name = "Accounts Receivable", Description = "Customer receivable balance", SortOrder = 7 }
        },
        
        ["POD_UPDATE"] = new()
        {
            new() { Code = "COD_COLLECTED", Name = "COD Collected", Description = "Cash on Delivery amount collected", SortOrder = 1 },
            new() { Code = "DELIVERY_CHARGES", Name = "Delivery Charges", Description = "Additional delivery charges", SortOrder = 2 },
            new() { Code = "COURIER_CASH", Name = "Courier Cash", Description = "Cash held by courier", SortOrder = 3 }
        },
        
        ["PREPAID_AWB_ISSUE"] = new()
        {
            new() { Code = "SALE_AMOUNT", Name = "Sale Amount", Description = "Prepaid AWB sale value", SortOrder = 1 },
            new() { Code = "VAT_AMOUNT", Name = "VAT Amount", Description = "VAT on sale", SortOrder = 2 },
            new() { Code = "TOTAL_AMOUNT", Name = "Total Amount", Description = "Total sale including VAT", SortOrder = 3 }
        },
        
        ["CREDIT_NOTE"] = new()
        {
            new() { Code = "CREDIT_AMOUNT", Name = "Credit Amount", Description = "Credit note value", SortOrder = 1 },
            new() { Code = "TAX_ADJUSTMENT", Name = "Tax Adjustment", Description = "Tax adjustment on credit", SortOrder = 2 },
            new() { Code = "ACCOUNTS_RECEIVABLE", Name = "Accounts Receivable", Description = "AR reduction", SortOrder = 3 }
        },
        
        ["DEBIT_NOTE"] = new()
        {
            new() { Code = "DEBIT_AMOUNT", Name = "Debit Amount", Description = "Debit note value", SortOrder = 1 },
            new() { Code = "TAX_ADJUSTMENT", Name = "Tax Adjustment", Description = "Tax adjustment on debit", SortOrder = 2 },
            new() { Code = "ACCOUNTS_RECEIVABLE", Name = "Accounts Receivable", Description = "AR increase", SortOrder = 3 }
        },
        
        ["COD_REMITTANCE"] = new()
        {
            new() { Code = "COD_AMOUNT", Name = "COD Amount", Description = "COD amount being remitted", SortOrder = 1 },
            new() { Code = "COMMISSION", Name = "Commission", Description = "Commission/fee deducted", SortOrder = 2 },
            new() { Code = "NET_PAYMENT", Name = "Net Payment", Description = "Net amount paid to customer", SortOrder = 3 }
        },
        
        ["EXPENSE_ENTRY"] = new()
        {
            new() { Code = "EXPENSE_AMOUNT", Name = "Expense Amount", Description = "Base expense amount", SortOrder = 1 },
            new() { Code = "TAX_AMOUNT", Name = "Tax Amount", Description = "Tax on expense", SortOrder = 2 },
            new() { Code = "TOTAL_AMOUNT", Name = "Total Amount", Description = "Total expense including tax", SortOrder = 3 }
        },
        
        ["CASH_RECEIPT"] = new()
        {
            new() { Code = "RECEIPT_AMOUNT", Name = "Receipt Amount", Description = "Amount received", SortOrder = 1 },
            new() { Code = "DISCOUNT_ALLOWED", Name = "Discount Allowed", Description = "Discount given on collection", SortOrder = 2 },
            new() { Code = "ACCOUNTS_RECEIVABLE", Name = "Accounts Receivable", Description = "AR reduction", SortOrder = 3 },
            new() { Code = "CASH_BANK", Name = "Cash/Bank", Description = "Cash or bank account credited", SortOrder = 4 }
        },
        
        ["VENDOR_BILL"] = new()
        {
            new() { Code = "BILL_AMOUNT", Name = "Bill Amount", Description = "Base bill amount", SortOrder = 1 },
            new() { Code = "TAX_AMOUNT", Name = "Tax Amount", Description = "Tax on bill", SortOrder = 2 },
            new() { Code = "TOTAL_AMOUNT", Name = "Total Amount", Description = "Total bill including tax", SortOrder = 3 },
            new() { Code = "ACCOUNTS_PAYABLE", Name = "Accounts Payable", Description = "AP increase", SortOrder = 4 }
        },
        
        ["CASH_PAYMENT"] = new()
        {
            new() { Code = "PAYMENT_AMOUNT", Name = "Payment Amount", Description = "Amount paid", SortOrder = 1 },
            new() { Code = "ACCOUNTS_PAYABLE", Name = "Accounts Payable", Description = "AP reduction", SortOrder = 2 },
            new() { Code = "CASH_BANK", Name = "Cash/Bank", Description = "Cash or bank account debited", SortOrder = 3 }
        },
        
        ["BANK_TRANSFER"] = new()
        {
            new() { Code = "TRANSFER_AMOUNT", Name = "Transfer Amount", Description = "Amount transferred", SortOrder = 1 },
            new() { Code = "SOURCE_BANK", Name = "Source Bank", Description = "Bank account debited", SortOrder = 2 },
            new() { Code = "DESTINATION_BANK", Name = "Destination Bank", Description = "Bank account credited", SortOrder = 3 }
        }
    };

    public static List<FieldDefinition> GetFieldsForTransactionType(string transactionTypeCode)
    {
        return _fieldsByTransactionType.TryGetValue(transactionTypeCode, out var fields) 
            ? fields 
            : new List<FieldDefinition>();
    }

    public static IReadOnlyDictionary<string, List<FieldDefinition>> GetAllDefinitions() => _fieldsByTransactionType;
}
