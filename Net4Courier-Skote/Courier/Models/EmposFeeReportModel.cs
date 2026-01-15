using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class EmposFeeReportModel
    {
        public string CourierMovement { get; set; }
        public string ParceltType { get; set; }
        public decimal Qunaity { get; set; }
        public decimal CourierCharge { get; set; }
        public decimal OtherCharge { get; set; }
        public decimal  TotalCharges { get; set; }
        public decimal Tax { get; set; }
        public decimal VAT { get; set; }

    }
}