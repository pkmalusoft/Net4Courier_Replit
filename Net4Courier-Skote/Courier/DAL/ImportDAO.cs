using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Net4Courier.Models;
using System.Configuration;
namespace Net4Courier.DAL
{
    public class ImportDAO
    {

        

        public static List<ImportManifestVM> GetImportManifestList(int ShipmentTypeId)
        {
            ImportManifestSearch paramobj = (ImportManifestSearch)(HttpContext.Current.Session["ImportManifestSearch"]);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_ImportManifestList";
            cmd.CommandType = CommandType.StoredProcedure;
            if (paramobj.AWBNo==null)
            {
                paramobj.AWBNo = "";
            }
            cmd.Parameters.AddWithValue("@AWBNO", paramobj.AWBNo);

            if (paramobj.FromDate != null)
                cmd.Parameters.AddWithValue("@FromDate", Convert.ToDateTime(paramobj.FromDate).ToString("MM/dd/yyyy"));
            else
                cmd.Parameters.AddWithValue("@FromDate", "");

            if (paramobj.ToDate != null)
                cmd.Parameters.AddWithValue("@ToDate", Convert.ToDateTime(paramobj.ToDate).ToString("MM/dd/yyyy"));
            else
                cmd.Parameters.AddWithValue("@ToDate", "");


            cmd.Parameters.AddWithValue("@ShipmentTypeId", ShipmentTypeId);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<ImportManifestVM> objList = new List<ImportManifestVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    ImportManifestVM     obj = new ImportManifestVM();
                    obj.ID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ID"].ToString());
                    obj.ManifestNumber= ds.Tables[0].Rows[i]["ManifestNumber"].ToString();
                    obj.CreatedDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["CreatedDate"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());                    
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static List<ImportManifestVM> GetTranshipmentManifestList(int ShipmentTypeId)
        {
            ImportManifestSearch paramobj = (ImportManifestSearch)(HttpContext.Current.Session["TranshipmentSearch"]);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_TranshipmentManifestList";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@AWBNO", "");
            if (paramobj.FromDate != null)
                cmd.Parameters.AddWithValue("@FromDate", Convert.ToDateTime(paramobj.FromDate).ToString("MM/dd/yyyy"));

            if (paramobj.ToDate != null)
                cmd.Parameters.AddWithValue("@ToDate", Convert.ToDateTime(paramobj.ToDate).ToString("MM/dd/yyyy"));

            cmd.Parameters.AddWithValue("@ShipmentTypeId", ShipmentTypeId);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<ImportManifestVM> objList = new List<ImportManifestVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    ImportManifestVM obj = new ImportManifestVM();
                    obj.ID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ID"].ToString());
                    obj.ManifestNumber = ds.Tables[0].Rows[i]["ManifestNumber"].ToString();
                    obj.MAWB= ds.Tables[0].Rows[i]["MAWB"].ToString();
                    obj.CustomerName = ds.Tables[0].Rows[i]["CustomerName"].ToString();
                    obj.CreatedDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["CreatedDate"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());                    
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static string GetMaxManifestNo(int Companyid, int BranchId,DateTime ManifestDate,string ShipmentType)
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
                        cmd.CommandText = "GetMaxManifestNo";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;

                        cmd.Parameters.AddWithValue("@CompanyId", Companyid);
                        cmd.Parameters.AddWithValue("@BranchId", BranchId);
                        cmd.Parameters.AddWithValue("@ManifestDate",ManifestDate.ToString("yyyyMMdd"));
                        cmd.Parameters.AddWithValue("@ShipmentType", ShipmentType);

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

      


        public static List<TranshipmentModel> GetTranshipmenItems(int ImportShipmentTypeId,string CountryName)
        {
            var awbList = new List<TranshipmentModel>();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetTranshipmentItem";
            cmd.CommandType = CommandType.StoredProcedure;            
            cmd.Parameters.AddWithValue("@ImportShipmentId", ImportShipmentTypeId);
            cmd.Parameters.AddWithValue("@CountryName", CountryName);
            
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);                      

            if (ds != null && ds.Tables.Count > 0)
            {
                int i = 1;
                foreach (DataRow objDataRow in ds.Tables[0].Rows)
                {                    
                    awbList.Add(new TranshipmentModel()
                    {
                        SNo = i++,
                        InScanID = Convert.ToInt32(objDataRow["InScanID"].ToString()),
                        HAWBNo = objDataRow["HAWBNo"].ToString(),
                        AWBDate = objDataRow["AWBDate"].ToString(),
                        Customer = objDataRow["Customer"].ToString(),
                        ConsignorPhone = objDataRow["ConsignorPhone"].ToString(),
                        Consignor = objDataRow["Consignor"].ToString(),
                        ConsignorLocationName = objDataRow["ConsignorLocationName"].ToString(),
                        ConsignorCountryName = objDataRow["ConsignorCountryName"].ToString(),
                        ConsignorCityName = objDataRow["ConsignorCityName"].ToString(),
                        Consignee = objDataRow["Consignee"].ToString(),
                        ConsigneeCountryName = objDataRow["ConsigneeCountryName"].ToString(),
                        ConsigneeCityName = objDataRow["ConsigneeCityName"].ToString(),
                        ConsigneeLocationName = objDataRow["ConsigneeLocationName"].ToString(),
                        ConsignorAddress1_Building = objDataRow["ConsignorAddress1_Building"].ToString(),
                        ConsignorMobile = objDataRow["ConsignorMobile"].ToString(),
                        ConsigneeMobile = objDataRow["ConsigneeMobile"].ToString(),
                        Weight = CommonFunctions.ParseDecimal(objDataRow["Weight"].ToString()),
                        Pieces = objDataRow["Pieces"].ToString(),
                        CourierCharge = CommonFunctions.ParseDecimal(objDataRow["CourierCharge"].ToString()),
                        OtherCharge = CommonFunctions.ParseDecimal(objDataRow["OtherCharge"].ToString()),
                        PaymentMode = objDataRow["PaymentMode"].ToString(),
                        ReceivedBy = objDataRow["ReceivedBy"].ToString(),
                        CollectedBy = objDataRow["CollectedBy"].ToString(),
                        FAWBNo = objDataRow["FAWBNo"].ToString(),
                        FAgentName = objDataRow["FAgentName"].ToString(),
                        ForwardingCharge = CommonFunctions.ParseDecimal(objDataRow["ForwardingCharge"].ToString()),
                        CourierType = objDataRow["CourierType"].ToString(),
                        ParcelType= objDataRow["ParcelType"].ToString(),
                        ParcelTypeID =Convert.ToInt32(objDataRow["ParcelTypeId"].ToString()),
                        ProductTypeID = Convert.ToInt32(objDataRow["ProductTypeID"].ToString()),
                        PickedUpEmpID = Convert.ToInt32(objDataRow["PickedUpEmpID"].ToString()),
                        DepotReceivedBy = Convert.ToInt32(objDataRow["DepotReceivedBy"].ToString()),
                        CourierStatusID = Convert.ToInt32(objDataRow["CourierStatusID"].ToString()),
                        StatusTypeId = Convert.ToInt32(objDataRow["StatusTypeId"].ToString()),
                        FagentID= Convert.ToInt32(objDataRow["FAgentID"].ToString()),
                        MovementType = objDataRow["MovementType"].ToString(),
                        CourierStatus = objDataRow["CourierStatus"].ToString(),
                        remarks = objDataRow["remarks"].ToString(), //Department and Bag no is missing                                                               
                        DataError = CommonFunctions.ParseDecimal(objDataRow["CourierCharge"].ToString())==0 ? true :false,
                        AWBChecked = CommonFunctions.ParseDecimal(objDataRow["CourierCharge"].ToString()) == 0 ? true : false
                    });
                    //AWBNo AWBDate Bag NO.	Shipper ReceiverName    ReceiverContactName ReceiverPhone   ReceiverAddress DestinationLocation DestinationCountry Pcs Weight CustomsValue    COD Content Reference Status  SynchronisedDateTime

                }
            }
            return awbList;
        }

        public static List<TranshipmentCountry> GetTranshipmenCountryList(int ImportShipmentTypeId)
        {
            var awbList = new List<TranshipmentCountry>();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_TranshipmentCountryList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ShipmentId", ImportShipmentTypeId);
            
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            if (ds != null && ds.Tables.Count > 0)
            {
                int i = 1;
                foreach (DataRow objDataRow in ds.Tables[0].Rows)
                {
                    awbList.Add(new TranshipmentCountry()
                    {
                        SNo = i++,
                        CountryName  =objDataRow["ConsigneeCountryName"].ToString(),                        
                        TotalAWB = Convert.ToInt32(objDataRow["TotalAwbNo"].ToString()),
                        ValidAWB = Convert.ToInt32(objDataRow["ValidAwb"].ToString()),
                        ErrAWB = Convert.ToInt32(objDataRow["ErrAwb"].ToString())
                    });
                    

                }
            }
            return awbList;
        }

        public static string EditManualDataFixation(int ShipmentId, int AgentID,string FieldName,string SourceValue,string TargetValue)
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
                        cmd.CommandText = "SP_TranshipmentEditManualFixation";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;

                        cmd.Parameters.AddWithValue("@ShipmentId", ShipmentId);
                        cmd.Parameters.AddWithValue("@AgentID", AgentID);
                        cmd.Parameters.AddWithValue("@FieldName", FieldName);
                        cmd.Parameters.AddWithValue("@SourceValue", SourceValue);
                        cmd.Parameters.AddWithValue("@TargetValue", TargetValue);                                               

                        con.Open();
                        cmd.ExecuteNonQuery();                        
                        con.Close();
                        return "ok";
                    }
                }
            }
            catch (Exception e)
            {
                return "Failed";
            }
            return "ok";

        }

        public static string EditManualRateFixation(int ShipmentId, string CountryName)
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
                        cmd.CommandText = "SP_GetTranshipmentAWBCourierCharge";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;

                        cmd.Parameters.AddWithValue("@ShipmentId", ShipmentId);
                        cmd.Parameters.AddWithValue("@ConsigneeCountryName",CountryName);
                      
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                        return "ok";
                    }
                }
            }
            catch (Exception e)
            {
                return "Failed";
            }
            return "ok";

        }

        public static string EditManualCostFixation(int ShipmentId, string CountryName)
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
                        cmd.CommandText = "SP_GetTranshipmentAWBFwdAgentRate";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;

                        cmd.Parameters.AddWithValue("@ShipmentId", ShipmentId);
                        cmd.Parameters.AddWithValue("@ConsigneeCountryName", CountryName);

                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                        return "ok";
                    }
                }
            }
            catch (Exception e)
            {
                return "Failed";
            }
            return "ok";

        }

        public static List<TranshipmentModel> GetBulkTranshipmenItems(string MAWB,int AgentID)
        {
            var awbList = new List<TranshipmentModel>();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "Select * From TranshipmentAWB Where MAWB='" + MAWB + "'";
            cmd.CommandType = CommandType.Text;
            //cmd.Parameters.AddWithValue("@ImportShipmentId", ImportShipmentTypeId);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            if (ds != null && ds.Tables.Count > 0)
            {
                int i = 1;
                foreach (DataRow objDataRow in ds.Tables[0].Rows)
                {
                    awbList.Add(new TranshipmentModel()
                    {
                        SNo = i++,
                        CustomerId = AgentID,
                        HAWBNo = objDataRow["HAWBNo"].ToString(),
                        AWBDate = objDataRow["AWBDate"].ToString(),
                        Customer = objDataRow["Customer"].ToString(),
                        ConsignorPhone = objDataRow["TelephoneNo"].ToString(),
                        Consignor = objDataRow["Consignor"].ToString(),
                        ConsignorLocationName = objDataRow["ConsignorLocation"].ToString(),
                        ConsignorCountryName = objDataRow["ConsignorCountry"].ToString(),
                        ConsignorCityName = objDataRow["ConsignorCity"].ToString(),
                        Consignee = objDataRow["Consignee"].ToString(),
                        ConsigneeCountryName = objDataRow["ConsigneeCountry"].ToString(),
                        ConsigneeCityName = objDataRow["ConsigneeCity"].ToString(),
                        ConsigneeLocationName = objDataRow["ConsigneeLocation"].ToString(),
                        ConsignorAddress1_Building = objDataRow["ConsignorAddress"].ToString(),
                        ConsigneeAddress1_Building = objDataRow["ConsigneeAddress"].ToString(),
                        ConsignorMobile = objDataRow["ConsignorTelephone"].ToString(),
                        ConsigneeMobile = objDataRow["ConsigneeTelephone"].ToString(),
                        Weight = CommonFunctions.ParseDecimal(objDataRow["Weight"].ToString()),
                        Pieces = objDataRow["Pieces"].ToString(),
                        CourierCharge = CommonFunctions.ParseDecimal(objDataRow["CourierCharge"].ToString()),
                        OtherCharge = CommonFunctions.ParseDecimal(objDataRow["OtherCharge"].ToString()),
                        PaymentMode = objDataRow["PaymentMode"].ToString(),
                        ReceivedBy = objDataRow["ReceiverName"].ToString(),
                        CollectedBy = objDataRow["CollectedName"].ToString(),
                        FAWBNo = objDataRow["FwdNo"].ToString(),
                        FAgentName = objDataRow["ForwardingAgent"].ToString(),
                        CourierType = objDataRow["Couriertype"].ToString(),
                        ParcelType = objDataRow["ParcelType"].ToString(),
                        MovementType = objDataRow["MovementType"].ToString(),
                        CourierStatus = objDataRow["CourierStatus"].ToString(),
                        remarks = objDataRow["Remarks"].ToString(), //Department and Bag no is missing                                                               
                        DataError = false ,//  CommonFunctions.ParseDecimal(objDataRow["CourierCharge"].ToString()) == 0 ? true : false,
                        AWBChecked = true // CommonFunctions.ParseDecimal(objDataRow["CourierCharge"].ToString()) == 0 ? true : false
                    });
                    //AWBNo AWBDate Bag NO.	Shipper ReceiverName    ReceiverContactName ReceiverPhone   ReceiverAddress DestinationLocation DestinationCountry Pcs Weight CustomsValue    COD Content Reference Status  SynchronisedDateTime

                }
            }
            return awbList;
        }
    }
}