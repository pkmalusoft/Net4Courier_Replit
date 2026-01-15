

using System;
using System.Collections.Generic;

namespace Net4Courier.Models
{
    public class AWBBookIssueVM : AWBBOOKIssue
    {

        public List<AWBDetailVM> Details { get; set; }
    }
    

    public class AWBBookIssueSearch {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string DocumentNo { get; set; }
        public List<AWBBookIssueList> Details { get; set; }
    }

    public class AWBBookIssueList : AWBBOOKIssue { 
        public string EmployeeName { get; set; }
    }


     
}
