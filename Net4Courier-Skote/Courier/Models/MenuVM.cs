// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.MenuVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System;

namespace Net4Courier.Models
{
  public class MenuVM
  {
    public int MenuID { get; set; }

    public string Title { get; set; }

    public string Link { get; set; }

    public int? ParentID { get; set; }

    public int? Ordering { get; set; }

    public int? SubLevel { get; set; }

    public string RoleID { get; set; }

    public string CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public int? IsActive { get; set; }

    public string imgclass { get; set; }

    public bool? PermissionRequired { get; set; }

    public int? MenuOrder { get; set; }

    public bool? IsAccountMenu { get; set; }

    public bool Active { get; set; }
  }
}
