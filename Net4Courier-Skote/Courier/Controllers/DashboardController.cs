using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.DAL;
using Net4Courier.Models;

namespace Net4Courier.Controllers
{
    [SessionExpire]
    public class DashboardController : Controller
    {
        Entities1 db = new Entities1();
        // GET: Dashboard
        public ActionResult Index()
        {
            int userid = Convert.ToInt32(Session["UserID"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            DashBoardInfo model = db.DashBoardInfoes.Where(cc => cc.BranchId == branchid && cc.FyearId == yearid).FirstOrDefault();
            DashboardVM vm = new DashboardVM();
            if (model != null)
            {
                vm.ID = model.ID;
                vm.RevenueYTD = model.RevenueYTD;
                vm.RevenueMTD = model.RevenueMTD;
                vm.TotalJobs = model.TotalJobs;
                vm.TotalInStock = model.TotalInStock;
                vm.InStockCBM = model.InStockCBM;
                vm.Top1City = model.Top1City;
                vm.Top1CityName = model.Top1CityName;
                vm.Top2City = model.Top2City;
                vm.Top2CityName = model.Top2CityName;
                vm.Top3City = model.Top3City;
                vm.Top3CityName = model.Top3CityName;
                vm.RevenueSeriesA = model.RevenueSeriesA;
                vm.RevenueSeriesB = model.RevenueSeriesB;
                vm.RevenueSeriesC = model.RevenueSeriesC;
                vm.RevenueSeriesD = model.RevenueSeriesD;
                vm.CountSeriesA = model.CountSeriesA;
                vm.CountSeriesB = model.CountSeriesB;
                vm.CountSeriesC = model.CountSeriesC;
                vm.CountSeriesD = model.CountSeriesD;
                vm.ShipmentList = DashboardDAO.GetDashboardConsignmentList(branchid, yearid);
                vm.PaymentModeWiseList= DashboardDAO.GetDashboardPaymentConsignmentList(branchid, yearid);


            }
            else
            {
                vm.ShipmentList = new List<QuickAWBVM>();
                vm.PaymentModeWiseList = new List<PaymentModeWiseCount>();
                vm.TotalJobs = 0;
                vm.TotalInStock = 0;
                vm.InStockCBM = 0;
            }
            List<int> RoleId = (List<int>)Session["RoleID"];

            int roleid = RoleId[0];

            if (roleid == 1)
            {
                var Query = (from t in db.Menus where t.IsAccountMenu.Value == false && t.RoleID == null orderby t.MenuOrder select t).ToList();
                Session["Menu"] = Query;
                ViewBag.UserName = SourceMastersModel.GetUserFullName(Convert.ToInt32(Session["UserId"].ToString()), Session["UserType"].ToString());
                 
            }
            else
            {
                //List<Menu> Query2 = new List<Menu>();
                var Query = (from t in db.Menus join t1 in db.MenuAccessLevels on t.MenuID equals t1.MenuID where t1.RoleID == roleid && t.IsAccountMenu.Value == false orderby t.MenuOrder select t).ToList();

                var Query1 = (from t in db.Menus join t1 in db.MenuAccessLevels on t.MenuID equals t1.ParentID where t1.RoleID == roleid && t.ParentID == 0 && t.IsAccountMenu.Value == false orderby t.MenuOrder select t).ToList();

                var Query2 = (from t in db.Menus join t1 in db.MenuAccessLevels on t.MenuID equals t1.ParentID where t1.RoleID == roleid && t.IsAccountMenu.Value == false orderby t.MenuOrder select t).ToList();

                if (Query2 != null)
                {
                    foreach (Menu q in Query1)
                    {
                        var query3 = Query.Where(cc => cc.MenuID == q.MenuID).FirstOrDefault();
                        if (query3 == null)
                            Query2.Add(q);
                    }
                }

                if (Query1 != null)
                {
                    foreach (Menu q in Query1)
                    {
                        var query3 = Query.Where(cc => cc.MenuID == q.MenuID).FirstOrDefault();
                        if (query3 == null)
                            Query.Add(q);
                    }
                }



                Session["Menu"] = Query;
            }
                return View(vm);
        }


        //DashboardReprocess
        public JsonResult DashboardReprocess()
        {
            int userid = Convert.ToInt32(Session["UserID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            var dta = DashboardDAO.DashboardReprocess(branchid, yearid, userid);
            return Json(new { status = "OK", message = "ReProcessed Succesfully!" });
        }
    }
}