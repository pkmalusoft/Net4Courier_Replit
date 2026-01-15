// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.UserRegistrationVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System.Collections.Generic;

namespace Net4Courier.Models
{
  public class UserRegistrationVM
  {
    public int UserID { get; set; }

    public string UserName { get; set; }

    public string Password { get; set; }

        public string ConfirmPassword { get; set; }
        public string Password1 { get; set; }

    public string Phone { get; set; }

    public string EmailId { get; set; }

    public bool IsActive { get; set; }

    public int RoleID { get; set; }

    public string RoleName { get; set; }

    public bool EmailNotify { get; set; }
    public int UserReferenceId { get; set; }

        public string  BranchId { get; set; }
        public int[] SelectedValues { get; set; }
        public string SelectedValues1 { get; set; }
        public string DefaultBranchName { get; set; }
        public int DefaultBranchId { get; set; }

        public int AdditionalBranchId { get; set; }

        public List<UserBranchVM> Details { get; set; }
    }

    public class UserBranchVM :UserInBranch
    {
        public bool IsDeleted
        {
            get; set;
        }
        
        public string BranchName { get; set; }
    }
}
