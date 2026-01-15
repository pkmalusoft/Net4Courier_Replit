using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class CustomerNotificationModel
    {
        public int id { get; set; }
        public int? employeeid { get; set; }
        public int? jobid { get; set; }
        public string EmpName { get; set; }
        public DateTime? Datetime { get; set; }
        public string Message { get; set; }
        public bool? IsEmail { get; set; }
        public bool? IsWhatsapp { get; set; }
        public bool? IsSms { get; set; }
    }
}