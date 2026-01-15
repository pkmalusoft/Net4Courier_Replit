using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class AcExpenseAllocationVM
    {
        public Nullable<int> AcAnalysisHeadAllocationID { get; set; }
        public Nullable<int> AcHead { get; set; }
        public Nullable<decimal> ExpAllocatedAmount { get; set; }
    }
}