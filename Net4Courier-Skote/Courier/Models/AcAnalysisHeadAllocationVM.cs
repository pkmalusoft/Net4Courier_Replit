using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class AcAnalysisHeadAllocationVM
    {
        public int AcAnalysisHeadAllocationID { get; set; }
        public Nullable<int> AcjournalDetailID { get; set; }
        public Nullable<int> AnalysisHeadID { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public string AnalysisHead { get; set; }
    }
}