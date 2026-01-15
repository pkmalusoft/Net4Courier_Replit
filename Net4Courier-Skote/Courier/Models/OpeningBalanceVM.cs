using System;
using System.Collections.Generic;

namespace Net4Courier.Models
{
    public class OpeningBalanceVM
    {

        public int AcHeadID { get; set; }
        public int AcFinancialYearID { get; set; }
        public int CrDr { get; set; }
        public decimal Amount { get; set; }
        public string AcHead { get; set; }
        public DateTime Opdate { get; set; }

        public string Remarks { get; set; }
        public int? PartyId { get; set; }
        public string StatusSDSC { get; set; }
        public string BranchId { get; set; }

    }

    public class OpeningInvoiceSearch
    {
        public string OpeningDate { get; set; }
        public string InvoiceType { get; set; }
        public int PartyId { get; set; }
        public string PartyName { get; set; }
        public string Remarks { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }

    public class YearEndProcessSearch
    {
        public string CurrentYear { get; set; }
        public string CurrentStartDate { get; set; }
        public string CurrentEndDate { get; set; }
        
        public string NextYear { get; set; }
        public string NextYearStartDate { get; set; }
        public string NextYearEndDate { get; set; }
        public int CurrentFinancialYearId { get; set; }        
        public int NextFinancialYearId { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string ProcessType { get; set; }
        public string Remarks { get; set; }
        public string Comments { get; set; }
        public string CustomerType { get; set; }
        public int SupplierTypeID { get; set; }
        public string Status { get; set; }
        public List<YearEndProcessAccounts> OpeningDetails { get; set; }
        public List<YearEndProcessIncomeExpense> IncomeExpDetails { get; set; }
        public List<YearEndProcessPL> PLDetails { get; set; }
        public List<YearEndProcessCustomer> CustomerInvDetails { get; set; }
        public List<YearEndProcessSupplier> SupplierInvDetails { get; set; }
    }

    public class YearEndProcessAccounts
    {
        public string Particulars { get; set; }

        public string AcHeadName { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal Transactions { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal NextYearOpening { get; set; }
        public decimal Credit { get; set; }
    }
    public class YearEndProcessIncomeExpense
    {
        public string AcType { get; set; }
        public int  AcHeadId { get; set; }
        public string AcHeadName { get; set; }        
        public decimal ClosingBalance { get; set; }
        
        
    }
    public class YearEndProcessPL
    {
        public string VoucherNo { get; set; }
        public int AcHeadId { get; set; }
        public string AcHeadName { get; set; }
        public decimal Amount { get; set; }
        public bool updatestatus { get; set; }

    }

    public class YearEndProcessCustomer
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceDate { get; set; }
        public decimal Amount{ get; set; }
        public decimal ClosingAmount { get; set; }
        public decimal OpeningAmount { get; set; }
        public decimal Difference { get; set; }
        public bool Selected { get; set; }
        public List<YearEndProcessCustomer> Details { get; set; }


    }

    public class YearEndProcessSupplier
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceDate { get; set; }
        public decimal Amount { get; set; }
        public decimal ClosingAmount { get; set; }
        public decimal OpeningAmount { get; set; }
        public decimal Difference { get; set; }
        public bool Selected { get; set; }
        public List<YearEndProcessSupplier> Details { get; set; }


    }

}