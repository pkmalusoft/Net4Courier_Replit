using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using System.Data;
using Net4Courier.DAL;
using System.Data.Entity;
using Newtonsoft.Json;
using System.Reflection;
using ClosedXML.Excel;
using System.IO;

namespace Net4Courier.Controllers
{
    [SessionExpire]
    public class MCPaymentVoucherController : Controller
    {
        Entities1 db = new Entities1();
        // GET: MCPaymentPrint
        public ActionResult Index()
        {

            MCPaymentPrintSearch obj = (MCPaymentPrintSearch)Session["MCPaymentPrintSearch"];
            MCPaymentPrintSearch model = new MCPaymentPrintSearch();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            if (obj == null)
            {
                DateTime pFromDate;
                DateTime pToDate;
                //int pStatusId = 0;
                pFromDate = CommonFunctions.GetLastDayofMonth().Date;
                pToDate = CommonFunctions.GetLastDayofMonth().Date;
                obj = new MCPaymentPrintSearch();
                obj.FromDate = pFromDate;
                obj.ToDate = pToDate;
                //obj.DocumentNo = "";
                Session["MCPaymentPrintSearch"] = obj;
                model.FromDate = pFromDate;
                model.ToDate = pToDate;                
            }
            else
            {
                model = obj;
            }
            
                List<MCPaymentAWB> lst = PickupRequestDAO.GetMCPaymentVoucherList();
                model.Details = lst;
                
           
            

            return View(model);


        }
        [HttpPost]
        public ActionResult Index(MCPaymentPrintSearch obj)
        {
            Session["MCPaymentPrintSearch"] = obj;
            if (obj.Download == true)
            {
                DataTable dt = PickupRequestDAO.GetMCPaymentVoucherExcelReport();


                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(dt);
                    wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wb.Style.Font.Bold = true;
                    string FileName = "MCPVRegister_" + DateTime.Now.ToString("MMddyyyyHHMM") + ".xlsx";
                    Response.Clear();
                    Response.Buffer = true;
                    Response.Charset = "";
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment;filename=" + FileName + ".xlsx");

                    using (MemoryStream MyMemoryStream = new MemoryStream())
                    {
                        wb.SaveAs(MyMemoryStream);
                        MyMemoryStream.WriteTo(Response.OutputStream);
                        Response.Flush();
                        Response.End();
                    }
                }
            }

            return RedirectToAction("Index");
        }
        

        // GET: MCPaymentPrint
        public ActionResult MCClosing()
        {

            MCPaymentPrintSearch obj = (MCPaymentPrintSearch)Session["MCPaymentPrintSearch"];
            MCPaymentPrintSearch model = new MCPaymentPrintSearch();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            if (obj == null)
            {
                DateTime pFromDate;
                DateTime pToDate;
                //int pStatusId = 0;
                pFromDate = CommonFunctions.GetLastDayofMonth().Date;
                pToDate = CommonFunctions.GetLastDayofMonth().Date;
                obj = new MCPaymentPrintSearch();
                obj.FromDate = pFromDate;
                obj.ToDate = pToDate;
                //obj.DocumentNo = "";
                Session["MCPaymentPrintSearch"] = obj;
                model.FromDate = pFromDate;
                model.ToDate = pToDate;
            }
            else
            {
                model = obj;
            }

            List<MCPaymentAWB> lst = PickupRequestDAO.GetMCPaymentVoucherClosingList();
            model.Details = lst;




            return View(model);


        }
        [HttpPost]
        public ActionResult MCClosing(MCPaymentPrintSearch obj)
        {
            Session["MCPaymentPrintSearch"] = obj;
            return RedirectToAction("MCClosing");
        }
        public ActionResult Create()
        {

            MCPaymentPrint obj = (MCPaymentPrint)Session["MCPaymentPrint"];
            MCPaymentPrint model = new MCPaymentPrint();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            if (obj == null)
            {
                DateTime pFromDate;
                DateTime pToDate;
                //int pStatusId = 0;
                pFromDate = CommonFunctions.GetFirstDayofMonth().Date;
                pToDate = CommonFunctions.GetLastDayofMonth().Date;
                obj = new MCPaymentPrint();
               // obj.FromDate = pFromDate;
               // obj.ToDate = pToDate;
                //obj.DocumentNo = "";
                Session["MCPaymentPrint"] = obj;
                //model.FromDate = pFromDate;
                //model.ToDate = pToDate;
            }
            else
            {
                model = obj;
            }
            
                MCPaymentPrint mcpayment = PickupRequestDAO.GetMCPaymentPrintVoucherPending();
                model.DocumentNo = 0;
                model.Details = new List<MCPaymentAWB>();

            model.ConsignorDetails = mcpayment.ConsignorDetails;
            Session["MCPaymentConsignor"] = model.ConsignorDetails;
            Session["MCPaymentDetailsSource"] = mcpayment.Details;
            Session["MCPaymentDetails"] = new List<MCPaymentAWB>();
            return View(model);


        }
        [HttpPost]
        public ActionResult Create(MCPaymentPrint obj)
        {
            Session["MCPaymentPrint"] = obj;
            return RedirectToAction("Print");
        }

        public ActionResult Edit(int id = 0)
        {

           
            MCPaymentVoucherVM model = new MCPaymentVoucherVM();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            
            if (id > 0)
            {
                model = PickupRequestDAO.GetMCPaymentPrintVoucherPrintList(id);
                                
                
            }
            

            return View(model);


        }
        //[HttpPost]
        //public ActionResult Edit(MCPaymentPrint obj)
        //{
        //    Session["MCPaymentPrint"] = obj;
        //    return RedirectToAction("Print");
        //}

        //[HttpPost]
        //public ActionResult ShowItemList(int CustomerId, string FromDate, string ToDate, string MAWB)
        //{
        //    AgentInvoiceVM vm = new AgentInvoiceVM();
        //    vm.Details = PickupRequestDAO.GetAgentShipmentList(CustomerId, Convert.ToDateTime(FromDate), Convert.ToDateTime(ToDate), MAWB);
        //    return PartialView("InvoiceList", vm);

        //}

        //[HttpPost]
        //public ActionResult ConsignorAWBList(int CustomerId, string FromDate, string ToDate, string MAWB)
        //{
        //    AgentInvoiceVM vm = new AgentInvoiceVM();
        //    vm.Details = PickupRequestDAO.GetAgentShipmentList(CustomerId, Convert.ToDateTime(FromDate), Convert.ToDateTime(ToDate), MAWB);
        //    return PartialView("InvoiceList", vm);

        //}

        public ActionResult DeleteConfirmed(int id)
        {
            //int k = 0;
            if (id != 0)
            {
                DataTable dt = PickupRequestDAO.DeleteMCPaymentVoucher(id);
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
        public ActionResult DeleteAWBConfirmed(int DetailId,int VoucherID)
        {
            //int k = 0;
            if (DetailId != 0)
            {
                DataTable dt = PickupRequestDAO.DeleteMCPaymentPrintAWB(DetailId,VoucherID);
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

            return RedirectToAction("Edit",new { id = VoucherID });


        }


        [HttpPost]
        public JsonResult SaveMCPaymentVoucher(string PrintDate, string ConsignorDetails, string Details)
        {

            string message = "";
            string DocumentNo = "0";
            string status = "Failed";
            try
            {
                ConsignorDetails.Replace("{}", "");
                var IConsignorDetails = JsonConvert.DeserializeObject<List<MCPaymentConsignor>>(ConsignorDetails);
                //DataTable ds = new DataTable();
                DataSet dt = new DataSet();
                dt = ToDataTable(IConsignorDetails);

                Details.Replace("{}", "");
                var IDetails = JsonConvert.DeserializeObject<List<MCPaymentInScan>>(Details);
                //DataTable ds1 = new DataTable();
                DataSet dt1 = new DataSet();
                dt1 = ToDataTable(IDetails);


                int FyearId = Convert.ToInt32(Session["fyearid"]);
                string consignorxml = dt.GetXml();
                string awbxml = dt1.GetXml();
                //xml = xml.Replace("T00:00:00+05:30", "");
                if (Session["UserID"] != null)
                {
                    int userid = Convert.ToInt32(Session["UserID"].ToString());
                    int CompanyID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                    int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());

                    string result = PickupRequestDAO.SaveMCPaymentVoucher(userid, BranchId, FyearId, consignorxml, awbxml, PrintDate);
                    if (result == "0")
                    {
                        message = "MC Vouchers are not generated!";
                        DocumentNo = "0";
                    }
                    else if (result.Contains("Error:"))
                    {
                        return Json(new { Status = status, DocumentNo = "", Message = result}, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        int docno = 0;
                        docno = Int32.Parse(result);
                        if (docno >0)
                        {
                            message = "MC Voucher are Generated,Opening Print Page!";
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

            return Json(new { Status=status, DocumentNo = DocumentNo, Message = message }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult SaveMCClosing(string Details)
        {

            string message = "";
            string DocumentNo = "0";
            string status = "Failed";
            try
            {
               
                Details.Replace("{}", "");
                var IDetails = JsonConvert.DeserializeObject<List<MCPaymentAWB>>(Details);
              

               
              
                if (Session["UserID"] != null)
                {
                    int userid = Convert.ToInt32(Session["UserID"].ToString());
                    int CompanyID = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                    int BranchId = Convert.ToInt32(Session["CurrentBranchID"].ToString());
                    string result = "";
                    foreach(var item in IDetails)
                    {
                        var mcvoucher = db.MCPaymentVouchers.Find(item.MCPaymentVoucherID);
                        mcvoucher.Closed = true;
                        mcvoucher.ClosedBy = userid;
                        mcvoucher.ClosedDate = CommonFunctions.GetCurrentDateTime();
                        db.Entry(mcvoucher).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    
                    
                        message = IDetails.Count.ToString() + " MC Payment Closed successfully!";
                    status = "OK"; 

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


        public ActionResult VoucherPrint(int id =0, int DocumentNo =0,int MSheet=0)
        {
            ViewBag.ReportName = "Voucher Printing";                        
            AccountsReportsDAO.MCPaymentVoucherReport(id, DocumentNo,MSheet);
            
            return View();
        }


        public ActionResult VoucherMultiplePrint()
        {
            ViewBag.ReportName = "Voucher Printing";
            var obj = (MCPaymentMultiplePrint)Session["VoucherMultiplePrint"];
            AccountsReportsDAO.MCPaymentVoucherMultiplePrint(obj);
            return View();
        }

        [HttpPost]
        public string GetVoucherMultiplePrint(MCPaymentMultiplePrint obj)
        {
            ViewBag.ReportName = "Voucher Printing";
            Session["VoucherMultiplePrint"] = obj;

            return "ok";
        }
        [HttpPost]
        public ActionResult ShowConsignor(string Consignor, bool statuschecked,bool all)
        {
            MCPaymentPrint vm = new MCPaymentPrint();
            try
            {
                vm.ConsignorDetails = new List<MCPaymentConsignor>();
                var Details1 = new List<MCPaymentConsignor>();
                var details = (List<MCPaymentConsignor>)Session["MCPaymentConsignor"];
                if (Consignor != "")
                {
                    var finditem = details.Where(cc => cc.Consignor == Consignor).FirstOrDefault();
                    details.Remove(finditem);
                    finditem.ConsignorChecked = statuschecked;
                    details.Add(finditem);
                    Details1 = details.OrderByDescending(cc => cc.ConsignorChecked == true).ToList();
                    vm.ConsignorDetails = Details1;
                    Session["MCPaymentConsignor"] = Details1;
                    var awbdetailssource = (List<MCPaymentAWB>)Session["MCPaymentDetailsSource"];
                    var items = awbdetailssource.Where(cc => cc.Consignor == Consignor).ToList();
                    
                    var awbdetails = (List<MCPaymentAWB>)Session["MCPaymentDetails"];
                    if (statuschecked == true)
                    {
                        var awbdetailremove = awbdetails.Where(cc => cc.Consignor == Consignor).ToList();
                        if (awbdetailremove.Count==0)
                            awbdetails.AddRange(items);
                    }
                    else
                    {
                        var awbdetailremove = awbdetails.Where(cc => cc.Consignor == Consignor).ToList();
                        if (awbdetailremove.Count >0)
                        {
                            foreach (var item in awbdetailremove)
                            {
                                awbdetails.Remove(item);
                            }
                        }

                    }

                    Session["MCPaymentDetails"] = awbdetails;

                    //if (details != null)
                    //{
                    //    for (int i = 0; i < details.Count; i++)
                    //    {
                    //        if (details[i].Consignor == Consignor)
                    //            details[i].ConsignorChecked = statuschecked;

                    //        if (details[i].ConsignorChecked == true)
                    //            Details1.Add(details[i]);
                    //    }

                    //    for (int i = 0; i < details.Count; i++)
                    //    {


                    //        if (details[i].ConsignorChecked == false)
                    //            Details1.Add(details[i]);
                    //    }

                    //    vm.ConsignorDetails = Details1;
                    //    Session["MCPaymentConsignor"] = Details1;
                    //}

                }
                else if (all==true)
                {
                    var details1 = (List<MCPaymentConsignor>)Session["MCPaymentConsignor"];
                    
                    for (int i = 0; i < details1.Count; i++)
                    {
                        details[i].ConsignorChecked = statuschecked;                        
                    }
                    vm.ConsignorDetails = details1;
                    vm.ConsignorAllSelected = statuschecked;
                    Session["MCPaymentConsignor"] = details1;
                    if (statuschecked == true)
                    {
                        var awbdetailssource = (List<MCPaymentAWB>)Session["MCPaymentDetailsSource"];
                        
                        Session["MCPaymentDetails"] = awbdetailssource;
                    }
                    else
                    {
                        Session["MCPaymentDetails"] = new List<MCPaymentAWB>();
                    }
                }
                else
                {
                    var awbdetailssource = (List<MCPaymentAWB>)Session["MCPaymentDetailsSource"];                    
                    Session["MCPaymentDetails"] = awbdetailssource;
                }              
                
                return PartialView("ConsignorList", vm);


            }
            catch (Exception ex)
            {
                throw ex;

            }


        }
        [HttpPost]
        public ActionResult ShowAWB(string AWBNo, bool statuschecked,bool all)
        {
            MCPaymentPrint vm = new MCPaymentPrint();
            try
            {
                if (AWBNo != "")
                {
                    vm.Details = new List<MCPaymentAWB>();
                    var Details1 = new List<MCPaymentAWB>();
                    var details = (List<MCPaymentAWB>)Session["MCPaymentDetails"];
                    if (details != null)
                    {
                        var finditem=details.Where(cc => cc.AWBNo == AWBNo).FirstOrDefault();
                        details.Remove(finditem);
                        finditem.AWBChecked = statuschecked;
                        details.Add(finditem);
                        Details1= details.OrderByDescending(cc => cc.AWBChecked == true).ToList();
                        
                        //for (int i = 0; i < details.Count; i++)
                        //{
                        //    if (details[i].AWBNo == AWBNo)
                        //        details[i].AWBChecked = statuschecked;

                        //    if (details[i].AWBChecked == true)
                        //        Details1.Add(details[i]);
                        //}

                        //for (int i = 0; i < details.Count; i++)
                        //{
                        //    if (details[i].AWBChecked == false)
                        //        Details1.Add(details[i]);
                        //}

                        vm.Details = Details1;
                        Session["MCPaymentDetails"] = Details1;
                    }
                }
                else if (all==true)
                {
                    vm.Details = new List<MCPaymentAWB>();
                    var details = (List<MCPaymentAWB>)Session["MCPaymentDetails"];
                    for (int i = 0; i < details.Count; i++)
                    {

                        details[i].AWBChecked = statuschecked;
                    }
                    vm.AWBAllSelected = statuschecked;
                    vm.Details = details;
                    Session["MCPaymentDetails"] = details;
                }
                else
                {
                    vm.Details = new List<MCPaymentAWB>();
                    var details = (List<MCPaymentAWB>)Session["MCPaymentDetails"];
                    vm.Details = details;
                    Session["MCPaymentDetails"] = details;
                }

                return PartialView("AWBList", vm);


            }
            catch (Exception ex)
            {
                throw ex;

            }


        }
        //public ActionResult VoucherMultiplePrint(MCPaymentMultiplePrint obj)
        //{
        //    ViewBag.ReportName = "Voucher Printing";
        //    AccountsReportsDAO.MCPaymentVoucherMultiplePrint(obj);

        //    return View();
        //}

    }
}