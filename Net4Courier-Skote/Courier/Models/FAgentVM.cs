// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.FAgentVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

namespace Net4Courier.Models
{
  public class FAgentVM
  {
    public int FAgentID { get; set; }

    public int AcCompanyID { get; set; }

    public int AcHeadID { get; set; }

    public string FAgentName { get; set; }

    public string ReferenceCode { get; set; }

    public string ContactPerson { get; set; }

    public string Address1 { get; set; }

    public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string AcHeadName { get; set; }
        public string Phone { get; set; }
        
    public string Fax { get; set; }

    public string Email { get; set; }

    public string WebSite { get; set; }

    public string CountryName{ get; set; }

    public string LocationName { get; set; }

    public string CityName{ get; set; }

    public int CurrencyID { get; set; }

    public int ZoneCategoryID { get; set; }

    public bool StatusSigned { get; set; }

    public bool StatusActive { get; set; }

    public bool StatusDefault { get; set; }

    public string FAgentCode { get; set; }

   public int SupplierID { get; set; }
        public string SupplierName { get; set; }
    }

    public class FAgentRate {

        public int ID { get; set; }
        public int InscanId { get; set; }
        public int FAgentID {get;set;}
       public int ProductTypeID { get; set; }
        public string ConsigneeCountryname { get; set; }
        public decimal Weight { get; set; }
        public int FAgentRateId { get; set; }
        
        public decimal Rate { get; set; }
    }

}
