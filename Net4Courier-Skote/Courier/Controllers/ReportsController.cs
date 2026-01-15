using Net4Courier.DAL;
using Net4Courier.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClosedXML;
using ClosedXML.Excel;
using System.IO;
using System.Data;

namespace Net4Courier.Controllers
{
    [SessionExpire]
    public class ReportsController :Controller
    {
        Entities1 db = new Entities1();
        #region "Empost"
        public ActionResult EmposFeeReport()
        {
            ViewBag.ReportName = "Empost Fee Analysis Report";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("EmpostFee_"))
                {
                    Session["ReportOutput"] = null;
                }
            }
            return View();
        }
        public ActionResult ReportFrame()
        {
            if (Session["ReportOutput"] != null)
                ViewBag.ReportOutput = Session["ReportOutput"].ToString();
            else
            {
                string reportpath = AccountsReportsDAO.GenerateDefaultReport();
                ViewBag.ReportOutput = reportpath; // "~/Reports/DefaultReport.pdf";
            }
            return PartialView();
        }
        public ActionResult PrintSearch()
        {
            AccountsReportParam reportparam = SessionDataModel.GetAccountsParam();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            DateTime pFromDate;
            DateTime pToDate;

            if (reportparam == null)
            {
                pFromDate = CommonFunctions.GetFirstDayofMonth().Date; //.AddDays(-1);
                pToDate = CommonFunctions.GetLastDayofMonth().Date;
                //pFromDate = AccountsDAO.CheckParamDate(pFromDate, yearid).Date;
                //pToDate = AccountsDAO.CheckParamDate(pToDate, yearid).Date;
                reportparam = new AccountsReportParam();
                reportparam.FromDate = pFromDate;
                reportparam.ToDate = pToDate;
                reportparam.AcHeadId = 0;
                reportparam.AcHeadName = "";
                reportparam.Output = "PDF";
            }
            else
            {
                if (reportparam.FromDate.Date.ToString() == "01-01-0001 00:00:00" || reportparam.FromDate.Date.ToString() == "01-01-0001")
                {
                    pFromDate = CommonFunctions.GetFirstDayofMonth().Date; //.AddDays(-1);
                    pToDate = CommonFunctions.GetLastDayofMonth().Date;
                    
                    
                    reportparam.FromDate = AccountsDAO.CheckParamDate(pFromDate, yearid);
                    reportparam.ToDate = AccountsDAO.CheckParamDate(pToDate, yearid);
                    reportparam.Output = "PDF";
                }
                //else
                //{
                //    reportparam.FromDate = AccountsDAO.CheckParamDate(reportparam.FromDate, yearid);
                //    reportparam.ToDate = AccountsDAO.CheckParamDate(reportparam.ToDate, yearid);



                //}

            }
                        
            SessionDataModel.SetAccountsParam(reportparam);

            return View(reportparam);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PrintSearch([Bind(Include = "FromDate,ToDate,Output")] AccountsReportParam picker)
        {
            AccountsReportParam model = new AccountsReportParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate,
                Output = picker.Output
            };


            ViewBag.Token = model;
            SessionDataModel.SetAccountsParam(model);
            
            AccountsReportsDAO.GenerateEmposFeeReport();
            return RedirectToAction("EmposFeeReport", "Reports");


        }

        #endregion

        #region "AWBRegister"
        public ActionResult AWBRegister()
        {
            ViewBag.ReportName = "AirWay Bill Register";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("AWBRegister_"))
                {
                    Session["ReportOutput"] = null;
                }
            }
           
            return View();
        }


        public ActionResult AWBReportParam()
        {
            AWBReportParam reportparam = SessionDataModel.GetAWBReportParam();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            DateTime pFromDate;
            DateTime pToDate;

            if (reportparam == null)
            {
                pFromDate = CommonFunctions.GetFirstDayofMonth().Date; //.AddDays(-1);
                pToDate = CommonFunctions.GetLastDayofMonth().Date;
                pFromDate = AccountsDAO.CheckParamDate(pFromDate, yearid).Date;
                pToDate = AccountsDAO.CheckParamDate(pToDate, yearid).Date;
                reportparam = new AWBReportParam();
                reportparam.FromDate = pFromDate;
                reportparam.ToDate = pToDate;
                reportparam.ParcelTypeId = 0;
                reportparam.PaymentModeId= 0;
                reportparam.MovementId = "1,2,3,4";
                reportparam.Output = "PDF";
                reportparam.SortBy = "Date Wise";
                reportparam.ReportType = "Date";
            }
            else
            {
                if (reportparam.FromDate.Date.ToString() == "01-01-0001 00:00:00" || reportparam.FromDate.Date.ToString() == "01-01-0001")
                {
                    pFromDate = CommonFunctions.GetFirstDayofMonth().Date; //.AddDays(-1);
                    pToDate = CommonFunctions.GetLastDayofMonth().Date;
                    
                    reportparam.FromDate = AccountsDAO.CheckParamDate(pFromDate, yearid);
                    reportparam.ToDate = AccountsDAO.CheckParamDate(pToDate, yearid);
                    reportparam.Output = "PDF";
                }
                else
                {
                    reportparam.FromDate = AccountsDAO.CheckParamDate(reportparam.FromDate, yearid);
                    reportparam.ToDate = AccountsDAO.CheckParamDate(reportparam.ToDate, yearid);

                }

            }

            SessionDataModel.SetAWBReportParam(reportparam);
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            ViewBag.parceltype = db.ParcelTypes.ToList();
            ViewBag.Movement = db.CourierMovements.ToList();
            return View(reportparam);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AWBReportParam([Bind(Include = "FromDate,ToDate,PaymentModeId,SelectedValues,MovementId,ParcelTypeId,Output,ReportType,SortBy")] AWBReportParam picker)
        {
            AWBReportParam model = new AWBReportParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate,
                PaymentModeId=picker.PaymentModeId,
                ParcelTypeId=picker.ParcelTypeId,
                MovementId =picker.MovementId,
                Output = picker.Output,
                ReportType=picker.ReportType,
                SortBy =picker.SortBy
                
            };
            model.MovementId = "";
            if (picker.SelectedValues != null)
            {
                foreach (var item in picker.SelectedValues)
                {
                    if (model.MovementId == "")
                    {
                        model.MovementId = item.ToString();
                    }
                    else
                    {
                        model.MovementId = model.MovementId + "," + item.ToString();
                    }

                }
            }

            ViewBag.Token = model;
            SessionDataModel.SetAWBReportParam(model);
            if (model.ReportType == "Summary")
                AccountsReportsDAO.GenerateAWBRegisterSummary();
            else
                AccountsReportsDAO.GenerateAWBRegister();
            return RedirectToAction("AWBRegister", "Reports");


        }


        #endregion

        #region "TaxRegister"
        public ActionResult TaxRegister()
        {
            TaxReportParam reportparam = SessionDataModel.GetTaxReportParam();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            DateTime pFromDate;
            DateTime pToDate;
            if (reportparam == null || reportparam.FromDate.Date.ToString().Contains("0001"))
            {
                pFromDate = CommonFunctions.GetFirstDayofMonth().Date; //.AddDays(-1);
                pToDate = CommonFunctions.GetLastDayofMonth().Date;
                pFromDate = AccountsDAO.CheckParamDate(pFromDate, yearid).Date;
                pToDate = AccountsDAO.CheckParamDate(pToDate, yearid).Date;
                reportparam = new TaxReportParam();
                reportparam.FromDate = pFromDate;
                reportparam.ToDate = pToDate;
                reportparam.Output = "PDF";
                reportparam.SortBy = "NonZeroTax";
                reportparam.ReportType = "Date";
                reportparam.TransactionType = "Receipts & Payments";
            }
            else
            {
                if (reportparam.FromDate.Date.ToString() == "01-01-0001 00:00:00" || reportparam.FromDate.Date.ToString() == "01-01-0001")
                {
                    pFromDate = CommonFunctions.GetFirstDayofMonth().Date; //.AddDays(-1);
                    pToDate = CommonFunctions.GetLastDayofMonth().Date;
                    pFromDate = AccountsDAO.CheckParamDate(pFromDate, yearid).Date;
                    pToDate = AccountsDAO.CheckParamDate(pToDate, yearid).Date;
                    reportparam.FromDate = pFromDate;
                    reportparam.ToDate = pToDate;
                    reportparam.Output = "PDF";
                }
                else
                {
                    reportparam.FromDate = AccountsDAO.CheckParamDate(reportparam.FromDate, yearid);
                    reportparam.ToDate = AccountsDAO.CheckParamDate(reportparam.ToDate, yearid);
                }

            }

            SessionDataModel.SetTaxReportParam(reportparam);
            List<VoucherTypeVM> lsttype = new List<VoucherTypeVM>();
            lsttype.Add(new VoucherTypeVM { TypeName = "Receipts & Payments" });
            lsttype.Add(new VoucherTypeVM { TypeName = "Receipts" });
            lsttype.Add(new VoucherTypeVM { TypeName = "Payments" });
            
            ViewBag.VoucherTypes = lsttype;
            AccountsReportsDAO.GenerateTaxRegister();

            ViewBag.ReportName = "Tax Register";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("TaxRegister_"))
                {
                    Session["ReportOutput"] = null;
                }
            }

            return View(reportparam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TaxRegister([Bind(Include = "FromDate,ToDate,TransactionType,Output,ReportType,SortBy")] TaxReportParam picker)
        {
            TaxReportParam model = new TaxReportParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate,
                TransactionType = picker.TransactionType,
                Output = picker.Output,
                ReportType = picker.ReportType,
                SortBy = picker.SortBy

            };

            ViewBag.Token = model;
            SessionDataModel.SetTaxReportParam(model);

            AccountsReportsDAO.GenerateTaxRegister();
            return RedirectToAction("TaxRegister", "Reports");


        }

        public ActionResult VATRegister()
        {
            TaxReportParam reportparam =(TaxReportParam)Session["VATReportParam"];
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            DateTime pFromDate;
            DateTime pToDate;
            if (reportparam == null || reportparam.FromDate.Date.ToString().Contains("0001"))
            {
                pFromDate = CommonFunctions.GetFirstDayofMonth().Date; //.AddDays(-1);
                pToDate = CommonFunctions.GetLastDayofMonth().Date;
                pFromDate = AccountsDAO.CheckParamDate(pFromDate, yearid).Date;
                pToDate = AccountsDAO.CheckParamDate(pToDate, yearid).Date;
                reportparam = new TaxReportParam();
                reportparam.FromDate = pFromDate;
                reportparam.ToDate = pToDate;
                reportparam.Output = "PDF";
                reportparam.SortBy = "NonZeroTax";
                reportparam.ReportType = "Date";
                reportparam.TransactionType = "Summary";
            }
            else
            {
                if (reportparam.FromDate.Date.ToString() == "01-01-0001 00:00:00" || reportparam.FromDate.Date.ToString() == "01-01-0001")
                {
                    pFromDate = CommonFunctions.GetFirstDayofMonth().Date; //.AddDays(-1);
                    pToDate = CommonFunctions.GetLastDayofMonth().Date;
                    pFromDate = AccountsDAO.CheckParamDate(pFromDate, yearid).Date;
                    pToDate = AccountsDAO.CheckParamDate(pToDate, yearid).Date;
                    reportparam.FromDate = pFromDate;
                    reportparam.ToDate = pToDate;
                    reportparam.Output = "PDF";
                }
                else
                {
                    reportparam.FromDate = AccountsDAO.CheckParamDate(reportparam.FromDate, yearid);
                    reportparam.ToDate = AccountsDAO.CheckParamDate(reportparam.ToDate, yearid);
                }

            }

            Session["VATReportParam"] = reportparam;
            List<VoucherTypeVM> lsttype = new List<VoucherTypeVM>();
            lsttype.Add(new VoucherTypeVM { TypeName = "Summary" });
            lsttype.Add(new VoucherTypeVM { TypeName = "Detail" });
            

            ViewBag.VoucherTypes = lsttype;
            AccountsReportsDAO.GenerateVatRegister(reportparam);

            ViewBag.ReportName = "VAT Register";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("VATRegister_"))
                {
                    Session["ReportOutput"] = null;
                }
            }

            return View(reportparam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VATRegister([Bind(Include = "FromDate,ToDate,TransactionType,Output,ReportType,SortBy")] TaxReportParam picker)
        {
            TaxReportParam model = new TaxReportParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate,
                TransactionType = picker.TransactionType,
                Output = picker.Output,
                ReportType = picker.ReportType,
                SortBy = picker.SortBy

            };

            ViewBag.Token = model;
            
            Session["VATReportParam"] = model;
            AccountsReportsDAO.GenerateVatRegister(model);
            return RedirectToAction("VATRegister", "Reports");


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TaxReportParam([Bind(Include = "FromDate,ToDate,TransactionType,Output,ReportType,SortBy")] TaxReportParam picker)
        {
            TaxReportParam model = new TaxReportParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate,
                TransactionType= picker.TransactionType,                
                Output = picker.Output,
                ReportType = picker.ReportType,
                SortBy = picker.SortBy

            };            

            ViewBag.Token = model;
            SessionDataModel.SetTaxReportParam(model);

            AccountsReportsDAO.GenerateTaxRegister();
            return RedirectToAction("TaxRegister", "Reports");


        }


        #endregion

        #region "CustomerLedger"
        public ActionResult CustomerLedger()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            CustomerLedgerReportParam model = SessionDataModel.GetCustomerLedgerReportParam();
            if (model == null)
            {
                model = new CustomerLedgerReportParam
                {
                    FromDate = AccountsDAO.CheckParamDate(CommonFunctions.GetFirstDayofMonth().Date, yearid), //.AddDays(-1);,
                    ToDate = AccountsDAO.CheckParamDate(CommonFunctions.GetLastDayofMonth().Date, yearid) ,
                    AsonDate = AccountsDAO.CheckParamDate(CommonFunctions.GetLastDayofMonth().Date, yearid),
                    CustomerId = 0,
                    CustomerName = "",
                    Output = "PDF",
                    ReportType = "Ledger",
                    CustomerType="CR"
                };
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }
            
            model.FromDate = AccountsDAO.CheckParamDate(model.FromDate, yearid).Date;
            model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;
            
            SessionDataModel.SetCustomerLedgerParam(model);

            

            ViewBag.ReportName = "Customer Ledger";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("CustomerLedger") && model.ReportType == "Ledger")
                {
                    Session["ReportOutput"] = null;
                }
                else if (!currentreport.Contains("CustomerOutStanding") && model.ReportType == "OutStanding")
                {
                    Session["ReportOutput"] = null;
                }
            }

            return View(model);

        }

        [HttpPost]
        public ActionResult CustomerLedger(CustomerLedgerReportParam picker)
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            CustomerLedgerReportParam model = new CustomerLedgerReportParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate,
                CustomerId = picker.CustomerId,
                CustomerName = picker.CustomerName,
                Output =picker.Output,
                ReportType = picker.ReportType,
                AsonDate = picker.AsonDate,
                CustomerType=picker.CustomerType

            };

            model.FromDate = AccountsDAO.CheckParamDate(model.FromDate, yearid).Date;
            model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.Token = model;
            SessionDataModel.SetCustomerLedgerParam(model);
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.GenerateCustomerLedgerDetailReport();

            return RedirectToAction("CustomerLedger", "Reports");


        }
        #endregion

        #region "CustomerOutstanding"
        public ActionResult CustomerOutstanding()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            //var _ReportLog = db.ReportLogs.Where(cc => cc.ReportName == "Customer Outstanding").OrderByDescending(cc => cc.CreatedDate).ToList();
            CustomerLedgerReportParam model = SessionDataModel.GetCustomerLedgerReportParam();
            if (model == null)
            {
                model = new CustomerLedgerReportParam
                {
                    FromDate = AccountsDAO.CheckParamDate(CommonFunctions.GetFirstDayofMonth().Date, yearid), //.AddDays(-1);,
                    ToDate = AccountsDAO.CheckParamDate(CommonFunctions.GetLastDayofMonth().Date, yearid),
                    CustomerId = 0,
                    CustomerName = "",
                    Output = "PDF",
                    CustomerType="CR",
                    ReportType = "OutStanding"
                };
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }

            model.FromDate = AccountsDAO.CheckParamDate(model.FromDate, yearid).Date;
            model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            SessionDataModel.SetCustomerLedgerParam(model);

            

            ViewBag.ReportName = "Customer Outstanding";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("CustomerLedger") && model.ReportType == "Ledger")
                {
                    Session["ReportOutput"] = null;
                }
                else if (!currentreport.Contains("CustomerOutStanding") && model.ReportType == "OutStanding")
                {
                    Session["ReportOutput"] = null;
                }
            }

          
            return View(model);

        }

        [HttpPost]
        public ActionResult CustomerOutstanding(CustomerLedgerReportParam picker)
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            CustomerLedgerReportParam model = new CustomerLedgerReportParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                CustomerId = picker.CustomerId,
                CustomerName = picker.CustomerName,
                Output = picker.Output,
                ReportType = "OutStanding",// picker.ReportType,
                CustomerType =picker.CustomerType
            };
            model.FromDate = AccountsDAO.CheckParamDate(model.FromDate, yearid).Date;
            model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.Token = model;
            SessionDataModel.SetCustomerLedgerParam(model);
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            if (model.ReportType == "Ledger")
            {
                //AccountsReportsDAO.GenerateCustomerLedgerReport();
                AccountsReportsDAO.GenerateCustomerLedgerDetailReport();
            }
            else if (model.ReportType == "OutStanding")
            {
                AccountsReportsDAO.GenerateCustomerOutStandingReport();
            }
            else if (model.ReportType == "AWBUnAllocated")
            {
                AccountsReportsDAO.GenerateAWBOutStandingReport();
            }
            else if (model.ReportType == "AWBOutStanding")
            {
                AccountsReportsDAO.GenerateAWBUnInvoiced();
            }

            return RedirectToAction("CustomerOutstanding", "Reports");


        }
        #endregion

        #region "CustomerStatement"
        public ActionResult CustomerStatement()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            CustomerLedgerReportParam model =(CustomerLedgerReportParam)Session["CustomerStatementParam"];// SessionDataModel.GetCustomerLedgerReportParam();
            if (model == null)
            {
                model = new CustomerLedgerReportParam
                {
                    FromDate = AccountsDAO.CheckParamDate(CommonFunctions.GetFirstDayofMonth().Date, yearid), //.AddDays(-1);,
                    ToDate = AccountsDAO.CheckParamDate(CommonFunctions.GetLastDayofMonth().Date, yearid),
                    AsonDate = AccountsDAO.CheckParamDate(CommonFunctions.GetLastDayofMonth().Date, yearid),
                    CustomerId = 0,
                    CustomerName = "",
                    Output = "PDF",
                    ReportType = "Ledger",
                    CustomerType="CR"
                };
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }
            if (model.AsonDate.ToString() == "01-01-0001 00:00:00")
            {
                model.AsonDate = CommonFunctions.GetLastDayofMonth().Date;
            }
            

            model.AsonDate = AccountsDAO.CheckParamDate(model.AsonDate, yearid).Date;
            model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            Session["CustomerStatementParam"] = model;
            ViewBag.ReportName = "Customer Statement";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("CustomerStatement"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }

        [HttpPost]
        public ActionResult CustomerStatement(CustomerLedgerReportParam picker)
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            CustomerLedgerReportParam model = new CustomerLedgerReportParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                CustomerId = picker.CustomerId,
                CustomerName = picker.CustomerName,
                Output = picker.Output,
                ReportType = picker.ReportType,
                AsonDate = picker.AsonDate,
                CustomerType=picker.CustomerType
            };
            model.AsonDate = AccountsDAO.CheckParamDate(model.AsonDate, yearid).Date;
            model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            Session["CustomerStatementParam"] = model;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.GenerateCustomerStatementReport();
            //if (picker.CustomerType == "CR")
            //    AccountsReportsDAO.GenerateCustomerStatementReport();
            //else
            //    AccountsReportsDAO.GenerateCOLoaderStatementReport();

            return RedirectToAction("CustomerStatement", "Reports");


        }
        #endregion


        #region "UnAllocated Invoice and Receipts"
        public ActionResult UnallocatedInvoiceReceipts()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            CustomerLedgerReportParam model = (CustomerLedgerReportParam)Session["UnAllocatedInvoiceReceipt"];
            if (model == null)
            {
                model = new CustomerLedgerReportParam
                {
                    FromDate = AccountsDAO.CheckParamDate(CommonFunctions.GetFirstDayofMonth().Date, yearid), //.AddDays(-1);,
                    ToDate = AccountsDAO.CheckParamDate(CommonFunctions.GetLastDayofMonth().Date, yearid),
                    AsonDate = AccountsDAO.CheckParamDate(CommonFunctions.GetLastDayofMonth().Date, yearid),
                    CustomerId = 0,
                    CustomerName = "",
                    Output = "PDF",
                    ReportType = "Receipts",
                    CustomerType = "CR"
                };
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }

            model.FromDate = AccountsDAO.CheckParamDate(model.FromDate, yearid).Date;
            model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

         
            Session["UnAllocatedInvoiceReceipt"] = model;


            ViewBag.ReportName = "Unallocated Invoice & Receipts";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("UnAllocatedInvoiceReceipt") && model.ReportType == "Receipts")
                {
                    Session["ReportOutput"] = null;
                }
                else if (!currentreport.Contains("UnAllocatedInvoiceReceipt") && model.ReportType == "Invoice")
                {
                    Session["ReportOutput"] = null;
                }
            }

            return View(model);

        }

        [HttpPost]
        public ActionResult UnallocatedInvoiceReceipts(CustomerLedgerReportParam picker)
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            CustomerLedgerReportParam model = new CustomerLedgerReportParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate,
                CustomerId = picker.CustomerId,
                CustomerName = picker.CustomerName,
                Output = picker.Output,
                ReportType = picker.ReportType,
                AsonDate = picker.AsonDate,
                CustomerType = picker.CustomerType

            };

            model.FromDate = AccountsDAO.CheckParamDate(model.FromDate, yearid).Date;
            model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.Token = model;
            Session["UnAllocatedInvoiceReceipt"] = model;
            
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.GenerateUnAllocatedInvoiceReceiptReport();

            return RedirectToAction("UnallocatedInvoiceReceipts", "Reports");


        }
        #endregion
        #region supplierledger
        public ActionResult SupplierLedger()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            var supplierMasterTypes = (from d in db.SupplierTypes select d).ToList();
            ViewBag.SupplierType = supplierMasterTypes;
            SupplierLedgerReportParam model = SessionDataModel.GetSupplierLedgerReportParam();
            if (model == null)
            {
                model = new SupplierLedgerReportParam
                {
                    FromDate = AccountsDAO.CheckParamDate(CommonFunctions.GetFirstDayofMonth().Date, yearid), //.AddDays(-1);,
                    ToDate = AccountsDAO.CheckParamDate(CommonFunctions.GetLastDayofMonth().Date, yearid),
                    AsonDate = AccountsDAO.CheckParamDate(CommonFunctions.GetLastDayofMonth().Date, yearid),                
                    SupplierTypeId = 1,
                    SupplierId = 0,
                    SupplierName = "",
                    Output = "PDF",
                    ReportType = "Ledger"
                };
            }
            if (model.AsonDate.ToString() == "01-01-0001 00:00:00")
            {
                model.AsonDate = CommonFunctions.GetLastDayofMonth().Date;
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofYear().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }
            SessionDataModel.SetSupplierLedgerParam(model);

            model.FromDate = AccountsDAO.CheckParamDate(model.FromDate, yearid).Date;
            model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.ReportName = "Supplier Ledger";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("SupplierLedger") && model.ReportType == "Ledger")
                {
                    Session["ReportOutput"] = null;
                }
                else if (!currentreport.Contains("CustomerOutStanding") && model.ReportType == "OutStanding")
                {
                    Session["ReportOutput"] = null;
                }
            }

            return View(model);

        }

        [HttpPost]
        public ActionResult SupplierLedger(SupplierLedgerReportParam picker)
        {

            SupplierLedgerReportParam model = new SupplierLedgerReportParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                AsonDate = picker.AsonDate,
                SupplierId = picker.SupplierId,
                SupplierName = picker.SupplierName,
                SupplierTypeId=picker.SupplierTypeId,
                Output = picker.Output,
                ReportType = "Ledger"
            };

            ViewBag.Token = model;
            SessionDataModel.SetSupplierLedgerParam(model);
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            if (model.ReportType == "Ledger")
            {
                //AccountsReportsDAO.GenerateCustomerLedgerReport();
                AccountsReportsDAO.GenerateSupplierLedgerDetailReport();
            }
            else if (model.ReportType == "Statement")
            {
                AccountsReportsDAO.GenerateSupplierStatementDetailReport();
            }

            return RedirectToAction("SupplierLedger", "Reports");


        }
        #endregion

        #region supplierStatement
        public ActionResult SupplierStatement()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            var supplierMasterTypes = (from d in db.SupplierTypes select d).ToList();
            ViewBag.SupplierType = supplierMasterTypes;
            SupplierLedgerReportParam model = SessionDataModel.GetSupplierLedgerReportParam();
            if (model == null)
            {
                model = new SupplierLedgerReportParam
                {
                    FromDate = CommonFunctions.GetFirstDayofMonth().Date, //.AddDays(-1);,
                    AsonDate = CommonFunctions.GetLastDayofMonth().Date, //.AddDays(-1);,
                    ToDate = CommonFunctions.GetLastDayofMonth().Date,
                    SupplierTypeId = 1,
                    SupplierId = 0,
                    SupplierName = "",
                    Output = "PDF",
                    ReportType = "Statement"
                };
            }
            if (model.AsonDate.ToString() == "01-01-0001 00:00:00")
            {
                model.AsonDate = CommonFunctions.GetLastDayofMonth().Date;
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofYear().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }


            model.FromDate = AccountsDAO.CheckParamDate(model.FromDate, yearid).Date;
            model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            SessionDataModel.SetSupplierLedgerParam(model);
            ViewBag.ReportName = "Supplier Statement";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("SupplierStatement"))
                {
                    Session["ReportOutput"] = null;
                }
            }

            return View(model);

        }

        [HttpPost]
        public ActionResult SupplierStatement(SupplierLedgerReportParam picker)
        {
            SupplierLedgerReportParam model = SessionDataModel.GetSupplierLedgerReportParam();
            model = new SupplierLedgerReportParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                AsonDate = picker.AsonDate,
                SupplierId = picker.SupplierId,
                SupplierName = picker.SupplierName,
                SupplierTypeId = picker.SupplierTypeId,
                Output = picker.Output,
                ReportType = "Statement"
            };

            ViewBag.Token = model;
            SessionDataModel.SetSupplierLedgerParam(model);
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            AccountsReportsDAO.GenerateSupplierStatementDetailReport();


            return RedirectToAction("SupplierStatement", "Reports");


        }
        #endregion

        #region supplierOutStanding
        public ActionResult SupplierOutStanding()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            var supplierMasterTypes = (from d in db.SupplierTypes select d).ToList();
            ViewBag.SupplierType = supplierMasterTypes;
            SupplierLedgerReportParam model = SessionDataModel.GetSupplierLedgerReportParam();
            if (model == null)
            {
                model = new SupplierLedgerReportParam
                {
                    FromDate = CommonFunctions.GetFirstDayofMonth().Date, //.AddDays(-1);,
                    AsonDate = CommonFunctions.GetFirstDayofMonth().Date, //.AddDays(-1);,
                    ToDate = CommonFunctions.GetLastDayofMonth().Date,
                    SupplierTypeId = 1,
                    SupplierId = 0,
                    SupplierName = "",
                    Output = "PDF",
                    ReportType = "Ledger"
                };
            }
            if (model.AsonDate.ToString() == "01-01-0001 00:00:00")
            {
                model.AsonDate = CommonFunctions.GetLastDayofMonth().Date;
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofYear().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }
            SessionDataModel.SetSupplierLedgerParam(model);

            model.FromDate = AccountsDAO.CheckParamDate(model.FromDate, yearid).Date;
            model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.ReportName = "Supplier OutStanding";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("SupplierOutStanding") )
                {
                    Session["ReportOutput"] = null;
                }
                
            }

            return View(model);

        }

        [HttpPost]
        public ActionResult SupplierOutStanding(SupplierLedgerReportParam picker)
        {

            SupplierLedgerReportParam model = new SupplierLedgerReportParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                AsonDate = picker.AsonDate,
                SupplierId = picker.SupplierId,
                SupplierName = picker.SupplierName,
                SupplierTypeId = picker.SupplierTypeId,
                Output = picker.Output,
                ReportType = "Ledger"
            };

            ViewBag.Token = model;
            SessionDataModel.SetSupplierLedgerParam(model);
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            
                //AccountsReportsDAO.GenerateCustomerLedgerReport();
           AccountsReportsDAO.GenerateSupplierOutStandingReport();
            

            return RedirectToAction("SupplierOutStanding", "Reports");


        }
        #endregion
        #region "CustomerAging"
        public ActionResult CustomerAging()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            CustomerLedgerReportParam model = SessionDataModel.GetCustomerLedgerReportParam();
            if (model == null)
            {
                model = new CustomerLedgerReportParam
                {

                    AsonDate = CommonFunctions.GetLastDayofMonth().Date, //.AddDays(-1);,
                    CustomerId = 0,
                    CustomerName = "",
                    Output = "PDF",
                    ReportType = "Summary",
                    CustomerType="CR"
                };
            }
            if (model.AsonDate.ToString() == "01-01-0001 00:00:00")
            {
                model.AsonDate = CommonFunctions.GetLastDayofMonth().Date;
            }

            SessionDataModel.SetCustomerLedgerParam(model);

            model.AsonDate = AccountsDAO.CheckParamDate(model.FromDate, yearid).Date;
            ViewBag.ReportName = "Customer Aging Report";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("CustomerAging"))
                {
                    Session["ReportOutput"] = null;
                }
            }

            return View(model);

        }

        [HttpPost]
        public ActionResult CustomerAging(CustomerLedgerReportParam picker)
        {

            CustomerLedgerReportParam model = new CustomerLedgerReportParam
            {
                CustomerId = picker.CustomerId,
                CustomerName = picker.CustomerName,
                Output = picker.Output,
                ReportType = picker.ReportType,
                AsonDate = picker.AsonDate,
                CustomerType=picker.CustomerType
            };

            ViewBag.Token = model;
            SessionDataModel.SetCustomerLedgerParam(model);
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            if (model.CustomerType == "CR")
            {
                AccountsReportsDAO.GenerateCustomerAgingReport();
            }
            else
            {
                AccountsReportsDAO.GenerateCOLoaderAgingReport();
            }

            return RedirectToAction("CustomerAging", "Reports");


        }
        #endregion


        #region "SupplierAging"
        public ActionResult SupplierAging()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            var supplierMasterTypes = (from d in db.SupplierTypes select d).ToList();
            ViewBag.SupplierType = supplierMasterTypes;

            SupplierLedgerReportParam model = SessionDataModel.GetSupplierLedgerReportParam();
            if (model == null)
            {
                model = new SupplierLedgerReportParam
                {

                    AsonDate = CommonFunctions.GetLastDayofMonth().Date, //.AddDays(-1);,
                    SupplierId = 0,
                    SupplierName = "",
                    Output = "PDF",
                    ReportType = "Summary"
                };
            }
            if (model.AsonDate.ToString() == "01-01-0001 00:00:00")
            {
                model.AsonDate = CommonFunctions.GetLastDayofMonth().Date;
            }

            SessionDataModel.SetSupplierLedgerParam(model);

            model.AsonDate = AccountsDAO.CheckParamDate(model.FromDate, yearid).Date;
            ViewBag.ReportName = "Supplier Aging Report";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("SupplierAging"))
                {
                    Session["ReportOutput"] = null;
                }
            }

            return View(model);

        }

        [HttpPost]
        public ActionResult SupplierAging(SupplierLedgerReportParam picker)
        {

            SupplierLedgerReportParam model = new SupplierLedgerReportParam
            {
                SupplierId = picker.SupplierId,
                SupplierName = picker.SupplierName,
                Output = picker.Output,
                ReportType = picker.ReportType,
                AsonDate = picker.AsonDate,
                SupplierTypeId =picker.SupplierTypeId
            };

            ViewBag.Token = model;
            SessionDataModel.SetSupplierLedgerParam(model);
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            AccountsReportsDAO.GenerateSupplierAgingReport();

            return RedirectToAction("SupplierAging", "Reports");


        }
        #endregion

        #region "SalesRevenueSummary"
        public ActionResult SalesRevenueSummary()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            AccountsReportParam model = (AccountsReportParam)(Session["SalesRevenueSummaryParam"]);
            if (model == null)
            {
                model = new AccountsReportParam
                {
                    FromDate = AccountsDAO.CheckParamDate(CommonFunctions.GetFirstDayofMonth().Date, yearid), //.AddDays(-1);,
                    ToDate = AccountsDAO.CheckParamDate(CommonFunctions.GetLastDayofMonth().Date, yearid),                    
                    Output = "PDF",
                    CurrentPeriod =false
                };
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }

            model.FromDate = AccountsDAO.CheckParamDate(model.FromDate, yearid).Date;
            model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            Session["SalesRevenueSummaryParam"] = model;



            ViewBag.ReportName = "Customer Sales Summary";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("CustomerSalesSummary") && model.ReportType == "Ledger")
                {
                    Session["ReportOutput"] = null;
                }
             
            }

            return View(model);

        }

        [HttpPost]
        public ActionResult SalesRevenueSummary(AccountsReportParam picker)
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            AccountsReportParam model = new AccountsReportParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate,               
                Output = picker.Output,
                ReportType = picker.ReportType,
                CurrentPeriod = picker.CurrentPeriod
                

            };

            model.FromDate = AccountsDAO.CheckParamDate(model.FromDate, yearid).Date;
            model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.Token = model;
            Session["SalesRevenueSummaryParam"] = model;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.GenerateSalesRevenueSummaryReport();

            return RedirectToAction("SalesRevenueSummary", "Reports");


        }
        #endregion

        #region supplier Unallocated Invoice Payment 
        public ActionResult UnAllocatedSupplierPayments()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            var supplierMasterTypes = (from d in db.SupplierTypes select d).ToList();
            ViewBag.SupplierType = supplierMasterTypes;
            SupplierLedgerReportParam model = (SupplierLedgerReportParam)Session["UnAllocatedInvoicePayment"];
            
            if (model == null)
            {
                model = new SupplierLedgerReportParam
                {
                    FromDate = AccountsDAO.CheckParamDate(CommonFunctions.GetFirstDayofMonth().Date, yearid), //.AddDays(-1);,
                    ToDate = AccountsDAO.CheckParamDate(CommonFunctions.GetLastDayofMonth().Date, yearid),
                    AsonDate = AccountsDAO.CheckParamDate(CommonFunctions.GetLastDayofMonth().Date, yearid),
                    SupplierTypeId = 1,
                    SupplierId = 0,
                    SupplierName = "",
                    Output = "PDF",
                    ReportType = "Payments"
                };
            }
            if (model.AsonDate.ToString() == "01-01-0001 00:00:00")
            {
                model.AsonDate = CommonFunctions.GetLastDayofMonth().Date;
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofYear().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }
            
            Session["UnAllocatedInvoicePayment"] = model;
            
            model.FromDate = AccountsDAO.CheckParamDate(model.FromDate, yearid).Date;
            model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.ReportName = "Supplier Ledger";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("UnAllocatedSupplierInvoice") && model.ReportType == "Payments")
                {
                    Session["ReportOutput"] = null;
                }
                else if (!currentreport.Contains("UnAllocatedSupplierInvoice") && model.ReportType == "Invoice")
                {
                    Session["ReportOutput"] = null;
                }
            }

            return View(model);

        }

        [HttpPost]
        public ActionResult UnAllocatedSupplierPayments(SupplierLedgerReportParam picker)
        {

            SupplierLedgerReportParam model = new SupplierLedgerReportParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate, //.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                AsonDate = picker.AsonDate,
                SupplierId = picker.SupplierId,
                SupplierName = picker.SupplierName,
                SupplierTypeId = picker.SupplierTypeId,
                Output = picker.Output,
                ReportType = picker.ReportType
            };

            ViewBag.Token = model;
            Session["UnAllocatedInvoicePayment"] = model;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
         
                //AccountsReportsDAO.GenerateCustomerLedgerReport();
            AccountsReportsDAO.GenerateUnAllocatedSupplierInvoicePaymentReport();
            
            return RedirectToAction("UnAllocatedSupplierPayments", "Reports");


        }
        #endregion

        #region "CODRegister & Pending"
        public ActionResult CODRegister()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            CODReportParam model = (CODReportParam)Session["CODReportParam"];
            if (model == null)
            {
                model = new CODReportParam 
                {

                    FromDate = CommonFunctions.GetFirstDayofMonth().Date, //.AddDays(-1);,                    
                    ToDate = CommonFunctions.GetLastDayofMonth().Date, //.AddDays(-1);,                    
                    Output = "PDF",
                    ReportType = "Detail",
                    MovementId="1,2,3,4"
                };
                Session["CODReportParam"] = model;
            }            
            
            ViewBag.ReportName = "COD Register Report";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("CODRegister") && !currentreport.Contains("CODSummary"))
                {
                    Session["ReportOutput"] = null;
                }
            }
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            ViewBag.parceltype = db.ParcelTypes.ToList();
            ViewBag.Movement = db.CourierMovements.ToList();
            return View(model);

        }

        [HttpPost]
        public ActionResult CODRegister(CODReportParam picker)
        {

          
            picker.MovementId = "";
            if (picker.SelectedValues != null)
            {
                foreach (var item in picker.SelectedValues)
                {
                    if (picker.MovementId == "")
                    {
                        picker.MovementId = item.ToString();
                    }
                    else
                    {
                        picker.MovementId = picker.MovementId + "," + item.ToString();
                    }

                }
            }
            Session["CODReportParam"] = picker;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            if (picker.ReportType=="Detail")
                AccountsReportsDAO.GenerateCODRegister();
            else
                AccountsReportsDAO.GenerateCODSummary();

            return RedirectToAction("CODRegister", "Reports");
        }

        
        public ActionResult CODPending()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            CODReportParam model = (CODReportParam)Session["CODReportParam"];
            if (model == null)
            {
                model = new CODReportParam
                {

                    FromDate = CommonFunctions.GetFirstDayofMonth().Date, //.AddDays(-1);,                    
                    ToDate = CommonFunctions.GetLastDayofMonth().Date, //.AddDays(-1);,                    
                    Output = "PDF",
                    ReportType = "Detail",
                    MovementId = "1,2,3,4"
                };
                Session["CODReportParam"] = model;
            }

            ViewBag.ReportName = "COD Pending Report";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("CODPending"))
                {
                    Session["ReportOutput"] = null;
                }
            }
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            ViewBag.parceltype = db.ParcelTypes.ToList();
            ViewBag.Movement = db.CourierMovements.ToList();
            return View(model);

        }

        [HttpPost]
        public ActionResult CODPending(CODReportParam picker)
        {


            picker.MovementId = "";
            if (picker.SelectedValues != null)
            {
                foreach (var item in picker.SelectedValues)
                {
                    if (picker.MovementId == "")
                    {
                        picker.MovementId = item.ToString();
                    }
                    else
                    {
                        picker.MovementId = picker.MovementId + "," + item.ToString();
                    }

                }
            }
            Session["CODReportParam"] = picker;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.GenerateCODPending();

            return RedirectToAction("CODPending", "Reports");
        }



        #endregion

        #region "SalesRegistersummary"
        public ActionResult SalesRegisterSummary()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            ViewBag.Employee = db.EmployeeMasters.ToList();
            SalesRegisterSummaryParam model = (SalesRegisterSummaryParam)Session["SalesRegisterSummaryParam"];
            if (model == null)
            {
                model = new SalesRegisterSummaryParam
                {
                    FromDate = CommonFunctions.GetFirstDayofMonth().Date, //.AddDays(-1);,
                    ToDate = CommonFunctions.GetLastDayofMonth().Date,
                    EmployeeID = 0,
                    CustomerId = 0,
                    CustomerName = "",
                    Output = "PDF",
                    ReportType = "Ledger"
                };
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }

            Session["SalesRegisterSummaryParam"] = model;

            //model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.ReportName = "Sales Register Summary";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("SalesRegisterSummary"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }

        [HttpPost]
        public ActionResult SalesRegisterSummary(SalesRegisterSummaryParam picker)
        {

            SalesRegisterSummaryParam model = new SalesRegisterSummaryParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                CustomerId = picker.CustomerId,
                CustomerName = picker.CustomerName,
                Output = picker.Output,
                EmployeeID = picker.EmployeeID,
                ReportType = picker.ReportType
            };

            ViewBag.Token = model;
            Session["SalesRegisterSummaryParam"] = model;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.GenerateSalesRegisterSummaryReport();

            return RedirectToAction("SalesRegisterSummary", "Reports");


        }



        #endregion

        #region "SalesReport"
        public ActionResult SalesReport()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            ViewBag.Employee = db.EmployeeMasters.ToList();
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            SalesReportParam model = (SalesReportParam)Session["SalesReportParam"];
            if (model == null)
            {
                model = new SalesReportParam
                {
                    FromDate = AccountsDAO.CheckParamDate(CommonFunctions.GetFirstDayofMonth().Date, yearid).Date, //.AddDays(-1);,
                    ToDate = AccountsDAO.CheckParamDate(CommonFunctions.GetLastDayofMonth().Date, yearid).Date, //.AddDays(-1);,,                   
                    EmployeeID = 0,
                    CustomerId = 0,
                    CustomerName = "",
                    Output = "PDF",
                    ReportType = "Ledger",
                    MovementId="1,2,3,4"
                };
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }

            model.FromDate = AccountsDAO.CheckParamDate(model.FromDate, yearid).Date;
            model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            Session["SalesReportParam"] = model;
            ViewBag.ReportName = "Sales Report";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("SalesReport"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }

        [HttpPost]
        public ActionResult SalesReport(SalesReportParam picker)
        {

            SalesReportParam model = new SalesReportParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                CustomerId = picker.CustomerId,
                CustomerName = picker.CustomerName,
                Output = picker.Output,
                EmployeeID = picker.EmployeeID,
                ReportType = picker.ReportType,
                CustomerType = picker.CustomerType,
                PaymentModeId = picker.PaymentModeId,
                MovementId = picker.MovementId
            };
            model.MovementId = "";
            if (picker.SelectedValues != null)
            {
                foreach (var item in picker.SelectedValues)
                {
                    if (model.MovementId == "")
                    {
                        model.MovementId = item.ToString();
                    }
                    else
                    {
                        model.MovementId = model.MovementId + "," + item.ToString();
                    }

                }
            }
            ViewBag.Token = model;
            Session["SalesReportParam"] = model;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.GenerateSalesReport();

            return RedirectToAction("SalesReport", "Reports");


        }
        #endregion


        #region "MaterialCostReport"
        public ActionResult MaterialCostLedger()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            ViewBag.Employee = db.EmployeeMasters.ToList();
            MaterialCostLedgerParam model = (MaterialCostLedgerParam)Session["MaterialCostLedgerParam"];
            if (model == null)
            {
                model = new MaterialCostLedgerParam  
                {
                    FromDate = CommonFunctions.GetFirstDayofMonth().Date, //.AddDays(-1);,
                    ToDate = CommonFunctions.GetLastDayofMonth().Date,                     
                    Output = "PDF",
                    ReportType = "Ledger",
                    Pending=true
                };
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }

            Session["MaterialCostLedgerParam"] = model;

            //model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.ReportName = "Material Cost Ledger Report";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("MaterialCostLedger"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }

        [HttpPost]
        public ActionResult MaterialCostLedger(MaterialCostLedgerParam picker)
        {

            MaterialCostLedgerParam model = new MaterialCostLedgerParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),                
                Output = picker.Output,                
                ReportType = picker.ReportType,
                Pending=picker.Pending,
                Shipper =picker.Shipper,
                Receiver =picker.Receiver

            };

            ViewBag.Token = model;
            Session["MaterialCostLedgerParam"] = model;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.GenerateMaterialCostLedgerReport();

            return RedirectToAction("MaterialCostLedger", "Reports");


        }
        #endregion


        #region "MaterialCostReport"
        public ActionResult MCPaymentList()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            ViewBag.Employee = db.EmployeeMasters.ToList();
            MaterialCostLedgerParam model = (MaterialCostLedgerParam)Session["MCPaymentListParam"];
            if (model == null)
            {
                model = new MaterialCostLedgerParam
                {
                    FromDate = CommonFunctions.GetFirstDayofMonth().Date, //.AddDays(-1);,
                    ToDate = CommonFunctions.GetLastDayofMonth().Date,
                    Output = "PDF",
                    ReportType = "Ledger",
                    Pending = true
                };
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }

            Session["MCPaymentListParam"] = model;

            //model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.ReportName = "Material Cost Payment List";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("MaterialCostPaymentList"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }

        [HttpPost]
        public ActionResult MCPaymentList(MaterialCostLedgerParam picker)
        {

            MaterialCostLedgerParam model = new MaterialCostLedgerParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                Output = picker.Output,
                ReportType = picker.ReportType,
                Pending = picker.Pending,
                Shipper =picker.Shipper,
                Receiver =picker.Receiver

            };

            ViewBag.Token = model;
            Session["MCPaymentListParam"] = model;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.GenerateMaterialCostPaymentList();

            return RedirectToAction("MCPaymentList", "Reports");


        }
        #endregion
        #region "CustomerInvoiceRegister"
        public ActionResult CustomerInvoiceRegister()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            CustomerInvoiceReportParam model = (CustomerInvoiceReportParam)Session["CustomerInvoiceRegisterParam"];

            if (model == null)
            {
                model = new CustomerInvoiceReportParam
                {
                    FromDate = CommonFunctions.GetFirstDayofMonth().Date, //.AddDays(-1);,
                    ToDate = CommonFunctions.GetLastDayofMonth().Date,                    
                    CustomerId = 0,
                    CustomerName = "",
                    Output = "PDF",
                    ReportType = "Ledger",
                    FromNo="",
                    ToNo="",
                    DateWise=true
                };
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }
            Session["CustomerInvoiceRegisterParam"]=model;

            //model.FromDate = AccountsDAO.CheckParamDate(model.FromDate, yearid).Date;
            //model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.ReportName = "Customer Invoice Register";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("CustomerInvoiceRegister"))
                {
                    Session["ReportOutput"] = null;
                }
                
            }

            return View(model);

        }

        [HttpPost]
        public ActionResult CustomerInvoiceRegister(CustomerInvoiceReportParam picker)
        {

            CustomerInvoiceReportParam model = new CustomerInvoiceReportParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate,
                CustomerId = picker.CustomerId,
                CustomerName = picker.CustomerName,
                Output = picker.Output,
                ReportType = picker.ReportType,                
                FromNo=picker.FromNo,
                ToNo=picker.ToNo,
                DateWise=picker.DateWise
            };

            ViewBag.Token = model;
            Session["CustomerInvoiceRegisterParam"] = model;
            
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.GenerateCustomerInvoiceRegisterReport();

            return RedirectToAction("CustomerInvoiceRegister", "Reports");


        }

        [HttpGet]
        public JsonResult GetCustomerInvoiceNo(string term)
        {
            if (term.Trim() != "")
            {
                var customerlist = (from c1 in db.CustomerInvoices
                                    where c1.CustomerID > 0 && c1.CustomerInvoiceNo.Contains(term)
                                    orderby c1.CustomerInvoiceNo ascending
                                    select new { InvoiceNo = c1.CustomerInvoiceNo }).Take(20).ToList();

                return Json(customerlist, JsonRequestBehavior.AllowGet);
            }
            else
            {

                var customerlist = (from c1 in db.CustomerInvoices
                                    where c1.CustomerID > 0 
                                    orderby c1.CustomerInvoiceNo ascending
                                    select new { InvoiceNo = c1.CustomerInvoiceNo }).Take(20).ToList();

                return Json(customerlist, JsonRequestBehavior.AllowGet);
            }


        }

        public ActionResult CustomerInvoicePrintMultiple()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            CustomerLedgerReportParam model = (CustomerLedgerReportParam)Session["CustomerInvoicePrintParam"];

            if (model == null)
            {
                model = new CustomerLedgerReportParam
                {
                    FromDate = CommonFunctions.GetFirstDayofMonth().Date, //.AddDays(-1);,
                    ToDate = CommonFunctions.GetLastDayofMonth().Date,
                    AsonDate = CommonFunctions.GetFirstDayofMonth().Date, //.AddDays(-1);,
                    CustomerId = 0,
                    CustomerName = "",
                    Output = "PDF",
                    ReportType = "Ledger"
                };
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }
            SessionDataModel.SetCustomerLedgerParam(model);

            model.FromDate = AccountsDAO.CheckParamDate(model.FromDate, yearid).Date;
            model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.ReportName = "Customer Invoice Print";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("CustomerInvoiceMultiplePrint"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }
        [HttpPost]
        public ActionResult CustomerInvoicePrintMultiple(CustomerLedgerReportParam picker)
        {

            CustomerLedgerReportParam model = new CustomerLedgerReportParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                CustomerId = picker.CustomerId,
                CustomerName = picker.CustomerName,
                Output = picker.Output,
                ReportType = picker.ReportType,
                AsonDate = picker.AsonDate
            };

            ViewBag.Token = model;
            Session["CustomerInvoicePrintParam"] = model;

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.CustomerInvoiceMultiplePrint();

            return RedirectToAction("CustomerInvoicePrintMultiple", "Reports");


        }


        #endregion

        #region "CustomerInvoiceOpeningRegister"

    
        public ActionResult CustomerInvoiceOpeningRegister()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            AcInvoiceOpeningParam model = (AcInvoiceOpeningParam)Session["CustomerInvoiceOpeningRegisterParam"];

            if (model == null)
            {
                model = new AcInvoiceOpeningParam
                {                    
                    CustomerId = 0,
                    CustomerName = "",
                    Output = "PDF",
                    ReportType = "Ledger",
                    CustomerType = "CR"
                };
            }            

            ViewBag.ReportName = "Customer Invoice Opening Register";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("CustomerInvoiceOpeningRegister"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }

        [HttpPost]
        public ActionResult CustomerInvoiceOpeningRegister(AcInvoiceOpeningParam picker)
        {

            AcInvoiceOpeningParam model = new AcInvoiceOpeningParam
            {              
                CustomerId = picker.CustomerId,
                CustomerName = picker.CustomerName,
                Output = picker.Output,
                ReportType = picker.ReportType  ,
                CustomerType=picker.CustomerType
            };

            ViewBag.Token = model;
            Session["CustomerInvoiceOpeningRegisterParam"] = model;

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.GenerateCustomerOpeningRegisterReport();

            return RedirectToAction("CustomerInvoiceOpeningRegister", "Reports");


        }
        #endregion

        #region "SupplierInvoiceOpeningRegister"
        public ActionResult SupplierInvoiceOpeningRegister()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            var supplierMasterTypes = (from d in db.SupplierTypes select d).ToList();
            ViewBag.SupplierType = supplierMasterTypes;

            AcInvoiceOpeningParam model = (AcInvoiceOpeningParam)Session["SupplierInvoiceOpeningRegisterParam"];

            if (model == null)
            {
                model = new AcInvoiceOpeningParam
                {
                    SupplierTypeId=0,
                    SupplierId = 0,
                    SupplierName = "",
                    Output = "PDF",
                    ReportType = "Ledger"
                };
            }

            ViewBag.ReportName = "Supplier Invoice Opening Register";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("SupplierInvoiceOpeningRegister"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }

        [HttpPost]
        public ActionResult SupplierInvoiceOpeningRegister(AcInvoiceOpeningParam picker)
        {

            AcInvoiceOpeningParam model = new AcInvoiceOpeningParam
            {
                SupplierId = picker.SupplierId,
                SupplierName = picker.SupplierName,
                Output = picker.Output,
                ReportType = picker.ReportType,SupplierTypeId=picker.SupplierTypeId
            };

            ViewBag.Token = model;
            Session["SupplierInvoiceOpeningRegisterParam"] = model;

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.GenerateSupplierOpeningRegisterReport();

            return RedirectToAction("SupplierInvoiceOpeningRegister", "Reports");


        }
        #endregion


        #region "SalesRegisterCountry"
        public ActionResult SalesRegisterCountry()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            ViewBag.Employee = db.EmployeeMasters.ToList();
            SalesRegisterCountryParam model = (SalesRegisterCountryParam)Session["SalesRegisterCountryParam"];
            if (model == null)
            {
                model = new SalesRegisterCountryParam
                {
                    FromDate = CommonFunctions.GetFirstDayofMonth().Date, //.AddDays(-1);,
                    ToDate = CommonFunctions.GetLastDayofMonth().Date,               
                    CustomerId = 0,
                    CustomerName = "",
                    Output = "PDF",
                    ReportType = "Ledger"
                };
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }

            Session["SalesRegisterCountryParam"] = model;

            //model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.ReportName = "Sales Register Country Wise";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("SalesRegisterCountry"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }

        [HttpPost]
        public ActionResult SalesRegisterCountry(SalesRegisterCountryParam picker)
        {

            SalesRegisterCountryParam model = new SalesRegisterCountryParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                CustomerId = picker.CustomerId,
                CustomerName = picker.CustomerName,
                Output = picker.Output,                
                ReportType = picker.ReportType
            };

            ViewBag.Token = model;
            Session["SalesRegisterCountryParam"] = model;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.GenerateSalesRegisterCountryReport();

            return RedirectToAction("SalesRegisterCountry", "Reports");


        }



        #endregion

        #region "ImportList"
        public ActionResult ImportList()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            
            DateParam model = (DateParam)Session["ImportListParam"];
            if (model == null)
            {
                model = new DateParam
                {
                    FromDate = CommonFunctions.GetFirstDayofMonth().Date, //.AddDays(-1);,
                    ToDate = CommonFunctions.GetLastDayofMonth().Date,                    
                    Output = "PDF",
                    ReportType = "ImportList"
                };
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }

            Session["ImportListParam"] = model;

            //model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.ReportName = "Import Vat Invoice Register";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("ImportShipmentList"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }

        [HttpPost]
        public ActionResult ImportList(DateParam picker)
        {

            DateParam model = new DateParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),                
                Output = picker.Output,
                ReportType = picker.ReportType
            };

            ViewBag.Token = model;
            Session["ImportListParam"] = model;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.ImportListrepot();

            return RedirectToAction("ImportList", "Reports");


        }
        #endregion


        #region "CollectionReport"
        public ActionResult CollectionReport()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            DateParam model = (DateParam)Session["CollectionListParam"];
            if (model == null)
            {
                model = new DateParam
                {
                    FromDate = CommonFunctions.GetFirstDayofMonth().Date, //.AddDays(-1);,
                    ToDate = CommonFunctions.GetLastDayofMonth().Date,
                    Output = "PDF",
                    ReportType = "ImportList"
                };
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }

            Session["ImportListParam"] = model;

            //model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.ReportName = "Collection List";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("DailyCollectionReport"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }

        [HttpPost]
        public ActionResult CollectionReport(DateParam picker)
        {

            DateParam model = new DateParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                Output = picker.Output,
                ReportType = picker.ReportType
            };

            ViewBag.Token = model;
            Session["CollectionListParam"] = model;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.CollectionReport();

            return RedirectToAction("CollectionReport", "Reports");


        }
        #endregion

        #region "ExportManifestRegister"
        public ActionResult ExportManifestRegister()
        {
            ViewBag.ReportName = "Export Manifest Register";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("ExportManifestRegister_"))
                {
                    Session["ReportOutput"] = null;
                }
            }
            ViewBag.FwdAgentId = db.SupplierMasters.Where(cc=>cc.SupplierTypeID==4).OrderBy(cc => cc.SupplierName).ToList(); 
            return View();
        }

        public JsonResult GetExportManifestList(int FAgentId)
        {
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            var list = db.ExportShipments.Where(c => c.FAgentID == FAgentId && c.BranchID == branchid && c.AcFinancialYearID == yearid).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ExportManifestReportParam()
        {
            ExportManifestReportParam reportparam =(ExportManifestReportParam)Session["ExportManifestReportParam"];
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            DateTime pFromDate;
            DateTime pToDate;

            if (reportparam == null)
            {
                pFromDate = CommonFunctions.GetFirstDayofMonth().Date; //.AddDays(-1);
                pToDate = CommonFunctions.GetLastDayofMonth().Date;
                reportparam = new ExportManifestReportParam();
                reportparam.FromDate = pFromDate;
                reportparam.ToDate = pToDate;
                reportparam.Output = "PDF";                
                
            }
            else
            {
                if (reportparam.FromDate.Date.ToString() == "01-01-0001 00:00:00" || reportparam.FromDate.Date.ToString() == "01-01-0001")
                {
                    pFromDate = CommonFunctions.GetFirstDayofMonth().Date; //.AddDays(-1);
                    reportparam.FromDate = pFromDate;
                    reportparam.Output = "PDF";
                }
                else
                {

                }

            }

            Session["ExportManifestReportParam"] = reportparam;
            ViewBag.FwdAgentId = db.SupplierMasters.Where(cc => cc.SupplierTypeID == 4).OrderBy(cc => cc.SupplierName).ToList();
            return View(reportparam);

        }

        [HttpPost]        
        public ActionResult ExportManifestReportParam(ExportManifestReportParam picker)
        {
            ExportManifestReportParam model = new ExportManifestReportParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate,
                FAgentID = picker.FAgentID,
                ManifestId=picker.ManifestId,
                Output = picker.Output,
                ReportType = picker.ReportType,
                SortBy = picker.SortBy

            };

            ViewBag.Token = model;
            //SessionDataModel.SetTaxReportParam(model);
            Session["ExportManifestReportParam"] = model;
            AccountsReportsDAO.GenerateExportManifestRegister();
            return RedirectToAction("ExportManifestRegister", "Reports");
        }


        #endregion

        #region "DRSCashFlowReport"
        public ActionResult DRSCashFlow()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            DRSCashFlowReportParam model =(DRSCashFlowReportParam)Session["DRSCashFlowReportParam"];
            if (model == null)
            {
                model = new DRSCashFlowReportParam
                {
                    AsonDate = CommonFunctions.GetLastDayofMonth().Date, //.AddDays(-1);,                    
                    Output = "PDF",
                    ReportType = ""
                    
                };
            }

            Session["DRSCashFlowReportParam"] = model;            

            ViewBag.ReportName = "DRS Cash Flow";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("DRSCashFlow"))
                {
                    Session["ReportOutput"] = null;
                }
                
            }

            return View(model);

        }

        [HttpPost]
        public ActionResult DRSCashFlow(DRSCashFlowReportParam picker)
        {

            DRSCashFlowReportParam model = new DRSCashFlowReportParam
            {
                AsonDate = picker.AsonDate,               
                
                Output = picker.Output,
                ReportType = picker.ReportType               
                

            };

            ViewBag.Token = model;
            Session["DRSCashFlowReportParam"] = model;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.GenerateDRSCashFlowReport(model);

            return RedirectToAction("DRSCashFlow", "Reports");


        }
        #endregion

        #region "PDCRegistser"
        public ActionResult PDCRegister()
        {
            var banklist = (from c in db.AcHeads join g in db.AcGroups on c.AcGroupID equals g.AcGroupID where g.AcGroup1 == "Bank" select new { AcHeadID = c.AcHeadID, AcHead = c.AcHead1 }).ToList();
            ViewBag.BankList = banklist;
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            PDCRegister model = (PDCRegister)Session["PDCRegisterReportParam"];
            if (model == null)
            {
                model = new PDCRegister
                {
                    AsonDate = CommonFunctions.GetLastDayofMonth().Date, //.AddDays(-1);,                    
                    Output = "PDF",
                    ReportType = ""

                };
            }

            Session["PDCRegisterReportParam"] = model;

            ViewBag.ReportName = "PDC Register";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("PDCRegister"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }
        #endregion
        [HttpPost]
        public ActionResult PDCRegister(PDCRegister picker)
        {

            PDCRegister model = new PDCRegister
            {
                AsonDate = picker.AsonDate,
                AcHeadID = picker.AcHeadID,
                Output = picker.Output,
                ReportType = picker.ReportType,
                AllBank=picker.AllBank
                

            };

            ViewBag.Token = model;
            Session["PDCRegisterReportParam"] = model;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.GeneratePDCReport(model);

            return RedirectToAction("PDCRegister", "Reports");


        }


        #region "InvoicePendingRegister"
        public ActionResult InvoicePendingRegister()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            DateParam model = (DateParam)Session["InvoicePendingParam"];
            if (model == null)
            {
                model = new DateParam
                {
                    FromDate = CommonFunctions.GetFirstDayofMonth().Date, //.AddDays(-1);,
                    ToDate = CommonFunctions.GetLastDayofMonth().Date,
                    Output = "PDF",
                    ReportType = "ImportList"
                };
            }

            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }

            Session["InvoicePendingParam"] = model;

            //model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.ReportName = "Invoice Pending Register";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("InvoicePendingRegister"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }

        [HttpPost]
        public ActionResult InvoicePendingRegister(DateParam picker)
        {

            DateParam model = new DateParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                Output = picker.Output,
                ReportType = picker.ReportType
            };

            ViewBag.Token = model;
            Session["InvoicePendingParam"] = model;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.InvoicePendingrepot();

            return RedirectToAction("InvoicePendingRegister", "Reports");


        }
        #endregion


        #region "DeliveryPendingRegister"
        public ActionResult DeliveryPendingRegister()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            ViewBag.Deliverdby = db.EmployeeMasters.ToList().OrderBy(cc => cc.EmployeeName);
            DateParam model = (DateParam)Session["DeliveryPendingParam"];
            if (model == null)
            {
                model = new DateParam
                {
                    FromDate = CommonFunctions.GetFirstDayofMonth().Date, //.AddDays(-1);,
                    ToDate = CommonFunctions.GetLastDayofMonth().Date,
                    Output = "PDF",
                    ReportType = "PendingList",
                    SearchConsignor ="",
                    SearchCity ="",
                    CustomerId=0
                };
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }

            Session["DeliveryPendingParam"] = model;

            //model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.ReportName = "Delivery Pending Register";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("DeliveryPendingList"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }

        [HttpPost]
        public ActionResult DeliveryPendingRegister(DateParam picker)
        {

            DateParam model = new DateParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                Output = picker.Output,
                ReportType = picker.ReportType,
                DeliveredBy=picker.DeliveredBy,
                SearchConsignor=picker.SearchConsignor,
                CustomerId=picker.CustomerId,
                SearchCity =picker.SearchCity 
            };

            ViewBag.Token = model;
            Session["DeliveryPendingParam"] = model;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.DeliveryPendingRegister();

            return RedirectToAction("DeliveryPendingRegister", "Reports");


        }
        #endregion


        #region "OutScanPendingRegister"
        public ActionResult OutScanPendingRegister()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            ViewBag.Deliverdby = db.EmployeeMasters.ToList().OrderBy(cc => cc.EmployeeName);
            DateParam model = (DateParam)Session["OutScanPendingParam"];
            if (model == null)
            {
                model = new DateParam
                {
                    FromDate = CommonFunctions.GetFirstDayofMonth().Date, //.AddDays(-1);,
                    ToDate = CommonFunctions.GetLastDayofMonth().Date,
                    Output = "PDF",
                    ReportType = "PendingList",
                    SearchConsignor = "",
                    SearchCity = "",
                    CustomerId = 0
                };
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }

            Session["OutScanPendingParam"] = model;

            //model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.ReportName = "OutScan Pending Register";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("OutScanPendingList"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }

        [HttpPost]
        public ActionResult OutScanPendingRegister(DateParam picker)
        {

            DateParam model = new DateParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                Output = picker.Output,
                ReportType = picker.ReportType,
                DeliveredBy = picker.DeliveredBy,
                SearchConsignor = picker.SearchConsignor,
                CustomerId = picker.CustomerId,
                SearchCity = picker.SearchCity
            };

            ViewBag.Token = model;
            Session["OutScanPendingParam"] = model;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.OutScanPendingRegister();

            return RedirectToAction("OutScanPendingRegister", "Reports");


        }
        #endregion
        #region DRRAnlaysis
        public ActionResult DRRLedger()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            DateParam model = (DateParam)Session["DRRAnlaysisParam"];
            if (model == null)
            {
                model = new DateParam
                {
                    FromDate = CommonFunctions.GetLastDayofMonth().Date, //.AddDays(-1);,
                    ToDate = CommonFunctions.GetLastDayofMonth().Date,
                    Output = "EXCEL",
                    ReportType = "ImportList",
                    SearchConsignor = "",
                    SearchCity = ""
                };
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }

            Session["DRRAnlaysisParam"] = model;

            //model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.ReportName = "DRR Ledger";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("DRRLedger"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }
        [HttpPost]
        public ActionResult DRRLedger(DateParam picker)
        {

            DateParam model = new DateParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                Output = picker.Output,
                ReportType = picker.ReportType,
                SearchConsignor = picker.SearchConsignor,
                SearchCity = picker.SearchCity
            };

            ViewBag.Token = model;
            Session["DRRAnlaysisParam"] = model;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.DRRAnlaysisReport();

            return RedirectToAction("DRRLedger", "Reports");


        }
        #endregion

        #region DRRPending
        public ActionResult DRRPending()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            DateParam model = (DateParam)Session["DRRPendigParam"];
            if (model == null)
            {
                model = new DateParam
                {
                    FromDate = CommonFunctions.GetLastDayofMonth().Date, //.AddDays(-1);,
                    ToDate = CommonFunctions.GetLastDayofMonth().Date,
                    Output = "PDF",
                    ReportType = "ImportList",
                    SearchConsignor = "",
                    SearchCity = ""
                };
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }

            Session["DRRAnlaysisParam"] = model;

            //model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.ReportName = "DRR Pending";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("DRRPending"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }
        [HttpPost]
        public ActionResult DRRPending(DateParam picker)
        {

            DateParam model = new DateParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                Output = picker.Output,
                ReportType = picker.ReportType,
                SearchConsignor = picker.SearchConsignor,
                SearchCity = picker.SearchCity
            };

            ViewBag.Token = model;
            Session["DRRPendingParam"] = model;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.DRRPendingReport();

            return RedirectToAction("DRRPending", "Reports");


        }
        #endregion
        #region "OutScanList"
        public ActionResult OutScanList()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            ViewBag.Deliverdby = db.EmployeeMasters.ToList().OrderBy(cc => cc.EmployeeName);
            DateParam model = (DateParam)Session["OutScanListParam"];
            if (model == null)
            {
                model = new DateParam
                {
                    FromDate = AccountsDAO.CheckParamDate(CommonFunctions.GetLastDayofMonth().Date,yearid), //.AddDays(-1);,
                    ToDate = AccountsDAO.CheckParamDate(CommonFunctions.GetLastDayofMonth().Date,yearid),
                    Output = "EXCEL",
                    ReportType = "PendingList"                  
                  
                };
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }

            Session["OutScanListParam"] = model;

            //model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.ReportName = "OutScan List";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("OutScanList"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }

        [HttpPost]
        public ActionResult OutScanList(DateParam picker)
        {

            DateParam model = new DateParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                Output = picker.Output,
                ReportType = picker.ReportType,
                DeliveredBy = picker.DeliveredBy                                
                
            };

            ViewBag.Token = model;
            Session["OutScanListParam"] = model;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.OutScanList();

            return RedirectToAction("OutScanList", "Reports");


        }
        #endregion

        #region "DeliveryPendingRegister"
        public ActionResult SkylarkDeliveryRunSheet()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            ViewBag.Deliverdby = db.EmployeeMasters.ToList().OrderBy(cc => cc.EmployeeName);
            DateParam model = (DateParam)Session["DeliveryRunSheet"];
            if (model == null)
            {
                model = new DateParam
                {
                    FromDate = CommonFunctions.GetLastDayofMonth().Date, //.AddDays(-1);,
                    ToDate = CommonFunctions.GetLastDayofMonth().Date,
                    Output = "PDF",
                    ReportType = "PendingList",
                    SearchConsignor = "",
                    SearchCity = "",
                    CustomerId = 0
                };
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }

            Session["DeliveryRunSheet"] = model;

            //model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.ReportName = "Delivery Run Sheet Register";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("SkylarkDeliveryList"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }

        [HttpPost]
        public ActionResult SkylarkDeliveryRunSheet(DateParam picker)
        {

            DateParam model = new DateParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date,
                Output = picker.Output,
                ReportType = picker.ReportType,
                DeliveredBy = picker.DeliveredBy
                 
            };

            ViewBag.Token = model;
            Session["DeliveryRunSheet"] = model;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.DeliveryRunSheetRegister();

            return RedirectToAction("SkylarkDeliveryRunSheet", "Reports");


        }


        public ActionResult SkylarkCollectionRegister()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            ViewBag.Deliverdby = db.EmployeeMasters.ToList().OrderBy(cc => cc.EmployeeName);
            DateParam model = (DateParam)Session["SkylarkCollectionSheet"];
            if (model == null)
            {
                model = new DateParam
                {
                    FromDate = CommonFunctions.GetLastDayofMonth().Date, //.AddDays(-1);,
                    ToDate = CommonFunctions.GetLastDayofMonth().Date,
                    Output = "PDF",
                    ReportType = "PendingList",
                    SearchConsignor = "",
                    SearchCity = "",
                    CustomerId = 0
                };
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }

            Session["SkylarkCollectionSheet"] = model;

            //model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.ReportName = "Skylark Collection Register";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("SkylarkCollectionList"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }

        [HttpPost]
        public ActionResult SkylarkCollectionRegister(DateParam picker)
        {

            DateParam model = new DateParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date,
                Output = picker.Output,
                ReportType = picker.ReportType,
                DeliveredBy = picker.DeliveredBy

            };

            ViewBag.Token = model;
            Session["SkylarkCollectionSheet"] = model;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.SkylarkCollectionRegister();

            return RedirectToAction("SkylarkCollectionRegister", "Reports");


        }

        public ActionResult SkylarkReturnToConsignor()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            ViewBag.Deliverdby = db.EmployeeMasters.ToList().OrderBy(cc => cc.EmployeeName);
            DateParam model = (DateParam)Session["SkylarkReturnSheet"];
            if (model == null)
            {
                model = new DateParam
                {
                    FromDate = CommonFunctions.GetLastDayofMonth().Date, //.AddDays(-1);,
                    ToDate = CommonFunctions.GetLastDayofMonth().Date,
                    Output = "PDF",
                    ReportType = "PendingList",
                    SearchConsignor = "",
                    SearchCity = "",
                    CustomerId = 0
                };
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }

            Session["SkylarkReturnSheet"] = model;

            //model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.ReportName = "AWB Return to Consignor";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("AWBReturnConsignorList"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }

        [HttpPost]
        public ActionResult SkylarkReturnToConsignor(DateParam picker)
        {

            DateParam model = new DateParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date,
                Output = picker.Output,
                ReportType = picker.ReportType,
                DeliveredBy = picker.DeliveredBy

            };

            ViewBag.Token = model;
            Session["SkylarkReturnSheet"] = model;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.SkylarkReturntoConsignor();

            return RedirectToAction("SkylarkReturnToConsignor", "Reports");


        }
        #endregion



        #region AWBList
        [HttpGet]
        public ActionResult AWBList(int id = 0)
        {
            ViewBag.Movement = db.CourierMovements.ToList();
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            AWBListSearch obj = (AWBListSearch)Session["AWBListSearch"];
            AWBListSearch model = new AWBListSearch();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            ViewBag.Title = "AWB Register Export";
            if (obj == null)
            {
                DateTime pFromDate;
                DateTime pToDate;
                //int pStatusId = 0;
                pFromDate = CommonFunctions.GetFirstDayofMonth().Date;
                pToDate = CommonFunctions.GetLastDayofMonth().Date;
                pFromDate = AccountsDAO.CheckParamDate(pFromDate, yearid).Date;
                pToDate = AccountsDAO.CheckParamDate(pToDate, yearid).Date;
                obj = new AWBListSearch();
                obj.FromDate = pFromDate;
                obj.ToDate = pToDate;
                obj.StatusID = 0;
                obj.Destination = "";
                obj.MovementID = "1,2,3,4";
                obj.PaymentModeId = 0;
                obj.Origin = "";
                obj.Destination = "";
                obj.Customer = "";
                obj.CustomerId = 0;
                Session["AWBListSearch"] = obj;
                model.FromDate = pFromDate;
                model.ToDate = pToDate;
                model.StatusID = 0;
                model.Details = new List<QuickAWBVM>();
            }
            else
            {
                obj.MovementID = "";
                if (obj.SelectedValues != null)
                {
                    foreach (var item in obj.SelectedValues)
                    {
                        if (obj.MovementID == "")
                        {
                            obj.MovementID = item.ToString();
                        }
                        else
                        {
                            obj.MovementID = obj.MovementID + "," + item.ToString();
                        }

                    }
                }
                else if (obj.SelectedValues==null)
                {
                    obj.MovementID = "1,2,3,4";
                }
                model = obj;

                model.FromDate = AccountsDAO.CheckParamDate(obj.FromDate, yearid);
                model.ToDate = AccountsDAO.CheckParamDate(obj.ToDate, yearid);
                model.Details = new List<QuickAWBVM>();                
                Session["AWBListSearch"] = model;
            }

            ViewBag.CourierStatus = db.CourierStatus.Where(cc => cc.CourierStatusID >= 4).ToList();
            ViewBag.CourierStatusList = db.CourierStatus.Where(cc => cc.CourierStatusID >= 4).ToList();
            ViewBag.StatusTypeList = db.tblStatusTypes.ToList();
            ViewBag.CourierStatusId = 0;
            ViewBag.PageID = id.ToString();
            //ViewBag.StatusId = StatusId;    
            return View(model);

        }

        [HttpPost]
        public ActionResult AWBList(AWBListSearch obj)
        {
            Session["AWBListSearch"] = obj;
            obj.MovementID = "";
            if (obj.SelectedValues != null)
            {
                foreach (var item in obj.SelectedValues)
                {
                    if (obj.MovementID == "")
                    {
                        obj.MovementID = item.ToString();
                    }
                    else
                    {
                        obj.MovementID = obj.MovementID + "," + item.ToString();
                    }

                }
            }
            Session["AWBListSearch"] = obj;
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            DataTable dt = PickupRequestDAO.GetAWBListReportExcel(obj.StatusID, obj.FromDate, obj.ToDate, branchid, 1,"", obj.MovementID, obj.PaymentModeId, "", obj.Origin, obj.Destination);
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;
                string FileName = "AWBRegister_" + DateTime.Now.ToString("MMddyyyyHHMM") + ".xlsx";
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
            return RedirectToAction("AWBList");
        }
        #endregion

        #region "CustomerAWBList"
        public ActionResult CustomerAWBList()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            ViewBag.Deliverdby = db.EmployeeMasters.ToList().OrderBy(cc => cc.EmployeeName);
            DateParam model = (DateParam)Session["CustomerAWBListParam"];
            if (model == null)
            {
                model = new DateParam
                {
                    FromDate = CommonFunctions.GetFirstDayofMonth().Date, //.AddDays(-1);,
                    ToDate = CommonFunctions.GetLastDayofMonth().Date,
                    Output = "PDF",
                    ReportType = "PendingList"

                };
            }
            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
            }

            Session["CustomerAWBListParam"] = model;

            //model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.ReportName = "Customer AWB List";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("CustomerAWBList"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }

        [HttpPost]
        public ActionResult CustomerAWBList(DateParam picker)
        {

            DateParam model = new DateParam
            {
                FromDate = picker.FromDate,
                ToDate = picker.ToDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                Output = picker.Output,
                ReportType = picker.ReportType,
                SearchConsignor = picker.SearchConsignor,
                CustomerId=picker.CustomerId

            };

            ViewBag.Token = model;
            Session["CustomerAWBListParam"] = model;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.CustomerAWBList();

            return RedirectToAction("CustomerAWBList", "Reports");


        }
        #endregion

      
        public ActionResult CustomerListExcel()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            ViewBag.Deliverdby = db.EmployeeMasters.ToList().OrderBy(cc => cc.EmployeeName);
            CustomerLedgerReportParam model = (CustomerLedgerReportParam)Session["CustomerListParam"];
            if (model == null)
            {
                model = new CustomerLedgerReportParam
                {   
                     CustomerType="Customer"

                };
            }
            
            Session["CustomerListParam"] = model;

            
            ViewBag.ReportName = "Customer List";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("CustomerList"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }
        
        [HttpPost]
        public ActionResult CustomerListExcel(CustomerLedgerReportParam CustomerParam)
        {

            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            DataTable dt = PickupRequestDAO.GetCustomerListReportExcel(CustomerParam.CustomerType);
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;
                string FileName = "CustomerList_" + DateTime.Now.ToString("MMddyyyyHHMM") + ".xlsx";
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
                    //return File(MyMemoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", FileName);
                }
            }
            return RedirectToAction("CustomerListExcel", "Reports");

        }

        #region "customerlist"
        
        public ActionResult CustomerMasterList(AcInvoiceOpeningParam picker)
        {
            if (picker.CustomerType == null || picker.Output == null)
            {
                picker.CustomerType = "CR";
                picker.Output = "PDF";
            }

            AcInvoiceOpeningParam model = new AcInvoiceOpeningParam
            {
                CustomerId = picker.CustomerId,
                CustomerName = picker.CustomerName,
                Output = picker.Output,
                ReportType = picker.ReportType,
                CustomerType = picker.CustomerType
            };
                     

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.GenerateCustomerReport(model);

            return View(model);


        }
        #endregion

        #region "supplierList"
       
        public ActionResult SupplierMasterList(AcInvoiceOpeningParam picker)
        {
            var supplierMasterTypes = (from d in db.SupplierTypes select d).ToList();
            ViewBag.SupplierType = supplierMasterTypes;
            if (picker.SupplierTypeId == null ||picker.Output==null)
            {
                picker.SupplierTypeId = 1;
                picker.Output = "PDF";
            }
            AcInvoiceOpeningParam model = new AcInvoiceOpeningParam
            {
                SupplierId = picker.SupplierId,
                SupplierName = picker.SupplierName,
                Output = picker.Output,
                ReportType = picker.ReportType,
                SupplierTypeId = picker.SupplierTypeId
            };
 

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.GenerateSupplierMasterList(model);
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("SupplierList"))
                {
                    Session["ReportOutput"] = null;
                }

            }
            return View(model);


        }
        #endregion

        #region "OtherChargePrint"
        //public ActionResult AWBOtherChargePrint()
        //{
        //    int yearid = Convert.ToInt32(Session["fyearid"].ToString());

        //    AWBOtherChargeReportParam model = (AWBOtherChargeReportParam)Session["CustomerInvoicePrintParam"];

        //    if (model == null)
        //    {
        //        model = new AWBOtherChargeReportParam
        //        {
        //            FromDate = CommonFunctions.GetFirstDayofMonth().Date, //.AddDays(-1);,
        //            ToDate = CommonFunctions.GetLastDayofMonth().Date,
        //            AsonDate = CommonFunctions.GetFirstDayofMonth().Date, //.AddDays(-1);,
        //            CustomerId = 0,
        //            CustomerName = "",
        //            Output = "PDF",
        //            ReportType = "Ledger"
        //        };
        //    }
        //    if (model.FromDate.ToString() == "01-01-0001 00:00:00")
        //    {
        //        model.FromDate = CommonFunctions.GetFirstDayofMonth().Date;
        //    }

        //    if (model.ToDate.ToString() == "01-01-0001 00:00:00")
        //    {
        //        model.ToDate = CommonFunctions.GetLastDayofMonth().Date;
        //    }
        //    SessionDataModel.SetCustomerLedgerParam(model);

        //    model.FromDate = AccountsDAO.CheckParamDate(model.FromDate, yearid).Date;
        //    model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

        //    ViewBag.ReportName = "AWBOtherChargePrint";
        //    if (Session["ReportOutput"] != null)
        //    {
        //        string currentreport = Session["ReportOutput"].ToString();
        //        if (!currentreport.Contains("AWBOtherChargePrint"))
        //        {
        //            Session["ReportOutput"] = null;
        //        }

        //    }

        //    return View(model);

        //}
        
        public ActionResult AWBOtherChargePrint(AWBOtherChargeReportParam picker)
        {
            AWBOtherChargeReportParam model = new AWBOtherChargeReportParam();

            if (picker.FromDate.ToShortDateString().Contains("0001"))
            {
                model = new AWBOtherChargeReportParam
                {
                    FromDate = CommonFunctions.GetFirstDayofMonth().Date,
                    ToDate = CommonFunctions.GetLastDayofMonth().Date,
                    OtherChargeID = picker.OtherChargeID,
                    AWBNo = picker.AWBNo,
                    InvoiceNo = picker.InvoiceNo,
                    Output = "PDF",
                    ReportType = picker.ReportType

                };
            }
            else
            {
                model.FromDate = picker.FromDate;
                model.ToDate = picker.ToDate;
                model.OtherChargeID = picker.OtherChargeID;
                model.AWBNo = picker.AWBNo;
                model.InvoiceNo = picker.InvoiceNo;
                model.Output = picker.Output;
                model.ReportType = picker.ReportType;
            }
            

            if (model.InvoiceNo == null)
                model.InvoiceNo = "";
            if (model.AWBNo == null)
                model.AWBNo = "";


            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.AWBOtherChargeDetail(model);
            ViewBag.ReportName = "AWBOtherChargePrint";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("AWBOtherChargePrint"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);
            //return RedirectToAction("AWBOtherChargePrint", "Reports");


        }

        #endregion



        #region AWBStatusList
        [HttpGet]
        public ActionResult AWBStatusList(int id = 0)
        {
            ViewBag.Movement = db.CourierMovements.ToList();
            
            AWBListSearch obj = (AWBListSearch)Session["AWBStatusListSearch"];
            AWBListSearch model = new AWBListSearch();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            ViewBag.Title = "AWB Status Register Report";
            if (obj == null)
            {
                DateTime pFromDate;
                DateTime pToDate;
                //int pStatusId = 0;
                pFromDate = CommonFunctions.GetLastDayofMonth().Date; // DateTimeOffset.Now.Date;// CommonFunctions.GetFirstDayofMonth().Date; // DateTime.Now.Date; //.AddDays(-1) ; // FromDate = DateTime.Now;
                pToDate = CommonFunctions.GetLastDayofMonth().Date; // DateTime.Now.Date.AddDays(1); // // ToDate = DateTime.Now;

                obj = new AWBListSearch();
                obj.FromDate = pFromDate;
                obj.ToDate = pToDate;
                obj.StatusID = 0;
                obj.Destination = "";
                obj.MovementID = "1,2,3,4";
                obj.PaymentModeId = 0;
                obj.Origin = "";
                obj.Destination = "";
                obj.Customer = "";
                obj.CustomerId = 0;
                Session["AWBStatusListSearch"] = obj;
                model.FromDate = pFromDate;
                model.ToDate = pToDate;
                model.StatusID = 0;
                model.Details = new List<QuickAWBVM>();
            }
            else
            {
                obj.MovementID = "";
                if (obj.SelectedValues != null)
                {
                    foreach (var item in obj.SelectedValues)
                    {
                        if (obj.MovementID == "")
                        {
                            obj.MovementID = item.ToString();
                        }
                        else
                        {
                            obj.MovementID = obj.MovementID + "," + item.ToString();
                        }

                    }
                }
                else if (obj.SelectedValues == null)
                {
                    obj.MovementID = "1,2,3,4";
                }
                model = obj;
                model.Details = new List<QuickAWBVM>();
                Session["AWBListSearch"] = obj;
            }

            ViewBag.CourierStatus = db.CourierStatus.Where(cc => cc.CourierStatusID >= 4).ToList();
            ViewBag.CourierStatusList = db.CourierStatus.Where(cc => cc.CourierStatusID >= 4).ToList();
            ViewBag.StatusTypeList = db.tblStatusTypes.ToList();
            ViewBag.CourierStatusId = 0;
            ViewBag.PageID = id.ToString();
            //ViewBag.StatusId = StatusId;    
            return View(model);

        }

        [HttpPost]
        public ActionResult AWBStatusList(AWBListSearch obj)
        {
            Session["AWBListSearch"] = obj;
            obj.MovementID = "";
            if (obj.SelectedValues != null)
            {
                foreach (var item in obj.SelectedValues)
                {
                    if (obj.MovementID == "")
                    {
                        obj.MovementID = item.ToString();
                    }
                    else
                    {
                        obj.MovementID = obj.MovementID + "," + item.ToString();
                    }

                }
            }
            Session["AWBStatusListSearch"] = obj;
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            DataTable dt = PickupRequestDAO.GetAWBStatusListReportExcel(obj.StatusID, obj.FromDate, obj.ToDate, branchid, obj.MovementID,obj.Customer);
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;
                string FileName = "AWBStatusRegister_" + DateTime.Now.ToString("MMddyyyyHHMM") + ".xlsx";
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
            return RedirectToAction("AWBStatusList");
        }


        [HttpGet]
        public JsonResult GetCustomerName(string term)
        {
            bool enablecashcustomer = (bool)Session["EnableCashCustomerInvoice"];
            if (term.Trim() != "")
            {
                
                    var customerlist = (from c1 in db.CustomerMasters
                                        where c1.StatusActive == true && c1.CustomerID > 0 && c1.CustomerName.ToLower().StartsWith(term.ToLower())
                                        orderby c1.CustomerName ascending
                                        select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(25).ToList();

                    return Json(customerlist, JsonRequestBehavior.AllowGet);
                
            }
            else
            {
               

                    var customerlist = (from c1 in db.CustomerMasters
                                        where c1.CustomerID > 0  
                                        orderby c1.CustomerName ascending
                                        select new { CustomerID = c1.CustomerID, CustomerName = c1.CustomerName, CustomerType = c1.CustomerType }).Take(25).ToList();
                    return Json(customerlist, JsonRequestBehavior.AllowGet);
                 
            }




        }
        #endregion


        #region "CustomerAWBPrint"
        [HttpGet]
        public ActionResult CustomerAWBPrint()
        {
                        
            CustomerAWBPrintParam model = (CustomerAWBPrintParam)Session["CustomerAWBPrintParam"];
            if (model == null)
            {
                model = new CustomerAWBPrintParam
                {
                  
                    Output = "PDF"                    

                };
            }
            
            Session["CustomerAWBPrintParam"] = model;

          

            ViewBag.ReportName = "Customer AWB Print";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("CustomerAWBPrint"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }
        [HttpPost]
        public ActionResult CustomerAWBPrint(CustomerAWBPrintParam param)
        {

            if (param==null)
            {
                param = new CustomerAWBPrintParam();
                param.CustomerId = 0;
            }
            if (param.CustomerId > 0)
            {
                AccountsReportsDAO.CustomerAWBPrint(param.CustomerId,param.AWBCount);

            }

            Session["CustomerAWBPrintParam"] = param;
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("CustomerAWBPrint"))
                {
                    Session["ReportOutput"] = null;
                }

            }
            return RedirectToAction("CustomerAWBPrint", "Reports"); 


        }
        #endregion

        #region ImportRegisterExcel
        [HttpGet]
        public ActionResult ImportAWBList(int id = 0)
        {
            ViewBag.Movement = db.CourierMovements.ToList();
            ViewBag.PaymentMode = db.tblPaymentModes.ToList();
            AWBListSearch obj = (AWBListSearch)Session["ImportAWBListSearch"];
            AWBListSearch model = new AWBListSearch();
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            int depotId = Convert.ToInt32(Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());
            ViewBag.Title = "Import AWB Register";
            if (obj == null)
            {
                DateTime pFromDate;
                DateTime pToDate;
                //int pStatusId = 0;
                pFromDate = CommonFunctions.GetLastDayofMonth().Date; // DateTimeOffset.Now.Date;// CommonFunctions.GetFirstDayofMonth().Date; // DateTime.Now.Date; //.AddDays(-1) ; // FromDate = DateTime.Now;
                pToDate = CommonFunctions.GetLastDayofMonth().Date; // DateTime.Now.Date.AddDays(1); // // ToDate = DateTime.Now;

                obj = new AWBListSearch();
                obj.FromDate = pFromDate;
                obj.ToDate = pToDate;
                obj.StatusID = 0;
                obj.Destination = "";
                obj.MovementID = "3";
                obj.PaymentModeId = 0;
                obj.Origin = "";
                obj.Destination = "";
                obj.Customer = "";
                obj.CustomerId = 0;
                Session["ImportAWBListSearch"] = obj;
                model.FromDate = pFromDate;
                model.ToDate = pToDate;
                model.StatusID = 0;
                model.Details = new List<QuickAWBVM>();
            }
            else
            {
                obj.MovementID = "3";
         
                model = obj;
                model.Details = new List<QuickAWBVM>();
                Session["ImportAWBListSearch"] = obj;
            }

            ViewBag.CourierStatus = db.CourierStatus.Where(cc => cc.CourierStatusID >= 4).ToList();
            ViewBag.CourierStatusList = db.CourierStatus.Where(cc => cc.CourierStatusID >= 4).ToList();
            ViewBag.StatusTypeList = db.tblStatusTypes.ToList();
            ViewBag.CourierStatusId = 0;
            ViewBag.PageID = id.ToString();
            //ViewBag.StatusId = StatusId;    
            return View(model);

        }

        [HttpPost]
        public ActionResult ImportAWBList(AWBListSearch obj)
        {
            
            obj.MovementID = "3";
            Session["ImportAWBListSearch"] = obj;
            int branchid = Convert.ToInt32(Session["CurrentBranchID"].ToString());
            DataTable dt = PickupRequestDAO.GetImportAWBListReportExcel(obj.FromDate, obj.ToDate, branchid);
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;
                string FileName = "ImportAWBRegister_" + DateTime.Now.ToString("MMddyyyyHHMM") + ".xlsx";
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
            return RedirectToAction("ImportAWBList");
        }
        #endregion



        #region "CoLoaderInvoicePendingRegister"
        public ActionResult COLoaderInvoicePendingRegister()
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            DateParam model = (DateParam)Session["CLInvoicePendingParam"];
            if (model == null)
            {
                model = new DateParam
                {
                    FromDate = AccountsDAO.CheckParamDate(CommonFunctions.GetFirstDayofMonth().Date,yearid), //.AddDays(-1);,
                    ToDate = AccountsDAO.CheckParamDate(CommonFunctions.GetLastDayofMonth().Date,yearid),
                    Output = "PDF",
                    ReportType = "ImportList"
                };
            }

            if (model.FromDate.ToString() == "01-01-0001 00:00:00")
            {
                model.FromDate = AccountsDAO.CheckParamDate(CommonFunctions.GetFirstDayofMonth().Date, yearid);
            }

            if (model.ToDate.ToString() == "01-01-0001 00:00:00")
            {
                model.ToDate = AccountsDAO.CheckParamDate(CommonFunctions.GetLastDayofMonth().Date, yearid);
            }

            Session["CLInvoicePendingParam"] = model;

            //model.ToDate = AccountsDAO.CheckParamDate(model.ToDate, yearid).Date;

            ViewBag.ReportName = "Uninvoiced Coloaders Shipment";
            if (Session["ReportOutput"] != null)
            {
                string currentreport = Session["ReportOutput"].ToString();
                if (!currentreport.Contains("CoLoaderInvoicePendingRegister"))
                {
                    Session["ReportOutput"] = null;
                }

            }

            return View(model);

        }

        [HttpPost]
        public ActionResult COLoaderInvoicePendingRegister(DateParam picker)
        {
            int yearid = Convert.ToInt32(Session["fyearid"].ToString());

            DateParam model = new DateParam
            {
                FromDate = AccountsDAO.CheckParamDate(picker.FromDate,yearid),
                ToDate = AccountsDAO.CheckParamDate(picker.ToDate, yearid),
                Output = picker.Output,
                ReportType = picker.ReportType,
                CustomerId =picker.CustomerId,
                SearchConsignor =picker.SearchConsignor
            };

            ViewBag.Token = model;
            Session["CLInvoicePendingParam"] = model;
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            AccountsReportsDAO.CoLoaderInvoicePendingrepot();

            return RedirectToAction("COLoaderInvoicePendingRegister", "Reports");


        }
        #endregion
    }
}