// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.InScanVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System;
using System.Collections.Generic;

namespace Net4Courier.Models
{
    public class InScanMasterVM
    {
        public int InScanID { get; set; }
        public DateTime PickupRequestDate { get; set; }
        public DateTime TransactionDate { get; set; }
        public int PickupBy {get;set;}
    public DateTime PickupDateTime { get; set; }
    public string DeviceID { get; set; }
    public string AWBNo { get; set; }
    public int InScanSheetNo { get; set; }
    public DateTime InScanDate { get; set; }
        public int BranchID { get; set; }
        public int DepotID { get; set; }

        public string DepotCountryID { get; set; }
        public int CustomerID { get; set; }
        public int ReceivedByID { get; set; }


public string Consignor { get; set; }
public string ConsignorContact { get; set; }
public string ConsignorPhone { get; set; }
public string ConsignorAddress1_Building { get; set; }
public string ConsignorAddress2_Street { get; set; }
public string ConsignorAddress2_Pincode { get; set; }
public string ConsignorCityName { get; set; }
public string ConsignorCountryName { get; set; }

public string ConsignorLocationName { get; set; }
        
        public string Consignee { get; set; }
public string  ConsigneeCityName { get; set; }

        public string ConsigneeLocationName { get; set; }
        public string ConsigneeContact { get; set; }
public string ConsigneePhone { get; set; }
public string ConsigneeAddress1_Building { get; set; }
public string ConsigneeAddress2_Street { get; set; }
public string ConsigneeAddress3_PinCode { get; set; }
public string ConsigneeCountryName { get; set; }

public string Pieces { get; set; }
public decimal Weight { get; set; }
public string CargoDescription { get; set; }
public float StatedWeight { get; set; }
        public decimal CBM_length { get; set; }
        public decimal CBM_width { get; set; }
        public decimal CBM_height { get; set; }
        public decimal CBM { get; set; }
        public string BagNo { get; set; }
public string PalletNo { get; set; }
public string HandlingInstruction { get; set; }
public string SpecialInstructions { get; set; }
public int TypeOfGoodID { get; set; }
public string Remarks { get; set; }
        
        //Company Details
        public int AcCompanyID { get; set; }
               

        //Service Details       
                
        public int MovementID { get; set; }
        public int ParcelTypeID { get; set; } //dropdown
        public int ProductTypeID { get; set; } //dropdown
        //

        
        public int CustomerRateID { get; set; }
        public int StatusPaymentMode { get; set; }
        public  string StatusAWB { get; set; }
        public  bool StatusClose { get; set; }
        public bool StatusPrepaid { get; set; }
        public bool StatusPrepaidConsignee { get; set; }
        public int AcTaxJournalID { get; set; }
        public int AcJournalID { get; set; }
        
        public int CustomsCollectedBy { get; set; }
        
        public decimal Discount { get; set; }
        public decimal FuelSurcharge { get; set; }
        public int InvoiceID { get; set; }
        public string InvoiceValue { get; set; }
        public  decimal NetTotal { get; set; }
        public decimal CourierCharge { get; set; }
                public decimal PackingCharge { get; set; }
        public decimal PickupCharges { get; set; }
        public decimal CustomsValue { get; set; }
        public decimal OtherCharge { get; set; }
        public decimal TotalCharge { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal Tax { get; set; }
        public int TaxconfigurationID { get; set; }
        public string SalesExec { get; set; }
        public int UserID { get; set; }
        public string FAgent { get; set; }
        public string FAWBNo { get; set; }
        public decimal FAgentCharges { get; set; }
        public bool StatusReconciled { get; set; }
        public int HeldBy { get; set; }
        public DateTime HeldOn { get; set; }
        public string HeldReason { get; set; }
        public int ReleasedBy { get; set; }
        public DateTime ReleasedOn { get; set; }
        public string ReleasedReason { get; set; }
        public string Destination { get; set; }
        public int DestinationBranchID { get; set; }
        public string DestinationID { get; set; }
        public string  DestinationLocation { get; set; }
        public decimal MaterialCost { get; set; }
        public string MaterialDescription { get; set; }
        public string NCND { get; set; }
        public string ReceivedByName { get; set; }
        public DateTime ReceivedDate { get; set; }
        public int PrepaidAwbDetailID { get; set; }








    }
}
