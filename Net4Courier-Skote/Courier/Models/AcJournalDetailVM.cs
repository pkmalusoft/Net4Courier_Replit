using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class VoucherTypeVM
    {
        public string TypeName { get; set; }
    }
    public class AcJournalMasterVM:AcJournalMaster
    {
        public string MasterHead { get; set; }
        public decimal Amount { get; set; }
    }
    public class AcJournalDetailVM
    {
        public int ID { get; set; }
        public bool IsDeleted { get; set; }
        public int AcHeadID { get; set; }
        public string AcHead { get; set; }
        public string Rem { get; set; }
        public decimal Amt { get; set; }
        public decimal TaxPercent { get; set; }

        public decimal TaxAmount { get; set; }
        public int AcJournalDetID { get; set; }        
        public bool AmountIncludingTax { get; set; }
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }

        public List<AcExpenseAllocationVM> AcExpAllocationVM { get; set; }
    }
}