using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class StockVM: ItemPurchase
    {
        public string StockEntryType { get; set; }
        public string ItemType { get; set; }
        public int ItemTypeId { get; set; }

    }

    public class StockSearch
    {
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
        public string StockEntryType { get; set; }
        public List<StockVM> Details { get; set; }
    }
}