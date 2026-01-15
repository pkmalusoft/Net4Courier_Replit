using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using Newtonsoft.Json;
using System.IO;
using Net4Courier.Models;
using Net4Courier.DAL;
using System.Reflection;
using ExcelDataReader;

namespace Net4Courier.Controllers
{
    //
    [SessionExpire]
    public class BankReconciliationController : Controller
    {
        Entities1 db = new Entities1();

        public ActionResult Index()
        {
            var banklist = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
            ViewBag.BankList = banklist;
            ViewBag.ChequeStatus =(from c in db.BRMasters select new { BRCode = c.BRCode, comment = c.Comment + "(" + c.Type +")" }).OrderBy(cc => cc.BRCode).ToList();
            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int Fyearid = Convert.ToInt32(Session["fyearid"]);
            BankReconcSearch obj = (BankReconcSearch)Session["BankReconcSearch"];
            BankReconcSearch model = new BankReconcSearch();
            
            if (obj != null && !obj.FromDate.ToString().Contains("0001"))
            {
                List<BankDetails> translist = new List<BankDetails>();

                model.FromDate = obj.FromDate;
                model.ToDate = obj.ToDate;
                model.FromDate = AccountsDAO.CheckParamDate(model.FromDate, Fyearid).Date;
                model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, Fyearid).Date;
                model.BankHeadID = obj.BankHeadID;
                model.FilterStatus = obj.FilterStatus;
                model.ReconcDate = obj.ReconcDate;
                model.ReconcDate = CommonFunctions.GetBranchDateTime();
                model.ChangeStatus = obj.ChangeStatus;
                translist = AccountsDAO.GetBankDetails(Fyearid ,BranchID);
                model.Details = translist;
                AccountsReportParam param = new AccountsReportParam();
                param.FromDate = CommonFunctions.GetFirstDayofYear().Date;
                param.ToDate = model.ToDate;
                if (model.BankHeadID > 0)
                {
                    param.AcHeadId = model.BankHeadID;
                    decimal balanceamount = AccountsReportsDAO.GetLedgerBalance(param);
                    model.LedgerBalance = balanceamount;
                }
                model.CSVDetails = new List<BankDetails>();
                Session["BankDetails"] = translist;
            }
            else
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
                
                model.FromDate = AccountsDAO.CheckParamDate(model.FromDate, Fyearid).Date;
                model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, Fyearid).Date;
                model.ReconcDate = CommonFunctions.GetBranchDateTime(); ; // AccountsDAO.CheckParamDate(CommonFunctions.GetLastDayofMonth().Date,Fyearid);
                Session["BankReconcSearch"] = model;
                List<BankDetails> translist = new List<BankDetails>();
                model.Details = translist;
                model.CSVDetails = translist;

                Session["BankDetails"] = translist;

            }
            StatusModel result = AccountsDAO.CheckDateValidate(model.FromDate.ToString(), Fyearid);
            if (result.Status == "PeriodLock") //Period locked
            {
                ViewBag.Message = "Bank reconciliation is not allowed for locked period ! Please un-lock the period and attempt again.";
                ViewBag.SaveEnable = false;
            }
            else
            {

                result = AccountsDAO.CheckDateValidate(model.ToDate.ToString(), Fyearid);
                if (result.Status == "PeriodLock") //Period locked
                {
                    ViewBag.Message = "Bank reconciliation is not allowed for locked period ! Please un-lock the period and attempt again.";
                    ViewBag.SaveEnable = false;
                }
            }

            return View(model);
        }
    
        [HttpPost]
        public ActionResult Index(BankReconcSearch obj)
        {
            Session["BankReconcSearch"] = obj;
            return RedirectToAction("Index");

        }

        [HttpPost]
        public ActionResult ShowChequeList(int DetailId, bool statuschecked, string StatusTrans, bool all)
        {
            BankReconcSearch vm = new BankReconcSearch();
            try
            {
                if (DetailId > 0)
                {
                    vm.Details = new List<BankDetails>();
                    var Details1 = new List<BankDetails>();
                    var details = (List<BankDetails>)Session["BankDetails"];
                    if (details != null)
                    {
                        var finditem = details.Where(cc => cc.AcBankDetailID == DetailId).FirstOrDefault();
                        details.Remove(finditem);
                        if (statuschecked == true)
                        {
                            finditem.StatusReconciled = statuschecked;
                            finditem.ChangeStatus = StatusTrans;
                        }
                        else
                        {
                            finditem.StatusReconciled = false;
                            finditem.ChangeStatus = "";
                        }
                        details.Add(finditem);
                        Details1 = details.OrderByDescending(cc => cc.StatusReconciled == true).ToList();
                                                
                        vm.Details = Details1;
                        Session["BankDetails"] = Details1;
                    }
                }
                else if (all == true)
                {
                    vm.Details = new List<BankDetails>();
                    var details = (List<BankDetails>)Session["BankDetails"];
                    for (int i = 0; i < details.Count; i++)
                    {

                        details[i].StatusReconciled = statuschecked;
                    }
                    vm.AllSelected = statuschecked;
                    vm.Details = details;
                    Session["BankDetails"] = details;
                }
                else
                {
                    vm.Details = new List<BankDetails>();
                    var details = (List<BankDetails>)Session["BankDetails"];
                    vm.Details = details;
                    Session["BankDetails"] = details;
                }
                ViewBag.ChequeStatus = (from c in db.BRMasters select new { BRCode = c.BRCode, comment = c.Comment + "(" + c.Type + ")" }).OrderBy(cc => cc.BRCode).ToList();
                return PartialView("BankChequeList", vm);


            }
            catch (Exception ex)
            {
                throw ex;

            }


        }

        [HttpPost]
        public JsonResult SaveReconc(DateTime ReconcDate, string Details)
        {

            string message = "";
            string DocumentNo = "0";
            string status = "Failed";
            try
            {
                
                Details.Replace("{}", "");
                var IDetails = JsonConvert.DeserializeObject<List<BankDetails>>(Details);
                //DataTable ds1 = new DataTable();
                DataSet dt1 = new DataSet();
                dt1 = ToDataTable(IDetails);

                int FyearId = Convert.ToInt32(Session["fyearid"]);                
                string awbxml = dt1.GetXml();
                //xml = xml.Replace("T00:00:00+05:30", "");
                if (Session["UserID"] != null)
                {
                    int userid = Convert.ToInt32(Session["UserID"].ToString());
                    int CompanyID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                    int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());

                    string result = AccountsDAO.SaveBankReconc(userid, BranchId, FyearId,  awbxml, ReconcDate);
                    if (result == "0")
                    {
                        message = "No Cheque are reconcilied!";
                        DocumentNo = "0";
                    }
                    else
                    {
                        int docno = 0;
                        docno = Int32.Parse(result);
                        if (docno > 0)
                        {
                            message = "Cheque Reconciliation Updated Successfully, Item Count : " + docno.ToString();
                            status = "Ok";
                            DocumentNo = docno.ToString();
                        }
                        else
                        {
                            message = DocumentNo;
                        }
                    }

                }
                else
                {
                    message = "MC Vouchers are not generated!";
                    DocumentNo = "0";
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return Json(new { Status = status, DocumentNo = DocumentNo, Message = message }, JsonRequestBehavior.AllowGet);
        }

        private List<BankDetails> GetDataFromCSVFile(Stream stream)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            BankReconcSearch vm = new BankReconcSearch();
            vm.Details = new List<BankDetails>();
            var Details1 = new List<BankDetails>();
            var listdetails = (List<BankDetails>)Session["BankDetails"];

            List<BankDetails> details = new List<BankDetails>();
            try
            {
                using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
                {
                    var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true // To set First Row As Column Names    
                        }
                    });

                    if (dataSet.Tables.Count > 0)
                    {
                        //DataView dv = dataSet.Tables[0].DefaultView;
                        //dv.RowFilter = "AWBNo==''";
                        //DataSet ds1 = dv.DataViewManager.DataSet;

                        var dataTable = dataSet.Tables[0];
                        string xml = dataSet.GetXml();
                        if (dataSet != null && dataSet.Tables.Count > 0)
                        {
                            if (dataSet.Tables[0].Rows.Count > 0)
                            {
                                DataTable dt = dataSet.Tables[0];
                                for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                                {
                                    BankDetails obj = new BankDetails();
                                    obj.VoucherDate = Convert.ToDateTime(dt.Rows[i]["TransactionDate"].ToString());
                                    obj.ValueDate = Convert.ToDateTime(dt.Rows[i]["ValueDate"].ToString());
                                    obj.ReconcDate = Convert.ToDateTime(dt.Rows[i]["ValueDate"].ToString());
                                    obj.Debit= CommonFunctions.ParseDecimal(dt.Rows[i]["Debit"].ToString());
                                    obj.Credit = CommonFunctions.ParseDecimal(dt.Rows[i]["Credit"].ToString());
                                    obj.Remarks = dt.Rows[i]["Description"].ToString();
                                    
                                    details.Add(obj);
                                    
                                }
                            }
                        }

                       
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return details;
            

             
        }

        [HttpPost]
        public JsonResult ImportFile(HttpPostedFileBase importFile)
        {
          
            if (importFile == null) return Json(new { Status = 0, Message = "No File Selected" });

            try
            {
                List<BankDetails> fileData = GetDataFromCSVFile(importFile.InputStream);
                BankReconcSearch obj = (BankReconcSearch)Session["BankReconcSearch"];
                fileData = (from c in fileData where c.ValueDate >= obj.FromDate && c.ValueDate <= obj.ToDate select c).ToList();
                Session["BankCSVDetails"] = fileData;

                return Json(new { Status = 1, data = fileData, Message = "File Imported Successfully " });
            }
            catch (Exception ex)
            {
                return Json(new { Status = 0, Message = ex.Message });
            }
        }
        [HttpPost]
        public ActionResult ShowBankCSVList()
        {
            BankReconcSearch vm = new BankReconcSearch();
            try
            {
               
                    vm.CSVDetails = new List<BankDetails>();
                    
                    var details = (List<BankDetails>)Session["BankCSVDetails"];
                    if (details != null)
                    {


                    vm.CSVDetails = details;
                    }
                    else
                    {
                        vm.CSVDetails = new List<BankDetails>();
                    }
               
                return PartialView("BankCSVList", vm);


            }
            catch (Exception ex)
            {
                throw ex;

            }


        }
        public ActionResult ShowAutoReconc(string StatusTrans)
        {
            BankReconcSearch vm = new BankReconcSearch();
            var Details1 = new List<BankDetails>();
            try
            {

                vm.CSVDetails = new List<BankDetails>();

                var details = (List<BankDetails>)Session["BankCSVDetails"];
                var Bankdetails = (List<BankDetails>)Session["BankDetails"];
                int i = 0;
                foreach(var item in Bankdetails)
                {
                    if (item.ChequeNo != null)
                    {
                        if (item.ChequeNo != "")
                        {
                            var csvdetail = details.Where(cc => cc.Remarks.Contains(item.ChequeNo)).FirstOrDefault();
                            if (csvdetail != null)
                            {
                                Bankdetails[i].Remarks = csvdetail.Remarks;
                                Bankdetails[i].StatusReconciled = true;
                                Bankdetails[i].ChangeStatus = StatusTrans;
                            }
                        }
                    }
                    i++;
                }
                Details1 = Bankdetails.OrderByDescending(cc => cc.StatusReconciled == true).ToList();

                vm.Details = Details1;
                Session["BankDetails"] = Details1;

                ViewBag.ChequeStatus = (from c in db.BRMasters select new { BRCode = c.BRCode, comment = c.Comment + "(" + c.Type + ")" }).OrderBy(cc => cc.BRCode).ToList();
                return PartialView("BankChequeList", vm);


            }
            catch (Exception ex)
            {
                throw ex;

            }


        }
        public ActionResult Report()
        {

            SourceMastersModel obj = new SourceMastersModel();
            var isadd = obj.GetAddpermission(Convert.ToInt32(Session["UserRoleID"]), "/BankReconciliation/Report");
            if (isadd == false)
            {
                return RedirectToAction("AccessDenied", "Errors");
            }
            AccountsReportParam reportparam = (AccountsReportParam)Session["BankReconcReportParam"];
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());           
            
            ViewBag.AccountType = (from d in db.AcTypes where d.BranchId == branchid select d).ToList();            
            ViewBag.VoucherType = db.DocumentNoSetups.ToList();
            var banklist = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
            ViewBag.BankList = banklist;
            DateTime pFromDate;
            DateTime pToDate;

            if (reportparam == null)
            {
                pFromDate = CommonFunctions.GetFirstDayofMonth().Date; //.AddDays(-1);
                pToDate = CommonFunctions.GetLastDayofMonth().Date;
                reportparam = new AccountsReportParam();
                reportparam.FromDate = pFromDate;
                reportparam.ToDate = pToDate;
                reportparam.AcHeadId = 0;
                reportparam.AcHeadName = "";
                reportparam.Output = "PDF";
                reportparam.VoucherTypeId = "";
                reportparam.CurrentPeriod = false;
            }
            else
            {
                if (reportparam.ToDate.Date.ToString() == "01-01-0001 00:00:00" || reportparam.FromDate.Date.ToString() == "01-01-0001")
                {
                    pFromDate = CommonFunctions.GetFirstDayofMonth().Date; //.AddDays(-1);
                    reportparam.ToDate = pFromDate;
                    reportparam.Output = "PDF";
                }

            }
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("AccBankReconc"))
                {
                    Session["ReportOutput"] = null;
                }
            }

            ViewBag.ReportName = "Bank Reconciliation Report";
            return View(reportparam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Report(AccountsReportParam picker)
        {
            ViewBag.ReportName = "Bank Reconciliation Report";
            var achead = db.AcHeads.Find(picker.AcHeadId);
            picker.AcHeadName = achead.AcHead1;
            Session["BankReconcReportParam"]=picker;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            
            string reportfilename = AccountsReportsDAO.GenerateBankReconcReport(picker);

            AccountsReportParam reportparam = SessionDataModel.GetAccountsLedgerParam();
            if (picker.Output != "PDF")
            {

                return RedirectToAction("Download", "Accounts", new { file = reportparam.ReportFileName });
            }
            else
            {

                return RedirectToAction("Report", "BankReconciliation");

            }




        }

        public static DataSet ToDataTable<T>(List<T> items)
        {
            DataSet ds = new DataSet();
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            ds.Tables.Add(dataTable);
            //put a breakpoint here and check datatable
            return ds;
        }
        [HttpGet]
        // [CustomAuthorize]     
        public ActionResult Indexold()
        {
            Models.BankModal m = new Models.BankModal();
            //var banklist = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
            var banklist = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
            m.dtpFrom = DateTime.Now.Date.AddDays(-7);
            m.dtpTo = DateTime.Now.Date;
            //int userID = (HttpContext.User as CustomPrincipal).UserId;
            //string tablename = "R_Sales_" +((HttpContext.User as CustomPrincipal).ClientID);
            //DataTable dtproduct = SalesOnlineReport.GetProducts(tablename);
            //DataTable dtBranches = SalesOnlineReport.GetLocation(userID);
            //branch = "''";
            //foreach (DataRow dr in dtBranches.Rows)
            //{

            //    branch += ",'" + dr[0].ToString() + "'";
            //}
            ViewBag.BankList = banklist;
            //ViewBag.UserBranchID = (HttpContext.User as CustomPrincipal).UserBranchID;            
            ViewBag.chkSection = false;
            ViewBag.chkProduct = false;
            ViewBag.chkInvoice = false;
            ViewBag.Data = new DataTable();
            ViewBag.isLoadingFirst = true;
            return View(m);
        }

        //[CustomAuthorize]
        public ActionResult Indexold(Models.BankModal obj)
        {

            DataTable dt = new DataTable();
            var attachedFile = System.Web.HttpContext.Current.Request.Files[0];

            if (attachedFile == null || attachedFile.ContentLength <= 0 || !attachedFile.FileName.EndsWith(".csv")) return Json(null);
            var csvReader = new StreamReader(attachedFile.InputStream);
            var uploadModelList = new List<CsvRecordsViewModel>();

            var csvData = csvReader.ReadToEnd().Split('\n')
                                            .Skip(1)                  // skip header 
                                            .Select(s => s.Trim())   // delete whitespace
                                            .Select(l => l.Split(',')) // get arrays of values
                                            .Where(l => l.Length > 0)  //nonempty strings
                                            .Select(l => l);


            csvData = csvData.Take((csvData.Count() - 1));//skiping last row
            List<CsvRecordsViewModel> LstRes = new List<CsvRecordsViewModel>();

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("BankName", typeof(String));
            dataTable.Columns.Add("ChequeAmount", typeof(decimal));
            dataTable.Columns.Add("ChequeNo", typeof(String));
            dataTable.Columns.Add("TransactionID", typeof(String));

            foreach (var row in csvData.Select(l => new { BankName = l[0], ChequeAmount = Convert.ToDecimal(l[1]), ChequeNo = l[2], TransactionID = l[3] }))
            {
                var uploadModelRecord = new CsvRecordsViewModel();
                uploadModelRecord.BankName = row.BankName;
                uploadModelRecord.ChequeAmount = row.ChequeAmount;
                uploadModelRecord.ChequeNo = row.ChequeNo;
                uploadModelRecord.TransactionID = row.TransactionID;

                uploadModelList.Add(uploadModelRecord);// newModel needs to be an object of type ContextTables.
                dataTable.Rows.Add(new Object[] { row.BankName, row.ChequeAmount, row.ChequeNo, row.TransactionID });
                LstRes.Add(uploadModelRecord);
            }

            ViewBag.BankList = objBank;
            ViewBag.StatusList = objStatus;
            ViewBag.chkSection = false;
            ViewBag.chkProduct = false;
            ViewBag.chkInvoice = false;
            ViewBag.isLoadingFirst = true;

            BankModal rest = new BankModal();
            rest.ResultViewModel = DBOperations.ConvertToList<CsvRecordsViewModel>(DBOperations.BankRecon(dataTable, true, Convert.ToDateTime(obj.dtpFrom), Convert.ToDateTime(obj.dtpTo), obj.cboProduct));
            rest.dtpFrom = obj.dtpFrom;
            rest.dtpTo = obj.dtpTo;
            rest.cboProduct = obj.cboProduct;
            ModelState.Clear();
            return View(rest);
        }

        //CSV File Download
        public FilePathResult DownloadSampleStatement()
        {
            String fileName = "Sample_Statement.csv";
            String filePath = Path.Combine(Server.MapPath("~/Uploads"), fileName);
            return File(filePath, "multipart/form-data", fileName);
        }

        [HttpPost]
        public ActionResult Save(Models.BankModal model)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("BankName", typeof(String));
            dataTable.Columns.Add("ChequeNo", typeof(String));
            dataTable.Columns.Add("ChequeAmount", typeof(Decimal));
            dataTable.Columns.Add("ChequeStatus", typeof(String));
            dataTable.Columns.Add("IsCleared", typeof(Boolean));
            dataTable.Columns.Add("Remarks", typeof(String));
            dataTable.Columns.Add("TransactionID", typeof(String));
            dataTable.Columns.Add("Status", typeof(int));

            var validItems = model.ResultViewModel.Where(r => r.Status >= 0 && r.IsCleared==true).Select(r => r);

            foreach (var row in validItems)
            {
                dataTable.Rows.Add(new Object[] { row.BName, row.ChqNo, row.ChqAmount, row.ChqStatus, row.IsCleared, row.Remarks, row.TransactionID, row.Status });
            }
            DBOperations.SaveReconciliation(dataTable, true);

            return RedirectToAction("Index", "BankReconciliation");
        }

        public static List<SelectListItem> objBank = new List<SelectListItem>()
        {
                new SelectListItem { Text = "SBI", Value = "SBI" },
                new SelectListItem { Text = "Bank of Baroda", Value = "Bank of Baroda" },
        };

        public static List<SelectListItem> objStatus = new List<SelectListItem>()
        {
                new SelectListItem { Text = "Cleared", Value = "1" },
                new SelectListItem { Text = "Bounced", Value = "2" },
        };
    }
}