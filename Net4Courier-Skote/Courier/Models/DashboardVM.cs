using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class DashboardVM :DashBoardInfo
    {
        public List<QuickAWBVM> ShipmentList { get; set; }

        public List<PaymentModeWiseCount> PaymentModeWiseList { get; set; }

    }

    public class PaymentModeWiseCount
    {
        public int ID { get; set; }
        public string PaymentMode { get; set; }
        public int AWBCount { get; set; }
        public int AWBPercent { get; set; }

    }
}