using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class CustomerContractVM :CustomerMultiContract
    {
        public string CustomerName { get; set; }
        public string CustomerRateType { get; set; }
        public bool Allocated { get; set; }
        public string MovementId { get; set; }
        public int[] SelectedValues { get; set; }
    }
}