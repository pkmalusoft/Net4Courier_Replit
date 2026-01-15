using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class DCODReceiptVM:DomesticCODReceipt
    {
        public decimal TotalCharges { get; set; }
        public  List<DCODReceiptDetailVM> Details { get; set; }
    }

    public class DCODReceiptDetailVM :DomesticCODReceiptDetail
    {
       public DateTime AWBDateTime { get; set; }     
        public string ConsigneeName { get; set; }
        public string ConsigneeCountryName { get; set; }        
        public decimal CustomCharge { get; set; }        
        //public decimal TotalCharges { get; set; }
        public int MovementId { get; set; }
        public bool AWBChecked { get; set; }
    }
}