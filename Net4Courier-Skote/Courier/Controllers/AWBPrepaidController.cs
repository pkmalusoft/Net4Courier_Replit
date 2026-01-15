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
    public class AWBPrepaidController : Controller
    {
        Entities1 db = new Entities1();
        // GET: AWBBookIssue
        public ActionResult Index()
        {

            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            AWBPrepaidSearch obj = (AWBPrepaidSearch)Session["AWBPrepaidSearch"];
            AWBPrepaidSearch model = new AWBPrepaidSearch();
            AWBDAO _dao = new AWBDAO();
            if (obj != null)
            {
                List<AWBPrepaidList> translist = new List<AWBPrepaidList>();
                translist = AWBDAO.GetAWBPrepaid(BranchID);
                model.FromDate = obj.FromDate;
                model.ToDate = obj.ToDate;
                model.DocumentNo = obj.DocumentNo;
                model.Details = translist;
            }
            else
            {   
                model.FromDate = CommonFunctions.GetLastDayofMonth().Date;
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
                Session["AWBPrepaidSearch"] = model;
                List<AWBPrepaidList> translist = new List<AWBPrepaidList>();
                translist = AWBDAO.GetAWBPrepaid(BranchID);
                model.Details = translist;

            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(AWBPrepaidSearch obj)
        {
            Session["AWBPrepaidSearch"] = obj;
            return RedirectToAction("Index");
        }


        public ActionResult Create(int id = 0)
        {
            ViewBag.Employee = db.EmployeeMasters.ToList();
            ViewBag.Department = db.Departments.ToList();
            int FyearId = Convert.ToInt32(Session["fyearid"]);
            AWBPrepaidVM cust = new AWBPrepaidVM();
            cust.Details = new List<AWBDetailVM>();
            if (Session["UserID"] != null)
            {
                var branchid = Convert.ToInt32(Session["CurrentBranchID"]);

                if (id > 0)
                {
                    ViewBag.Title = "Modify";
                    //cust = RP.GetRecPayByRecpayID(id);

                    var acheadforcash = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Cash" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    var acheadforbank = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    ViewBag.achead = acheadforcash;
                    ViewBag.acheadbank = acheadforbank;
                    PrepaidAWB dpay = db.PrepaidAWBs.Find(id);
                    cust.Documentno = dpay.Documentno;
                    cust.PrepaidAWBID = dpay.PrepaidAWBID;
                    cust.TransDate = dpay.TransDate;
                    var cashOrBankID = (from t in db.AcHeads where t.AcHead1 == dpay.BankName select t.AcHeadID).FirstOrDefault();
                    cust.CashBank = (cashOrBankID).ToString();
                    cust.ChequeBank = (cashOrBankID).ToString();
                    cust.ChequeNo = dpay.ChequeNo;
                    cust.ChequeDate = dpay.ChequeDate;                    
                    cust.CustomerID = Convert.ToInt32(dpay.CustomerID);
                    cust.CustomerName = db.CustomerMasters.Find(dpay.CustomerID).CustomerName;
                    cust.AWBNOFrom = dpay.AWBNOFrom;
                    cust.AWBNOTo = dpay.AWBNOTo;
                    cust.NoOfAWBs = dpay.NoOfAWBs;                    
                    cust.Total = dpay.Total;
                    cust.CourierCharge = dpay.CourierCharge;
                    cust.OriginLocation = dpay.OriginLocation;
                    cust.DestinationLocation = dpay.DestinationLocation;
                    cust.Consignee = dpay.Consignee;
                    cust.OriginPlaceID = dpay.OriginPlaceID;
                    cust.PickupSubLocality = dpay.PickupSubLocality;
                    cust.DestinationPlaceID = dpay.DestinationPlaceID;
                    cust.DeliverySubLocality = dpay.DeliverySubLocality;
                    //cust.DepartmentID = dpay.DepartmentID;
                    cust.Reference = dpay.Reference;
                    cust.Prepaid = dpay.Prepaid;
                    
                    cust.Details = new List<AWBDetailVM>();
                    List<AWBDetailVM> details = new List<AWBDetailVM>();
                    //details = (from c in db.AWBDetails where c.PrepaidAWBID == id select new AWBDetailVM { AWBNo = c.AWBNo }).ToList();
                    //cust.Details = details;
                    details = AWBDAO.GetAWBPrepaidDetail(id);
                    cust.Details = details;
                }
                else
                {
                    ViewBag.Title = "Create";
                    string DocNo = AWBDAO.GetMaxPrepaidAWBDocumentNo();
                    
                    var acheadforcash = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Cash" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
                    var acheadforbank = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();

                    ViewBag.achead = acheadforcash;
                    ViewBag.acheadbank = acheadforbank;

                    DateTime pFromDate = CommonFunctions.GetCurrentDateTime();// AccountsDAO.CheckParamDate(DateTime.Now, FyearId).Date;
                    cust.TransDate = pFromDate;
                    cust.PrepaidAWBID = 0;
                    cust.Documentno = DocNo;
                    
                }
            }
            else
            {
                return RedirectToAction("Home", "Home");
            }

            return View(cust);

        }

        [HttpPost]
        public ActionResult Create(AWBPrepaidVM vm)
        {
            int fyearid = Convert.ToInt32(Session["fyearid"]);
            int companyid = Convert.ToInt32(Session["CurrentCompanyID"]);
            int branchid = Convert.ToInt32(Session["CurrentBranchID"]);
            int UserId = Convert.ToInt32(Session["UserID"]);
            PrepaidAWB v = new PrepaidAWB();
            if (vm.Prepaid == true)
            {
                if (vm.CashBank != null)
                {
                    vm.StatusEntry = "CS";
                    int acheadid = Convert.ToInt32(vm.CashBank);
                    var achead = (from t in db.AcHeads where t.AcHeadID == acheadid select t.AcHead1).FirstOrDefault();
                    vm.BankName = achead;
                    vm.AcHeadID = acheadid;
                }
                else
                {
                    vm.StatusEntry = "BK";
                    int acheadid = Convert.ToInt32(vm.ChequeBank);
                    var achead = (from t in db.AcHeads where t.AcHeadID == acheadid select t.AcHead1).FirstOrDefault();
                    vm.BankName = achead;

                    vm.AcHeadID = acheadid;
                }
            }
            else
            {
                vm.StatusEntry = "";
                vm.ChequeDate = null;
                vm.BankName = "";
                vm.AcHeadID = 0;
            }
            if (vm.PrepaidAWBID == 0)
            {
                v.AcCompanyID = companyid;
                v.Prepaid = vm.Prepaid;
                v.CreatedUserID=UserId;
                v.CreatedDate = DateTime.Now;
                v.ModifiedDate = DateTime.Now;
                v.ModifiedUserID = UserId;
                v.AcHeadID = vm.AcHeadID;
                v.BranchID = branchid;
                v.Documentno = vm.Documentno;
                v.TransDate = vm.TransDate;                
                v.StatusEntry = vm.StatusEntry;
                v.BankName = vm.BankName;
                v.NoOfAWBs = vm.NoOfAWBs;
                v.CourierCharge = vm.CourierCharge;
                v.Total = vm.Total;
                v.CustomerID = vm.CustomerID;
                v.OriginLocation = vm.OriginLocation;
                v.DestinationLocation = vm.DestinationLocation;
                v.Consignee = vm.Consignee;
                v.OriginPlaceID = vm.OriginPlaceID;
                v.PickupSubLocality = vm.PickupSubLocality;
                v.DestinationPlaceID = vm.DestinationPlaceID;
                v.DeliverySubLocality = vm.DeliverySubLocality;
                v.ChequeDate = vm.ChequeDate;
                v.ChequeNo = vm.ChequeNo;
                v.AWBNOFrom = vm.AWBNOFrom;
                v.AWBNOTo = vm.AWBNOTo;
                v.FYearId = fyearid;
                v.Reference = vm.Reference;                
                //v.DepartmentID = vm.DepartmentID;
                v.AcjournalID = null;
                db.PrepaidAWBs.Add(v);
                db.SaveChanges();
                //generate awb in AWBDetail
                AWBDAO.GenerateAWBPrepaid(v.PrepaidAWBID);
                AWBDAO.SavePrepaidAWBPosting(v.PrepaidAWBID, fyearid, branchid, companyid);
            }
            else
            {
                PrepaidAWB preawb = db.PrepaidAWBs.Find(vm.PrepaidAWBID);
                preawb.TransDate = vm.TransDate;
                preawb.BankName = vm.BankName;
                preawb.AcHeadID = vm.AcHeadID;
                preawb.ChequeDate = vm.ChequeDate;
                preawb.ChequeNo = vm.ChequeNo;
                preawb.CourierCharge = vm.CourierCharge;
                preawb.Total = vm.Total;
                preawb.StatusEntry = vm.StatusEntry;
                preawb.BranchID = branchid;                
                preawb.OriginLocation = vm.OriginLocation;
                preawb.DestinationLocation = vm.DestinationLocation;
                preawb.OriginPlaceID = vm.OriginPlaceID;
                preawb.PickupSubLocality = vm.PickupSubLocality;
                preawb.DestinationPlaceID = vm.DestinationPlaceID;
                preawb.DeliverySubLocality = vm.DeliverySubLocality;
                preawb.Consignee = vm.Consignee;
                preawb.AWBNOFrom = vm.AWBNOFrom;
                preawb.AWBNOTo = vm.AWBNOTo;
                preawb.NoOfAWBs = vm.NoOfAWBs;
                preawb.Reference = vm.Reference;
                preawb.ModifiedDate = DateTime.Now;
                preawb.ModifiedUserID = UserId;
                preawb.Prepaid = vm.Prepaid;
                db.Entry(preawb).State = EntityState.Modified;
                db.SaveChanges();
                //posting
                //generate awb in AWBDetail
                AWBDAO.GenerateAWBPrepaid(preawb.PrepaidAWBID);
                AWBDAO.SavePrepaidAWBPosting(preawb.PrepaidAWBID, fyearid, branchid, companyid);
            }

            return RedirectToAction("Index", "AWBPrepaid");

        }

       
        [HttpGet]
        public JsonResult GetCustomerName(string term)
        {
           
            if (term.Trim() != "")
            {




                var customerlist = (from c1 in db.CustomerMasters
                                    where c1.StatusActive == true && c1.CustomerID > 0 && (c1.CustomerType == "CR") && c1.CustomerName.ToLower().StartsWith(term.ToLower())
                                    orderby c1.CustomerName ascending
                                    select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType, LocationName = c1.LocationName }).Take(25).ToList();

                return Json(customerlist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var customerlist = (from c1 in db.CustomerMasters
                                    where c1.CustomerID > 0 && (c1.CustomerType == "CR")
                                    orderby c1.CustomerName ascending
                                    select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType, LocationName = c1.LocationName }).Take(25).ToList();
                return Json(customerlist, JsonRequestBehavior.AllowGet);
            }
            



        }
        public ActionResult DeleteConfirmed(int id)
        {
            //int k = 0;
            if (id != 0)
            {
                DataTable dt = AWBDAO.DeleteAWBPrepaid(id);
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

        public JsonResult ReceiptPrint(int id)
        {
            string reportpath = "";
            //int k = 0;
            if (id != 0)
            {
                reportpath = AccountsReportsDAO.GeneratePrepaidReceipt(id);

            }

            return Json(new { path = reportpath, result = "ok" }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public ActionResult AWBPrintReport(int id = 0,string FromAWB="",string ToAWB="")
        {
            int uid = Convert.ToInt32(Session["UserID"].ToString());
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());


            string companyname = Session["CompanyName"].ToString();
            
            
            AccountsReportsDAO.GeneratePrepaidAWBPrintReport(id,FromAWB,ToAWB);

            ViewBag.Id = id;
            ViewBag.ReportName = "Customer AWB Issue Print";
            return View();

        }
        [HttpPost]
        public JsonResult ShowAWBPrintReport(int id = 0, string AWBNOFrom = "", string AWBNOTo = "")
        {
            //int uid = Convert.ToInt32(Session["UserID"].ToString());
            //int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            //int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            //int companyId = Convert.ToInt32(Session["CurrentCompanyID"].ToString());


            //string companyname = Session["CompanyName"].ToString();


            string reportpath=AccountsReportsDAO.GeneratePrepaidAWBPrintReport(id,AWBNOFrom,AWBNOTo);

            //ViewBag.Id = id;
            //ViewBag.ReportName = "Customer AWB Issue Print";
            //return PartialView("Accounts",InvoiceSearch",model);
            //return RedirectToAction("AWBPrintReport",new { id = id, FromAWB = AWBNOFrom, ToAWB = AWBNOTo });

            return Json(new { ReportPath = reportpath }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult GetAWB(string term,int Id)
        {

            if (term.Trim() != "")
            {


                var customerlist = (from c1 in db.AWBDetails
                                    where c1.PrepaidAWBID ==Id && c1.AWBNo.ToLower().StartsWith(term.ToLower())
                                    orderby c1.AWBNo ascending
                                    select new { ID = c1.ID,AWBNo = c1.AWBNo}).Take(25).ToList();

                return Json(customerlist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var customerlist = (from c1 in db.AWBDetails
                                    where c1.PrepaidAWBID ==Id
                                    orderby c1.AWBNo ascending
                                    select new { ID = c1.ID, AWBNo = c1.AWBNo }).Take(25).ToList();
                return Json(customerlist, JsonRequestBehavior.AllowGet);
            }




        }
    }
}