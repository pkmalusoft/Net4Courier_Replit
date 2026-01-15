using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class SkylarkInscanModel
    {
        public int ID  {get;set;}
        public string BatchNo { get; set; }
            public int CustomerID  {get;set;}
        public int CourierID { get; set; }
        public int VehicleID { get; set; }
        public DateTime EntryDateTime { get; set; }
        public int EmpID { get; set; }
        public int TotalAWB { get; set; }
        public string CustomerName { get; set; }
        public string EmployeeName { get; set; }
       public string VehicleNo { get; set; }
        public int ReceivedBy { get; set; }

        public DateTime ReceivedDate { get; set; }
        public string AWBBatchNo { get; set; }
        public int AWBBatchID { get; set; }
        public List<CMS_BulkInscanMobileDetail> Details { get; set; }
    }


    public class SkylarkInscanSearch
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string BatchNo { get; set; }
        public int CustomerID { get; set; }

        public int CollectedBy { get; set; }
        public string CustomerName { get; set; }
        public List<SkylarkInscanModel> Details { get; set; }
    }
    public class SkylarkReturntoConsignorSearch
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        
        public List<SkylarkReturntoConsignorAWB> Details { get; set; }
    }

    public class SkylarkReturntoConsignorAWB
    {
        
        public string AWBNo { get; set; }
        public int InScanId { get; set; }
        public DateTime AWBDate { get; set; }

        public DateTime EntryDate { get; set; }
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }

        public string CourierName { get; set; }
        public string CourierStatus { get; set; }
        public int CourierStatusId { get; set; }

    }
}