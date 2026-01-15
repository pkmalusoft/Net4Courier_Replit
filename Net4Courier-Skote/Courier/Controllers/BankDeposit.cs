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

namespace Net4Courier.Controllers
{
    //
    [SessionExpire]
    public class BankDepositController : Controller
    {
        Entities1 db = new Entities1();

        public ActionResult Index()
        {
            var banklist = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
            ViewBag.BankList = banklist;
            ViewBag.ChequeStatus = db.ChequeStatus.OrderBy(cc=>cc.ID).ToList();
            int BranchID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int Fyearid = Convert.ToInt32(Session["fyearid"]);
            BankReconcSearch obj = (BankReconcSearch)Session["ChequeDepositSearch"];
            BankReconcSearch model = new BankReconcSearch();
            
            if (obj != null)
            {
                List<BankDetails> translist = new List<BankDetails>();

                model.FromDate = obj.FromDate;
                model.ToDate = obj.ToDate;
                //StatusModel statu = AccountsDAO.CheckDateValidate(obj.FromDate.ToString(), Fyearid);
                //string vdate = statu.ValidDate;
                //model.FromDate =Convert.ToDateTime( vdate);
                //model.ToDate =Convert.ToDateTime( AccountsDAO.CheckDateValidate(obj.ToDate.ToString(), Fyearid).ValidDate);
                model.BankHeadID = obj.BankHeadID;
                model.FilterStatus = obj.FilterStatus;
                model.ReconcDate = obj.ReconcDate;
                model.ChangeStatus = obj.ChangeStatus;
                
                if (obj.DepositHeadID != 0)
                    model.DepositHeadID = obj.DepositHeadID;

                if (obj.ReconcDate != null)
                    model.ReconcDate = obj.ReconcDate;
                translist = AccountsDAO.GetChequeDetails(Fyearid ,BranchID);
                model.Details = translist;
                Session["ChequeDetails"] = translist;
            }
            else
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
                model.ReconcDate = CommonFunctions.GetLastDayofMonth().Date;
                model.FilterStatus = "Pending";
                Session["ChequeDepositSearch"] = model;
                List<BankDetails> translist = new List<BankDetails>();
                model.Details = translist;
                Session["ChequeDetails"] = translist;

            }
            return View(model);
        }
    
        [HttpPost]
        public ActionResult Index(BankReconcSearch obj)
        {
            Session["ChequeDepositSearch"] = obj;
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
                    var details = (List<BankDetails>)Session["ChequeDetails"];
                    if (details != null)
                    {
                        var finditem = details.Where(cc => cc.AcBankDetailID == DetailId).FirstOrDefault();
                        details.Remove(finditem);
                        finditem.StatusReconciled = statuschecked;
                        finditem.ChangeStatus = StatusTrans;
                        details.Add(finditem);
                        Details1 = details.OrderByDescending(cc => cc.StatusReconciled == true).ToList();
                                                
                        vm.Details = Details1;
                        Session["ChequeDetails"] = Details1;
                    }
                }
                else if (all == true)
                {
                    vm.Details = new List<BankDetails>();
                    var details = (List<BankDetails>)Session["ChequeDetails"];
                    for (int i = 0; i < details.Count; i++)
                    {

                        details[i].StatusReconciled = statuschecked;
                    }
                    vm.AllSelected = statuschecked;
                    vm.Details = details;
                    Session["ChequeDetails"] = details;
                }
                else
                {
                    vm.Details = new List<BankDetails>();
                    var details = (List<BankDetails>)Session["ChequeDetails"];
                    vm.Details = details;
                    Session["ChequeDetails"] = details;
                }
                ViewBag.ChequeStatus = db.ChequeStatus.OrderBy(cc => cc.ID).ToList();
                return PartialView("BankChequeList", vm);


            }
            catch (Exception ex)
            {
                throw ex;

            }


        }

        [HttpPost]
        public JsonResult SaveBankDeposit(DateTime ReconcDate, int BankHeadID, int DepositBankHeadId, string Details)
        {
            BankReconcSearch obj = (BankReconcSearch)Session["ChequeDepositSearch"];
            obj.DepositHeadID = DepositBankHeadId;
            obj.ReconcDate = ReconcDate;
            Session["ChequeDepositSearch"] = obj;
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

                    string result = AccountsDAO.SaveBankDeposit(userid, BranchId, FyearId,  awbxml, ReconcDate,BankHeadID,DepositBankHeadId);
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