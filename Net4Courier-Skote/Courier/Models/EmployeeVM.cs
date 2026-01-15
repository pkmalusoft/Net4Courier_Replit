// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.EmployeeVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System;

namespace Net4Courier.Models
{
    public class EmployeeVM
    {
        public int EmployeeID { get; set; }

        public string EmployeeCode { get; set; }

        public string EmployeeName { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string Address3 { get; set; }

        public string Phone { get; set; }

        public string Fax { get; set; }

        public string Email { get; set; }

        public string MobileNo { get; set; }

        public int CountryID { get; set; }

        public int DesignationID { get; set; }

        public DateTime JoinDate { get; set; }
        
        public int? Depot { get; set; }
        public string DepotName { get; set; }
        public string MobileDeviceID { get; set; }

        public string MobileDevicePWD { get; set; }

        public string Password { get; set; }

        public bool StatusCommision { get; set; }

        public int AcHeadID { get; set; }
        public bool StatusActive { get; set; }

        public bool StatusDefault { get; set; }

        public string Designation { get; set; }

        public int BranchID { get; set; }

        public int RoleID { get; set; }
        public int AcCompanyID {get;set;}
        public string CountryName { get; set; }
        public string CityName { get; set; }
        public string LocationName { get; set; }
        public int? UserID { get; set; }
  }
}
