using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class DRRRecPayVM:DRRRecPay
    {
        public int AchHeadID { get; set; }
        public string CashBank { get; set; }        
        public string ChequeBank { get; set; }        
        public decimal TotalCharges { get; set; }
        public List<DRRRecPayDetailVM> Details { get; set; }
    }

    public class DRRRecPayDetailVM : DRRRecPayDetail
    {public string AWBNo { get; set; }
        public DateTime AWBDateTime { get; set; }
        public string  AWBDate { get; set; }
        public string ConsigneeName { get; set; }
        public string ConsignorName { get; set; }
        public string ConsigneeCountryName { get; set; }
        public decimal CustomCharge { get; set; }
        public decimal MaterialCost { get; set; }
        public decimal AmountReceived { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountPending { get; set; }
        //public decimal TotalCharges { get; set; }
        public int MovementId { get; set; }
        public bool AWBChecked { get; set; }
    }
}