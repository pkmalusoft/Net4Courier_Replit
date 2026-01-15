// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.AgentVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System;

namespace Net4Courier.Models
{
  public class AgentVM
  {
    public int ID { get; set; }

    public int AcCompanyID { get; set; }

    public string AgentCode { get; set; }

    public string AgentName { get; set; }

    public string ReferenceCode { get; set; }

    public string ContactPerson { get; set; }

    public string Address1 { get; set; }

    public string Address2 { get; set; }

    public string Address3 { get; set; }

    public string Phone { get; set; }

    public string Fax { get; set; }

    public string Email { get; set; }

    //public int? CountryID { get; set; }

    //public int? CityID { get; set; }

    //public int? LocationID { get; set; }

    public int? CurrencyID { get; set; }

    public bool? StatusActive { get; set; }

    public Decimal? CreditLimit { get; set; }
        public string sCreditLimit { get; set; }

        public bool? StatusGPA { get; set; }

    public bool? StatusWalkIn { get; set; }

    public int? BranchID { get; set; }

    public int? ZoneCategoryID { get; set; }

    public string UserName { get; set; }

    public string Password { get; set; }
        public string AcHeadName { get; set; }
    public int? AcHeadID { get; set; }

    public int RoleID { get; set; }

    

    public string Type { get; set; }

    public string MobileDeviceID { get; set; }

    public string MobileDevicePwd { get; set; }

    public int AgentID { get; set; }

    public string WebSite { get; set; }
        public int? AgentType { get; set; } 

        public string CountryName { get; set; }
        public string CityName { get; set; }
        public string LocationName { get; set; }

        public int UserID { get; set; }
        public bool EmailNotify { get; set; }
    }
}
