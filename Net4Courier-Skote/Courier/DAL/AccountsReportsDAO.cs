using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Net4Courier.Models;
using System.Data.SqlClient;
using System.IO;
using System.Web.Hosting;
using CrystalDecisions.ReportAppServer.CommonObjectModel;
using BarcodeLib.Barcode.CrystalReports;
using BarcodeLib.Barcode;

namespace Net4Courier.DAL
{
    public class AccountsReportsDAO
    {
        public static string RateChartPrintReport(int id)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            AccountsReportParam reportparam = SessionDataModel.GetAccountsParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_RateChartPrint";
            comd.Parameters.AddWithValue("@CustomerRateID", id);
            

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "RateChartPrint");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "RateChartPrint.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "RateChartPrint.rpt"));

            rd.SetDataSource(ds);


            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            //   rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Rate Chart Print");                      
            
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue("");

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "RateChartPrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            //reportparam.ReportFileName = reportname;
            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);

            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        public static string GenerateLedgerReport(AccountsReportParam reportparam)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            reportparam = SessionDataModel.GetAccountsLedgerParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;

            Random rnd = new Random();
            int sessionid = rnd.Next();

            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "sp_accledger";
            comd.CommandTimeout = 200;
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@AcHeadId", reportparam.AcHeadId);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@YearId", yearid);
            if (reportparam.VoucherTypeId == null)
                reportparam.VoucherTypeId = "";
            comd.Parameters.AddWithValue("@VoucherType", reportparam.VoucherTypeId);

            comd.Parameters.AddWithValue("@CurrentPeriod", reportparam.CurrentPeriod);
            comd.Parameters.AddWithValue("@SessionID", sessionid);

            comd.Connection.Open();
            comd.ExecuteNonQuery();


            SqlCommand comd1 = new SqlCommand();
            comd1.CommandText = "Select  * From RptAccLedger Where SessionId=" + sessionid.ToString()  + " Order by Orderno, TransDate,voucherno, debit ";
            comd1.Connection = sqlConn;
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd1;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AccLedger");
            SqlCommand comd2 = new SqlCommand();
            comd2.CommandText = "Delete From RptAccLedger Where SessionId=" + sessionid.ToString();
            comd2.Connection = sqlConn;
            comd2.ExecuteNonQuery();

            

          

            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"),"AccLedger.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();           

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AccLedger.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetReportHeader2(branchid);
            string companyname = SourceMastersModel.GetReportHeader1(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["AccountHead"].CurrentValues.AddValue(reportparam.AcHeadName);
            string period = "Period From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "AccLedger_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
             //   SaveReportLog("General Ledger",  reportparam.AcHeadName + "(" + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy") + ")", "/ReportsPDF/"+ reportname, userid);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "AccLedger_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "AccLedger_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            reportparam.ReportFileName = reportname;
            SessionDataModel.SetAccountsParam(reportparam);
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }


        public static string GenerateTrialBalanceReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            AccountsReportParam1 reportparam = SessionDataModel.GetAccountsParam1();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            string paramdate = reportparam.AsOnDate.ToString("MM/dd/yyyy");
            paramdate = paramdate.Replace('-', '/');
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandTimeout = 0;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_AccTrialBalance";
            if (reportparam.isPeriod==true)
                comd.Parameters.AddWithValue("@AsOnDate", reportparam.ToDate.ToString("MM/dd/yyy"));
            else
                comd.Parameters.AddWithValue("@AsOnDate", reportparam.AsOnDate.ToString("MM/dd/yyy"));

            
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@YearId", yearid);


            if (reportparam.isPeriod == true)
                comd.Parameters.AddWithValue("@StartDate", reportparam.FromDate.ToString("MM/dd/yyy"));
            else
                comd.Parameters.AddWithValue("@StartDate", "");

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AccTrialBalance");

            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "AccTrialBalance.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AccTrialBalance.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetReportHeader2(branchid);
            string companyname = SourceMastersModel.GetReportHeader1(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            string reporttile = "Trial Balance";
            rd.ParameterFields["AccountHead"].CurrentValues.AddValue(reporttile);
            if (reportparam.isPeriod == false)
            {
                string period = "As on :" + reportparam.AsOnDate.Date.ToString("dd MMMM yyyy");
                rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);
            }
            else
            {
                string period = "From :" + reportparam.FromDate.Date.ToString("dd MMM yyyy") + " To " + reportparam.ToDate.Date.ToString("dd MMM yyyy");
                rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);
            }

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "AccTrialBal_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
                HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "AccTrialBal_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "AccTrialBal_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            reportparam.ReportFileName = reportname;
            SessionDataModel.SetAccountsParam1(reportparam);
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

        }

        public static string GenerateTrialBalanceReportV2()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            //AccountsReportParam1 reportparam = SessionDataModel.GetAccountsParam1();
            AccountsReportParam1 reportparam =(AccountsReportParam1)HttpContext.Current.Session["AccountsParamV2"];
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            string paramdate = reportparam.AsOnDate.ToString("MM/dd/yyyy");
            paramdate = paramdate.Replace('-', '/');
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandTimeout = 0;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_AccTrialBalanceV2";
            
             comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyy"));
            
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyy"));

            comd.Parameters.AddWithValue("@BranchId", branchid);
            

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AccTrialBalance");

            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "AccTrialBalanceV2.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AccTrialBalanceV2.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetReportHeader2(branchid);
            string companyname = SourceMastersModel.GetReportHeader1(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            string reporttile = "Trial Balance Current Period";
            rd.ParameterFields["AccountHead"].CurrentValues.AddValue(reporttile);
            if (reportparam.isPeriod == false)
            {
                string period = "For the Period " + reportparam.FromDate.Date.ToString("dd MMM yyyy") + " " +  reportparam.ToDate.Date.ToString("dd MMM yyyy"); 
                rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);
            }
            else
            {
                string period = "From :" + reportparam.FromDate.Date.ToString("dd MMM yyyy") + " To " + reportparam.ToDate.Date.ToString("dd MMM yyyy");
                rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);
            }

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "AccTrialBal_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
                HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "AccTrialBal_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "AccTrialBal_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            reportparam.ReportFileName = reportname;
            SessionDataModel.SetAccountsParam1(reportparam);
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

        }

        public static string GenerateTradingAccountReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            AccountsReportParam reportparam = SessionDataModel.GetAccountsParam2();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandTimeout = 0;
            comd.CommandText = "SP_AccTradingAccount";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@YearId", yearid);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AccTradingAccount");

            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "AccTradingAccount.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AccTradingAccount.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetReportHeader2(branchid);
            string companyname = SourceMastersModel.GetReportHeader1(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            string reporttile = "TRADING AND PROFIT & LOSS ACCOUNT";
            rd.ParameterFields["AccountHead"].CurrentValues.AddValue(reporttile);
            string period = "Period From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "AccTrading_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
                HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "AccTrading_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "AccTrading_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }

            rd.Close();
            rd.Dispose();
            reportparam.ReportFileName = reportname;
            SessionDataModel.SetAccountsParam2(reportparam);
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }


        public static string GenerateBalanceSheetReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            AccountsReportParam1 reportparam = SessionDataModel.GetAccountsParam1();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            string paramdate = reportparam.AsOnDate.ToString("MM/dd/yyyy");
            paramdate = paramdate.Replace('-', '/');
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandTimeout=0;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_BalanceSheet";
            comd.Parameters.AddWithValue("@AsOnDate", paramdate);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@YearId", yearid);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AccBalanceSheet");

            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "AccBalanceSheet.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AccBalanceSheet.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetReportHeader2(branchid);
            string companyname = SourceMastersModel.GetReportHeader1(branchid);

            // Assign the params collection to the report viewer
            //rd.ParameterFields[0].DefaultValues.AddValue(companyname);
            //rd.ParameterFields[0].CurrentValues.AddValue(companyname);

            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            string reporttile = "Balance Sheet";
            rd.ParameterFields["AccountHead"].CurrentValues.AddValue(reporttile);
            string period = "As on :" + reportparam.AsOnDate.Date.ToString("dd MMMM yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "AccBalanceSheet_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
                HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "AccBalanceSheet_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "AccBalanceSheet_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            reportparam.ReportFileName = reportname;
            SessionDataModel.SetAccountsParam1(reportparam);
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            rd.Close();
            rd.Dispose();
            return reportpath;

        }

        public static string GenerateHistoricalBalanceSheetReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            AccountsReportParam1 reportparam = SessionDataModel.GetAccountsParam1();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            string paramdate = reportparam.AsOnDate.ToString("MM/dd/yyyy");
            paramdate = paramdate.Replace('-', '/');
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_HistoricalBalanceSheet";
            comd.Parameters.AddWithValue("@AsOnDate", paramdate);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@YearId", yearid);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AccHistoricalBalanceSheet");

            // generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "AccHistoricalBalanceSheet.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AccHistoricalBalanceSheet.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetReportHeader2(branchid);
            string companyname = SourceMastersModel.GetReportHeader1(branchid);
            string AsOndate = paramdate.ToString();

            // Assign the params collection to the report viewer
            //rd.ParameterFields[0].DefaultValues.AddValue(companyname);
            //rd.ParameterFields[0].CurrentValues.AddValue(companyname);

            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);

            rd.ParameterFields["CurrentDate"].CurrentValues.AddValue(reportparam.AsOnDate.ToString("MMM yyyy"));
            rd.ParameterFields["PrevDate"].CurrentValues.AddValue(reportparam.AsOnDate.Date.AddMonths(-1).ToString("MMM yyyy"));

            string reporttile = "Balance Sheet";
            rd.ParameterFields["AccountHead"].CurrentValues.AddValue(reporttile);
            string period = "As on :" + reportparam.AsOnDate.Date.ToString("dd MMMM yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "AccHistoricalBalanceSheet_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
                HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "AccBalanceSheet_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "AccBalanceSheet_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            reportparam.ReportFileName = reportname;
            SessionDataModel.SetAccountsParam1(reportparam);
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            rd.Close();
            rd.Dispose();
            return reportpath;

        }
        public static string GenerateEmposFeeReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            AccountsReportParam reportparam = SessionDataModel.GetAccountsParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "sp_EmpostAnalysisReport";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@FYearId", yearid);
            
            SqlDataAdapter sqlAdapter = new SqlDataAdapter(); 
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "EmpostAnalysisReport");

            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "EmpostAnalysisReport.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "EmpostFeeReport.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetReportHeader2(branchid);
            string companyname = SourceMastersModel.GetReportHeader1(branchid);

            // Assign the params collection to the report viewer
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            //rd.ParameterFields[0].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Empost Fees Statement");
            string period = "For the Period From " + reportparam.FromDate.Date.ToString("dd MMM yyyy") + " to " + reportparam.ToDate.Date.ToString("dd MMM yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "EmpostFee_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "EmpostFee_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "EmpostFee_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        public static string GenerateBankReconcReport(AccountsReportParam reportparam)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;

            Random rnd = new Random();
            int sessionid = rnd.Next();

            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_BankReconcReport";
            comd.CommandTimeout = 200;
            comd.Parameters.AddWithValue("@AsonDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@BankLedgerAmount", reportparam.BankAmount);
            comd.Parameters.AddWithValue("@AcHeadId", reportparam.AcHeadId);
            comd.Parameters.AddWithValue("@FYearID", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);            
            
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AccLedger");



            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "BankReconcReport.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AccBankReconcReport.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetReportHeader2(branchid);
            string companyname = SourceMastersModel.GetReportHeader1(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["AccountHead"].CurrentValues.AddValue(reportparam.AcHeadName);
            string period = "Bank Reconciliation Report As on Date " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "AccBankReconcReport_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
                //   SaveReportLog("General Ledger",  reportparam.AcHeadName + "(" + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy") + ")", "/ReportsPDF/"+ reportname, userid);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "AccBankReconcReport_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "AccBankReconcReport_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            reportparam.ReportFileName = reportname;
            SessionDataModel.SetAccountsParam(reportparam);
            return reportpath;

           
        }
        public static decimal GetLedgerBalance(AccountsReportParam reportparam)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

  
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;

            Random rnd = new Random();
            int sessionid = rnd.Next();

            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "sp_accledger";
            comd.CommandTimeout = 200;
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@AcHeadId", reportparam.AcHeadId);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@YearId", yearid);
            if (reportparam.VoucherTypeId == null)
                reportparam.VoucherTypeId = "";
            comd.Parameters.AddWithValue("@VoucherType", reportparam.VoucherTypeId);

            comd.Parameters.AddWithValue("@CurrentPeriod", reportparam.CurrentPeriod);
            comd.Parameters.AddWithValue("@SessionID", sessionid);

            comd.Connection.Open();
            comd.ExecuteNonQuery();


            SqlCommand comd1 = new SqlCommand();
            comd1.CommandText = "Select Isnull(sum(Isnull(Debit,0))-Sum(Isnull(Credit,0)),0) as 'Amount' From RptAccLedger Where SessionId=" + sessionid.ToString();
            comd1.Connection = sqlConn;
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd1;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AccLedger");

            SqlCommand comd2 = new SqlCommand();
            comd2.CommandText = "Delete From RptAccLedger Where SessionId=" + sessionid.ToString();
            comd2.Connection = sqlConn;
            comd2.ExecuteNonQuery();


            decimal LedgerAmount = 0;
            if (ds.Tables.Count>0)
            {
                if (ds.Tables[0].Rows.Count>0)
                {
                    LedgerAmount = Convert.ToDecimal(ds.Tables[0].Rows[0][0].ToString());
                }    
            }

            return LedgerAmount;
            
          
        }
        public static string GenerateCustomerReceipt(int id)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            AccountsReportParam reportparam = SessionDataModel.GetAccountsParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_GetCustomerReceipt";
            comd.Parameters.AddWithValue("@Id", id);
            
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "CustomerReceipt");

            //generate XSD to design report
            System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CustomerReceipt.xsd"));
            ds.WriteXmlSchema(writer);
            writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CustomerReceipt.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetReportHeader2(branchid);
            string companyname = SourceMastersModel.GetReportHeader1(branchid);

            // Assign the params collection to the report viewer
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            //rd.ParameterFields[0].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("RECEIPT VOUCHER");
            //string period = "Period From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            //rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "CustomerReceipt_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            
             //reportparam.ReportFileName = reportname;
             rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            reportpath = "~/ReportsPDF/" + reportname;
            rd.Close();
            rd.Dispose();
            return reportname;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }


        public static string GenerateCOLoaderReceipt(int id)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            AccountsReportParam reportparam = SessionDataModel.GetAccountsParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_GetCOLoaderReceipt";
            comd.Parameters.AddWithValue("@Id", id);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "CustomerReceipt");

            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "COLoaderReceipt.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "COLoaderReceipt.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetReportHeader2(branchid);
            string companyname = SourceMastersModel.GetReportHeader1(branchid);

            // Assign the params collection to the report viewer
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            //rd.ParameterFields[0].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("RECEIPT VOUCHER");
            //string period = "Period From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            //rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "COLoaderReceipt_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);

            //reportparam.ReportFileName = reportname;
            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            reportpath = "~/ReportsPDF/" + reportname;
            rd.Close();
            rd.Dispose();
            return reportname;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }

        public static string GenerateSupplierPayment(int id)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            AccountsReportParam reportparam = SessionDataModel.GetAccountsParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_GetSupplierPayment";
            comd.Parameters.AddWithValue("@Id", id);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "SupplierPaymentVoucher");

            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "SupplierPaymentVoucher.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "SupplierPaymentPrint.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetReportHeader2(branchid);
            string companyname = SourceMastersModel.GetReportHeader1(branchid);

            // Assign the params collection to the report viewer
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            //rd.ParameterFields[0].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("PAYMENT VOUCHER");
            //string period = "Period From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            //rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "SupplierPaymentPrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);

            //reportparam.ReportFileName = reportname;
            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            reportpath = "~/ReportsPDF/" + reportname;
            rd.Close();
            rd.Dispose();
            return reportname;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        public static string GenerateDefaultReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();
            AccountsReportParam reportparam = SessionDataModel.GetAccountsParam();
                    
            //comd.CommandText = "up_GetAllCustomer"; comd.Parameters.Add("@Companyname", SqlDbType.VarChar, 50);
            //if (TextBox1.Text.Trim() != "")
            //    comd.Parameters[0].Value = TextBox1.Text;
            //else
            //    comd.Parameters[0].Value = DBNull.Value;
            //SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            //sqlAdapter.SelectCommand = comd;
            //DataSet ds = new DataSet();
            //sqlAdapter.Fill(ds, "AccLedger");

            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(Server.MapPath("~/Reports"),"AccLedger.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();           

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "DefaultReport.rpt"));

            //rd.SetDataSource(ds);


            string companyaddress = SourceMastersModel.GetReportHeader2(branchid);
            string companyname = SourceMastersModel.GetReportHeader1(branchid);

            // Assign the params collection to the report viewer
            //rd.ParameterFields[0].DefaultValues.AddValue(companyname);
            //rd.ParameterFields[0].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["AccountHead"].CurrentValues.AddValue("Default Report");
            string period = "Reprot Period as on Date "; // + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            //string reportname = "AccLedger_" + DateTime.Now.ToString("ddMMyyHHmm") + ".pdf";
            string reportname = "DefaultReport.pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);

            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            rd.Close();
            rd.Dispose();
            reportpath = "~/ReportsPDF/" + reportname;
            return reportpath;
            //Session["ReportOutput"] = "~/ReportsPDF/" + reportname;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //return stream;
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));
            //SaveStreamAsFile(reportpath, stream, reportname);
            //reportpath = Path.Combine(Server.MapPath("~/ReportsPDF"),reportname);            
            //return reportpath;
        }

        public static string GeneratePrepaidReceipt(int id)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            AccountsReportParam reportparam = SessionDataModel.GetAccountsParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_PrepaidAWBReceipt";
            comd.Parameters.AddWithValue("@PrepaidAWBId", id);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "PrepaidReceipt");

            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "PrepaidReceipt.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "PrepaidReceipt.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetReportHeader2(branchid);
            string companyname = SourceMastersModel.GetReportHeader1(branchid);

            // Assign the params collection to the report viewer
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            //rd.ParameterFields[0].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("RECEIPT VOUCHER");
            //string period = "Period From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            //rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "PrepaidReceipt_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);

            //reportparam.ReportFileName = reportname;
            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            reportpath = "~/ReportsPDF/" + reportname;
            rd.Close();
            rd.Dispose();
            return reportname;

          
        }
        public static string GenerateAWBRegister()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            AWBReportParam reportparam = SessionDataModel.GetAWBReportParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "sp_AWBRegisterReport";
            comd.CommandTimeout = 0;
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@FYearId",yearid);
            if (reportparam.PaymentModeId == null)
            {
                comd.Parameters.AddWithValue("@PaymentModeId", 0);
            }
            else
            {
                comd.Parameters.AddWithValue("@PaymentModeId", reportparam.PaymentModeId);
            }
            if (reportparam.ParcelTypeId == null)
            {
                comd.Parameters.AddWithValue("@ParcelTypeId", 0);
            }
            else
            {
                comd.Parameters.AddWithValue("@ParcelTypeId", reportparam.ParcelTypeId);
            }
            comd.Parameters.AddWithValue("@MovementId", reportparam.MovementId);

            comd.Parameters.AddWithValue("@SortBy", reportparam.SortBy);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AWBRegister");

            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "AWBRegister.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            if (reportparam.ReportType == "Date")
            {

                rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AWBRegister.rpt"));
            }
            else if (reportparam.ReportType == "Movement")
            {
                rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AWBRegister_MovementWise.rpt"));
            }
            else if (reportparam.ReportType == "PaymentMode")
            {
                rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AWBRegister_Payment.rpt"));
            }
            else if (reportparam.ReportType == "ParcelType")
            {
                rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AWBRegister_ParcelType.rpt"));
            }
            else if (reportparam.ReportType == "Courier")
            {
                rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AWBRegister_Summary.rpt"));
            }

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetReportHeader2(branchid);
            string companyname = SourceMastersModel.GetReportHeader1(branchid);

            // Assign the params collection to the report viewer
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            //rd.ParameterFields[0].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Airway Bill Register");
            string period = "For the Period From " + reportparam.FromDate.Date.ToString("dd MMM yyyy") + " to " + reportparam.ToDate.Date.ToString("dd MMM yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);


            rd.ParameterFields["GroupBy"].CurrentValues.AddValue(reportparam.ReportType);
            rd.ParameterFields["SortBy"].CurrentValues.AddValue(reportparam.SortBy);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            
            string reportname = "AWBRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "AWBRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "AWBRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }

        public static string GenerateAWBRegisterSummary()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            AWBReportParam reportparam = SessionDataModel.GetAWBReportParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "sp_AWBRegisterReportSummary";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@FYearId", yearid);
            if (reportparam.PaymentModeId == null)
            {
                comd.Parameters.AddWithValue("@PaymentModeId", 0);
            }
            else
            {
                comd.Parameters.AddWithValue("@PaymentModeId", reportparam.PaymentModeId);
            }
            if (reportparam.ParcelTypeId == null)
            {
                comd.Parameters.AddWithValue("@ParcelTypeId", 0);
            }
            else
            {
                comd.Parameters.AddWithValue("@ParcelTypeId", reportparam.ParcelTypeId);
            }
            comd.Parameters.AddWithValue("@MovementId", reportparam.MovementId);

            comd.Parameters.AddWithValue("@SortBy", reportparam.SortBy);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AWBRegisterSummary");

            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "AWBRegisterSummary.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AWBRegisterSummaryReport.rpt"));
            
            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetReportHeader2(branchid);
            string companyname = SourceMastersModel.GetReportHeader1(branchid);

            // Assign the params collection to the report viewer
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            //rd.ParameterFields[0].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Airway Bill Register Summary");
            string period = "For the Period From " + reportparam.FromDate.Date.ToString("dd MMM yyyy") + " to " + reportparam.ToDate.Date.ToString("dd MMM yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);


            rd.ParameterFields["GroupBy"].CurrentValues.AddValue(reportparam.ReportType);
            rd.ParameterFields["SortBy"].CurrentValues.AddValue(reportparam.SortBy);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();

            string reportname = "AWBRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "AWBRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "AWBRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }

        public static string GenerateTaxRegister()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            TaxReportParam reportparam = SessionDataModel.GetTaxReportParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "sp_TaxRegisterReport";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@FYearId", yearid);
            if (reportparam.TransactionType == null)
            {
                comd.Parameters.AddWithValue("@VoucherType", "All");
            }
            else
            {
                comd.Parameters.AddWithValue("@VoucherType", reportparam.TransactionType);
            }
            
            if (reportparam.SortBy == null)
                reportparam.SortBy = "All";
            comd.Parameters.AddWithValue("@SortBy", reportparam.SortBy);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "TaxRegister");

            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "TaxRegister.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "TaxRegister.rpt"));            

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetReportHeader2(branchid);
            string companyname = SourceMastersModel.GetReportHeader1(branchid);

            // Assign the params collection to the report viewer
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            //rd.ParameterFields[0].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Tax Register");
            string period = "For the Period From " + reportparam.FromDate.Date.ToString("dd MMM yyyy") + " to " + reportparam.ToDate.Date.ToString("dd MMM yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);


          //  rd.ParameterFields["GroupBy"].CurrentValues.AddValue(reportparam.ReportType);
           // rd.ParameterFields["SortBy"].CurrentValues.AddValue(reportparam.SortBy);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();

            string reportname = "TaxRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "TaxRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "TaxRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }


        public static string GenerateVatRegister(TaxReportParam reportparam)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();
                        
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "sp_VATRegisterReport";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@FYearId", yearid);
            if (reportparam.TransactionType == null)
            {
                comd.Parameters.AddWithValue("@VoucherType", "Summary");
            }
            else
            {
                comd.Parameters.AddWithValue("@VoucherType", reportparam.TransactionType);
            }

            if (reportparam.SortBy == null)
                reportparam.SortBy = "All";
            comd.Parameters.AddWithValue("@SortBy", reportparam.SortBy);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "VATRegister");

            ////generate XSD to design report
            //if (reportparam.TransactionType == "Detail")
            //{
            //    System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "VATRegister.xsd"));
            //    ds.WriteXmlSchema(writer);
            //    writer.Close();
            //}
            //else
            //{
            //    System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "VATRegisterSummary.xsd"));
            //    ds.WriteXmlSchema(writer);
            //    writer.Close();
            //}

            
            ReportDocument rd = new ReportDocument();
            if (reportparam.TransactionType == "Detail")
                rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "VATRegister.rpt"));
            else
                rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "VATRegisterSummary.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetReportHeader2(branchid);
            string companyname = SourceMastersModel.GetReportHeader1(branchid);

            // Assign the params collection to the report viewer
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            //rd.ParameterFields[0].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            if (reportparam.TransactionType == "Detail")
                rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("VAT Register Detail");
            else
                rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("VAT Register Summary");
            string period = "For the Period From " + reportparam.FromDate.Date.ToString("dd MMM yyyy") + " to " + reportparam.ToDate.Date.ToString("dd MMM yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);


            //  rd.ParameterFields["GroupBy"].CurrentValues.AddValue(reportparam.ReportType);
            // rd.ParameterFields["SortBy"].CurrentValues.AddValue(reportparam.SortBy);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();

            string reportname = "VATRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "VATRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "VATRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        public static string GenerateExportManifestRegister()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            ExportManifestReportParam reportparam = (ExportManifestReportParam)HttpContext.Current.Session["ExportManifestReportParam"]; 
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_ExportManifestRegister";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@FAgentId", reportparam.FAgentID);
            comd.Parameters.AddWithValue("@ManifestId", reportparam.ManifestId);
            
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "ExportManifestRegister");

            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "ExportManifestRegister.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();

            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "ExportManifestRegister.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetReportHeader2(branchid);
            string companyname = SourceMastersModel.GetReportHeader1(branchid);

            // Assign the params collection to the report viewer
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            //rd.ParameterFields[0].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Export Manifest Register");
            string period = "For the Period From " + reportparam.FromDate.Date.ToString("dd MMM yyyy") + " to " + reportparam.ToDate.Date.ToString("dd MMM yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);


            //  rd.ParameterFields["GroupBy"].CurrentValues.AddValue(reportparam.ReportType);
            // rd.ParameterFields["SortBy"].CurrentValues.AddValue(reportparam.SortBy);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();

            string reportname = "ExportManifestRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "ExportManifestRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "ExportManifestRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        public static string GenerateProfitLostReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            AccountsReportParam reportparam = SessionDataModel.GetAccountsParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "sp_accledger";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@AcHeadId", reportparam.AcHeadId);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@YearId", yearid);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AccLedger");

            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(Server.MapPath("~/Reports"),"AccLedger.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();           

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AccLedger.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetReportHeader2(branchid);
            string companyname = SourceMastersModel.GetReportHeader1(branchid);

            // Assign the params collection to the report viewer
            //rd.ParameterFields[0].DefaultValues.AddValue(companyname);
            //rd.ParameterFields[0].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["AccountHead"].CurrentValues.AddValue(reportparam.AcHeadName);
            string period = "Period From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "AccLedger_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "AccLedger_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "AccLedger_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }


        public static string GenerateInboundAWBReport(int id)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();


            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_InboundAWBPrintReport";
            comd.Parameters.AddWithValue("@InscanId", id);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AWBPrint");

            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "InboundAWBPrint.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();

            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "InboundAWBPrint.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);

            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("AWB PRINT");
            string period = "AWB Print";
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);


            //  rd.ParameterFields["GroupBy"].CurrentValues.AddValue(reportparam.ReportType);
            // rd.ParameterFields["SortBy"].CurrentValues.AddValue(reportparam.SortBy);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();

            string reportname = "AWBPrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);

            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }

        //awb print without barcode
        public static string GenerateAWBReport(int id)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_AWBPrintReport";
            comd.Parameters.AddWithValue("@InscanId", id);            

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AWBPrint");

            //generate XSD to design report
            System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "AWBPrint.xsd"));
            ds.WriteXmlSchema(writer);
            writer.Close();

            ReportDocument rd = new ReportDocument();

            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AWBPrint.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);

            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);            
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("AWB PRINT");
            string period = "AWB Print";
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);


            //  rd.ParameterFields["GroupBy"].CurrentValues.AddValue(reportparam.ReportType);
            // rd.ParameterFields["SortBy"].CurrentValues.AddValue(reportparam.SortBy);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();

            string reportname = "AWBPrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                    
            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }


        //awb print with barcode
        public static string GenerateAWBPrintLabelReport(int id)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();


            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_AWBPrintLabelReport";
            comd.Parameters.AddWithValue("@InscanId", id);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AWBPrint");

            //generate XSD to design report


            DataTable table1 = new DataTable();
            DataSet dsMain = new DataSet();
            table1 = ds.Tables[0].Copy();

            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "AWBPrintLabel.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            DataTable table2 = new DataTable();
            table2 = ds.Tables[1].Copy();
            table2.TableName = "AWBPrintPackingItem";
            DataSet ds2 = new DataSet();
            ds2.Tables.Add(table2);
            //generate XSD to design report
            //System.IO.StreamWriter writer2 = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "AWBPrintPackingItem.xsd"));
            //ds2.WriteXmlSchema(writer2);
            //writer2.Close();

        

            //add a new column named "Barcode" to the DataSet, the new column data type is byte[]
            //table1.Columns.Add(new DataColumn("Barcode", typeof(byte[])));
            // string barcode = "*" + table1.Rows[0]["AWBNo"].ToString() + "*";
            LinearCrystal barcode = new LinearCrystal();

            LinearCrystal awbbarcode = new LinearCrystal();
            //Barcode settings
            barcode.Type = BarcodeType.CODE128;
            //''barcode.BarHeight = 10 ''//50 pixel
            barcode.ImageFormat = System.Drawing.Imaging.ImageFormat.Png;
            barcode.ShowText = false;
            barcode.UOM = UnitOfMeasure.PIXEL;

            awbbarcode.Type = BarcodeType.CODE128;
            awbbarcode.ImageFormat = System.Drawing.Imaging.ImageFormat.Png;
            awbbarcode.ShowText = false;
            awbbarcode.UOM = UnitOfMeasure.PIXEL;
            //barcode.BarWidth = 100;
            //barcode.BarHeight = 50;
            //barcode.LeftMargin = 2;
            //barcode.RightMargin = 2;
            //barcode.TopMargin = 2;
            //barcode.BottomMargin = 2;
            //barcode.Rotate = RotateOrientation.BottomFacingRight;
            //int rowindex = 0;
            for (var rowindex = 0; rowindex < table1.Rows.Count; rowindex++)
            {
                barcode.Data = table1.Rows[rowindex]["AWBNo"].ToString();
                byte[] imageData = barcode.drawBarcodeAsBytes();
                table1.Rows[rowindex]["Barcode"] = imageData;

                awbbarcode.Data = table1.Rows[rowindex]["AWBNo"].ToString();
                byte[] awbimageData = awbbarcode.drawBarcodeAsBytes();
                table1.Rows[rowindex]["AWBBarcode"] = awbimageData;
            }


            dsMain.Tables.Add(table1);


            ReportDocument rd = new ReportDocument();

            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AWBPrintLabel.rpt"));

            rd.SetDataSource(dsMain);
           

            rd.Subreports[0].SetDataSource(ds2);


            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress1 = SourceMastersModel.GetCompanyAddress(branchid);
           // string companyaddress2 = SourceMastersModel.GetCompanyAddress2(branchid);
            //string companyaddress3 = SourceMastersModel.GetCompanyAddress3(branchid);
            //string companyname = SourceMastersModel.GetCompany1(branchid);
            //string companyPhone = SourceMastersModel.GetCompanyPhoneNo(branchid);
            //string companylocation = SourceMastersModel.GetCompanyLocation(branchid);
            //string CompanyCity = SourceMastersModel.GetCompanyCity(branchid);
            //string CompanyCountry = SourceMastersModel.GetCompanyCountry(branchid);

            // Assign the params collection to the report viewer
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue("");
            rd.ParameterFields["CompanyAddress1"].CurrentValues.AddValue(companyaddress1);
            rd.ParameterFields["CompanyAddress2"].CurrentValues.AddValue("");
            rd.ParameterFields["CompanyAddress3"].CurrentValues.AddValue("");
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue("");
            rd.ParameterFields["CompanyCity"].CurrentValues.AddValue("");
            rd.ParameterFields["CompanyCountry"].CurrentValues.AddValue("");
            rd.ParameterFields["CompanyPhone"].CurrentValues.AddValue("");

            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("AWB PRINT");
            string period = "AWB Print";
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);


            //  rd.ParameterFields["GroupBy"].CurrentValues.AddValue(reportparam.ReportType);
            // rd.ParameterFields["SortBy"].CurrentValues.AddValue(reportparam.SortBy);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();

            string reportname = "AWBPrintLabel_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);

            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        public static string GenerateAirbestAWBReport(int id)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();


            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_AWBPrintReport";
            comd.Parameters.AddWithValue("@InscanId", id);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AWBPrint");

            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "AWBPrint.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            //For subreport
            SqlCommand comd1;
            comd1 = new SqlCommand();
            comd1.Connection = sqlConn;
            comd1.CommandType = CommandType.StoredProcedure;
            comd1.CommandText = "SP_AWBPrintItem";
            comd1.Parameters.AddWithValue("@InScanID", id);

            SqlDataAdapter sqlAdapter1 = new SqlDataAdapter();
            sqlAdapter1.SelectCommand = comd1;
            DataSet ds1 = new DataSet();
            sqlAdapter1.Fill(ds1, "AWBPrintItem");
            
            DataTable table1 = new DataTable();
            table1 = ds1.Tables[0].Copy();
            

            DataTable table2 = new DataTable();
            table2 = ds1.Tables[1].Copy();
            table2.TableName = "AWBPrintItem";

            DataSet ds2 = new DataSet();
            ds2.Tables.Add(table1);

            DataSet ds3 = new DataSet();
            ds3.Tables.Add(table2);


            //generate XSD to design report
            //System.IO.StreamWriter writer1 = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "AWBPrintItem.xsd"));
            //ds1.WriteXmlSchema(writer1);
            //writer1.Close();

            ReportDocument rd = new ReportDocument();

            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AWBPrint.rpt"));

            rd.SetDataSource(ds);

            rd.Subreports[0].SetDataSource(ds2);

            rd.Subreports["Subreport2"].SetDataSource(ds3);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);

            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("AWB PRINT");
            string period = "AWB Print";
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);


            //  rd.ParameterFields["GroupBy"].CurrentValues.AddValue(reportparam.ReportType);
            // rd.ParameterFields["SortBy"].CurrentValues.AddValue(reportparam.SortBy);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();

            string reportname = "AWBPrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);

            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        public static string GenerateDayBookReport(AccountsReportParam reportparam)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

           // AccountsReportParam reportparam = SessionDataModel.GetAccountsParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_AccDayBook";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));          
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@YearId", yearid);
            comd.Parameters.AddWithValue("@VoucherType", reportparam.VoucherTypeId);
            comd.Parameters.AddWithValue("@Exceptional", reportparam.Exceptional);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AccDayBook");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "DayBook.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();           

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "DayBook.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer          
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Day Book");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "DayBook_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "AccLedger_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "AccLedger_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            reportparam.ReportFileName = reportname;
            SessionDataModel.SetAccountsParam(reportparam);
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }

        //not used
        public static string GenerateCustomerLedgerReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            CustomerLedgerReportParam reportparam = SessionDataModel.GetCustomerLedgerReportParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_CustomerLedger";            
            comd.Parameters.AddWithValue("@CustomerId", reportparam.CustomerId);
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));            
            comd.Parameters.AddWithValue("@FYearId", yearid);
            
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "CustomerLedger");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CustomerLedger.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();           

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CustomerLedger.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Customer Ledger");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "CustomerLedger_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "CustomerLedger_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "CustomerLedger_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }

        public static string GenerateCustomerLedgerDetailReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            CustomerLedgerReportParam reportparam = SessionDataModel.GetCustomerLedgerReportParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandTimeout = 50;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_CustomerLedgerDetail";
            comd.Parameters.AddWithValue("@CustomerId", reportparam.CustomerId);
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@CustomerType", reportparam.CustomerType);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "CustomerLedger");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/reportsxsd"), "CustomerLedger.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CustomerLedger.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Customer Ledger");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "CustomerLedger_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "CustomerLedger_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "CustomerLedger_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }

        public static string GenerateSalesRevenueSummaryReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            AccountsReportParam reportparam = (AccountsReportParam)HttpContext.Current.Session["SalesRevenueSummaryParam"];
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandTimeout = 50;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_CustomerSalesSummary";
    
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@Invoiced",reportparam.CurrentPeriod);
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
        

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "CustomerSalesSummary");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/reportsxsd"), "CustomerSalesSummary.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CustomerSalesSummary.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Customer Sales Summary");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "CustomerSalesSummary_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "CustomerSalesSummary_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "CustomerSalesSummary_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        public static string GenerateUnAllocatedInvoiceReceiptReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            CustomerLedgerReportParam reportparam = (CustomerLedgerReportParam)HttpContext.Current.Session["UnAllocatedInvoiceReceipt"];
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandTimeout = 50;
            comd.CommandType = CommandType.StoredProcedure;
            if (reportparam.ReportType =="Receipts")
                comd.CommandText = "SP_UnallocatedReceipt";
            else
                comd.CommandText = "SP_UnallocatedInvoice";
            


            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@CustomerType", reportparam.CustomerType);
            comd.Parameters.AddWithValue("@CustomerId", reportparam.CustomerId);
            

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "UnAllocatedReceipts");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/reportsxsd"), "UnAllocatedReceipts.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "UnAllocatedInvoiceReceipt.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            
            if (reportparam.ReportType == "Receipts")
                rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Un Allocated Customer Receipts");
            else
                rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Un Allocated Customer Invoice");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "UnAllocatedInvoiceReceipt_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "UnAllocatedInvoiceReceipt_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "UnAllocatedInvoiceReceipt_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        public static string GenerateCustomerOutStandingReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();
            
            Random rnd = new Random();
            int sessionid = rnd.Next();
            CustomerLedgerReportParam reportparam = SessionDataModel.GetCustomerLedgerReportParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandTimeout = 200;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_CustomerOutStanding";            
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@CustomerId", reportparam.CustomerId);
            comd.Parameters.AddWithValue("@BranchID", branchid);
            comd.Parameters.AddWithValue("@CustomerType", reportparam.CustomerType);
            comd.Parameters.AddWithValue("@SessionId", sessionid);
            comd.Connection.Open();
            comd.ExecuteNonQuery();

            SqlCommand comd1 = new SqlCommand();
            comd1.CommandText = "Select  * From RptCustomerOutstanding where SessionId=" +  sessionid.ToString() + " Order by CustomerName";
            comd1.Connection = sqlConn;

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd1;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "CustomerOutStanding");


            SqlCommand comd2 = new SqlCommand();
            comd2.CommandText = "Delete from RptCustomerOutstanding where SessionId=" + sessionid.ToString(); 
            comd2.Connection = sqlConn;
            comd2.ExecuteNonQuery();


            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CustomerOutStanding.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CustomerOutStanding.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Customer OutStanding Report");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "CustomerOutStanding_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;                
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
                string CustomerName = "All";
                if (reportparam.CustomerName != "" && reportparam.CustomerName!=null)
                    CustomerName = reportparam.CustomerName;
                  SaveReportLog("Customer Outstanding", CustomerName + "(" + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy") + ")", "/ReportsPDF/" + reportname, userid);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "CustomerOutStanding_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "CustomerOutStanding_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
               

        public static string GenerateAWBOutStandingReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int depotid = Convert.ToInt32(HttpContext.Current.Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            CustomerLedgerReportParam reportparam = SessionDataModel.GetCustomerLedgerReportParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_AWBOutStanding";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@CustomerId", reportparam.CustomerId);
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@DepotId", depotid);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AWBOutStanding");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "AWBOutStanding.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AWBOutStanding.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            if (reportparam.CustomerName!="" && reportparam.CustomerName!=null)
              rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("AWB OutStanding Report for Customer " + reportparam.CustomerName );
            else
                rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("AWB OutStanding Report for All Customer");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "AWBOutStanding_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "AWBOutStanding_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "AWBOutStanding_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        public static string GenerateAWBUnInvoiced()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int depotid = Convert.ToInt32(HttpContext.Current.Session["CurrentDepotID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            CustomerLedgerReportParam reportparam = SessionDataModel.GetCustomerLedgerReportParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_AWBOutStanding1";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@CustomerId", reportparam.CustomerId);
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@DepotId", depotid);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AWBUnInvoiced");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "AWBUnInvoiced.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AWBUnInvoiced.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            if (reportparam.CustomerName != "" && reportparam.CustomerName != null)
                rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("AWB UnInvoiced Report for Customer " + reportparam.CustomerName);
            else
                rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("AWB UnInvoiced Report for All Customer");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "AWBUnInvoiced_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "AWBUnInvoiced_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "AWBUnInvoiced_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        public static string GenerateCOLoaderAgingReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            CustomerLedgerReportParam reportparam = SessionDataModel.GetCustomerLedgerReportParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_CoLoaderAging";
            comd.Parameters.AddWithValue("@CustomerId", reportparam.CustomerId);
            comd.Parameters.AddWithValue("@AsonDate", reportparam.AsonDate.ToString("MM/dd/yyyy"));
            //comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchID", branchid);
            comd.Parameters.AddWithValue("@ReportOption", reportparam.ReportType);            
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "CustomerAging");

            //generate XSD to design report          
            //if (reportparam.ReportType == "Detail")
            //{
            //    System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CustomerAgingDetail.xsd"));

            //    ds.WriteXmlSchema(writer);
            //    writer.Close();
            //}
            //else
            //{
            //    System.IO.StreamWriter writer1 = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CustomerAgingSummary.xsd"));
            //    ds.WriteXmlSchema(writer1);
            //    writer1.Close();
            //}


            ReportDocument rd = new ReportDocument();
            if (reportparam.ReportType == "Detail")
                rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CustomerAgingDetail.rpt"));
            else
                rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CustomerAgingSummary.rpt"));
            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer
            //rd.ParameterFields[0].DefaultValues.AddValue(companyname);
            //rd.ParameterFields[0].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("CO-Loader Aging " + reportparam.ReportType);
            string period = " As on " + reportparam.AsonDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "CustomerAging_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "CustomerAging_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "CustomerAging_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        public static string GenerateCustomerAgingReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            CustomerLedgerReportParam reportparam = SessionDataModel.GetCustomerLedgerReportParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_CustomerAging";
            comd.Parameters.AddWithValue("@CustomerId", reportparam.CustomerId);
            comd.Parameters.AddWithValue("@AsonDate", reportparam.AsonDate.ToString("MM/dd/yyyy"));
            //comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchID", branchid);
            comd.Parameters.AddWithValue("@ReportOption", reportparam.ReportType);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "CustomerAging");

            //generate XSD to design report          
            //if (reportparam.ReportType == "Detail")
            //{
            //    System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CustomerAgingDetail.xsd"));

            //    ds.WriteXmlSchema(writer);
            //    writer.Close();
            //}
            //else
            //{
            //    System.IO.StreamWriter writer1 = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CustomerAgingSummary.xsd"));
            //    ds.WriteXmlSchema(writer1);
            //    writer1.Close();
            // }


                ReportDocument rd = new ReportDocument();
            if (reportparam.ReportType == "Detail")
                rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CustomerAgingDetail.rpt"));
            else
                rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CustomerAgingSummary.rpt"));
            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer
            //rd.ParameterFields[0].DefaultValues.AddValue(companyname);
            //rd.ParameterFields[0].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Customer Aging " + reportparam.ReportType);
            string period = " As on " + reportparam.AsonDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "CustomerAging_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "CustomerAging_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "CustomerAging_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }

        public static string GenerateSupplierLedgerDetailReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            SupplierLedgerReportParam reportparam = SessionDataModel.GetSupplierLedgerReportParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_SupplierLedgerDetail";
            comd.Parameters.AddWithValue("@SupplierId", reportparam.SupplierId);
            comd.Parameters.AddWithValue("@SupplierTypeId", reportparam.SupplierTypeId);            
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "SupplierLedgerDetail");

            //generate XSD  to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "SupplierLedger.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "SupplierLedger.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Supplier Ledger");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "SupplierLedger_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "SupplierLedger_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "SupplierLedger_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }

        public static string GenerateSupplierOutStandingReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            SupplierLedgerReportParam reportparam = SessionDataModel.GetSupplierLedgerReportParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_SupplierOutStanding";
            comd.Parameters.AddWithValue("@SupplierId", reportparam.SupplierId);
            comd.Parameters.AddWithValue("@SupplierTypeId", reportparam.SupplierTypeId);
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "SupplierOutStanding");

            //generate XSD  to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "SupplierOutStanding.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "SupplierOutStanding.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Supplier OutStanding");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "SupplierOutStanding_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "SupplierOutStanding_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "SupplierOutStanding_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        public static string GenerateSupplierStatementDetailReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            SupplierLedgerReportParam reportparam = SessionDataModel.GetSupplierLedgerReportParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_SupplierStatement";
            comd.Parameters.AddWithValue("@SupplierId", reportparam.SupplierId);
            comd.Parameters.AddWithValue("@SupplierTypeId", reportparam.SupplierTypeId);
            comd.Parameters.AddWithValue("@AsonDate", reportparam.AsonDate.ToString("MM/dd/yyyy"));
            //comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "SupplierLedgerDetail");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "SupplierStatement.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "SupplierStatement.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Supplier Statement");
            string period = "As on " + reportparam.AsonDate.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "SupplierStatement_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "SupplierStatement_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "SupplierStatement_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }


        public static string GenerateSupplierAgingReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            SupplierLedgerReportParam reportparam = SessionDataModel.GetSupplierLedgerReportParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_SupplierAging";
            comd.Parameters.AddWithValue("@SupplierId", reportparam.SupplierId);
            comd.Parameters.AddWithValue("@SupplierTypeId", reportparam.SupplierTypeId);            
            comd.Parameters.AddWithValue("@AsonDate", reportparam.AsonDate.ToString("MM/dd/yyyy"));
            //comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@ReportOption", reportparam.ReportType);
            comd.Parameters.AddWithValue("@BranchId", branchid);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "SupplierAging");

            //generate XSD to design report      
            if (reportparam.ReportType == "Detail")
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "SupplierAgingDetail.xsd"));
                ds.WriteXmlSchema(writer);
                writer.Close();
            }
            else
            {
                System.IO.StreamWriter writer1 = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "SupplierAgingSummary.xsd"));
                ds.WriteXmlSchema(writer1);
                writer1.Close();
            }

            ReportDocument rd = new ReportDocument();

            if (reportparam.ReportType == "Detail")
                rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "SupplierAgingDetail.rpt"));
            else
                rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "SupplierAgingSummary.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Supplier Aging " + reportparam.ReportType);
            string period = "As on " + reportparam.AsonDate.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "SupplierAging_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "SupplierAging_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "SupplierAging_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }

        public static string GenerateUnAllocatedSupplierInvoicePaymentReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

           SupplierLedgerReportParam reportparam = (SupplierLedgerReportParam)HttpContext.Current.Session["UnAllocatedInvoicePayment"];
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandTimeout = 50;
            comd.CommandType = CommandType.StoredProcedure;
            if (reportparam.ReportType == "Payments")
                comd.CommandText = "SP_UnallocatedPayment";
            else
                comd.CommandText = "SP_UnallocatedSupplierInvoice";



            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@SupplierTypeId", reportparam.SupplierTypeId);
            comd.Parameters.AddWithValue("@SupplierId", reportparam.SupplierId);

            
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "UnAllocatedPayments");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/reportsxsd"), "UnAllocatedPayments.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "UnAllocatedSupplierInvoice.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            if (reportparam.ReportType=="Payments")
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Un Allocated Supplier Payments");
            else
                rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Un Allocated Supplier Invoice");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "UnAllocatedSupplierInvoice_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "UnAllocatedSupplierInvoice_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "UnAllocatedSupplierInvoice_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        public static string GenerateCustomerStatementReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            CustomerLedgerReportParam reportparam =(CustomerLedgerReportParam)HttpContext.Current.Session["CustomerStatementParam"];// SessionDataModel.GetCustomerLedgerReportParam();

            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandTimeout = 0;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_CustomerStatement";
            comd.Parameters.AddWithValue("@CustomerId", reportparam.CustomerId);
            comd.Parameters.AddWithValue("@AsonDate", reportparam.AsonDate.ToString("MM/dd/yyyy"));
            //comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchID",branchid);
            comd.Parameters.AddWithValue("@CustomerType", reportparam.CustomerType);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "CustomerLedgerDetail");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CustomerStatement.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CustomerStatement.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Customer Statement");
            string period = " As on " + reportparam.AsonDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "CustomerStatement_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "CustomerStatement_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "CustomerStatement_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        public static string GenerateCOLoaderStatementReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            CustomerLedgerReportParam reportparam = (CustomerLedgerReportParam)HttpContext.Current.Session["CustomerStatementParam"];// SessionDataModel.GetCustomerLedgerReportParam();

            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_COLoaderStatement";
            comd.Parameters.AddWithValue("@CustomerId", reportparam.CustomerId);
            comd.Parameters.AddWithValue("@AsonDate", reportparam.AsonDate.ToString("MM/dd/yyyy"));
            //comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchID", branchid);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "CustomerLedgerDetail");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CustomerStatement.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CustomerStatement.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("CO-Loader Statement");
            string period = " As on " + reportparam.AsonDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "CustomerStatement_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "CustomerStatement_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "CustomerStatement_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        public static string GenerateCODRegister()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            CODReportParam reportparam = (CODReportParam)HttpContext.Current.Session["CODReportParam"];
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_CODRegisterReport";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@FYearId", yearid);
           
            if (reportparam.ParcelTypeId == null)
            {
                comd.Parameters.AddWithValue("@ParcelTypeId", 0);
            }
            else
            {
                comd.Parameters.AddWithValue("@ParcelTypeId", reportparam.ParcelTypeId);
            }
            comd.Parameters.AddWithValue("@MovementId", reportparam.MovementId);

            comd.Parameters.AddWithValue("@SortBy", reportparam.SortBy);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "CODRegister");

            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CODRegister.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CODRegister.rpt"));
            //if (reportparam.ReportType == "Date")
            //{
                            
            //}
            //else if (reportparam.ReportType == "Movement")
            //{
            //    rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CODRegister_MovementWise.rpt"));
            //}
            //else if (reportparam.ReportType == "PaymentMode")
            //{
            //    rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CODRegister_Payment.rpt"));
            //}
            //else if (reportparam.ReportType == "ParcelType")
            //{
            //    rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CODRegister_ParcelType.rpt"));
            //}
            //else if (reportparam.ReportType == "Summary")
            //{
            //    rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CODRegister_Summary.rpt"));
            //}

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetReportHeader2(branchid);
            string companyname = SourceMastersModel.GetReportHeader1(branchid);

            // Assign the params collection to the report viewer
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            //rd.ParameterFields[0].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("COD Register");
            string period = "For the Period From " + reportparam.FromDate.Date.ToString("dd MMM yyyy") + " to " + reportparam.ToDate.Date.ToString("dd MMM yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);


            rd.ParameterFields["GroupBy"].CurrentValues.AddValue(reportparam.ReportType);
            rd.ParameterFields["SortBy"].CurrentValues.AddValue(reportparam.SortBy);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();

            string reportname = "CODRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "CODRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "CODRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        public static string GenerateCODPending()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            CODReportParam reportparam = (CODReportParam)HttpContext.Current.Session["CODReportParam"];
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_CODRegisterPending";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@FYearId", yearid);

            if (reportparam.ParcelTypeId == null)
            {
                comd.Parameters.AddWithValue("@ParcelTypeId", 0);
            }
            else
            {
                comd.Parameters.AddWithValue("@ParcelTypeId", reportparam.ParcelTypeId);
            }
            comd.Parameters.AddWithValue("@MovementId", reportparam.MovementId);

            comd.Parameters.AddWithValue("@SortBy", reportparam.SortBy);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "CODPending");

            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CODPending.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CODPending.rpt"));
            //if (reportparam.ReportType == "Date")
            //{

            //}
            //else if (reportparam.ReportType == "Movement")
            //{
            //    rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CODRegister_MovementWise.rpt"));
            //}
            //else if (reportparam.ReportType == "PaymentMode")
            //{
            //    rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CODRegister_Payment.rpt"));
            //}
            //else if (reportparam.ReportType == "ParcelType")
            //{
            //    rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CODRegister_ParcelType.rpt"));
            //}
            //else if (reportparam.ReportType == "Summary")
            //{
            //    rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CODRegister_Summary.rpt"));
            //}

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetReportHeader2(branchid);
            string companyname = SourceMastersModel.GetReportHeader1(branchid);

            // Assign the params collection to the report viewer
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            //rd.ParameterFields[0].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("COD Pending");
            string period = "For the Period From " + reportparam.FromDate.Date.ToString("dd MMM yyyy") + " to " + reportparam.ToDate.Date.ToString("dd MMM yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);


            rd.ParameterFields["GroupBy"].CurrentValues.AddValue(reportparam.ReportType);
            rd.ParameterFields["SortBy"].CurrentValues.AddValue(reportparam.SortBy);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();

            string reportname = "CODPending_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "CODPending_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "CODPending_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        public static string GenerateCODSummary()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            CODReportParam reportparam = (CODReportParam)HttpContext.Current.Session["CODReportParam"];
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_CODRegisterSummary";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@FYearId", yearid);

            if (reportparam.ParcelTypeId == null)
            {
                comd.Parameters.AddWithValue("@ParcelTypeId", 0);
            }
            else
            {
                comd.Parameters.AddWithValue("@ParcelTypeId", reportparam.ParcelTypeId);
            }
            comd.Parameters.AddWithValue("@MovementId", reportparam.MovementId);

            comd.Parameters.AddWithValue("@SortBy", reportparam.SortBy);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "CODSummary");

            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CODSummary.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CODSummary.rpt"));
            //if (reportparam.ReportType == "Date")
            //{

            //}
            //else if (reportparam.ReportType == "Movement")
            //{
            //    rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CODRegister_MovementWise.rpt"));
            //}
            //else if (reportparam.ReportType == "PaymentMode")
            //{
            //    rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CODRegister_Payment.rpt"));
            //}
            //else if (reportparam.ReportType == "ParcelType")
            //{
            //    rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CODRegister_ParcelType.rpt"));
            //}
            //else if (reportparam.ReportType == "Summary")
            //{
            //    rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CODRegister_Summary.rpt"));
            //}

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetReportHeader2(branchid);
            string companyname = SourceMastersModel.GetReportHeader1(branchid);

            // Assign the params collection to the report viewer
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            //rd.ParameterFields[0].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("COD Register Summary");
            string period = "For the Period From " + reportparam.FromDate.Date.ToString("dd MMM yyyy") + " to " + reportparam.ToDate.Date.ToString("dd MMM yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);


            rd.ParameterFields["GroupBy"].CurrentValues.AddValue(reportparam.ReportType);
            rd.ParameterFields["SortBy"].CurrentValues.AddValue(reportparam.SortBy);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();

            string reportname = "CODSummary_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "CODSummary_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "CODSummary_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        #region "ReceiptVoucher"
        //Account Cash/Bank book receipt/payment voucher print        
        public static string GenerateReceiptPaymentVoucherPrint(int id)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_ReceiptPaymentVoucher";
            comd.Parameters.AddWithValue("@ID", id);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AcReceiptPaymentVoucherPrint");

            string PaymentType = "";
            string TransType = "";
            string Title = "";
            if (ds.Tables[0].Rows.Count > 0)
            {
                PaymentType = ds.Tables[0].Rows[0]["PaymentType"].ToString();
                TransType = ds.Tables[0].Rows[0]["TransType"].ToString();
            }
            if (PaymentType == "1" && TransType == "1")
                Title = "CASH RECEIPT VOUCHER";
            else if (PaymentType == "2" && TransType == "1")
                Title = "BANK RECEIPT VOUCHER";
            else if (PaymentType == "1" && TransType == "2")
                Title = "CASH PAYMENT VOUCHER";
            else if (PaymentType == "2" && TransType == "2")
                Title = "BANK PAYMENT VOUCHER";
 

            //generate XSD to design report            
            //if (PaymentType == "1") //--Cash
            //{
            //    System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "AcReceiptPaymentVoucherPrint.xsd"));
            //    ds.WriteXmlSchema(writer);
            //    writer.Close();
            //}
            //else //--Bank
            //{
            //    System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "BankReceiptPaymentVoucherPrint.xsd"));
            //    ds.WriteXmlSchema(writer);
            //    writer.Close();
            //}


            ReportDocument rd = new ReportDocument();

            if (PaymentType == "1") //--cash
                rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CashReceiptPaymentVoucherPrint.rpt"));
            else
                rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "BankReceiptPaymentVoucherPrint.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue(Title);
            //string period = "As on " + reportparam.FromDate.Date.ToString("dd-MM-yyyy"); // + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            //rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "AcReceiptPaymentVoucherPrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            rd.Close();
            rd.Dispose();
            reportpath = "~/ReportsPDF/" + reportname;
            rd.Close();
            rd.Dispose();
            return reportname;

        }

        public static string GenerateJournalVoucherPrint(int id)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_JournalVoucher";
            comd.Parameters.AddWithValue("@ID", id);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AcJournalVoucherPrint");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "AcJournalVoucherPrint.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AcJournalVoucherPrint.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("JOURNAL VOUCHER");
            //string period = "As on " + reportparam.FromDate.Date.ToString("dd-MM-yyyy"); // + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            //rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "AcJournalVoucherPrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportname;

        }


        public static string GenerateDebitNoteVoucherPrint(int id)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_DebitNoteVoucher";
            comd.Parameters.AddWithValue("@ID", id);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "DebitNoteVoucherPrint");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "DebitNoteVoucherPrint.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "DebitNoteVoucherPrint.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("DEBIT NOTE VOUCHER");
            //string period = "As on " + reportparam.FromDate.Date.ToString("dd-MM-yyyy"); // + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            //rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "AcJournalVoucherPrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportname;

        }

        public static string GenerateCreditNoteVoucherPrint(int id)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();
            
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_CreditNoteVoucher";
            comd.Parameters.AddWithValue("@ID", id);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "CreditNoteVoucherPrint");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CreditNoteVoucherPrint.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CreditNoteVoucherPrint.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("CREDIT NOTE VOUCHER");
            //string period = "As on " + reportparam.FromDate.Date.ToString("dd-MM-yyyy"); // + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            //rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "CreditNoteVoucherPrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportname;

        }

        #endregion

        #region "DRSRunSheet"
        public static string GenerateDRSRunSheet(int id, string output)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();


            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandTimeout = 0;
            comd.CommandText = "SP_DRSRunSheet";
            comd.Parameters.AddWithValue("@DRSID", id);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "DRSRunSheet");

            
            DataTable table1 = new DataTable();
            DataSet dsMain = new DataSet();
            table1 = ds.Tables[0].Copy();
            //generate XSD to design report

            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "DRSRunSheet.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            //add a new column named "Barcode" to the DataSet, the new column data type is byte[]
            //table1.Columns.Add(new DataColumn("Barcode", typeof(byte[])));
            // string barcode = "*" + table1.Rows[0]["AWBNo"].ToString() + "*";
            try
            {
                LinearCrystal barcode = new LinearCrystal();


                //Barcode settings
                barcode.Type = BarcodeType.CODE128;
                //''barcode.BarHeight = 10 ''//50 pixel
                barcode.ImageFormat = System.Drawing.Imaging.ImageFormat.Png;
                barcode.ShowText = false;
                barcode.UOM = UnitOfMeasure.PIXEL;
                //barcode.BarWidth = 100;
                //barcode.BarHeight = 50;
                //barcode.LeftMargin = 2;
                //barcode.RightMargin = 2;
                //barcode.TopMargin = 2;
                //barcode.BottomMargin = 2;
                //barcode.Rotate = RotateOrientation.BottomFacingRight;
                //int rowindex = 0;
                for (var rowindex = 0; rowindex < table1.Rows.Count; rowindex++)
                {
                    barcode.Data = table1.Rows[rowindex]["AWBNo"].ToString();// + "-" + table1.Rows[rowindex]["BoxNo"].ToString();
                    byte[] imageData = barcode.drawBarcodeAsBytes();
                    table1.Rows[rowindex]["Barcode"] = imageData;
                }

                dsMain.Tables.Add(table1);
            }
            catch(Exception ex)
            {

            }
            finally
            {                  
                try
                {
                    dsMain.Tables.Add(table1);
                }
                catch(Exception ex)
                {
                    
                }
            }
            

            ReportDocument rd = new ReportDocument();

            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "DRSRunSheet.rpt"));

            rd.SetDataSource(dsMain);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);

            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
          //  rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("DELIVERY RUN SHEET");
            string period = "";
            try
            {
                period=Convert.ToDateTime(ds.Tables[0].Rows[0]["DRSDate"]).ToString("dd MMM yyyy");
            }            
            catch(Exception ex1)
            {

            }
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);


            //  rd.ParameterFields["GroupBy"].CurrentValues.AddValue(reportparam.ReportType);
            // rd.ParameterFields["SortBy"].CurrentValues.AddValue(reportparam.SortBy);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();

            
            if (output == "PDF")
            {
                string reportname = "DRSRunSheet_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
                string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                //reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
                HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
                rd.Close();
                rd.Dispose();
                return reportpath;
            }
            else
            {
                string reportname1 = "DRSRunSheet_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                string reportpath1 = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname1);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath1);
                HttpContext.Current.Session["ExcelOutput"] = "~/ReportsPDF/" + reportname1;
                rd.Close();
                rd.Dispose();
                return reportpath1;
            }            
            
        }

       
        #endregion

        #region "MCPaymentPrintVoucher"
        public static string GenerateMCPaymentPrintVoucher(int id)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();


            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_MCPaymentVoucherPrint";
            comd.Parameters.AddWithValue("@RecPayId", id);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "MCPaymentVoucher");

            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "MCPaymentVoucher.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();

            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "MCPaymentVoucher.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);

            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            //  rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("MC PAYMENT VOUCHER");
            string period = "";           
            //rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);


            //  rd.ParameterFields["GroupBy"].CurrentValues.AddValue(reportparam.ReportType);
            // rd.ParameterFields["SortBy"].CurrentValues.AddValue(reportparam.SortBy);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();

            string reportname = "MCPaymentVoucher_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);

            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        #endregion

        #region "DRSReceiptVoucher"
        public static string GenerateDRSReceiptVoucher(int id)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();


            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_DRSReceiptVoucherPrint";
            comd.Parameters.AddWithValue("@RecPayId", id);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "DRSReceiptVoucher");

            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "DRSReceiptVoucher.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();

            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "DRSReceiptVoucher.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);

            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            //  rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("DRS RECEIPT VOUCHER");
            string period = "";
            //rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);


            //  rd.ParameterFields["GroupBy"].CurrentValues.AddValue(reportparam.ReportType);
            // rd.ParameterFields["SortBy"].CurrentValues.AddValue(reportparam.SortBy);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();

            string reportname = "DRSReceiptVoucher_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);

            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }

        #endregion

        public static string GenerateSalesReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            SalesReportParam reportparam = (SalesReportParam)(HttpContext.Current.Session["SalesReportParam"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_SalesRegister";
            comd.Parameters.AddWithValue("@CustomerId", reportparam.CustomerId);
            comd.Parameters.AddWithValue("@EmployeeId", reportparam.EmployeeID);
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@CustomerType",reportparam.CustomerType);
            comd.Parameters.AddWithValue("@PaymentModeId", reportparam.PaymentModeId);
            comd.Parameters.AddWithValue("@MovementId", reportparam.MovementId);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "SalesReport");

            string salesman = "All";
            string customername = "All";
            if (reportparam.EmployeeID > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    salesman = ds.Tables[0].Rows[0]["SalesMan"].ToString();
                }

            }

            if (reportparam.CustomerId > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    customername = ds.Tables[0].Rows[0]["CustomerName"].ToString();
                }

            }
            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "SalesReport.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "SalesReport.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["SalesMan"].CurrentValues.AddValue(salesman);
            rd.ParameterFields["CustomerName"].CurrentValues.AddValue(customername);

            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Sales Report");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "SalesReport_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "SalesReport_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "SalesReport_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        public static string GenerateSalesRegisterSummaryReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            SalesRegisterSummaryParam reportparam = (SalesRegisterSummaryParam)(HttpContext.Current.Session["SalesRegisterSummaryParam"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_SalesRegisterSummary";
            //comd.Parameters.AddWithValue("@CustomerId", reportparam.CustomerId);
            //comd.Parameters.AddWithValue("@EmployeeId", reportparam.EmployeeID);
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "SalesReport");

            string salesman = "All";
            string customername = "All";
            if (reportparam.EmployeeID > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    salesman = ds.Tables[0].Rows[0]["SalesMan"].ToString();
                }

            }

            if (reportparam.CustomerId > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    customername = ds.Tables[0].Rows[0]["CustomerName"].ToString();
                }

            }
            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "SalesRegisterSummary.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "SalesRegisterSummary.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["SalesMan"].CurrentValues.AddValue(salesman);
            rd.ParameterFields["CustomerName"].CurrentValues.AddValue(customername);

            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Sales Report");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "SalesRegisterSummary_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "SalesRegisterSummary_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "SalesRegisterSummary_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }

        public static string GenerateSalesRegisterCountryReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            SalesRegisterCountryParam reportparam = (SalesRegisterCountryParam)(HttpContext.Current.Session["SalesRegisterCountryParam"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_SalesRegisterCountry";
            //comd.Parameters.AddWithValue("@CustomerId", reportparam.CustomerId);
            //comd.Parameters.AddWithValue("@EmployeeId", reportparam.EmployeeID);
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "SalesRegisterCountry");
            
            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "SalesRegisterCountry.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "SalesRegisterCountry.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
           // rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);            

            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Sales Register Summary Country Wise");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "SalesRegisterCountry_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "SalesRegisterCountry_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "SalesRegisterCountry_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        
        public static string GenerateMaterialCostLedgerReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            MaterialCostLedgerParam reportparam = (MaterialCostLedgerParam)(HttpContext.Current.Session["MaterialCostLedgerParam"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_MaterialCostLedger";            
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            if (reportparam.Shipper == null)
                reportparam.Shipper = "";
            if (reportparam.Receiver == null)
                reportparam.Receiver = "";
            comd.Parameters.AddWithValue("@Shipper", reportparam.Shipper);
            comd.Parameters.AddWithValue("@Receiver", reportparam.Receiver);
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@Pending", reportparam.Pending);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "MaterialCostLedger");


            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "MaterialCostLedger.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "MaterialCostLedger.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            //rd.ParameterFields["SalesMan"].CurrentValues.AddValue(salesman);
            //rd.ParameterFields["CustomerName"].CurrentValues.AddValue(customername);

            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Material Cost Ledger Report");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "MaterialCostLedger_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "MaterialCostLedger_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "MaterialCostLedger_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

          
        }

        public static string GenerateMaterialCostPaymentList()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            MaterialCostLedgerParam reportparam = (MaterialCostLedgerParam)(HttpContext.Current.Session["MCPaymentListParam"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_MaterialCostPaymentList";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            if (reportparam.Shipper == null)
                reportparam.Shipper = "";
            if (reportparam.Receiver == null)
                reportparam.Receiver = "";
            comd.Parameters.AddWithValue("@Shipper", reportparam.Shipper);
            comd.Parameters.AddWithValue("@Receiver", reportparam.Receiver);
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
    
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "MaterialCostPaymentList");


            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "MaterialCostPaymentList.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "MaterialCostPaymentList.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            //rd.ParameterFields["SalesMan"].CurrentValues.AddValue(salesman);
            //rd.ParameterFields["CustomerName"].CurrentValues.AddValue(customername);

            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Material Cost Payment List");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "MaterialCostPaymentList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "MaterialCostPaymentList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "MaterialCostPaymentList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;


        }
        public static string GenerateCustomerInvoiceRegisterReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();
            string period = "";
            CustomerInvoiceReportParam reportparam = (CustomerInvoiceReportParam)(HttpContext.Current.Session["CustomerInvoiceRegisterParam"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_CustomerInvoiceRegister";

            comd.Parameters.AddWithValue("@CustomerId", reportparam.CustomerId);
            if (reportparam.FromDate==null)
            {
                comd.Parameters.AddWithValue("@FromDate", "");
                    }
            else
            {
                comd.Parameters.AddWithValue("@FromDate", Convert.ToDateTime(reportparam.FromDate).ToString("MM/dd/yyyy"));
            }
            if (reportparam.ToDate==null)
            {
                comd.Parameters.AddWithValue("@ToDate", "");
            }
            else
            {
                comd.Parameters.AddWithValue("@ToDate", Convert.ToDateTime(reportparam.ToDate).ToString("MM/dd/yyyy"));
                period = "From " + Convert.ToDateTime(reportparam.FromDate).Date.ToString("dd-MM-yyyy") + " to " +  Convert.ToDateTime(reportparam.ToDate).Date.ToString("dd-MM-yyyy");
            }
            if (reportparam.FromNo == null)
            {
                comd.Parameters.AddWithValue("@FromNo", "");
            }
            else
            {
                comd.Parameters.AddWithValue("@FromNo", reportparam.FromNo);
            }
            if (reportparam.ToNo == null)
            {
                comd.Parameters.AddWithValue("@ToNo", "");
            }
            else
            {
                comd.Parameters.AddWithValue("@ToNo", reportparam.ToNo);
                period = "From " + reportparam.FromNo + " to " + reportparam.ToNo;
            }
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchID",branchid);           
            
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "CustomerInvoiceRegister");


            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CustomerInvoiceRegister.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CustomerInvoiceRegister.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            //rd.ParameterFields["SalesMan"].CurrentValues.AddValue(salesman);
            //rd.ParameterFields["CustomerName"].CurrentValues.AddValue(customername);

            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Customer Invoice Register");
           
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "CustomerInvoiceRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "CustomerInvoiceRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "CustomerInvoiceRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;


        }


        public static string CustomerInvoiceMultiplePrint()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            CustomerLedgerReportParam reportparam = (CustomerLedgerReportParam)(HttpContext.Current.Session["CustomerInvoicePrintParam"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_CustomerInvoiceMultiplePrint";            
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@CustomerId", reportparam.CustomerId);
            comd.Parameters.AddWithValue("@InvoiceNos","");

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "CustomerInvoiceMultiple");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CustomerInvoiceMultiplePrint.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CustomerInvoiceMultiplePrint.rpt"));

            rd.SetDataSource(ds);


            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            //   rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("INVOICE");

            string totalwords = ""; // NumberToWords.ConvertAmount(Convert.ToDouble(ds.Tables[0].Rows[0]["InvoiceTotal"].ToString()), monetaryunit);

            //            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue("");

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            rd.ParameterFields["TotalWords"].CurrentValues.AddValue(totalwords);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "CustomerInvoiceMultiplePrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            //reportparam.ReportFileName = reportname;
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "CustomerInvoiceMultiplePrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "CustomerInvoiceMultiplePrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }

            //rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);

            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        #region OpeningRegister

        public string GenerateInvoicePending(InvoicePendingDateParam reportparam)
        {
            int companyId = Convert.ToInt32(HttpContext.Current.Session["CurrentCompanyID"].ToString());
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_GetInvoicePending";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@FromDate", reportparam.FromDate);
                        cmd.Parameters.AddWithValue("@ToDate", reportparam.ToDate);                        
                        cmd.Parameters.AddWithValue("@MovementId", "1,2,3");
                        cmd.Parameters.AddWithValue("@Companyid", companyId);
                        cmd.Parameters.AddWithValue("@FYearId", yearid);
                        cmd.Parameters.AddWithValue("@BranchId", branchid);
                        cmd.Connection = con;
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataSet ds = new DataSet();
                        da.Fill(ds);

                        //generate XSD to design report            
                        System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "GeneratedInvoicePending.xsd"));
                        ds.WriteXmlSchema(writer);
                        writer.Close();

                        ReportDocument rd = new ReportDocument();
                        rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "GenerateInvoicePending.rpt"));

                        rd.SetDataSource(ds);

                        //Set Paramerter Field Values -General
                        #region "param"
                        string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
                        string companyname = SourceMastersModel.GetCompanyname(branchid);
                        string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

                        // Assign the params collection to the report viewer            
                        rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
                        rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
                        rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
                        //rd.ParameterFields["SalesMan"].CurrentValues.AddValue(salesman);
                        //rd.ParameterFields["CustomerName"].CurrentValues.AddValue(customername);
                        //if (obj.CustomerType == "CR")
                            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Invoice Generation Pending");
                        //else
                        //    rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("CO-Loader Invoice Opening Register");
                        string period = "From Date " + Convert.ToDateTime(reportparam.FromDate).ToString("dd MMM yyyy") + " To " + Convert.ToDateTime(reportparam.ToDate).ToString("dd MMM yyyy");
                        rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

                        string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
                        rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
                        #endregion

                        //Response.Buffer = false;
                        //Response.ClearContent();
                        //Response.ClearHeaders();
                        string reportname = "CustomerInvoiceOpeningRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
                        string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                        if (reportparam.Output == "PDF")
                        {
                            reportparam.ReportFileName = reportname;
                            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
                        }
                        else if (reportparam.Output == "EXCEL")
                        {

                            reportname = "GenerateInvoicePending_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                            reportparam.ReportFileName = reportname;
                            reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                            rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
                        }
                        else if (reportparam.Output == "WORD")
                        {
                            reportname = "GenerateInvoiceOpeningRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                            reportparam.ReportFileName = reportname;
                            reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                            rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
                        }
                        rd.Close();
                        rd.Dispose();
                        HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
                        return reportpath;

                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "OK";

        }
        public static string GenerateCustomerReport(AcInvoiceOpeningParam reportparam)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();
            string yearfrom = HttpContext.Current.Session["FyearFrom"].ToString();
          ;
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_GetCustomerList";

            if (reportparam.CustomerType == null)
                 reportparam.CustomerType = "CR";

            comd.Parameters.AddWithValue("@CustomerType", reportparam.CustomerType);
            //comd.Parameters.AddWithValue("@PartyId", reportparam.CustomerId);
            //comd.Parameters.AddWithValue("@AcFinancialYearId", yearid);
            //comd.Parameters.AddWithValue("@BranchId", branchid);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "CustomerList");


            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CustomerList.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CustomerList.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            //rd.ParameterFields["SalesMan"].CurrentValues.AddValue(salesman);
            //rd.ParameterFields["CustomerName"].CurrentValues.AddValue(customername);
            if (reportparam.CustomerType == "CR")
                rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Customer List");
            else if (reportparam.CustomerType=="CL")
                rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("CO-Loader List");

            string period = "As on " + Convert.ToDateTime(yearfrom).ToString("dd MMM yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "CustomerList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "CustomerList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "CustomerList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;


        }

        public static string GenerateSupplierMasterList(AcInvoiceOpeningParam reportparam)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();
            string yearfrom = HttpContext.Current.Session["FyearFrom"].ToString();
        
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_SupplierList";

            if (reportparam.SupplierTypeId == 0)
                reportparam.SupplierTypeId = 1;
            comd.Parameters.AddWithValue("@SupplierTypeId", reportparam.SupplierTypeId);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "SupplierList");


            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "SupplierList.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "SupplierList.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            //rd.ParameterFields["SalesMan"].CurrentValues.AddValue(salesman);
            //rd.ParameterFields["CustomerName"].CurrentValues.AddValue(customername);

            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Supplier List");
            string period = "As on " + Convert.ToDateTime(yearfrom).ToString("dd MMM yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "SupplierList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "SupplierList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "SupplierList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;


        }
        public static string GenerateCustomerOpeningRegisterReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();
            string yearfrom = HttpContext.Current.Session["FyearFrom"].ToString();
            AcInvoiceOpeningParam reportparam = (AcInvoiceOpeningParam)(HttpContext.Current.Session["CustomerInvoiceOpeningRegisterParam"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_AcInvoiceOpeningReport";
            
            comd.Parameters.AddWithValue("@Type", reportparam.CustomerType);
            comd.Parameters.AddWithValue("@PartyId", reportparam.CustomerId);            
            comd.Parameters.AddWithValue("@AcFinancialYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "InvoiceOpeningRegister");


            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CustomerInvoiceOpeningRegister.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CustomerInvoiceOpeningRegister.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            //rd.ParameterFields["SalesMan"].CurrentValues.AddValue(salesman);
            //rd.ParameterFields["CustomerName"].CurrentValues.AddValue(customername);
            if (reportparam.CustomerType=="CR")
                rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Customer Invoice Opening Register");
            else
                rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("CO-Loader Invoice Opening Register");
            string period = "As on " + Convert.ToDateTime(yearfrom).ToString("dd MMM yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "CustomerInvoiceOpeningRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "CustomerInvoiceOpeningRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "CustomerInvoiceOpeningRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;


        }

        public static string GenerateSupplierOpeningRegisterReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();
            string yearfrom = HttpContext.Current.Session["FyearFrom"].ToString();
            AcInvoiceOpeningParam reportparam = (AcInvoiceOpeningParam)(HttpContext.Current.Session["SupplierInvoiceOpeningRegisterParam"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_AcInvoiceOpeningReport";

            comd.Parameters.AddWithValue("@Type", "S");
            comd.Parameters.AddWithValue("@PartyId", reportparam.SupplierId);
            comd.Parameters.AddWithValue("@AcFinancialYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@SupplierTypeId",reportparam.SupplierTypeId);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "SupplierInvoiceOpeningRegister");


            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "SupplierInvoiceOpeningRegister.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "SupplierInvoiceOpeningRegister.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            //rd.ParameterFields["SalesMan"].CurrentValues.AddValue(salesman);
            //rd.ParameterFields["CustomerName"].CurrentValues.AddValue(customername);

            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Supplier Invoice Opening Register");
            string period = "As on " + Convert.ToDateTime(yearfrom).ToString("dd MMM yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "SupplierInvoiceOpeningRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "SupplierInvoiceOpeningRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "SupplierInvoiceOpeningRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;


        }

        public static string GenerateAccountOpeningRegisterReport(string output)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();
            string yearfrom= HttpContext.Current.Session["FyearFrom"].ToString(); 
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_AcHeadOpeningRegister";
           
            comd.Parameters.AddWithValue("@AcFinancialYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AcOpeningRegister");


            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "AcOpeningRegister.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AcOpeningRegister.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            //rd.ParameterFields["SalesMan"].CurrentValues.AddValue(salesman);
            //rd.ParameterFields["CustomerName"].CurrentValues.AddValue(customername);

            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Account Opening Register");
            string period = "As on " + Convert.ToDateTime(yearfrom).ToString("dd MMM yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "AcOpeningRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (output == "PDF")
            {
               // reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (output == "EXCEL")
            {

                reportname = "AcOpeningRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
              //  reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (output == "WORD")
            {
                reportname = "AcOpeningRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
              //  reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;


        }
        #endregion

        public static string CustomerVATTaxInvoiceReport(int id, string InvoiceDetailIDs="")
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            AccountsReportParam reportparam = SessionDataModel.GetAccountsParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_VATTaxInvoiceReport";
            comd.Parameters.AddWithValue("@InvoiceId", id);
            comd.Parameters.AddWithValue("@InvoiceDetailIDs", InvoiceDetailIDs);            

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "TAXInvoiceReport");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "VATTaxInvoicePrint.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "VATTaxInvoicePrint.rpt"));

            rd.SetDataSource(ds);


            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            //   rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("INVOICE");

            //string totalwords = NumberToWords.ConvertAmount(Convert.ToDouble(ds.Tables[0].Rows[0]["InvoiceTotal"].ToString()), monetaryunit);

            //            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue("");

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime(); 
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            //rd.ParameterFields["TotalWords"].CurrentValues.AddValue(totalwords);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "VATTaxInvoicePrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            //reportparam.ReportFileName = reportname;
            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);

            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }

        public static string SkylarkVATTaxInvoiceReport(string AWBNo)
        {
            
            

            AccountsReportParam reportparam = SessionDataModel.GetAccountsParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "CMS_VATTaxInvoiceReport";
            comd.Parameters.AddWithValue("@AWBNo",AWBNo);
            

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "TAXInvoiceReport");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "VATTaxInvoicePrint.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "VATTaxInvoicePrint.rpt"));

            rd.SetDataSource(ds);


            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(1);
            string companyname = SourceMastersModel.GetCompanyname(1);
            string companylocation = SourceMastersModel.GetCompanyLocation(1);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            //   rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("INVOICE");

            //string totalwords = NumberToWords.ConvertAmount(Convert.ToDouble(ds.Tables[0].Rows[0]["InvoiceTotal"].ToString()), monetaryunit);

            //            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue("");

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(-1, "Employee") + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            //rd.ParameterFields["TotalWords"].CurrentValues.AddValue(totalwords);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "VATTaxInvoicePrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            //reportparam.ReportFileName = reportname;
            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);

            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        public static string CustomerInvoiceReport(int id, string monetaryunit)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            AccountsReportParam reportparam = SessionDataModel.GetAccountsParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_CustomerInvoiceReport";
            comd.Parameters.AddWithValue("@CustomerInvoiceId", id);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "CustomerInvoiceReport");
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CustomerInvoicePrint.rpt"));

            if (ds.Tables.Count > 1)
            {

                DataTable table1 = new DataTable();
                table1 = ds.Tables[0].Copy();
                DataSet ds1 = new DataSet();
                ds1.Tables.Add(table1);


                System.IO.StreamWriter writer1 = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CustomerInvoicePrint.xsd"));
                ds1.WriteXmlSchema(writer1);
                writer1.Close();

                DataTable table2 = new DataTable();
                table2 = ds.Tables[1].Copy();
                table2.TableName = "OtherChargeItem";
                DataSet ds2 = new DataSet();
                ds2.Tables.Add(table2);
                //generate XSD to design report
                //System.IO.StreamWriter writer2 = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "InvoiceOtherChargeItem.xsd"));
                //ds2.WriteXmlSchema(writer2);
                //writer2.Close();

                rd.SetDataSource(ds1);

                rd.Subreports[0].SetDataSource(ds2);

            }
            else
            {
                //generate XSD to design report            
                //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CustomerInvoicePrint.xsd"));
                //ds.WriteXmlSchema(writer);
                //writer.Close();
                rd.SetDataSource(ds);
            }

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            //   rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("INVOICE");

            string totalwords = NumberToWords.ConvertAmount(Convert.ToDouble(ds.Tables[0].Rows[0]["InvoiceTotal"].ToString()), monetaryunit);

            //            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue("");

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            rd.ParameterFields["TotalWords"].CurrentValues.AddValue(totalwords);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string InvoiceNo = ds.Tables[0].Rows[0]["CustomerInvoiceNo"].ToString();
            string reportname = "CustomerInvoice_" + InvoiceNo + "_" + DateTime.Now.ToString("ddMMyymmss") + ".pdf"; //HHmmss
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            //reportparam.ReportFileName = reportname;
            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);

            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        public static string CODInvoiceReport(int id, string monetaryunit)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            AccountsReportParam reportparam = SessionDataModel.GetAccountsParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_CODInvoiceReport";
            comd.Parameters.AddWithValue("@CODInvoiceId", id);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "SP_CODInvoiceReport");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CODInvoicePrint.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CODInvoicePrint.rpt"));

            rd.SetDataSource(ds);


            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            //   rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("INVOICE");

            string totalwords = NumberToWords.ConvertAmount(Convert.ToDouble(ds.Tables[0].Rows[0]["InvoiceTotal"].ToString()), monetaryunit);

            //            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue("");

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            rd.ParameterFields["TotalWords"].CurrentValues.AddValue(totalwords);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "CODInvoicePrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            //reportparam.ReportFileName = reportname;
            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);

            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }

        //export shipment print button report
        public static string ExportShipmentReport(int id)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            AccountsReportParam reportparam = SessionDataModel.GetAccountsParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_ExportShipmentReport";
            comd.Parameters.AddWithValue("@ExportID", id);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "ExportShipmentPrint");

            //generate XSD to design report            
            System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "ExportShipmentPrint.xsd"));
            ds.WriteXmlSchema(writer);
            writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "ExportShipmentPrint.rpt"));

            rd.SetDataSource(ds);


            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            //   rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Export Manifest Register");

            string totalwords = "";// NumberToWords.ConvertAmount(Convert.ToDouble(ds.Tables[0].Rows[0]["InvoiceTotal"].ToString()), monetaryunit);

            //            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue("");

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            rd.ParameterFields["TotalWords"].CurrentValues.AddValue(totalwords);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "ExportShipmentPrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            //reportparam.ReportFileName = reportname;
            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);

            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }

        public static DataTable ExportShipmentExcelReport(int id)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            AccountsReportParam reportparam = SessionDataModel.GetAccountsParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_ExportShipmentReportExcel";
            comd.Parameters.AddWithValue("@ExportID", id);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "ExportShipmentPrint");
            return ds.Tables[0];

            
           
        }
        //import shipment print button report
        public static string ImportShipmentReport(int id)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            AccountsReportParam reportparam = SessionDataModel.GetAccountsParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_ImportShipmentReport";
            comd.Parameters.AddWithValue("@ImportID", id);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "ImportShipmentPrint");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "ImportShipmentPrint.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "ImportShipmentPrint.rpt"));

            rd.SetDataSource(ds);


            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            //   rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("IMPORT SHIPMENT RECEIPT");

            string totalwords = "";// NumberToWords.ConvertAmount(Convert.ToDouble(ds.Tables[0].Rows[0]["InvoiceTotal"].ToString()), monetaryunit);

            //            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue("");

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            rd.ParameterFields["TotalWords"].CurrentValues.AddValue(totalwords);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "ImportShipmentPrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            //reportparam.ReportFileName = reportname;
            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);

            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        public static DataSet ImageTable(string ImageFile)
        {
            DataTable data = new DataTable();
            DataRow row;
            data.TableName = "Images";
            data.Columns.Add("img", System.Type.GetType("System.Byte[]"));
            FileStream fs = new FileStream(ImageFile, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            row = data.NewRow();
            row[0] = br.ReadBytes((int)br.BaseStream.Length);
            data.Rows.Add(row);
            br = null;
            fs.Close();
            fs = null;
            DataSet ds1 = new DataSet();
            ds1.Tables.Add(data);
            return ds1;
        }
        public static string COLoaderInvoiceReport(int id, string output)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            AccountsReportParam reportparam = SessionDataModel.GetAccountsParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_CoLoadInvoicePrint";
            comd.Parameters.AddWithValue("@AgentInvoiceId", id);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AgentInvoiceReport");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "AgentInvoicePrint.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            //Tried for Image
            //string strPath;
            //strPath = System.Web.HttpContext.Current.Request.MapPath("~/UploadFiles/defaultlogo.png");
            //DataSet ImageDataset = ImageTable(strPath);                       

            //System.IO.StreamWriter writer1 = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CompanyLogo.xsd"));
            //ImageDataset.WriteXmlSchema(writer1);
            //writer1.Close();

            //System.Drawing.Image drawingImage;
            //drawingImage = System.Drawing.Image.FromStream(new System.IO.MemoryStream());

            //drawingImage.Save(strPath, System.Drawing.Imaging.ImageFormat.Bmp);

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AgentInvoiceReport.rpt"));
            rd.SetDataSource(ds);


            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            //   rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("CO Loader Invoice");

           // string totalwords = NumberToWords.ConvertAmount(Convert.ToDouble(ds.Tables[0].Rows[0]["InvoiceTotal"].ToString()), monetaryunit);

            //            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue("");

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            //rd.ParameterFields["TotalWords"].CurrentValues.AddValue(totalwords);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();

            if (output == "PDF")
            {
                string reportname = "AgentInvoiceReport_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
                string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                //reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
                HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
                rd.Close();
                rd.Dispose();
                return reportpath;
            }
            else
            {
                string reportname1 = "AgentInvoiceReport_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                string reportpath1 = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname1);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath1);
                HttpContext.Current.Session["ExcelOutput"] = "~/ReportsPDF/" + reportname1;
                rd.Close();
                rd.Dispose();
                return reportpath1;
            }
             
        }

        public static string ImportListrepot()
        {                       
                int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
                int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
                int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
                string usertype = HttpContext.Current.Session["UserType"].ToString();

                DateParam reportparam = (DateParam)(HttpContext.Current.Session["ImportListParam"]);
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                SqlConnection sqlConn = new SqlConnection(strConnString);
                SqlCommand comd;
                comd = new SqlCommand();
                comd.Connection = sqlConn;
                comd.CommandType = CommandType.StoredProcedure;
                comd.CommandText = "SP_ImportShipmentList";                
                comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
                comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
                comd.Parameters.AddWithValue("@FYearId", yearid);
                comd.Parameters.AddWithValue("@BranchId", branchid);
                SqlDataAdapter sqlAdapter = new SqlDataAdapter();
                sqlAdapter.SelectCommand = comd;
                DataSet ds = new DataSet();
                sqlAdapter.Fill(ds, "ImportShipmentList");

            //generate XSD to design report            
            System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "ImportShipmentList.xsd"));
            ds.WriteXmlSchema(writer);
            writer.Close();

            ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "ImportShipmentList.rpt"));

                rd.SetDataSource(ds);

                //Set Paramerter Field Values -General
                #region "param"
                string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
                string companyname = SourceMastersModel.GetCompanyname(branchid);
                string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

                // Assign the params collection to the report viewer            
                rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
                rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
                // rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);            

                rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Import VAT Invoice List");
                string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
                rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

                string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
                rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
                #endregion

                //Response.Buffer = false;
                //Response.ClearContent();
                //Response.ClearHeaders();
                string reportname = "ImportShipmentList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
                string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                if (reportparam.Output == "PDF")
                {
                    reportparam.ReportFileName = reportname;
                    rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
                }
                else if (reportparam.Output == "EXCEL")
                {

                    reportname = "ImportShipmentList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                    reportparam.ReportFileName = reportname;
                    reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                    rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
                }
                else if (reportparam.Output == "WORD")
                {
                    reportname = "ImportShipmentList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                    reportparam.ReportFileName = reportname;
                    reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                    rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
                }
                rd.Close();
                rd.Dispose();
                HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
                return reportpath;

                //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                //stream.Seek(0, SeekOrigin.Begin);
                //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

                //return File(stream, "application/pdf", "AccLedger.pdf");
            
        }


        public static string CollectionReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            DateParam reportparam = (DateParam)(HttpContext.Current.Session["CollectionListParam"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_DailyCollectionReport";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "DailyCollectionReport");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "DailyCollectionReport.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "DailyCollectionReport.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            // rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);            

            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Daily Collection Report");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "DailyCollectionReport_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "DailyCollectionReport_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "DailyCollectionReport_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");

        }

        #region MCPaymentvoucherprint
        public static string MCPaymentVoucherReport(int id=0, int DocumentNo =0,int MSheet=0)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            AccountsReportParam reportparam = SessionDataModel.GetAccountsParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_MCPaymentVoucherReport";
            
            comd.Parameters.AddWithValue("@MCPaymentVoucherId", id);
            comd.Parameters.AddWithValue("@DocumentNo", DocumentNo);
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@MultipleSheet", MSheet);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "MCVoucherPaymentReport");
            
            DataTable table1 = new DataTable();
            table1 = ds.Tables[0].Copy();

            DataTable table2 = new DataTable();
            table2 = ds.Tables[1].Copy();
            table2.TableName = "MCPaymentVoucher_Sub1";

            DataSet ds1 = new DataSet();
            ds1.Tables.Add(table1);

            DataSet ds2 = new DataSet();
            ds2.Tables.Add(table2);

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "MCPaymentVoucher.xsd"));
            //ds1.WriteXmlSchema(writer);
            //writer.Close();

            //System.IO.StreamWriter writer1 = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "MCPaymentVoucher_Sub1.xsd"));
            //ds2.WriteXmlSchema(writer1);
            //writer1.Close();


            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "MCPaymentVoucher.rpt"));

            rd.SetDataSource(ds1);
            
            rd.Subreports["Subreport1"].SetDataSource(ds2);


            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            //   rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("PAYMENT VOUCHER");

            //string totalwords = NumberToWords.ConvertAmount(Convert.ToDouble(ds.Tables[0].Rows[0]["InvoiceTotal"].ToString()), monetaryunit);

            //            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
         //   rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue("");

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            //rd.ParameterFields["TotalWords"].CurrentValues.AddValue(totalwords);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "MCVoucherPaymentReport_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            //reportparam.ReportFileName = reportname;
            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);

            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            
        }

        public static string MCPaymentVoucherMultiplePrint(MCPaymentMultiplePrint obj)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            AccountsReportParam reportparam = SessionDataModel.GetAccountsParam();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_MCPaymentVoucherMultipleReport";

            comd.Parameters.AddWithValue("@MCPaymentVoucherId",obj.MCPaymentVoucherId);
            comd.Parameters.AddWithValue("@InscanIds",  obj.InscanID);
            comd.Parameters.AddWithValue("@MultipleSheet", obj.MultiplePrint);
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "MCVoucherPaymentReport");

            DataTable table1 = new DataTable();
            table1 = ds.Tables[0].Copy();

            DataTable table2 = new DataTable();
            table2 = ds.Tables[1].Copy();
            table2.TableName = "MCPaymentVoucher_Sub1";

            DataSet ds1 = new DataSet();
            ds1.Tables.Add(table1);

            DataSet ds2 = new DataSet();
            ds2.Tables.Add(table2);

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "MCPaymentVoucher.xsd"));
            //ds1.WriteXmlSchema(writer);
            //writer.Close();

            //System.IO.StreamWriter writer1 = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "MCPaymentVoucher_Sub1.xsd"));
            //ds2.WriteXmlSchema(writer1);
            //writer1.Close();


            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "MCPaymentVoucher.rpt"));

            rd.SetDataSource(ds1);

            rd.Subreports["Subreport1"].SetDataSource(ds2);


            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            //   rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("PAYMENT VOUCHER");

            //string totalwords = NumberToWords.ConvertAmount(Convert.ToDouble(ds.Tables[0].Rows[0]["InvoiceTotal"].ToString()), monetaryunit);

            //            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            //   rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue("");

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            //rd.ParameterFields["TotalWords"].CurrentValues.AddValue(totalwords);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "MCVoucherPaymentReport_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            //reportparam.ReportFileName = reportname;
            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);

            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;


        }
        #endregion

        #region DRSCashFlowReport
        public static string GenerateDRSCashFlowReport(DRSCashFlowReportParam reportparam)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_DRSCashflowReport";
            
            comd.Parameters.AddWithValue("@AsonDate", reportparam.AsonDate.ToString("MM/dd/yyyy"));            
            comd.Parameters.AddWithValue("@FYearid", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "DRSCashFlowReport");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/reportsxsd"), "DRSCashFlowReport.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "DRSCashFlowReport.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            //rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("DRS Cash Flow");
            string period = " As on " + reportparam.AsonDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "DRSCashFlowReport_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "DRSCashFlowReport_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "DRSCashFlowReport_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        #endregion

        #region "PDC Report"
        public static string GeneratePDCReport(PDCRegister reportparam)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();


            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_PDCRegister";

            comd.Parameters.AddWithValue("@AsonDate", reportparam.AsonDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@BankHeadID", reportparam.AcHeadID);
            comd.Parameters.AddWithValue("@FYearid", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "PDCRegister");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/reportsxsd"), "PDCRegister.xsd"));            
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "PDCRegister.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            //rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("PDC Register");
            string period = " As on " + reportparam.AsonDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "PDCRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "PDCRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "PDCRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        #endregion
        #region "DeliveryPendingRegister"
        public static string DeliveryPendingRegister()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            DateParam reportparam = (DateParam)(HttpContext.Current.Session["DeliveryPendingParam"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_DeliveryPendingList";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@CustomerId", reportparam.CustomerId);
            comd.Parameters.AddWithValue("@CityName", reportparam.SearchCity);
            comd.Parameters.AddWithValue("@DeliveredBy", reportparam.DeliveredBy);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "DeliveryPendingList");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "DeliveryPendingList.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "DeliveryPendingList.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            // rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);            

            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Delivery Pending Register");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "DeliveryPendingList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "DeliveryPendingList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "DeliveryPendingList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");

        }
        #endregion
        #region "DeliveryPendingRegister"
        public static string DeliveryRunSheetRegister()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            DateParam reportparam = (DateParam)(HttpContext.Current.Session["DeliveryRunSheet"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_SkylarkDeliveryList";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            //comd.Parameters.AddWithValue("@ToDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@EmployeeID", reportparam.DeliveredBy);
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
          
          
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "DeliveryList");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "SkylarkDeliveryList.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "SkylarkDeliveryList.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            // rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);            

            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Delivery Register");
            string period = "As on Date " + reportparam.FromDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "SkylarkDeliveryList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "SkylarkDeliveryList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "SkylarkDeliveryList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");

        }

        public static string SkylarkCollectionRegister()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            DateParam reportparam = (DateParam)(HttpContext.Current.Session["SkylarkCollectionSheet"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_SkylarkCollectionRegister";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            //comd.Parameters.AddWithValue("@ToDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@EmployeeID", reportparam.DeliveredBy);
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);


            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "SkylarkCollectionList");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "SkylarkCollectionList.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "SkylarkCollectionList.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            // rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);            

            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Skylark Collection Register");
            string period = "As on Date " + reportparam.FromDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "SkylarkCollectionList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "SkylarkCollectionList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "SkylarkCollectionList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");

        }

        public static string SkylarkReturntoConsignor()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            DateParam reportparam = (DateParam)(HttpContext.Current.Session["SkylarkReturnSheet"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "CMS_SkylarkReturntoCosignorList";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));            
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);


            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AWBReturnConsignorList");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "SkylarkReturnConsignorList.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AWBReturnConsignorList.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            // rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);            

            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Skylark Collection Register");
            string period = "As on Date " + reportparam.FromDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "AWBReturnConsignorList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "AWBReturnConsignorList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "AWBReturnConsignorList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");

        }
        #endregion


        #region "OutScanPendingRegister"
        public static string OutScanPendingRegister()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            DateParam reportparam = (DateParam)(HttpContext.Current.Session["OutScanPendingParam"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_OutScanPendingList";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@CustomerId", reportparam.CustomerId);
            comd.Parameters.AddWithValue("@CityName", reportparam.SearchCity);
            //comd.Parameters.AddWithValue("@DeliveredBy", reportparam.DeliveredBy);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "OutScanPendingList");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "OutScanPendingList.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "OutScanPendingList.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            // rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);            

            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("OutScan Pending Register");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "OutScanPendingList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "OutScanPendingList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "OutScanPendingList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");

        }
        #endregion
        #region "InvoicePending"
        public static string InvoicePendingrepot()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            DateParam reportparam = (DateParam)(HttpContext.Current.Session["InvoicePendingParam"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "sp_InvoicePendingRegister";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@FYearId", yearid);            

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "InvoicePendingRegister");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "InvoicePendingRegister.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "InvoicePendingRegister.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            // rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);            

            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("AWB DETAILS UNINVOICED");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "InvoicePendingRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "InvoicePendingRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")  
            {
                reportname = "InvoicePendingRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");

        }

        public static string CoLoaderInvoicePendingrepot()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            DateParam reportparam = (DateParam)(HttpContext.Current.Session["CLInvoicePendingParam"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "sp_COLoaderInvoicePendingRegister";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@CustomerId", reportparam.CustomerId);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "InvoicePendingRegister");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CoLoaderInvoicePendingRegister.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CoLoaderInvoicePendingRegister.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            // rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);            

            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Uninvoice Coloader Shipments");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "CoLoaderInvoicePendingRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "CoLoaderInvoicePendingRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "CoLoaderInvoicePendingRegister_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");

        }
        #endregion

        #region "DRRAnlaysis"
        public static string DRRAnlaysisReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            DateParam reportparam = (DateParam)(HttpContext.Current.Session["DRRAnlaysisParam"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_DRRLedger";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);            
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "DRRLedger");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "DRRLedger.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "DRRLedger.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            // rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);            

            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("DRR Ledger");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "DRRLedger_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "DRRLedger_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "DRRLedger_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");

        }

        public static string DRRPendingReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            DateParam reportparam = (DateParam)(HttpContext.Current.Session["DRRPendingParam"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_GetDRRPending";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "DRRLedger");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "DRRPending.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "DRRPending.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            // rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);            

            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("DRR Pending");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "DRRPending_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "DRRPending_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "DRRPending_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");

        }
        #endregion


        #region "OutScanList"
        public static string OutScanList()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            DateParam reportparam = (DateParam)(HttpContext.Current.Session["OutScanListParam"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_OutScanList";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);            
            comd.Parameters.AddWithValue("@DeliveredBy", reportparam.DeliveredBy);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "OutScanList");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "OutScanList.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "OutScanList.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            // rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);            

            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("OutScan List");
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "OutScanList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "OutScanList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "OutScanList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");

        }
        #endregion

        #region "CustomerAWBList"
        public static string CustomerAWBList()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            DateParam reportparam = (DateParam)(HttpContext.Current.Session["CustomerAWBListParam"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_CustomerAWBList";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@BranchId", branchid);
            comd.Parameters.AddWithValue("@FYearId", yearid);            
            comd.Parameters.AddWithValue("@CustomerName", reportparam.SearchConsignor);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "CustomerAWBList");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CustomerAWBList.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CustomerAWBList.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            // rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);            

            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("AWB List of Customer : " + reportparam.SearchConsignor);
            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "CustomerAWBList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "CustomerAWBList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "CustomerAWBList_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");

        }
        #endregion


        #region AWB Prepaid Issue
        public static string GeneratePrepaidAWBPrintReport(int id,string FromAWB,string ToAWB)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();


            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_PrepaidAWBIssuePrint";
            comd.Parameters.AddWithValue("@PrepaidAWBId", id);
            comd.Parameters.AddWithValue("@FromAWB", FromAWB);
            comd.Parameters.AddWithValue("@ToAWB", ToAWB);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "PrepaidAWBPrint");

            //generate XSD to design report
            System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "PrepaidAWBPrint.xsd"));
            ds.WriteXmlSchema(writer);
            writer.Close();

            ReportDocument rd = new ReportDocument();

            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "PrepaidAWBPrint.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
          
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);

            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("AWB PRINT");
            string period = "AWB Print";
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);



            string reportname = "PrepaidAWBPrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);

            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

           
        }
        #endregion


        #region "AWBOtherCharge"
        public static string AWBOtherChargeDetail(AWBOtherChargeReportParam reportparam)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

          //  AWBOtherChargeReportParam reportparam = (AWBOtherChargeReportParam)(HttpContext.Current.Session["AWBOtherChargeReportParam"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_CustomerInvoiceOtherChargeDetail";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));

            comd.Parameters.AddWithValue("@AWBNo", reportparam.AWBNo);
            comd.Parameters.AddWithValue("@InvoiceNo", reportparam.InvoiceNo);
            comd.Parameters.AddWithValue("@OtherChargeID", reportparam.OtherChargeID);
            

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AWBOtherChargeDetail");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "AWBOtherChargeDetail.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AWBOtherChargeDetail.rpt"));

            rd.SetDataSource(ds);


            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            //   rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("INVOICE");

            string totalwords = ""; // NumberToWords.ConvertAmount(Convert.ToDouble(ds.Tables[0].Rows[0]["InvoiceTotal"].ToString()), monetaryunit);

            //            string period = "From " + reportparam.FromDate.Date.ToString("dd-MM-yyyy") + " to " + reportparam.ToDate.Date.ToString("dd-MM-yyyy");
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue("");

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);
          
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "AWBOtherChargePrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            //reportparam.ReportFileName = reportname;
            if (reportparam.Output == "PDF")
            {
                reportparam.ReportFileName = reportname;
                rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            }
            else if (reportparam.Output == "EXCEL")
            {

                reportname = "AWBOtherChargePrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".xlsx";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.ExcelWorkbook, reportpath);
            }
            else if (reportparam.Output == "WORD")
            {
                reportname = "AWBOtherChargePrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".doc";
                reportparam.ReportFileName = reportname;
                reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
                rd.ExportToDisk(ExportFormatType.WordForWindows, reportpath);
            }

            //rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);

            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");
        }
        #endregion


        #region reportlog
        //SP_SaveReportLog
        public static int SaveReportLog(string ReportName, string Description, string ReportPath, int userid)
        {
            int iReturn = 0;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_SaveReportLog";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@ReportName", ReportName);
            cmd.Parameters.AddWithValue("@ReportDescription", Description);
            cmd.Parameters.AddWithValue("@ReportPath", ReportPath);
            cmd.Parameters.AddWithValue("@CreatedBy", userid);
            cmd.Parameters.AddWithValue("@CreatedDate", CommonFunctions.GetCurrentDateTime());

            try
            {
                cmd.Connection.Open();
                iReturn = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {

            }

            return iReturn;
        }
        #endregion


        public static string CustomerAWBPrint(int CustomerId,int AWBCount)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            DateParam reportparam = (DateParam)(HttpContext.Current.Session["CustomerAWBListParam"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_CustomerAWBPrint";
             
            comd.Parameters.AddWithValue("@CustomerId",CustomerId);
            comd.Parameters.AddWithValue("@AWBCount",AWBCount);
          
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "CustomerAWBPrint");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "CustomerAWBPrint.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "CustomerAWBPrint.rpt"));

            rd.SetDataSource(ds);

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
            // rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);            

          

            
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "CustomerAWBPrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
             
              //  reportparam.ReportFileName = reportname;
               rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);
            
           
            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;

            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.Write(Path.Combine(Server.MapPath("~/Reports"), "AccLedger.pdf"));

            //return File(stream, "application/pdf", "AccLedger.pdf");

        }
    }
}