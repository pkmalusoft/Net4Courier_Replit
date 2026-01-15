// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.BranchVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

namespace Net4Courier.Models
{
  public class BranchVM
  {
    public int BranchID { get; set; }

    public string BranchName { get; set; }

    public string Address1 { get; set; }

    public string Address2 { get; set; }

    public string Address3 { get; set; }

    public int CountryID { get; set; }

    public int CityID { get; set; }

    public int LocationID { get; set; }

    public string KeyPerson { get; set; }

    public int DesignationID { get; set; }

    public string Phone { get; set; }

    public string PhoneNo1 { get; set; }

    public string PhoneNo2 { get; set; }

    public string PhoneNo3 { get; set; }

    public string PhoneNo4 { get; set; }

    public string MobileNo1 { get; set; }

    public string MobileNo2 { get; set; }

    public string EMail { get; set; }

    public string Website { get; set; }

    public string BranchPrefix { get; set; }

    public int CurrencyID { get; set; }

    public int AcCompanyID { get; set; }

    public bool StatusAssociate { get; set; }

    public string AWBFormat { get; set; }

    public string Country { get; set; }

    public string City { get; set; }

    public string Location { get; set; }


        public string CountryName { get; set; }

        public string CityName { get; set; }

        public string LocationName { get; set; }

        public string Currency { get; set; }

        public string InvoicePrefix { get; set; }

        public string InvoiceFormat { get; set; }

        public string VATRegistrationNo { get; set; }
        public decimal VATPercent { get; set; }
        public int AcFinancialYearID { get; set; }
        public int VATAccountId { get; set; }
        public bool DRRProcess { get; set; }
        public decimal ImportVatThreshold { get; set; }

        public int RateType { get; set; } //1 -Base Wegith 2- Base Margin
  }
}
