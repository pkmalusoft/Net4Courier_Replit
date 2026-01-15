using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Net4Courier.DAL;
using Net4Courier.Models;

namespace Net4Courier.Controllers
{
    [SessionExpire]
    public class PeriodLocksController : Controller
    {
        private Entities1 db = new Entities1();

        // GET: PeriodLocks
        public ActionResult Index()
        {
            var AcFinancialYearID = Convert.ToInt32(Session["fyearid"]);
            return View(db.PeriodLocks.Where(cc=>cc.FYearID==AcFinancialYearID).ToList());
        }

        // GET: PeriodLocks/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PeriodLock periodLock = db.PeriodLocks.Find(id);
            if (periodLock == null)
            {
                return HttpNotFound();
            }
            return View(periodLock);
        }

        // GET: PeriodLocks/Create
        public ActionResult Create()
        {
            PeriodLock model =new PeriodLock();
            model.StartDate = CommonFunctions.GetCurrentDateTime();
            model.EndDate = CommonFunctions.GetCurrentDateTime();
            return View(model);
        }

        // POST: PeriodLocks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PeriodLock periodLock)
        {
            try
            {
                var AcFinancialYearID = Convert.ToInt32(Session["fyearid"]);
                //DateTime date = DateTime.UtcNow.Date;
                periodLock.UserName = Convert.ToInt32(Session["UserID"].ToString());
                periodLock.StatusChangeDate = CommonFunctions.GetCurrentDateTime();
                periodLock.FYearID = AcFinancialYearID;
                db.PeriodLocks.Add(periodLock);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added Period Lock.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {

                TempData["ErrorMsg"] = "Getting Error."+ex.Message; 
                return View(periodLock);
            }
            
        }

        // GET: PeriodLocks/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PeriodLock periodLock = db.PeriodLocks.Find(id);
            if (periodLock == null)
            {
                return HttpNotFound();
            }
            return View(periodLock);
        }

        // POST: PeriodLocks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PeriodLock periodLock)
        {
            try
            {
                periodLock.UserName = Convert.ToInt32(Session["UserID"].ToString());
                periodLock.StatusChangeDate = CommonFunctions.GetBranchDateTime();
                db.Entry(periodLock).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Updated Period Lock.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = "Getting Error." + ex.Message;
                return View(periodLock);
            }
        }

        // GET: PeriodLocks/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PeriodLock periodLock = db.PeriodLocks.Find(id);
            if (periodLock == null)
            {
                return HttpNotFound();
            }
            return View(periodLock);
        }

        // POST: PeriodLocks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PeriodLock periodLock = db.PeriodLocks.Find(id);
            db.PeriodLocks.Remove(periodLock);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public JsonResult CheckPeriodLock(string vEntryDate)
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            bool PeriodLock = false;
            StatusModel result = AccountsDAO.CheckDateValidate(vEntryDate, yearid);
            
            string PeriodLockMessage = "";
            if (result.Status == "PeriodLock") //Period locked
            {
                PeriodLock = true;
                PeriodLockMessage = result.Message;
            }
            else
            {
                PeriodLock = false;
                PeriodLockMessage = "Period is active";
            }

            return Json(new { PeriodLock = PeriodLock, message = PeriodLockMessage }, JsonRequestBehavior.AllowGet);
        }
    
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
