using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.DAL;
using Net4Courier.Models;

namespace Net4Courier.Controllers
{
    [SessionExpire]
    //[Authorize]
    public class RevenueTypeController : Controller
    {
        SourceMastersModel objSourceMastersModel = new SourceMastersModel();
        Entities1 db = new Entities1();

        public ActionResult Index()
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            List<RevenueAcHeadVM> model = new List<RevenueAcHeadVM>();
            model = AccountsDAO.GetRevenueTypeList(branchid);


            return View(model);
        }



        public ActionResult Details(int id = 0)
        {
            RevenueType revenuetype = objSourceMastersModel.GetRevenueTypeById(id);
            if (revenuetype == null)
            {
                return HttpNotFound();
            }
            return View(revenuetype);
        }


        public ActionResult Create(int acheadid)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            var branches = (from c in db.BranchMasters where c.BranchID == branchid select new { BranchID = c.BranchID, BranchName = c.BranchName }).ToList();
            branches.Add(new { BranchID = -1, BranchName = "All Branch" });
            ViewBag.Branches = branches;
            ViewBag.AcheadId = acheadid;

            //income category item
            var accounthead = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcCategoryID == 3 where (c.AcBranchID == branchid || c.AcBranchID == -1) select c).ToList().OrderBy(s => s.AcHead1);

            //expense category item
            var costaccounthead = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where (g.AcCategoryID == 4 || g.AcCategoryID == 2) where (c.AcBranchID == branchid || c.AcBranchID == -1) select c).ToList().OrderBy(s => s.AcHead1);

            ViewBag.accounthead = accounthead;
            ViewBag.costaccounthead = costaccounthead;
            List<VoucherTypeVM> lsttype = new List<VoucherTypeVM>();
            lsttype.Add(new VoucherTypeVM { TypeName = "Revenue" });
            lsttype.Add(new VoucherTypeVM { TypeName = "Cost" });
            lsttype.Add(new VoucherTypeVM { TypeName = "OtherRevenue" });
            ViewBag.RevenueGroup = lsttype;
            RevenueType model = new RevenueType();
            model.RevenueTypeID = 0;
            if (acheadid > 0)

            {
                var acheadfrompage = Convert.ToInt32(Session["AcheadPage"].ToString());
                if (acheadfrompage == 2)
                    model.AcHeadID = acheadid;
                else if (acheadfrompage == 3)
                    model.CostAcHeadID = acheadid;
            }
            //ViewBag.accounthead = db.AcHeads.OrderBy(x => x.AcHead1).ToList();
            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RevenueType revenuetype)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            var branches = (from c in db.BranchMasters where c.BranchID == branchid select new { BranchID = c.BranchID, BranchName = c.BranchName }).ToList();

            branches.Add(new { BranchID = -1, BranchName = "All Branch" });
            ViewBag.Branches = branches;
            //income category item
            var accounthead = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcCategoryID == 3 where (c.AcBranchID == branchid || c.AcBranchID == -1) select c).ToList().OrderBy(s => s.AcHead1);

            //expense category item
            var costaccounthead = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcCategoryID == 4 where (c.AcBranchID == branchid || c.AcBranchID == -1) select c).ToList().OrderBy(s => s.AcHead1);

            ViewBag.accounthead = accounthead;
            ViewBag.costaccounthead = costaccounthead;
            List<VoucherTypeVM> lsttype = new List<VoucherTypeVM>();
            lsttype.Add(new VoucherTypeVM { TypeName = "Revenue" });
            lsttype.Add(new VoucherTypeVM { TypeName = "Cost" });
            lsttype.Add(new VoucherTypeVM { TypeName = "OtherRevenue" });
            ViewBag.RevenueGroup = lsttype;
            if (ModelState.IsValid)
            {


                var query = (from t in db.RevenueTypes where t.RevenueType1 == revenuetype.RevenueType1 && (t.BranchID == revenuetype.BranchID) select t).ToList();

                if (query.Count > 0)
                {
                    ViewBag.accounthead = db.AcHeads.ToList();
                    ViewBag.SuccessMsg = "Revenue Type is already exist!";

                    return View(revenuetype);
                }


                revenuetype.RevenueTypeID = GetMaxNumberRevenueType();
                //revenuetype.BranchID = branchid;
                db.RevenueTypes.Add(revenuetype);
                db.SaveChanges();


                //objSourceMastersModel.SaveRevenueType(revenuetype);
                TempData["SuccessMSG"] = "You have successfully added Revenue Type.";
                return RedirectToAction("Index");
            }


            //var accounthead = db.AcHeads.ToList().OrderBy(s => s.AcHead1);
            //foreach (var item in accounthead)
            //{
            //    var acgroup = db.AcGroups.Where(d => d.AcGroupID == item.AcGroupID).FirstOrDefault();
            //    if (acgroup != null)
            //    {
            //        var actype = db.AcTypes.Where(d => d.Id == acgroup.AcTypeId).FirstOrDefault();
            //        if (actype != null)
            //        {
            //            item.AcHead1 = item.AcHead1 + " - " + actype.AccountType;
            //        }
            //    }
            //}
            //ViewBag.accounthead = accounthead;

            return View(revenuetype);
        }

        public int GetMaxNumberRevenueType()
        {

            var query = db.RevenueTypes.OrderByDescending(item => item.RevenueTypeID).FirstOrDefault();

            if (query == null)
            {
                return 1;
            }
            else
            {
                return query.RevenueTypeID + 1;
            }

        }

        public JsonResult GetRevenueTypeName(string name, string achead)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int ac = Convert.ToInt32(achead);
            var revenue = (from c in db.RevenueTypes where c.RevenueType1 == name && c.AcHeadID == ac && c.BranchID == branchid select c).FirstOrDefault();

            Status obj = new Status();

            if (revenue == null)
            {
                obj.flag = 0;

            }
            else
            {
                obj.flag = 1;
            }

            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        public class Status
        {
            public int flag { get; set; }
        }

        public ActionResult Edit(int id = 0)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            var branches = (from c in db.BranchMasters where c.BranchID == branchid select new { BranchID = c.BranchID, BranchName = c.BranchName }).ToList();
            branches.Add(new { BranchID = -1, BranchName = "All Branch" });
            ViewBag.Branches = branches;

            //income category item
            var accounthead = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcCategoryID == 3 where (c.AcBranchID == branchid || c.AcBranchID == -1) select c).ToList().OrderBy(s => s.AcHead1);

            //expense category item
            var costaccounthead = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcCategoryID == 4 where (c.AcBranchID == branchid || c.AcBranchID == -1) select c).ToList().OrderBy(s => s.AcHead1);

            ViewBag.accounthead = accounthead;
            ViewBag.costaccounthead = costaccounthead;
            ViewBag.accounthead = accounthead;
            List<VoucherTypeVM> lsttype = new List<VoucherTypeVM>();
            lsttype.Add(new VoucherTypeVM { TypeName = "Revenue" });
            lsttype.Add(new VoucherTypeVM { TypeName = "Cost" });
            lsttype.Add(new VoucherTypeVM { TypeName = "OtherRevenue" });
            ViewBag.RevenueGroup = lsttype;
            RevenueType revenuetype = objSourceMastersModel.GetRevenueTypeById(id);
            if (revenuetype == null)
            {
                return HttpNotFound();
            }
            return View(revenuetype);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(RevenueType revenuetype)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            var branches = (from c in db.BranchMasters where c.BranchID == branchid select new { BranchID = c.BranchID, BranchName = c.BranchName }).ToList();
            branches.Add(new { BranchID = -1, BranchName = "All Branch" });
            ViewBag.Branches = branches;

            //income category item
            var accounthead = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcCategoryID == 3 where (c.AcBranchID == branchid || c.AcBranchID == -1) select c).ToList().OrderBy(s => s.AcHead1);

            //expense category item
            var costaccounthead = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcCategoryID == 4 where (c.AcBranchID == branchid || c.AcBranchID == -1) select c).ToList().OrderBy(s => s.AcHead1);

            ViewBag.accounthead = accounthead;
            ViewBag.costaccounthead = costaccounthead;
            ViewBag.accounthead = accounthead;
            List<VoucherTypeVM> lsttype = new List<VoucherTypeVM>();
            lsttype.Add(new VoucherTypeVM { TypeName = "Revenue" });
            lsttype.Add(new VoucherTypeVM { TypeName = "Cost" });
            lsttype.Add(new VoucherTypeVM { TypeName = "OtherRevenue" });
            ViewBag.RevenueGroup = lsttype;
            if (ModelState.IsValid)
            {
                var query = (from t in db.RevenueTypes where t.RevenueTypeID != revenuetype.RevenueTypeID && t.RevenueType1 == revenuetype.RevenueType1 && (t.BranchID == revenuetype.BranchID) select t).ToList();

                if (query.Count > 0)
                {
                    ViewBag.accounthead = db.AcHeads.ToList();
                    ViewBag.SuccessMsg = "Revenue Type is already exist!";

                    return View(revenuetype);
                }

                objSourceMastersModel.SaveRevenueTypeById(revenuetype);
                TempData["SuccessMSG"] = "You have successfully updated Revenue Type.";
                return RedirectToAction("Index");
            }
            //var accounthead = db.AcHeads.ToList().OrderBy(s => s.AcHead1);
            //foreach (var item in accounthead)
            //{
            //    var acgroup = db.AcGroups.Where(d => d.AcGroupID == item.AcGroupID).FirstOrDefault();
            //    if (acgroup != null)
            //    {
            //        var actype = db.AcTypes.Where(d => d.Id == acgroup.AcTypeId).FirstOrDefault();
            //        if (actype != null)
            //        {
            //            item.AcHead1 = item.AcHead1 + " - " + actype.AccountType;
            //        }
            //    }
            //}
            //ViewBag.accounthead = accounthead;
            return View(revenuetype);
        }




        public ActionResult DeleteConfirmed(int id)
        {
            //objSourceMastersModel.DeleteRevenueType(id);
            RevenueType revenueType = db.RevenueTypes.Find(id);
            if (revenueType != null)
            {
                if (revenueType.BranchID == -1)
                {
                    TempData["SuccessMSG"] = "Could not delete the common Items!";
                    return RedirectToAction("Index");
                }
                db.RevenueTypes.Remove(revenueType);
                db.SaveChanges();
                TempData["SuccessMSG"] = "You have successfully deleted Revenue Type.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["SuccessMSG"] = "Deleete Failed";
                return RedirectToAction("Index");
            }
        }
        public JsonResult GetCostHeadName(int RevenueTypeID)
        {

            int CostheadID = 0;
            string CostHeadName = "";

            var revenue = (from c in db.RevenueTypes where c.RevenueTypeID == RevenueTypeID select c).FirstOrDefault();
            if (revenue.CostAcHeadID != null)
            {
                var dbachead = db.AcHeads.Where(cc => cc.AcHeadID == revenue.CostAcHeadID).FirstOrDefault();
                if (dbachead != null)
                {
                    CostheadID = dbachead.AcHeadID;
                    CostHeadName = dbachead.AcHead1;
                }
            }
            if (CostheadID > 0)
            { return Json(new { Status = "OK", CostHeadID = CostheadID, CostHeadName = CostHeadName }, JsonRequestBehavior.AllowGet); }

            else
            {
                return Json(new { Status = "Failed", CostHeadID = CostheadID, CostHeadName = CostHeadName }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}