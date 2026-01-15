// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.MenuRoleVM
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

namespace Net4Courier.Models
{
  public class MenuRoleVM
  {
    public int? MenuID { get; set; }

    public string Title { get; set; }

    public string Name { get; set; }

    public int? RoleId { get; set; }

    public string Role { get; set; }

    public string Menu { get; set; }

    public int ParentID { get; set; }

    public long MenuAccessID { get; set; }
  }

    public class MenuAccessLevelVM:MenuAccessLevel
    {
        public string ParentMenuName { get; set; }
        public string MenuName { get; set; }
    }
}
