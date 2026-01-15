using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace  Net4Courier.Models
{
    public class StaffNoteModel
    {
        public int id { get; set; } 
        public int? employeeid { get; set; } 
        public int? jobid { get; set; } 
        public string EmpName { get; set; } 
        public DateTime? Datetime { get; set; } 
        public string TaskDetails { get; set; } 
    }
}