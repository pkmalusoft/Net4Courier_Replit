using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class AcTypeModel
    {
        public int Id { get; set; }
        public string AcType { get; set; }
        public int? AcCategoryID { get; set; }
        public string AcCategory { get; set; }
       
    }

    public class AcOpeningMasterVm
    {
        public int AcOpeningID { get; set; }

        public int AcHeadID { get; set; }
        public decimal Amount { get; set; }
        public string AcHead { get; set; }
        public string AcNature { get; set; }
        public bool IsDeleted { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
    }
}