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
    public class BranchMasterController : Controller
    {
         Entities1 db = new Entities1();

      
        public ActionResult Index()
        {

            //List<BranchVM> lst = (from b in db.BranchMasters join t1 in db.CountryMasters on b.CountryID equals t1.CountryID join t2 in db.CityMasters on b.CityID equals t2.CityID join t3 in db.LocationMasters on b.LocationID equals t3.LocationID join t4 in db.CurrencyMasters on b.CurrencyID equals t4.CurrencyID select new BranchVM {BranchID=b.BranchID,BranchName=b.BranchName,Country=t1.CountryName,City=t2.City,Location=t3.Location,Currency=t4.CurrencyName }).ToList();
            List<BranchVM> lst = (from b in db.BranchMasters join t4 in db.CurrencyMasters on b.CurrencyID equals t4.CurrencyID select new BranchVM { BranchID = b.BranchID, BranchName = b.BranchName, CountryName = b.CountryName, CityName = b.CityName, LocationName = b.LocationName, Currency = t4.CurrencyName }).ToList();



            return View(lst);
        }

        //
        // GET: /BranchMaster/Details/5

        public ActionResult Details(int id = 0)
        {
            BranchMaster branchmaster = db.BranchMasters.Find(id);
            if (branchmaster == null)
            {
                return HttpNotFound();
            }
            return View(branchmaster);
        }

      

        public ActionResult Create()
        {
            ViewBag.years = db.AcFinancialYears.ToList();
            ViewBag.designation = db.Designations.ToList();
            ViewBag.currency = db.CurrencyMasters.ToList();
            List<AcHeadSelectAll_Result> x = null;
            //x = db.AcHeadSelectAll(AcCompanyID).ToList();
            var x1 = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID  select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).OrderBy(c=>c.AcHead).ToList();

            ViewBag.heads = x1;
            var ratetypes = new SelectList(new[]
                                       {
                                            new { ID = "1", trans = "BaseWeight" },
                                            new { ID = "2", trans = "BaseMargin" },

                                        },
          "ID", "trans", 1);
            ViewBag.RateType = ratetypes;
            return View();
        }

     

        [HttpPost]
  
        public ActionResult Create(BranchVM item)
        {
            ViewBag.years = db.AcFinancialYears.ToList();
            ViewBag.designation = db.Designations.ToList();
            ViewBag.currency = db.CurrencyMasters.ToList();
            List<AcHeadSelectAll_Result> x = null;
            //x = db.AcHeadSelectAll(AcCompanyID).ToList();
            var x1 = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).OrderBy(c => c.AcHead).ToList();

            ViewBag.heads = x1;
            try
            {
                BranchMaster a = new BranchMaster();

                int max = (from d in db.BranchMasters orderby d.BranchID descending select d.BranchID).FirstOrDefault();



                if (max == null)
                {
                    a.BranchID = 1;
                    a.BranchName = item.BranchName;
                    a.Address1 = item.Address1;
                    a.Address2 = item.Address2;
                    a.Address3 = item.Address3;
                    a.CountryID = 1; // item.CountryID;
                    a.CityID = 19; // item.CityID;
                    a.LocationID = 7; // item.LocationID;
                    a.CountryName = item.CountryName;
                    a.CityName = item.CityName;
                    a.LocationName = item.LocationName;
                    a.KeyPerson = item.KeyPerson;
                    a.DesignationID = item.DesignationID;
                    a.Phone = item.Phone;
                    a.PhoneNo1 = item.PhoneNo1;
                    //  a.PhoneNo2 = item.PhoneNo2;
                    //                    a.PhoneNo3 = item.PhoneNo3;
                    //                  a.PhoneNo4 = item.PhoneNo4;
                    a.MobileNo1 = item.MobileNo1;
                    //                a.MobileNo2 = item.MobileNo2;

                    a.EMail = item.EMail;

                    a.Website = item.Website;
                    a.BranchPrefix = item.BranchPrefix;
                    a.CurrencyID = item.CurrencyID;
                    a.AcCompanyID = item.AcCompanyID;
                    a.StatusAssociate = item.StatusAssociate;
                    a.AWBFormat = item.AWBFormat;
                    a.InvoicePrefix = item.InvoicePrefix;
                    a.InvoiceFormat = item.InvoiceFormat;
                    a.AcFinancialYearID = item.AcFinancialYearID;
                    a.VATAccountId = item.VATAccountId;
                    a.DRRProcess = item.DRRProcess;
                    a.ImportVatThreshold = item.ImportVatThreshold;
                    a.CustomerRateType = item.RateType;
                }
                else
                {
                    a.BranchID = max + 1;
                    a.BranchName = item.BranchName;
                    a.Address1 = item.Address1;
                    a.Address2 = item.Address2;
                    a.Address3 = item.Address3;
                    //a.CountryID = 1; // item.CountryID;
                    //a.CityID = 19; // item.CityID;
                    //a.LocationID = 7; // item.LocationID;
                    a.LocationName = item.LocationName;
                    a.CityName = item.CityName;
                    a.CountryName = item.CountryName;
                    a.KeyPerson = item.KeyPerson;
                    a.DesignationID = item.DesignationID;
                    a.Phone = item.Phone;
                    a.PhoneNo1 = item.PhoneNo1;
                    //a.PhoneNo2 = item.PhoneNo2;
                    //a.PhoneNo3 = item.PhoneNo3;
                    //a.PhoneNo4 = item.PhoneNo4;
                    a.MobileNo1 = item.MobileNo1;
                    //a.MobileNo2 = item.MobileNo2;
                    a.EMail = item.EMail;
                    a.Website = item.Website;
                    a.BranchPrefix = item.BranchPrefix;
                    a.CurrencyID = item.CurrencyID;
                    a.AcCompanyID = item.AcCompanyID;
                    a.StatusAssociate = item.StatusAssociate;
                    a.AWBFormat = item.AWBFormat;
                    a.InvoicePrefix = item.InvoicePrefix;
                    a.InvoiceFormat = item.InvoiceFormat;
                    a.VATPercent = item.VATPercent;
                    a.VATRegistrationNo = item.VATRegistrationNo;
                    a.AcFinancialYearID = item.AcFinancialYearID;
                    a.VATAccountId = item.VATAccountId;
                    a.DRRProcess = item.DRRProcess;
                    a.ImportVatThreshold = item.ImportVatThreshold;
                    a.CustomerRateType = item.RateType;
                }


                db.BranchMasters.Add(a);
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully added Branch.";
                return RedirectToAction("Index");
            }catch(Exception ex)
            {
                TempData["ErrorMsg"] = ex.Message;
            }

          
            return View(item);
        }

       

        public ActionResult Edit(int id)
        {
            ViewBag.years = db.AcFinancialYears.ToList();
            var data = (from c in db.BranchMasters where c.BranchID == id select c).FirstOrDefault();
            BranchVM v = new BranchVM();
            //ViewBag.BranchID = db.BranchMasters.ToList();
            //ViewBag.country = db.CountryMasters.ToList();
            //ViewBag.city = db.CityMasters.ToList().Where(x=>x.CountryID==data.CountryID);
            //ViewBag.location = db.LocationMasters.ToList().Where(x=>x.CityID==data.CityID);
            ViewBag.designation = db.Designations.ToList();
            ViewBag.currency = db.CurrencyMasters.ToList();            
            var x1 = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).OrderBy(c => c.AcHead).ToList();

            ViewBag.heads = x1;
            try
            {
                if (data == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    v.BranchID = data.BranchID;
                    v.BranchName = data.BranchName;
                    v.Address1 = data.Address1;
                    v.Address2 = data.Address2;
                    v.Address3 = data.Address3;
                    //v.CountryID = data.CountryID.Value;
                    //v.CityID = data.CityID.Value;
                    //v.LocationID = data.LocationID.Value;
                    v.LocationName = data.LocationName;
                    v.CityName = data.CityName;
                    v.CountryName = data.CountryName;
                    v.KeyPerson = data.KeyPerson;
                    v.DesignationID = data.DesignationID.Value;
                    v.Phone = data.Phone;
                    v.PhoneNo1 = data.PhoneNo1;
                    v.PhoneNo2 = data.PhoneNo2;
                    v.PhoneNo3 = data.PhoneNo3;
                    v.PhoneNo4 = data.PhoneNo4;
                    v.MobileNo1 = data.MobileNo1;
                    v.MobileNo2 = data.MobileNo2;
                    v.EMail = data.EMail;
                    v.Website = data.Website;
                    v.BranchPrefix = data.BranchPrefix;
                    v.CurrencyID = data.CurrencyID.Value;
                    v.AcCompanyID = data.AcCompanyID.Value;
                    v.StatusAssociate = data.StatusAssociate.Value;
                    v.AWBFormat = data.AWBFormat;
                    v.InvoicePrefix = data.InvoicePrefix;
                    v.InvoiceFormat = data.InvoiceFormat;
                    v.AcFinancialYearID = Convert.ToInt32(data.AcFinancialYearID);
                    if (data.ImportVatThreshold == null)
                        v.ImportVatThreshold = 0;
                    else
                        v.ImportVatThreshold = Convert.ToDecimal(data.ImportVatThreshold);
                    v.DRRProcess = data.DRRProcess;
                    if (data.VATAccountId != null)
                    {
                        v.VATAccountId = Convert.ToInt32(data.VATAccountId);
                    }
                    else
                    {
                        v.VATAccountId = 0;
                    }
                    if (data.VATPercent == null)
                    { v.VATPercent = 0; }
                    else
                    {
                        v.VATPercent = Convert.ToDecimal(data.VATPercent);
                    }

                    v.VATRegistrationNo = data.VATRegistrationNo;
                    v.RateType =Convert.ToInt32(data.CustomerRateType);
                }
            }catch(Exception ex)
            {
                TempData["ErrorMsg"] = ex.Message;

            }
          
            return View(v);
    
        }

        //
        // POST: /BranchMaster/Edit/5

        [HttpPost]
      
        public ActionResult Edit(BranchVM b)
        {
            ViewBag.years = db.AcFinancialYears.ToList();
            
            
             
            ViewBag.designation = db.Designations.ToList();
            ViewBag.currency = db.CurrencyMasters.ToList();
            var x1 = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).OrderBy(c => c.AcHead).ToList();

            ViewBag.heads = x1;
            try
            {
                BranchMaster a = new BranchMaster();
                a = db.BranchMasters.Find(b.BranchID);
                //a.BranchID = b.BranchID;
                a.BranchName = b.BranchName;
                a.Address1 = b.Address1;
                a.Address2 = b.Address2;
                a.Address3 = b.Address3;
                //a.CountryID = 1; // item.CountryID;
                //a.CityID = 19; // item.CityID;
                //a.LocationID = 7; // item.LocationID;
                a.CountryName = b.CountryName;
                a.CityName = b.CityName;
                a.LocationName = b.LocationName;
                a.KeyPerson = b.KeyPerson;
                a.DesignationID = b.DesignationID;
                a.Phone = b.Phone;
                a.PhoneNo1 = b.PhoneNo1;
                //a.PhoneNo2 = b.PhoneNo2;
                //a.PhoneNo3 = b.PhoneNo3;
                //a.PhoneNo4 = b.PhoneNo4;
                a.MobileNo1 = b.MobileNo1;
                //a.MobileNo2 = b.MobileNo2;
                a.EMail = b.EMail;
                a.Website = b.Website;
                a.BranchPrefix = b.BranchPrefix;
                a.CurrencyID = b.CurrencyID;
                a.AcCompanyID = b.AcCompanyID;
                a.StatusAssociate = b.StatusAssociate;
                a.AWBFormat = b.AWBFormat;
                a.InvoicePrefix = b.InvoicePrefix;
                a.InvoiceFormat = b.InvoiceFormat;
                a.VATPercent = b.VATPercent;
                a.VATRegistrationNo = b.VATRegistrationNo;
                a.AcFinancialYearID = b.AcFinancialYearID;
                a.DRRProcess = b.DRRProcess;
                a.ImportVatThreshold = b.ImportVatThreshold;
                if (b.VATAccountId == 0)
                {
                    a.VATAccountId = null;
                }
                else
                {
                    a.VATAccountId = b.VATAccountId;
                }

                a.CustomerRateType = b.RateType;

                //if (ModelState.IsValid)
                //{
                db.Entry(a).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "You have successfully Updated Branch.";
                return RedirectToAction("Index");
                //}
            }
            catch(Exception ex)
            {
                TempData["ErrorMsg"] = ex.Message;

            }
            return View(b);
        
        }

        //
        // GET: /BranchMaster/Delete/5

        //public ActionResult Delete(int id=0)
        //{
        //    BranchMaster branchmaster = db.BranchMasters.Find(id);
        //    if (branchmaster == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(branchmaster);
        //}

        //
        // POST: /BranchMaster/Delete/5

        //[HttpPost, ActionName("Delete")]
      
        
        public JsonResult DeleteConfirmed(int id)
        {
            //int k = 0;
            if (id != 0)
            {
                var inscan = db.InScanMasters.Where(cc => cc.BranchID == id).FirstOrDefault();
                if (inscan == null)
                {
                    BranchMaster branchmaster = db.BranchMasters.Find(id);
                    db.BranchMasters.Remove(branchmaster);
                    db.SaveChanges();
                    string status = "OK";
                    string message = "You have successfully Deleted Branch.";
                    return Json(new { status = status, message = message });

                }
                else
                {
                    string status = "Failed";
                    string message = "Branch could not delete,Transactions exists!";
                    return Json(new { status = status, message = message });
                }                
            }

            return Json(new { status = "OK", message = "Contact Admin!" });


        }
        public ActionResult GeneralSetup(int id)
        {
            int setuptypeid = 0;
            if (Session["SetupTypeID"] != null)
            {
                setuptypeid = Convert.ToInt32(Session["SetupTypeID"].ToString());
            }
            else
            {
                Session["SetupTypeID"] = 1;
                Session["SetupBranchID"] = id;
                setuptypeid = 1;
            }
            GeneralSetup v = new GeneralSetup();
            GeneralSetupVM vm = new GeneralSetupVM();
            v = db.GeneralSetups.Where(cc => cc.BranchId == id && cc.SetupTypeID == setuptypeid).FirstOrDefault();

            if (v == null)
            {

                vm.SetupID = 0;
                vm.BranchId = id;
                vm.SetupTypeID = setuptypeid; // db.GeneralSetupTypes.FirstOrDefault().ID;
                vm.TextDoc = "";
                vm.Text1 = "";
                vm.Text2 = "";
                vm.Text3 = "";
                vm.Text4 = "";
                vm.Text5 = "";
            }
            else
            {
                vm.SetupID = v.SetupID;
                vm.BranchId = v.BranchId;
                vm.SetupTypeID = v.SetupTypeID;
                vm.TextDoc = v.Text1;
                vm.Text1 = v.Text1;
                vm.Text2 = v.Text2;
                vm.Text3 = v.Text3;
                vm.Text4 = v.Text4;
                vm.Text5 = v.Text5;            
                

            }
            
            ViewBag.SetupType = db.GeneralSetupTypes.ToList();
            return View(vm);

        }

        [HttpPost]
        public ActionResult GeneralSetup(GeneralSetupVM vm)
        {
            int companyid = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            GeneralSetup v = new GeneralSetup();
            v = db.GeneralSetups.Where(cc => cc.BranchId == vm.BranchId && cc.SetupTypeID == vm.SetupTypeID).FirstOrDefault();

            if (v==null)
            {
                v = new GeneralSetup();
                int max = (from d in db.GeneralSetups orderby d.SetupID descending select d.SetupID).FirstOrDefault();
                v.SetupID = max + 1; ;
                v.BranchId = vm.BranchId;
                v.AcCompanyId = companyid;
                v.SetupTypeID = vm.SetupTypeID;
                //v.Text1 = vm.TextDoc;
                if (v.SetupTypeID != 1)
                {
                    v.Text1 = vm.TextDoc;
                }
                else if(v.SetupID==1)
                {
                    v.Text1 = vm.Text1;
                    v.Text2 = vm.Text2;
                    v.Text3 = vm.Text3;
                    v.Text4 = vm.Text4;
                    v.Text5 = vm.Text5;
                }
                db.GeneralSetups.Add(v);
                db.SaveChanges();
            }
            else
            {
                if (v.SetupTypeID != 1)
                {
                    v.Text1 = vm.TextDoc;
                }
                else if (v.SetupID==1) //invoice footer
                {
                    v.Text1 = vm.Text1; // Doc;
                    v.Text2 = vm.Text2;
                    v.Text3 = vm.Text3;
                    v.Text4 = vm.Text4;
                    v.Text5 = vm.Text5;
                }

                db.Entry(v).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("Index");

        }

        public ActionResult SetupType()
        {
            GeneralSetupVM vm = new GeneralSetupVM();
            if (Session["SetupTypeID"] !=null) {
                int setuptypeid = Convert.ToInt32(Session["SetupTypeID"].ToString());
                vm.SetupTypeID = setuptypeid;
           }
            int branchid = Convert.ToInt32(Session["SetupBranchId"]);
            ViewBag.SetupType = db.GeneralSetupTypes.ToList();
            ViewBag.BranchName = db.BranchMasters.Find(branchid).BranchName;
            return View(vm);
        }


        [HttpPost]
        public ActionResult SetupType(GeneralSetupVM vm)
        {

            if (vm.SetupTypeID != null)
                Session["SetupTypeID"] = vm.SetupTypeID;

            int branchid = Convert.ToInt32(Session["SetupBranchId"]);

            return RedirectToAction("GeneralSetup", "BranchMaster", new {id= branchid});                      
            
        }
        [HttpGet]
        public JsonResult GetBranches()
        {
            var lstcourier = (from c in db.BranchMasters select new { BranchID = c.BranchID, BranchName = c.BranchName }).ToList();
            return Json(new { data = lstcourier }, JsonRequestBehavior.AllowGet);
        }
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public JsonResult GetCity(int id)
        {
            List<CityM> objCity = new List<CityM>();
            var city = (from c in db.CityMasters where c.CountryID == id select c).ToList();

            foreach (var item in city)
            {
                objCity.Add(new CityM { City = item.City, CityID = item.CityID });

            }
            return Json(objCity, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLocation(int id)
        {
            List<LocationM> objLoc = new List<LocationM>();
            var loc = (from c in db.LocationMasters where c.CityID == id select c).ToList();

            foreach (var item in loc)
            {
                objLoc.Add(new LocationM { Location = item.Location, LocationID = item.LocationID });

            }
            return Json(objLoc, JsonRequestBehavior.AllowGet);
        }

        public class CityM
        {
            public int CityID { get; set; }
            public String City { get; set; }
        }

        public class LocationM
        {
            public int LocationID { get; set; }
            public String Location { get; set; }
        }
    }
}