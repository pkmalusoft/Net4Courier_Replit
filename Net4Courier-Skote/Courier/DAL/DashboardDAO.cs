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
using System.Web.Hosting;
using System.IO;
namespace Net4Courier.DAL
{
    public class DashboardDAO
    {
        public static List<QuickAWBVM> GetDashboardConsignmentList(int BranchId, int FYearId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DashboardShipmentList";
            cmd.CommandType = CommandType.StoredProcedure;            
            cmd.Parameters.AddWithValue("@BranchId", BranchId);
            cmd.Parameters.AddWithValue("@FYearId", FYearId);
            

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<QuickAWBVM> objList = new List<QuickAWBVM>();
            QuickAWBVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new QuickAWBVM();
                    obj.InScanID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["InScanID"].ToString());
                    obj.HAWBNo = ds.Tables[0].Rows[i]["AWBNo"].ToString();
                    obj.shippername = ds.Tables[0].Rows[i]["Consignor"].ToString();
                    obj.consigneename = ds.Tables[0].Rows[i]["Consignee"].ToString();
                    obj.ConsignorCountryName = ds.Tables[0].Rows[i]["ConsignorCountryName"].ToString();
                    obj.ConsignorCityName = ds.Tables[0].Rows[i]["ConsignorCityName"].ToString();
                    obj.ConsigneeCountryName = ds.Tables[0].Rows[i]["ConsigneeCountryName"].ToString();
                    obj.ConsigneeCityName = ds.Tables[0].Rows[i]["ConsigneeCityName"].ToString();
                    obj.InScanDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["TransactionDate"].ToString());
                    obj.CourierStatus = ds.Tables[0].Rows[i]["CourierStatus"].ToString();
                    //obj.StatusType = ds.Tables[0].Rows[i]["StatusType"].ToString();
                    obj.totalCharge = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["TotalCharge"].ToString());
                    obj.paymentmode = ds.Tables[0].Rows[i]["Paymentmode"].ToString();
                    obj.ConsigneePhone = ds.Tables[0].Rows[i]["ConsigneePhone"].ToString();
                    obj.ConsignorPhone = ds.Tables[0].Rows[i]["ConsignorPhone"].ToString();
                    obj.CreatedByName = ds.Tables[0].Rows[i]["CreatedByName"].ToString();
                    obj.LastModifiedByName = ds.Tables[0].Rows[i]["LastModifiedByName"].ToString();
                    obj.CreatedByDate = ds.Tables[0].Rows[i]["CreatedByDate"].ToString();
                    obj.LastModifiedDate =ds.Tables[0].Rows[i]["LastModifiedDate"].ToString();                    
                    obj.Weight = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Weight"].ToString());
                    obj.Pieces = ds.Tables[0].Rows[i]["Pieces"].ToString();                                                           
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static List<PaymentModeWiseCount> GetDashboardPaymentConsignmentList(int BranchId, int FYearId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DashboardShipmentList1";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@BranchId", BranchId);
            cmd.Parameters.AddWithValue("@FYearId", FYearId);


            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<PaymentModeWiseCount> objList = new List<PaymentModeWiseCount>();
            PaymentModeWiseCount obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new PaymentModeWiseCount();
                    obj.ID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ID"].ToString());
                    obj.PaymentMode = ds.Tables[0].Rows[i]["PaymentModeText"].ToString();
                    obj.AWBCount = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["TotalAWB"].ToString());
                    obj.AWBPercent = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AWBPercent"].ToString());

                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static string  DashboardReprocess(int BranchId, int FyearID,int UserId)
        {

            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_DashboardChart";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@BranchId", BranchId); 
                cmd.Parameters.AddWithValue("@FYearId", FyearID);
                cmd.Parameters.AddWithValue("@UserId", UserId);
                cmd.Parameters.AddWithValue("@ProcessDate", CommonFunctions.GetCurrentDateTime());

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);

                 

                if (ds != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count>0)
                    {
                        return "OK";
                    }
                }
                return "OK";

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}