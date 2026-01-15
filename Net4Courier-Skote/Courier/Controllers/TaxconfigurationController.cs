using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;

namespace Net4Courier.Controllers
{
    [SessionExpire]
    public class TaxConfigurationController : Controller
    {
        Entities1 db = new Entities1();

        public ActionResult Index()
        {
          
            List<TaxConfigurationVM> lst = (from t in db.TaxConfigurations join a in db.AcHeads on t.SalesHeadID equals a.AcHeadID join c in db.ParcelTypes on t.ParcelTypeId equals c.ID join m in db.CourierMovements on t.CourierMoveMentID equals m.MovementID select new TaxConfigurationVM { TaxConfigurationID= t.TaxConfigurationID, Movement = m.MovementType, CourierType = c.ParcelType1, TaxPercentage = t.TaxPercentage, AcHead = a.AcHead1, MinimumRate = t.MinimumRate.Value, AcCompanyID = t.AcCompanyID.Value, EffectFromDate = t.EffectFromDate.Value }).ToList();

         
            return View(lst.OrderByDescending(cc=>cc.EffectFromDate));
        }

     

        public ActionResult Details(int id = 0)
        {
            TaxConfiguration taxconfiguration = db.TaxConfigurations.Find(id);
            if (taxconfiguration == null)
            {
                return HttpNotFound();
            }
            return View(taxconfiguration);
        }

      

        public ActionResult Create()
        {
            ViewBag.couriermode = db.CourierMovements.ToList();
            ViewBag.parceltypes= db.ParcelTypes.ToList();
            ViewBag.achead = (from a in db.AcHeads join g in db.AcGroups on a.AcGroupID equals g.AcGroupID join t in db.AcTypes on g.AcTypeId equals t.Id where t.AccountType=="Direct Income" select a).ToList();
            return View();
        }

      

        [HttpPost]
        public ActionResult Create(TaxConfigurationVM item)
        {
            int companyid = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            if (ModelState.IsValid)
            {
                TaxConfiguration obj = new TaxConfiguration();

                int max = (from c in db.TaxConfigurations orderby c.TaxConfigurationID descending select c.TaxConfigurationID).FirstOrDefault();

                if (max == null)
                {
                    obj.TaxConfigurationID = 1;
                    obj.CourierMoveMentID = item.MoveMentID;
                    obj.ParcelTypeId = item.ParcelTypeId;
                    obj.TaxPercentage = item.TaxPercentage;
                    obj.SalesHeadID = item.SaleHeadID;
                    obj.MinimumRate = item.MinimumRate;
                    obj.AcCompanyID = companyid;
                    obj.EffectFromDate = item.EffectFromDate;

                }
                else
                {
                    obj.TaxConfigurationID = max + 1;
                    obj.CourierMoveMentID = item.MoveMentID;
                    obj.ParcelTypeId = item.ParcelTypeId;
                    obj.TaxPercentage = item.TaxPercentage;
                    obj.SalesHeadID = item.SaleHeadID;
                    obj.MinimumRate = item.MinimumRate;
                    obj.AcCompanyID = companyid;
                    obj.EffectFromDate = item.EffectFromDate;
                }

                db.TaxConfigurations.Add(obj);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added TaxConfiguration.";
                return RedirectToAction("Index");
            }

            return View(item);
        }

    

        public ActionResult Edit(int id)
        {
            TaxConfigurationVM obj = new TaxConfigurationVM();
            ViewBag.couriermode = db.CourierMovements.ToList();
            ViewBag.parceltypes = db.ParcelTypes.ToList();
            ViewBag.achead = (from a in db.AcHeads join g in db.AcGroups on a.AcGroupID equals g.AcGroupID join t in db.AcTypes on g.AcTypeId equals t.Id where t.AccountType == "Direct Income" select a).ToList();

            var data = (from d in db.TaxConfigurations where d.TaxConfigurationID == id select d).FirstOrDefault();



            if (data == null)
            {
                return HttpNotFound();
            }
            else
            {
                obj.TaxConfigurationID= data.TaxConfigurationID;
                obj.MoveMentID = data.CourierMoveMentID;
                obj.ParcelTypeId = data.ParcelTypeId;
                obj.TaxPercentage = data.TaxPercentage;
                obj.SaleHeadID = data.SalesHeadID;
                obj.MinimumRate = data.MinimumRate.Value;
                obj.AcCompanyID = data.AcCompanyID.Value;
                obj.EffectFromDate = data.EffectFromDate.Value;
            }
            return View(obj);
        }

      
        [HttpPost]
        public ActionResult Edit(TaxConfigurationVM data)
        {
            int companyid = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            TaxConfiguration obj = new TaxConfiguration();
            obj.TaxConfigurationID = data.TaxConfigurationID;
            obj.CourierMoveMentID = data.MoveMentID;
            obj.ParcelTypeId = data.ParcelTypeId;
            obj.TaxPercentage = data.TaxPercentage;
            obj.SalesHeadID = data.SaleHeadID;
            obj.MinimumRate = data.MinimumRate;
            obj.AcCompanyID = companyid;
            obj.EffectFromDate = data.EffectFromDate;

            if (ModelState.IsValid)
            {
                db.Entry(obj).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Updated TaxConfiguration.";
                return RedirectToAction("Index");
            }
            return View();
        }

      
        public ActionResult DeleteConfirmed(int id)
        {
            TaxConfiguration taxconfiguration = db.TaxConfigurations.Find(id);
            db.TaxConfigurations.Remove(taxconfiguration);
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully Deleted TaxConfiguration.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}