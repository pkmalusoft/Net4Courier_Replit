// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.UserLoginVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

namespace Net4Courier.Models
{
  public class UserLoginVM
  {
    public string UserName { get; set; }

    public string Password { get; set; }
        public string NewPassword { get; set; }
        public int BranchID { get; set; }

    public int DepotID { get; set; }

        public int AcFinancialYearID { get; set; }
        public string UserType { get; set; }
  }
}
