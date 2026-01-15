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
    public class InboundShipmentDAO
    {

        public static List<InboundShipmentModel> GetAWBList(int StatusId, DateTime FromDate, DateTime ToDate, int BranchId, int DepotId, string AWBNo, int MovementId, int PaymentModeId, string ConsignorText, string Origin, string Destination,int CustomerId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetInBoundShipmentList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@pStatusId", StatusId);
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@BranchId", BranchId);
            cmd.Parameters.AddWithValue("@DepotId", DepotId);

            cmd.Parameters.AddWithValue("@MovementId", MovementId);
            cmd.Parameters.AddWithValue("@PaymentModeId", PaymentModeId);
            if (ConsignorText == null)
                ConsignorText = "";
            else if (ConsignorText != "")
                ConsignorText = ConsignorText + "%";
            cmd.Parameters.AddWithValue("@ConsignorConsignee", ConsignorText);
            if (Origin == null)
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
            cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<InboundShipmentModel> objList = new List<InboundShipmentModel>();
            InboundShipmentModel obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new InboundShipmentModel();
                    obj.ShipmentID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["InScanID"].ToString());
                    obj.AWBNo = ds.Tables[0].Rows[i]["AWBNo"].ToString();
                    obj.Consignor = ds.Tables[0].Rows[i]["shippername"].ToString();
                    obj.Consignee = ds.Tables[0].Rows[i]["consigneename"].ToString();
                    obj.Destination = ds.Tables[0].Rows[i]["destination"].ToString();
                    obj.AWBDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["InScanDate"].ToString());
                    obj.CourierStatus = ds.Tables[0].Rows[i]["CourierStatus"].ToString();
                    obj.StatusType = ds.Tables[0].Rows[i]["StatusType"].ToString();
                    obj.NetTotal = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["totalCharge"].ToString());
                    obj.PaymentMode = ds.Tables[0].Rows[i]["paymentmode"].ToString();
                    obj.ConsigneePhone = ds.Tables[0].Rows[i]["ConsigneePhone"].ToString();
                    obj.CreatedByName = ds.Tables[0].Rows[i]["CreatedByName"].ToString();
                    obj.LastModifiedByName = ds.Tables[0].Rows[i]["LastModifiedByName"].ToString();
                    obj.CreatedDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["CreatedByDate"].ToString());
                    obj.LastModifiedDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["LastModifiedDate"].ToString());
                    //obj.InvoiceId = Convert.ToInt32(ds.Tables[0].Rows[i]["InvoiceID"].ToString());
                    //obj.COInvoiceId = Convert.ToInt32(ds.Tables[0].Rows[i]["COInvoiceID"].ToString());
                    //obj.ImportShipmentId = Convert.ToInt32(ds.Tables[0].Rows[i]["ImportShipmentId"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }


        public static List<AWBBatchList> GetAWBBatchList(int branchid, int FyearId, AWBBatchSearch paramobj,int CustomerID)
        {
            
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetInboundAWBBatchList";
            cmd.CommandType = CommandType.StoredProcedure;

            if (paramobj.FromDate != null)
                cmd.Parameters.AddWithValue("@FromDate", Convert.ToDateTime(paramobj.FromDate).ToString("MM/dd/yyyy"));

            if (paramobj.ToDate != null)
                cmd.Parameters.AddWithValue("@ToDate", Convert.ToDateTime(paramobj.ToDate).ToString("MM/dd/yyyy"));

            cmd.Parameters.AddWithValue("@BranchID", branchid);
            cmd.Parameters.AddWithValue("@FYearId", FyearId);
            cmd.Parameters.AddWithValue("@AWBNo", paramobj.DocumentNo);
            cmd.Parameters.AddWithValue("@CustomerId", CustomerID);

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
                    obj.CreatedByName = ds.Tables[0].Rows[i]["CreatedByName"].ToString();
                    obj.ModifiedByName = ds.Tables[0].Rows[i]["ModifiedByName"].ToString();
                    obj.CreatedDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["CreatedDate"].ToString());
                    obj.ModifiedDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["ModifiedDate"].ToString());
                    obj.TotalAWB = Convert.ToInt32(ds.Tables[0].Rows[i]["TotalAWB"].ToString());
                    obj.CustomerName= ds.Tables[0].Rows[i]["CustomerName"].ToString();
                    obj.MAWB = ds.Tables[0].Rows[i]["MAWB"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static List<InboundShipmentModel> GetBatchAWBInfo(int BatchID, int InscanID = 0)
        {
            List<InboundShipmentModel> list = new List<InboundShipmentModel>();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GETBatchInboundAWBList";
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
                        InboundShipmentModel obj = new InboundShipmentModel();
                        obj.IsDeleted = false;
                        obj.ShipmentID = CommonFunctions.ParseInt(dt.Rows[i]["ShipmentID"].ToString());
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
                        obj.ConsignorMobileNo= dt.Rows[i]["ConsignorMobileNo"].ToString();
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
                        obj.ParcelTypeID = dt.Rows[i]["ParcelTypeId"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["ParcelTypeId"].ToString());
                        //bj.DocumentTypeId = dt.Rows[i]["DocumentTypeId"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["DocumentTypeId"].ToString());
                        obj.ProductTypeID = dt.Rows[i]["ProductTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["ProductTypeID"].ToString());
                        obj.MovementType = dt.Rows[i]["MovementType"].ToString();
                        obj.ParcelType = dt.Rows[i]["ParcelType"].ToString();
                        obj.ProductType = dt.Rows[i]["ProductName"].ToString();
                        obj.CargoDescription = dt.Rows[i]["CargoDescription"].ToString();
                        obj.Pieces = dt.Rows[i]["Pieces"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["Pieces"].ToString());
                        obj.Weight = dt.Rows[i]["Weight"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["Weight"].ToString());
                        obj.ManifestWeight = dt.Rows[i]["ManifestWeight"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["ManifestWeight"].ToString());
                        obj.TaxPercent = dt.Rows[i]["TaxPercent"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["TaxPercent"].ToString());
                        obj.TaxAmount = dt.Rows[i]["TaxAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["TaxAmount"].ToString());
                        obj.SurchargePercent = dt.Rows[i]["SurchargePercent"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["SurchargePercent"].ToString());
                        obj.SurchargeAmount = dt.Rows[i]["SurchargeAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["SurchargeAmount"].ToString());
                        //obj.SpecialInstructions = dt.Rows[i]["SpecialNotes"] == DBNull.Value ? "" : dt.Rows[i]["SpecialNotes"].ToString();
                        obj.CustomerRateID = dt.Rows[i]["CustomerRateID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["CustomerRateID"].ToString());
                        obj.CurrencyID = dt.Rows[i]["CurrencyID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["CurrencyID"].ToString());
                         obj.Currency = dt.Rows[i]["CurrencyName"].ToString();
                        obj.CustomsValue = dt.Rows[i]["CustomsValue"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["CustomsValue"].ToString());
                        obj.ForwardingCharge = dt.Rows[i]["ForwardingCharge"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["ForwardingCharge"].ToString());
                        obj.FAgentName = dt.Rows[i]["FAgentName"] == DBNull.Value ? "" : dt.Rows[i]["FAgentName"].ToString();
                        //obj.CustomerRateType = GetCustomerRateName(obj.CustomerRateTypeId);
                        obj.BagNo = dt.Rows[i]["BagNo"] == DBNull.Value ? "" : dt.Rows[i]["BagNo"].ToString();
                        obj.MAWB = dt.Rows[i]["MAWB"] == DBNull.Value ? "" : dt.Rows[i]["MAWB"].ToString();
                        obj.AcJournalID = dt.Rows[i]["AcJournalID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["AcJournalID"].ToString());
                        obj.FAgentID = dt.Rows[i]["FAgentID"] == DBNull.Value ? -1 : Convert.ToInt32(dt.Rows[i]["FAgentID"].ToString());
                        obj.ForwardingAWBNo = dt.Rows[i]["ForwardingAWBNo"].ToString();
                        obj.VoucherNo= dt.Rows[i]["VoucherNo"] == DBNull.Value ? "" : dt.Rows[i]["VoucherNo"].ToString();
                        obj.Route = dt.Rows[i]["Route"] == DBNull.Value ? "" : dt.Rows[i]["Route"].ToString();
                        obj.AWBStatus = dt.Rows[i]["AWBStatus"].ToString();
                        list.Add(obj);
                    }

                }

            }


            return list;

        }

        public static string GetMaxBathcNo(DateTime BatchDate, int BranchId, int FYearId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetMaxInboundAWBBatchNo";
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

        //upload csv extracted to inboundshipment model awb
        public static List<InboundShipmentModel> GetShipmentValidAWBDetails(int BranchID, string Details, string AWBNo, int CustomerID)
        {
            List<InboundShipmentModel> list = new List<InboundShipmentModel>();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_GetInboundShipmentValidAWBDetails";
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
                        InboundShipmentModel obj = new InboundShipmentModel();

                        obj.ShipmentID = 0;
                        obj.SNo = i + 1;
                        obj.AWBNo = dt.Rows[i]["AWBNo"].ToString();
                        obj.AWBDate = Convert.ToDateTime(dt.Rows[i]["AWBDate"].ToString());
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
                        obj.ParcelTypeID = Convert.ToInt32(dt.Rows[i]["ParcelTypeID"].ToString());
                        obj.MovementID = Convert.ToInt32(dt.Rows[i]["MovementID"].ToString());
                        obj.ProductTypeID = Convert.ToInt32(dt.Rows[i]["ProductTypeID"].ToString());
                        obj.ParcelType = dt.Rows[i]["ParcelType"].ToString();
                        obj.MovementType = dt.Rows[i]["MovementType"].ToString();
                        obj.ProductType = dt.Rows[i]["ProductType"].ToString();
                        obj.MaterialCost = Convert.ToDecimal(dt.Rows[i]["MaterialCost"].ToString());
                        obj.Pieces = Convert.ToInt32(dt.Rows[i]["Pieces"].ToString());
                        obj.Weight = CommonFunctions.ParseDecimal(dt.Rows[i]["Weight"].ToString());
                        obj.CourierCharge = CommonFunctions.ParseDecimal(dt.Rows[i]["CourierCharge"].ToString());
                        obj.OtherCharge = Convert.ToDecimal(dt.Rows[i]["OtherCharge"].ToString());
                        obj.NetTotal = Convert.ToDecimal(dt.Rows[i]["NetTotal"].ToString());
                        obj.BagNo = dt.Rows[i]["BagNo"].ToString();
                        obj.CargoDescription = dt.Rows[i]["CargoDescription"].ToString();
                        obj.Remarks = dt.Rows[i]["Remarks"].ToString();
                        obj.MAWB = dt.Rows[i]["MAWB"].ToString();
                        obj.EntrySource = "EXL";
                        obj.CurrencyID = Convert.ToInt32(dt.Rows[i]["CurrencyID"].ToString());
                        obj.Currency = dt.Rows[i]["Currency"].ToString();
                        obj.CustomsValue = Convert.ToDecimal(dt.Rows[i]["CustomsValue"].ToString());
                        //obj.CourierStatusID = Convert.ToInt32(dt.Rows[i]["CourierStatusID"].ToString());
                        //obj.StatusTypeId = Convert.ToInt32(dt.Rows[i]["StatusTypeId"].ToString()); 
                        obj.CustomerID = CustomerID;
                        obj.ForwardingAWBNo = dt.Rows[i]["FwdNo"].ToString();
                        obj.Route = dt.Rows[i]["Route"].ToString();
                        obj.FAgentID = CommonFunctions.ParseInt(dt.Rows[i]["FAgentID"].ToString());
                        obj.AWBValidStatus = Convert.ToBoolean(dt.Rows[i]["AWBValidStatus"].ToString());
                        if (obj.AWBValidStatus==false)
                        {
                            obj.AwbValidStatusclass = "awbvalidfalse";
                        }

                        else
                        {
                            obj.AwbValidStatusclass = "";
                        }
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

        public static SaveStatusModel SaveAWBBatch(int BATCHID, int BranchID, int AcCompanyID, int UserID, int FYearID, string Details)
        {
            SaveStatusModel status = new SaveStatusModel();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_SaveInboundShipmentBatch";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Parameters.AddWithValue("@BatchID", BATCHID);
                cmd.Parameters.AddWithValue("@BranchID", BranchID);
                cmd.Parameters.AddWithValue("@AcCompanyID", AcCompanyID);                
                cmd.Parameters.AddWithValue("@UserID", UserID);
                cmd.Parameters.AddWithValue("@FYearId", FYearID);
                Details = Details.Replace("'", "");
                cmd.Parameters.AddWithValue("@FormatForXMLitem", Details);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    status.Status  = ds.Tables[0].Rows[0]["Status"].ToString();
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
                cmd.CommandText = "SP_SaveInboundBatchShipmentPosting";
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
        //SP_AWBPosting
        public static string ShipmentAWBPosting(int Id)
        {
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_InboundAWBPosting " + Id.ToString();
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

        public static SaveStatusModel UpdateShipmementTrackStatus(int BATCHID, int BranchID, int AcCompanyID, int UserID, int FYearID, int NewCourierStatusId, DateTime EntryDate, string Remarks)
        {
            SaveStatusModel status = new SaveStatusModel();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_UpdateShipmentBatchStatus";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BatchID", BATCHID);
                cmd.Parameters.AddWithValue("@BranchID", BranchID);
                cmd.Parameters.AddWithValue("@AcCompanyID", AcCompanyID);
                cmd.Parameters.AddWithValue("@UserID", UserID);
                cmd.Parameters.AddWithValue("@FYearId", FYearID);
                cmd.Parameters.AddWithValue("@NewCourierStatusID", NewCourierStatusId);
                cmd.Parameters.AddWithValue("@EntryDate", Convert.ToDateTime(EntryDate).ToString("MM/dd/yyyy HH:mm"));
                cmd.Parameters.AddWithValue("@Remarks", Remarks);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    status.Status = ds.Tables[0].Rows[0]["Status"].ToString();
                    status.Message = ds.Tables[0].Rows[0]["Message"].ToString();
                    //status.TotalImportCount = Convert.ToInt32(ds.Tables[0].Rows[0]["TotalImportCount"].ToString());
                    //status.TotalSavedCount = Convert.ToInt32(ds.Tables[0].Rows[0]["TotalSaved"].ToString());
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
        public static SaveStatusModel UpdateBatchShipmementTrackStatus(int BATCHID, int BranchID, int AcCompanyID, int UserID, int FYearID, int NewCourierStatusId,DateTime EntryDate,string Remarks)
        {
            SaveStatusModel status = new SaveStatusModel();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_UpdateInboundShipmentBatchStatus";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BatchID", BATCHID);
                cmd.Parameters.AddWithValue("@BranchID", BranchID);
                cmd.Parameters.AddWithValue("@AcCompanyID", AcCompanyID);
                cmd.Parameters.AddWithValue("@UserID", UserID);
                cmd.Parameters.AddWithValue("@FYearId", FYearID);
                cmd.Parameters.AddWithValue("@NewCourierStatusID", NewCourierStatusId);
                cmd.Parameters.AddWithValue("@EntryDate", Convert.ToDateTime(EntryDate).ToString("MM/dd/yyyy HH:mm"));
                cmd.Parameters.AddWithValue("@Remarks", Remarks);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    status.Status = ds.Tables[0].Rows[0]["Status"].ToString();
                    status.Message = ds.Tables[0].Rows[0]["Message"].ToString();
                    //status.TotalImportCount = Convert.ToInt32(ds.Tables[0].Rows[0]["TotalImportCount"].ToString());
                    //status.TotalSavedCount = Convert.ToInt32(ds.Tables[0].Rows[0]["TotalSaved"].ToString());
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
        public static SaveStatusModel DeleteAWBBatch(int BATCHID, int BranchID, int AcCompanyID, int UserID, int FYearID, string Details)
        {
            SaveStatusModel status = new SaveStatusModel();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_DeleteInboundShipmentBatch";
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

        public static DataTable DeleteBatch(int BatchId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteInboundBatch";
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


        public static List<ExportAWBList> GetManifestAWBInfo(int SearchOption, string AWBNo,int ManifestID,int FYearId,int BranchId,string OriginCountry,string DestinationCountry,int CoLoaderID)
        {
            List<ExportAWBList> list = new List<ExportAWBList>();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetManifestAWBSearch";
            cmd.CommandType = CommandType.StoredProcedure;
            
            cmd.Parameters.AddWithValue("@SearchOption", SearchOption);
            cmd.Parameters.AddWithValue("@AWBNo", AWBNo);
            cmd.Parameters.AddWithValue("@FYearId", FYearId);
            cmd.Parameters.AddWithValue("@BranchID", BranchId);
            cmd.Parameters.AddWithValue("@ManifestID", ManifestID);
            cmd.Parameters.AddWithValue("@OriginCountry", OriginCountry);
            cmd.Parameters.AddWithValue("@DestinationCountry", DestinationCountry);
            cmd.Parameters.AddWithValue("@COLoaderID", CoLoaderID);

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
                        ExportAWBList obj = new ExportAWBList();
                        obj.InScanId = CommonFunctions.ParseInt(dt.Rows[i]["InScanId"].ToString());
                        obj.ShipmentID = CommonFunctions.ParseInt(dt.Rows[i]["ShipmentID"].ToString());
                        obj.AWBDate = Convert.ToDateTime(dt.Rows[i]["AWBDate"].ToString());
                        obj.AWBNo = dt.Rows[i]["AWBNo"].ToString();
                        
                       
                        obj.Consignor = dt.Rows[i]["Consignor"].ToString();
                        
                        obj.ConsignorCountryName = dt.Rows[i]["ConsignorCountryName"].ToString();
                        obj.ConsignorCityName = dt.Rows[i]["ConsignorCityName"].ToString();
                       

                        obj.Consignee = dt.Rows[i]["Consignee"].ToString();
                        obj.ConsigneeCountryName = dt.Rows[i]["ConsigneeCountryName"].ToString();
                         
                        obj.ConsigneeCityName = dt.Rows[i]["ConsigneeCityName"].ToString();
                       

                       
                        obj.CargoDescription = dt.Rows[i]["CargoDescription"].ToString();
                        obj.Pieces = dt.Rows[i]["Pieces"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["Pieces"].ToString());
                        obj.Weight = dt.Rows[i]["Weight"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["Weight"].ToString());
                        // obj.CurrencyID = dt.Rows[i]["CurrencyID"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["CurrencyID"].ToString());
                        //obj.Currency = dt.Rows[i]["CurrencyName"].ToString();
                        //obj.CustomsValue = dt.Rows[i]["CustomsValue"] == DBNull.Value ? 0 : Convert.ToDecimal(dt.Rows[i]["CustomsValue"].ToString());
                        //obj.FAgentID = dt.Rows[i]["FAgentId"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i]["FAgentId"].ToString());
                        //obj.FAgentName = dt.Rows[i]["FAgentName"] == DBNull.Value ? "" : dt.Rows[i]["FAgentName"].ToString();
                        //obj.CustomerRateType = GetCustomerRateName(obj.CustomerRateTypeId);
                        obj.BagNo = dt.Rows[i]["BagNo"] == DBNull.Value ? "" : dt.Rows[i]["BagNo"].ToString();
                       // obj.MAWB = dt.Rows[i]["MAWB"] == DBNull.Value ? "" : dt.Rows[i]["MAWB"].ToString();
                        list.Add(obj);
                    }

                }

            }


            return list;

        }


        //upload csv extracted to Manifest Export
        public static List<ExportShipmentDetailVM> GetManifestValidAWBDetails(int BranchID, string Details, string AWBNo, int ExportID)
        {
            List<ExportShipmentDetailVM> list = new List<ExportShipmentDetailVM>();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_GetExportManifestValidAWBDetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BranchID", BranchID);

                cmd.Parameters.AddWithValue("@FormatForXMLitem", Details);
                cmd.Parameters.AddWithValue("@ExportID", ExportID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);


                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        ExportShipmentDetailVM obj = new ExportShipmentDetailVM();

                        obj.ShipmentDetailID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ShipmentDetailID"].ToString());
                        obj.ExportID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ExportID"].ToString());
                        obj.InboundShipmentID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["InboundShipmentID"].ToString());
                        obj.InscanId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["InscanId"].ToString());
                        obj.CurrencyID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CurrencyID"].ToString());
                        obj.AWBNo = ds.Tables[0].Rows[i]["AWBNo"].ToString();
                        obj.AWBDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["AWBDate"].ToString());
                        obj.PCS = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["PCS"].ToString());
                        obj.Weight = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Weight"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());                    
                        obj.Contents = ds.Tables[0].Rows[i]["Contents"].ToString();
                        obj.Shipper = ds.Tables[0].Rows[i]["Shipper"].ToString();
                        obj.Receiver = ds.Tables[0].Rows[i]["Receiver"].ToString();
                        obj.OriginCountry = ds.Tables[0].Rows[i]["OriginCountry"].ToString();
                        obj.ConsignorAddress1 = ds.Tables[0].Rows[i]["ConsignorAddress1_Building"].ToString();
                        obj.ConsignorAddress2 = ds.Tables[0].Rows[i]["ConsignorAddress2_Street"].ToString();
                        obj.ConsigneeAddress1 = ds.Tables[0].Rows[i]["ConsigneeAddress1_Building"].ToString();
                        obj.ConsigneeAddress2 = ds.Tables[0].Rows[i]["ConsigneeAddress2_Street"].ToString();
                        obj.OriginCity = ds.Tables[0].Rows[i]["OriginCity"].ToString();
                        obj.DestinationCountry = ds.Tables[0].Rows[i]["DestinationCountry"].ToString();
                        obj.DestinationCity = ds.Tables[0].Rows[i]["DestinationCity"].ToString();
                        obj.ConsigneePhone = ds.Tables[0].Rows[i]["ConsigneePhone"].ToString();
                        obj.ConsignorPhone = ds.Tables[0].Rows[i]["ConsignorPhone"].ToString();
                        obj.BagNo = ds.Tables[0].Rows[i]["ExpBagNo"].ToString();
                        obj.CurrenySymbol = ds.Tables[0].Rows[i]["CurrencyName"].ToString();
                        obj.CustomsValue = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Value"].ToString());
                        obj.FwdAgentName = ds.Tables[0].Rows[i]["FwdAgentName"].ToString();
                        obj.FwdAgentAWBNo = ds.Tables[0].Rows[i]["FwdAgentAWBNo"].ToString();
                        obj.FwdAgentId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["FwdAgentId"].ToString());
                        obj.FwdCharge = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["FwdCharge"].ToString());

                        obj.VerifiedWeight = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["VerifiedWeight"].ToString());
                        obj.AWBChecked = true;
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


        public static string UpdateColoaderShipmentRate(int BatchId)
        {
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_UpdateColoaderShipmentCourierCharge";
                cmd.CommandTimeout = 2000;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BatchID", BatchId);
                cmd.Parameters.AddWithValue("@ConsigneeCountryName", "");
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();

                return "OK";
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }

        public static string UpdateColoaderShipmentFWDRate(int BatchId)
        {
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_UpdateColoaderShipmentFwdAgentRate";
                cmd.CommandTimeout = 2000;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BatchID", BatchId);
                cmd.Parameters.AddWithValue("@ConsigneeCountryName", "");
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();

                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public static SaveStatusModel SaveColoaderInvoiceBatch(int AgentInvoiceID,  string Details)
        {
            SaveStatusModel status = new SaveStatusModel();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_SaveCoLoaderInvoiceBulk";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0; 
                cmd.Parameters.AddWithValue("@AgentInvoiceID ", AgentInvoiceID);
              
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


        public static DataTable GetColoaderBatchReportExcel(int BatchID)
        {
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_CoLoaderBatchReportExcel";
            cmd.CommandType = CommandType.StoredProcedure;
            
            cmd.Parameters.AddWithValue("@BatchId", BatchID);
            

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            return ds.Tables[0];


        }

        public static DataTable GetTaxInvoiceAWBExcel(int ShipmentInvoiceId)
        {
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_TaxInvoiceAWBExcel";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@ShipmentInvoiceId", ShipmentInvoiceId);


            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            return ds.Tables[0];


        }

        public static List<ImportFixationSource> GetTranshipmentSource(int BatchID, string FieldName)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetTranshipmentSourceValue";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@BatchId", BatchID);
            cmd.Parameters.AddWithValue("@FieldName", FieldName);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<ImportFixationSource> objList = new List<ImportFixationSource>();
            ImportFixationSource obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new ImportFixationSource();
                    obj.SourceValue = ds.Tables[0].Rows[i][0].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }
    }
}