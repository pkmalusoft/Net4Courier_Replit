using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class AcHeadSelectAllVM
    {       
            public int AcHeadID { get; set; }
            public string AcHeadKey { get; set; }
            public string AcHead { get; set; }
            public Nullable<int> AcGroupID { get; set; }
            public Nullable<int> ParentID { get; set; }
            public Nullable<int> HeadOrder { get; set; }
            public Nullable<bool> StatusHide { get; set; }
            public Nullable<int> UserID { get; set; }
            public string Prefix { get; set; }
            public Nullable<bool> StatusControlAC { get; set; }
            public string AcGroup { get; set; }
            public string AccountDescription { get; set; }
            public string AccountType { get; set; }

        public bool TaxApplicable { get; set; }
        public decimal TaxPercent { get; set; }
        
    }
}