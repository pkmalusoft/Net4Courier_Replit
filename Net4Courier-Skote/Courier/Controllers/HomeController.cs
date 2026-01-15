using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using HealthCareApp;

namespace Net4Courier.Controllers
{
    public class HomeController : Controller
    {
     
        Entities1 db=new Entities1();


        public ActionResult Home()
        {
                var compdetail = db.AcCompanies.FirstOrDefault();
            ViewBag.CompanyName = compdetail.AcCompany1;
            ViewBag.ContactPerson = compdetail.KeyPerson;
            ViewBag.ContactTelephone = compdetail.Phone;
            ViewBag.Email = compdetail.EMail;
            ViewBag.BranchCount = db.BranchMasters.Count();

            if (compdetail.AcceptSystem == true)
                ViewBag.AcceptSystem = "YES";
            else
                ViewBag.AcceptSystem = "NO";

            if (Session["LoginStatus"]!=null)
            {
                TempData["LoginErrorMsg"] = Session["StatusMessage"].ToString();
                TempData["Modal"] = "Login";
            }
            else if (Session["ForgotStatus"] != null)
            {
                TempData["ForgotErrorMsg"] = Session["StatusMessage"].ToString();
                TempData["Modal"] = "Forgot";
            }
            else if (Session["ResetStatus"] != null)
            {
                TempData["ResetErrorMsg"] = Session["StatusMessage"].ToString();
                TempData["Modal"] = "Reset";
            }
            else
            {
                TempData["LoginErrorMsg"] = null;
                TempData["Modal"] = null;
                TempData["ResetErrorMsg"] = null;
                TempData["LoginErrorMsg"] = null;
            }

            Session["LoginStatus"] = null;
            Session["StatusMessage"] = null;
            Session["ResetStatus"] = null;
            Session["ForgotStatus"] = null;
            return View();
            
           
        }

        public ActionResult Index()
        {

            List<ImpExpVM> lst = (from c in db.ImpExps join e in db.UserRegistrations on c.Employee equals e.UserID select new ImpExpVM { ImpExpID = c.ImpExpID, fname = c.FName, EmpName = e.UserName, ImpDate = c.ImportDate, status = c.Status }).ToList();


            return View(lst);
        }
        public ActionResult TrialExpireIndex()
        {
            var compdetail = db.AcCompanies.FirstOrDefault();
            ViewBag.CompanyName = compdetail.AcCompany1;
            ViewBag.ContactPerson = compdetail.KeyPerson;
            ViewBag.ContactTelephone = compdetail.Phone;
            ViewBag.Email = compdetail.EMail;
            ViewBag.BranchCount = db.BranchMasters.Count();
            return View();
        }
        [HttpPost]
        public ActionResult TrialExpire(ComapanyVM u)
        {
            var accomp = u;
            if (u.Accept == true)
            {
                var accompany = db.AcCompanies.FirstOrDefault();
                
                accompany.AcceptSystem = true;
                db.Entry(accompany).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["TrialSuccessMessage"] = "Thank you for Accept our System!";
            }
            else
            {
                TempData["TrialMessage"] = "Sorry to see you go. Hope to be of service in future. Thank you. !!";
            }
                
            return RedirectToAction("Home", "Home");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="formCollection"></param>
        /// <param name="Command"></param>
        /// <returns></returns>
        public ActionResult UploadFile(FormCollection formCollection, string Command)
        {
            if (Request != null)
            {
                Session["fname"] = formCollection["fname"].ToString();

                string x = Session["fname"].ToString();
                HttpPostedFileBase filebase = Request.Files["UploadedFile"];
                if (!string.IsNullOrEmpty(filebase.FileName))
                {
                    if ((filebase.FileName.Contains(".xlsx") || filebase.FileName.Contains(".xls")))
                    {
                        MappingManager manager = new MappingManager();
                        TempData["SelectedFile"] = filebase;
                        return RedirectToAction("Mapping", "Mapping");
                    }
                    else
                    {
                        TempData["Message"] = "Please select excel file only.";
                    }
                }
                else
                {
                    TempData["Message"] = "Please select file.";
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult UnderDevelopment()
        {
            return View();
        }


        [HttpPost]
        public JsonResult ChangeBranch(int id)
        {
            try
            {
                var branch = db.BranchMasters.Find(id);
                tblDepot depot = db.tblDepots.Where(cc => cc.BranchID == branch.BranchID).FirstOrDefault();

                //Year Setting                


                //int? branchid = (from u2 in db.tblDepots where u2.ID == id select u2.BranchID).FirstOrDefault();
                Session["CurrentBranchID"] = depot.BranchID;
                Session["CurrentDepotID"] = depot.ID; // id;
                Session["CurrentDepot"] = branch.BranchName;
                Session["YearEndProcessSearch"] = null;
                int startyearid =Convert.ToInt32(db.BranchMasters.Find(depot.BranchID).AcFinancialYearID);
                DateTime branchstartdate =Convert.ToDateTime(db.AcFinancialYears.Find(startyearid).AcFYearFrom);

                var allyear = (from c in db.AcFinancialYears where c.AcFYearFrom>=branchstartdate select c).OrderByDescending(cc=>cc.AcFYearFrom).ToList();
                Session["FYear"] = allyear;

                return Json(new { status = "ok", depotname = depot.Depot, message = "Active Depot Selection changed to " + depot.Depot }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { status = "Failed", depotname = "", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public JsonResult ChangeYear(int id)
        {
            try
            {

                AcFinancialYear finacialyear = db.AcFinancialYears.Find(id);
                if (finacialyear != null)
                {
                    Session["fyearid"] = finacialyear.AcFinancialYearID;
                    Session["CurrentYear"] = (finacialyear.AcFYearFrom.Value.Date.ToString("dd MMM yyyy") + " - " + finacialyear.AcFYearTo.Value.Date.ToString("dd MMM yyyy"));
                    Session["FyearFrom"] = finacialyear.AcFYearFrom;
                    Session["FyearTo"] = finacialyear.AcFYearTo;
                    Session["YearEndProcessSearch"] = null;
                }

                return Json(new { status = "ok", yearname = finacialyear.ReferenceName, message = "Financial Year Selected Changed to " + finacialyear.ReferenceName }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = "Failed",  message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
