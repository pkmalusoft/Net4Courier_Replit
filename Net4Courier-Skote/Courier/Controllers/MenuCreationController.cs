using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;
using System.Data.Entity;

namespace Net4Courier.Controllers
{
    public class MenuCreationController : Controller
    {
      Entities1 db=new Entities1();

      public ActionResult Index()
      {
          return View(db.Menus.ToList());
      }

      public ActionResult Create()
      {
          ViewBag.Menu = db.Menus.ToList();

          return View();
      }

      //
      // POST: /MenuCreation/Create

      [HttpPost]
      public ActionResult Create(MenuVM menu)
      {
          int max = (from c in db.Menus orderby c.MenuID descending select c.MenuID).FirstOrDefault();
          menu.MenuID = max + 1;
          Menu a = new Menu();
          a.Title = menu.Title;
          a.Link = menu.Link;
          if (menu.ParentID != null)
          {
              a.ParentID = menu.ParentID;
          }
          a.MenuOrder = menu.MenuOrder;
          a.IsAccountMenu = menu.IsAccountMenu;
          a.PermissionRequired = menu.PermissionRequired;
          if (menu.Active == true)
          {
              a.IsActive = 1;
          }
          else
          {
              a.IsActive = 0;
          }

          db.Menus.Add(a);
          db.SaveChanges();
          TempData["SuccessMsg"] = "You have successfully added Menu.";
          return View("Index", db.Menus.ToList());



      }

      public ActionResult Edit(int id = 0)
      {
          Menu menu = db.Menus.Find(id);
          MenuVM v = new MenuVM();
          if (menu == null)
          {
              return HttpNotFound();
          }
          else
          {
             
              v.MenuID = menu.MenuID;
              v.Title = menu.Title;
              v.ParentID = menu.ParentID;
              v.Link = menu.Link;
              v.MenuOrder = menu.MenuOrder;
              v.IsAccountMenu = menu.IsAccountMenu;
              v.PermissionRequired = menu.PermissionRequired;

              if (menu.IsActive == 0)
              {
                  v.Active = false;
              }
              else
              {
                  v.Active = true;
              }
          }
          ViewBag.Menu = db.Menus.ToList();
         
          return View(v);
      }

      //
      // POST: /MenuCreation/Edit/5

      [HttpPost]
      public ActionResult Edit(MenuVM menu)
      {
          Menu a = new Menu();
          if (ModelState.IsValid)
          {
              a.MenuID = menu.MenuID;
              a.Title = menu.Title;
              a.Link = menu.Link;
              if (menu.ParentID != null)
              {
                  a.ParentID = menu.ParentID;
              }
              a.MenuOrder = menu.MenuOrder;
              a.IsAccountMenu = menu.IsAccountMenu;
              a.PermissionRequired = menu.PermissionRequired;
              if (menu.Active == true)
              {
                  a.IsActive = 1;
              }
              else
              {
                  a.IsActive = 0;
              }

              db.Entry(a).State = EntityState.Modified;
              db.SaveChanges();
              TempData["SuccessMsg"] = "You have successfully Updated Menu.";
              return View("Index", db.Menus.ToList());
          }
          return View(menu);
      }

      public JsonResult DeleteConfirmed(int id)
      {
          Menu x = (from c in db.Menus where c.MenuID == id select c).FirstOrDefault();
            try
            {
                if (x == null)
                {
                    return Json(new { status = "Failed", message = "Menu not found" });
                }
                else
                {
                    db.Menus.Remove(x);
                    db.SaveChanges();
                    TempData["SuccessMsg"] = "You have successfully Deleted Menu.";
                    return Json(new { status = "OK", message = "You have successfully Deleted Menu." });
                }
            }
            catch(Exception ex)
            {
                return Json(new { status = "Failed", message = ex.Message });
            }
            
        }

    

    }

      
}
