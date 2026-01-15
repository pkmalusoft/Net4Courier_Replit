using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;
using Net4Courier.DAL;
using System.Data.Entity;

namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class MenuRoleController : Controller
    {
        Entities1 db = new Entities1();

        public ActionResult Index()
        {
        //    List<MenuRoleVM> model = new List<MenuRoleVM>();
        //    var query =
        //                    (from t in db.MenuAccessLevels
        //                     join t1 in db.RoleMasters on t.RoleID equals t1.RoleID
        //                     join t2 in db.Menus on t.MenuID equals t2.MenuID
        //                     select new MenuRoleVM
        //                     {
        //                         Name = t1.RoleName,
        //                         Title = t2.Title,
        //                         MenuAccessID = t.MenuAccessID

        //                         //    MenuAccessID = Convert.ToInt32(t1.Id),

        //                     }).ToList();



        //    return View(query);

            List<MenuRoleVM> lst = (from c in db.MenuAccessLevels join t1 in db.Menus on c.MenuID equals t1.MenuID join t2 in db.RoleMasters on c.RoleID equals t2.RoleID select new MenuRoleVM { RoleId=c.RoleID, MenuAccessID = c.MenuAccessID, Role = t2.RoleName, Menu = t1.Title }).ToList();
            return View(lst);
        }

        //public ActionResult Index()
        //{
        //    List<MenuRoleVM> m = new List<MenuRoleVM>();
        //    var query = (from t in db.MenuAccessLevels join t1 in db.RoleMasters on t.RoleID equals t1.RoleID join t2 in db.Menus on t.MenuID equals t2.MenuID select new MenuRoleVM { Name = t1.RoleName, Title = t2.Title, MenuAccessID = t.MenuAccessID }).ToList();

        //}

        public ActionResult Create(int id=0)
        {
            var Query = (from t in db.Menus where t.IsAccountMenu.Value == false orderby t.MenuOrder select t).ToList();
            var query = (from t in db.RoleMasters
                         where t.RoleID == 1 
                         select t.RoleID).ToList();
            
                Session["AllRoleID"] = query;
                Session["MenuAll"] = Query;
            ViewBag.Menu = db.Menus.ToList();
            ViewBag.Roles = db.RoleMasters.Where(cc=>cc.RoleName!="Admin").ToList();
            MenuRoleVM vm = new MenuRoleVM();
            vm.RoleId = id;
            return View(vm);
        }

        [HttpPost]
        public ActionResult Create(MenuRoleVM v)
        {
            MenuAccessLevel a = new MenuAccessLevel();
            if (ModelState.IsValid)
            {
                a.MenuID = v.MenuID;
                a.RoleID = v.RoleId;

                db.MenuAccessLevels.Add(a);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added Menu Role Assigment.";
                return RedirectToAction("Index");
            }

            return View();
        }

        public ActionResult Edit(int id)
        {
            ViewBag.Menu = db.Menus.ToList();
            ViewBag.Roles = db.RoleMasters.ToList();

            MenuAccessLevel a = db.MenuAccessLevels.Find(id);
            MenuRoleVM v = new MenuRoleVM();
            if (a == null)
            {
                return HttpNotFound();
            }
            else
            {
                v.MenuAccessID = a.MenuAccessID;
                v.MenuID = a.MenuID;
                v.RoleId = a.RoleID;
            }

            return View(v);

        }

        [HttpPost]
        public ActionResult Edit(MenuRoleVM v)
        {
            MenuAccessLevel a = new MenuAccessLevel();


            if (ModelState.IsValid)
            {
                a.MenuAccessID = v.MenuAccessID;
                a.MenuID = v.MenuID;
                a.RoleID = v.RoleId;

                db.Entry(a).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Updated Menu Role Assigment.";
                return RedirectToAction("Index");
            }

            return View();
        }

        public ActionResult DeleteConfirmed(int id)
        {
            MenuAccessLevel a = db.MenuAccessLevels.Find(id);
            if (a == null)
            {
                return HttpNotFound();

            }
            else
            {
                db.MenuAccessLevels.Remove(a);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Deleted Menu Role Assigment.";
                return RedirectToAction("Index");
            }
        }

        public JsonResult GetRoleAccess(int RoleId)
        {
            var menulist = db.MenuAccessLevels.Where(cc => cc.RoleID == RoleId).ToList();

            return Json(new { status="ok", data = menulist }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public JsonResult SaveRoleAccess(int RoleId,string menus)
        {
            try
            {
                PickupRequestDAO _dao = new PickupRequestDAO();
                _dao.SaveMenuAccess(RoleId, menus);
                return Json(new { status = "ok", message= "saved successfully"}, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { status = "faiiled", message =ex.Message }, JsonRequestBehavior.AllowGet);
            }
            
        }


        public ActionResult AccessRights(int id=2)
        {
            ViewBag.RoleId = id;
            ViewBag.Roles = db.RoleMasters.Where(cc => cc.RoleName != "Admin").ToList();
            List<MenuAccessLevelVM> lst = (from c in db.MenuAccessLevels join t1 in db.Menus on c.MenuID equals t1.MenuID
                                           join t2 in db.RoleMasters on c.RoleID equals t2.RoleID
                                           join t3 in db.Menus on c.ParentID equals t3.MenuID
                                    where c.RoleID==id select new MenuAccessLevelVM { MenuAccessID=c.MenuAccessID, RoleID = c.RoleID,  ParentMenuName=t3.Title, MenuName = t1.Title ,IsAdd=c.IsAdd,IsModify=c.IsModify,IsDelete=c.IsDelete,Isprint=c.Isprint,IsView=c.IsView }).ToList();
            return View(lst);
        }

        [HttpPost]
        public JsonResult SaveAccessLevel(MenuAccessLevelVM item)        
        {
            var menuaccesslevel = db.MenuAccessLevels.Find(item.MenuAccessID);
            menuaccesslevel.IsAdd = item.IsAdd;
            menuaccesslevel.IsModify = item.IsModify;
            menuaccesslevel.Isprint = item.Isprint;
            menuaccesslevel.IsDelete = item.IsDelete;
            db.Entry(menuaccesslevel).State = EntityState.Modified;
            db.SaveChanges();            

            return Json(new { status ="ok"}, JsonRequestBehavior.AllowGet);

        }
    }
}
