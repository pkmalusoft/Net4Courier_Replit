using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Net4Courier.Models;
using Net4Courier.DAL;
namespace Net4Courier.Models
{
    public class OpennnigBalanceVM
    {
      
        public int AcHeadID { get; set; }
        public int  AcFinancialYearID { get; set; }
        public int  CrDr { get; set; }
        public decimal Amount { get; set; }
        public string AcHead { get; set; }
      

    }
}