using Net4Courier.DAL;
using Net4Courier.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Net4Courier.Controllers
{
    [SessionExpireFilter]
    public class AWBBookIssueController : Controller
    {
        Entities1 db = new Entities1();
        // GET: AWBBookIssue
        public ActionResult Index()
        {
          
            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            AWBBookIssueSearch obj = (AWBBookIssueSearch)Session["AWBBookIssueSearch"];
            AWBBookIssueSearch model = new AWBBookIssueSearch();
            AWBDAO _dao = new AWBDAO();
            if (obj != null)
            {
                List<AWBBookIssueList> translist = new List<AWBBookIssueList>();
                translist = AWBDAO.GetAWBBookIssue(BranchID);
                model.FromDate = obj.FromDate;
                model.ToDate = obj.ToDate;
                model.DocumentNo = obj.DocumentNo;                
                model.Details = translist;
            }
            else
            {   model.FromDate =CommonFunctions.GetLastDayofMonth().Date;
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
                Session["AWBBookIssueSearch"] = model;
                List<AWBBookIssueList> translist = new List<AWBBookIssueList>();
                translist = AWBDAO.GetAWBBookIssue(BranchID);
                model.Details = translist;

            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(AWBBookIssueSearch obj)
        {
            Session["AWBBookIssueSearch"] = obj;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Create(int id = 0)
        {
            int AcCompanyID = Convert.ToInt32(Session["CurrentCompanyID"].ToString());
            var branchid = Convert.ToInt32(Session["CurrentBranchID"]);
            ViewBag.Employee = db.EmployeeMasters.ToList();
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            AWBBookIssueVM vm = new AWBBookIssueVM();
            vm.Details = new List<AWBDetailVM>();
            if (Session["UserID"] != null)
            {
              

                if (id > 0)
                {
                    ViewBag.Title = "Modify";

                    AWBBOOKIssue dpay = db.AWBBOOKIssues.Find(id);
                    vm.Documentno = dpay.Documentno;
                    vm.AWBBOOKIssueID = dpay.AWBBOOKIssueID;
                    vm.TransDate = dpay.TransDate;                    
                    vm.EmployeeID = Convert.ToInt32(dpay.EmployeeID);
                    vm.NoOfAWBs = dpay.NoOfAWBs;                    
                    vm.BookNo = dpay.BookNo;
                    vm.EmployeeID = dpay.EmployeeID;
                    vm.AcCompanyID = dpay.AcCompanyID;
                    vm.AWBNOFrom = dpay.AWBNOFrom;
                    vm.AWBNOTo= dpay.AWBNOTo;
                    List<AWBDetailVM> details = new List<AWBDetailVM>();
                    //details = (from c in db.AWBDetails where c.AWBBookIssueID == id select new AWBDetailVM { AWBNo = c.AWBNo}).ToList();

                    details = AWBDAO.GetAWBBookIssueDetail(id);
                    vm.Details = details;

                }
                else
                {
                    ViewBag.Title = "Create";
                    string DocNo = AWBDAO.GetMaxAWBBookIssueDocumentNo();                    

                    DateTime pFromDate = AccountsDAO.CheckParamDate(DateTime.Now, FyearId).Date;
                    vm.TransDate= pFromDate;
                    vm.AWBBOOKIssueID = 0;
                    vm.Documentno = DocNo;
                    vm.AcCompanyID = AcCompanyID;
                }
            }
            else
            {
                return RedirectToAction("Home", "Home");
            }

            return View(vm);

        }

        [HttpPost]
        public ActionResult Create(AWBBookIssueVM vm)
        {
            int fyearid = Convert.ToInt32(Session["fyearid"]);
            int companyid = Convert.ToInt32(Session["CurrentCompanyID"]);
            int branchid = Convert.ToInt32(Session["CurrentBranchID"]);
            int UserId = Convert.ToInt32(Session["UserID"]);
            AWBBOOKIssue v = new AWBBOOKIssue();
            int RPID = 0;
            
            if (vm.AWBBOOKIssueID == 0)
            {
                v.AcCompanyID = companyid;
                v.BranchID = branchid;
                //vm.BookNo = branchid;                                
                v.Documentno = vm.Documentno;
                v.BookNo = vm.BookNo;
                v.AWBNOFrom = vm.AWBNOFrom;
                v.AWBNOTo = vm.AWBNOTo;
                v.EmployeeID = vm.EmployeeID;
                v.NoOfAWBs = vm.NoOfAWBs;
                v.TransDate = vm.TransDate;
                v.CreatedUserID = UserId;
                v.CreatedDate = DateTime.Now;
                v.ModifiedUserID = UserId;
                v.ModifiedDate = DateTime.Now;
                db.AWBBOOKIssues.Add(v);
                db.SaveChanges();
                //generate awb in inscantables
                AWBDAO.GenerateAWBBookIssue(v.AWBBOOKIssueID);
            }
            else
            {
                v = db.AWBBOOKIssues.Find(vm.AWBBOOKIssueID);
                v.Documentno = vm.Documentno;
                v.BookNo = vm.BookNo;
                v.AWBNOFrom = vm.AWBNOFrom;
                v.AWBNOTo = vm.AWBNOTo;
                v.EmployeeID = vm.EmployeeID;
                v.NoOfAWBs = vm.NoOfAWBs;
                v.TransDate = vm.TransDate;
                v.ModifiedUserID = UserId;
                v.ModifiedDate = DateTime.Now;
                db.Entry(v).State = EntityState.Modified;
                db.SaveChanges();
                //generate awb in inscantables
                AWBDAO.GenerateAWBBookIssue(v.AWBBOOKIssueID);
            }
            
          

            return RedirectToAction("Index", "AWBBookIssue", new { id = 0 });

        }

        [HttpGet]
        public JsonResult GetAWBBook(string StartAWB,string EndAWB,int AWBBookIssueId=0,int PrepaidAWBID=0)
        {
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            string result = AWBDAO.CheckAWBStock(Convert.ToInt32(StartAWB), Convert.ToInt32(EndAWB));
            if (result == "ok")
            {

                List<AWBDetailVM> listawb = AWBDAO.CheckAWBDuplicate(Convert.ToInt32(StartAWB), Convert.ToInt32(EndAWB), AWBBookIssueId, PrepaidAWBID);

                if (listawb.Count == 0)
                {
                    List<AWBDetailVM> awbs = new List<AWBDetailVM>();

                    for (int i = Convert.ToInt32(StartAWB); i <= Convert.ToInt32(EndAWB); i++)
                    {
                        AWBDetailVM obj = new AWBDetailVM();
                        //obj.AWBBOOKIssueID = 0;
                        //obj.AWBBOOKIssueDetailID = 0;
                        obj.AWBNo = i.ToString();
                        awbs.Add(obj);
                    }
                    Session["AWBBook"] = awbs;
                    return Json(new { status = "ok", awbs = awbs }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = "Failed", message = "Duplicate AWB , " + listawb.Count.ToString() + " AWBs are exist!" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { status = "Failed", message = result }, JsonRequestBehavior.AllowGet);
            }
            

        }

        public ActionResult DeleteConfirmed(int id)
        {
            //int k = 0;
            if (id != 0)
            {
                DataTable dt = AWBDAO.DeleteAWBCourier(id);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        //if (dt.Rows[0][0] == "OK")
                        TempData["SuccessMsg"] = dt.Rows[0][1].ToString();
                    }

                }
                else
                {
                    TempData["ErrorMsg"] = "Error at delete";
                }
            }

            return RedirectToAction("Index");


        }
    }
}