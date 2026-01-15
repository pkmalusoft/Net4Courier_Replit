 using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using Net4Courier.DAL;
namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class DepotController : Controller
    {
        private Entities1 db = new Entities1();

        //
        // GET: /Depot/

        public ActionResult Index()
        {
            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            List<DepotVM> lst = (from c in db.tblDepots join d in db.BranchMasters on c.BranchID equals d.BranchID select new DepotVM { ID = c.ID, Depot = c.Depot, CityName = c.CityName, CountryName = c.CountryName, IsOwn = c.IsOwn.Value, BranchID=c.BranchID,BranchName=d.BranchName }).ToList();
            return View(lst);
        }

        //
        // GET: /Depot/Details/5

     
        // GET: /Depot/Create

        public ActionResult Create()
        {
          //  ViewBag.Country = db.CountryMasters.ToList();
 //           ViewBag.Hubs = (from c in db.CityMasters where c.IsHub == true select c).ToList();
            ViewBag.Agent = db.AgentMasters.ToList();
            ViewBag.Branch = db.BranchMasters.ToList();
            return View();
        }

        //
        // POST: /Depot/Create

        [HttpPost]
        public ActionResult Create(DepotVM v)
        {
            tblDepot a = new tblDepot();
            if (ModelState.IsValid)
            {
               // a.CountryID = 1;// v.CountryID;
                //a.CityID = 19;// v.CityID;
                a.CountryName = v.CountryName;
                a.CityName = v.CityName;
                a.Depot = v.Depot;
                a.CompanyID = 1;
                a.BranchID = v.BranchID;//  Convert.ToInt32(Session["CurrentBranchID"].ToString());
                a.IsOwn = v.IsOwn;
                if (v.IsOwn == false)
                {
                    a.AgentID = v.AgentID;
                }

                db.tblDepots.Add(a);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added Depot.";
                return RedirectToAction("Index");
            }

            return View();
        }

        //
        // GET: /Depot/Edit/5

        public ActionResult Edit(int id = 0)
        {
              tblDepot tbldepot = db.tblDepots.Find(id);

            //ViewBag.Country = db.CountryMasters.ToList();
            //ViewBag.Hubs = (from c in db.CityMasters where c.IsHub == true && c.CountryID==tbldepot.CountryID select c).ToList();
            ViewBag.Agent = db.AgentMasters.ToList();
            ViewBag.Branch = db.BranchMasters.ToList();
            DepotVM v = new DepotVM();
            if (tbldepot == null)
            {
                return HttpNotFound();
            }
            else
            {
                v.ID = tbldepot.ID;
                //v.CountryID = tbldepot.CountryID.Value;
                //v.CityID = tbldepot.CityID.Value;
                //a.CityID = 19;// v.CityID;
                v.CountryName = tbldepot.CountryName;
                v.CityName = tbldepot.CityName;

                v.CompanyID = tbldepot.CompanyID;
                v.Depot = tbldepot.Depot;
                v.IsOwn = tbldepot.IsOwn.Value;
                v.AgentID = tbldepot.AgentID;
                v.BranchID = Convert.ToInt32(tbldepot.BranchID);
                
                
                if (v.IsOwn == true)
                {
                    v.Own = "True";

                }
                else
                {

                    v.Own = "False";
                }
            }
            return View(v);
        }

        //
        // POST: /Depot/Edit/5

        [HttpPost]
        public ActionResult Edit(DepotVM v)
        {
            tblDepot a = new tblDepot();
            if (ModelState.IsValid)
            {
                a.ID = v.ID;
                a.CompanyID = v.CompanyID;
                a.BranchID = v.BranchID; //  Convert.ToInt32(Session["CurrentBranchID"].ToString());
                a.CityID = 19;// v.CityID;                
                a.CountryName = v.CountryName;
                a.CityName = v.CityName;
                a.IsOwn = v.IsOwn;
                a.AgentID = v.AgentID;
                a.Depot = v.Depot;

                db.Entry(a).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Updated Depot.";
                return RedirectToAction("Index");
            }
            return View();
        }

        public ActionResult DeleteConfirmed(int id)
        {
            tblDepot dep = db.tblDepots.Find(id);
            db.tblDepots.Remove(dep);
            db.SaveChanges();
            TempData["SuccessMsg"] = "You have successfully Deleted Depot.";
            return RedirectToAction("Index");
        }

        public JsonResult GetCity(int id)
        {
            List<CityM> objCity = new List<CityM>();
            var city = (from c in db.CityMasters where c.CountryID == id  select c).ToList();

            foreach (var item in city)
            {
                objCity.Add(new CityM { City = item.City, CityID = item.CityID });

            }
            return Json(objCity, JsonRequestBehavior.AllowGet);
        }

        public class CityM
        {
            public int CityID { get; set; }
            public String City { get; set; }
        }
    }
}