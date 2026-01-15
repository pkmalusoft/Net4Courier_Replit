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
namespace Net4Courier.DAL
{
    public class CustomerOpeningDAO
    {

        public static List<CustomerOpeningVM> CustomerOpeningList(int FYearId, int BranchID, int CustomerID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetCustomerOpeningList";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@AcFinancialYearID", SqlDbType.Int);
            cmd.Parameters["@AcFinancialYearID"].Value = FYearId;

            cmd.Parameters.Add("@CustomerID", SqlDbType.Int);
            cmd.Parameters["@CustomerID"].Value = CustomerID;

            cmd.Parameters.Add("@BranchId", SqlDbType.Int);
            cmd.Parameters["@BranchId"].Value = BranchID;

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<CustomerOpeningVM> objList = new List<CustomerOpeningVM>();
            CustomerOpeningVM obj;

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new CustomerOpeningVM();
                    obj.AcOPInvoiceMasterId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ACOPInvoiceMasterID"].ToString());
                    obj.CustomerID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CustomerId"].ToString());
                    obj.AcJournalID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AcJournalID"].ToString());
                    obj.CustomerName = ds.Tables[0].Rows[i]["CustomerName"].ToString();                                        
                    obj.Debit = Convert.ToDecimal(ds.Tables[0].Rows[i]["Debit"].ToString());
                    obj.Credit = Convert.ToDecimal(ds.Tables[0].Rows[i]["Credit"].ToString());
                    obj.Balance = Convert.ToDecimal(ds.Tables[0].Rows[i]["Balance"].ToString());
                    obj.CustomerType = ds.Tables[0].Rows[i]["CustomerType"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }


        public static List<AcInvoiceOpeningDetailVM> CustomerOpeningDetail(int ACOpInvoiceMasterId)
        {


            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetCustomerOpeningDetail";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@AcOpInvoiceMasterID", SqlDbType.Int);
            cmd.Parameters["@AcOpInvoiceMasterID"].Value = ACOpInvoiceMasterId;
            
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<AcInvoiceOpeningDetailVM> objList = new List<AcInvoiceOpeningDetailVM>();
            AcInvoiceOpeningDetailVM obj;

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new AcInvoiceOpeningDetailVM();
                    obj.AcOPInvoiceMasterID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ACOPInvoiceMasterID"].ToString());
                    obj.AcOPInvoiceDetailID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AcOPInvoiceDetailID"].ToString());
                    obj.InvoiceDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["InvoiceDate"].ToString());
                    obj.InvoiceNo = ds.Tables[0].Rows[i]["InvoiceNo"].ToString();
                    obj.Debit = Convert.ToDecimal(ds.Tables[0].Rows[i]["Debit"].ToString());
                    obj.Credit = Convert.ToDecimal(ds.Tables[0].Rows[i]["Credit"].ToString());                    
                    objList.Add(obj);
                }
            }
            return objList;
        }


        
       
        public static List<SupplierOpeningVM> SupplierOpeningList(int FYearId, int BranchID, int SupplierID,int @SupplierTypeID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetSupplierOpeningList";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@AcFinancialYearID", SqlDbType.Int);
            cmd.Parameters["@AcFinancialYearID"].Value = FYearId;
            cmd.Parameters.Add("@BranchId", SqlDbType.Int);
            cmd.Parameters["@BranchId"].Value = BranchID;

            cmd.Parameters.Add("@SupplierTypeID", SqlDbType.Int);
            cmd.Parameters["@SupplierTypeID"].Value = SupplierTypeID;           
            

            cmd.Parameters.Add("@SupplierID", SqlDbType.Int);
            cmd.Parameters["@SupplierID"].Value = SupplierID;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<SupplierOpeningVM> objList = new List<SupplierOpeningVM>();
            SupplierOpeningVM obj;

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new SupplierOpeningVM();
                    obj.AcOPInvoiceMasterId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ACOPInvoiceMasterID"].ToString());
                    obj.SupplierID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["SupplierID"].ToString());
                    obj.AcJournalID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AcJournalID"].ToString());
                    obj.SupplierName = ds.Tables[0].Rows[i]["SupplierName"].ToString();
                    obj.Debit = Convert.ToDecimal(ds.Tables[0].Rows[i]["Debit"].ToString());
                    obj.Credit = Convert.ToDecimal(ds.Tables[0].Rows[i]["Credit"].ToString());
                    obj.Balance = Convert.ToDecimal(ds.Tables[0].Rows[i]["Balance"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static DataTable GetCustomerListinExcel()
        {
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_CoLoaderBatchReportExcel";
            cmd.CommandType = CommandType.StoredProcedure;

            //cmd.Parameters.AddWithValue("@BatchId", BatchID);


            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            return ds.Tables[0];


        }
    }
}