using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class RevenueAcHeadVM
    {
        public string RevenueType { get; set; }
        public string AcHead { get; set; }
        public string RevenueCode { get; set; }
        public int RevenueTypeID { get; set; }
        public string CostAcHead { get; set; }
        public int CostAcHeadId { get; set; }
    }
}