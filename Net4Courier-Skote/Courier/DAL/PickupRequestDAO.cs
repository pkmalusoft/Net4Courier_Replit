using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Net4Courier.Models;
using System.Web.Hosting;
using ClosedXML;
using ClosedXML.Excel;
using System.IO;
namespace Net4Courier.DAL
{
    public class PickupRequestDAO
    {
        public static bool  CheckCustomerNameExist(string CustomerName,int CustomerId=0)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            if (CustomerId > 0)
                cmd.CommandText = "select CustomerName from CustomerMaster where lower(rtrim(Isnull(CustomerName,''))) ='" + CustomerName.Trim().ToLower() + "' and CustomerId<>" + CustomerId.ToString();
            else
                cmd.CommandText = "select CustomerName from CustomerMaster where lower(rtrim(Isnull(CustomerName,''))) ='" + CustomerName.Trim().ToLower() + "'"; 
            cmd.CommandType = CommandType.Text;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<CountryMasterVM> objList = new List<CountryMasterVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count>0)
                {
                    return true;
                }
            }
            return false;
        }

        public static List<CityVM> GetSelectedCityName(string CityID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "select x.CityID,x.City,x.CountryID,y.CountryName from CityMaster x, CountryMaster y Where x.CityID in(" + CityID + ") and x.CountryID=y.CountryID"; 
            cmd.CommandType = CommandType.Text;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<CityVM> objList = new List<CityVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    CityVM obj = new CityVM();
                    obj.CityID = Convert.ToInt32(ds.Tables[0].Rows[i]["CityID"].ToString());
                    obj.CountryID= Convert.ToInt32(ds.Tables[0].Rows[i]["CountryID"].ToString());
                    obj.City = ds.Tables[0].Rows[i]["City"].ToString();
                    obj.CountryName = ds.Tables[0].Rows[i]["CountryName"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static List<CityVM> GetSelectedCountryName(string CountryIDs)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "select y.CountryID,y.CountryName from CountryMaster y Where y.CountryID in(" + CountryIDs + ")";
            cmd.CommandType = CommandType.Text;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<CityVM> objList = new List<CityVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    CityVM obj = new CityVM();
                    
                    obj.CountryID = Convert.ToInt32(ds.Tables[0].Rows[i]["CountryID"].ToString());
                  
                    obj.CountryName = ds.Tables[0].Rows[i]["CountryName"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static List<CountryMasterVM> GetCountryName()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "select distinct CountryName from LocationMaster where isnull(CountryName, '') <> ''";
            cmd.CommandType = CommandType.Text;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<CountryMasterVM> objList = new List<CountryMasterVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    CountryMasterVM obj = new CountryMasterVM();
                    
                    obj.CountryName = ds.Tables[0].Rows[i]["CountryName"].ToString();
                    
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static List<CityVM> GetCityName()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "select distinct Top 100  CityName,CountryName from LocationMaster where isnull(CityName, '') <> '' and isnull(CountryName, '') <> '' ";
            cmd.CommandType = CommandType.Text;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<CityVM> objList = new List<CityVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    CityVM obj = new CityVM();

                    obj.City = ds.Tables[0].Rows[i]["CityName"].ToString();
                    obj.CountryName = ds.Tables[0].Rows[i]["CountryName"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static List<LocationVM> GetLocationName()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
           cmd.CommandText = "select distinct LocationName=LocationName, CityName,CountryName,Isnull(PlaceID,'') as 'CountryCode' from LocationMaster where isnull(locationname,'')<>'' and isnull(CityName, '') <> '' and isnull(CountryName, '') <> '' order by LocationName";
            cmd.CommandType = CommandType.Text;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<LocationVM> objList = new List<LocationVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    LocationVM obj = new LocationVM();

                    obj.Location = ds.Tables[0].Rows[i]["LocationName"].ToString();
                    obj.CityName = ds.Tables[0].Rows[i]["CityName"].ToString();
                    obj.CountryName = ds.Tables[0].Rows[i]["CountryName"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static List<LocationVM> CheckLocationName(string Locationname)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "select distinct LocationName , CityName,CountryName, CountryCode=Isnull(PlaceID,'') from LocationMaster where isnull(locationname,'')<>'' and isnull(CityName, '') <> '' and isnull(CountryName, '') <> '' and LocationName='" + Locationname + "' order by LocationName";
            cmd.CommandType = CommandType.Text;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<LocationVM> objList = new List<LocationVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    LocationVM obj = new LocationVM();

                    obj.Location = ds.Tables[0].Rows[i]["LocationName"].ToString();
                    obj.CityName = ds.Tables[0].Rows[i]["CityName"].ToString();
                    obj.CountryName = ds.Tables[0].Rows[i]["CountryName"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public string GetMaxPickupRequest(int Companyid,int BranchId)
        {
            DataTable dt = new DataTable();
            string MaxPickUpNo = "";
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "GetMaxPickupRequest";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;

                        cmd.Parameters.AddWithValue("@CompanyId", Companyid);
                        cmd.Parameters.AddWithValue("@BranchId", BranchId);
                        con.Open();
                        SqlDataAdapter SqlDA = new SqlDataAdapter(cmd);
                        SqlDA.Fill(dt);
                        if (dt.Rows.Count > 0)
                            MaxPickUpNo = dt.Rows[0][0].ToString();


                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {

            }
            return MaxPickUpNo;

        }

        public string GetMaxCustomerCode(int BranchId)
        {
            DataTable dt = new DataTable();
            string MaxPickUpNo = "";
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "GetMaxCustomerNo";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;

                        cmd.Parameters.AddWithValue("@BranchId", BranchId);

                        con.Open();
                        SqlDataAdapter SqlDA = new SqlDataAdapter(cmd);
                        SqlDA.Fill(dt);
                        if (dt.Rows.Count > 0)
                            MaxPickUpNo = dt.Rows[0][0].ToString();


                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {

            }
            return MaxPickUpNo;

        }


        public string GetMaAWBNo(int CompanyId,int BranchId=1,int ShipmentModeID=0)
        {
            DataTable dt = new DataTable();
            string MaxPickUpNo = "";
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "GetMaxAWBNo";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;

                        cmd.Parameters.AddWithValue("@CompanyId", CompanyId);
                        cmd.Parameters.AddWithValue("@BranchId", BranchId);                        
                        cmd.Parameters.AddWithValue("@ShipmentModeId", ShipmentModeID);
                        con.Open();
                        SqlDataAdapter SqlDA = new SqlDataAdapter(cmd);
                        SqlDA.Fill(dt);
                        if (dt.Rows.Count > 0)
                            MaxPickUpNo = dt.Rows[0][0].ToString();


                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {

            }
            return MaxPickUpNo;

        }


        public string GetMaxInScanSheetNo(DateTime InscanDate, int Companyid, int BranchId,string Type)
        {
            DataTable dt = new DataTable();
            string MaxPickUpNo = "";
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "GetMaxInScanSheetNo";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;                        
                        cmd.Parameters.AddWithValue("@CompanyId", Companyid);
                        cmd.Parameters.AddWithValue("@BranchId", BranchId);
                        cmd.Parameters.AddWithValue("@SourceType", Type);
                        cmd.Parameters.AddWithValue("@InscanDate", InscanDate.ToString("MM/dd/yyyy"));
                        con.Open();
                        SqlDataAdapter SqlDA = new SqlDataAdapter(cmd);
                        SqlDA.Fill(dt);
                        if (dt.Rows.Count > 0)
                            MaxPickUpNo = dt.Rows[0][0].ToString();


                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {

            }
            return MaxPickUpNo;

        }

        public string GetMaxInvoiceNo(int Companyid, int BranchId,int FYearId)
        {
            DataTable dt = new DataTable();
            string MaxPickUpNo = "";
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "GetMaxInvoiceNo";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;

                        cmd.Parameters.AddWithValue("@CompanyId", Companyid);
                        cmd.Parameters.AddWithValue("@BranchId", BranchId);
                        cmd.Parameters.AddWithValue("@FYearId", FYearId);
                        con.Open();
                        SqlDataAdapter SqlDA = new SqlDataAdapter(cmd);
                        SqlDA.Fill(dt);
                        if (dt.Rows.Count > 0)
                            MaxPickUpNo = dt.Rows[0][0].ToString();


                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return MaxPickUpNo;

        }
        public string GetMaxCODInvoiceNo(int Companyid, int BranchId, int FYearId)
        {
            DataTable dt = new DataTable();
            string MaxPickUpNo = "";
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "GetMaxCODInvoiceNo";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;

                        cmd.Parameters.AddWithValue("@CompanyId", Companyid);
                        cmd.Parameters.AddWithValue("@BranchId", BranchId);
                        cmd.Parameters.AddWithValue("@FYearId", FYearId);
                        con.Open();
                        SqlDataAdapter SqlDA = new SqlDataAdapter(cmd);
                        SqlDA.Fill(dt);
                        if (dt.Rows.Count > 0)
                            MaxPickUpNo = dt.Rows[0][0].ToString();


                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return MaxPickUpNo;

        }
        public string GetMaxAgentInvoiceNo(int Companyid, int BranchId, int FYearId)
        {
            DataTable dt = new DataTable();
            string MaxPickUpNo = "";
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "GetMaxAgentInvoiceNo";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;

                        cmd.Parameters.AddWithValue("@CompanyId", Companyid);
                        cmd.Parameters.AddWithValue("@BranchId", BranchId);
                        cmd.Parameters.AddWithValue("@FYearId", FYearId);
                        con.Open();
                        SqlDataAdapter SqlDA = new SqlDataAdapter(cmd);
                        SqlDA.Fill(dt);
                        if (dt.Rows.Count > 0)
                            MaxPickUpNo = dt.Rows[0][0].ToString();


                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return MaxPickUpNo;

        }

        public string GetMaxTaxInvoiceNo(int Companyid, int BranchId, int FYearId)
        {
            DataTable dt = new DataTable();
            string MaxPickUpNo = "";
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "GetTAXInvoiceNo";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;

                        cmd.Parameters.AddWithValue("@CompanyId", Companyid);
                        cmd.Parameters.AddWithValue("@BranchId", BranchId);
                        cmd.Parameters.AddWithValue("@FYearId", FYearId);
                        con.Open();
                        SqlDataAdapter SqlDA = new SqlDataAdapter(cmd);
                        SqlDA.Fill(dt);
                        if (dt.Rows.Count > 0)
                            MaxPickUpNo = dt.Rows[0][0].ToString();


                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return MaxPickUpNo;

        }

        public string GetMaxDRSNo(int Companyid, int BranchId,DateTime EntryDate)
        {
            DataTable dt = new DataTable();
            string MaxPickUpNo = "";
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "GetMaxDRSSheetNo";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;

                        cmd.Parameters.AddWithValue("@CompanyId", Companyid);
                        cmd.Parameters.AddWithValue("@BranchId", BranchId);
                        cmd.Parameters.AddWithValue("@EntryDate", EntryDate);
                        con.Open();
                        SqlDataAdapter SqlDA = new SqlDataAdapter(cmd);
                        SqlDA.Fill(dt);
                        if (dt.Rows.Count > 0)
                            MaxPickUpNo = dt.Rows[0][0].ToString();


                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {

            }
            return MaxPickUpNo;

        }


        public string GetMaxDomesticReceiptNo(int Companyid, int BranchId, int FYearId)
        {
            DataTable dt = new DataTable();
            string MaxPickUpNo = "";
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "GetMaxDomesticCODReceiptNo";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;

                        cmd.Parameters.AddWithValue("@CompanyId", Companyid);
                        cmd.Parameters.AddWithValue("@BranchId", BranchId);
                        cmd.Parameters.AddWithValue("@FYearId", FYearId);
                        con.Open();
                        SqlDataAdapter SqlDA = new SqlDataAdapter(cmd);
                        SqlDA.Fill(dt);
                        if (dt.Rows.Count > 0)
                            MaxPickUpNo = dt.Rows[0][0].ToString();


                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return MaxPickUpNo;

        }


        public string GetMaxMCDocumentNo(int Companyid, int BranchId, int FYearId)
        {
            DataTable dt = new DataTable();
            string MaxPickUpNo = "";
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "GetMaxMCDocumentNo";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;

                        cmd.Parameters.AddWithValue("@CompanyId", Companyid);
                        cmd.Parameters.AddWithValue("@BranchId", BranchId);
                        cmd.Parameters.AddWithValue("@FYearId", FYearId);
                        con.Open();
                        SqlDataAdapter SqlDA = new SqlDataAdapter(cmd);
                        SqlDA.Fill(dt);
                        if (dt.Rows.Count > 0)
                            MaxPickUpNo = dt.Rows[0][0].ToString();


                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return MaxPickUpNo;

        }

        // Generate a random password of a given length (optional)  
        public string RandomPassword(int size = 0)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(RandomString(4, true));
            builder.Append(RandomNumber(1000, 9999));
            builder.Append(RandomString(2, false));
            return builder.ToString();
        }
        // Generate a random string with a given size and case.   
        // If second parameter is true, the return string is lowercase  
        public string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }

        // Generate a random number between two numbers    
        public int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        public string SaveMenuAccess(int RoleId ,string Menus)
        {
            DataTable dt = new DataTable();
            string MaxPickUpNo = "";
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SaveRoleMenuAccessRights";
                            cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;

                        cmd.Parameters.AddWithValue("@RoleId", RoleId);
                        cmd.Parameters.AddWithValue("@MenusList", Menus);
                        con.Open();
                        cmd.ExecuteNonQuery();

                        //SqlDataAdapter SqlDA = new SqlDataAdapter(cmd);
                        //SqlDA.Fill(dt);
                        //if (dt.Rows.Count > 0)
                        //    MaxPickUpNo = dt.Rows[0][0].ToString();


                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {

            }
            return MaxPickUpNo;
        }


        public string GetMaxCODReceiptNo(int Companyid, int BranchId)
        {
            DataTable dt = new DataTable();
            string MaxPickUpNo = "";
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "GetMaxCODReceiptNo";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;

                        cmd.Parameters.AddWithValue("@CompanyId", Companyid);
                        cmd.Parameters.AddWithValue("@BranchId", BranchId);
                        con.Open();
                        SqlDataAdapter SqlDA = new SqlDataAdapter(cmd);
                        SqlDA.Fill(dt);
                        if (dt.Rows.Count > 0)
                            MaxPickUpNo = dt.Rows[0][0].ToString();


                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {

            }
            return MaxPickUpNo;

        }

        public static MCDetail GetAWBMCDetail(int InscanId)
        {
            DataTable dt = new DataTable();
            string MaxPickUpNo = "";
            MCDetail obj = new MCDetail();
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_GetInscanMCDetail";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;

                        cmd.Parameters.AddWithValue("@InscanId", InscanId);                   

                        con.Open();
                        SqlDataAdapter SqlDA = new SqlDataAdapter(cmd);
                        SqlDA.Fill(dt);
                      
                        if (dt.Rows.Count > 0)
                        {
                            obj.COD = Convert.ToDecimal(dt.Rows[0]["COD"].ToString());
                            obj.MCAmount = Convert.ToDecimal(dt.Rows[0]["MCAmount"].ToString());
                            obj.MCReceived = Convert.ToDecimal(dt.Rows[0]["MCReceived"].ToString());
                            obj.MCPaid = Convert.ToDecimal(dt.Rows[0]["MCPaid"].ToString());
                            obj.CODStatus = dt.Rows[0]["CODStatus"].ToString();
                            obj.MCStatus = dt.Rows[0]["MCStatus"].ToString();
                            obj.MCPaymentDetail = dt.Rows[0]["MCPaymentDetail"].ToString();
                            obj.MCReceivedDetail = dt.Rows[0]["MCReceivedDetail"].ToString();
                            obj.Confirmation = dt.Rows[0]["Confirmation"].ToString();
                            obj.MCClosedDetail = dt.Rows[0]["MCClosedDetail"].ToString();
                        }


                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {

            }
            return obj;

        }
        //public string DeleteExportShipment(int Id)
        //{            
        //    try
        //    {
        //        //string json = "";
        //        string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
        //        using (SqlConnection con = new SqlConnection(strConnString))
        //        {

        //            using (SqlCommand cmd = new SqlCommand())
        //            {
        //                cmd.CommandText = "Delete from exportshipmentDetails where ExportId=" + Id.ToString();
        //                cmd.CommandType = CommandType.Text;
        //                cmd.Connection = con;
        //                con.Open();
        //                cmd.ExecuteNonQuery();
        //                cmd.CommandText = "Delete from exportshipment where Id=" + Id.ToString();
        //                cmd.ExecuteNonQuery();
        //                con.Close();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.Message;
        //    }
        //    return "OK";

        //}
        public static DataTable DeleteExportShipment(int ExportId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteExportShipment";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ExportID", ExportId);
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


        //SP_AWBPosting
        public string AWBAccountsPosting(int Id)
        {
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_AWBPosting " + Id.ToString();
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = con;
                        con.Open();
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

        //Generate Invoice Posting
        public string GenerateInvoicePosting(int Id)
        {
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_GenerateInvoicePosting " + Id.ToString();
                        cmd.CommandTimeout = 0;
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = con;
                        con.Open();
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
        public string RevertTaxInvoiceUpdate(int Id)
        {
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "Update Inboundshipment set TaxInvoiceId=null where TaxInvoiceId=" + Id.ToString();
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = con;
                        con.Open();
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


        public string RevertCODReceiptUpdate(int Id)
        {
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "Update Inboundshipment set CODreceiptID=null where CODreceiptID=" + Id.ToString();
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = con;
                        con.Open();
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
        public string GenerateTaxInvoicePosting(int Id)
        {
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_GenerateTaxInvoicePosting " + Id.ToString();
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = con;
                        con.Open();
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

        public string GenerateCOLoaderPosting(int Id)
        {
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_GenerateCOLoaderInvoicePosting " + Id.ToString();
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = con;
                        con.Open();
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

        //Generate COD Receipt posting
        public string GenerateCODPosting(int Id)
        {
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_CODReceiptPosting " + Id.ToString();
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = con;
                        con.Open();
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

        //Generate Domestic COD Receipt posting
        public string GenerateDomesticCODPosting(int Id)
        {
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_DomesticCODReceiptPosting " + Id.ToString();
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = con;
                        con.Open();
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

        ///Export Manifest Posting SP_ExportManifestPosting 5
        ///
        public string GenerateExportManifestPosting(int Id)
        {
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_GenerateExportManifestPosting " + Id.ToString();
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = con;
                        con.Open();
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


        public int GenerateCustomerInvoiceAll(InvoiceAllParam obj)
        {
            int companyId = Convert.ToInt32(HttpContext.Current.Session["CurrentCompanyID"].ToString());
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();
            int InvoiceCount = 0;
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_GenerateMultipleInvoice";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@FromDate", obj.FromDate.ToString("MM/dd/yyyy"));
                        cmd.Parameters.AddWithValue("@ToDate", obj.ToDate.ToString("MM/dd/yyyy"));
                        cmd.Parameters.AddWithValue("@InvoiceDate", obj.InvoiceDate.ToString("MM/dd/yyyy"));
                        cmd.Parameters.AddWithValue("@MovementId", obj.MovementId);
                        cmd.Parameters.AddWithValue("@Companyid", companyId);
                        cmd.Parameters.AddWithValue("@FYearId",  yearid);
                        cmd.Parameters.AddWithValue("@BranchId", branchid);
                        cmd.Parameters.AddWithValue("@UserId", userid);
                        cmd.Parameters.AddWithValue("@EntryDate", CommonFunctions.GetCurrentDateTime().ToString("MM/dd/yyyy hh:mm"));
                        cmd.Parameters.AddWithValue("@CustomerIDs", obj.CustomerIDS);
                        
                        cmd.Connection = con;
                       
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            InvoiceCount = Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
                        }
                  }
                }
            }
            catch (Exception ex)
            {
                return InvoiceCount;
            }
            return InvoiceCount;

        }

        public int GenerateCOInvoiceAll(InvoiceAllParam obj)
        {
            int companyId = Convert.ToInt32(HttpContext.Current.Session["CurrentCompanyID"].ToString());
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();
            int InvoiceCount = 0;
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_GenerateMultipleCOInvoice";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@FromDate", obj.FromDate.ToString("MM/dd/yyyy"));
                        cmd.Parameters.AddWithValue("@ToDate", obj.ToDate.ToString("MM/dd/yyyy"));
                        cmd.Parameters.AddWithValue("@InvoiceDate", obj.InvoiceDate.ToString("MM/dd/yyyy"));
                        cmd.Parameters.AddWithValue("@MovementId", 4);
                        cmd.Parameters.AddWithValue("@Companyid", companyId);
                        cmd.Parameters.AddWithValue("@FYearId", yearid);
                        cmd.Parameters.AddWithValue("@BranchId", branchid);
                        cmd.Parameters.AddWithValue("@UserId", userid);
                        cmd.Parameters.AddWithValue("@EntryDate", CommonFunctions.GetBranchDateTime().ToString("MM/dd/yyyy HH:mm"));
                        cmd.Connection = con;
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            InvoiceCount = Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
            return InvoiceCount;

        }
        public static List<CustomerInvoicePendingModel> GenerateInvoicePending(InvoiceAllParam obj)
        {
            int companyId = Convert.ToInt32(HttpContext.Current.Session["CurrentCompanyID"].ToString());
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();
            List<CustomerInvoicePendingModel> list = new List<CustomerInvoicePendingModel>();
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
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@FromDate", obj.FromDate.ToString("MM/dd/yyyy"));
                        cmd.Parameters.AddWithValue("@ToDate", obj.ToDate.ToString("MM/dd/yyyy"));                        
                        cmd.Parameters.AddWithValue("@MovementId", obj.MovementId);
                        cmd.Parameters.AddWithValue("@Companyid", companyId);
                        cmd.Parameters.AddWithValue("@FYearId", yearid);
                        cmd.Parameters.AddWithValue("@BranchId", branchid);                        
                        cmd.Connection = con;
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        //CustomerInvoicePendingModel
                      
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                
                                var item= new CustomerInvoicePendingModel();
                                item.Id = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["Id"].ToString());
                                item.CustomerId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CustomerID"].ToString());
                                item.CustomerName = ds.Tables[0].Rows[i]["CustomerName"].ToString();
                                item.TotalAWB = Convert.ToInt32(ds.Tables[0].Rows[i]["TotalAWB"].ToString());
                                item.CourierCharge = Convert.ToDecimal(ds.Tables[0].Rows[i]["CourierCharge"].ToString());
                                item.OtherCharge = Convert.ToDecimal(ds.Tables[0].Rows[i]["OtherCharge"].ToString());
                                item.NetTototal = Convert.ToDecimal(ds.Tables[0].Rows[i]["NetTotal"].ToString());
                                item.CustomerChecked = false;
                                list.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return list;

        }

        public static List<CustomerInvoicePendingModel> GenerateCoLoaderInvoicePending(InvoiceAllParam obj)
        {
            int companyId = Convert.ToInt32(HttpContext.Current.Session["CurrentCompanyID"].ToString());
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();
            List<CustomerInvoicePendingModel> list = new List<CustomerInvoicePendingModel>();
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_GetCoLoaderInvoicePending";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@FromDate", obj.FromDate.ToString("MM/dd/yyyy"));
                        cmd.Parameters.AddWithValue("@ToDate", obj.ToDate.ToString("MM/dd/yyyy"));
                        cmd.Parameters.AddWithValue("@MovementId", "4");
                        cmd.Parameters.AddWithValue("@Companyid", companyId);
                        cmd.Parameters.AddWithValue("@FYearId", yearid);
                        cmd.Parameters.AddWithValue("@BranchId", branchid);
                        cmd.Connection = con;
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        //CustomerInvoicePendingModel

                        if (ds != null && ds.Tables.Count > 0)
                        {
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {

                                var item = new CustomerInvoicePendingModel();
                                item.Id = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["Id"].ToString());
                                item.CustomerId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CustomerId"].ToString());
                                item.CustomerName = ds.Tables[0].Rows[i]["CustomerName"].ToString();
                                item.TotalAWB = Convert.ToInt32(ds.Tables[0].Rows[i]["TotalAWB"].ToString());
                                item.CourierCharge = Convert.ToDecimal(ds.Tables[0].Rows[i]["CourierCharge"].ToString());
                                item.OtherCharge = Convert.ToDecimal(ds.Tables[0].Rows[i]["OtherCharge"].ToString());
                                item.NetTototal = Convert.ToDecimal(ds.Tables[0].Rows[i]["NetTotal"].ToString());

                                list.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return list;

        }
        public static List<QuickAWBVM> GetAWBList(int StatusId,DateTime FromDate,DateTime ToDate,int BranchId,int DepotId, string AWBNo,int MovementId,int PaymentModeId,string ConsignorText,string Origin,string Destination,int CreatedBy)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetAWBList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@pStatusId",StatusId);
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@BranchId", BranchId);
            cmd.Parameters.AddWithValue("@DepotId", DepotId);
                         
            cmd.Parameters.AddWithValue("@MovementId", MovementId);
            cmd.Parameters.AddWithValue("@PaymentModeId", PaymentModeId);
            if (ConsignorText == null)
                ConsignorText = "";
            else if(ConsignorText!="")
                ConsignorText = ConsignorText + "%";
            cmd.Parameters.AddWithValue("@ConsignorConsignee", ConsignorText);
            if (Origin == null )
                Origin = "";
            else if (Origin.Trim() == "")
                Origin = "";

            cmd.Parameters.AddWithValue("@Origin", Origin);
            if (Destination == null)
                Destination = "";
            else if (Destination.Trim() == "")
                Destination = "";

            cmd.Parameters.AddWithValue("@Destination", Destination);
             
            if (AWBNo == null)
                AWBNo = "";
                cmd.Parameters.AddWithValue("@AWBNo", AWBNo.Trim());

            cmd.Parameters.AddWithValue("@CreatedBy", CreatedBy);
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
                    obj.HAWBNo = ds.Tables[0].Rows[i]["HAWBNo"].ToString();
                    obj.shippername= ds.Tables[0].Rows[i]["shippername"].ToString();
                    obj.consigneename = ds.Tables[0].Rows[i]["consigneename"].ToString();
                    obj.destination = ds.Tables[0].Rows[i]["destination"].ToString();
                    obj.InScanDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["InScanDate"].ToString());
                    obj.CourierStatus = ds.Tables[0].Rows[i]["CourierStatus"].ToString();
                    obj.StatusType = ds.Tables[0].Rows[i]["StatusType"].ToString();
                    obj.totalCharge = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["totalCharge"].ToString());
                    obj.Weight = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Weight"].ToString());
                    obj.paymentmode = ds.Tables[0].Rows[i]["paymentmode"].ToString();
                    obj.ConsigneePhone= ds.Tables[0].Rows[i]["ConsigneePhone"].ToString();
                    obj.CreatedByName = ds.Tables[0].Rows[i]["CreatedByName"].ToString();
                    obj.LastModifiedByName= ds.Tables[0].Rows[i]["LastModifiedByName"].ToString();
                    obj.CreatedByDate = ds.Tables[0].Rows[i]["CreatedByDate"].ToString();
                    obj.LastModifiedDate = ds.Tables[0].Rows[i]["LastModifiedDate"].ToString();
                    obj.InvoiceId = Convert.ToInt32(ds.Tables[0].Rows[i]["InvoiceID"].ToString());
                    obj.COInvoiceId = Convert.ToInt32(ds.Tables[0].Rows[i]["COInvoiceID"].ToString());
                    obj.ImportShipmentId = Convert.ToInt32(ds.Tables[0].Rows[i]["ImportShipmentId"].ToString());
                    obj.CollectedBy = ds.Tables[0].Rows[i]["CollectedBy"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static List<QuickAWBVM> PrintAWBRegister()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            DatePicker param = (DatePicker)HttpContext.Current.Session["AWBRegisterPrintSearch"];
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_PrintAWBRegister";
            cmd.CommandType = CommandType.StoredProcedure;
            if (param.StatusId == null)
                param.StatusId = 0;
            cmd.Parameters.AddWithValue("@StatusId", param.StatusId);
            cmd.Parameters.AddWithValue("@FromDate", param.FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", param.ToDate.ToString("MM/dd/yyyy"));            
            cmd.Parameters.AddWithValue("@PaymentModeId", param.paymentId);
            cmd.Parameters.AddWithValue("@AcHeadId", param.AcHeadId);            
            cmd.Parameters.AddWithValue("@BranchId", branchid);
            cmd.Parameters.AddWithValue("@FYearId", yearid);
            if (param.MovementId==null || param.MovementId=="")
                cmd.Parameters.AddWithValue("@MovementId", "1,2,3,4");
            else
                cmd.Parameters.AddWithValue("@MovementId", param.MovementId);
            
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
                    obj.HAWBNo = ds.Tables[0].Rows[i]["HAWBNo"].ToString();
                    obj.shippername = ds.Tables[0].Rows[i]["shippername"].ToString();
                    obj.consigneename = ds.Tables[0].Rows[i]["consigneename"].ToString();
                    obj.origin = ds.Tables[0].Rows[i]["origin"].ToString();
                    obj.destination = ds.Tables[0].Rows[i]["destination"].ToString();
                    obj.InScanID = Convert.ToInt32(ds.Tables[0].Rows[i]["InScanID"].ToString());
                    obj.InScanDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["InScanDate"].ToString());
                    obj.CourierStatus = ds.Tables[0].Rows[i]["CourierStatus"].ToString();
                    obj.StatusType = ds.Tables[0].Rows[i]["StatusType"].ToString();
                    obj.Pieces = ds.Tables[0].Rows[i]["Pieces"].ToString();
                    obj.Weight = Convert.ToDecimal(ds.Tables[0].Rows[i]["Weight"].ToString());
                    obj.CourierCharge = Convert.ToDecimal(ds.Tables[0].Rows[i]["CourierCharge"].ToString());
                    obj.OtherCharge = Convert.ToDecimal(ds.Tables[0].Rows[i]["OtherCharge"].ToString());
                    obj.totalCharge = Convert.ToDecimal(ds.Tables[0].Rows[i]["totalCharge"].ToString());
                    obj.MovementTypeID= Convert.ToInt32(ds.Tables[0].Rows[i]["MovementID"].ToString());
                    //obj.MovementTypeID = Convert.ToInt32(ds.Tables[0].Rows[i]["MovementID"].ToString());
                    obj.paymentmode = ds.Tables[0].Rows[i]["paymentmode"].ToString();
                    obj.ConsigneePhone = ds.Tables[0].Rows[i]["ConsigneePhone"].ToString();
                    obj.ConsigneeMobile = ds.Tables[0].Rows[i]["ConsigneeMobile"].ToString();
                    obj.ConsigneeAddress1_Building = ds.Tables[0].Rows[i]["ConsigneeAddress1_Building"].ToString();
                    obj.ConsigneeAddress3_PinCode = ds.Tables[0].Rows[i]["ConsigneeAddress3_PinCode"].ToString();
                    obj.ConsigneeLocationName = ds.Tables[0].Rows[i]["ConsigneeLocationName"].ToString();
                    obj.ConsigneeCityName = ds.Tables[0].Rows[i]["ConsigneeCityName"].ToString();
                    obj.ConsigneeCountryName = ds.Tables[0].Rows[i]["ConsigneeCountryName"].ToString();
                    obj.Description  = ds.Tables[0].Rows[i]["Description"].ToString();
                    obj.FWDAgentNumber  = ds.Tables[0].Rows[i]["FWDAgentNumber"].ToString();
                    obj.FAgentName = ds.Tables[0].Rows[i]["FAgentName"].ToString();
                    obj.ParcelType = ds.Tables[0].Rows[i]["ParcelType"].ToString();
                    obj.MovementType = ds.Tables[0].Rows[i]["MovementType"].ToString();
                    obj.ProductName = ds.Tables[0].Rows[i]["ProductName"].ToString();
                    obj.TaxAmount = Convert.ToDecimal(ds.Tables[0].Rows[i]["Taxamount"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }

        //Generate SupplierInvoice posting
        public string GenerateSupplierInvoicePosting(int Id)
        {
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_SupplierInvoicePosting " + Id.ToString();
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = con;
                        con.Open();
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

        //Generate SupplierInvoice posting
        public string GenerateForwardingInvoicePosting(int Id)
        {
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_ForwardingInvoicePosting " + Id.ToString();
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = con;
                        con.Open();
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



        public static List<InScanVM> GetInScanList(DateTime FromDate, DateTime ToDate, int FyearId,int BranchId,int DepotId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetInScanList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@FYearId", FyearId);
            cmd.Parameters.AddWithValue("@BranchID", BranchId);
            cmd.Parameters.AddWithValue("@DepotId", DepotId);            

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<InScanVM> objList = new List<InScanVM>();
            InScanVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new InScanVM();                      
                    obj.QuickInscanID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["QuickInscanID"].ToString());
                    obj.InScanSheetNo= ds.Tables[0].Rows[i]["InscanSheetNumber"].ToString();
                    obj.QuickInscanDateTime = Convert.ToDateTime(ds.Tables[0].Rows[i]["QuickInscanDateTime"].ToString());
                    obj.CollectedBy = ds.Tables[0].Rows[i]["CollectedBy"].ToString();
                    obj.ReceivedBy = ds.Tables[0].Rows[i]["ReceivedBy"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static List<AWBList> GetInScannedItems(int QuickInscanId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetQuickInscanItems";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@QuickInscanId", QuickInscanId);            

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<AWBList> objList = new List<AWBList>();
            AWBList obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new AWBList();
                    obj.InScanId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["InScanId"].ToString());
                    obj.ShipmentDetailId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ShipmentDetailId"].ToString());
                    obj.Origin= ds.Tables[0].Rows[i]["Origin"].ToString();
                    obj.Destination = ds.Tables[0].Rows[i]["Destination"].ToString();
                    obj.AWB= ds.Tables[0].Rows[i]["AWB"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static List<AWBList> GetInScannedItemsByCourier(int CollectedEmpId,DateTime CollectedDate)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DomesticInscanCollectedBy";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CollectedDate", CollectedDate);
            cmd.Parameters.AddWithValue("@EmployeeId", CollectedEmpId);            

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet(); 
            da.Fill(ds);
            List<AWBList> objList = new List<AWBList>();
            AWBList obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new AWBList();
                    obj.InScanId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["InScanId"].ToString());
                    obj.ShipmentDetailId = 0;
                    obj.Origin = ds.Tables[0].Rows[i]["Origin"].ToString();
                    obj.Destination = ds.Tables[0].Rows[i]["Destination"].ToString();
                    obj.AWB = ds.Tables[0].Rows[i]["AWB"].ToString();
                    obj.CourierStatusId = Convert.ToInt32(ds.Tables[0].Rows[i]["CourierStatusId"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static decimal GetCollectedAmountByCourier(int CollectedEmpId, DateTime CollectedDate)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetCourierCollectedAmount";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CollectedDate", CollectedDate);
            cmd.Parameters.AddWithValue("@EmployeeId", CollectedEmpId);
            decimal Amount = 0;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<AWBList> objList = new List<AWBList>();
            AWBList obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                if( ds.Tables[0].Rows.Count>0)
                {
                    if (ds.Tables[0].Rows[0][0] == System.DBNull.Value)
                        Amount = 0;
                    else
                        Amount = Convert.ToDecimal(ds.Tables[0].Rows[0][0].ToString());
                }
            }
            return Amount;
        }


        public static string GetDayClose(int CollectedEmpId, DateTime CollectedDate)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_CheckCourierDayClose";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@EMPID", CollectedEmpId);
            cmd.Parameters.AddWithValue("@TransDate", CollectedDate.ToString("MM/dd/yyyy"));
       
            decimal Amount = 0;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<AWBList> objList = new List<AWBList>();
            AWBList obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    if (ds.Tables[0].Rows[0][0].ToString() == "Valid")
                        return "OK";
                    else
                        return "Failed";
                }
            }
            return "Failed";
        }

        public static List<CourierCollectionVM> GetCourierCollectedAWB(int CollectedEmpId, DateTime CollectedDate,int DRSReceiptId=0)
        {
            List<CourierCollectionVM> list = new List<CourierCollectionVM>();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetCourierCollectedAWB";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CollectedDate", CollectedDate);
            cmd.Parameters.AddWithValue("@EmployeeId", CollectedEmpId);
            cmd.Parameters.AddWithValue("@DRSReceiptId", DRSReceiptId);
            decimal Amount = 0;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
             
            CourierCollectionVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new CourierCollectionVM();
                    obj.CollectionId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CollectionId"].ToString());
                    obj.AWBNo = ds.Tables[0].Rows[i]["AWBNo"].ToString();
                    obj.PickupCash = Convert.ToDecimal(ds.Tables[0].Rows[i]["PickupCash"].ToString());
                    obj.COD = Convert.ToDecimal(ds.Tables[0].Rows[i]["COD"].ToString());
                    obj.OtherAmount = Convert.ToDecimal(ds.Tables[0].Rows[i]["OtherAmount"].ToString());
                    obj.MaterialCost = Convert.ToDecimal(ds.Tables[0].Rows[i]["MaterialCost"].ToString());
                    obj.ExpenseAmount = Convert.ToDecimal(ds.Tables[0].Rows[i]["ExpenseAmount"].ToString());
                    obj.CollectionType = ds.Tables[0].Rows[i]["CollectionType"].ToString();
                    obj.Remarks = ds.Tables[0].Rows[i]["Remarks"].ToString();
                    list.Add(obj);
                }
            }
            return list;
        }
        public static string SaveCourierCollectedReceiptId(string CollectionIds, string Expenses = "", int DRSReceiptId = 0)
        {
            List<CourierCollection> list = new List<CourierCollection>();
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_SaveCourierCollection";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@DRSRecpayID", DRSReceiptId);
                cmd.Parameters.AddWithValue("@Collections", CollectionIds);
                cmd.Parameters.AddWithValue("@Expenses", Expenses);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();

                return "OK";
            }
            catch(Exception ex)
            {
                return ex.Message;
            }

        }
        public static List<DRSVM> GetOutScanList(DateTime FromDate, DateTime ToDate, int FyearId, int BranchId, int DepotId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetOutScanList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 0;
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@FYearId", FyearId);
            cmd.Parameters.AddWithValue("@BranchID", BranchId);
            cmd.Parameters.AddWithValue("@DepotId", DepotId);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<DRSVM> objList = new List<DRSVM>();
            DRSVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new DRSVM();
                    obj.DRSID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["DRSID"].ToString());
                    obj.DRSNo = ds.Tables[0].Rows[i]["DRSNo"].ToString();
                    obj.DRSDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["DRSDate"].ToString());
                    obj.Deliver = ds.Tables[0].Rows[i]["DeliveredBy"].ToString();
                    obj.CourierName = ds.Tables[0].Rows[i]["CourierName"].ToString();
                    obj.vehicle = ds.Tables[0].Rows[i]["vehicle"].ToString();
                    obj.TotalCourierCharge= CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["TotalCourierCharge"].ToString());
                    obj.TotalMaterialCost = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["TotalMaterialCost"].ToString());
                    obj.TotalVAT = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["TotalVAT"].ToString());
                    obj.TotalAWB=CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["TotalAWB"].ToString());
                    obj.APISuccess = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["APISuccess"].ToString());
                    obj.APIFailed = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["APIFailed"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static List<PODVM> GetPODList(DateTime FromDate, DateTime ToDate, int FyearId, int BranchId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetPODList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@FYearId", FyearId);
            cmd.Parameters.AddWithValue("@BranchID", BranchId);
            
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<PODVM> objList = new List<PODVM>();
            PODVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new PODVM();
                    obj.PODID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["PODID"].ToString());
                    obj.AWBNo = ds.Tables[0].Rows[i]["RefNo"].ToString();
                    obj.DeliveredDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["DeliveredDate"].ToString());
                    obj.Shipper = ds.Tables[0].Rows[i]["Shipper"].ToString();
                    obj.Consignee = ds.Tables[0].Rows[i]["Consignee"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static List<ShipmentInvoiceVM> GetShipmentMAWBList(DateTime FromDate,DateTime ToDate,int BranchID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetShipmentMAWB";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@BranchID", BranchID);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<ShipmentInvoiceVM> objList = new List<ShipmentInvoiceVM>(); 
            ShipmentInvoiceVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new ShipmentInvoiceVM();
                    obj.MAWB = ds.Tables[0].Rows[i]["MAWB"].ToString();
                    obj.ShipmentImportID = Convert.ToInt32(ds.Tables[0].Rows[i]["ShipmentImportID"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static List<ShipmentInvoiceDetailVM> GetShipmentforTAXInvoice(string MAWB, int ShipmentImportID,int BranchID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetimportAWBforTax";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MAWB", MAWB);
            cmd.Parameters.AddWithValue("@ShipmentImportID", ShipmentImportID);
            cmd.Parameters.AddWithValue("@BranchID",BranchID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<ShipmentInvoiceDetailVM> objList = new List<ShipmentInvoiceDetailVM>();
            ShipmentInvoiceDetailVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new ShipmentInvoiceDetailVM();
                    obj.AWBNo = ds.Tables[0].Rows[i]["AWBNo"].ToString();
                    obj.AWBDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["AWBDate"].ToString());
                    obj.Shipper = ds.Tables[0].Rows[i]["Shipper"].ToString();
                    obj.Receiver = ds.Tables[0].Rows[i]["Receiver"].ToString();
                    obj.CurrencyID = Convert.ToInt32(ds.Tables[0].Rows[i]["CurrencyID"].ToString());
                    obj.ExchangeRate = Convert.ToDecimal (ds.Tables[0].Rows[i]["ExchangeRate"].ToString());
                    obj.BagNo = ds.Tables[0].Rows[i]["BagNo"].ToString();
                    obj.CurrencyName = ds.Tables[0].Rows[i]["CurrencyName"].ToString();
                    obj.ShipmentImportDetailID = Convert.ToInt32(ds.Tables[0].Rows[i]["ShipmentImportDetailID"].ToString());
                    obj.TaxP = Convert.ToDecimal(ds.Tables[0].Rows[i]["TaxP"].ToString());
                    obj.Tax = Convert.ToDecimal(ds.Tables[0].Rows[i]["Tax"].ToString());
                    obj.CustomValue = Convert.ToDecimal(ds.Tables[0].Rows[i]["CustomValue"].ToString());
                    obj.adminCharges = Convert.ToDecimal(ds.Tables[0].Rows[i]["AdminCharges"].ToString());
                    obj.AWBChecked = Convert.ToBoolean(ds.Tables[0].Rows[i]["AWBChecked"].ToString());
                    obj.InvoiceValue = Convert.ToDecimal(ds.Tables[0].Rows[i]["InvoiceValue"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static List<ShipmentInvoiceDetailVM> GetShipmentforTAXInvoiceDetail(int ShipmentInvoiceID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_ImportTaxInvoiceDetail";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ShipmentInvoiceID", ShipmentInvoiceID);
             
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<ShipmentInvoiceDetailVM> objList = new List<ShipmentInvoiceDetailVM>();
            ShipmentInvoiceDetailVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new ShipmentInvoiceDetailVM();
                    
                    obj.ShipmentInvoiceDetailID =Convert.ToInt32(ds.Tables[0].Rows[i]["ShipmentInvoiceDetailID"].ToString());
                    obj.ShipmentInvoiceID = Convert.ToInt32(ds.Tables[0].Rows[i]["ShipmentInvoiceID"].ToString());
                    obj.AWBNo = ds.Tables[0].Rows[i]["AWBNo"].ToString();
                    obj.AWBDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["AWBDate"].ToString());
                    obj.Shipper = ds.Tables[0].Rows[i]["Consignor"].ToString();
                    obj.Receiver = ds.Tables[0].Rows[i]["Consignee"].ToString();
                    obj.CurrencyID = Convert.ToInt32(ds.Tables[0].Rows[i]["CurrencyID"].ToString());
                    obj.CurrencyName = ds.Tables[0].Rows[i]["CurrencyName"].ToString();

                    obj.ShipmentImportDetailID = Convert.ToInt32(ds.Tables[0].Rows[i]["ShipmentImportDetailID"].ToString());
                    obj.CustomValue = Convert.ToDecimal(ds.Tables[0].Rows[i]["CustomsValue"].ToString());
                    obj.TaxP = Convert.ToDecimal(ds.Tables[0].Rows[i]["TaxP"].ToString());
                    obj.Tax = Convert.ToDecimal(ds.Tables[0].Rows[i]["Tax"].ToString());
                    obj.ExchangeRate = Convert.ToDecimal(ds.Tables[0].Rows[i]["ExchangeRate"].ToString());
                    obj.adminCharges = Convert.ToDecimal(ds.Tables[0].Rows[i]["AdminCharges"].ToString());
                    obj.AWBChecked = Convert.ToBoolean(ds.Tables[0].Rows[i]["AWBChecked"].ToString());
                    obj.InvoiceValue = Convert.ToDecimal(ds.Tables[0].Rows[i]["InvoiceValue"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static List<ShipmentInvoiceVM> GetTaxInvoiceList( string AWBNo, DateTime FromDate, DateTime ToDate, int FyearId, int BranchId, int DepotId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetTAxInvoiceList";
            cmd.CommandType = CommandType.StoredProcedure;
            if (AWBNo==null)
                cmd.Parameters.AddWithValue("@AWBNo", "");
            else
                cmd.Parameters.AddWithValue("@AWBNo", AWBNo);
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@FYearId", FyearId);
            cmd.Parameters.AddWithValue("@BranchID", BranchId);
            

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<ShipmentInvoiceVM> objList = new List<ShipmentInvoiceVM>();
            ShipmentInvoiceVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new ShipmentInvoiceVM();
                    obj.ShipmentInvoiceID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ShipmentInvoiceID"].ToString());
                    obj.InvoiceNo = ds.Tables[0].Rows[i]["InvoiceNo"].ToString();
                    obj.InvoiceDate  = Convert.ToDateTime(ds.Tables[0].Rows[i]["InvoiceDate"].ToString());
                    obj.InvoiceTotal  = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["InvoiceTotal"].ToString());
                    obj.EnteredBy = ds.Tables[0].Rows[i]["EnteredBy"].ToString();
                    obj.MAWB = ds.Tables[0].Rows[i]["MAWB"].ToString();
                    obj.VatTotal = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["VatTotal"].ToString());
                    obj.AdminCharges = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["AdminCharges"].ToString());
                    //if (ds.Tables[0].Rows[i]["ImportDate"]!=System.DBNull.Value)
                    //    obj.ImportDate =  Convert.ToDateTime(ds.Tables[0].Rows[i]["ImportDate"].ToString());

                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static List<CustomerContractVM> GetCustomerContracts(int CustomerId,string CourierType)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetCustomerContracts";
            cmd.CommandType = CommandType.StoredProcedure;
            //cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            //cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
            cmd.Parameters.AddWithValue("@CourierType", "");
            //cmd.Parameters.AddWithValue("@BranchID", BranchId);
            //cmd.Parameters.AddWithValue("@DepotId", DepotId);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<CustomerContractVM> objList = new List<CustomerContractVM>();
            CustomerContractVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new CustomerContractVM();
                    obj.ID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ID"].ToString());
                    obj.CustomerID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CustomerID"].ToString());
                    obj.CustomerRateTypeID= Convert.ToInt32(ds.Tables[0].Rows[i]["CustomerRateTypeID"].ToString());
                    obj.CustomerRateType = ds.Tables[0].Rows[i]["CustomerRateType"].ToString();
                    obj.CustomerName = ds.Tables[0].Rows[i]["CustomerName"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static List<ZoneChartVM> GetZoneChartList()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetZoneChartList";
            cmd.CommandType = CommandType.StoredProcedure;           

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<ZoneChartVM> objList = new List<ZoneChartVM>();
            ZoneChartVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new ZoneChartVM();
                    obj.ZoneChartID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ZoneChartID"].ToString());
                    obj.ZoneCategoryID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ZoneCategoryID"].ToString());
                    obj.ZoneID = Convert.ToInt32(ds.Tables[0].Rows[i]["ZoneID"].ToString());
                    obj.ZoneName = ds.Tables[0].Rows[i]["ZoneName"].ToString();
                    obj.ZoneCategory = ds.Tables[0].Rows[i]["ZoneCategory"].ToString();
                    obj.ZoneType = ds.Tables[0].Rows[i]["ZoneType"].ToString();
                    obj.Cities = ds.Tables[0].Rows[i]["Cities"].ToString();
                    obj.Countries = ds.Tables[0].Rows[i]["Countries"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static List<ZoneChartDetailsVM> GetZoneChartDetail(int ZoneChartID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetZoneChartDetail";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ZoneChartID", ZoneChartID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<ZoneChartDetailsVM> objList = new List<ZoneChartDetailsVM>();
            ZoneChartDetailsVM  obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new ZoneChartDetailsVM();
                    obj.ZoneChartID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ZoneChartID"].ToString());
                    obj.ZoneChartDetailID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ZoneChartDetailID"].ToString());
                    obj.CityID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CityID"].ToString());
                    obj.CountryID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CountryID"].ToString());
                    
                    obj.CityName = ds.Tables[0].Rows[i]["CityName"].ToString();
                    obj.CountryName = ds.Tables[0].Rows[i]["CountryName"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static List<CustRateVM> GetCustomerRateList()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetCustomerRates";
            cmd.CommandType = CommandType.StoredProcedure;

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<CustRateVM> objList = new List<CustRateVM>();
            CustRateVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new CustRateVM();
                    obj.CustomerRateID   = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CustomerRateID"].ToString());
                    obj.CustomerRateTypeID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CustomerRateTypeID"].ToString());                    
                    obj.CustomerRateType = ds.Tables[0].Rows[i]["CustomerRateType"].ToString();
                    obj.ZoneCategory = ds.Tables[0].Rows[i]["ZoneCategory"].ToString();
                    obj.ZoneName= ds.Tables[0].Rows[i]["ZoneName"].ToString();
                    obj.ProductName = ds.Tables[0].Rows[i]["ProductName"].ToString();
                    obj.FAgentName = ds.Tables[0].Rows[i]["FAgentName"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static decimal GetSurcharge()
        {
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_GetRecentSurcharge";
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                List<CustRateVM> objList = new List<CustRateVM>();
                CustRateVM obj;
                if (ds != null && ds.Tables.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        return Convert.ToDecimal(ds.Tables[0].Rows[0][0].ToString());
                    }
                }
            }catch(Exception ex)
            {
                return 0;
            }
            return 0;
            //SP_GetRecentSurcharge
        }
        #region "HoldReleasePage"
        public static HoldVM GetAWBDetailsForHold(string AWBNo,int InscanId,int ShipmentDetailId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetShipmentDetailForHold";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AWBNo", AWBNo);
            cmd.Parameters.AddWithValue("@InScanId", InscanId);
            cmd.Parameters.AddWithValue("@ShipmentDetailId",ShipmentDetailId);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<HoldVM> objList = new List<HoldVM>();
            HoldVM obj = new HoldVM();
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {                    
                    obj.InScanID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["InScanId"].ToString());
                    obj.ShipmentDetailID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ShipmentDetailId"].ToString());
                    obj.Consignor = ds.Tables[0].Rows[i]["Consignor"].ToString();
                    obj.Consignee = ds.Tables[0].Rows[i]["Consignee"].ToString();
                    obj.Pieces = ds.Tables[0].Rows[i]["Pieces"].ToString();
                    obj.CollectedByName = ds.Tables[0].Rows[i]["CollectedBy"].ToString();
                    obj.Weight = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Weight"].ToString());
                    obj.CourierCharges = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["CourierCharges"].ToString());
                    obj.StatusPaymentMOde = ds.Tables[0].Rows[i]["StatusPaymentMOde"].ToString();
                    obj.OriginCountry = ds.Tables[0].Rows[i]["OriginCountry"].ToString();
                    obj.ConsigneeCountry = ds.Tables[0].Rows[i]["ConsigneeCountry"].ToString();
                    obj.AWBNo = ds.Tables[0].Rows[i]["AWBNo"].ToString();
                    obj.AWBType = ds.Tables[0].Rows[i]["Type"].ToString();
                    obj.CourierStatus = ds.Tables[0].Rows[i]["CourierStatus"].ToString();
                    obj.CourierStatusID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CourierStatusID"].ToString());
                    obj.AWBDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["AWBDate"].ToString());
                    if (obj.CourierStatusID ==5 || obj.CourierStatusID==21 || obj.CourierStatusID==22 || obj.CourierStatusID==19)
                    {
                        obj.ActionType = "On-Hold";
                    }
                    else if (obj.CourierStatusID==18)
                    {
                        obj.ActionType = "Released";
                    }
                    obj.HistoryDetails = GetHoldHistoryList(AWBNo);
                }
            }
            return obj;
        }

        public static List<HoldVM> GetHoldAWBList(DateTime FromDate,DateTime ToDate,int BranchId, int StatusId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetHoldList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ActionType", StatusId);
            cmd.Parameters.AddWithValue("@BranchId", BranchId);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<HoldVM> objList = new List<HoldVM>();
            HoldVM obj = new HoldVM();
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new HoldVM();
                    obj.HoldReleaseID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["HoldReleaseid"].ToString());
                    obj.AWBDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["AWBDate"].ToString());
                    obj.EntryDate =Convert.ToDateTime(ds.Tables[0].Rows[i]["EntryDate"].ToString());
                    obj.InScanID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["InScanId"].ToString());
                    obj.ShipmentDetailID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ShipmentDetailId"].ToString());                                        
                    obj.EnteredBy = ds.Tables[0].Rows[i]["EnteredBy"].ToString();
                    obj.AWBNo = ds.Tables[0].Rows[i]["AWBNo"].ToString();
                    obj.ActionType = ds.Tables[0].Rows[i]["ActionType"].ToString();                    
                    obj.CourierStatus = ds.Tables[0].Rows[i]["CourierStatus"].ToString();
                    obj.Remarks = ds.Tables[0].Rows[i]["Remarks"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static List<HoldVM> GetHoldHistoryList(string AWBNo, int InscanId=0,int ShipmentDetailId=0)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetHoldHistoryList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AWBNo",AWBNo);
            cmd.Parameters.AddWithValue("@InscanId", InscanId);
            cmd.Parameters.AddWithValue("@ShipmentDetailId", ShipmentDetailId);
            
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<HoldVM> objList = new List<HoldVM>();
            HoldVM obj = new HoldVM();
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new HoldVM();
                    obj.HoldReleaseID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["HoldReleaseid"].ToString());
                    obj.AWBDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["AWBDate"].ToString());
                    obj.EntryDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["EntryDate"].ToString());
                    obj.InScanID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["InScanId"].ToString());
                    obj.ShipmentDetailID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ShipmentDetailId"].ToString());
                    obj.EnteredBy = ds.Tables[0].Rows[i]["EnteredBy"].ToString();
                    obj.AWBNo = ds.Tables[0].Rows[i]["AWBNo"].ToString();
                    obj.ActionType = ds.Tables[0].Rows[i]["ActionType"].ToString();
                    obj.CourierStatus = ds.Tables[0].Rows[i]["CourierStatus"].ToString();
                    obj.Remarks = ds.Tables[0].Rows[i]["Remarks"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }
        #endregion
        #region "COLOADER Invoice"
        public static List<AgentInvoiceVM> GetAgentInvoiceList(DateTime FromDate, DateTime ToDate, string InvoiceNo, int FyearId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetAgentInvoiceList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@FYearId", FyearId);
            if (InvoiceNo == null)
                InvoiceNo = "";
            cmd.Parameters.AddWithValue("@InvoiceNo", InvoiceNo);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<AgentInvoiceVM> objList = new List<AgentInvoiceVM>();
            AgentInvoiceVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new AgentInvoiceVM();
                    obj.AgentInvoiceID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AgentInvoiceID"].ToString());
                    obj.InvoiceNo = ds.Tables[0].Rows[i]["InvoiceNo"].ToString();
                    obj.InvoiceDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["InvoiceDate"].ToString());
                    obj.CustomerName = ds.Tables[0].Rows[i]["CustomerName"].ToString();
                    obj.CustomerID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CustomerID"].ToString());
                    obj.InvoiceTotal = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["InvoiceTotal"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static List<AgentInvoiceDetailVM> GetAgentShipmentList(int CustomerId,DateTime FromDate, DateTime ToDate, string MAWB,string MovementId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetAgentAWBList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            
            if (MAWB == null)
                MAWB = "";
            cmd.Parameters.AddWithValue("@MAWB", MAWB);

            cmd.Parameters.AddWithValue("@MovementId", MovementId);
           
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<AgentInvoiceDetailVM> objList = new List<AgentInvoiceDetailVM>();
            AgentInvoiceDetailVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new AgentInvoiceDetailVM();
                    obj.AgentInvoiceDetailID = 0;
                    obj.AgentInvoiceID = 0;
                    obj.ShipmentID = Convert.ToInt32(ds.Tables[0].Rows[i]["ShipmentID"].ToString());
                    obj.InscanID = Convert.ToInt32(ds.Tables[0].Rows[i]["InscanID"].ToString());
                    obj.AWBDateTime= Convert.ToDateTime(ds.Tables[0].Rows[i]["AWBDate"].ToString());
                    obj.AWBNo = ds.Tables[0].Rows[i]["AWBNo"].ToString();                                        
                    obj.ConsigneeName = ds.Tables[0].Rows[i]["Consignee"].ToString();
                    obj.ConsigneeCountryName = ds.Tables[0].Rows[i]["ConsigneeCountryName"].ToString();                    
                    obj.CourierCharge= CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["CourierCharge"].ToString());
                    obj.OtherCharge = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["OtherCharge"].ToString());
                    obj.TaxPercentage = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["TaxPercent"].ToString());
                    obj.VATAmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["TaxAmount"].ToString());
                    obj.SurchargePercent = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["SurchargePercent"].ToString());
                    obj.FuelSurcharge = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["SurchargeAmount"].ToString());
                    obj.AWBChecked = true;  
                    obj.NetValue = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["NetTotal"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }
        #endregion


        #region "Transhipment"
        public static List<TranshipmentAWB> CheckTranshipment(DateTime ManifestDate, string MAWB, string AWBNos)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetTranshipmentDuplicate";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ManifestDate", ManifestDate.ToString("MM/dd/yyyy"));            
            cmd.Parameters.AddWithValue("@MAWB", MAWB);
            if (AWBNos == null)
                AWBNos = "";
            cmd.Parameters.AddWithValue("@AWBNos", AWBNos);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<TranshipmentAWB> objList = new List<TranshipmentAWB>();
            TranshipmentAWB obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new TranshipmentAWB();
                    obj.InScanId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["InscanId"].ToString());
                    obj.AWBNo = ds.Tables[0].Rows[i]["AWBNo"].ToString();                    
                    obj.Status = ds.Tables[0].Rows[i]["Status"].ToString();
                    obj.Message = ds.Tables[0].Rows[i]["Message"].ToString();

                    objList.Add(obj);
                }
            }
            return objList;
        }
        #endregion

        #region "VehcileList"
        public static List<VehiclesVM> GetVehiclesVM()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_VehicleList";
            cmd.CommandType = CommandType.StoredProcedure;

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<VehiclesVM> objList = new List<VehiclesVM>();
            VehiclesVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new VehiclesVM();
                    obj.VehicleID = Convert.ToInt32(ds.Tables[0].Rows[i]["VehicleID"].ToString());
                    obj.VehicleDescription = ds.Tables[0].Rows[i]["VehicleDescription"].ToString();
                    obj.VehicleNO =ds.Tables[0].Rows[i]["VehicleNo"].ToString();
                    obj.RegistrationNo = ds.Tables[0].Rows[i]["RegistrationNo"].ToString();
                    obj.Model = ds.Tables[0].Rows[i]["Model"].ToString();                    
                    obj.EmployeeName = ds.Tables[0].Rows[i]["EmployeeName"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }


        public static List<VehicleBinMasterVM> GetVehiclesBinList()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetVehicleBinList";
            cmd.CommandType = CommandType.StoredProcedure;

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<VehicleBinMasterVM> objList = new List<VehicleBinMasterVM>();
            VehicleBinMasterVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                  
                    obj = new VehicleBinMasterVM();
                    obj.VehicleBinID     = Convert.ToInt32(ds.Tables[0].Rows[i]["VehicleBinID"].ToString());
                    obj.VehicleId = Convert.ToInt32(ds.Tables[0].Rows[i]["VehicleId"].ToString());
                    obj.VehicleName = ds.Tables[0].Rows[i]["Vehiclename"].ToString();
                    obj.BinIds = ds.Tables[0].Rows[i]["BinIds"].ToString();
                    obj.BinDetail = ds.Tables[0].Rows[i]["BinDetail"].ToString();
                    
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static List<VehicleMaster> GetPendingVehicleforBin()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "Select VehicleID,VehicleName = c.RegistrationNo + ' - ' + c.VehicleDescription from VehicleMaster c where VehicleId not in(select VehicleID from tblVehicleBin)";
            cmd.CommandType = CommandType.Text;

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<VehicleMaster> objList = new List<VehicleMaster>();
            VehicleMaster obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {

                    obj = new VehicleMaster();
                    
                    obj.VehicleID = Convert.ToInt32(ds.Tables[0].Rows[i]["VehicleID"].ToString());
                    obj.VehicleDescription = ds.Tables[0].Rows[i]["VehicleName"].ToString();
                    

                    objList.Add(obj);
                }
            }
            return objList;
        }
        #endregion

        #region "CustomerInvoice"
        public static List<CustomerInvoiceVM> GetInvoiceList(DateTime FromDate, DateTime ToDate, string InvoiceNo, int FyearId)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetInvoiceList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@FYearId", FyearId);

            if (InvoiceNo == null)
                InvoiceNo = "";
            cmd.Parameters.AddWithValue("@InvoiceNo", @InvoiceNo);

            cmd.Parameters.AddWithValue("@BranchID", branchid);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<CustomerInvoiceVM> objList = new List<CustomerInvoiceVM>();
            CustomerInvoiceVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new CustomerInvoiceVM();
                    obj.CustomerInvoiceID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CustomerInvoiceID"].ToString());
                    obj.CustomerInvoiceNo = ds.Tables[0].Rows[i]["CustomerInvoiceNo"].ToString();
                    obj.InvoiceDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["InvoiceDate"].ToString());
                    obj.CustomerName = ds.Tables[0].Rows[i]["CustomerName"].ToString();
                    obj.CustomerID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CustomerID"].ToString());
                    obj.InvoiceTotal = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["InvoiceTotal"].ToString());
                    obj.TotalAWB = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["TotalAWB"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static int CheckInvoiceReceipt(int InvoiceId,int CustomerId)
        {
            int ReceiptId = 0;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_CheckInvoiceReceipt";
            cmd.CommandType = CommandType.StoredProcedure;            
            cmd.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            
            if (ds != null && ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count>0)
                {
                    ReceiptId  = Convert.ToInt32(ds.Tables[0].Rows[0]["RecPayID"]);
                }
            }

            return ReceiptId;

          }
        public static List<CustomerInvoiceDetailVM> GetCustomerAWBforInvoice(int CustomerId, DateTime FromDate, DateTime ToDate,string MovementId,int InvoiceId)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetCustomerAWBForInvoice";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@MovementId", MovementId);
            cmd.Parameters.AddWithValue("@BranchID",branchid);
            cmd.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<CustomerInvoiceDetailVM> objList = new List<CustomerInvoiceDetailVM>();
            CustomerInvoiceDetailVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new CustomerInvoiceDetailVM();
                    obj.CustomerInvoiceID = 0;
                    obj.CustomerInvoiceDetailID = 0;
                    obj.InscanID = Convert.ToInt32(ds.Tables[0].Rows[i]["InscanID"].ToString());
                    obj.AWBDateTime = Convert.ToDateTime(ds.Tables[0].Rows[i]["TransactionDate"].ToString());
                    obj.AWBNo = ds.Tables[0].Rows[i]["AWBNo"].ToString();
                    obj.ConsigneeName = ds.Tables[0].Rows[i]["Consignee"].ToString();
                    obj.ConsigneeCountryName = ds.Tables[0].Rows[i]["ConsigneeCountryName"].ToString();
                    obj.CourierCharge = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["CourierCharge"].ToString());
                    obj.OtherCharge = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["OtherCharge"].ToString());
                    obj.VATAmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["VATAmount"].ToString());                    
                    obj.SurchargePercent = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["FuelSurchargePercent"].ToString());
                    obj.FuelSurcharge = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["FuelSurchargeAmount"].ToString());
                    obj.NetValue = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["NetTotal"].ToString());
                    obj.Weight = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Weight"].ToString());
                    obj.Pieces = ds.Tables[0].Rows[i]["Pieces"].ToString();
                    obj.AWBChecked =false;
                    objList.Add(obj);
                }
            }
            return objList;
        }
        #endregion
        #region "CODInvoice"
        public static List<CODInvoiceVM> GetCODInvoiceList(DateTime FromDate, DateTime ToDate, string InvoiceNo, int FyearId)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetCODInvoiceList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@FYearId", FyearId);

            if (InvoiceNo == null)
                InvoiceNo = "";
            cmd.Parameters.AddWithValue("@InvoiceNo", @InvoiceNo);

            cmd.Parameters.AddWithValue("@BranchID", branchid);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<CODInvoiceVM> objList = new List<CODInvoiceVM>();
            CODInvoiceVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new CODInvoiceVM();
                    obj.CODInvoiceID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CODInvoiceID"].ToString());
                    obj.InvoiceNo = ds.Tables[0].Rows[i]["InvoiceNo"].ToString();
                    obj.InvoiceDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["InvoiceDate"].ToString());
                    obj.CustomerName = ds.Tables[0].Rows[i]["CustomerName"].ToString();
                    obj.CustomerID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CustomerID"].ToString());
                    obj.InvoiceTotal = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["InvoiceTotal"].ToString());
                    obj.TotalAWB = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["TotalAWB"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static int CheckCODInvoiceReceipt(int InvoiceId, int CustomerId)
        {
            int ReceiptId = 0;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_CheckInvoiceReceipt";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            if (ds != null && ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    ReceiptId = Convert.ToInt32(ds.Tables[0].Rows[0]["RecPayID"]);
                }
            }

            return ReceiptId;

        }
        public static List<CODInvoiceDetailVM> GetCODAWBforInvoice(string CustomerName, DateTime FromDate, DateTime ToDate, string MovementId,string Type)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetCODAWBForInvoice";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CustomerName", CustomerName);
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@MovementId", MovementId);
            cmd.Parameters.AddWithValue("@BranchID", branchid);
            cmd.Parameters.AddWithValue("@Type", Type);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<CODInvoiceDetailVM> objList = new List<CODInvoiceDetailVM>();
            CODInvoiceDetailVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new CODInvoiceDetailVM();
                    obj.CODInvoiceID = 0;
                    obj.CODInvoiceDetailID = 0;
                    obj.InScanID = Convert.ToInt32(ds.Tables[0].Rows[i]["InscanID"].ToString());
                    obj.AWBDateTime = Convert.ToDateTime(ds.Tables[0].Rows[i]["TransactionDate"].ToString());
                    obj.AWBNo = ds.Tables[0].Rows[i]["AWBNo"].ToString();
                    obj.ConsigneeName = ds.Tables[0].Rows[i]["Consignee"].ToString();
                    obj.ConsigneeCountryName = ds.Tables[0].Rows[i]["ConsigneeCountryName"].ToString();
                    obj.ConsignorCountryName = ds.Tables[0].Rows[i]["ConsignorCountryName"].ToString();
                    obj.Consignor = ds.Tables[0].Rows[i]["Consignor"].ToString();
                    obj.CourierCharge = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["CourierCharge"].ToString());
                    obj.OtherCharge = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["OtherCharge"].ToString());
                    obj.VATAmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["VATAmount"].ToString());
                    obj.SurchargePercent = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["FuelSurchargePercent"].ToString());
                    obj.FuelSurcharge = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["FuelSurchargeAmount"].ToString());
                    obj.NetValue = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["NetTotal"].ToString());
                    obj.AWBChecked = false;
                    objList.Add(obj);
                }
            }
            return objList;
        }
        #endregion
        #region ForwardingAgentInvice
        public static List<SupplierInvoiceVM> GetForwardingInvoiceList(DateTime FromDate, DateTime ToDate, string InvoiceNo, int FyearId)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetForwardingInvoiceList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@FYearId", FyearId);

            if (InvoiceNo == null)
                InvoiceNo = "";
            cmd.Parameters.AddWithValue("@InvoiceNo", @InvoiceNo);

            cmd.Parameters.AddWithValue("@BranchID", branchid);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<SupplierInvoiceVM> objList = new List<SupplierInvoiceVM>();
            SupplierInvoiceVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new SupplierInvoiceVM();
                    obj.SupplierInvoiceID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["SupplierInvoiceID"].ToString());
                    obj.InvoiceNo = ds.Tables[0].Rows[i]["InvoiceNo"].ToString();
                    obj.InvoiceDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["InvoiceDate"].ToString());
                    obj.SupplierName = ds.Tables[0].Rows[i]["SupplierName"].ToString();
                    obj.SupplierID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["SupplierID"].ToString());
                    obj.InvoiceTotal = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["InvoiceTotal"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static int GetFowardingAWBCost(int SupplierId, DateTime FromDate, DateTime ToDate, string MovementId,DateTime InvoiceDate, string Remarks,string InvoiceNo,decimal FuelPercentage,decimal AdditionalCharges )
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int UserId=Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            int FYearId = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int SupplierInvoiceId = 0;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetFWDSupplierInvoice";// old procedure "SP_GetFWDSupplierAWBForInvoice1";
            cmd.CommandType = CommandType.StoredProcedure;
            
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@SupplierId", SupplierId);
            cmd.Parameters.AddWithValue("@MovementId", MovementId);
            cmd.Parameters.AddWithValue("@BranchID", branchid);
            cmd.Parameters.AddWithValue("@InvoiceDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@Remarks", Remarks);
            cmd.Parameters.AddWithValue("@UserId", FYearId);
            cmd.Parameters.AddWithValue("@EntryDate", CommonFunctions.GetCurrentDateTime().ToString("MM/dd/yyyy hh:mm"));
            cmd.Parameters.AddWithValue("@InvoiceNo", InvoiceNo);            
            cmd.Parameters.AddWithValue("@FYearId", FYearId);
            cmd.Parameters.AddWithValue("@FuelPercentage", FuelPercentage);
            cmd.Parameters.AddWithValue("@AdditionalCharges", AdditionalCharges);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                     SupplierInvoiceId = Convert.ToInt32(ds.Tables[0].Rows[i]["SupplierInvoiceID"].ToString());                    
                }
            }

            return SupplierInvoiceId;
        }


        public static DataTable DeleteForwardingInvoice(int InvoiceId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteFowardingAgentInvoice";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@InvoiceId", InvoiceId);
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
        #endregion

        #region "MaterialcostLedger"

        public static DataTable DeleteMCPaymentVoucher(int Id)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteMCPaymentVoucherPrint";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Id", Id);
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

        public static DataTable DeleteMCPaymentPrintAWB(int MCPaymentVoucherDetailID, int MCPaymentVoucherID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteMCPaymentPrintAWB";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MCPaymentVoucherDetailID", MCPaymentVoucherDetailID);
            cmd.Parameters.AddWithValue("@MCPaymentVoucherID", MCPaymentVoucherID);
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
        public static List<MCPaymentAWB> GenerateMaterialCostPrintVoucher()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            MCPaymentPrint reportparam = (MCPaymentPrint)(HttpContext.Current.Session["MCPaymentPrint"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_MaterialVoucherPrintPending";
            //comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            //comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            //if (reportparam.Shipper == null)
            //    reportparam.Shipper = "";
            //if (reportparam.Receiver == null)
            //    reportparam.Receiver = "";
            //comd.Parameters.AddWithValue("@Shipper", reportparam.Shipper);
            //comd.Parameters.AddWithValue("@Receiver", reportparam.Receiver);
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            //comd.Parameters.AddWithValue("@Printed", reportparam.Printed);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "MaterialCostLedger");

            List<MCPaymentAWB> objList = new List<MCPaymentAWB>();
            MCPaymentAWB obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new MCPaymentAWB();
                    obj.DRRDetailID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["DRRDetailID"].ToString());
                    obj.InScanID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["InScanID"].ToString());
                    obj.AWBNo = ds.Tables[0].Rows[i]["AWBNo"].ToString();
                    obj.DocumentNo = ds.Tables[0].Rows[i]["DocumentNo"].ToString();
                    obj.Consignor = ds.Tables[0].Rows[i]["Consignor"].ToString();
                    obj.Consignee = ds.Tables[0].Rows[i]["Consignee"].ToString();
                    obj.DRRDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["DRRDate"].ToString());

                    obj.MCAmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["MaterialCost"].ToString());
                    obj.Receivedmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["MCReceivedAmt"].ToString());
                    obj.PaidAmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["PaidAmount"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;

        }

        public static MCPaymentPrint GetMCPaymentPrintVoucherPending()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();
            MCPaymentPrint mcpayment = new MCPaymentPrint();
            MCPaymentPrint reportparam = (MCPaymentPrint)(HttpContext.Current.Session["MCPaymentPrint"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_MaterialVoucherPrintPending";           
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
          
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "MaterialCostLedger");
            DataTable dt1 = ds.Tables[0];
            DataTable dt2 = ds.Tables[1];

            List<MCPaymentConsignor> objList1 = new List<MCPaymentConsignor>();
            MCPaymentConsignor obj1;

            List<MCPaymentAWB> objList = new List<MCPaymentAWB>();
            MCPaymentAWB obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    obj1 = new MCPaymentConsignor(); 
                    //obj.DRRDetailID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["DRRDetailID"].ToString());
                    //obj.InScanID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["InScanID"].ToString());
                    //obj.AWBNo = ds.Tables[0].Rows[i]["AWBNo"].ToString();
                    //obj.DocumentNo = ds.Tables[0].Rows[i]["DocumentNo"].ToString();
                    obj1.Consignor = ds.Tables[0].Rows[i]["Consignor"].ToString();
                    //obj.Consignee = ds.Tables[0].Rows[i]["Consignee"].ToString();
                    //obj.DRRDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["DRRDate"].ToString());

                    //obj.MCAmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["MaterialCost"].ToString());
                    //obj.Receivedmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["MCReceivedAmt"].ToString());
                    //obj.PaidAmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["PaidAmount"].ToString());
                    objList1.Add(obj1);
                }

                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    obj = new MCPaymentAWB();
                    obj.DRRDetailID = CommonFunctions.ParseInt(dt2.Rows[i]["DRRDetailID"].ToString());
                    obj.InScanID = CommonFunctions.ParseInt(dt2.Rows[i]["InScanID"].ToString());
                    obj.AWBNo = dt2.Rows[i]["AWBNo"].ToString();
                    obj.DocumentNo = dt2.Rows[i]["DocumentNo"].ToString();
                    obj.Consignor = dt2.Rows[i]["Consignor"].ToString();
                    obj.Consignee = dt2.Rows[i]["Consignee"].ToString();
                    obj.DRRDate = Convert.ToDateTime(dt2.Rows[i]["DRRDate"].ToString());

                    obj.MCAmount = CommonFunctions.ParseDecimal(dt2.Rows[i]["MaterialCost"].ToString());
                    obj.Receivedmount = CommonFunctions.ParseDecimal(dt2.Rows[i]["MCReceivedAmt"].ToString());
                    obj.PaidAmount = CommonFunctions.ParseDecimal(dt2.Rows[i]["PaidAmount"].ToString());
                    obj.VoucherNo = dt2.Rows[i]["VoucherNo"].ToString();
                    obj.Printed = false;
                    objList.Add(obj);
                }
            }
            mcpayment.Details = objList;
            mcpayment.ConsignorDetails = objList1;
            return mcpayment;

        }
        public static MCPaymentVoucherVM GetMCPaymentPrintVoucherPrintList(int MCPaymentVoucherID)
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
            comd.CommandText = "SP_MaterialVoucherPrintList";
            comd.Parameters.AddWithValue("@MCPaymentVoucherID", MCPaymentVoucherID);
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "MaterialCostLedger");
            DataTable dt1 = ds.Tables[0];
            DataTable dt2 = ds.Tables[1];

            MCPaymentVoucherVM mcpayment = new MCPaymentVoucherVM();
            

            List<MCPaymentAWB> objList = new List<MCPaymentAWB>();
            MCPaymentAWB obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < dt1.Rows.Count; i++)
                {

                    mcpayment.MCPaymentVoucherID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["MCPaymentVoucherID"].ToString());
                    mcpayment.VoucherNo = ds.Tables[0].Rows[i]["VoucherNo"].ToString();
                    mcpayment.PrintDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["PrintDate"].ToString());
                    mcpayment.DocumentNo = Convert.ToInt32(ds.Tables[0].Rows[i]["DocumentNo"].ToString());
                    mcpayment.TotalAWB = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["TotalAWB"].ToString());
                    mcpayment.TotalAmount =CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["TotalAmount"].ToString());
                    mcpayment.Consignor = ds.Tables[0].Rows[i]["Consignor"].ToString();
                }

                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    obj = new MCPaymentAWB();
                    obj.MCPaymentVoucherID = CommonFunctions.ParseInt(dt2.Rows[i]["MCPaymentVoucherID"].ToString());
                    obj.MCPaymentVoucherDetailID = CommonFunctions.ParseInt(dt2.Rows[i]["MCPaymentVoucherDetailID"].ToString());
                    obj.AWBDate = Convert.ToDateTime(dt2.Rows[i]["AWBDate"].ToString());
                    obj.DRRDetailID = CommonFunctions.ParseInt(dt2.Rows[i]["DRRDetailID"].ToString());
                    obj.InScanID = CommonFunctions.ParseInt(dt2.Rows[i]["InScanID"].ToString());
                    obj.AWBNo = dt2.Rows[i]["AWBNo"].ToString();                                        
                    obj.Consignee = dt2.Rows[i]["Consignee"].ToString();
                    obj.ConsigneeCountry = dt2.Rows[i]["ConsigneeCountryName"].ToString();
                    obj.MCAmount = CommonFunctions.ParseDecimal(dt2.Rows[i]["MaterialCost"].ToString());
                    obj.Receivedmount = CommonFunctions.ParseDecimal(dt2.Rows[i]["MCReceivedAmt"].ToString());
                    obj.PaidAmount = CommonFunctions.ParseDecimal(dt2.Rows[i]["PaidAmount"].ToString());
                                        
                    objList.Add(obj);
                }
            }

          
            mcpayment.Details = objList;
       
            return mcpayment;

        }
        public static DataTable GetMCPaymentVoucherExcelReport()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            MCPaymentPrintSearch reportparam = (MCPaymentPrintSearch)(HttpContext.Current.Session["MCPaymentPrintSearch"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_MaterialVoucherList";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            if (reportparam.AWBNo == null)
                comd.Parameters.AddWithValue("@VoucherNo", "");
            else
                comd.Parameters.AddWithValue("@VoucherNo", reportparam.AWBNo);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "MaterialCostLedger");
            return ds.Tables[0];



        }
        public static List<MCPaymentAWB> GetMCPaymentVoucherList()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            MCPaymentPrintSearch reportparam = (MCPaymentPrintSearch)(HttpContext.Current.Session["MCPaymentPrintSearch"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_MaterialVoucherList";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));                     
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            if (reportparam.AWBNo==null)
                comd.Parameters.AddWithValue("@VoucherNo", "");
            else
                comd.Parameters.AddWithValue("@VoucherNo", reportparam.AWBNo);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "MaterialCostLedger");

            List<MCPaymentAWB> objList = new List<MCPaymentAWB>();
            MCPaymentAWB obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new MCPaymentAWB();                   
                    obj.MCPaymentVoucherID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["MCPaymentVoucherID"].ToString());
                    obj.Consignor = ds.Tables[0].Rows[i]["Consignor"].ToString();
                    obj.VoucherNo = ds.Tables[0].Rows[i]["VoucherNo"].ToString();                    
                    obj.PrintDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["PrintDate"].ToString());
                    obj.TotalAWB = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["TotalAWB"].ToString());
                    obj.TotalAmount= CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["TotalAmount"].ToString());
                    obj.EmployeeName= ds.Tables[0].Rows[i]["EmployeeName"].ToString();
                    if (ds.Tables[0].Rows[i]["ClosedDate"]!=System.DBNull.Value)
                    {
                        obj.ClosedDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["ClosedDate"].ToString());
                    }
                    
                    obj.Closed= Convert.ToBoolean(ds.Tables[0].Rows[i]["Closed"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;

        }

        public static List<MCPaymentAWB> GetMCPaymentVoucherClosingList()
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();

            MCPaymentPrintSearch reportparam = (MCPaymentPrintSearch)(HttpContext.Current.Session["MCPaymentPrintSearch"]);
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection(strConnString);
            SqlCommand comd;
            comd = new SqlCommand();
            comd.Connection = sqlConn;
            comd.CommandType = CommandType.StoredProcedure;
            comd.CommandText = "SP_MaterialVoucherClosingPending";
            comd.Parameters.AddWithValue("@FromDate", reportparam.FromDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
            comd.Parameters.AddWithValue("@FYearId", yearid);
            comd.Parameters.AddWithValue("@BranchId", branchid);
            if (reportparam.AWBNo == null)
                comd.Parameters.AddWithValue("@VoucherNo", "");
            else
                comd.Parameters.AddWithValue("@VoucherNo", reportparam.AWBNo);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = comd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "MaterialCostLedger");

            List<MCPaymentAWB> objList = new List<MCPaymentAWB>();
            MCPaymentAWB obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new MCPaymentAWB();
                    obj.MCPaymentVoucherID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["MCPaymentVoucherID"].ToString());
                    obj.Consignor = ds.Tables[0].Rows[i]["Consignor"].ToString();
                    obj.VoucherNo = ds.Tables[0].Rows[i]["VoucherNo"].ToString();
                    obj.PrintDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["PrintDate"].ToString());
                    obj.TotalAWB = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["TotalAWB"].ToString());
                    obj.TotalAmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["TotalAmount"].ToString());

                    objList.Add(obj);
                }
            }
            return objList;

        }
        public static string SaveMCPaymentVoucher(int UserID,int BranchID,int FYearID, string ConsignorDetails,string Details,string PrintDate)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_MaterialVoucherPrintGenerate";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PrintDate", Convert.ToDateTime(PrintDate).ToString("MM/dd/yyyy HH:mm"));
                cmd.Parameters.AddWithValue("@ConsignorDetails", ConsignorDetails); //formated xml param
                cmd.Parameters.AddWithValue("@Details", Details);//formated xml param
                cmd.Parameters.AddWithValue("@FYearId", FYearID);
                cmd.Parameters.AddWithValue("@BranchId", BranchID);                                
                cmd.Parameters.AddWithValue("@CreatedBy", UserID);
                cmd.Parameters.AddWithValue("@CreatedDate", CommonFunctions.GetCurrentDateTime().ToString("MM/dd/yyyy HH:mm"));

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                int DocumentNo=0;
                int VoucherCount = 0;
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DocumentNo= CommonFunctions.ParseInt(ds.Tables[0].Rows[0]["DocumentNo"].ToString());
                    VoucherCount = CommonFunctions.ParseInt(ds.Tables[0].Rows[0]["VoucherCount"].ToString());
                    if (VoucherCount > 0)
                        return DocumentNo.ToString();
                    else
                        return "0";
                }
                else
                {
                    return "0";
                }
            }
            catch (Exception ex)
            {
                return "Error:" + ex.Message;
            }
            
        }
        #endregion

        public static QuickAWBVM  CheckAWB(string AWBNo)
        {
            int InScanId = 0;
            QuickAWBVM vm = new QuickAWBVM();
            vm.InScanID = 0;
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "Select InScanId,AWBProcessed from InscanMaster where Isnull(Isdeleted,0)=0 and AWBNo='" + AWBNo.Trim() + "'";
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = con;
                        SqlDataAdapter sqlAdapter = new SqlDataAdapter();
                        sqlAdapter.SelectCommand = cmd;
                        DataSet ds = new DataSet();
                        sqlAdapter.Fill(ds, "AWB");
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                vm.InScanID = Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
                                vm.AWBProcessed = Convert.ToBoolean(ds.Tables[0].Rows[0][1].ToString());
                                //InScanId = Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return vm;
            }
            return vm;

        }


        public static int GetFinancialYearID(string TransDate,int BranchID)
        {
            int AcFinancialYearID = 0;
            
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_GetTransDateFinancialYearID";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@TransDate", Convert.ToDateTime(TransDate).ToString("MM/dd/yyyy"));
                        cmd.Parameters.AddWithValue("@BranchId", BranchID);
                        cmd.Connection = con;
                        SqlDataAdapter sqlAdapter = new SqlDataAdapter();
                        sqlAdapter.SelectCommand = cmd;
                        DataSet ds = new DataSet();
                        sqlAdapter.Fill(ds, "AWB");
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                AcFinancialYearID = Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
                              
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
            return AcFinancialYearID;

        }

        public static List<QuickAWBVM> GetAWBListReport(int StatusId, DateTime FromDate, DateTime ToDate, int BranchId, int DepotId, string AWBNo, string MovementId, int PaymentModeId, string ConsignorText, string Origin, string Destination)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "sp_AWBRegisterReport";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@BranchId", BranchId);
            cmd.Parameters.AddWithValue("@FYearId", BranchId);
            cmd.Parameters.AddWithValue("@PaymentModeId", PaymentModeId);
            cmd.Parameters.AddWithValue("@ParcelTypeId", 0);
            if (MovementId == null)
                MovementId = "1,2,3,4";
            cmd.Parameters.AddWithValue("@MovementId", MovementId);
            cmd.Parameters.AddWithValue("@SortBy", "Date Wise");

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
                    obj.shippername = ds.Tables[0].Rows[i]["consignor"].ToString();
                    obj.consigneename = ds.Tables[0].Rows[i]["Consignee"].ToString();
                    obj.origin = ds.Tables[0].Rows[i]["ConsignorCountryName"].ToString();
                    obj.destination = ds.Tables[0].Rows[i]["ConsigneeCountryName"].ToString();
                    obj.ParcelType = ds.Tables[0].Rows[i]["ParcelType"].ToString();
                    obj.MovementType = ds.Tables[0].Rows[i]["MovementType"].ToString();
                    obj.InScanDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["AWBDate"].ToString());
                    obj.CourierCharge = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["CourierCharge"].ToString());
                    obj.OtherCharge = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["OtherCharge"].ToString());
                    //obj.totalCharge = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["totalCharge"].ToString());
                    obj.Weight = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Weight"].ToString());
                    obj.TaxAmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["TaxAmount"].ToString());
                    obj.materialcost = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["MaterialCost"].ToString());
                    obj.Pieces = ds.Tables[0].Rows[i]["Pieces"].ToString();
                    obj.paymentmode = ds.Tables[0].Rows[i]["PaymentModeText"].ToString();
                    obj.ConsigneePhone = ds.Tables[0].Rows[i]["ConsigneePhone"].ToString();
                    obj.FWDAgentNumber = ds.Tables[0].Rows[i]["FWDAgentNumber"].ToString();
                    obj.FAgentName = ds.Tables[0].Rows[i]["FAgentName"].ToString();
                    obj.CourierStatus = ds.Tables[0].Rows[i]["CollectedBy"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static DataTable GetAWBListReportExcel(int StatusId, DateTime FromDate, DateTime ToDate, int BranchId, int DepotId, string AWBNo, string MovementId, int PaymentModeId, string ConsignorText, string Origin, string Destination)
        {
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_AWBRegisterReportExcel";
            cmd.CommandTimeout = 0;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@BranchId", BranchId);
            cmd.Parameters.AddWithValue("@FYearId", yearid);
            cmd.Parameters.AddWithValue("@PaymentModeId", PaymentModeId);
            cmd.Parameters.AddWithValue("@ParcelTypeId", 0);
            if (MovementId == null)
                MovementId = "1,2,3,4";
            cmd.Parameters.AddWithValue("@MovementId", MovementId);
            cmd.Parameters.AddWithValue("@SortBy", "Date Wise");

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            return ds.Tables[0];

            
        }

        public static DataTable GetAWBStatusListReportExcel(int StatusId, DateTime FromDate, DateTime ToDate, int BranchId,string MovementId, string ConsignorText)
        {
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetAWBStatusList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@BranchId", BranchId);
            cmd.Parameters.AddWithValue("@FYearId", yearid);            
            if (MovementId == null)
                MovementId = "1,2,3,4";
            cmd.Parameters.AddWithValue("@MovementId", MovementId);
            cmd.Parameters.AddWithValue("@CourierStatusID",StatusId);

            if (ConsignorText == null)
                ConsignorText = "";
            cmd.Parameters.AddWithValue("@Consignor", ConsignorText);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            return ds.Tables[0];


        }

        public static DataTable GetCustomerListReportExcel(string CustomerType)
        {
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetCustomerListExcel";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CustomerType", CustomerType);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            return ds.Tables[0];


        }


        public static DataTable GetImportAWBListReportExcel(DateTime FromDate, DateTime ToDate, int BranchId)
        {
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_ImportRegisterExcel";
            cmd.CommandTimeout = 0;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
           
            cmd.Parameters.AddWithValue("@FYearId", yearid);
            cmd.Parameters.AddWithValue("@BranchId", BranchId);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            return ds.Tables[0];


        }


        public static List<QuickAWBVM> GetAWBConflicts(int FyearId, int BranchId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetAWBConflicts";
            cmd.CommandType = CommandType.StoredProcedure;           
            cmd.Parameters.AddWithValue("@FYearId", FyearId);
            cmd.Parameters.AddWithValue("@BranchID", BranchId);
            cmd.Parameters.AddWithValue("@Process", 0);
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
                    obj.TransactionDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["AWBDate"].ToString());
                    obj.paymentmode = ds.Tables[0].Rows[i]["PaymentModeCode"].ToString();
                    obj.DRRNo= ds.Tables[0].Rows[i]["DRRNo"].ToString();
                    obj.CourierStatus= ds.Tables[0].Rows[i]["CourierStatus"].ToString();
                    obj.totalCharge = Convert.ToDecimal(ds.Tables[0].Rows[i]["TotalCharge"].ToString());
                    obj.remarks = ds.Tables[0].Rows[i]["Condition"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static StatusModel RefreshAWBConflicts(int FyearId, int BranchId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetAWBConflicts";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FYearId", FyearId);
            cmd.Parameters.AddWithValue("@BranchID", BranchId);
            cmd.Parameters.AddWithValue("@Process", 1);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            
            StatusModel obj = new StatusModel();
            if (ds != null && ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count>0)
                {
                    obj = new StatusModel();
                    obj.Status = ds.Tables[0].Rows[0][0].ToString();                     
                }
            }
            return obj;
        }
    }


}