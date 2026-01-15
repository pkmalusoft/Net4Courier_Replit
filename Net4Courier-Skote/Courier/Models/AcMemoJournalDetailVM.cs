using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class AcMemoJournalDetailVM
    {
        public int AcHeadID { get; set; }
        public string AcHead { get; set; }
        public string Rem { get; set; }
        public decimal Amt { get; set; }

        public bool IsDeleted { get; set; }
        public int AcMemoDetailID { get; set; }
        
    }
}