// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.PickupRequestVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System;
using System.ComponentModel.DataAnnotations;

namespace Net4Courier.Models
{
  public class PickupRequestVM
  {
        public int InScanID { get; set; }
        public int EnquiryID { get; set; }
        public int? BranchID { get; set; }
        public int? DepotID { get; set; }
        public int? AcCompanyID { get; set; }
        public string EnquiryNo { get; set; }

    public string AWBNo { get; set; }

    public DateTime? EnquiryDate { get; set; }

    public int? ConsignerCountryId { get; set; }

    public int? ConsignerCityId { get; set; }

    public int? ConsignerLocationId { get; set; }

    public int? ConsigneeCityId { get; set; }

    public int? ConsigneeLocationId { get; set; }

    public int? DescriptionID { get; set; }

        [RegularExpression(@"^(((\d{1,3})(,\d{3})*)|(\d+))(.\d+)?$", ErrorMessage = "The Value format must be 00.00 or 00,00")]        
        public double? Weight { get; set; }

    

    public int? ConsigneeCountryID { get; set; }

    public int? CustomerID { get; set; }

    public string Consignee { get; set; }

    public string Consignor { get; set; }

    public string ConsigneeAddress { get; set; }

    public string ConsignorAddress { get; set; }

    public string ConsigneePhone { get; set; }

        //[RegularExpression(@"^\(\d{3}\)\s{0,1}\d{3}-\d{7}$", ErrorMessage = "Enter a valid number")]
        public string ConsignorPhone { get; set; }
        public string Email { get; set; }
    public int? EmployeeID { get; set; }

    public string Remarks { get; set; }

    public Decimal? CourierCharge { get; set; }
        public Decimal? MaterialCost { get; set; }

        public int? CollectedEmpID { get; set; }

    public string ShipmentType { get; set; }

    public string Vehicle { get; set; }

    public int VehicleTypeId { get; set; }

    public DateTime? ReadyTime { get; set; }

    public TimeSpan? OfficeTimeFrom { get; set; }

    public TimeSpan? OfficeTimeTo { get; set; }

    public string ConsigneeContact { get; set; }

    public string ConsignorContact { get; set; }

    public int? EnteredByID { get; set; }

    public string UserName { get; set; }
        public string UserID { get; set; }

        public string ConsigneeAddress1 { get; set; }

    public string ConsignorAddress1 { get; set; }

    public string ConsigneeAddress2 { get; set; }

    public string ConsignorAddress2 { get; set; }

    public string RequestSource { get; set; }

    public bool? IsEnquiry { get; set; }

    public string ConsignorLocationName { get; set; }

    public string ConsigneeLocationName { get; set; }

    public virtual CustomerMaster CustomerMaster { get; set; }

    public virtual EmployeeMaster EmployeeMaster { get; set; }

    public string eCollectedBy { get; set; }

    public string eAssignedTo { get; set; }

    public bool vehreq { get; set; }

    public string employeename { get; set; }
        public string Description { get; set; }
        public string Pieces { get; set; }
        public string PickupRequestStatus { get; set; }
     public int? PickupRequestStatusId { get; set; }
        public int? SubReasonId { get; set; }
        public DateTime? CollectedTime { get; set; }
        public string Referral { get; set; }
        public int BusinessType { get; set; }

        public string ConsigneeCountryName { get; set; }
        public string ConsigneeCityName { get; set; }
        public string ConsignorCountryName { get; set; }
        public string ConsignorCityName { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCode { get; set; }
        public int? DocumentTypeId { get; set; }

        public string PickupLocation { get; set; }
        public string DeliveryLocation { get; set; }

        public string PickupLocationPlaceId { get; set; }
        public string DeliveryLocationPlaceId { get; set; }

        public string PickupSubLocality { get; set; }
        public string DeliverySubLocality { get; set; }
        public int PaymentModeId { get; set; }


    }
}
