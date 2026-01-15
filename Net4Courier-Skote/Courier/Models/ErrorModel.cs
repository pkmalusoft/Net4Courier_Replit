using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class SaveStatusModel
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public int TotalImportCount { get; set; }
        public int TotalSavedCount { get; set; }
    }
}