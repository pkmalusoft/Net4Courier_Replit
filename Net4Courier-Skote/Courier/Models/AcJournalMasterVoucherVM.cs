using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class AcJournalMasterVoucherVM
    {
        public int AcJournalID { get; set; }
        public AcJournalMasterVoucherVM() { }
        public DateTime TransDate { get; set; }
        public string Remark { get; set; }
        public string Refference { get; set; }
        public string AccountHead { get; set; }
        public decimal Amount { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public string AcRemark { get; set; }
        public int acHeadId { get; set; }
        public int acFYearId { get; set; }
        public string VoucherNo { get; set; }
        public string VoucherType { get; set; }
        public int userId { get; set; }
        public int BranchId { get; set; }
        public bool statusDelete { get; set; }
        public int ShiftID { get; set; }
        public string AcAnalysisHeadDetail { get; set; }
        public string AcJournalDetail { get; set; }
        public bool IsCustomerSelected { get; set; }
        public short IsDebit { get; set; }
        public int AcFinancialYearID { get; set; }
        public int AcCompanyID { get; set; }

        public List<AcJournalDetailsList> acJournalDetailsList { get; set; }



    }
}