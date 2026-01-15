using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class AcHeadControlList
    {
        public long Id { get; set; }
        public int? PageControlId { get; set; }
        public string PageControlName { get; set; }
        public int? PageControlField { get; set; }
        public string PageControlFieldName { get; set; }
        public int? AcHeadId { get; set; }
        public string AccountName { get; set; }
        public string AccountNature { get; set; }
        public string Check_Sum { get; set; }
        public string AccountHeadName { get; set; }
    }
}