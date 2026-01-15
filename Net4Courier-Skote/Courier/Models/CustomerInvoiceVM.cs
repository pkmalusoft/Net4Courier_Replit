// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.CustomerInvoiceVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Net4Courier.Models
{
  public class CustomerInvoiceVM:CustomerInvoice
  {  
        public string GridHtml { get; set; }
    public string CustomerName { get; set; }
        public string CustomerCode { get; set; }
        public string VATTRN { get; set; }
        public string CustomerCountryName { get; set; }
        public string CurrencyName { get; set; }
        public string CustomerPhoneNo { get; set; }
        public string Address1 { get; set; }
        public string Pincode { get; set; }
        public string CustomerCityName { get; set; }
        public string InvoiceTotalInWords { get; set; }
        public string ParcelType { get; set; }
        public string CourierMovement { get; set; }
        public string LogoFilePath { get; set; }
        public DateTime FromDate { get; set; }
         
    public DateTime ToDate { get; set; }
        public int MovementTypeID { get; set; }
        public decimal TotalCourierCharge { get; set; }
        public decimal TotalCharges { get; set; }
        public decimal TotalOtherCharge { get; set; }
        public decimal TotalVat { get; set; }
        public decimal TotalSurcharge { get; set; }
        [AllowHtml]
        public string invoiceFooter { get; set; }

        public GeneralSetup generalSetup { get; set; }
        public List<Net4Courier.Models.CustomerInvoiceDetailVM> CustomerInvoiceDetailsVM { get; set; }

        public string MovementId { get; set; }
        public int[] SelectedValues { get; set; }

        public string InvoiceFooter1 { get; set; }
        public string InvoiceFooter2 { get; set; }
        public string InvoiceFooter3 { get; set; }
        public string InvoiceFooter4 { get; set; }
        public string InvoiceFooter5 { get; set; }
        public string BankDetail1 { get; set; }
        public string BankDetail2 { get; set; }
        public string BankDetail3 { get; set; }
        public string BankDetail4 { get; set; }
        public string BranchTRN{ get; set; }
        public int TotalAWB { get; set; }

        public string AWBNo { get; set; }
    }

    public class CustomerInvoiceDetailVM :CustomerInvoiceDetail
    {
        public DateTime AWBDateTime { get; set; }
        public string Origin { get; set; }
        public string Consignor { get; set; }
        
        public string  ConsigneeName { get; set; }
        public string ConsigneeCountryName { get; set; }
        public string ConsigneeCityName { get; set; }
        public decimal TotalCharges { get; set; }
        public decimal? Weight { get; set; }
        public string Pieces { get; set; }
        public int? MovementId { get; set; }

        public int? ParcelTypeId { get; set; }        
        public bool AWBChecked { get; set; }
    }
    public class AWBOtherChargeSearch
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string InvoiceNo { get; set; }
        public List<CustomerInvoiceOtherChargeDetail> Details { get; set; }
    }

    public class CustomerInvoiceOtherChargeDetail
    {
        public int InScanID { get; set; }
        public int CustomerInvoiceID { get; set; }
        public string CustomerInvoiceNo { get; set; }
        public string AWbNo { get; set; }
        public DateTime AWBDate { get; set; }
        public string OtherChargeName { get; set; }
        public decimal Amount { get; set; }
       
    }
    public class InvoiceDatePicker
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }
        public bool Create { get; set; }

        public int? StatusId { get; set; }
        public int? AgentId { get; set; }
    }

    public class CustomerInvoiceSearch
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string InvoiceNo { get; set; }
        public List<CustomerInvoiceVM> Details { get; set; }
    }

    public class CustomerInvoicePendingModel
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public bool CustomerChecked { get; set; }
        public string CustomerName { get; set; }
        public int TotalAWB { get; set; }
        public decimal CourierCharge { get; set; }
        public decimal OtherCharge { get; set; }
        public decimal NetTototal { get; set; }
    }
}
