using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Net4Courier.Models;
using System.Configuration;
using System.Xml;
using System.IO;
using System.Web.Hosting;

namespace Net4Courier.DAL
{
    public class ExportDAO
    {

        public static List<ImportShipmentVM> GetImportShipmentManifestList(int ShipmentTypeId)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            ImportShipmentSearch paramobj = (ImportShipmentSearch)(HttpContext.Current.Session["ImportShipmentSearch"]);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_ImportShipmentManifestList";
            cmd.CommandType = CommandType.StoredProcedure;
            //if (paramobj.AWBNo == null)
            //{
            //    paramobj.AWBNo = "";
            //}
            //cmd.Parameters.AddWithValue("@AWBNO", paramobj.AWBNo);

            if (paramobj.FromDate != null)
                cmd.Parameters.AddWithValue("@FromDate", Convert.ToDateTime(paramobj.FromDate).ToString("MM/dd/yyyy"));
            else
                cmd.Parameters.AddWithValue("@FromDate", "");

            if (paramobj.ToDate != null)
                cmd.Parameters.AddWithValue("@ToDate", Convert.ToDateTime(paramobj.ToDate).ToString("MM/dd/yyyy"));
            else
                cmd.Parameters.AddWithValue("@ToDate", "");

            cmd.Parameters.AddWithValue("@FYearId", yearid);
            cmd.Parameters.AddWithValue("@BranchID", branchid);

            //cmd.Parameters.AddWithValue("@ShipmentTypeId", ShipmentTypeId);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<ImportShipmentVM> objList = new List<ImportShipmentVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    ImportShipmentVM obj = new ImportShipmentVM();
                    obj.ID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ID"].ToString());
                    obj.ManifestNumber = ds.Tables[0].Rows[i]["ManifestNumber"].ToString();
                    obj.CreatedDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["CreatedDate"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());                    
                    //obj.ShipmentTypeId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ShipmentTypeId"].ToString());
                    //obj.ShipmentType = ds.Tables[0].Rows[i]["ShipmentType"].ToString();
                    obj.AWBNumbers = ds.Tables[0].Rows[i]["AWBNumbers"].ToString();
                    obj.AgentName = ds.Tables[0].Rows[i]["AgentName"].ToString();
                    obj.MAWB = ds.Tables[0].Rows[i]["MAWB"].ToString();
                    //if (obj.ShipmentType == "Domestic")
                    //{
                    //    obj.AgentName = ds.Tables[0].Rows[i]["FAgentName"].ToString();
                    //}
                    //else
                    //{
                    //    obj.AgentName = ds.Tables[0].Rows[i]["AgentName"].ToString();
                    //}

                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static List<ExportShipmentFormModel> GetExportManifestList(int ShipmentTypeId)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            ExportShipmentSearch paramobj = (ExportShipmentSearch)(HttpContext.Current.Session["ExportShipmentSearch"]);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_ExportShipmentList";
            cmd.CommandType = CommandType.StoredProcedure;
            //if (paramobj.AWBNo == null)
            //{
            //    paramobj.AWBNo = "";
            //}
            //cmd.Parameters.AddWithValue("@AWBNO", paramobj.AWBNo);

            if (paramobj.FromDate != null)
                cmd.Parameters.AddWithValue("@FromDate", Convert.ToDateTime(paramobj.FromDate).ToString("MM/dd/yyyy"));
            else
                cmd.Parameters.AddWithValue("@FromDate", "");

            if (paramobj.ToDate != null)
                cmd.Parameters.AddWithValue("@ToDate", Convert.ToDateTime(paramobj.ToDate).ToString("MM/dd/yyyy"));
            else
                cmd.Parameters.AddWithValue("@ToDate", "");

            cmd.Parameters.AddWithValue("@FYearId",yearid);
           cmd.Parameters.AddWithValue("@BranchID", branchid);
            cmd.Parameters.AddWithValue("@AWBNo", paramobj.AWBNo);
            //cmd.Parameters.AddWithValue("@ShipmentTypeId", ShipmentTypeId);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<ExportShipmentFormModel> objList = new List<ExportShipmentFormModel>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    ExportShipmentFormModel obj = new ExportShipmentFormModel();
                    obj.ID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ID"].ToString());
                    obj.ManifestNumber = ds.Tables[0].Rows[i]["ManifestNumber"].ToString();
                    obj.MAWB = ds.Tables[0].Rows[i]["MAWB"].ToString();
                    obj.ManifestDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["ManifestDate"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());                    
                    obj.ShipmentTypeId= CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ShipmentTypeId"].ToString());
                    obj.ShipmentType = ds.Tables[0].Rows[i]["ShipmentType"].ToString();
                    obj.AWBNumbers= ds.Tables[0].Rows[i]["AWBNumbers"].ToString();
                    obj.FAgentName = ds.Tables[0].Rows[i]["FAgentName"].ToString();
                    obj.AgentName = ds.Tables[0].Rows[i]["AgentName"].ToString();
                    obj.MAWBWeight = Convert.ToDecimal(ds.Tables[0].Rows[i]["MAWBWeight"].ToString());
                    obj.CD = ds.Tables[0].Rows[i]["CD"].ToString();
                    obj.Bags =Convert.ToInt32(ds.Tables[0].Rows[i]["Bags"].ToString());
                    obj.RunNo =ds.Tables[0].Rows[i]["RunNo"].ToString();
                    //if (obj.ShipmentType == "Domestic")
                    //{

                    //}
                    //else
                    //{

                    //}

                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static List<ImportShipmentDetailVM> GetExportedManifestShipmentList(int ExportId)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetExportManifestShipmentList";
            cmd.CommandType = CommandType.StoredProcedure;            
            cmd.Parameters.AddWithValue("@ExportId", ExportId);

            //cmd.Parameters.AddWithValue("@ShipmentTypeId", ShipmentTypeId);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<ImportShipmentDetailVM> objList = new List<ImportShipmentDetailVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    ImportShipmentDetailVM obj = new ImportShipmentDetailVM();
                    obj.ShipmentDetailID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ShipmentDetailID"].ToString());
                    obj.ExportShipmentID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ExportID"].ToString());
                    obj.AWB = ds.Tables[0].Rows[i]["AWB"].ToString();
                    obj.AWBDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["AWBDate"].ToString());
                    obj.PCS =CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["PCS"].ToString());
                    obj.Weight = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Weight"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());                    
                    obj.Contents =ds.Tables[0].Rows[i]["Contents"].ToString();
                    obj.Shipper = ds.Tables[0].Rows[i]["Shipper"].ToString();
                    obj.COD = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Value"].ToString());
                    obj.Receiver = ds.Tables[0].Rows[i]["Reciver"].ToString();
                    obj.DestinationCountry = ds.Tables[0].Rows[i]["DestinationCountry"].ToString();
                    obj.DestinationCity = ds.Tables[0].Rows[i]["DestinationCity"].ToString();
                    obj.BagNo = ds.Tables[0].Rows[i]["BagNo"].ToString();
                    obj.ReceiverTelephone= ds.Tables[0].Rows[i]["ConsigneePhone"].ToString();
                    obj.ReceiverAddress = ds.Tables[0].Rows[i]["ReceiverAddress"].ToString();
                    obj.ReceiverContact = ds.Tables[0].Rows[i]["ConsigneeContact"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static string GetMaxExportManifestNo(int Companyid, int BranchId, DateTime ManifestDate, int ShipmentTypeId)
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
                        cmd.CommandText = "GetMaxExportManifestNo";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;

                        cmd.Parameters.AddWithValue("@CompanyId", Companyid);
                        cmd.Parameters.AddWithValue("@BranchId", BranchId);
                        cmd.Parameters.AddWithValue("@ManifestDate", ManifestDate.ToString("MM/dd/yyyy"));
                        cmd.Parameters.AddWithValue("@ShipmentTypeId", ShipmentTypeId);

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

        public static List<ShipmentInvoiceVM> GetExportShipmentMAWBList(int AgentID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetExportShipmentMAWB";
            cmd.CommandType = CommandType.StoredProcedure;            
            cmd.Parameters.AddWithValue("@AgentId", AgentID);

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
                    obj.ShipmentImportID = Convert.ToInt32(ds.Tables[0].Rows[i]["ExportID"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static string GetExportShipmentXML(int id,string reportname)
        {
            DataTable dt = new DataTable();
            XmlReader reader;
            XmlDocument xmlDoc;
            
            string reportpath = Path.Combine(HostingEnvironment.MapPath("~/ReportsPDF"), reportname);
            string MaxPickUpNo = "";
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_ExportShipmentXML";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;

                        cmd.Parameters.AddWithValue("@ID", id);
                        //cmd.Parameters.AddWithValue("@BranchId", BranchId);
                        //cmd.Parameters.AddWithValue("@ManifestDate", ManifestDate.ToString("MM/dd/yyyy"));
                        //cmd.Parameters.AddWithValue("@ShipmentTypeId", ShipmentTypeId);

                        con.Open();
                        SqlDataAdapter SqlDA = new SqlDataAdapter(cmd);
                        SqlDA.Fill(dt);
                        reader = cmd.ExecuteXmlReader();
                        xmlDoc = new XmlDocument();
                        while (reader.Read())
                        {
                            xmlDoc.Load(reader);
                        }
                                            
                        xmlDoc.Save(reportpath);
                       
                    }
                    con.Close();
                    return reportpath;
                }                
            }
            catch (Exception e)
            {

            }
            return reportpath;

        }

        public static SaveStatusModel SaveExportShipment(int ExportID, int UserID, string Details)
        {
            SaveStatusModel status = new SaveStatusModel();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_SaveExportShipmentDetail";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ExportID", ExportID);               
                cmd.Parameters.AddWithValue("@UserID", UserID);                
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
        public static SaveStatusModel SaveExportShipmentDeleted(int ExportID, int UserID, string Details)
        {
            SaveStatusModel status = new SaveStatusModel();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_SaveDeletedExportShipmentDetail";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ExportID", ExportID);
                cmd.Parameters.AddWithValue("@UserID", UserID);
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
        public static List<ExportShipmentDetailVM> GetExportShipmentDetail(int ExportId)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetExportShipmentDetail";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ExportID", ExportId);

            //cmd.Parameters.AddWithValue("@ShipmentTypeId", ShipmentTypeId);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<ExportShipmentDetailVM> objList = new List<ExportShipmentDetailVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    ExportShipmentDetailVM obj = new ExportShipmentDetailVM();
                    obj.ShipmentDetailID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ShipmentDetailID"].ToString());
                    obj.ExportID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ExportID"].ToString());
                    obj.InboundShipmentID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["InboundShipmentID"].ToString());
                    obj.InscanId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["InscanId"].ToString());
                    obj.AWBNo = ds.Tables[0].Rows[i]["AWBNo"].ToString();
                    obj.AWBDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["AWBDate"].ToString());
                    obj.PCS = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["PCS"].ToString());
                    obj.Weight = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Weight"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());                    
                    obj.Contents = ds.Tables[0].Rows[i]["Contents"].ToString();
                    obj.Shipper = ds.Tables[0].Rows[i]["Shipper"].ToString();
                    obj.Receiver = ds.Tables[0].Rows[i]["Receiver"].ToString();
                    obj.OriginCountry = ds.Tables[0].Rows[i]["OriginCountry"].ToString();
                    obj.ConsignorAddress1 = ds.Tables[0].Rows[i]["ConsignorAddress1_Building"].ToString();
                    obj.ConsignorAddress2= ds.Tables[0].Rows[i]["ConsignorAddress2_Street"].ToString();
                    obj.ConsigneeAddress1 = ds.Tables[0].Rows[i]["ConsigneeAddress1_Building"].ToString();
                    obj.ConsigneeAddress2 = ds.Tables[0].Rows[i]["ConsigneeAddress2_Street"].ToString();
                    obj.OriginCity = ds.Tables[0].Rows[i]["OriginCity"].ToString();
                    obj.DestinationCountry = ds.Tables[0].Rows[i]["DestinationCountry"].ToString();
                    obj.DestinationCity = ds.Tables[0].Rows[i]["DestinationCity"].ToString();
                    obj.ConsigneePhone = ds.Tables[0].Rows[i]["ConsigneePhone"].ToString();
                    obj.ConsignorPhone = ds.Tables[0].Rows[i]["ConsignorPhone"].ToString();
                    obj.BagNo = ds.Tables[0].Rows[i]["BagNo"].ToString();
                    obj.CurrenySymbol = ds.Tables[0].Rows[i]["CurrencyName"].ToString();
                    obj.CustomsValue = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["CustomsValue"].ToString());
                    obj.FwdAgentName = ds.Tables[0].Rows[i]["FwdAgentName"].ToString();
                    obj.FwdAgentAWBNo = ds.Tables[0].Rows[i]["FwdAgentAWBNo"].ToString();
                    obj.FwdAgentId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["FwdAgentId"].ToString());
                    obj.FwdCharge = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["FwdCharge"].ToString());

                    obj.VerifiedWeight = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["VerifiedWeight"].ToString());
                    obj.AWBChecked = true;
                    objList.Add(obj);
                }
            }
            return objList;
        }
    }
}