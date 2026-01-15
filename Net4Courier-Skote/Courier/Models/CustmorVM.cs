// Decompiled with JetBrains decompiler
// Type: LTMSV2.Models.CustmorVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\LTMSV2dll

using System;

namespace Net4Courier.Models
{
    public class CustmorVM
    {
        public int CustomerID { get; set; }

        public int AcCompanyID { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string CustomerType { get; set; }

        public int CustomerRateTypeID { get; set; }

        public string ReferenceCode { get; set; }

        public string ContactPerson { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string Address3 { get; set; }


        public string Phone { get; set; }

        public string Mobile { get; set; }

        public string Fax { get; set; }

        public string Email { get; set; }

        public string Website { get; set; }

        public int CountryID { get; set; }

        public int CityID { get; set; }

        public int LocationID { get; set; }

        public int CurrenceyID { get; set; }

        public bool StatusActive { get; set; }

        public Decimal CreditLimit { get; set; }

        public bool StatusTaxable { get; set; }

        public int EmployeeID { get; set; }

        public bool StatusCommission { get; set; }

        public bool StatusGPA { get; set; }

        public bool StatusWalkIn { get; set; }

        public int CustomerRateTypeIntID { get; set; }

        public int CourierServiceID { get; set; }

        public int BranchID { get; set; }

        public string CustomerUsername { get; set; }

        public string Password { get; set; }

        public int RoleID { get; set; }

        public TimeSpan? OfficeTimeFrom { get; set; }

        public TimeSpan? OfficeTimeTo { get; set; }

        public string Referal { get; set; }
        public int BusinessTypeId { get; set; }
        public bool EmailNotify { get; set; }

        public string CountryName { get; set; }
        public string CityName { get; set; }

        public string LocationName { get; set; }
        public int? UserID { get; set; }
        public int? DepotID { get; set; }

        public int? ApprovedBy { get; set; }
        public string ApprovedUserName { get; set; }
        public DateTime ApprovedOn { get; set; }

        public string VATTRN { get; set; }
        public bool ChkApprovedBy { get; set; }

        public string PlaceID { get; set; }
        public string CountryCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string CreatedBy { get; set; }

        public bool APIEnabled { get; set; }

        public string GETAPI { get; set; }
        public string POSTAPI { get; set; }
    }
}
