using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;
using Net4Courier.DAL;
using System.Data.Entity;
using System.Configuration;
using Newtonsoft.Json;


namespace Net4Courier.Controllers
{
    [SessionExpire]
    public class EmployeeZoneController : Controller
    {
        // GET: EmployeeZone
        Entities1 db = new Entities1();

        public ActionResult Index()
        {

            List<ZoneEmpVM> lst = (from c in db.EmpZoneAllocations join t in db.EmployeeMasters 
                                     on c.EmployeeID equals t.EmployeeID 
                                     select new ZoneEmpVM { EmpZoneAllocationID=c.EmpZoneAllocationID,EffectFromDate=c.EffectFromDate ,EmployeeName=t.EmployeeName }).ToList();

            return View(lst);
        }

        public ActionResult Create(int id = 0)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            ViewBag.ZoneCategory = db.ZoneCategories.ToList();
            ViewBag.Zones = db.ZoneMasters.ToList();
            ViewBag.Country = db.CountryMasters.ToList();
            ViewBag.City = db.CityMasters.ToList();
            ViewBag.Employee = db.EmployeeMasters.ToList();

            ZoneEmpVM v = new ZoneEmpVM();
            if (id == 0)
            {
                v.EmpZoneAllocationID = 0;
                v.Details = new List<ZomeEmpDetailsVM>();
                ViewBag.Title = "Employee Zone - Create";
            }
            else
            {
                ViewBag.Title = "Employee Zone - Modify";
                EmpZoneAllocation z = db.EmpZoneAllocations.Find(id);
                v.EmpZoneAllocationID = z.EmpZoneAllocationID;
                v.EmployeeID = z.EmployeeID;
                v.EffectFromDate = z.EffectFromDate;
                v.AcCompanyID = z.AcCompanyID;

                var lst = (from c in db.EmpZoneAllocationDetails
                           join z1 in db.ZoneChartDetails on c.ZoneChartDetailID equals z1.ZoneChartDetailID
                           join zone in db.ZoneMasters on c.ZoneId equals zone.ZoneID
                           join l in db.LocationMasters on z1.LocationID equals l.LocationID
                           where c.EmpZoneAllocationID==id
                           select new ZomeEmpDetailsVM { ZoneChartDetailID=c.ZoneChartDetailID, ZoneName=zone.ZoneName, EmpZoneAllocationDetailID = c.EmpZoneAllocationDetailID, EmpZoneAllocationID = c.EmpZoneAllocationID, LocationName = l.LocationName }).ToList();
                v.Details = lst;

            }
            //Convert.ToInt32(Session["depotcountry"].ToString());
            return View(v);
        }

        [HttpPost]
        public ActionResult Create(ZoneEmpVM v)
        {
            int companyid = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            EmpZoneAllocation z = new EmpZoneAllocation();

            if (v.EmpZoneAllocationID == 0)
            {
                int? max= 0;
                max = (from c in db.EmpZoneAllocations orderby c.EmpZoneAllocationID descending select c.EmpZoneAllocationID).FirstOrDefault();
                
                if (max == null)
                    max = 0;

                //z.EmpZoneAllocationID = Convert.ToInt32(max) + 1;
                z.EmployeeID = v.EmployeeID;
                z.EffectFromDate = v.EffectFromDate;
                z.AcCompanyID = companyid;
                db.EmpZoneAllocations.Add(z);
                db.SaveChanges();
            }
            else
            {
                z = db.EmpZoneAllocations.Find(v.EmpZoneAllocationID);
                z.EmployeeID = v.EmployeeID;
                z.EffectFromDate = v.EffectFromDate;
                db.Entry(z).State = EntityState.Modified;
                db.SaveChanges();

                //deleting the details items
                var details = (from d in db.EmpZoneAllocationDetails where d.EmpZoneAllocationID == z.EmpZoneAllocationID select d).ToList();
                db.EmpZoneAllocationDetails.RemoveRange(details);
                db.SaveChanges();
            }

            List<ZoneChartDetail> l = new List<ZoneChartDetail>();
            foreach (var i in v.Details)
            {
                EmpZoneAllocationDetail s = new EmpZoneAllocationDetail();
                int? max1 = 0;
                max1 = (from c in db.EmpZoneAllocationDetails orderby c.EmpZoneAllocationDetailID descending select c.EmpZoneAllocationDetailID).FirstOrDefault();

                if (max1 == null)
                    max1 = 0;
                //s.EmpZoneAllocationDetailID = Convert.ToInt32( max1) + 1;
                s.EmpZoneAllocationID = Convert.ToInt32(max1) + 1;
                s.EmpZoneAllocationID = z.EmpZoneAllocationID;
                s.ZoneChartDetailID = i.ZoneChartDetailID;
                s.ZoneId = i.ZoneId;
                db.EmpZoneAllocationDetails.Add(s);
                db.SaveChanges();                

            }
            if (v.EmpZoneAllocationID == 0)
            {
                TempData["SuccessMsg"] = "You have successfully added Employee Zone";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["SuccessMsg"] = "You have successfully updated Employee Zone";
                return RedirectToAction("Index");
            }
        }


        public ActionResult DeleteConfirmed(int id)
        {
            EmpZoneAllocation z = db.EmpZoneAllocations.Find(id);
            if (z == null)
            {
                return HttpNotFound();
            }
            else
            {
                var details = (from d in db.EmpZoneAllocationDetails where d.EmpZoneAllocationID == z.EmpZoneAllocationID select d).ToList();
                db.EmpZoneAllocationDetails.RemoveRange(details);
                db.SaveChanges();
                
                db.EmpZoneAllocations.Remove(z);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully deleted Zone Emp.";
                return RedirectToAction("Index");
            }


        }

        //GetEmpZone
        [HttpGet]
        public JsonResult GetEmpZone(string term)
        {
            if (term.Trim() != "")
            {
                var list = (from c1 in db.ZoneCategories
                                    join c2 in db.ZoneCharts on c1.ZoneCategoryID equals c2.ZoneCategoryID
                                    join c3 in db.ZoneMasters on c2.ZoneID equals c3.ZoneID
                                    where c1.ZoneCategory1 == "Employee" && c3.ZoneName.Contains(term.ToLower())
                                    orderby c3.ZoneName ascending
                                    select new { ZoneId = c3.ZoneID, ZoneName = c3.ZoneName,ZoneChartId=c2.ZoneChartID }).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = (from c1 in db.ZoneCategories
                                    join c2 in db.ZoneCharts on c1.ZoneCategoryID equals c2.ZoneCategoryID
                                    join c3 in db.ZoneMasters on c2.ZoneID equals c3.ZoneID
                                    where c1.ZoneCategory1 == "Employee" 
                                    orderby c3.ZoneName ascending
                                    select new { ZoneId = c3.ZoneID, ZoneName = c3.ZoneName,ZoneChartId=c2.ZoneChartID }).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);

            }

            

        }

        [HttpGet]
        public JsonResult GetEmpZoneLocation(string term,int ZoneId,int ZoneChartId)
        {
            if (term.Trim() != "")
            {
                var list = (from c1 in db.LocationMasters
                            join c2 in db.ZoneChartDetails on c1.LocationID equals c2.LocationID
                            join c3 in db.ZoneCharts on c2.ZoneChartID equals c3.ZoneChartID
                            where c3.ZoneChartID==ZoneChartId && c3.ZoneID==ZoneId 
                            && c1.LocationName.Contains(term.ToLower())
                            orderby c1.LocationName ascending
                            select new { ZoneChartDetailId = c2.ZoneChartDetailID,LocationName = c1.LocationName }).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = (from c1 in db.LocationMasters
                            join c2 in db.ZoneChartDetails on c1.LocationID equals c2.LocationID
                            join c3 in db.ZoneCharts on c2.ZoneChartID equals c3.ZoneChartID
                            where c3.ZoneChartID == ZoneChartId && c3.ZoneID == ZoneId                            
                            orderby c1.LocationName ascending
                            select new { ZoneChartDetailId = c2.ZoneChartDetailID, LocationName = c1.LocationName }).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);

            }



        }
    }
}