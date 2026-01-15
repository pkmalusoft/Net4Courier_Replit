
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

namespace Net4Courier.DAL
{
    public class SkylarkDAO
    {
        public static List<SkylarkInscanModel> SKylarkInScanList(int FYearId, int BranchID, DateTime FromDate, DateTime ToDate, int CollectedBy, int CustomerId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "CMS_SkylarkInscanList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@FromDate", SqlDbType.VarChar);
            cmd.Parameters["@FromDate"].Value = FromDate.ToString("MM/dd/yyyy");

            cmd.Parameters.Add("@ToDate", SqlDbType.VarChar);
            cmd.Parameters["@ToDate"].Value = ToDate.ToString("MM/dd/yyyy");
            //if (CollectedBy == null)
            //    CollectedBy =0;
            cmd.Parameters.AddWithValue("@CollectedBy", CollectedBy);

            cmd.Parameters.AddWithValue("@CustomerID", CustomerId);

            cmd.Parameters.Add("@FYearID", SqlDbType.Int);            
            cmd.Parameters["@FYearID"].Value = FYearId;

            cmd.Parameters.Add("@BranchID", SqlDbType.Int);
            cmd.Parameters["@BranchID"].Value = BranchID;
          

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<SkylarkInscanModel> objList = new List<SkylarkInscanModel>();
            SkylarkInscanModel obj;

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new SkylarkInscanModel();
                    obj.ID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ID"].ToString());
                    obj.CustomerID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CustomerID"].ToString());
                    obj.EntryDateTime = Convert.ToDateTime(ds.Tables[0].Rows[i]["EntryDateTime"].ToString());
                    obj.CustomerName = ds.Tables[0].Rows[i]["CustomerName"].ToString();
                    obj.EmployeeName = ds.Tables[0].Rows[i]["EmployeeName"].ToString();
                    obj.BatchNo = ds.Tables[0].Rows[i]["BatchNo"].ToString();
                    obj.VehicleNo = ds.Tables[0].Rows[i]["VehicleNo"].ToString();                    
                    obj.TotalAWB = Convert.ToInt32(ds.Tables[0].Rows[i]["TotalAWB"].ToString());
                    obj.AWBBatchID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AWBBatchID"].ToString());
                    obj.AWBBatchNo = ds.Tables[0].Rows[i]["AWBBatchNo"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static string SaveBulkInScan(int ID, int ReceivedBy,DateTime ReceivedDate)
        {
            
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "CMS_SaveBulkInScan";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Parameters.AddWithValue("@BulkInScanID", ID);
                cmd.Parameters.AddWithValue("@ReceivedBy", ReceivedBy);
                cmd.Parameters.AddWithValue("@ReceivedDate", ReceivedDate.ToString("MM/dd/yyyy hh:mm") );
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }          
                     
        }
        public static SkylarkInscanModel SKylarkInScanByID(int Id,int FYearID,int BranchID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "CMS_SkylarkInscanDetail";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ID", SqlDbType.Int);
            cmd.Parameters["@ID"].Value = Id;

            cmd.Parameters.Add("@FYearID", SqlDbType.VarChar);
            cmd.Parameters["@FYearID"].Value = FYearID;
            
            cmd.Parameters.AddWithValue("@BranchID", BranchID);

            

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<SkylarkInscanModel> objList = new List<SkylarkInscanModel>();
            SkylarkInscanModel obj=new SkylarkInscanModel();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    
                    obj.ID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ID"].ToString());
                    obj.CustomerID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CustomerID"].ToString());
                    obj.EntryDateTime = Convert.ToDateTime(ds.Tables[0].Rows[i]["EntryDateTime"].ToString());
                    obj.CustomerName = ds.Tables[0].Rows[i]["CustomerName"].ToString();
                    obj.EmployeeName = ds.Tables[0].Rows[i]["EmployeeName"].ToString();
                    obj.BatchNo = ds.Tables[0].Rows[i]["BatchNo"].ToString();
                    obj.VehicleNo = ds.Tables[0].Rows[i]["VehicleNo"].ToString();
                    obj.TotalAWB = Convert.ToInt32(ds.Tables[0].Rows[i]["TotalAWB"].ToString());

                    obj.AWBBatchID =  CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AWBBatchID"].ToString());
                    obj.ReceivedBy = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ReceivedBy"].ToString());
                    obj.AWBBatchNo = ds.Tables[0].Rows[i]["AWBBatchNo"].ToString();
                    if (ds.Tables[0].Rows[i]["ReceivedDate"] != System.DBNull.Value)
                        obj.ReceivedDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["ReceivedDate"].ToString());
                    else
                        obj.ReceivedDate = CommonFunctions.GetBranchDateTime();//.ToString("dd-MM-yyyy hh:mm");
                    objList.Add(obj);
                }
            }
            return obj;
        }


        public static List<SkylarkReturntoConsignorAWB> SKylarkReturntoConsignorList(int FYearId, int BranchID, DateTime FromDate, DateTime ToDate)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "CMS_SkylarkReturntoCosignorList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@FromDate", SqlDbType.VarChar);
            cmd.Parameters["@FromDate"].Value = FromDate.ToString("MM/dd/yyyy");

            cmd.Parameters.Add("@ToDate", SqlDbType.VarChar);
            cmd.Parameters["@ToDate"].Value = ToDate.ToString("MM/dd/yyyy");
            
            cmd.Parameters.Add("@FYearID", SqlDbType.Int);
            cmd.Parameters["@FYearID"].Value = FYearId;

            cmd.Parameters.Add("@BranchID", SqlDbType.Int);
            cmd.Parameters["@BranchID"].Value = BranchID;


            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<SkylarkReturntoConsignorAWB> objList = new List<SkylarkReturntoConsignorAWB>();
            SkylarkReturntoConsignorAWB obj;

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new SkylarkReturntoConsignorAWB();
                    obj.InScanId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["InScanID"].ToString());
                    obj.AWBNo = ds.Tables[0].Rows[i]["AWBNo"].ToString();
                    obj.AWBDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["TransactionDate"].ToString());
                    obj.EntryDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["EntryDate"].ToString());
                    obj.CustomerName = ds.Tables[0].Rows[i]["Consignor"].ToString();
                    obj.CourierName = ds.Tables[0].Rows[i]["EmployeeName"].ToString();
                    obj.CourierStatus = ds.Tables[0].Rows[i]["CourierStatus"].ToString();
                    obj.CourierStatusId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CourierStatusId"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static string SaveReturnedtoConsignor(int InScanID,string AWBNo, int EmpID,int UserID,string Remarks, DateTime ReturnDate)
        {

            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "CMS_AWBReturntoConsignor";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@InScanID", InScanID);
                cmd.Parameters.AddWithValue("@AWBNo", AWBNo); 
                cmd.Parameters.AddWithValue("@EmpID", EmpID);
                cmd.Parameters.AddWithValue("@CourierUserID", UserID);
                cmd.Parameters.AddWithValue("@Remarks", Remarks);
                cmd.Parameters.AddWithValue("@EntryDate", ReturnDate.ToString("MM/dd/yyyy hh:mm"));
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public static string RecheckAWBStatus(string AWBNo)
        {

            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_ReCheckAWBStatus";
                cmd.CommandType = CommandType.StoredProcedure;
               
                cmd.Parameters.AddWithValue("@AWBNo", AWBNo);
                
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
    }
}