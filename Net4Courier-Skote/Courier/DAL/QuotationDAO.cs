using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Collections;
using Net4Courier.Models;
using System.Configuration;
 
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
 
 
using System.IO;
using System.Web.Hosting;
 
using System.Text;
namespace Net4Courier.DAL
{
    public class QuotationDAO
    {

        public static string GetMaxQuotationNo(int BranchId, int FyearId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetMaxQuotationNo";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@BranchId", BranchId);
            cmd.Parameters.AddWithValue("@FYearId", FyearId);
           

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return ds.Tables[0].Rows[0][0].ToString() + '-' + ds.Tables[0].Rows[0][1].ToString();
            }
            else
            {
                return "";
            }

        }


        public static List<QuotationVM> GetQuotationList(DateTime FromDate, DateTime ToDate, string InvoiceNo, int FyearId)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetQuotationList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@FYearId", FyearId);

            if (InvoiceNo == null)
                InvoiceNo = "";
            cmd.Parameters.AddWithValue("@QuotationNo", @InvoiceNo);

            cmd.Parameters.AddWithValue("@BranchID", branchid);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<QuotationVM> objList = new List<QuotationVM>();
            QuotationVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new QuotationVM();
                    obj.QuotationID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["QuotationID"].ToString());
                    obj.QuotationNo = ds.Tables[0].Rows[i]["QuotationNo"].ToString() + "/" + ds.Tables[0].Rows[i]["Version"].ToString();
                    obj.QuotationDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["QuotationDate"].ToString());
                    obj.CustomerName = ds.Tables[0].Rows[i]["CustomerName"].ToString();
                    obj.CustomerID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CustomerID"].ToString());
                    obj.QuotationValue = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["QuotationValue"].ToString());
                    
                    objList.Add(obj);
                }
            }
            return objList;
        }


        public static string QuotationReport(int QuotationID)
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
            comd.CommandText = "SP_QuotationPrint";
           
            comd.Parameters.AddWithValue("@QuotationID", QuotationID);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "QuotationPrint");

            //generate XSD to design report            
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "QuotationPrint.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "QuotationPrint.rpt"));

            rd.SetDataSource(ds);


            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
           // ShippingEntities db = new ShippingEntities();
            StringBuilder reportheader = new StringBuilder();

            //var branch = db.BranchMasters.Find(branchid);
            //reportheader.AppendLine(branch.Address1);
            //reportheader.AppendLine(branch.Address2);
            //reportheader.AppendLine(branch.Address3);
            //reportheader.AppendLine(branch.CityName);
            //reportheader.AppendLine(branch.CountryName);
            //reportheader.AppendLine(" Phone :" + branch.MobileNo1);
            string companyname = SourceMastersModel.GetCompanyname(branchid);
            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer            
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(reportheader.ToString());
            //   rd.ParameterFields["CompanyLocation"].CurrentValues.AddValue(companylocation);
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("Quotation");

            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue("");

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetCurrentDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);

            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();
            string reportname = "JobQuotationPrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            //reportparam.ReportFileName = reportname;
            rd.ExportToDisk(ExportFormatType.PortableDocFormat, reportpath);

            rd.Close();
            rd.Dispose();
            HttpContext.Current.Session["ReportOutput"] = "~/ReportsPDF/" + reportname;
            return reportpath;


        }
    }
}