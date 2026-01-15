using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Configuration;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using Net4Courier.DAL;
using System.Data.Entity;

namespace Net4Courier.Controllers
{
    [SessionExpire]
    //   [Authorize]
    public class DRSExpenseController : Controller
    {
        SourceMastersModel MM = new SourceMastersModel();
        RecieptPaymentModel RP = new RecieptPaymentModel();
        CustomerRcieptVM cust = new CustomerRcieptVM();
        Entities1 Context1 = new Entities1();

        EditCommanFu editfu = new EditCommanFu();       
   
        [HttpGet]
        public ActionResult Index( )
        {
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            ViewBag.Deliverdby = Context1.EmployeeMasters.Where(cc => cc.BranchID == BranchID).ToList().OrderBy(cc => cc.EmployeeName);
            CourierExpenseSearch obj = (CourierExpenseSearch)Session["CourierExpenseSearch"];
            CourierExpenseSearch model = new CourierExpenseSearch();
            AWBDAO _dao = new AWBDAO();
            if (obj != null)
            {
                List<CourierExpensesVM> translist = new List<CourierExpensesVM>();

                model.FromDate = obj.FromDate;
                model.ToDate = obj.ToDate;
                model.EmployeeID = obj.EmployeeID;
            
                translist = ReceiptDAO.GetDRSExpenses(FyearId, model.FromDate, model.ToDate,model.EmployeeID);
                model.Details = translist;
            }
            else
            {
                model.FromDate = CommonFunctions.GetLastDayofMonth().Date;
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
                Session["CourierExpenseSearch"] = model;
                List<CourierExpensesVM> translist = new List<CourierExpensesVM>();
                translist = ReceiptDAO.GetDRSExpenses(FyearId, model.FromDate, model.ToDate,model.EmployeeID);
                model.Details = translist;

            }
            return View(model);
                                
           
        }
        [HttpPost]
        public ActionResult Index(CourierExpenseSearch obj)
        {

            Session["CourierExpenseSearch"] = obj;
            return RedirectToAction("Index");
        }
            [HttpGet]
        public ActionResult Create(int id=0)
        {
            var branchid = Convert.ToInt32(Session["CurrentBranchID"]);
            ViewBag.Deliverdby = Context1.EmployeeMasters.Where(cc=>cc.BranchID == branchid).ToList().OrderBy(cc=>cc.EmployeeName);
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            CourierExpensesVM cust = new CourierExpensesVM();
            ViewBag.Revenuetypes = Context1.RevenueTypes.Where(cc => cc.RevenueGroup == "Cost" && cc.BranchID == branchid).OrderBy(cc => cc.RevenueType1).ToList();
            if (Session["UserID"] != null)
            {
              

                if (id > 0)
                {
                    ViewBag.Title = "Modify";
                    
                    CourierExpens dpay = Context1.CourierExpenses.Find(id);
                    if (dpay != null)
                    {

                        cust.ExpenseID = dpay.ExpenseID;
                        cust.ExpenseDate = dpay.ExpenseDate;
                        cust.DRSID = dpay.DRSID;
                        cust.RevenueTypeID = dpay.RevenueTypeID;
                        cust.ExpenseAmount = dpay.ExpenseAmount;
                        cust.EmployeeID = Convert.ToInt32(dpay.EmployeeID);
                        cust.IsLocked = dpay.IsLocked;
                        
                        cust.DRSNo = "";
                        if (dpay.DRSID != null && dpay.DRSID != 0)
                        {
                            var drsno = Context1.DRS.Find(cust.DRSID);
                            if (drsno != null)
                            {
                                cust.DRSNo = drsno.DRSNo;
                                cust.DRSDate = drsno.DRSDate;
                                cust.DRSAmount = Convert.ToDecimal(drsno.TotalCourierCharge);
                            }
                        }

                       
                        cust.Remarks = dpay.Remarks;
                        

                        //BindMasters_ForEdit(cust);
                    }
                }
                else
                {
                    ViewBag.Title = "Create";
                   

                    DateTime pFromDate = CommonFunctions.GetCurrentDateTime(); // AccountsDAO.CheckParamDate(DateTime.Now, FyearId).Date;
                    cust.ExpenseDate = pFromDate;
                    cust.ExpenseID = 0;                                       
                    
                }
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
           
            return View(cust);

        }

        [HttpPost]
        public ActionResult Create(CourierExpensesVM vm)
        {
            int fyearid = Convert.ToInt32(Session["fyearid"]);
            int companyid = Convert.ToInt32(Session["CurrentCompanyID"]);
            int branchid = Convert.ToInt32(Session["CurrentBranchID"]);
            
            CourierExpens v = new CourierExpens();
            int RPID = 0;
             
            if (vm.ExpenseID ==0)
            {
                
                v.FYearID = fyearid;
                v.BranchID = branchid;
                //string DocNo = ReceiptDAO.GetMaxDRSReceiptNO();
                //vm.DocumentNo = DocNo;
                vm.UserID= Convert.ToInt32(Session["UserID"]);
                v.EmployeeID = vm.EmployeeID;
                v.DRSID = vm.DRSID;
                v.RevenueTypeID = vm.RevenueTypeID;
                v.ExpenseAmount = vm.ExpenseAmount;
                v.ExpenseDate = vm.ExpenseDate;
                v.Remarks = vm.Remarks;
                Context1.CourierExpenses.Add(v);
                Context1.SaveChanges();

            }
            else
            {               
               
               CourierExpens recpay = Context1.CourierExpenses.Find(vm.ExpenseID);
                if (recpay != null)
                {
                    recpay.EmployeeID = vm.EmployeeID;
                    recpay.DRSID = vm.DRSID;
                    recpay.RevenueTypeID = vm.RevenueTypeID;
                    recpay.ExpenseAmount = vm.ExpenseAmount;
                    recpay.ExpenseDate = vm.ExpenseDate;
                    recpay.Remarks = vm.Remarks;
                    Context1.Entry(recpay).State = EntityState.Modified;
                    Context1.SaveChanges();

                }
            }
           


            return RedirectToAction("Index", "DRSExpense", new { id=0 });

        }
        [HttpGet]
        public JsonResult GetDRSNo(string term,string DeliveredBy="",int RecPayId=0)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            int DeliveredId=0;
            if (DeliveredBy != "")
                DeliveredId = Convert.ToInt32(DeliveredBy);
            var details = ReceiptDAO.GetDRSSummary(term,DeliveredId,RecPayId,branchid,FyearId);
            return Json(details, JsonRequestBehavior.AllowGet);


            //if (term.Trim() != "")
            //{
            //    var drslist = (from c1 in Context1.DRS
            //                   where c1.DRSNo.ToLower().Contains(term.Trim().ToLower())
            //                   && (c1.DRSRecPayId==null || c1.DRSRecPayId == 0 || c1.DRSRecPayId==RecPayId)  && (c1.DeliveredBy== DeliveredId || DeliveredBy=="")
            //                   && (c1.TotalMaterialCost >0 || c1.TotalCourierCharge>0)
            //                   && (c1.Pending==false)
            //                   orderby c1.DRSNo ascending
            //                   select new {DRSID=c1.DRSID, DRSNo = c1.DRSNo, DRSDate = c1.DRSDate,TotalAmount=c1.TotalCourierCharge + c1.TotalMaterialCost }).ToList();

            //    return Json(drslist, JsonRequestBehavior.AllowGet);
            //}
            //else
            //{
            //    var drslist = (from c1 in Context1.DRS
            //                   where (c1.DRSRecPayId == null || c1.DRSRecPayId == 0 || c1.DRSRecPayId == RecPayId) && (c1.DeliveredBy == DeliveredId || DeliveredBy == "")
            //                   && (c1.TotalMaterialCost > 0 || c1.TotalCourierCharge > 0)
            //                   && (c1.Pending == false)
            //                   orderby c1.DRSNo ascending
            //                   select new {DRSID=c1.DRSID, DRSNo=c1.DRSNo ,DRSDate=c1.DRSDate, TotalAmount = c1.TotalCourierCharge + c1.TotalMaterialCost}).ToList();

            //    return Json(drslist, JsonRequestBehavior.AllowGet);
            //}

        }

         

  
        
        [HttpPost]
        public JsonResult DeleteConfirmed(int id)
        {
            //int k = 0;
            if (id != 0)
            {
                DataTable dt = ReceiptDAO.DeleteCourierExpenses(id);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        string status = dt.Rows[0][0].ToString();
                        string message = dt.Rows[0][1].ToString();
                       
                        return Json(new { status = status, message = message });
                    }

                }
                else
                {
                    return Json(new { status = "OK", message = "Contact Admin!" });
                }
            }

            return Json(new { status = "Failed", message = "Contact Admin!" });

        }

        public ActionResult PrintVoucher(int id = 0)
        {
            int uid = Convert.ToInt32(Session["UserID"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());

            AccountsReportsDAO.GenerateDRSReceiptVoucher(id);
            ViewBag.ReportName = "DRS Receipt Voucher";
            return View();

        }

        
    }
}
