// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.VehiclesVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System;

namespace Net4Courier.Models
{
    public class VehicleTypeVM:tblVehicleType
    {

    }
  public class VehiclesVM
  {
    public int VehicleID { get; set; }

    public string VehicleDescription { get; set; }

    public string RegistrationNo { get; set; }

    public string Model { get; set; }

    public int VehicleTypeId { get; set; }

    public Decimal VehicleValue { get; set; }

    public DateTime ValueDate { get; set; }

    public DateTime PurchaseDate { get; set; }

    public DateTime RegExpirydate { get; set; }

    public int AcCompanyID { get; set; }

    public string VehicleNO { get; set; }
        public int EmployeeId { get; set; }

        public string EmployeeName { get; set; }
  }


    public class VehicleBinMasterVM : tblVehicleBin
    {
        public string VehicleName { get; set; }
        public string BinDetail { get; set; }
        public int[] SelectedValues { get; set; }
    }
}
