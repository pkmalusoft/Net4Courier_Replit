using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class AcGroupModel
    {
        public int AcGroupID { get; set; }
        public Nullable<int> BranchID { get; set; }
        public string AcGroup { get; set; }
        public string AcType { get; set; }
        public string AcClass { get; set; }
        public Nullable<int> ParentID { get; set; }
        public Nullable<int> GroupOrder { get; set; }
        public Nullable<short> StaticEdit { get; set; }
        public Nullable<bool> StatusHide { get; set; }
        public Nullable<int> UserID { get; set; }
        public Nullable<int> AcCategoryID { get; set; }
        public string GroupCode { get; set; }
        public string ParentNode { get; set; }
        public string AcCategory { get; set; }
    }
}