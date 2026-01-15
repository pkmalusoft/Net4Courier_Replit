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
    public class CreditNoteDAO
    {

        public static List<CreditNoteVM> CustomerJVList(int FYearId, int BranchID, DateTime FromDate, DateTime ToDate, string CreditNoteNo, string InvoiceNo)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "CustomerJVList";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@FYearID", SqlDbType.Int);
            cmd.Parameters["@FYearID"].Value = FYearId;

            cmd.Parameters.Add("@BranchID", SqlDbType.Int);
            cmd.Parameters["@BranchID"].Value = BranchID;

            cmd.Parameters.Add("@FromDate", SqlDbType.VarChar);
            cmd.Parameters["@FromDate"].Value = FromDate.ToString("MM/dd/yyyy");

            cmd.Parameters.Add("@ToDate", SqlDbType.VarChar);
            cmd.Parameters["@ToDate"].Value = ToDate.ToString("MM/dd/yyyy");

            cmd.Parameters.Add("@CreditNoteNo", SqlDbType.VarChar);
            if (CreditNoteNo == null)
            {
                cmd.Parameters["@CreditNoteNo"].Value = "";
            }
            else
            {
                cmd.Parameters["@CreditNoteNo"].Value = CreditNoteNo;
            }
            cmd.Parameters.Add("@InvoiceNo", SqlDbType.VarChar);
            if (InvoiceNo == null)
            {
                cmd.Parameters["@InvoiceNo"].Value = "";
            }
            else

            {
                cmd.Parameters["@InvoiceNo"].Value = InvoiceNo;
            }



            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<CreditNoteVM> objList = new List<CreditNoteVM>();
            CreditNoteVM obj;

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new CreditNoteVM();
                    obj.AcJournalID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AcJournalID"].ToString());
                    obj.CreditNoteID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CreditNoteID"].ToString());
                    obj.Date = Convert.ToDateTime(ds.Tables[0].Rows[i]["CreditNoteDate"].ToString());
                    obj.CustomerID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CustomerID"].ToString());
                    obj.CreditNoteNo = ds.Tables[0].Rows[i]["CreditNoteNo"].ToString();
                    obj.CustomerName = ds.Tables[0].Rows[i]["CustomerName"].ToString();
                    obj.Description = ds.Tables[0].Rows[i]["Description"].ToString();
                    obj.Amount = Convert.ToDecimal(ds.Tables[0].Rows[i]["Amount"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static List<CreditNoteVM> CreditNoteList(int FYearId, int BranchID, DateTime FromDate, DateTime ToDate,string CreditNoteNo,string InvoiceNo)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "CreditNoteList";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@FYearID", SqlDbType.Int);
            cmd.Parameters["@FYearID"].Value = FYearId;

            cmd.Parameters.Add("@BranchID", SqlDbType.Int);
            cmd.Parameters["@BranchID"].Value = BranchID;

            cmd.Parameters.Add("@FromDate", SqlDbType.VarChar);
            cmd.Parameters["@FromDate"].Value = FromDate.ToString("MM/dd/yyyy");

            cmd.Parameters.Add("@ToDate", SqlDbType.VarChar);
            cmd.Parameters["@ToDate"].Value = ToDate.ToString("MM/dd/yyyy");

            cmd.Parameters.Add("@CreditNoteNo", SqlDbType.VarChar);
            if (CreditNoteNo==null)
            {
                cmd.Parameters["@CreditNoteNo"].Value = "";
            }
            else
            {
                cmd.Parameters["@CreditNoteNo"].Value = CreditNoteNo;
            }
            cmd.Parameters.Add("@InvoiceNo", SqlDbType.VarChar);
            if (InvoiceNo == null)
            {
                cmd.Parameters["@InvoiceNo"].Value = "";
            }
            else
                
            {
                cmd.Parameters["@InvoiceNo"].Value = InvoiceNo;
            }
            
            

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<CreditNoteVM> objList = new List<CreditNoteVM>();
            CreditNoteVM obj;

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new CreditNoteVM();
                    obj.AcJournalID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AcJournalID"].ToString());
                    obj.CreditNoteID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CreditNoteID"].ToString());
                    obj.Date = Convert.ToDateTime(ds.Tables[0].Rows[i]["CreditNoteDate"].ToString());
                    obj.CustomerID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CustomerID"].ToString());
                    obj.CreditNoteNo = ds.Tables[0].Rows[i]["CreditNoteNo"].ToString();
                    obj.CustomerName= ds.Tables[0].Rows[i]["CustomerName"].ToString();
                    obj.Description = ds.Tables[0].Rows[i]["Description"].ToString();
                    obj.Amount = Convert.ToDecimal(ds.Tables[0].Rows[i]["Amount"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }


        public static void InsertJournalOfCreditNote(int CreditNoteID, int fyearId)
        {
            //SP_InsertJournalEntryForRecPay
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_InsertJournalEntryForCreditNote";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CreditNoteID", CreditNoteID);
            cmd.Parameters.AddWithValue("@AcFinnancialYearId", fyearId);
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();

            //Context1.SP_InsertJournalEntryForRecPay(RecpayID, fyaerId);
        }

        public static void InsertJournalOfDebitNote(int DebitNoteID, int fyearId)
        {
            //SP_InsertJournalEntryForRecPay
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_InsertJournalEntryForDebitNote";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@DebitNoteID", DebitNoteID);
            cmd.Parameters.AddWithValue("@AcFinnancialYearId", fyearId);
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();

            //Context1.SP_InsertJournalEntryForRecPay(RecpayID, fyaerId);
        }
        public static List<DebitNoteVM> DebitNoteList(int FYearId, int BranchID, DateTime FromDate, DateTime ToDate)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "DebitNoteList";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@FYearID", SqlDbType.Int);
            cmd.Parameters["@FYearID"].Value = FYearId;

            cmd.Parameters.Add("@BranchID", SqlDbType.Int);
            cmd.Parameters["@BranchID"].Value = BranchID;

            cmd.Parameters.Add("@FromDate", SqlDbType.VarChar);
            cmd.Parameters["@FromDate"].Value = FromDate.ToString("MM/dd/yyyy");

            cmd.Parameters.Add("@ToDate", SqlDbType.VarChar);
            cmd.Parameters["@ToDate"].Value = ToDate.ToString("MM/dd/yyyy");

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<DebitNoteVM> objList = new List<DebitNoteVM>();
            DebitNoteVM obj;

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new DebitNoteVM();
                    obj.AcJournalID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AcJournalID"].ToString());
                    obj.DebitNoteId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["DebitNoteId"].ToString());
                    obj.Date = Convert.ToDateTime(ds.Tables[0].Rows[i]["DebitNoteDate"].ToString());
                    obj.SupplierID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["SupplierID"].ToString());
                    obj.DebitNoteNo = ds.Tables[0].Rows[i]["DebitNoteNo"].ToString();
                    obj.SupplierName = ds.Tables[0].Rows[i]["SupplierName"].ToString();
                    obj.Remarks = ds.Tables[0].Rows[i]["Remarks"].ToString();
                    obj.Amount = Convert.ToDecimal(ds.Tables[0].Rows[i]["Amount"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }

        

        public static DataTable DeleteDebiteNote(int id)
        {


            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "DeleteDebitNoteNo";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                return ds.Tables[0];

            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public static DataTable DeleteCreditNote(int id)
        {
           

            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "DeleteCreditNote";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);

                return ds.Tables[0];
            }
            catch (Exception ex)
            {
                throw ex;
            }

           
        }

    }
}