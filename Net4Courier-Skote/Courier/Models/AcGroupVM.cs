using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class AcGroupVM
    {
        public int AcGroupID { get; set; }
        public int AcCategoryID{get;set;}
        public int AcGroup{get;set;}
        public int ParentID{get;set;}
        public int AcCompanyID{get;set;}
        public int UserID{get;set;}
        public short IsGroupCodeAuto{get;set;}
        public string GroupCode{get;set;}
        public string subgroup { get; set; }
        public int? AcTypeId { get; set; }
        public string AcType { get; set; }
    }
}