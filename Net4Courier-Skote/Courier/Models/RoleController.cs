// Decompiled with JetBrains decompiler
// Type: CMSV2.Models.RoleController
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\CMSV2.dll

using CMSV2.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace CMSV2.Controllers
{
  public class RoleController : Controller
  {
    private Entities1 db = new Entities1();

    public ActionResult Index()
    {
      List<RoleMasterVM> roleMasterVmList = new List<RoleMasterVM>();
      foreach (RoleMaster roleMaster in this.db.RoleMasters.ToList<RoleMaster>())
        roleMasterVmList.Add(new RoleMasterVM()
        {
          RoleID = roleMaster.RoleID,
          RoleName = roleMaster.RoleName
        });
      return (ActionResult) this.View((object) roleMasterVmList);
    }

    public ActionResult Create()
    {
      return (ActionResult) this.View();
    }

    [HttpPost]
    public ActionResult Create(RoleMasterVM v)
    {
      if (this.ModelState.IsValid)
      {
        RoleMaster entity = new RoleMaster();
        int num = this.db.RoleMasters.OrderByDescending<RoleMaster, int>((Expression<Func<RoleMaster, int>>) (c => c.RoleID)).Select<RoleMaster, int>((Expression<Func<RoleMaster, int>>) (c => c.RoleID)).FirstOrDefault<int>();
        entity.RoleID = num + 1;
        entity.RoleName = v.RoleName;
        this.db.RoleMasters.Add(entity);
        this.db.SaveChanges();
        this.TempData["SuccessMsg"] = (object) "You have successfully added Role.";
      }
      return (ActionResult) this.RedirectToAction("Index");
    }

    public ActionResult Edit(int id)
    {
      RoleMasterVM roleMasterVm = new RoleMasterVM();
      RoleMaster roleMaster = this.db.RoleMasters.Where<RoleMaster>((Expression<Func<RoleMaster, bool>>) (c => c.RoleID == id)).FirstOrDefault<RoleMaster>();
      if (roleMaster == null)
        return (ActionResult) this.HttpNotFound();
      roleMasterVm.RoleID = roleMaster.RoleID;
      roleMasterVm.RoleName = roleMaster.RoleName;
      return (ActionResult) this.View((object) roleMasterVm);
    }

    [HttpPost]
    public ActionResult Edit(RoleMasterVM v)
    {
      RoleMaster entity = new RoleMaster();
      entity.RoleID = v.RoleID;
      entity.RoleName = v.RoleName;
      if (!this.ModelState.IsValid)
        return (ActionResult) this.View();
      this.db.Entry<RoleMaster>(entity).State = EntityState.Modified;
      this.db.SaveChanges();
      this.TempData["SuccessMSG"] = (object) "You have successfully Updated Role.";
      return (ActionResult) this.RedirectToAction("Index");
    }

    public ActionResult DeleteConfirmed(int id)
    {
      this.db.RoleMasters.Remove(this.db.RoleMasters.Find((object) id));
      this.db.SaveChanges();
      this.TempData["SuccessMSG"] = (object) "You have successfully Deleted Role.";
      return (ActionResult) this.RedirectToAction("Index");
    }
  }
}
