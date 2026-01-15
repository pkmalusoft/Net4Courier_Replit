using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Net4Courier.Models;
using System.Configuration;
using System.IO;
using CrystalDecisions.CrystalReports.Engine;
using System.Web.Hosting;
using CrystalDecisions.Shared;

namespace Net4Courier.DAL
{
    public class AWBDAO
    {

        #region "AWBBookIssue"
        public static DataTable DeleteAWBCourier(int AWBBookIssueID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteAWBBookIssue";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AWBBookIssueID", AWBBookIssueID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            if (ds.Tables[0].Rows.Count > 0)
            {
                return ds.Tables[0];
            }
            else
            {
                return null;
            }


        }
        public static List<AWBBookIssueList> GetAWBBookIssue(int branchId)
        {
            AWBBookIssueSearch paramobj = (AWBBookIssueSearch)(HttpContext.Current.Session["AWBBookIssueSearch"]);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetAWBBookIssueDetails";
            cmd.CommandType = CommandType.StoredProcedure;

            if (paramobj.DocumentNo != null)
                cmd.Parameters.AddWithValue("@DocumentNo", paramobj.DocumentNo);

            if (paramobj.FromDate != null)
                cmd.Parameters.AddWithValue("@FromDate", Convert.ToDateTime(paramobj.FromDate).ToString("MM/dd/yyyy"));

            if (paramobj.ToDate != null)
                cmd.Parameters.AddWithValue("@ToDate", Convert.ToDateTime(paramobj.ToDate).ToString("MM/dd/yyyy"));

            cmd.Parameters.AddWithValue("@BranchID", branchId);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<AWBBookIssueList> objList = new List<AWBBookIssueList>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    AWBBookIssueList obj = new AWBBookIssueList();
                    obj.AWBBOOKIssueID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AWBBOOKIssueID"].ToString());
                    obj.TransDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["TransDate"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.Documentno = ds.Tables[0].Rows[i]["Documentno"].ToString();
                    obj.AWBNOFrom = ds.Tables[0].Rows[i]["AWBNOFrom"].ToString();
                    obj.AWBNOTo = ds.Tables[0].Rows[i]["AWBNOTo"].ToString();
                    obj.BookNo = ds.Tables[0].Rows[i]["BookNo"].ToString();
                    obj.EmployeeName = ds.Tables[0].Rows[i]["EmployeeName"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static List<AWBDetailVM> GetAWBBookIssueDetail(int AWBBookIssueId)
        {
            
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GETAWBBookIssueDetail";
            cmd.CommandType = CommandType.StoredProcedure;            
            cmd.Parameters.AddWithValue("@AWBBookIssueID", AWBBookIssueId);
                       

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<AWBDetailVM> objList = new List<AWBDetailVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    AWBDetailVM obj = new AWBDetailVM();
                    obj.ID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ID"].ToString());
                    obj.AWBBookIssueID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AWBBookIssueID"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.AWBNo = ds.Tables[0].Rows[i]["AWBNo"].ToString();
                    obj.Consignor = ds.Tables[0].Rows[i]["Consignor"].ToString();                    
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static List<AWBDetailVM> GetAWBPrepaidDetail(int PrepaidAWBId)
        {
            
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GETAWBBookPrePaidDetail";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PrepaidAWBID", PrepaidAWBId);


            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<AWBDetailVM> objList = new List<AWBDetailVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    AWBDetailVM obj = new AWBDetailVM();
                    obj.ID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ID"].ToString());
                    obj.PrepaidAWBID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["PrepaidAWBID"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.AWBNo = ds.Tables[0].Rows[i]["AWBNo"].ToString();
                    obj.Consignor = ds.Tables[0].Rows[i]["Consignor"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static string GetMaxAWBBookIssueDocumentNo()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "select isnull(max(Isnull(cast(documentno as integer),0)) +1,1) From AWBBOOKIssue ";
            cmd.CommandType = CommandType.Text;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return ds.Tables[0].Rows[0][0].ToString();
            }
            else
            {
                return "";
            }

        }

        public static string GenerateAWBBookIssue(int AWBBookIssueID)
        {
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_GenerateAWBBookIssue";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;
                        con.Open();
                        cmd.Parameters.AddWithValue("@AWBBookIssueID", AWBBookIssueID);
                        cmd.ExecuteNonQuery();

                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "OK";


        }

        #endregion

        #region "AWBPrepaid"

        public static DataTable DeleteAWBPrepaid(int PrepaidAWBID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteAWBPrepaid";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PrepaidAWBID", PrepaidAWBID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            if (ds.Tables[0].Rows.Count > 0)
            {
                return ds.Tables[0];
            }
            else
            {
                return null;
            }


        }
        public static string GetMaxPrepaidAWBDocumentNo()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "select isnull(max(Isnull(cast(documentno as integer),0)) +1,1) From PrepaidAWB ";
            cmd.CommandType = CommandType.Text;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return ds.Tables[0].Rows[0][0].ToString();
            }
            else
            {
                return "";
            }

        }
        public static List<AWBPrepaidList> GetAWBPrepaid(int branchId)
        {
            AWBPrepaidSearch paramobj = (AWBPrepaidSearch)(HttpContext.Current.Session["AWBPrepaidSearch"]);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetAWBPrepaidDetails";
            cmd.CommandType = CommandType.StoredProcedure;

            if (paramobj.DocumentNo != null)
                cmd.Parameters.AddWithValue("@DocumentNo", paramobj.DocumentNo);

            if (paramobj.FromDate != null)
                cmd.Parameters.AddWithValue("@FromDate", Convert.ToDateTime(paramobj.FromDate).ToString("MM/dd/yyyy"));

            if (paramobj.ToDate != null)
                cmd.Parameters.AddWithValue("@ToDate", Convert.ToDateTime(paramobj.ToDate).ToString("MM/dd/yyyy"));

            cmd.Parameters.AddWithValue("@BranchID", branchId);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<AWBPrepaidList> objList = new List<AWBPrepaidList>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    AWBPrepaidList obj = new AWBPrepaidList();
                    obj.PrepaidAWBID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["PrepaidAWBID"].ToString());
                    obj.TransDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["TransDate"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.Documentno = ds.Tables[0].Rows[i]["Documentno"].ToString();
                    obj.AWBNOFrom = ds.Tables[0].Rows[i]["AWBNOFrom"].ToString();
                    obj.AWBNOTo = ds.Tables[0].Rows[i]["AWBNOTo"].ToString();
                    obj.Reference = ds.Tables[0].Rows[i]["Reference"].ToString();
                    obj.CustomerName = ds.Tables[0].Rows[i]["CustomerName"].ToString();
                    obj.NoOfAWBs = Convert.ToInt32(ds.Tables[0].Rows[i]["NoOfAWBs"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static AWBInfo GetAWBInfo(string AWBNo)
        {
            AWBInfo obj = new AWBInfo();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GETAWBUsedStatus";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AWBNo", @AWBNo);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            if (ds != null && ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {

                    obj.ReferenceID = CommonFunctions.ParseInt(ds.Tables[0].Rows[0]["ReferenceID"].ToString());
                    obj.AWBNo = ds.Tables[0].Rows[0]["AWBNo"].ToString();
                    obj.Status = ds.Tables[0].Rows[0]["Status"].ToString();
                    obj.Mode = ds.Tables[0].Rows[0]["Mode"].ToString();

                    if (obj.Status != "Available")
                        return obj;

                    obj.InScanID = CommonFunctions.ParseInt(ds.Tables[0].Rows[0]["InScanID"].ToString());
                    if (obj.InScanID > 0)
                        obj.QuickInScanID = CommonFunctions.ParseInt(ds.Tables[0].Rows[0]["QuickInScanId"].ToString());
                    else
                        obj.QuickInScanID = 0;
                    if (obj.Status == "Available" && (obj.Mode == "Prepaid"))
                    {
                        obj.CourierCharge = ds.Tables[0].Rows[0]["CourierCharge"] == DBNull.Value ? 0 : Convert.ToDecimal(ds.Tables[0].Rows[0]["CourierCharge"].ToString());
                    }
                    if (obj.Status == "Available" && (obj.Mode == "Prepaid" || obj.Mode == "NotPrepaid"))
                    {
                        obj.CustomerID = ds.Tables[0].Rows[0]["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(ds.Tables[0].Rows[0]["CustomerID"].ToString());
                        obj.CustomerName = ds.Tables[0].Rows[0]["CustomerName"].ToString();
                        obj.OriginLocation = ds.Tables[0].Rows[0]["OriginLocation"].ToString();
                        obj.DestinationLocation = ds.Tables[0].Rows[0]["DestinationLocation"].ToString();
                        obj.CountryName = ds.Tables[0].Rows[0]["CountryName"].ToString();
                        obj.CityName = ds.Tables[0].Rows[0]["CityName"].ToString();
                        obj.LocationName = ds.Tables[0].Rows[0]["LocationName"].ToString();
                        obj.LocationName = ds.Tables[0].Rows[0]["LocationName"].ToString();
                        obj.Phone = ds.Tables[0].Rows[0]["Phone"] == DBNull.Value ? "" : ds.Tables[0].Rows[0]["Phone"].ToString();
                        obj.Mobile = ds.Tables[0].Rows[0]["Mobile"] == DBNull.Value ? "" : ds.Tables[0].Rows[0]["Mobile"].ToString();
                        obj.Address1 = ds.Tables[0].Rows[0]["Address1"].ToString();
                        obj.Address2 = ds.Tables[0].Rows[0]["Address2"].ToString();
                        obj.Address3 = ds.Tables[0].Rows[0]["Address3"].ToString();
                        obj.PickupSubLocality = ds.Tables[0].Rows[0]["PickupSubLocality"].ToString();
                        obj.DeliverySubLocality = ds.Tables[0].Rows[0]["DeliverySubLocality"].ToString();
                        obj.OriginPlaceID = ds.Tables[0].Rows[0]["OriginPlaceID"].ToString();
                        obj.DestinationPlaceID = ds.Tables[0].Rows[0]["DestinationPlaceID"].ToString();

                    }
                    else
                    {
                        return obj;
                    }
                }
            }

            return obj;

        }
        public static AWBBatchDetail GetAWBTrackStatus(string AWBNo)
        {
            AWBBatchDetail obj = new AWBBatchDetail();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GETAWBTrackStatus";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AWBNo", @AWBNo);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            if (ds != null && ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {

                    if (ds.Tables[0].Rows[0]["Status"].ToString() == "StatusAvailable")
                    {
                        DataRow dr = ds.Tables[0].Rows[0];
                        obj.AWBTrackStatus = "Available";
                        obj.AssignedEmployeeID = dr["AssignedEmployeeID"] == DBNull.Value ? 0 : Convert.ToInt32(dr["AssignedEmployeeID"].ToString());
                        obj.AssignedForCollection = dr["AssignedForCollection"] == DBNull.Value ? false : Convert.ToBoolean(dr["AssignedForCollection"].ToString());
                        obj.AssignedDate = dr["AssignedDate"] == DBNull.Value ? "" : dr["AssignedDate"].ToString();
                        obj.QuickInscanId = dr["QuickInscanId"] == DBNull.Value ? 0 : Convert.ToInt32(dr["QuickInscanId"].ToString());
                        obj.InscanVehicleId = dr["InscanVehicleId"] == DBNull.Value ? 0 : Convert.ToInt32(dr["InscanVehicleId"].ToString());
                        obj.PickedUpEmpID = dr["PickedUpEmpID"] == DBNull.Value ? 0 : Convert.ToInt32(dr["PickedUpEmpID"].ToString()); //collected person id
                        obj.PickedupDate = dr["PickedupDate"] == DBNull.Value ? "" : dr["PickedupDate"].ToString();
                        obj.CollectedBy = dr["CollectedBy"] == DBNull.Value ? false : Convert.ToBoolean(dr["CollectedBy"].ToString());
                        obj.ReceivedDate = dr["ReceivedDate"] == DBNull.Value ? "" : dr["ReceivedDate"].ToString();
                        obj.DepotReceivedBy = dr["DepotReceivedBy"] == DBNull.Value ? 0 : Convert.ToInt32(dr["DepotReceivedBy"].ToString());
                        obj.ReceivedBy = dr["ReceivedBy"] == DBNull.Value ? false : Convert.ToBoolean(dr["ReceivedBy"].ToString());
                        obj.OutScanDelivery = dr["OutScanDelivery"] == DBNull.Value ? false : Convert.ToBoolean(dr["OutScanDelivery"].ToString());
                        obj.OutscanVehicleId = dr["OutscanVehicleId"] == DBNull.Value ? 0 : Convert.ToInt32(dr["OutscanVehicleId"].ToString());
                        obj.OutscanVehicleId = dr["OutScanDeliveredID"] == DBNull.Value ? 0 : Convert.ToInt32(dr["OutScanDeliveredID"].ToString());
                        obj.OutScanDate = dr["OutScanDate"] == DBNull.Value ? "" : dr["OutScanDate"].ToString();
                        obj.DelieveryAttemptDate = dr["DelieveryAttemptDate"] == DBNull.Value ? "" : dr["DelieveryAttemptDate"].ToString();
                        obj.DeliveryAttemptedBy = dr["DeliveryAttemptedBy"] == DBNull.Value ? 0 : Convert.ToInt32(dr["DeliveryAttemptedBy"].ToString());
                        obj.Delivered = dr["Delivered"] == DBNull.Value ? false : Convert.ToBoolean(dr["Delivered"].ToString());
                        obj.DeliveredDate = dr["DeliveredDate"] == DBNull.Value ? "" : dr["DeliveredDate"].ToString();
                        obj.DeliveredBy = dr["DeliveredBy"] == DBNull.Value ? 0 : Convert.ToInt32(dr["DeliveredBy"].ToString());
                        obj.CourierId = dr["CourierId"] == DBNull.Value ? 0 : Convert.ToInt32(dr["CourierId"].ToString());
                        obj.VehicleId = dr["VehicleId"] == DBNull.Value ? 0 : Convert.ToInt32(dr["VehicleId"].ToString());

                        return obj;
                    }
                    else
                    {
                        DataRow dr = ds.Tables[0].Rows[0];
                        obj.AWBTrackStatus = "Available";
                        obj.CourierId = dr["CourierId"] == DBNull.Value ? 0 : Convert.ToInt32(dr["CourierId"].ToString());
                        obj.VehicleId = dr["VehicleId"] == DBNull.Value ? 0 : Convert.ToInt32(dr["VehicleId"].ToString());
                        return obj;
                    }

                }
            }

            return obj;

        }

        public static string GenerateAWBPrepaid(int PrepaidAWBID)
        {
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_GenerateAWBPrepaid";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;
                        con.Open();
                        cmd.Parameters.AddWithValue("@PrepaidAWBID", PrepaidAWBID);
                        cmd.ExecuteNonQuery();

                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "OK";


        }
        public static string SavePrepaidAWBPosting(int PrepaidAWBId, int FyearId, int BranchId, int CompanyId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_PrepaidAWBPosting";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PrepaidAWBID", PrepaidAWBId);
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            return "ok";

        }
        //public static string SavePrepaidAWBPosting(int DRSRecPayId, int FyearId, int BranchId, int CompanyId)
        //{
        //    SqlCommand cmd = new SqlCommand();
        //    cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
        //    cmd.CommandText = "SP_DRSRecPayPosting";
        //    cmd.CommandType = CommandType.StoredProcedure;
        //    cmd.Parameters.AddWithValue("@RecPayId", DRSRecPayId);
        //    cmd.Connection.Open();
        //    cmd.ExecuteNonQuery();
        //    return "ok";

        //}
        public static string CheckAWBStock(int AWBFrom, int AWBTo)
        {
            string result = "";
            bool status1 = false;
            bool status2 = false;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_CheckAWBStock";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AWBFrom", AWBFrom);
            cmd.Parameters.AddWithValue("@AWBTo", AWBTo);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<AWBDetailVM> objList = new List<AWBDetailVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                status1 = Convert.ToBoolean(ds.Tables[0].Rows[0]["Status1"].ToString());
                status2 = Convert.ToBoolean(ds.Tables[0].Rows[0]["Status2"].ToString());
                result = ds.Tables[0].Rows[0]["Message"].ToString();

                if (status1 == true && status2 == true)
                    return "ok";
                else
                    return result;

            }

            return "failed";

        }
        public static List<AWBDetailVM> CheckAWBDuplicate(int AWBFrom, int AWBTo, int AWBBookIssueID, int PrePaidAWBID)
        {

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_CheckAWBDuplicate";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AWBFrom", AWBFrom);
            cmd.Parameters.AddWithValue("@AWBTo", AWBTo);
            cmd.Parameters.AddWithValue("@AWBBookIssueID", AWBBookIssueID);
            cmd.Parameters.AddWithValue("@PrePaidAWBID", PrePaidAWBID);


            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<AWBDetailVM> objList = new List<AWBDetailVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    AWBDetailVM obj = new AWBDetailVM();
                    obj.AWBNo = ds.Tables[0].Rows[i]["AWBNo"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }
        #endregion

        #region "AWBBAtch"
        public static DataTable DeleteBatchAWB1(int BatchID, int InScanID)
        {
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_DeleteBatchAWB";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;
                        con.Open();
                        cmd.Parameters.AddWithValue("@BatchId", BatchID);
                        cmd.Parameters.AddWithValue("@InscanId", InScanID);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataSet ds = new DataSet();
                        da.Fill(ds);

                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            return ds.Tables[0];

                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }



        }
        public static DataTable DeleteBatchAWB(int BatchID,int InScanID)
        {
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_DeleteBatchAWB";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;
                        con.Open();
                        cmd.Parameters.AddWithValue("@BatchId", BatchID);
                        cmd.Parameters.AddWithValue("@InscanId", InScanID);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataSet ds = new DataSet();
                        da.Fill(ds);

                        if (ds.Tables[0].Rows.Count > 0)
                        {
                           return ds.Tables[0];

                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            


        }

        public static List<AWBBatchDetail> GetDomesticBatchAWBInfo(int BatchID, int InscanID = 0)
        {
            List<AWBBatchDetail> list = new List<AWBBatchDetail>();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GETBatchDomesticAWBList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@BatchID", BatchID);
            //cmd.Parameters.AddWithValue("@InscanId", InscanID);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            if (ds != null && ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        AWBBatchDetail obj = new AWBBatchDetail();
                        obj.InScanID = CommonFunctions.ParseInt(dt.Rows[i]["InScanID"].ToString());
                        obj.AWBDate = Convert.ToDateTime(dt.Rows[i]["AWBDate"].ToString());
                        obj.AWBNo = dt.Rows[i]["AWBNo"].ToString();
                        //obj.CurrentCourierStatus = dt.Rows[i]["CourierStatus"].ToString();
                        //obj.CurrentStatusType = dt.Rows[i]["StatusType"].ToString();
                        obj.CustomerID = dt.Rows[i]["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["CustomerID"].ToString());
                        obj.CustomerName = dt.Rows[i]["CustomerName"].ToString();
                        obj.Consignor = dt.Rows[i]["Consignor"].ToString();
                        obj.ConsignorContact = dt.Rows[i]["ConsignorContact"].ToString();
                        obj.ConsignorAddress1_Building = dt.Rows[i]["ConsignorAddress1_Building"].ToString();
                        obj.ConsignorAddress2_Street = dt.Rows[i]["ConsignorAddress2_Street"].ToString();
                        obj.ConsignorAddress3_PinCode = dt.Rows[i]["ConsignorAddress3_PinCode"].ToString();
                        obj.ConsignorCountryName = dt.Rows[i]["ConsignorCountryName"].ToString();
                        obj.ConsignorCityName = dt.Rows[i]["ConsignorCityName"].ToString();
                        obj.ConsignorLocationName = dt.Rows[i]["ConsignorLocationName"].ToString();
                        obj.ConsignorPhone = dt.Rows[i]["ConsignorPhone"].ToString();
                        obj.ConsignorMobileNo = dt.Rows[i]["ConsignorMobileNo"].ToString();
                        obj.Consignee = dt.Rows[i]["Consignee"].ToString();
                        obj.ConsigneeContact = dt.Rows[i]["ConsigneeContact"].ToString();
                        obj.ConsigneeCountryName = dt.Rows[i]["ConsigneeCountryName"].ToString();
                        obj.ConsigneeAddress1_Building = dt.Rows[i]["ConsigneeAddress1_Building"].ToString();


                        obj.ConsigneeAddress2_Street = dt.Rows[i]["ConsigneeAddress2_Street"].ToString();
                        obj.ConsigneeAddress3_PinCode = dt.Rows[i]["ConsigneeAddress3_PinCode"].ToString();
                        obj.ConsigneeCityName = dt.Rows[i]["ConsigneeCityName"].ToString();
                        obj.ConsigneeLocationName = dt.Rows[i]["ConsigneeLocationName"].ToString();
                        obj.ConsigneePhone = dt.Rows[i]["ConsigneePhone"].ToString();
                        obj.ConsigneeMobileNo = dt.Rows[i]["ConsigneeMobileNo"].ToString();
                        obj.Remarks = dt.Rows[i]["Remarks"].ToString();

                        obj.CourierCharge = dt.Rows[i]["CourierCharge"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["CourierCharge"].ToString());
                        obj.MaterialCost = dt.Rows[i]["MaterialCost"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["MaterialCost"].ToString());
                        obj.OtherCharge = dt.Rows[i]["OtherCharge"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["OtherCharge"].ToString());
                        obj.NetTotal = dt.Rows[i]["NetTotal"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["NetTotal"].ToString());
                        obj.PaymentModeId = dt.Rows[i]["PaymentModeId"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["PaymentModeId"].ToString());
                        obj.MovementID = dt.Rows[i]["MovementID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["MovementID"].ToString());
                        obj.ParcelTypeId = dt.Rows[i]["ParcelTypeId"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["ParcelTypeId"].ToString());
                        //bj.DocumentTypeId = dt.Rows[i]["DocumentTypeId"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["DocumentTypeId"].ToString());
                        obj.ProductTypeID = dt.Rows[i]["ProductTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["ProductTypeID"].ToString());
                        obj.MovementType = dt.Rows[i]["MovementType"].ToString();
                        obj.PaymentModeText = dt.Rows[i]["PaymentModeText"].ToString();
                        obj.ParcelType = dt.Rows[i]["ParcelType"].ToString();
                        obj.ProductType = dt.Rows[i]["ProductName"].ToString();
                        obj.CargoDescription = dt.Rows[i]["CargoDescription"].ToString();
                        obj.Pieces = dt.Rows[i]["Pieces"] == DBNull.Value ? "0" : dt.Rows[i]["Pieces"].ToString();
                        obj.Weight = dt.Rows[i]["Weight"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["Weight"].ToString());
                        //obj.ManifestWeight = dt.Rows[i]["ManifestWeight"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["ManifestWeight"].ToString());
                        obj.TaxPercent = dt.Rows[i]["TaxPercent"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["TaxPercent"].ToString());
                        obj.TaxAmount = dt.Rows[i]["TaxAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["TaxAmount"].ToString());
                        obj.SurchargePercent = dt.Rows[i]["SurchargePercent"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["SurchargePercent"].ToString());
                        obj.SurchargeAmount = dt.Rows[i]["SurchargeAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["SurchargeAmount"].ToString());
                        //obj.SpecialInstructions = dt.Rows[i]["SpecialNotes"] == DBNull.Value ? "" : dt.Rows[i]["SpecialNotes"].ToString();
                        obj.CustomerRateID = dt.Rows[i]["CustomerRateID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["CustomerRateID"].ToString());
                        obj.CurrencyID = dt.Rows[i]["CurrencyID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["CurrencyID"].ToString());
                        obj.Currency = dt.Rows[i]["CurrencyName"].ToString();
                        obj.CustomsValue = dt.Rows[i]["CustomsValue"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["CustomsValue"].ToString());
                        //obj.FAgentID = dt.Rows[i]["FAgentId"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["FAgentId"].ToString());
                        //obj.FAgentName = dt.Rows[i]["FAgentName"] == DBNull.Value ? "" : dt.Rows[i]["FAgentName"].ToString();
                        //obj.CustomerRateType = GetCustomerRateName(obj.CustomerRateTypeId);
                      //  obj.BagNo = dt.Rows[i]["BagNo"] == DBNull.Value ? "" : dt.Rows[i]["BagNo"].ToString();
                       // obj.MAWB = dt.Rows[i]["MAWB"] == DBNull.Value ? "" : dt.Rows[i]["MAWB"].ToString();
                        obj.AcJournalID = dt.Rows[i]["AcJournalID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["AcJournalID"].ToString());
                        obj.VoucherNo = dt.Rows[i]["VoucherNo"] == DBNull.Value ? "" : dt.Rows[i]["VoucherNo"].ToString();
                        list.Add(obj);
                    }

                }

            }


            return list;

        }
        public static List<AWBBatchDetail> GetBatchAWBInfo(int BatchID,int InscanID=0)
        {
            List<AWBBatchDetail> list = new List<AWBBatchDetail>();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GETBatchAWBList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@BatchID", BatchID);
            //cmd.Parameters.AddWithValue("@InscanId", InscanID);
            
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            if (ds != null && ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        AWBBatchDetail obj = new AWBBatchDetail();
                        obj.InScanID = CommonFunctions.ParseInt(dt.Rows[i]["InScanID"].ToString());
                        obj.TransactionDate = dt.Rows[i]["TransactionDate"].ToString();
                        obj.AWBNo = dt.Rows[i]["AWBNo"].ToString();
                        //obj.CurrentCourierStatus = dt.Rows[i]["CourierStatus"].ToString();
                        //obj.CurrentStatusType = dt.Rows[i]["StatusType"].ToString();
                        obj.CustomerID = dt.Rows[i]["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["CustomerID"].ToString());
                        //obj.CustomerName = dt.Rows[i]["CustomerName"].ToString();
                        obj.Consignor = dt.Rows[i]["Consignor"].ToString();
                        obj.ConsignorAddress1_Building = dt.Rows[i]["ConsignorAddress1_Building"].ToString();
                        obj.ConsignorAddress2_Street = dt.Rows[i]["ConsignorAddress2_Street"].ToString();
                        obj.ConsignorAddress3_PinCode = dt.Rows[i]["ConsignorAddress3_PinCode"].ToString();
                        obj.ConsignorCountryName = dt.Rows[i]["ConsignorCountryName"].ToString();
                        obj.ConsignorContact = dt.Rows[i]["ConsignorContact"].ToString();
                        obj.ConsignorCityName = dt.Rows[i]["ConsignorCityName"].ToString();
                        obj.ConsignorLocationName = dt.Rows[i]["ConsignorLocationName"].ToString();
                        obj.ConsignorPhone = dt.Rows[i]["ConsignorPhone"].ToString();
                        obj.ConsignorMobileNo = dt.Rows[i]["ConsignorMobileNo"].ToString();
                        obj.Consignee = dt.Rows[i]["Consignee"].ToString();
                        obj.ConsigneeContact = dt.Rows[i]["ConsigneeContact"].ToString();
                        obj.ConsigneeAddress1_Building = dt.Rows[i]["ConsigneeAddress1_Building"].ToString();
                        obj.ConsigneeAddress2_Street = dt.Rows[i]["ConsigneeAddress2_Street"].ToString();
                        obj.ConsigneeAddress3_PinCode = dt.Rows[i]["ConsigneeAddress3_PinCode"].ToString();
                        obj.ConsigneeCountryName = dt.Rows[i]["ConsigneeCountryName"].ToString();
                        obj.ConsigneeCityName = dt.Rows[i]["ConsigneeCityName"].ToString();
                        obj.ConsigneeLocationName = dt.Rows[i]["ConsigneeLocationName"].ToString();
                        obj.ConsigneePhone = dt.Rows[i]["ConsigneePhone"].ToString();
                        obj.ConsigneeMobileNo = dt.Rows[i]["ConsigneeMobileNo"].ToString();
                        //obj.PickupLocation = dt.Rows[i]["PickupLocation"].ToString();
                        //obj.DeliveryLocation = dt.Rows[i]["DeliveryLocation"].ToString();
                        //obj.PickupSubLocality = dt.Rows[i]["PickupSubLocality"].ToString();
                        //obj.DeliverySubLocality = dt.Rows[i]["DeliverySubLocality"].ToString();
                        //obj.OriginPlaceID = dt.Rows[i]["OriginPlaceID"].ToString();
                        //obj.DestinationPlaceID = dt.Rows[i]["DestinationPlaceID"].ToString();
                        obj.CourierCharge = dt.Rows[i]["CourierCharge"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["CourierCharge"].ToString());
                        obj.MaterialCost = dt.Rows[i]["MaterialCost"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["MaterialCost"].ToString());
                        obj.OtherCharge = dt.Rows[i]["OtherCharge"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["OtherCharge"].ToString());
                        obj.NetTotal = dt.Rows[i]["NetTotal"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["NetTotal"].ToString());
                        //obj.PaymentModeId = dt.Rows[i]["PaymentModeId"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["PaymentModeId"].ToString());
                        //obj.MovementID = dt.Rows[i]["MovementID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["MovementID"].ToString());
                        //obj.ParcelTypeId = dt.Rows[i]["ParcelTypeId"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["ParcelTypeId"].ToString());
                        //obj.DocumentTypeId = dt.Rows[i]["DocumentTypeId"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["DocumentTypeId"].ToString());
                        //obj.ProductTypeID = dt.Rows[i]["ProductTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["ProductTypeID"].ToString());
                        obj.PaymentModeText = dt.Rows[i]["PaymentModeText"].ToString();
                        obj.CargoDescription = dt.Rows[i]["CargoDescription"].ToString();
                        obj.Pieces = dt.Rows[i]["Pieces"] == DBNull.Value ? "" : dt.Rows[i]["Pieces"].ToString();
                        obj.Weight = dt.Rows[i]["Weight"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["Weight"].ToString());
                        //obj.PickupRequestDate = dt.Rows[i]["PickupRequestDate"] == DBNull.Value ? "" : Convert.ToDateTime(dt.Rows[i]["PickupRequestDate"].ToString()).ToString("dd/MM/yyyy");
                        //obj.AssignedEmployeeID = dt.Rows[i]["AssignedEmployeeID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["AssignedEmployeeID"].ToString());
                        //obj.DepotReceivedBy = dt.Rows[i]["DepotReceivedBy"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["DepotReceivedBy"].ToString());
                        //obj.PickedUpEmpID = dt.Rows[i]["PickedUpEmpID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["PickedUpEmpID"].ToString());
                        //obj.PickedupDate = dt.Rows[i]["PickedupDate"] == DBNull.Value ? "" : Convert.ToDateTime(dt.Rows[i]["PickedupDate"].ToString()).ToString("dd/MM/yyyy");
                        obj.TaxPercent = dt.Rows[i]["TaxPercent"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["TaxPercent"].ToString());
                        obj.TaxAmount = dt.Rows[i]["TaxAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["TaxAmount"].ToString());
                        //obj.SpecialInstructions = dt.Rows[i]["SpecialNotes"] == DBNull.Value ? "" : dt.Rows[i]["SpecialNotes"].ToString();
                        //obj.CustomerRateTypeId = dt.Rows[i]["CustomerRateID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["CustomerRateID"].ToString());
                        //obj.FAgentID = dt.Rows[i]["FAgentId"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["FAgentId"].ToString());
                        //obj.FAgentName = dt.Rows[i]["FAgentName"] == DBNull.Value ? "" : dt.Rows[i]["FAgentName"].ToString();
                        //obj.CustomerRateType = GetCustomerRateName(obj.CustomerRateTypeId);
                        list.Add(obj);
                    }

                }

            }


            return list;

        }

        public static List<AWBBatchDetail> GetBatchAWBImportInfo(int BatchID, int InscanID = 0)
        {
            List<AWBBatchDetail> list = new List<AWBBatchDetail>();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GETBatchImportAWBList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@BatchID", BatchID);
            //cmd.Parameters.AddWithValue("@InscanId", InscanID);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            if (ds != null && ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        AWBBatchDetail  obj = new AWBBatchDetail();
                        obj.IsDeleted = false;
                        obj.InScanID = CommonFunctions.ParseInt(dt.Rows[i]["InScanID"].ToString());
                        obj.AWBDate = Convert.ToDateTime(dt.Rows[i]["AWBDate"].ToString());
                        obj.AWBNo = dt.Rows[i]["AWBNo"].ToString();
                        //obj.CurrentCourierStatus = dt.Rows[i]["CourierStatus"].ToString();
                        //obj.CurrentStatusType = dt.Rows[i]["StatusType"].ToString();
                        obj.CustomerID = dt.Rows[i]["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["CustomerID"].ToString());
                        //obj.CustomerName = dt.Rows[i]["CustomerName"].ToString();
                        obj.Consignor = dt.Rows[i]["Consignor"].ToString();
                        obj.ConsignorContact = dt.Rows[i]["ConsignorContact"].ToString();
                        obj.ConsignorAddress1_Building = dt.Rows[i]["ConsignorAddress1_Building"].ToString();
                        obj.ConsignorAddress2_Street = dt.Rows[i]["ConsignorAddress2_Street"].ToString();
                        obj.ConsignorAddress3_PinCode = dt.Rows[i]["ConsignorAddress3_PinCode"].ToString();
                        obj.ConsignorCountryName = dt.Rows[i]["ConsignorCountryName"].ToString();
                        obj.ConsignorCityName = dt.Rows[i]["ConsignorCityName"].ToString();
                        obj.ConsignorLocationName = dt.Rows[i]["ConsignorLocationName"].ToString();
                        obj.ConsignorPhone = dt.Rows[i]["ConsignorPhone"].ToString();
                        obj.ConsignorMobileNo = dt.Rows[i]["ConsignorMobileNo"].ToString();
                        obj.Consignee = dt.Rows[i]["Consignee"].ToString();
                        obj.ConsigneeContact = dt.Rows[i]["ConsigneeContact"].ToString();
                        obj.ConsigneeCountryName = dt.Rows[i]["ConsigneeCountryName"].ToString();
                        obj.ConsigneeAddress1_Building = dt.Rows[i]["ConsigneeAddress1_Building"].ToString();


                        obj.ConsigneeAddress2_Street = dt.Rows[i]["ConsigneeAddress2_Street"].ToString();
                        obj.ConsigneeAddress3_PinCode = dt.Rows[i]["ConsigneeAddress3_PinCode"].ToString();
                        obj.ConsigneeCityName = dt.Rows[i]["ConsigneeCityName"].ToString();
                        obj.ConsigneeLocationName = dt.Rows[i]["ConsigneeLocationName"].ToString();
                        obj.ConsigneePhone = dt.Rows[i]["ConsigneePhone"].ToString();
                        obj.ConsigneeMobileNo = dt.Rows[i]["ConsigneeMobileNo"].ToString();
                        obj.Remarks = dt.Rows[i]["Remarks"].ToString();

                        obj.CourierCharge = dt.Rows[i]["CourierCharge"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["CourierCharge"].ToString());
                        obj.MaterialCost = dt.Rows[i]["MaterialCost"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["MaterialCost"].ToString());
                        obj.OtherCharge = dt.Rows[i]["OtherCharge"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["OtherCharge"].ToString());
                        obj.NetTotal = dt.Rows[i]["NetTotal"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["NetTotal"].ToString());
                        obj.PaymentModeId = dt.Rows[i]["PaymentModeId"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["PaymentModeId"].ToString());
                        obj.MovementID = dt.Rows[i]["MovementID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["MovementID"].ToString());
                        obj.ParcelTypeId = dt.Rows[i]["ParcelTypeId"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["ParcelTypeId"].ToString());
                        //bj.DocumentTypeId = dt.Rows[i]["DocumentTypeId"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["DocumentTypeId"].ToString());
                        obj.ProductTypeID = dt.Rows[i]["ProductTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["ProductTypeID"].ToString());
                        obj.MovementType = dt.Rows[i]["MovementType"].ToString();
                        obj.ParcelType = dt.Rows[i]["ParcelType"].ToString();
                        obj.ProductType = dt.Rows[i]["ProductName"].ToString();
                        obj.CargoDescription = dt.Rows[i]["CargoDescription"].ToString();
                        obj.Pieces = dt.Rows[i]["Pieces"] == DBNull.Value ? "0" : dt.Rows[i]["Pieces"].ToString();
                        obj.Weight = dt.Rows[i]["Weight"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["Weight"].ToString());
                        
                        obj.TaxPercent = dt.Rows[i]["TaxPercent"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["TaxPercent"].ToString());
                        obj.TaxAmount = dt.Rows[i]["TaxAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["TaxAmount"].ToString());
                        obj.SurchargePercent = dt.Rows[i]["SurchargePercent"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["SurchargePercent"].ToString());
                        obj.SurchargeAmount = dt.Rows[i]["SurchargeAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["SurchargeAmount"].ToString());
                        //obj.SpecialInstructions = dt.Rows[i]["SpecialNotes"] == DBNull.Value ? "" : dt.Rows[i]["SpecialNotes"].ToString();
                        obj.CustomerRateID = dt.Rows[i]["CustomerRateID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["CustomerRateID"].ToString());
                        obj.CurrencyID = dt.Rows[i]["CurrencyID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["CurrencyID"].ToString());
                        obj.Currency = dt.Rows[i]["CurrencyName"].ToString();
                        obj.CustomsValue = dt.Rows[i]["CustomsValue"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["CustomsValue"].ToString());
                        //obj.ForwardingCharge = dt.Rows[i]["ForwardingCharge"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["ForwardingCharge"].ToString());
                        //obj.FAgentName = dt.Rows[i]["FAgentName"] == DBNull.Value ? "" : dt.Rows[i]["FAgentName"].ToString();
                        //obj.CustomerRateType = GetCustomerRateName(obj.CustomerRateTypeId);
                        
                        obj.AcJournalID = dt.Rows[i]["AcJournalID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["AcJournalID"].ToString());
                        obj.FAgentID = dt.Rows[i]["FAgentID"] == DBNull.Value ? -1 : Convert.ToInt32(dt.Rows[i]["FAgentID"].ToString());
                        obj.ForwardingAWBNo = dt.Rows[i]["ForwardingAWBNo"].ToString();
                        obj.VoucherNo = dt.Rows[i]["VoucherNo"] == DBNull.Value ? "" : dt.Rows[i]["VoucherNo"].ToString();
                        
                        
                        list.Add(obj);
                    }

                }

            }


            return list;

        }
        public static DataTable DeleteBatch(int BatchId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteDomesticBatch";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@BatchId", BatchId);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            if (ds.Tables[0].Rows.Count > 0)
            {
                return ds.Tables[0];
            }
            else
            {
                return null;
            }
        }
        public static List<AWBBatchDetail> GetBatchAWBDetail(int BatchID, int InscanID = 0)
        {
            List<AWBBatchDetail> list = new List<AWBBatchDetail>();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GETBatchAWB";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@BatchID", BatchID);
            cmd.Parameters.AddWithValue("@InscanId", InscanID);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            if (ds != null && ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        AWBBatchDetail obj = new AWBBatchDetail();
                        obj.InScanID = CommonFunctions.ParseInt(dt.Rows[i]["InScanID"].ToString());
                        obj.AWBDate = Convert.ToDateTime(dt.Rows[i]["TransactionDate"].ToString());
                        obj.AWBNo = dt.Rows[i]["AWBNo"].ToString();
                        obj.CurrentCourierStatus = dt.Rows[i]["CourierStatus"].ToString();
                        obj.CurrentStatusType = dt.Rows[i]["StatusType"].ToString();
                        obj.CustomerID = dt.Rows[i]["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["CustomerID"].ToString());
                        obj.CustomerName = dt.Rows[i]["CustomerName"].ToString();
                        obj.Consignor = dt.Rows[i]["Consignor"].ToString();
                        obj.ConsignorAddress1_Building = dt.Rows[i]["ConsignorAddress1_Building"].ToString();
                        obj.ConsignorAddress2_Street = dt.Rows[i]["ConsignorAddress2_Street"].ToString();
                        obj.ConsignorAddress3_PinCode = dt.Rows[i]["ConsignorAddress3_PinCode"].ToString();
                        obj.ConsignorCountryName = dt.Rows[i]["ConsignorCountryName"].ToString();
                        obj.ConsignorContact = dt.Rows[i]["ConsignorContact"].ToString();
                        obj.ConsignorCityName = dt.Rows[i]["ConsignorCityName"].ToString();
                        obj.ConsignorLocationName = dt.Rows[i]["ConsignorLocationName"].ToString();
                        obj.ConsignorPhone = dt.Rows[i]["ConsignorPhone"].ToString();
                        obj.ConsignorMobileNo = dt.Rows[i]["ConsignorMobileNo"].ToString();
                        obj.Consignee = dt.Rows[i]["Consignee"].ToString();
                        obj.ConsigneeContact = dt.Rows[i]["ConsigneeContact"].ToString();
                        obj.ConsigneeAddress1_Building = dt.Rows[i]["ConsigneeAddress1_Building"].ToString();
                        obj.ConsigneeAddress2_Street = dt.Rows[i]["ConsigneeAddress2_Street"].ToString();
                        obj.ConsigneeAddress3_PinCode = dt.Rows[i]["ConsigneeAddress3_PinCode"].ToString();
                        obj.ConsigneeCountryName = dt.Rows[i]["ConsigneeCountryName"].ToString();
                        obj.ConsigneeCityName = dt.Rows[i]["ConsigneeCityName"].ToString();
                        obj.ConsigneeLocationName = dt.Rows[i]["ConsigneeLocationName"].ToString();
                        obj.ConsigneePhone = dt.Rows[i]["ConsigneePhone"].ToString();
                        obj.ConsigneeMobileNo = dt.Rows[i]["ConsigneeMobileNo"].ToString();
                        obj.PickupLocation = dt.Rows[i]["PickupLocation"].ToString();
                        obj.DeliveryLocation = dt.Rows[i]["DeliveryLocation"].ToString();
                        obj.PickupSubLocality = dt.Rows[i]["PickupSubLocality"].ToString();
                        obj.DeliverySubLocality = dt.Rows[i]["DeliverySubLocality"].ToString();
                        obj.OriginPlaceID = dt.Rows[i]["OriginPlaceID"].ToString();
                        obj.DestinationPlaceID = dt.Rows[i]["DestinationPlaceID"].ToString();
                        obj.CourierCharge = dt.Rows[i]["CourierCharge"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["CourierCharge"].ToString());
                        obj.MaterialCost = dt.Rows[i]["MaterialCost"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["MaterialCost"].ToString());
                        obj.OtherCharge = dt.Rows[i]["OtherCharge"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["OtherCharge"].ToString());
                        obj.NetTotal = dt.Rows[i]["NetTotal"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["NetTotal"].ToString());
                        obj.PaymentModeId = dt.Rows[i]["PaymentModeId"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["PaymentModeId"].ToString());
                        obj.MovementID = dt.Rows[i]["MovementID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["MovementID"].ToString());
                        obj.ParcelTypeId = dt.Rows[i]["ParcelTypeId"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["ParcelTypeId"].ToString());
                        obj.DocumentTypeId = dt.Rows[i]["DocumentTypeId"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["DocumentTypeId"].ToString());
                        obj.ProductTypeID = dt.Rows[i]["ProductTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["ProductTypeID"].ToString());
                        obj.PaymentModeText = dt.Rows[i]["PaymentModeText"].ToString();
                        obj.CargoDescription = dt.Rows[i]["CargoDescription"].ToString();
                        obj.Pieces = dt.Rows[i]["Pieces"] == DBNull.Value ? "" : dt.Rows[i]["Pieces"].ToString();
                        obj.Weight = dt.Rows[i]["Weight"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["Weight"].ToString());
                        obj.PickupRequestDate = dt.Rows[i]["PickupRequestDate"] == DBNull.Value ? "" : Convert.ToDateTime(dt.Rows[i]["PickupRequestDate"].ToString()).ToString("dd/MM/yyyy");
                        obj.AssignedEmployeeID = dt.Rows[i]["AssignedEmployeeID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["AssignedEmployeeID"].ToString());
                        obj.DepotReceivedBy = dt.Rows[i]["DepotReceivedBy"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["DepotReceivedBy"].ToString());
                        obj.PickedUpEmpID = dt.Rows[i]["PickedUpEmpID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["PickedUpEmpID"].ToString());
                        obj.PickedupDate = dt.Rows[i]["PickedupDate"] == DBNull.Value ? "" : Convert.ToDateTime(dt.Rows[i]["PickedupDate"].ToString()).ToString("dd/MM/yyyy");
                        obj.TaxPercent = dt.Rows[i]["TaxPercent"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["TaxPercent"].ToString());
                        obj.TaxAmount = dt.Rows[i]["TaxAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["TaxAmount"].ToString());
                        obj.SpecialInstructions = dt.Rows[i]["SpecialNotes"] == DBNull.Value ? "" : dt.Rows[i]["SpecialNotes"].ToString();
                        obj.CustomerRateTypeId = dt.Rows[i]["CustomerRateID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["CustomerRateID"].ToString());
                        obj.FAgentID = dt.Rows[i]["FAgentId"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["FAgentId"].ToString());
                        obj.FAgentName = dt.Rows[i]["FAgentName"] == DBNull.Value ? "" : dt.Rows[i]["FAgentName"].ToString();
                        obj.CustomerRateType = GetCustomerRateName(obj.CustomerRateTypeId);
                        obj.Remarks = dt.Rows[i]["Remarks"] == DBNull.Value ? "" : dt.Rows[i]["Remarks"].ToString();
                        list.Add(obj);
                    }

                }

            }


            return list;

        }
        public static string GenerateAWBBatchReport(int id)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();
            string batchno = "";
            string batchdate = "";

            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_AWBBatchPrintReport";
            comd.Parameters.AddWithValue("@Id", id);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "AWBRegister");

            //generate XSD to design report
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(Path.Combine(HostingEnvironment.MapPath("~/ReportsXSD"), "AWBBatchPrint.xsd"));
            //ds.WriteXmlSchema(writer);
            //writer.Close();



            ReportDocument rd = new ReportDocument();

            rd.Load(Path.Combine(HostingEnvironment.MapPath("~/Reports"), "AWBBatchPrint.rpt"));

            rd.SetDataSource(ds);

            if(ds.Tables.Count>0)
            {
                if(ds.Tables[0].Rows.Count>0)
                {
                    batchno = ds.Tables[0].Rows[0]["BATCHNUMBER"].ToString();
                    batchdate= ds.Tables[0].Rows[0]["BATCHDATE"].ToString();
                }
            }

     

            //Set Paramerter Field Values -General
            #region "param"
            string companyaddress = SourceMastersModel.GetCompanyAddress(branchid);
            string companyname = SourceMastersModel.GetCompanyname(branchid);

            string companylocation = SourceMastersModel.GetCompanyLocation(branchid);

            // Assign the params collection to the report viewer
            rd.ParameterFields["CompanyName"].CurrentValues.AddValue(companyname);
            rd.ParameterFields["CompanyAddress"].CurrentValues.AddValue(companyaddress);
         
            rd.ParameterFields["ReportTitle"].CurrentValues.AddValue("AWB BATCH PRINT");
            string period = "AWB Print";
            rd.ParameterFields["ReportPeriod"].CurrentValues.AddValue(period);

            string userdetail = "printed by " + SourceMastersModel.GetUserFullName(userid, usertype) + " on " + CommonFunctions.GetBranchDateTime();
            rd.ParameterFields["UserDetail"].CurrentValues.AddValue(userdetail);


              rd.ParameterFields["BatchNo"].CurrentValues.AddValue(batchno);
             rd.ParameterFields["BatchDate"].CurrentValues.AddValue(batchdate);
            #endregion

            //Response.Buffer = false;
            //Response.ClearContent();
            //Response.ClearHeaders();

            string reportname = "AWBBatchPrint_" + DateTime.Now.ToString("ddMMyyHHmmss") + ".pdf";
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
        public static List<AWBBatchList> GetAWBBatchList(int branchid,int FyearId, AWBBatchSearch paramobj,int UserID)
        {
           // AWBBatchSearch paramobj = (AWBBatchSearch)(HttpContext.Current.Session["AWBBatchSearch"]);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetAWBBatchList";
            cmd.CommandType = CommandType.StoredProcedure;

            if (paramobj.FromDate != null)
                cmd.Parameters.AddWithValue("@FromDate", Convert.ToDateTime(paramobj.FromDate).ToString("MM/dd/yyyy"));

            if (paramobj.ToDate != null)
                cmd.Parameters.AddWithValue("@ToDate", Convert.ToDateTime(paramobj.ToDate).ToString("MM/dd/yyyy"));

            cmd.Parameters.AddWithValue("@BranchID", branchid);
            cmd.Parameters.AddWithValue("@FYearId", FyearId);
            cmd.Parameters.AddWithValue("@CreatedBy", UserID);
            if (paramobj.DocumentNo == null)
                paramobj.DocumentNo = "";
            cmd.Parameters.AddWithValue("@AWBNo", paramobj.DocumentNo);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<AWBBatchList> objList = new List<AWBBatchList>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    AWBBatchList obj = new AWBBatchList();
                    obj.ID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ID"].ToString());
                    obj.BatchNumber = ds.Tables[0].Rows[i]["BatchNumber"].ToString();
                    obj.BatchDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["BatchDate"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.AWBNumbers = ds.Tables[0].Rows[i]["AWBNumbers"].ToString();
                    obj.InscanSheetNo = ds.Tables[0].Rows[i]["InscanSheetNo"].ToString();
                    obj.DRSNo = ds.Tables[0].Rows[i]["DRSNo"].ToString();
                    obj.CollectedBy = ds.Tables[0].Rows[i]["CollectedBy"].ToString();
                    obj.DeliveredBy = ds.Tables[0].Rows[i]["DeliveredBy"].ToString();
                    obj.CreatedByName = ds.Tables[0].Rows[i]["CreatedByName"].ToString();
                    obj.CreatedDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["CreatedDate"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.TotalAWB = Convert.ToInt32(ds.Tables[0].Rows[i]["TotalAWB"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static string GetMaxBathcNo(DateTime BatchDate,int BranchId ,int FYearId )
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetMaxAWBBatchNo";
            cmd.CommandType = CommandType.StoredProcedure;
            
            cmd.Parameters.AddWithValue("@BatchDate", Convert.ToDateTime(BatchDate).ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@FYearID", FYearId);
            cmd.Parameters.AddWithValue("@BranchID", BranchId);
            
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
            if (ds.Tables[0].Rows.Count > 0)
            {
                return ds.Tables[0].Rows[0][0].ToString();
            }
            else
            {
                return "";
            }

        }
        public static string SaveAWBBatch(int BATCHID, int BranchID, int AcCompanyID, int DepotID, int UserID, int FYearID, string Details)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_SaveAWBBatchNew";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BatchID", BATCHID);
                cmd.Parameters.AddWithValue("@BranchID", BranchID);
                cmd.Parameters.AddWithValue("@AcCompanyID", AcCompanyID);
                cmd.Parameters.AddWithValue("@DepotID", DepotID);
                cmd.Parameters.AddWithValue("@UserID", UserID);
                cmd.Parameters.AddWithValue("@FYearId", FYearID);
                cmd.Parameters.AddWithValue("@FormatForXMLitem", Details);
                
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    int inscanid = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                    PickupRequestDAO dao = new PickupRequestDAO();
                    string result2=dao.AWBAccountsPosting(inscanid);
                    
                    if (result2 == "OK")
                    {
                        return "Ok";
                    }
                    else
                    {
                        return result2;
                    }
                    
                }
                else
                {
                    return "No AWB added";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        public static string SaveAWBBatchTrackStatus1(int BATCHID, int BranchID, int AcCompanyID, int DepotID, int UserID, int FYearID)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_UpdateBatchAWBTrackStatus1";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BatchID", BATCHID);
                cmd.Parameters.AddWithValue("@BranchID", BranchID);
                cmd.Parameters.AddWithValue("@AcCompanyID", AcCompanyID);
                cmd.Parameters.AddWithValue("@DepotID", DepotID);
                cmd.Parameters.AddWithValue("@UserID", UserID);
                cmd.Parameters.AddWithValue("@FYearId", FYearID);


                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    return "Ok";
                }
                else
                {
                    return "No AWB added";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        public static string SaveAWBBatchTrackStatus(int BATCHID, int BranchID, int AcCompanyID, int DepotID, int UserID, int FYearID)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_UpdateBatchAWBTrackStatus";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BatchID", BATCHID);
                cmd.Parameters.AddWithValue("@BranchID", BranchID);
                cmd.Parameters.AddWithValue("@AcCompanyID", AcCompanyID);
                cmd.Parameters.AddWithValue("@DepotID", DepotID);
                cmd.Parameters.AddWithValue("@UserID", UserID);
                cmd.Parameters.AddWithValue("@FYearId", FYearID);
               

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    return "Ok";
                }
                else
                {
                    return "No AWB added";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        public static string SaveAWBBatchPosting(int BATCHID, int BranchID, int AcCompanyID,int FYearID)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_AWBBatchPosting";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BatchId", BATCHID);
                cmd.Parameters.AddWithValue("@FYearId", FYearID);
                cmd.Parameters.AddWithValue("@BranchId", BranchID);
                cmd.Parameters.AddWithValue("@CompanyID", AcCompanyID);
                
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    return "Ok";
                }
                else
                {
                    return "No AWB added";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        public static string SaveTranshipmentUpload(int ImportShipmentId, int BranchID, int AcCompanyID, int DepotID, int UserID, int FYearID, string Details)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_SaveTranshipmentUpload";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ImportShipmentID", ImportShipmentId);
                cmd.Parameters.AddWithValue("@BranchID", BranchID);
                cmd.Parameters.AddWithValue("@AcCompanyID", AcCompanyID);
                cmd.Parameters.AddWithValue("@DepotID", DepotID);
                cmd.Parameters.AddWithValue("@UserID", UserID);
                cmd.Parameters.AddWithValue("@FYearId", FYearID);
                cmd.Parameters.AddWithValue("@FormatForXMLitem", Details);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    return "Ok";
                }
                else
                {
                    return "No AWB added";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public static string UpdateAWBBatch(int BATCHID, int BranchID, int AcCompanyID, int DepotID, int UserID, int FYearID, string Details)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_UpdateAWBBatch";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BatchID", BATCHID);
                cmd.Parameters.AddWithValue("@BranchID", BranchID);
                cmd.Parameters.AddWithValue("@AcCompanyID", AcCompanyID);
                cmd.Parameters.AddWithValue("@DepotID", DepotID);
                cmd.Parameters.AddWithValue("@UserID", UserID);
                cmd.Parameters.AddWithValue("@FYearId", FYearID);
                cmd.Parameters.AddWithValue("@FormatForXMLitem", Details);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    return "Ok";
                }
                else
                {
                    return "No AWB added";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public static string GenerateAWBJobCode(DateTime BatchDate)
        {
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_UpdateBatchCode";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;
                        con.Open();
                        cmd.Parameters.AddWithValue("@BatchDate", BatchDate.ToString("MM/dd/yyyy"));
                        cmd.ExecuteNonQuery();

                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "OK";


        }


        public static string GenerateBatchforInscan(DateTime BatchDate,int QuickInscanId)
        {
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_GenerateBatchNoforInscan";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;
                        con.Open();
                        cmd.Parameters.AddWithValue("@BatchDate", BatchDate.ToString("MM/dd/yyyy"));
                        cmd.Parameters.AddWithValue("@QuickInscanId", QuickInscanId);
                        cmd.ExecuteNonQuery();

                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "OK";


        }

        public static List<CustomerRateType> GetRateList(int CustomerId, int MovementId, int ProductTypeId, int PaymentModeId, int FAgentID, string CityName, string CountryName)
        {
            List<CustomerRateType> list = new List<CustomerRateType>();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetCustomerRateList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
            cmd.Parameters.AddWithValue("@MovementId", MovementId);
            cmd.Parameters.AddWithValue("@ProductTypeId", ProductTypeId);
            cmd.Parameters.AddWithValue("@PaymentModeId", PaymentModeId);
            cmd.Parameters.AddWithValue("@FAgentId", FAgentID);
            if (CityName.Contains("'"))
            {
                CityName = CityName.Replace("'", "''");
            }
            cmd.Parameters.AddWithValue("@CityName", CityName);
            if (CountryName.Contains("’"))
            {
                CountryName = CountryName.Replace("’", "''");
            }
            else if (CountryName.Contains("'"))
            {
                CountryName = CountryName.Replace("'", "''");
            }
            cmd.Parameters.AddWithValue("@CountryName", CountryName);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            if (ds != null && ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        CustomerRateType obj = new CustomerRateType();
                        //obj.CustomerRateTypeID = CommonFunctions.ParseInt(dt.Rows[i]["CustomerRateTypeID"].ToString());
                        obj.CustomerRateTypeID = CommonFunctions.ParseInt(dt.Rows[i]["CustomerRateID"].ToString());
                        obj.CustomerRateType1 = dt.Rows[i]["CustomerRateType"].ToString();

                        list.Add(obj);
                    }

                }

            }


            return list;

        }

        public static FAgentRate GetFAgentRate(int MovementId, int ProductTypeId, int FAgentID, string CountryName,decimal Weight)
        {
            FAgentRate list = new FAgentRate();
            FAgentRate obj = new FAgentRate();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_GetAWBFwdAgentRate";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@InscanId", 0);
                cmd.Parameters.AddWithValue("@pFAgentId", FAgentID);
                cmd.Parameters.AddWithValue("@pProductTypeID1", ProductTypeId);
                cmd.Parameters.AddWithValue("@pConsigneeContryName", CountryName);
                cmd.Parameters.AddWithValue("@pWeight", Weight);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
            
                if (ds != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        DataTable dt = ds.Tables[0];
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {

                            //obj.CustomerRateTypeID = CommonFunctions.ParseInt(dt.Rows[i]["CustomerRateTypeID"].ToString());
                            obj.FAgentID = CommonFunctions.ParseInt(dt.Rows[i]["FAgentID"].ToString());
                            obj.Rate = Convert.ToDecimal(dt.Rows[i]["Rate"].ToString());
                            obj.FAgentRateId = CommonFunctions.ParseInt(dt.Rows[i]["FAgentRateId"].ToString());


                        }

                    }

                }


                return obj;
            }
            catch(Exception ex)
            {
                return obj;
            }

        }

        public static List<CustomerRateVM> GetAllRateList(int CustomerId)
        {
            List<CustomerRateVM> list = new List<CustomerRateVM>();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetAllCustomerRate";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
            

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            if (ds != null && ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        CustomerRateVM obj = new CustomerRateVM();
                        obj.CourierServiceID = CommonFunctions.ParseInt(dt.Rows[i]["CourierServiceID"].ToString());
                        obj.MovementID = CommonFunctions.ParseInt(dt.Rows[i]["MovementID"].ToString());
                        obj.FAgentID = CommonFunctions.ParseInt(dt.Rows[i]["FAgentID"].ToString());
                        obj.CustomerRateID = CommonFunctions.ParseInt(dt.Rows[i]["CustomerRateID"].ToString());
                        obj.CustomerRateType = dt.Rows[i]["CustomerRateType"].ToString();
                        obj.CountryName = dt.Rows[i]["CountryName"].ToString();
                        obj.CityName = dt.Rows[i]["CityName"].ToString();
                        list.Add(obj);
                    }

                }

            }


            return list;

        }


        public static CustomerRateTypeVM GetCourierCharge(int RateTypeId, int CustomerId, int MovementId, int ProductTypeId, int PaymentModeId, decimal Weight, string CountryName, string CityName)
        {
            decimal CourierCharge = 0;
            CustomerRateTypeVM vm = new CustomerRateTypeVM();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetCourierCharge";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@CustomerRateId", RateTypeId);
            // cmd.Parameters.AddWithValue("@CustomerRateTypeId", RateTypeId);
            cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
            cmd.Parameters.AddWithValue("@MovementId", MovementId);
            cmd.Parameters.AddWithValue("@ProductTypeId", ProductTypeId);
            cmd.Parameters.AddWithValue("@PaymentModeId", PaymentModeId);
            cmd.Parameters.AddWithValue("@Weight", Weight);
            cmd.Parameters.AddWithValue("@CountryName", CountryName);
            cmd.Parameters.AddWithValue("@CityName", CityName);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            if (ds != null && ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        vm.CustomerRateType = dt.Rows[i]["CustomerRateType"].ToString();
                        vm.CustomerRateTypeID = Convert.ToInt32(dt.Rows[i]["CustomerRateTypeId"].ToString());
                        vm.CourierCharge = Convert.ToDecimal(dt.Rows[i]["CourierCharge"].ToString());


                    }

                }

            }


            return vm;

        }
        public static string GetCustomerRateName(int CustomerRateID)
        {
            string CustomerRateName = "";

            //SP_GetCustomerRateDetail
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetCustomerRateDetail";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@CustomerRateId", CustomerRateID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            if (ds != null && ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    CustomerRateName = dt.Rows[0]["CustomerRateType"].ToString();
                }
            }

            return CustomerRateName;
        }


        public static List<ZoneNameVM> GetZoneChartMaster(int RateTypeId,int ZoneChartID=0)
        {

            List<ZoneNameVM> vm = new List<ZoneNameVM>();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetZoneChartByCustomer";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@CustomerRateTypeId", RateTypeId);
            cmd.Parameters.AddWithValue("@ZoneChartID", ZoneChartID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            if (ds != null && ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        ZoneNameVM obj = new ZoneNameVM();
                        obj.ZoneID = Convert.ToInt32(dt.Rows[i]["ZoneChartID"].ToString());
                        //obj.MovementID = Convert.ToInt32(dt.Rows[i]["MovementId"].ToString());
                        obj.ZoneName = dt.Rows[i]["ZoneName"].ToString();
                        obj.ZoneType = dt.Rows[i]["ZoneType"].ToString();
                        obj.Countries = dt.Rows[i]["Countries"].ToString();
                        obj.Cities = dt.Rows[i]["Cities"].ToString();
                         
                        vm.Add(obj);

                    }

                }

            }


            return vm;

        }

        //For forwarding agent rate
        public static List<ZoneNameVM> GetZoneChartList()
        {

            List<ZoneNameVM> vm = new List<ZoneNameVM>();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetZoneChart";
            cmd.CommandType = CommandType.StoredProcedure;          

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            if (ds != null && ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        ZoneNameVM obj = new ZoneNameVM();
                        obj.ZoneID = Convert.ToInt32(dt.Rows[i]["ZoneChartID"].ToString());                      
                        obj.ZoneName = dt.Rows[i]["ZoneName"].ToString();
                        vm.Add(obj);

                    }

                }

            }


            return vm;

        }

        public static List<FAgentRateVM> GetForwardingAgentRateList()
        {

            List<FAgentRateVM> vm = new List<FAgentRateVM>();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetForwardingAgentRateList";
            cmd.CommandType = CommandType.StoredProcedure;

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            if (ds != null && ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        FAgentRateVM obj = new FAgentRateVM();
                        obj.FAgentRateID = Convert.ToInt32(dt.Rows[i]["FAgentRateID"].ToString());
                        obj.Fname = dt.Rows[i]["FAgentName"].ToString();
                        obj.CourierService = dt.Rows[i]["ProductName"].ToString();
                        obj.ZoneChartName = dt.Rows[i]["ZoneName"].ToString();
                        vm.Add(obj);

                    }

                }

            }


            return vm;

        }


        //upload csv extracted to inboundshipment model awb
        public static List<AWBBatchDetail> GetDomesticShipmentValidAWBDetails(int BranchID, string Details, string AWBNo, int CustomerID)
        {
            List<AWBBatchDetail> list = new List<AWBBatchDetail>();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_GetDomesticsShipmentValidAWBDetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BranchID", BranchID);

                cmd.Parameters.AddWithValue("@FormatForXMLitem", Details);
                cmd.Parameters.AddWithValue("@CustomerID", CustomerID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);


                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        AWBBatchDetail obj = new AWBBatchDetail();

                        obj.InScanID = 0;
                        obj.SNo = i + 1;
                        obj.AWBNo = dt.Rows[i]["AWBNo"].ToString();
                        obj.AWBDate = Convert.ToDateTime(dt.Rows[i]["AWBDate"].ToString());
                        obj.CustomerID = Convert.ToInt32(dt.Rows[i]["CustomerID"].ToString());
                        obj.CustomerName = dt.Rows[i]["CustomerName"].ToString();
                        obj.Consignor = dt.Rows[i]["Consignor"].ToString();
                        
                        obj.ConsignorContact = dt.Rows[i]["ConsignorContact"].ToString();
                        obj.ConsignorAddress1_Building = dt.Rows[i]["ConsignorAddress1"].ToString();
                        obj.ConsignorAddress2_Street = dt.Rows[i]["ConsignorAddress2"].ToString();
                        obj.ConsignorAddress3_PinCode = dt.Rows[i]["ConsignorAddress3"].ToString();
                        obj.ConsignorPhone = dt.Rows[i]["ConsignorPhone"].ToString();
                        obj.ConsignorMobileNo = dt.Rows[i]["ConsignorMobileNo"].ToString();
                        obj.ConsignorLocationName = dt.Rows[i]["ConsignorLocationName"].ToString();
                        obj.ConsignorCityName = dt.Rows[i]["ConsignorCityName"].ToString();
                        obj.ConsignorCountryName = dt.Rows[i]["ConsignorCountryName"].ToString();
                        obj.Consignee = dt.Rows[i]["Consignee"].ToString();
                        obj.ConsigneeContact = dt.Rows[i]["ConsigneeContact"].ToString();
                        obj.ConsigneeAddress1_Building = dt.Rows[i]["ConsigneeAddress1"].ToString();
                        obj.ConsigneeAddress2_Street = dt.Rows[i]["ConsigneeAddress2"].ToString();
                        obj.ConsigneeAddress3_PinCode = dt.Rows[i]["ConsigneeAddress3"].ToString();
                        obj.ConsigneeLocationName = dt.Rows[i]["ConsigneeLocationName"].ToString();
                        obj.ConsigneeCityName = dt.Rows[i]["ConsigneeCityName"].ToString();
                        obj.ConsigneeCountryName = dt.Rows[i]["ConsigneeCountryName"].ToString();
                        obj.ConsigneePhone = dt.Rows[i]["ConsigneePhone"].ToString();
                        obj.ConsigneeMobileNo = dt.Rows[i]["ConsigneeMobileNo"].ToString();
                        obj.PaymentModeId = Convert.ToInt32(dt.Rows[i]["PaymentModeId"].ToString());
                        obj.ParcelTypeId = Convert.ToInt32(dt.Rows[i]["ParcelTypeID"].ToString());
                        obj.MovementID = Convert.ToInt32(dt.Rows[i]["MovementID"].ToString());
                        obj.ProductTypeID = Convert.ToInt32(dt.Rows[i]["ProductTypeID"].ToString());
                        obj.ParcelType = dt.Rows[i]["ParcelType"].ToString();
                        obj.MovementType = dt.Rows[i]["MovementType"].ToString();
                        obj.ProductType = dt.Rows[i]["ProductType"].ToString();
                        obj.MaterialCost = Convert.ToDecimal(dt.Rows[i]["MaterialCost"].ToString());
                        obj.Pieces = dt.Rows[i]["Pieces"].ToString();
                        obj.Weight = CommonFunctions.ParseDecimal(dt.Rows[i]["Weight"].ToString());
                        obj.CourierCharge = CommonFunctions.ParseDecimal(dt.Rows[i]["CourierCharge"].ToString());
                        obj.OtherCharge = Convert.ToDecimal(dt.Rows[i]["OtherCharge"].ToString());
                        obj.NetTotal = Convert.ToDecimal(dt.Rows[i]["NetTotal"].ToString());
                        //obj.BagNo = dt.Rows[i]["BagNo"].ToString();
                        obj.CargoDescription = dt.Rows[i]["CargoDescription"].ToString();
                        obj.Remarks = dt.Rows[i]["Remarks"].ToString();
                        //obj.MAWB = dt.Rows[i]["MAWB"].ToString();
                        obj.EntrySource = 3; // "EXL";
                        obj.CurrencyID = Convert.ToInt32(dt.Rows[i]["CurrencyID"].ToString());
                        obj.Currency = dt.Rows[i]["Currency"].ToString();
                        obj.CustomsValue = Convert.ToDecimal(dt.Rows[i]["CustomsValue"].ToString());
                        //obj.CourierStatusID = Convert.ToInt32(dt.Rows[i]["CourierStatusID"].ToString());
                        //obj.StatusTypeId = Convert.ToInt32(dt.Rows[i]["StatusTypeId"].ToString()); 
                        
                        list.Add(obj);
                    }

                }
                else
                {
                    return list;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return list;
        }
        public static SaveStatusModel SaveDomesticAWBBatch(int BATCHID, int BranchID, int AcCompanyID, int UserID, int FYearID, string Details)
        {
            SaveStatusModel status = new SaveStatusModel();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_SaveDomesticShipmentBatch";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BatchID", BATCHID);
                cmd.Parameters.AddWithValue("@BranchID", BranchID);
                cmd.Parameters.AddWithValue("@AcCompanyID", AcCompanyID);
                cmd.Parameters.AddWithValue("@UserID", UserID);
                cmd.Parameters.AddWithValue("@FYearId", FYearID);
                cmd.Parameters.AddWithValue("@FormatForXMLitem", Details);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    status.Status = ds.Tables[0].Rows[0]["Status"].ToString();
                    status.Message = ds.Tables[0].Rows[0]["Message"].ToString();
                    status.TotalImportCount = Convert.ToInt32(ds.Tables[0].Rows[0]["TotalImportCount"].ToString());
                    status.TotalSavedCount = Convert.ToInt32(ds.Tables[0].Rows[0]["TotalSaved"].ToString());
                    //PickupRequestDAO dao = new PickupRequestDAO();
                    //string result2 = dao.AWBAccountsPosting(inscanid);
                    if (status.Status == "OK")
                        SaveDomesticAWBBatchPosting(BATCHID);

                    return status;

                }
                else
                {
                    status.Status = "Failed";
                    status.Message = "Saved Failed";
                    return status;
                }
            }
            catch (Exception ex)
            {
                status.Status = "Failed";
                status.Message = ex.Message;
                return status;
            }

        }
        public static SaveStatusModel SaveDomesticAWBBatchPosting(int BATCHID)
        {
            SaveStatusModel status = new SaveStatusModel();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_SaveDomesticBatchShipmentPosting";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BatchID", BATCHID);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                status.Status = "OK";



                return status;


            }
            catch (Exception ex)
            {
                status.Status = "Failed";
                status.Message = ex.Message;
                return status;
            }

        }
        #endregion

        #region "StockItem"
        public static List<StockVM> GetStockList(int branchId, int fyearid)
        {
            StockSearch paramobj = (StockSearch)(HttpContext.Current.Session["StockSearch"]);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetStockList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FYearId", fyearid);
            cmd.Parameters.AddWithValue("@BranchId", branchId);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<StockVM> objList = new List<StockVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    StockVM obj = new StockVM();
                    obj.ID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ID"].ToString());
                    obj.PurchaseDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["PurchaseDate"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.ReferenceNo = ds.Tables[0].Rows[i]["ReferenceNo"].ToString();
                    obj.AWBCount = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AWBCount"].ToString());
                    obj.AWBNOFrom = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AWBNOFrom"].ToString());
                    obj.AWBNOTo = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AWBNOTo"].ToString());
                    obj.Qty = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["Qty"].ToString());
                    obj.Rate = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Rate"].ToString());
                    obj.Amount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Amount"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }
        #endregion

        #region "POD"
        public static PODImage GetPODImage(int PODID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "Select * From POD where PODId=" + PODID.ToString();
            cmd.CommandType = CommandType.Text;
            PODImage _podimage = new PODImage();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<StockVM> objList = new List<StockVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    _podimage.id = CommonFunctions.ParseInt(ds.Tables[0].Rows[0]["id"].ToString());
                    _podimage.PODID = CommonFunctions.ParseInt(ds.Tables[0].Rows[0]["PODID"].ToString());
                    // _podimage.image = Convert.ToSByte(ds.Tables[0].Rows[0]["PODID"].ToString());
                }
            }
            return _podimage;
        }

        public static List<DRSVM> GetPODDRSDetails(int DeliveredBy,DateTime FromDate,DateTime ToDate,string term)
        {
            List<DRSVM> list = new List<DRSVM>();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_GetPODDRSList";
                cmd.CommandType = CommandType.StoredProcedure;
                
                cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
                cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
                cmd.Parameters.AddWithValue("@DeliveredBy", DeliveredBy);
                cmd.Parameters.AddWithValue("@term",term);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);


                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        DRSVM obj = new DRSVM();
                        obj.DRSID = CommonFunctions.ParseInt(dt.Rows[i]["DRSID"].ToString());
                        obj.DRSNo = dt.Rows[i]["DRSNo"].ToString();
                        obj.DRSDate = Convert.ToDateTime(dt.Rows[i]["DRSDate"].ToString());
                        list.Add(obj);
                    }

                }
                else
                {
                    return list;
                }
            }
            catch (Exception ex)
            {
                return list;
            }

            return list;
        }
        public static List<QuickAWBVM> GetPODDRSAWBDetails(int DeliveredBy,int DRSID, DateTime FromDate, DateTime ToDate, string term)
        {
            List<QuickAWBVM> list = new List<QuickAWBVM>();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_GetPODDRSAWBList";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
                cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
                cmd.Parameters.AddWithValue("@DeliveredBy", DeliveredBy);
                cmd.Parameters.AddWithValue("@DRSID", DRSID);                
                cmd.Parameters.AddWithValue("@term", term);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);


                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        QuickAWBVM obj = new QuickAWBVM();
                        obj.InScanID = CommonFunctions.ParseInt(dt.Rows[i]["InScanID"].ToString());
                        obj.HAWBNo = dt.Rows[i]["AWBNo"].ToString();
                        obj.TransactionDate = Convert.ToDateTime(dt.Rows[i]["TransactionDate"].ToString());
                        list.Add(obj);
                    }

                }
                else
                {
                    return list;
                }
            }
            catch (Exception ex)
            {
                return list;
            }

            return list;
        }
        public static List<AWBInfor> GetAWBInformation(string AWBNo ,int BranchID ,int FyearID)
            {
            List<AWBInfor> list = new List<AWBInfor>();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_GetAWBInfo";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@AWBNo", AWBNo);
                cmd.Parameters.AddWithValue("@FYearId", FyearID);
                cmd.Parameters.AddWithValue("@BranchID",BranchID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);


                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        AWBInfor obj = new AWBInfor();
                        obj.InScanID = CommonFunctions.ParseInt(dt.Rows[i]["InScanID"].ToString());
                        obj.ShipmentID = CommonFunctions.ParseInt(dt.Rows[i]["ShipmentID"].ToString());
                        obj.AWBNo = dt.Rows[i]["AWBNo"].ToString();
                        obj.AWBDate = Convert.ToDateTime(dt.Rows[i]["TransactionDate"].ToString());
                        obj.Consignor = dt.Rows[i]["Consignor"].ToString();
                        obj.ConsignorCountry = dt.Rows[i]["ConsignorCountryName"].ToString();
                        obj.ConsignorCity = dt.Rows[i]["ConsignorCityName"].ToString();
                        obj.Consignee = dt.Rows[i]["Consignee"].ToString();
                        obj.ConsigneeCountry = dt.Rows[i]["ConsigneeCountryName"].ToString();
                        obj.ConsigneeCity = dt.Rows[i]["ConsigneeCityName"].ToString();
                        obj.CourierStatusId = CommonFunctions.ParseInt(dt.Rows[i]["CourierStatusId"].ToString());
                        obj.CourierStatus = dt.Rows[i]["CourierStatus"].ToString();
                        obj.AWBType = dt.Rows[i]["AWBType"].ToString();
                        list.Add(obj);
                    }

                }
                else
                {
                    return list;
                }
            }
            catch (Exception ex)
            {
                return list;
            }

            return list;
        }

        //AWB Timeline
        public static AWBTracking GetAWBTimelineInformation(string AWBNo, int BranchID, int FyearID)
        {
            AWBTracking vm = new AWBTracking();
            QuickAWBVM awbvm = new QuickAWBVM();

            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_GETAWBTimeLine";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@AWBNo", AWBNo);
                cmd.Parameters.AddWithValue("@FYearId", FyearID);
                cmd.Parameters.AddWithValue("@BranchID", BranchID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);


                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {

                        awbvm.InScanID = CommonFunctions.ParseInt(dt.Rows[i]["InScanID"].ToString());
                        awbvm.InboundShipmentID = CommonFunctions.ParseInt(dt.Rows[i]["ShipmentID"].ToString());
                        awbvm.HAWBNo = dt.Rows[i]["AWBNo"].ToString();
                        awbvm.TransactionDate = Convert.ToDateTime(dt.Rows[i]["TransactionDate"].ToString());
                        awbvm.Consignor = dt.Rows[i]["Consignor"].ToString();
                        awbvm.Consignee = dt.Rows[i]["Consignee"].ToString();
                        awbvm.ConsignorCountryName = dt.Rows[i]["ConsignorCountryName"].ToString();
                        awbvm.ConsignorCityName = dt.Rows[i]["ConsignorCityName"].ToString();
                        awbvm.ConsigneeCountryName = dt.Rows[i]["ConsigneeCountryName"].ToString();
                        awbvm.ConsigneeCityName = dt.Rows[i]["ConsigneeCityName"].ToString();
                        awbvm.CourierStatusId = CommonFunctions.ParseInt(dt.Rows[i]["CourierStatusId"].ToString());
                        awbvm.CourierStatus = dt.Rows[i]["CourierStatus"].ToString();
                        awbvm.ParcelType = dt.Rows[i]["ParcelType"].ToString();
                        awbvm.paymentmode = dt.Rows[i]["PaymentModeText"].ToString();
                        awbvm.ProductName = dt.Rows[i]["ProductType"].ToString();

                    }

                }
                vm.AWB = awbvm;
                
            }
            catch (Exception ex)
            {
                return vm;
            }

            return vm;
        }

        public static SaveStatusModel SaveMultipleAWBStatus(int  EmployeeID,DateTime EntryDate,int CourierStatusId ,string Remarks, int BranchID,  int UserID, int FYearID, string Details)
        {
            SaveStatusModel status = new SaveStatusModel();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_SaveMultipleShipmentStatus";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EmployeeID", EmployeeID);
                cmd.Parameters.AddWithValue("@EntryDate", EntryDate.ToString("MM/dd/yyyy HH:mm"));
                cmd.Parameters.AddWithValue("@CourierStatusId", CourierStatusId);
                cmd.Parameters.AddWithValue("@Remarks", Remarks);
                cmd.Parameters.AddWithValue("@BranchID", BranchID);                 
                cmd.Parameters.AddWithValue("@UserID", UserID);
                cmd.Parameters.AddWithValue("@FYearId", FYearID);
                cmd.Parameters.AddWithValue("@FormatForXMLitem", Details);
                
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    status.Status = ds.Tables[0].Rows[0]["Status"].ToString();
                    status.Message = ds.Tables[0].Rows[0]["Message"].ToString();
                   
                  
                    return status;

                }
                else
                {
                    status.Status = "Failed";
                    status.Message = "Saved Failed";
                    return status;
                }
            }
            catch (Exception ex)
            {
                status.Status = "Failed";
                status.Message = ex.Message;
                return status;
            }

        }
        public static List<DRSDet> GetPODAWBDetails(int BranchID, int DRSID, string AWBNo, int DeliveredBy)
        {
            List<DRSDet> list = new List<DRSDet>();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_GetPODAWBDetails";
                cmd.CommandType = CommandType.StoredProcedure;
                
                cmd.Parameters.AddWithValue("@AWBNo", @AWBNo);
                cmd.Parameters.AddWithValue("@DRSID", DRSID);
                cmd.Parameters.AddWithValue("@BranchID", BranchID);
                cmd.Parameters.AddWithValue("@DeliveredBy", DeliveredBy);


                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);


                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        DRSDet obj = new DRSDet();
                        obj.InScanID = CommonFunctions.ParseInt(dt.Rows[i]["InScanID"].ToString());
                        obj.ShipmentDetailID = CommonFunctions.ParseInt(dt.Rows[i]["ShipmentDetailID"].ToString());
                        obj.AWB = dt.Rows[i]["AWB"].ToString();
                        obj.AWBDate = Convert.ToDateTime(dt.Rows[i]["AWBDate"].ToString());
                        obj.consignor = dt.Rows[i]["consignor"].ToString();
                        obj.consignee = dt.Rows[i]["consignee"].ToString();
                        obj.city = dt.Rows[i]["city"].ToString();
                        obj.address = dt.Rows[i]["address"].ToString();
                        obj.phone = dt.Rows[i]["phone"].ToString();
                        obj.COD = CommonFunctions.ParseDecimal(dt.Rows[i]["COD"].ToString());
                        obj.MaterialCost = CommonFunctions.ParseDecimal(dt.Rows[i]["MaterialCost"].ToString());
                        obj.CourierStatus = dt.Rows[i]["CourierStatus"].ToString();
                        obj.PayMode = dt.Rows[i]["PaymentMode"].ToString();
                        obj.deleted = false;
                        list.Add(obj);
                    }

                }
                else
                {
                    return list;
                }
            }
            catch (Exception ex)
            {
                return list;
            }

            return list;
        }


        public static AWBManifest GetShipmentManifestInformation(int InboundShipmentId, int InscanId, int? ExportId)
        {
            AWBManifest obj = new AWBManifest();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_GetShipmentManifest";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@InboundShipmentID", InboundShipmentId);
                cmd.Parameters.AddWithValue("@InScanID", InscanId);
                cmd.Parameters.AddWithValue("@ExportId", Convert.ToInt32(ExportId));

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);


                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                   
                      
                        obj.ExportID = CommonFunctions.ParseInt(dt.Rows[0]["ID"].ToString());
                      
                        obj.ManifestNumber = dt.Rows[0]["ManifestNumber"].ToString();                       
                        obj.ForwardingAgentName = dt.Rows[0]["ForwardingAgentName"].ToString();
                        obj.DeliveryAgentName = dt.Rows[0]["DeliveryAgentName"].ToString();
                        obj.FwdAgentAWBNo = dt.Rows[0]["FwdAgentAWBNo"].ToString();


                    return obj;

                }
                else
                {
                    return obj;
                }
            }
            catch (Exception ex)
            {
                return obj;
            }

            return obj;
        }
        #endregion

        #region "OutScan"

        //edit mode to bind items
        public static List<DRSDet> GetDRSAWBDetails(int DRSID)
        {
            List<DRSDet> list = new List<DRSDet>();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_GetDRSAWBDetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@DRSID", DRSID);                

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);


                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        DRSDet obj = new DRSDet();
                        obj.SNo = i + 1;
                        obj.InScanID = CommonFunctions.ParseInt(dt.Rows[i]["InScanID"].ToString());
                        obj.ShipmentDetailID = CommonFunctions.ParseInt(dt.Rows[i]["ShipmentDetailID"].ToString());
                        obj.AWB = dt.Rows[i]["AWB"].ToString();
                        obj.AWBDate = Convert.ToDateTime(dt.Rows[i]["AWBDate"].ToString());
                        obj.consignor = dt.Rows[i]["consignor"].ToString();
                        obj.consignee = dt.Rows[i]["consignee"].ToString();
                        obj.city = dt.Rows[i]["city"].ToString();
                        obj.address = dt.Rows[i]["address"].ToString();
                        obj.phone = dt.Rows[i]["phone"].ToString();
                        obj.COD = CommonFunctions.ParseDecimal(dt.Rows[i]["COD"].ToString());
                        obj.MaterialCost = CommonFunctions.ParseDecimal(dt.Rows[i]["MaterialCost"].ToString());
                        obj.CourierStatus = dt.Rows[i]["CourierStatus"].ToString();
                        obj.PayMode = dt.Rows[i]["PaymentMode"].ToString();
                        obj.deleted = false;
                        list.Add(obj);
                    }

                }
                else
                {
                    return list;
                }
            }
            catch (Exception ex)
            {
                return list;
            }

            return list;
        }
        
        //create mode to get awb detail
        public static List<DRSDet> GetOutscanAWBDetails(int BranchID, string Details,string AWBNo,int DeliveredBy) 
        {
            List<DRSDet> list = new List<DRSDet>();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_GetOutScanAWBDetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Parameters.AddWithValue("@BranchID", BranchID);
                cmd.Parameters.AddWithValue("@AWBNo", @AWBNo);                    
                cmd.Parameters.AddWithValue("@FormatForXMLitem", Details);
                cmd.Parameters.AddWithValue("@DeliveredBy", DeliveredBy);
                

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                

                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        DRSDet obj = new DRSDet();
                        obj.InScanID = CommonFunctions.ParseInt(dt.Rows[i]["InScanID"].ToString());
                        obj.SNo = i + 1;
                        obj.AWB = dt.Rows[i]["AWB"].ToString();
                        obj.AWBDate = Convert.ToDateTime(dt.Rows[i]["AWBDate"].ToString());
                        obj.consignor = dt.Rows[i]["consignor"].ToString();
                        obj.consignee = dt.Rows[i]["consignee"].ToString();
                        obj.city = dt.Rows[i]["city"].ToString();
                        obj.address = dt.Rows[i]["address"].ToString();
                        obj.phone = dt.Rows[i]["phone"].ToString();
                        obj.COD = CommonFunctions.ParseDecimal(dt.Rows[i]["COD"].ToString());
                        obj.MaterialCost = CommonFunctions.ParseDecimal(dt.Rows[i]["MaterialCost"].ToString());
                        obj.CourierStatus= dt.Rows[i]["CourierStatus"].ToString();
                        obj.PayMode = dt.Rows[i]["PaymentMode"].ToString();
                        obj.deleted = false;
                        list.Add(obj);
                    }

                }
                else
                {
                    return list;
                }
            }
            catch (Exception ex)
            {
                return list;
            }

            return list;
        }

        public static string SaveOutScan(int BranchID, int DRSID, int UserID, string Details)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_SAVEOutScanAWBDetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Parameters.AddWithValue("@BranchID", BranchID);
                cmd.Parameters.AddWithValue("@DRSID", DRSID);
                cmd.Parameters.AddWithValue("@FormatForXMLitem", Details);
                cmd.Parameters.AddWithValue("@UserID", UserID);                
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();                
                
                return "Ok";
                
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        #endregion

        #region userregistration
        public static string SaveUserRegistration(UserRegistration obj)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_SaveUserRegistration";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@UserID", obj.UserID);
            cmd.Parameters.AddWithValue("@UserName", obj.UserName);
            cmd.Parameters.AddWithValue("@Password", obj.Password);
            cmd.Parameters.AddWithValue("@EmailId", obj.EmailId);
            cmd.Parameters.AddWithValue("@IsActive", obj.IsActive);
            cmd.Parameters.AddWithValue("@RoleID", obj.RoleID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
            if (ds.Tables[0].Rows.Count > 0)
            {
                return ds.Tables[0].Rows[0][0].ToString();
            }
            else
            {
                return "";
            }

        }
        #endregion

        #region awbimport
        public static List<AWBBatchDetail> GetShipmentValidAWBDetails(int BranchID, string Details, string AWBNo)
        {
            List<AWBBatchDetail> list = new List<AWBBatchDetail>();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_GetShipmentValidAWBDetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BranchID", BranchID);
               

                cmd.Parameters.AddWithValue("@FormatForXMLitem", Details);
           

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);


                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        AWBBatchDetail obj = new AWBBatchDetail();

                        obj.InScanID = 0;
                        obj.SNo = i + 1;
                        obj.AWBNo = dt.Rows[i]["AWBNo"].ToString();
                        obj.AWBDate = Convert.ToDateTime(dt.Rows[i]["AWBDate"].ToString());
                        obj.CustomerID = Convert.ToInt32(dt.Rows[i]["CustomerID"].ToString());
                        obj.Consignor = dt.Rows[i]["Consignor"].ToString();
                        obj.ConsignorContact = dt.Rows[i]["ConsignorContact"].ToString();
                        obj.ConsignorAddress1_Building = dt.Rows[i]["ConsignorAddress1"].ToString();
                        obj.ConsignorAddress2_Street = dt.Rows[i]["ConsignorAddress2"].ToString();
                        obj.ConsignorAddress3_PinCode = dt.Rows[i]["ConsignorAddress3"].ToString();
                        obj.ConsignorPhone = dt.Rows[i]["ConsignorPhone"].ToString();
                        obj.ConsignorMobileNo = dt.Rows[i]["ConsignorMobileNo"].ToString();
                        obj.ConsignorLocationName = dt.Rows[i]["ConsignorLocationName"].ToString();
                        obj.ConsignorCityName = dt.Rows[i]["ConsignorCityName"].ToString();
                        obj.ConsignorCountryName = dt.Rows[i]["ConsignorCountryName"].ToString();
                        obj.Consignee = dt.Rows[i]["Consignee"].ToString();
                        obj.ConsigneeContact = dt.Rows[i]["ConsigneeContact"].ToString();
                        obj.ConsigneeAddress1_Building = dt.Rows[i]["ConsigneeAddress1"].ToString();
                        obj.ConsigneeAddress2_Street = dt.Rows[i]["ConsigneeAddress2"].ToString();
                        obj.ConsigneeAddress3_PinCode = dt.Rows[i]["ConsigneeAddress3"].ToString();
                        obj.ConsigneeLocationName = dt.Rows[i]["ConsigneeLocationName"].ToString();
                        obj.ConsigneeCityName = dt.Rows[i]["ConsigneeCityName"].ToString();
                        obj.ConsigneeCountryName = dt.Rows[i]["ConsigneeCountryName"].ToString();
                        obj.ConsigneePhone = dt.Rows[i]["ConsigneePhone"].ToString();
                        obj.ConsigneeMobileNo = dt.Rows[i]["ConsigneeMobileNo"].ToString();
                        obj.PaymentModeId = Convert.ToInt32(dt.Rows[i]["PaymentModeId"].ToString());
                        obj.ParcelTypeId = Convert.ToInt32(dt.Rows[i]["ParcelTypeID"].ToString());
                        obj.MovementID = Convert.ToInt32(dt.Rows[i]["MovementID"].ToString());
                        obj.ProductTypeID = Convert.ToInt32(dt.Rows[i]["ProductTypeID"].ToString());
                        obj.ParcelType = dt.Rows[i]["ParcelType"].ToString();
                        obj.MovementType = dt.Rows[i]["MovementType"].ToString();
                        obj.ProductType = dt.Rows[i]["ProductType"].ToString();
                        obj.MaterialCost = Convert.ToDecimal(dt.Rows[i]["MaterialCost"].ToString());
                        obj.Pieces = dt.Rows[i]["Pieces"].ToString();
                        obj.Weight = CommonFunctions.ParseDecimal(dt.Rows[i]["Weight"].ToString());
                        obj.CourierCharge = CommonFunctions.ParseDecimal(dt.Rows[i]["CourierCharge"].ToString());
                        obj.OtherCharge = Convert.ToDecimal(dt.Rows[i]["OtherCharge"].ToString());
                        obj.NetTotal = Convert.ToDecimal(dt.Rows[i]["NetTotal"].ToString());
                        //obj.BagNo = dt.Rows[i]["BagNo"].ToString();
                        obj.CargoDescription = dt.Rows[i]["CargoDescription"].ToString();
                        obj.Remarks = dt.Rows[i]["Remarks"].ToString();
                      
                        obj.EntrySource =3;
                        obj.CurrencyID = Convert.ToInt32(dt.Rows[i]["CurrencyID"].ToString());
                        obj.Currency = dt.Rows[i]["Currency"].ToString();
                        obj.CustomsValue = Convert.ToDecimal(dt.Rows[i]["CustomsValue"].ToString());
                        //obj.CourierStatusID = Convert.ToInt32(dt.Rows[i]["CourierStatusID"].ToString());
                        //obj.StatusTypeId = Convert.ToInt32(dt.Rows[i]["StatusTypeId"].ToString()); 
                        obj.CurrencyID = Convert.ToInt32(dt.Rows[i]["CurrencyID"].ToString());
                        obj.CustomerID = Convert.ToInt32(dt.Rows[i]["CustomerID"].ToString());
                        //obj.CustomerID = -2;
                        //obj.FAgentName = dt.Rows[i]["FwdNo"].ToString();
                        
                        obj.FAgentID = CommonFunctions.ParseInt(dt.Rows[i]["FAgentID"].ToString());
                        //obj.AWBValidStatus = Convert.ToBoolean(dt.Rows[i]["AWBValidStatus"].ToString());
                        //if (obj.AWBValidStatus == false)
                        //{
                        //    obj.AwbValidStatusclass = "awbvalidfalse";
                        //}

                        //else
                        //{
                        //    obj.AwbValidStatusclass = "";
                        //}
                        list.Add(obj);
                    }

                }
                else
                {
                    return list;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return list;
        }

        public static List<AWBBatchDetail> GetCustomerShipmentValidAWBDetails(int BranchID,int CustomerID,int CourierStatusID, string Details, string AWBNo)
        {
            List<AWBBatchDetail> list = new List<AWBBatchDetail>();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_GetCustomerShipmentValidAWBDetails";// "SP_GetShipmentValidAWBDetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BranchID", BranchID);
                cmd.Parameters.AddWithValue("@CustomerID", CustomerID);
                

                cmd.Parameters.AddWithValue("@FormatForXMLitem", Details);


                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);


                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        AWBBatchDetail obj = new AWBBatchDetail();

                        obj.InScanID = 0;
                        obj.SNo = i + 1;
                        obj.AWBNo = dt.Rows[i]["AWBNo"].ToString();
                        obj.AWBDate = Convert.ToDateTime(dt.Rows[i]["AWBDate"].ToString());
                        obj.CustomerID = 64967;// Convert.ToInt32(dt.Rows[i]["CustomerID"].ToString());
                        obj.Consignor = dt.Rows[i]["Consignor"].ToString();
                        obj.ConsignorContact = dt.Rows[i]["ConsignorContact"].ToString();
                        obj.ConsignorAddress1_Building = dt.Rows[i]["ConsignorAddress1"].ToString();
                        obj.ConsignorAddress2_Street = dt.Rows[i]["ConsignorAddress2"].ToString();
                        obj.ConsignorAddress3_PinCode = dt.Rows[i]["ConsignorAddress3"].ToString();
                        obj.ConsignorPhone = dt.Rows[i]["ConsignorPhone"].ToString();
                        obj.ConsignorMobileNo = dt.Rows[i]["ConsignorMobileNo"].ToString();
                        obj.ConsignorLocationName = dt.Rows[i]["ConsignorLocationName"].ToString();
                        obj.ConsignorCityName = dt.Rows[i]["ConsignorCityName"].ToString();
                        obj.ConsignorCountryName = dt.Rows[i]["ConsignorCountryName"].ToString();
                        obj.Consignee = dt.Rows[i]["Consignee"].ToString();
                        obj.ConsigneeContact = dt.Rows[i]["ConsigneeContact"].ToString();
                        obj.ConsigneeAddress1_Building = dt.Rows[i]["ConsigneeAddress1"].ToString();
                        obj.ConsigneeAddress2_Street = dt.Rows[i]["ConsigneeAddress2"].ToString();
                        obj.ConsigneeAddress3_PinCode = dt.Rows[i]["ConsigneeAddress3"].ToString();
                        obj.ConsigneeLocationName = dt.Rows[i]["ConsigneeLocationName"].ToString();
                        obj.ConsigneeCityName = dt.Rows[i]["ConsigneeCityName"].ToString();
                        obj.ConsigneeCountryName = dt.Rows[i]["ConsigneeCountryName"].ToString();
                        obj.ConsigneePhone = dt.Rows[i]["ConsigneePhone"].ToString();
                        obj.ConsigneeMobileNo = dt.Rows[i]["ConsigneeMobileNo"].ToString();
                        obj.PaymentModeId = Convert.ToInt32(dt.Rows[i]["PaymentModeId"].ToString());
                        obj.ParcelTypeId = Convert.ToInt32(dt.Rows[i]["ParcelTypeID"].ToString());
                        obj.MovementID = Convert.ToInt32(dt.Rows[i]["MovementID"].ToString());
                        obj.ProductTypeID = Convert.ToInt32(dt.Rows[i]["ProductTypeID"].ToString());
                        obj.ParcelType = dt.Rows[i]["ParcelType"].ToString();
                        obj.MovementType = dt.Rows[i]["MovementType"].ToString();
                        obj.ProductType = dt.Rows[i]["ProductType"].ToString();
                        obj.MaterialCost = Convert.ToDecimal(dt.Rows[i]["MaterialCost"].ToString());
                        obj.Pieces = dt.Rows[i]["Pieces"].ToString();
                        obj.Weight = CommonFunctions.ParseDecimal(dt.Rows[i]["Weight"].ToString());
                        obj.CourierCharge = CommonFunctions.ParseDecimal(dt.Rows[i]["CourierCharge"].ToString());
                        obj.OtherCharge = Convert.ToDecimal(dt.Rows[i]["OtherCharge"].ToString());
                        obj.NetTotal = Convert.ToDecimal(dt.Rows[i]["NetTotal"].ToString());
                        //obj.BagNo = dt.Rows[i]["BagNo"].ToString();
                        obj.CargoDescription = dt.Rows[i]["CargoDescription"].ToString();
                        obj.Remarks = dt.Rows[i]["Remarks"].ToString();

                        obj.EntrySource = 3;
                        obj.CurrencyID = Convert.ToInt32(dt.Rows[i]["CurrencyID"].ToString());
                        obj.Currency = dt.Rows[i]["Currency"].ToString();
                        obj.CustomsValue = 0; // Convert.ToDecimal(dt.Rows[i]["CustomsValue"].ToString());
                        //obj.CourierStatusID = Convert.ToInt32(dt.Rows[i]["CourierStatusID"].ToString());
                        //obj.StatusTypeId = Convert.ToInt32(dt.Rows[i]["StatusTypeId"].ToString()); 
                        obj.CustomerID = CustomerID;
                        //obj.FAgentName = dt.Rows[i]["FwdNo"].ToString();

                        obj.FAgentID = CommonFunctions.ParseInt(dt.Rows[i]["FAgentID"].ToString());
                        //obj.AWBValidStatus = Convert.ToBoolean(dt.Rows[i]["AWBValidStatus"].ToString());
                        //if (obj.AWBValidStatus == false)
                        //{
                        //    obj.AwbValidStatusclass = "awbvalidfalse";
                        //}

                        //else
                        //{
                        //    obj.AwbValidStatusclass = "";
                        //}
                        list.Add(obj);
                    }

                }
                else
                {
                    return list;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return list;
        }

        public static SaveStatusModel SaveAWBImportBatch(int BATCHID, int BranchID, int AcCompanyID, int UserID, int FYearID, string Details)
        {
            SaveStatusModel status = new SaveStatusModel();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_SaveAWBImportBatch";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Parameters.AddWithValue("@BatchID", BATCHID);
                cmd.Parameters.AddWithValue("@BranchID", BranchID);
                cmd.Parameters.AddWithValue("@AcCompanyID", AcCompanyID);
                cmd.Parameters.AddWithValue("@UserID", UserID);
                cmd.Parameters.AddWithValue("@FYearId", FYearID);
                cmd.Parameters.AddWithValue("@FormatForXMLitem", Details);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    status.Status = ds.Tables[0].Rows[0]["Status"].ToString();
                    status.Message = ds.Tables[0].Rows[0]["Message"].ToString();
                    status.TotalImportCount = Convert.ToInt32(ds.Tables[0].Rows[0]["TotalImportCount"].ToString());
                    status.TotalSavedCount = Convert.ToInt32(ds.Tables[0].Rows[0]["TotalSaved"].ToString());
                    //PickupRequestDAO dao = new PickupRequestDAO();
                    //string result2 = dao.AWBAccountsPosting(inscanid);
                    if (status.Status == "OK")
                        SaveAWBBatchPosting(BATCHID);

                    return status;

                }
                else
                {
                    status.Status = "Failed";
                    status.Message = "Saved Failed";
                    return status;
                }
            }
            catch (Exception ex)
            {
                status.Status = "Failed";
                status.Message = ex.Message;
                return status;
            }

        }

        public static SaveStatusModel SaveAWBBatchPosting(int BATCHID)
        {
            SaveStatusModel status = new SaveStatusModel();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_SaveBatchShipmentPosting";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BatchID", BATCHID);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                status.Status = "OK";



                return status;


            }
            catch (Exception ex)
            {
                status.Status = "Failed";
                status.Message = ex.Message;
                return status;
            }

        }
        #endregion

        #region "awb ecom batch"
        public static List<AWBBatchDetail> GetEcomShipmentValidAWBDetails(int BranchID, string Details, string AWBNo)
        {
            List<AWBBatchDetail> list = new List<AWBBatchDetail>();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_GetEcomShipmentValidAWBDetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BranchID", BranchID);


                cmd.Parameters.AddWithValue("@FormatForXMLitem", Details);


                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);


                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        AWBBatchDetail obj = new AWBBatchDetail();

                        obj.InScanID = 0;
                        obj.SNo = i + 1;
                        obj.AWBNo = dt.Rows[i]["AWBNo"].ToString();
                        obj.AWBDate = Convert.ToDateTime(dt.Rows[i]["AWBDate"].ToString());
                        obj.CustomerID = Convert.ToInt32(dt.Rows[i]["CustomerID"].ToString());
                        obj.Consignor = dt.Rows[i]["Consignor"].ToString();
                        obj.ConsignorContact = dt.Rows[i]["ConsignorContact"].ToString();
                        obj.ConsignorAddress1_Building = dt.Rows[i]["ConsignorAddress1"].ToString();
                        obj.ConsignorAddress2_Street = dt.Rows[i]["ConsignorAddress2"].ToString();
                        obj.ConsignorAddress3_PinCode = dt.Rows[i]["ConsignorAddress3"].ToString();
                        obj.ConsignorPhone = dt.Rows[i]["ConsignorPhone"].ToString();
                        obj.ConsignorMobileNo = dt.Rows[i]["ConsignorMobileNo"].ToString();
                        obj.ConsignorLocationName = dt.Rows[i]["ConsignorLocationName"].ToString();
                        obj.ConsignorCityName = dt.Rows[i]["ConsignorCityName"].ToString();
                        obj.ConsignorCountryName = dt.Rows[i]["ConsignorCountryName"].ToString();
                        obj.Consignee = dt.Rows[i]["Consignee"].ToString();
                        obj.ConsigneeContact = dt.Rows[i]["ConsigneeContact"].ToString();
                        obj.ConsigneeAddress1_Building = dt.Rows[i]["ConsigneeAddress1"].ToString();
                        obj.ConsigneeAddress2_Street = dt.Rows[i]["ConsigneeAddress2"].ToString();
                        obj.ConsigneeAddress3_PinCode = dt.Rows[i]["ConsigneeAddress3"].ToString();
                        obj.ConsigneeLocationName = dt.Rows[i]["ConsigneeLocationName"].ToString();
                        obj.ConsigneeCityName = dt.Rows[i]["ConsigneeCityName"].ToString();
                        obj.ConsigneeCountryName = dt.Rows[i]["ConsigneeCountryName"].ToString();
                        obj.ConsigneePhone = dt.Rows[i]["ConsigneePhone"].ToString();
                        obj.ConsigneeMobileNo = dt.Rows[i]["ConsigneeMobileNo"].ToString();
                        obj.PaymentModeId = Convert.ToInt32(dt.Rows[i]["PaymentModeId"].ToString());
                        obj.ParcelTypeId = Convert.ToInt32(dt.Rows[i]["ParcelTypeID"].ToString());
                        obj.MovementID = Convert.ToInt32(dt.Rows[i]["MovementID"].ToString());
                        obj.ProductTypeID = Convert.ToInt32(dt.Rows[i]["ProductTypeID"].ToString());
                        obj.ParcelType = dt.Rows[i]["ParcelType"].ToString();
                        obj.MovementType = dt.Rows[i]["MovementType"].ToString();
                        obj.ProductType = dt.Rows[i]["ProductType"].ToString();
                        obj.MaterialCost = Convert.ToDecimal(dt.Rows[i]["MaterialCost"].ToString());
                        obj.Pieces = dt.Rows[i]["Pieces"].ToString();
                        obj.Weight = CommonFunctions.ParseDecimal(dt.Rows[i]["Weight"].ToString());
                        obj.CourierCharge = CommonFunctions.ParseDecimal(dt.Rows[i]["CourierCharge"].ToString());
                        obj.OtherCharge = Convert.ToDecimal(dt.Rows[i]["OtherCharge"].ToString());
                        obj.NetTotal = Convert.ToDecimal(dt.Rows[i]["NetTotal"].ToString());
                        //obj.BagNo = dt.Rows[i]["BagNo"].ToString();
                        obj.CargoDescription = dt.Rows[i]["CargoDescription"].ToString();
                        obj.Remarks = dt.Rows[i]["Remarks"].ToString();

                        obj.EntrySource = 3;
                        obj.CurrencyID = Convert.ToInt32(dt.Rows[i]["CurrencyID"].ToString());
                        obj.Currency = dt.Rows[i]["Currency"].ToString();
                        obj.CustomsValue = Convert.ToDecimal(dt.Rows[i]["CustomsValue"].ToString());
                        //obj.CourierStatusID = Convert.ToInt32(dt.Rows[i]["CourierStatusID"].ToString());
                        //obj.StatusTypeId = Convert.ToInt32(dt.Rows[i]["StatusTypeId"].ToString()); 
                        obj.CustomerID = -2;
                        //obj.FAgentName = dt.Rows[i]["FwdNo"].ToString();

                        obj.FAgentID = CommonFunctions.ParseInt(dt.Rows[i]["FAgentID"].ToString());
                        //obj.AWBValidStatus = Convert.ToBoolean(dt.Rows[i]["AWBValidStatus"].ToString());
                        //if (obj.AWBValidStatus == false)
                        //{
                        //    obj.AwbValidStatusclass = "awbvalidfalse";
                        //}

                        //else
                        //{
                        //    obj.AwbValidStatusclass = "";
                        //}
                        list.Add(obj);
                    }

                }
                else
                {
                    return list;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return list;
        }


        public static SaveStatusModel SaveEcomAWBImportBatch(int BATCHID, int BranchID, int AcCompanyID, int UserID, int FYearID, string Details)
        {
            SaveStatusModel status = new SaveStatusModel();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_SaveEcomAWBImportBatch";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Parameters.AddWithValue("@BatchID", BATCHID);
                cmd.Parameters.AddWithValue("@BranchID", BranchID);
                cmd.Parameters.AddWithValue("@AcCompanyID", AcCompanyID);
                cmd.Parameters.AddWithValue("@UserID", UserID);
                cmd.Parameters.AddWithValue("@FYearId", FYearID);
                cmd.Parameters.AddWithValue("@FormatForXMLitem", Details);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    status.Status = ds.Tables[0].Rows[0]["Status"].ToString();
                    status.Message = ds.Tables[0].Rows[0]["Message"].ToString();
                    status.TotalImportCount = Convert.ToInt32(ds.Tables[0].Rows[0]["TotalImportCount"].ToString());
                    status.TotalSavedCount = Convert.ToInt32(ds.Tables[0].Rows[0]["TotalSaved"].ToString());
                    //PickupRequestDAO dao = new PickupRequestDAO();
                    //string result2 = dao.AWBAccountsPosting(inscanid);
                    if (status.Status == "OK")
                        SaveAWBBatchPosting(BATCHID);

                    return status;

                }
                else
                {
                    status.Status = "Failed";
                    status.Message = "Saved Failed";
                    return status;
                }
            }
            catch (Exception ex)
            {
                status.Status = "Failed";
                status.Message = ex.Message;
                return status;
            }

        }
        #endregion
    }
}