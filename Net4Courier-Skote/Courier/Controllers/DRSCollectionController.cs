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
    public class DRSCollectionController : Controller
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
            CourierCollectionSearch obj = (CourierCollectionSearch)Session["CourierCollectionSearch"];
            CourierCollectionSearch model = new CourierCollectionSearch();
            AWBDAO _dao = new AWBDAO();
            if (obj != null)
            {
                List<CourierCollectionVM> translist = new List<CourierCollectionVM>();

                model.FromDate = obj.FromDate;
                model.ToDate = obj.ToDate;
                model.EmployeeID = obj.EmployeeID;
            
                translist = ReceiptDAO.GetDRSCourierCollection(FyearId, model.FromDate, model.ToDate,model.EmployeeID);
                model.Details = translist;
            }
            else
            {
                model.FromDate = CommonFunctions.GetLastDayofMonth().Date;
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
                Session["CourierCollectionSearch"] = model;
                List<CourierCollectionVM> translist = new List<CourierCollectionVM>();
                translist = ReceiptDAO.GetDRSCourierCollection(FyearId, model.FromDate, model.ToDate,model.EmployeeID);
                model.Details = translist;

            }
            return View(model);
                                
           
        }
        [HttpPost]
        public ActionResult Index(CourierCollectionSearch obj)
        {

            Session["CourierCollectionSearch"] = obj;
            return RedirectToAction("Index");
        }
            [HttpGet]
        public ActionResult Create(int id=0)
        {
            var branchid = Convert.ToInt32(Session["CurrentBranchID"]);
            ViewBag.Deliverdby = Context1.EmployeeMasters.Where(cc=>cc.BranchID == branchid).ToList().OrderBy(cc=>cc.EmployeeName);
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            CourierCollectionVM cust = new CourierCollectionVM();
            ViewBag.PaymentMode = Context1.tblPaymentModes.ToList();
            if (Session["UserID"] != null)
            {
              

                if (id > 0)
                {
                    ViewBag.Title = "Modify";
                    
                    CourierCollection dpay = Context1.CourierCollections.Find(id);
                    if (dpay != null)
                    {

                        cust.CollectionId = dpay.CollectionID;
                        cust.AWBNo = dpay.AWBNumber;
                        cust.ChangeCOD = dpay.ChangeCOD;
                        if (dpay.codpending ==null)
                            cust.CODPending = false;
                        else
                        cust.CODPending = Convert.ToBoolean(dpay.codpending);
                        cust.CollectionType = dpay.CollectionType.Trim();
                        cust.PaymentModeId = Convert.ToInt32(dpay.PaymentModeId);
                        cust.CollectedDate =Convert.ToDateTime(dpay.CollectedDate);                        
                        cust.PickupCash = Convert.ToInt32(dpay.PickupCash);
                        cust.COD = Convert.ToInt32(dpay.COD);
                        cust.MaterialCost = Convert.ToInt32(dpay.MaterialCost);
                        cust.OtherAmount = Convert.ToInt32(dpay.OtherAmount);
                        cust.COD = Convert.ToInt32(dpay.OtherAmount);
                        if (dpay.DRSRecpayID == null)
                            cust.DRSRecpayID = 0;
                        else
                        {
                            cust.DRSRecpayID = Convert.ToInt32(dpay.DRSRecpayID);
                            var drsrecpay = Context1.DRSRecPays.Find(cust.DRSRecpayID);
                            ViewBag.DRSReceipt = drsrecpay.DocumentNo;
                        }

                        if (dpay.CourierId!=null)
                        {
                            cust.CourierID = Convert.ToInt32(dpay.CourierId);
                            var employee = Context1.EmployeeMasters.Where(cc => cc.UserID == cust.CourierID).FirstOrDefault();
                            if (employee!=null)
                            {
                                cust.EmployeeID = employee.EmployeeID;
                            }
                        }
                        cust.ChangeCOD = Convert.ToBoolean(dpay.ChangeCOD);
                        cust.CODPending = Convert.ToBoolean(dpay.codpending);
                        if (dpay.CustomerId != null)
                        {
                            cust.CustomerId = Convert.ToInt32(dpay.CustomerId);
                            var cust1 = Context1.CustomerMasters.Find(cust.CustomerId);
                            if (cust1 != null)
                                cust.CustomerName = cust1.CustomerName;
                        }
                        else
                        {
                            cust.CustomerId = 0;
                            cust.CustomerName = "";
                        }

                        cust.Remarks = dpay.Remarks;
                        

                        //BindMasters_ForEdit(cust);
                    }
                }
                else
                {
                    ViewBag.Title = "Create";
                   

                    DateTime pFromDate = CommonFunctions.GetCurrentDateTime(); // AccountsDAO.CheckParamDate(DateTime.Now, FyearId).Date;
                    cust.CollectedDate = pFromDate;
                    cust.CourierID = 0;                                       
                    
                }
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
           
            return View(cust);

        }

        [HttpPost]
        public JsonResult SaveCollection(CourierCollectionVM vm)
        {
            int fyearid = Convert.ToInt32(Session["fyearid"]);
            int companyid = Convert.ToInt32(Session["CurrentCompanyID"]);
            int branchid = Convert.ToInt32(Session["CurrentBranchID"]);
            var userid = Convert.ToInt32(Session["UserID"]);
            CourierCollection v = new CourierCollection();
            int RPID = 0;
            try
            {


                if (vm.CollectionId == 0)
                {

                    //v.FYearID = fyearid;
                    //v.BranchID = branchid;
                    //string DocNo = ReceiptDAO.GetMaxDRSReceiptNO();
                    //vm.DocumentNo = DocNo;
                    vm.CourierID = Convert.ToInt32(Session["UserID"]);
                    //v.EmployeeID = vm.EmployeeID;
                    v.AWBNumber = vm.AWBNo;
                    v.PickupCash = vm.PickupCash;
                    v.COD = vm.COD;
                    v.MaterialCost = vm.MaterialCost;
                    v.Remarks = vm.Remarks;
                    v.OtherAmount = vm.OtherAmount;
                    v.PickupCash = vm.PickupCash;
                    v.CustomerId = vm.CustomerId;
                    Context1.CourierCollections.Add(v);
                    Context1.SaveChanges();

                }
                else
                {

                    CourierCollection recpay = Context1.CourierCollections.Find(vm.CollectionId);
                    if (recpay != null)
                    {
                         

                        recpay.codpending = vm.CODPending;
                        recpay.ChangeCOD = vm.ChangeCOD;
                        recpay.PickupCash = vm.PickupCash;
                        recpay.MaterialCost = vm.MaterialCost;
                        recpay.COD = vm.COD;
                        recpay.OtherAmount = vm.OtherAmount;
                         
                        recpay.Remarks = vm.Remarks;
                        recpay.ModifiedDate = CommonFunctions.GetBranchDateTime();
                        recpay.ModifiedBy = userid;
                        Context1.Entry(recpay).State = EntityState.Modified;
                        Context1.SaveChanges();

                    }
                }

                return Json(new { status = "OK", message = "Updated Successfully" });
            }catch(Exception ex)
            {
                return Json(new { status = "Failed", InscanId = 0, message = ex.Message });

            }
        }
       
  
        
        //[HttpPost]
        //public JsonResult DeleteConfirmed(int id)
        //{
        //    //int k = 0;
        //    if (id != 0)
        //    {
        //        DataTable dt = ReceiptDAO.DeleteCourierExpenses(id);
        //        if (dt != null)
        //        {
        //            if (dt.Rows.Count > 0)
        //            {
        //                string status = dt.Rows[0][0].ToString();
        //                string message = dt.Rows[0][1].ToString();
                       
        //                return Json(new { status = status, message = message });
        //            }

        //        }
        //        else
        //        {
        //            return Json(new { status = "OK", message = "Contact Admin!" });
        //        }
        //    }

        //    return Json(new { status = "Failed", message = "Contact Admin!" });

        //}

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
