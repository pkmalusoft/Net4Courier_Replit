using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    [Serializable]
    public class CsvRecordsViewModel
    {
        public int ID { get; set; }
        public string BankName { get; set; }
        public string ChequeNo { get; set; }
        public decimal ChequeAmount { get; set; }
        public string BName { get; set; }
        public string ChqNo { get; set; }
        public decimal ChqAmount { get; set; }
        public Boolean IsCleared { get; set; }
        public int Status { get; set; }
        public string ChqStatus { get; set; }
        public string Remarks { get; set; }
        public DateTime ChequeDate { get; set; }
        public DateTime ValueDate { get; set; }
        public string PartyName { get; set; }
        public string TransactionID { get; set; }
    }
}