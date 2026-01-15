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
    public class DRSReceiptController : Controller
    {
        SourceMastersModel MM = new SourceMastersModel();
        RecieptPaymentModel RP = new RecieptPaymentModel();
        CustomerRcieptVM cust = new CustomerRcieptVM();
        Entities1 Context1 = new Entities1();

        EditCommanFu editfu = new EditCommanFu();       
   
      

        public JsonResult GetExchangeRateByCurID(string ID)
        {
            //List<SP_GetCustomerInvoiceDetailsForReciept_Result> AllInvoices = new List<SP_GetCustomerInvoiceDetailsForReciept_Result>();

            var ER = RP.GetExchgeRateByCurID(Convert.ToInt32(ID));

            return Json(ER, JsonRequestBehavior.AllowGet);
        }







        [HttpGet]
        public ActionResult Index()
        {
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            DRSReceiptSearch obj = (DRSReceiptSearch)Session["DRSReceiptSearch"];
            DRSReceiptSearch model = new DRSReceiptSearch();
            DateTime pFromDate;
            DateTime pToDate;
            if (obj == null)
            {
                obj = new DRSReceiptSearch();
                pFromDate = CommonFunctions.GetLastDayofMonth().Date;//.AddDays(-1); // FromDate = DateTime.Now;
                pToDate = CommonFunctions.GetLastDayofMonth().Date; // // ToDate = DateTime.Now;

                pFromDate = AccountsDAO.CheckParamDate(pFromDate, FyearId).Date;
                pToDate = AccountsDAO.CheckParamDate(pToDate, FyearId).Date;
                obj.FromDate = pFromDate;
                obj.ToDate = pToDate;
                obj.DocumentNo = "";
                Session["DRSReceiptSearch"] = obj;
                model = obj;

            }
            else
            {
                model = obj;

                StatusModel statu = AccountsDAO.CheckDateValidate(obj.FromDate.ToString(), FyearId);
                string vdate = statu.ValidDate;
                model.FromDate = Convert.ToDateTime(vdate);
                model.ToDate = Convert.ToDateTime(AccountsDAO.CheckDateValidate(obj.ToDate.ToString(), FyearId).ValidDate);
                Session["DRSReceiptSearch"] = model;
            }

            List<DRSReceiptVM>  Receipts = ReceiptDAO.GetDRSReceipts(FyearId, model.FromDate, model.ToDate,model.DocumentNo); // RP.GetAllReciepts();
            model.Details = Receipts;     

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(DRSReceiptSearch obj)
        {
            Session["DRSReceiptSearch"] = obj;
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult Create(int id=0)
        {
            ViewBag.Deliverdby = Context1.EmployeeMasters.ToList().OrderBy(cc=>cc.EmployeeName);
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            DRSReceiptVM cust = new DRSReceiptVM();
            cust.Details= new List<DRSReceiptDetailVM>();            
            if (Session["UserID"] != null)
            {
                var branchid = Convert.ToInt32(Session["CurrentBranchID"]);

                if (id > 0)
                {
                    ViewBag.Title = "Modify";
                    //cust = RP.GetRecPayByRecpayID(id);

                    var acheadforcash = (from c in Context1.AcHeads join g in Context1.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Cash" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    var acheadforbank = (from c in Context1.AcHeads join g in Context1.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    ViewBag.achead = acheadforcash;
                    ViewBag.acheadbank = acheadforbank;
                    DRSRecPay dpay = Context1.DRSRecPays.Find(id);
                    if (dpay != null)
                    {


                        cust.DocumentNo = dpay.DocumentNo;
                        cust.DRSRecPayID = dpay.DRSRecPayID;
                        cust.DRSRecPayDate = dpay.DRSRecPayDate;
                        cust.DRSDate = dpay.DRSDate;
                        var cashOrBankID = (from t in Context1.AcHeads where t.AcHead1 == dpay.BankName select t.AcHeadID).FirstOrDefault();
                        cust.CashBank = (cashOrBankID).ToString();
                        cust.ChequeBank = (cashOrBankID).ToString();
                        cust.ChequeNo = dpay.ChequeNo;
                        cust.ChequeDate = dpay.ChequeDate;
                        cust.DeliveredBy = Convert.ToInt32(dpay.DeliveredBy);
                        cust.PODDate = dpay.PODDate;

                        cust.DRSID = dpay.DRSID;
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

                        cust.DRSBased = dpay.DRSBased;
                        cust.Remarks = dpay.Remarks;
                        cust.ReceivedAmount = dpay.ReceivedAmount;
                        cust.Details = new List<DRSReceiptDetailVM>();

                        //BindMasters_ForEdit(cust);
                    }
                }
                else
                {
                    ViewBag.Title = "Create";
                    string DocNo = ReceiptDAO.GetMaxDRSReceiptNO();
                    //BindAllMasters(2);

                    var acheadforcash = (from c in Context1.AcHeads join g in Context1.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Cash" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    var acheadforbank = (from c in Context1.AcHeads join g in Context1.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();

                    ViewBag.achead = acheadforcash;
                    ViewBag.acheadbank = acheadforbank;

                    DateTime pFromDate = CommonFunctions.GetCurrentDateTime(); // AccountsDAO.CheckParamDate(DateTime.Now, FyearId).Date;
                    cust.DRSRecPayDate = pFromDate;
                    cust.DRSRecPayID = 0;
                    cust.DocumentNo = DocNo;
                    cust.DRSBased = 1;
                    if (Session["PODDate"] != null)
                        cust.PODDate = Convert.ToDateTime(Session["PODDate"].ToString());
                    if (Session["DRSBased"] != null)
                        cust.DRSBased = Convert.ToInt32(Session["DRSBased"].ToString());
                    //cust.CurrencyId = Convert.ToInt32(Session["CurrencyId"].ToString());
                }
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
           
            return View(cust);

        }

        [HttpPost]
        public ActionResult Create(DRSReceiptVM vm)
        {
            int fyearid = Convert.ToInt32(Session["fyearid"]);
            int companyid = Convert.ToInt32(Session["CurrentCompanyID"]);
            int branchid = Convert.ToInt32(Session["CurrentBranchID"]);
            
            DRSRecPay v = new DRSRecPay();
            int RPID = 0;
            if (vm.CashBank != null)
            {
                vm.StatusEntry = "CS";
                int acheadid = Convert.ToInt32(vm.CashBank);
                var achead = (from t in Context1.AcHeads where t.AcHeadID == acheadid select t.AcHead1).FirstOrDefault();
                vm.BankName = achead;
            }
            else
            {
                vm.StatusEntry = "BK";
                int acheadid = Convert.ToInt32(vm.ChequeBank);
                var achead = (from t in Context1.AcHeads where t.AcHeadID == acheadid select t.AcHead1).FirstOrDefault();
                vm.BankName = achead;
            }
            if (vm.DRSRecPayID==0)
            {
                vm.AcCompanyID = companyid;
                vm.FYearID = fyearid;
                vm.BranchId = branchid;
                string DocNo = ReceiptDAO.GetMaxDRSReceiptNO();
                vm.DocumentNo = DocNo;
                vm.UserID= Convert.ToInt32(Session["UserID"]);
                RPID = ReceiptDAO.AddDRSReceipt(vm, Session["UserID"].ToString()); //.AddCustomerRecieptPayment(RecP, Session["UserID"].ToString());
                Session["PODDate"] = vm.PODDate;
                Session["DRSBased"] = vm.DRSBased;
                string CollectionIds = "";
                string ExpenseIds = "";
                if (vm.AWBDetails!=null)
                {
                    foreach(var item in vm.AWBDetails)
                    {
                        if (item.CollectionType != "EXP")
                        {
                            if (CollectionIds == "")
                                CollectionIds = CollectionIds + item.CollectionId;
                            else
                                CollectionIds = CollectionIds + ',' + item.CollectionId;
                        }
                        else
                        {
                            if (ExpenseIds == "")
                                ExpenseIds = ExpenseIds + item.CollectionId;
                            else
                                ExpenseIds = ExpenseIds + ',' + item.CollectionId;

                        }
                    }

                    PickupRequestDAO.SaveCourierCollectedReceiptId(CollectionIds, ExpenseIds, RPID);
                }

            }
            else
            {               
                RPID = vm.DRSRecPayID;
                DRSRecPay recpay = Context1.DRSRecPays.Find(RPID);
                recpay.DRSRecPayDate = vm.DRSRecPayDate;
                recpay.DRSBased = vm.DRSBased;
                recpay.BankName = vm.BankName;
                recpay.ChequeDate = vm.ChequeDate;
                recpay.ChequeNo = vm.ChequeNo;                                                                
                recpay.ReceivedAmount = vm.ReceivedAmount;
                recpay.StatusEntry = vm.StatusEntry;
                recpay.BranchId = branchid;
                recpay.DeliveredBy = vm.DeliveredBy;
                recpay.ReceivedBy = vm.ReceivedBy;
                //recpay.UserID = Convert.ToInt32(Session["UserID"]);
                recpay.FYearID = fyearid;
                recpay.Remarks = vm.Remarks;
                int olddrsid = recpay.DRSID;
                recpay.DRSID = vm.DRSID;
                recpay.ModifiedBy= Convert.ToInt32(Session["UserID"]);
                recpay.ModifiedDate = CommonFunctions.GetBranchDateTime();
                Context1.Entry(recpay).State = EntityState.Modified;
                Context1.SaveChanges();

                if (olddrsid != recpay.DRSID)
                {
                    if (olddrsid > 0)
                    {
                        var _drs = Context1.DRS.Find(olddrsid);
                        _drs.DRSRecPayId = null;
                        Context1.Entry(_drs).State = EntityState.Modified;
                        Context1.SaveChanges();
                    }

                    if (recpay.DRSID > 0)
                    {

                        var _drsnew = Context1.DRS.Find(olddrsid);
                        _drsnew.DRSRecPayId = recpay.DRSRecPayID;
                        Context1.Entry(_drsnew).State = EntityState.Modified;
                        Context1.SaveChanges();
                    }
                }
            }
            //posting
            ReceiptDAO.SaveDRSRecPayPosting(RPID, fyearid, branchid, companyid);


            return RedirectToAction("Create", "DRSReceipt", new { id=0 });

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

        [HttpGet]
        public JsonResult GetDRSAWB(int DRSID,int RecPayId=0)
        {
            var details= ReceiptDAO.GetDRSAWB(DRSID, RecPayId);
           return Json(details, JsonRequestBehavior.AllowGet);
        }

        public static string NumberToWords(int number)
        {
            if (number == 0)
                return "Zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " Million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }
            return words;
        }
        
        [HttpPost]
        public JsonResult DeleteConfirmed(int id)
        {
            //int k = 0;
            if (id != 0)
            {
                DataTable dt = ReceiptDAO.DeleteDRSRecPay(id);
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

        public JsonResult GetCollectedAmount(int collectedby, DateTime Collecteddate,int DRSRecPayID = 0)
        {

            List<CourierCollectionVM> list = new List<CourierCollectionVM>();
            string status = PickupRequestDAO.GetDayClose(collectedby, Collecteddate);

            if (status!="OK")
            {
                return Json(new { status = "Failed", data = 0, message = "Collection Day not closed by the Courier in Skylark!" }, JsonRequestBehavior.AllowGet);
            }
            int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            decimal amount = PickupRequestDAO.GetCollectedAmountByCourier(collectedby, Collecteddate);

            if (amount != 0)
            {
                list = PickupRequestDAO.GetCourierCollectedAWB(collectedby, Collecteddate, DRSRecPayID);
                return Json(new { status = "ok", data = amount, awblist=list, message = "AWB Found" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
              

                return Json(new { status = "Failed", data = 0, message = "AWB Not Found!" }, JsonRequestBehavior.AllowGet);
            }



        }
    }
}
