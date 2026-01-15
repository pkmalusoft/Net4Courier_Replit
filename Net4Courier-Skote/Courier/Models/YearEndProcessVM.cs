using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Net4Courier.DAL;
namespace Net4Courier.Models
{
    public class YearEndProcessVM
    {
        Entities1 context = new Entities1();
        public string CurrentFYearFrom { get; set; }
        public string CurrentFYearTo { get; set; }
        public string Reference { get; set; }
        public string NewFYearFrom { get; set; }
        public string NewFYearTo { get; set; }


       
        
    }
}