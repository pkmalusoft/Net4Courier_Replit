using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class AgentInvoiceVM :AgentInvoice
    {
        public string MAWB { get; set; }
        public string CustomerName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string MovementId { get; set; }
        public int[] SelectedValues { get; set; }
       
        public int MovementTypeID { get; set; }
        public decimal TotalSurcharge { get; set; }
        public decimal TotalCourierCharge { get; set; }
        public decimal TotalOtherCharge { get; set; }
        public decimal TotalCharges { get; set; }
        public decimal TotalVat { get; set; }
        public List<AgentInvoiceDetailVM> Details { get; set; }
    }
    public class AgentDatePicker
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string MAWB { get; set; }
    }
        public class AgentInvoiceDetailVM : AgentInvoiceDetail
    {
        public string ConsigneeName { get; set; }
        public DateTime AWBDateTime { get; set; }
        public string ConsigneeCountryName { get; set; }
        public bool AWBChecked { get; set; }
    }

    public class AgentInvoiceSearch
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string InvoiceNo { get; set; }
        public List<AgentInvoiceVM> Details { get; set; }
    }
}