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
    public class CurrencyController : Controller
    {
         Entities1 db = new Entities1();

 

        public ActionResult Index()
        {
            List<CurrencyVM> lst = new List<CurrencyVM>();

            var data=db.CurrencyMasters.ToList();

             foreach (var item in data)
            {
               CurrencyVM c=new CurrencyVM();

                c.CurrencyID = item.CurrencyID;
                c.CurrencyName = item.CurrencyName;
                c.Symbol = item.Symbol;
                if (item.NoOfDecimals!=null)
                    c.NoOfDecimals = item.NoOfDecimals.Value;
                //if (item.CountryID!=null)
                //  c.CountryID = item.CountryID.Value;

                c.CountryName = item.CountryName;
                
                c.MonetaryUnit = item.MonetaryUnit;
                  c.StatusBaseCurrency = item.StatusBaseCurrency.Value;
                 lst.Add(c);
                
            }


            return View(lst);
        }

        //
        // GET: /Currency/Details/5

        public ActionResult Details(int id = 0)
        {
            CurrencyMaster currencymaster = db.CurrencyMasters.Find(id);
            if (currencymaster == null)
            {
                return HttpNotFound();
            }
            return View(currencymaster);
        }

        //
        // GET: /Currency/Create

        public ActionResult Create()
        {
            ViewBag.country = db.CountryMasters.ToList();
            return View();
        }

        public JsonResult CheckBaseCurrency()
        {
            IsExist a = new IsExist();

            var data = (from c in db.CurrencyMasters where c.StatusBaseCurrency == true select c).FirstOrDefault();
            if (data != null)
            {
                a.x = 1;
            }
            else
            {
                a.x = 0;
            }

            return Json(a, JsonRequestBehavior.AllowGet);
        }

        public class IsExist
        {
            public int x { get; set; }
        }

        //
        // POST: /Currency/Create

        [HttpPost]
       
        public ActionResult Create(CurrencyVM v)
        {
            if (ModelState.IsValid)
            {
                CurrencyMaster ob = new CurrencyMaster();


                int max = (from d in db.CurrencyMasters orderby d.CurrencyID descending select d.CurrencyID).FirstOrDefault();

                if (max == null)
                {
                    ob.CurrencyID = 1;
                    ob.CurrencyName = v.CurrencyName;
                    ob.Symbol = v.Symbol;
                    ob.NoOfDecimals = v.NoOfDecimals;
                    ob.CountryName = v.CountryName;
                    ob.StatusBaseCurrency = v.StatusBaseCurrency;
                    ob.MonetaryUnit = v.MonetaryUnit;
                    ob.ExchangeRate = v.ExchangeRate;
                    ob.CurrencyCode = v.CurrencyCode;
                }
                else
                {
                    ob.CurrencyID = max + 1;
                    ob.CurrencyName = v.CurrencyName;
                    ob.Symbol = v.Symbol;
                    ob.NoOfDecimals = v.NoOfDecimals;
                    ob.CountryName = v.CountryName;
                    ob.StatusBaseCurrency = v.StatusBaseCurrency;
                    ob.MonetaryUnit = v.MonetaryUnit;
                    ob.ExchangeRate = v.ExchangeRate;
                    ob.CurrencyCode = v.CurrencyCode;
                }

                db.CurrencyMasters.Add(ob);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added Currency.";
                return RedirectToAction("Index");
            }

            return View();

          
        }

        public JsonResult GetCurrencyName(string name)
        {
            var currency = (from c in db.CurrencyMasters where c.CurrencyName == name select c).FirstOrDefault();
            Status s = new Status();
            if (currency == null)
            {
                s.flag = 0;
            }
            else
            {
                s.flag = 1;
            }





            return Json(s, JsonRequestBehavior.AllowGet);
        }

        public class Status
        {
            public int flag { get; set; }
        }







        public ActionResult Edit(int id)
        {
            CurrencyVM c = new CurrencyVM();
            ViewBag.country = db.CountryMasters.ToList();
            var data = (from d in db.CurrencyMasters where d.CurrencyID == id select d).FirstOrDefault();

            if (data == null)
            {
                return HttpNotFound();
            }
            else
            {
                c.CurrencyID = data.CurrencyID;
                c.CurrencyName = data.CurrencyName;
                c.Symbol = data.Symbol;
                c.CountryName = data.CountryName;
                c.NoOfDecimals = data.NoOfDecimals.Value;
                //c.CountryID = data.CountryID.Value;
                c.StatusBaseCurrency = data.StatusBaseCurrency.Value;
                c.MonetaryUnit = data.MonetaryUnit;
                c.ExchangeRate = Convert.ToDecimal(data.ExchangeRate);
                c.CurrencyCode = data.CurrencyCode;
            }
            return View(c);
        }

        //
        // POST: /Currency/Edit/5

        [HttpPost]
       
        public ActionResult Edit(CurrencyVM v)
        {
            CurrencyMaster c = new CurrencyMaster();
            c = db.CurrencyMasters.Find(v.CurrencyID);            
            c.CurrencyName = v.CurrencyName;
            c.Symbol = v.Symbol;
            c.NoOfDecimals = v.NoOfDecimals;
            c.CountryName = v.CountryName;
            //c.CountryID = v.CountryID;
            c.StatusBaseCurrency = v.StatusBaseCurrency;
            c.MonetaryUnit = v.MonetaryUnit;
            c.ExchangeRate = v.ExchangeRate;
            c.CurrencyCode = v.CurrencyCode;

            if (ModelState.IsValid)
            {
                db.Entry(c).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Updated Currency.";
                return RedirectToAction("Index");
            }
            return View();
        }

     
     

       
      
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                CurrencyMaster currencymaster = db.CurrencyMasters.Find(id);
                db.CurrencyMasters.Remove(currencymaster);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Deleted Currency.";
            }
            catch (Exception ex)
            {
                 TempData["ErrorMsg"] = "Can Not Delete This Currency. Already in Use.";
                
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}