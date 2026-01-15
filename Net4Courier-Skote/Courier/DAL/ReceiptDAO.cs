using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Net4Courier.Models;

namespace Net4Courier.DAL
{
    public class ReceiptDAO
    {
        public static string RevertInvoiceIdtoInscanMaster(int InvoiceId)
        {
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandTimeout = 0;
                cmd.CommandText = "Update InscanMaster set InvoiceID=null where Isnull(InvoiceId,0)=" + Convert.ToString(InvoiceId);
                cmd.CommandType = CommandType.Text;
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                SqlCommand cmd1 = new SqlCommand();
                cmd1.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandTimeout = 0;
                cmd1.CommandText = "Update Inboundshipment set AgentInvoiceID=null where Isnull(AgentInvoiceId,0)=" + Convert.ToString(InvoiceId);
                cmd1.CommandType = CommandType.Text;
                cmd1.Connection.Open();
                cmd1.ExecuteNonQuery();



                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }






        }

        public static string CheckAWBInvoicedStatus(string AWBNo)
        {
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "Select inscanid from InscanMaster Where AWbNo in(" + AWBNo + ") and isdeleted=0 and Isnull(InvoiceID,0)>0";
                cmd.CommandType = CommandType.Text;
               
                
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count > 0)
                {

                    return "Invoiced";
                }
                else
                {
                    return "No";
                }

                               
            }
            catch (Exception ex)
            {
                return ex.Message;
            }






        }

        public static string RevertInvoiceIdtoInboundShipment(int InvoiceId)
        {
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "Update InboundShipment set AgentInvoiceID=null where Isnull(AgentInvoiceId,0)=" + Convert.ToString(InvoiceId);
                cmd.CommandType = CommandType.Text;
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }






        }

        //Delete tax invoice of importshipment 
        public static DataTable DeleteTaxInvoice(int ShipmentInvoiceId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteTaxInvoice";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ShipmentInvoiceId", @ShipmentInvoiceId);
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

        public static string GetMaxEmployeeCode()
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
                        cmd.CommandText = "SP_GetMaxEmployeeCode";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;
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
        public static List<OpeningInvoiceVM> GetCustomerOpeningReceipts(int CustomerId, int BranchId, int FYearId)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetCustomerOpeningCredit";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
            cmd.Parameters.AddWithValue("@BranchId", BranchId);
            cmd.Parameters.AddWithValue("@FYearId", FYearId);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<OpeningInvoiceVM> objList = new List<OpeningInvoiceVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    OpeningInvoiceVM obj = new OpeningInvoiceVM();
                    obj.ACOPInvoiceDetailId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ACOPInvoiceDetailId"].ToString());
                    obj.CustomerId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CustomerId"].ToString());
                    //obj.InvoiceDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["RecPayDate"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.RefNo = ds.Tables[0].Rows[i]["RefNo"].ToString();

                    if (ds.Tables[0].Rows[i]["Amount"] == DBNull.Value)
                    {
                        obj.Amount = 0;
                    }
                    else
                    {
                        obj.Amount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Amount"].ToString());
                    }

                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static List<OpeningInvoiceVM> GetSupplierOpeningPayments(int SupplierId, int BranchId, int FYearId)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetSupplierOpeningDebit";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SupplierId", SupplierId);
            cmd.Parameters.AddWithValue("@BranchId", BranchId);
            cmd.Parameters.AddWithValue("@FYearId", FYearId);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<OpeningInvoiceVM> objList = new List<OpeningInvoiceVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    OpeningInvoiceVM obj = new OpeningInvoiceVM();
                    obj.ACOPInvoiceDetailId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ACOPInvoiceDetailId"].ToString());
                    obj.SupplierId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["SupplierId"].ToString());
                    //obj.InvoiceDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["RecPayDate"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.RefNo = ds.Tables[0].Rows[i]["RefNo"].ToString();

                    if (ds.Tables[0].Rows[i]["Amount"] == DBNull.Value)
                    {
                        obj.Amount = 0;
                    }
                    else
                    {
                        obj.Amount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Amount"].ToString());
                    }

                    objList.Add(obj);
                }
            }
            return objList;
        }
        //CustomerInvoiceDetailForReceipt
        public static List<ReceiptVM> GetCustomerReceipts(int FYearId, DateTime FromDate, DateTime ToDate)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetAllRecieptsDetails";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@FYearId", FYearId);
            cmd.Parameters.AddWithValue("@BranchId", branchid);
            //cmd.Parameters.Add("@AcJournalDetailID", SqlDbType.Int);
            //cmd.Parameters["@AcJournalDetailID"].Value = AcJournalDetailID;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<ReceiptVM> objList = new List<ReceiptVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    ReceiptVM obj = new ReceiptVM();
                    obj.RecPayID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["RecPayID"].ToString());
                    obj.RecPayDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["RecPayDate"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.DocumentNo = ds.Tables[0].Rows[i]["DocumentNo"].ToString();
                    obj.PartyName = ds.Tables[0].Rows[i]["PartyName"].ToString();
                    obj.PartyName = ds.Tables[0].Rows[i]["PartyName"].ToString();
                    if (ds.Tables[0].Rows[i]["Amount"] == DBNull.Value)
                    {
                        obj.Amount = 0;
                    }
                    else
                    {
                        obj.Amount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Amount"].ToString());
                    }
                    obj.Currency = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Currency"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static List<ReceiptVM> GetCustomerReceiptsByDate(DateTime FromDate, DateTime ToDate, int FyearID, string ReceiptNo, string Type)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetAllRecieptsDetailsByDate";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@FromDate", Convert.ToDateTime(FromDate).ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@Todate", Convert.ToDateTime(ToDate).ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@FyearId", FyearID);
            cmd.Parameters.AddWithValue("@BranchId", branchid);
            cmd.Parameters.AddWithValue("@ReceiptNo", ReceiptNo);
            cmd.Parameters.AddWithValue("@Type", Type);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<ReceiptVM> objList = new List<ReceiptVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    ReceiptVM obj = new ReceiptVM();
                    obj.RecPayID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["RecPayID"].ToString());
                    obj.RecPayDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.DocumentNo = ds.Tables[0].Rows[i]["DocumentNo"].ToString();
                    obj.PaymentMode = ds.Tables[0].Rows[i]["PaymentMode"].ToString();
                    obj.BankName = ds.Tables[0].Rows[i]["BankName"].ToString();
                    obj.PartyName = ds.Tables[0].Rows[i]["PartyName"].ToString();
                    obj.Remarks = ds.Tables[0].Rows[i]["Remarks"].ToString();
                    if (ds.Tables[0].Rows[i]["Amount"] == DBNull.Value)
                    {
                        obj.Amount = 0;
                    }
                    else
                    {
                        obj.Amount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Amount"].ToString());
                    }
                    obj.Currency = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Currency"].ToString());
                    obj.ChequeNo = ds.Tables[0].Rows[i]["ChequeNo"].ToString();
                    obj.ChequeDate = ds.Tables[0].Rows[i]["ChequeDate"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static List<ReceiptVM> GetSupplierPaymentsByDate(DateTime FromDate, DateTime ToDate, int FyearID, string ReceiptNo)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetAllPaymentDetailsByDate";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@FromDate", Convert.ToDateTime(FromDate).ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@Todate", Convert.ToDateTime(ToDate).ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@FyearId", FyearID);
            cmd.Parameters.AddWithValue("@BranchId", branchid);
            cmd.Parameters.AddWithValue("@ReceiptNo", ReceiptNo);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<ReceiptVM> objList = new List<ReceiptVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    ReceiptVM obj = new ReceiptVM();
                    obj.RecPayID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["RecPayID"].ToString());
                    obj.RecPayDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.DocumentNo = ds.Tables[0].Rows[i]["DocumentNo"].ToString();
                    obj.PaymentMode = ds.Tables[0].Rows[i]["PaymentMode"].ToString();
                    obj.BankName = ds.Tables[0].Rows[i]["BankName"].ToString();
                    obj.Remarks = ds.Tables[0].Rows[i]["Remarks"].ToString();
                    obj.PartyName = ds.Tables[0].Rows[i]["PartyName"].ToString();
                    if (ds.Tables[0].Rows[i]["Amount"] == DBNull.Value)
                    {
                        obj.Amount = 0;
                    }
                    else
                    {
                        obj.Amount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Amount"].ToString());
                    }
                    obj.Currency = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Currency"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static List<CustomerInvoiceDetailForReceipt> GetCustomerInvoiceDetailsForReciept(int CustomerID, string FromDate, string ToDate)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetCustomerInvoiceDetailsForReciept";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters["@CustomerID"].Value = CustomerID;
            cmd.Parameters["@FromDate"].Value = Convert.ToDateTime(FromDate).Date;
            cmd.Parameters["@ToDate"].Value = Convert.ToDateTime(ToDate).Date;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<CustomerInvoiceDetailForReceipt> objList = new List<CustomerInvoiceDetailForReceipt>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    CustomerInvoiceDetailForReceipt obj = new CustomerInvoiceDetailForReceipt();
                    obj.InvoiceNo = ds.Tables[0].Rows[i]["InvoiceNo"].ToString();
                    obj.CustomerInvoiceID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["InvoiceNo"].ToString());
                    obj.InvoiceDate = CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["InvoiceDate"].ToString());
                    obj.CurrencyName = ds.Tables[0].Rows[i]["CurrencyName"].ToString();

                    if (ds.Tables[0].Rows[i]["AmountToBeReceived"] == DBNull.Value)
                    {
                        obj.AmountToBeReceived = 0;
                    }
                    else
                    {
                        obj.AmountToBeReceived = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["AmountToBeReceived"].ToString());
                    }

                    if (ds.Tables[0].Rows[i]["AmtPaidTillDate"] == DBNull.Value)
                    {
                        obj.AmtPaidTillDate = 0;
                    }
                    else
                    {
                        obj.AmtPaidTillDate = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["AmtPaidTillDate"].ToString());
                    }

                    if (ds.Tables[0].Rows[i]["Balance"] == DBNull.Value)
                    {
                        obj.Balance = 0;
                    }
                    else
                    {
                        obj.Balance = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Balance"].ToString());
                    }

                    if (ds.Tables[0].Rows[i]["Amount"] == DBNull.Value)
                    {
                        obj.Amount = 0;
                    }
                    else
                    {
                        obj.Amount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Amount"].ToString());
                    }


                    if (ds.Tables[0].Rows[i]["Advance"] == DBNull.Value)
                    {
                        obj.Advance = 0;
                    }
                    else
                    {
                        obj.Advance = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Advance"].ToString());
                    }



                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static string GetMaxCustomerReceiptNo()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetMaxCustomerReceiptNo";
            cmd.CommandType = CommandType.StoredProcedure;
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
        public static string GetMaxCOLoaderReceiptNo()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetMaxCOLoaderReceiptNo";
            cmd.CommandType = CommandType.StoredProcedure;
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
        public static int AddCustomerRecieptPayment(CustomerRcieptVM RecPy, string UserID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_InsertRecPay";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RecPayDate", RecPy.RecPayDate);
            cmd.Parameters.AddWithValue("@DocumentNo", RecPy.DocumentNo);
            cmd.Parameters.AddWithValue("@CustomerID", RecPy.CustomerID);
            //cmd.Parameters.AddWithValue("@SupplierID", RecPy.SupplierID);
            cmd.Parameters.AddWithValue("@BusinessCentreID", RecPy.BusinessCentreID);
            cmd.Parameters.AddWithValue("@BankName", RecPy.BankName);
            cmd.Parameters.AddWithValue("@ChequeNo", RecPy.ChequeNo);
            cmd.Parameters.AddWithValue("@ChequeDate", RecPy.ChequeDate);
            cmd.Parameters.AddWithValue("@Remarks", RecPy.Remarks);
            cmd.Parameters.AddWithValue("@AcJournalID", RecPy.AcJournalID);
            cmd.Parameters.AddWithValue("@StatusRec", RecPy.StatusRec);
            cmd.Parameters.AddWithValue("@StatusEntry", RecPy.StatusEntry);
            cmd.Parameters.AddWithValue("@StatusOrigin", RecPy.StatusOrigin);
            cmd.Parameters.AddWithValue("@FYearID", RecPy.FYearID);
            cmd.Parameters.AddWithValue("@AcCompanyID", RecPy.AcCompanyID);
            cmd.Parameters.AddWithValue("@EXRate", RecPy.EXRate);
            cmd.Parameters.AddWithValue("@FMoney", RecPy.FMoney);
            cmd.Parameters.AddWithValue("@UserID", RecPy.UserID);
            cmd.Parameters.AddWithValue("@EntryTime", CommonFunctions.GetCurrentDateTime());
            cmd.Parameters.AddWithValue("@AcOPInvoiceDetailID", RecPy.AcOPInvoiceDetailID);
            cmd.Parameters.AddWithValue("@OtherReceipt", RecPy.OtherReceipt);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
            if (ds.Tables[0].Rows.Count > 0)
            {
                return Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
            }
            else
            {
                return 0;
            }


        }

        public static List<CustomerReceivable> SPGetAllLocalCurrencyCustRecievable(int FinancialyearId)
        {
            List<CustomerReceivable> crecs = new List<CustomerReceivable>();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_InsertRecPay";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AcFinancialYearID", FinancialyearId);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
            if (ds.Tables[0].Rows.Count > 0)
            {

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    CustomerReceivable crec = new CustomerReceivable();
                    crec.InvoiceId = Convert.ToInt32(ds.Tables[0].Rows[i]["InvoiceId"].ToString());
                    crec.Receivable = Convert.ToDecimal(ds.Tables[0].Rows[i]["Receivable"].ToString());
                    crecs.Add(crec);
                }
            }
            else
            {

            }

            return crecs;
        }
        public static List<SupplierInvoiceVM> GetSupplierInvoiceList(DateTime FromDate, DateTime ToDate, int FyearID,int SupplierTypeId, string InvoiceNo)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetAllSupplierInvoice";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@FromDate", Convert.ToDateTime(FromDate).ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@Todate", Convert.ToDateTime(ToDate).ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@FyearId", FyearID);
            cmd.Parameters.AddWithValue("@BranchId", branchid);
            cmd.Parameters.AddWithValue("@SupplierTypeId",SupplierTypeId);            
            cmd.Parameters.AddWithValue("@InvoiceNo", InvoiceNo);
             

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<SupplierInvoiceVM> objList = new List<SupplierInvoiceVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    SupplierInvoiceVM obj = new SupplierInvoiceVM();
                    obj.SupplierInvoiceID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["SupplierInvoiceID"].ToString());
                    obj.InvoiceDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["InvoiceDate"].ToString());
                    obj.InvoiceNo = ds.Tables[0].Rows[i]["InvoiceNo"].ToString();                     
                    obj.SupplierName = ds.Tables[0].Rows[i]["SupplierName"].ToString();
                    if (ds.Tables[0].Rows[i]["Amount"] == DBNull.Value)
                    {
                        obj.Amount = 0;
                    }
                    else
                    {
                        obj.Amount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Amount"].ToString());                   
                    }
                    obj.SupplierType = ds.Tables[0].Rows[i]["SupplierType"].ToString();
                    obj.ReferenceNo = ds.Tables[0].Rows[i]["ReferenceNo"].ToString();
                    obj.Remarks = ds.Tables[0].Rows[i]["Remarks"].ToString();
                     
                    objList.Add(obj);
                }
            }
            return objList;
        }

        //public static CustomerRcieptVM GetRecPayByRecpayID(int RecpayID)
        //{
        //    SqlCommand cmd = new SqlCommand();
        //    CustomerRcieptVM cust = new CustomerRcieptVM();
        //    cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
        //    cmd.CommandText = "SP_GetCustomerRecieptByRecPayID";
        //    cmd.CommandType = CommandType.StoredProcedure;

        //    cmd.Parameters["@RecPayID"].Value = RecpayID;
        //    if (RecpayID <= 0)
        //        return new CustomerRcieptVM();
        //    var query = Context1.SP_GetCustomerRecieptByRecPayID(RecpayID);

        //    if (query != null)
        //    {
        //        var item = query.FirstOrDefault();
        //        cust.RecPayDate = item.RecPayDate;
        //        cust.DocumentNo = item.DocumentNo;
        //        cust.CustomerID = item.CustomerID;

        //        var cashOrBankID = (from t in Context1.AcHeads where t.AcHead1 == item.BankName select t.AcHeadID).FirstOrDefault();
        //        cust.CashBank = (cashOrBankID).ToString();
        //        cust.ChequeBank = (cashOrBankID).ToString();
        //        cust.ChequeNo = item.ChequeNo;
        //        cust.ChequeDate = item.ChequeDate;
        //        cust.Remarks = item.Remarks;
        //        cust.EXRate = item.EXRate;
        //        cust.FMoney = item.FMoney;
        //        cust.RecPayID = item.RecPayID;
        //        cust.SupplierID = item.SupplierID;
        //        cust.AcJournalID = item.AcJournalID;
        //        cust.StatusEntry = item.StatusEntry;

        //        var a = (from t in Context1.RecPayDetails where t.RecPayID == RecpayID select t.CurrencyID).FirstOrDefault();
        //        cust.CurrencyId = Convert.ToInt32(a.HasValue ? a.Value : 0);



        //    }

        //    else
        //    {
        //        return new CustomerRcieptVM();
        //    }

        //    return cust;
        //}

        public static string SP_GetMaxPVID()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetMaxPVID";
            cmd.CommandType = CommandType.StoredProcedure;
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
        public static int InsertRecpayDetailsForCust(int RecPayID, int InvoiceID, int JInvoiceID, decimal Amount, string Remarks, string StatusInvoice, bool StatusAdvance, string statusReceip, string InvDate, string InvNo, int CurrencyID, int invoiceStatus, decimal AdjustmentAmount)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_InsertRecPayDetailsForCustomer";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RecPayID", RecPayID);
            cmd.Parameters.AddWithValue("@InvoiceID", InvoiceID);
            cmd.Parameters.AddWithValue("@Amount", Amount);
            cmd.Parameters.AddWithValue("@Remarks", Remarks);
            cmd.Parameters.AddWithValue("@StatusInvoice", StatusInvoice);
            cmd.Parameters.AddWithValue("@StatusAdvance", StatusAdvance);
            cmd.Parameters.AddWithValue("@statusReceipt", statusReceip);
            cmd.Parameters.AddWithValue("@InvDate", InvDate);
            cmd.Parameters.AddWithValue("@InvNo", InvNo);
            cmd.Parameters.AddWithValue("@CurrencyID", CurrencyID);
            cmd.Parameters.AddWithValue("@invoiceStatus", invoiceStatus);
            cmd.Parameters.AddWithValue("@AdjustmentAmount", AdjustmentAmount);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
            if (ds.Tables[0].Rows.Count > 0)
            {
                return Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
            }
            else
            {
                return 0;
            }


        }

        public static void InsertJournalOfCustomer(int RecpayID, int fyearId)
        {
            //SP_InsertJournalEntryForRecPay
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_InsertJournalEntryForRecPay";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RecPayID", RecpayID);
            cmd.Parameters.AddWithValue("@AcFinnancialYearId", fyearId);
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();

            //Context1.SP_InsertJournalEntryForRecPay(RecpayID, fyaerId);
        }

        public static void InsertJournalOfCustomerDRR(int RecpayID, int fyearId)
        {
            //SP_InsertJournalEntryForRecPay
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_InsertJournalEntryForRecPayDRR";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RecPayID", RecpayID);
            cmd.Parameters.AddWithValue("@AcFinnancialYearId", fyearId);
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();

            //Context1.SP_InsertJournalEntryForRecPay(RecpayID, fyaerId);
        }
        //public int InsertRecpayDetailsForSup(int RecPayID, int InvoiceID, int JInvoiceID, decimal Amount, string Remarks, string StatusInvoice, bool StatusAdvance, string statusReceip, string InvDate, string InvNo, int CurrencyID, int invoiceStatus, int JobID)
        //{
        //    //todo:fix to run by sethu
        //    int query = Context1.SP_InsertRecPayDetailsForSupplier(RecPayID, InvoiceID, Amount, Remarks, StatusInvoice, StatusAdvance, statusReceip, InvDate, InvNo, CurrencyID, invoiceStatus, JobID);

        //    return query;
        //}


        //Delete Manifest Receipt
        public static DataTable DeleteManifestReceipt(int RecPayID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteManifestReceipts";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RecPayID", RecPayID);
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
        public static DataTable DeleteCustomerReceipt(int RecPayID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteCustomerReciepts";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RecPayID", RecPayID);
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

        public static DataTable DeleteAccountHead(int AcheadId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteAcHead";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AcHeadId", AcheadId);
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
        public static DataTable DeleteCustomer(int CustomerId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteCustomer";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
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
        public static DataTable DeleteInvoice(int InvoiceId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteCustomerInvoice";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CustomerInvoiceId", InvoiceId);
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
        public static DataTable DeleteCODInvoice(int InvoiceId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteCODInvoice";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CODInvoiceId", InvoiceId);
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
        public static DataTable DeleteCoLoaderInvoice(int InvoiceId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteCOLoaderInvoice";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AgentInvoiceId", InvoiceId);
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
        public static DataTable DeleteInscan(int InscanId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteInscan";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@InScanId", InscanId);
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
        public static DataTable DeleteCoLoaderShipment(int InscanId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteInboundshipment";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ShipmentId", InscanId);
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
        public static DataTable DeleteImportShipmentAWB(int ShipmentDetailId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteImportShipmentDetail";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ShipmentDetailId", ShipmentDetailId);
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
        //Delete inter branch impor shipment
        public static DataTable DeleteInterBranchImport(int ImportID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteImportShipment";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ImportId", ImportID);
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
        //Delete inter branch impor shipment
        public static string UpdateImportIDtoExport(int ImportID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_UpdateImportShipmentToExport";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ImportId", ImportID);
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            return "ok";            


        }

        //Delete import-Inscan items
        public static DataTable DeleteImportInscan(int QuickInscanId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteImportInScan";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@QuickInscanId", QuickInscanId);
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
        public static string DeleteDRSReconcDetail(int DRRID, int DRRDetailId)
        {

            SqlCommand cmd = new SqlCommand();
            try
            {
          
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteDRRDetail";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@DRRID", DRRID);
            cmd.Parameters.AddWithValue("@ID", DRRDetailId);
                cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            return "ok";
                }
        catch(Exception ex)
        {
                return ex.Message;
            }
        } 
        public static DataTable DeleteDRSReconc(int DRRID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteDRReconC";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@DRRID", DRRID);
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
        //Quick Inscan
        public static DataTable DeleteDepotInscan(int InscanId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteDepotInscan";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@QuickInScanId", InscanId);
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
        public static DataTable DeleteSupplierPayments(int RecPayID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteSupplierPayments";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RecPayID", RecPayID);
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

        public static DataTable DeleteDomesticCOD(int ReceiptId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteDomesticCODReciepts";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ReceiptID", ReceiptId);
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
        public static DataTable DeleteDRS(int DRSID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteDRS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@DRSID", DRSID);
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

        public static DataTable DeleteSupplier(int SupplierId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteSupplier";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SupplierId", SupplierId);
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

        public static DataTable DeleteForwardingAgent(int FAgentID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteForwardingAgent";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FAgentId", FAgentID);
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

        //Quick Customer Booking
        public static DataTable DeleteCustomerBooking(int InscanId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteCustomerBooking";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@InScanId", InscanId);
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
        public static List<ReceiptAllocationDetailVM> GetAWBAllocation(List<ReceiptAllocationDetailVM> list, int InvoiceId, decimal Amount,int RecpayId)
        {
            try

            {

                if (list == null)
                    list = new List<ReceiptAllocationDetailVM>();

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_GetInvoiceAWBAllocation";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@InvoiceId", InvoiceId);
                cmd.Parameters.AddWithValue("@ReceivedAmount", Amount);
                cmd.Parameters.AddWithValue("@RecPayId",  RecpayId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        DataRow drrow = ds.Tables[0].Rows[i];
                        ReceiptAllocationDetailVM item = new ReceiptAllocationDetailVM();
                        item.ID= Convert.ToInt32(drrow["ID"].ToString());
                        item.CustomerInvoiceId = Convert.ToInt32(drrow["CustomerInvoiceId"].ToString());
                        item.CustomerInvoiceDetailID = Convert.ToInt32(drrow["CustomerInvoiceDetailID"].ToString());
                        item.InScanID = Convert.ToInt32(drrow["InScanId"].ToString());
                        item.RecPayID = Convert.ToInt32(drrow["RecPayID"].ToString());
                        item.RecPayDetailID = Convert.ToInt32(drrow["RecPayDetailID"].ToString());
                        item.CustomerInvoiceDetailID = Convert.ToInt32(drrow["CustomerInvoiceDetailID"].ToString());
                        item.AWBNo = drrow["AWBNo"].ToString();
                        item.AWBDate = Convert.ToDateTime(drrow["AWBDate"].ToString()).ToString("dd-MM-yyyy");
                        item.TotalAmount = Convert.ToDecimal(drrow["TotalAmount"].ToString());
                        item.ReceivedAmount = Convert.ToDecimal(drrow["ReceivedAmount"].ToString());
                        item.PendingAmount = Convert.ToDecimal(drrow["PendingAmount"].ToString());
                        item.AllocatedAmount = Convert.ToDecimal(drrow["AllocatedAmount"].ToString());
                        item.Allocated = Convert.ToBoolean(drrow["Allocated"].ToString());

                        list.Add(item);

                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                return list;
            }

            return list;
        }

        #region "ExportCODReceipt"
        public static List<ExportShipmentVM> GetManifestId(int AgentId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetManifestId";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AgentId", AgentId);
            

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<ExportShipmentVM> objList = new List<ExportShipmentVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    ExportShipmentVM obj = new ExportShipmentVM();
                    obj.ID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ID"].ToString());
                    obj.ManifestNumber = ds.Tables[0].Rows[i]["ManifestNumber"].ToString();                    
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static List<CODReceiptVM> GetCODReceiptList(DateTime FromDate,DateTime ToDate,int ShipmentInvoiceID, int FYearID, int BranchId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_ManifestCODReceiptList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FromDate",FromDate);
            cmd.Parameters.AddWithValue("@ToDate", ToDate);
            cmd.Parameters.AddWithValue("@ShipmentInvoiceId ", ShipmentInvoiceID);
      
            cmd.Parameters.AddWithValue("@FYearID", FYearID);
            cmd.Parameters.AddWithValue("@BranchID", BranchId);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<CODReceiptVM> objList = new List<CODReceiptVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    CODReceiptVM obj = new CODReceiptVM();
                    obj.ReceiptID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ReceiptID"].ToString());
                    obj.ReceiptNo = ds.Tables[0].Rows[i]["ReceiptNo"].ToString();
                    obj.ReceiptDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["ReceiptDate"].ToString());
                    obj.InvoiceNo = ds.Tables[0].Rows[i]["InvoiceNo"].ToString();
                    obj.AgentName = ds.Tables[0].Rows[i]["SupplierName"].ToString();
                    obj.Amount = Convert.ToDecimal(ds.Tables[0].Rows[i]["Amount"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static List<CODReceiptVM> GetVATInvoice(int AgentId,int FYearID,int BranchId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetVATInvoice";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AgentId", AgentId);
            cmd.Parameters.AddWithValue("@FYearId", FYearID);
            cmd.Parameters.AddWithValue("@BranchId", BranchId);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<CODReceiptVM> objList = new List<CODReceiptVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    CODReceiptVM obj = new CODReceiptVM();
                    obj.ShipmentInvoiceID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ShipmentInvoiceId"].ToString());
                    obj.InvoiceNo = ds.Tables[0].Rows[i]["InvoiceNo"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static List<CODReceiptDetailVM> GetReceiptTAXInvoiceDetail(int ShipmentInvoiceID,int ReceiptID=0 )
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_ImportTaxInvoiceDetailforReceipt";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ShipmentInvoiceID", ShipmentInvoiceID);
            cmd.Parameters.AddWithValue("@ReceiptID", ReceiptID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<CODReceiptDetailVM> objList = new List<CODReceiptDetailVM>();
            CODReceiptDetailVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new CODReceiptDetailVM();
                    obj.AWBChecked = true;
                    obj.ReceiptDetailID = Convert.ToInt32(ds.Tables[0].Rows[i]["ReceiptDetailID"].ToString());
                   // obj.ShipmentInvoiceID = Convert.ToInt32(ds.Tables[0].Rows[i]["ShipmentInvoiceID"].ToString());
                    obj.AWBNo = ds.Tables[0].Rows[i]["AWBNo"].ToString();
                    obj.AWBDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["AWBDate"].ToString());
                    obj.Shipper = ds.Tables[0].Rows[i]["Consignor"].ToString();
                    obj.Receiver = ds.Tables[0].Rows[i]["Consignee"].ToString();
                    obj.InboundShipmentID = Convert.ToInt32(ds.Tables[0].Rows[i]["ShipmentID"].ToString());
                 
                    obj.COD = Convert.ToDecimal(ds.Tables[0].Rows[i]["COD"].ToString());
                  
                    objList.Add(obj);
                }
            }
            return objList;
        }
        #endregion
        #region "DRSREceipt"

        public static DataTable DeleteDRSRecPay(int DRSRecPayId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteDRSRecpay";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@DRSRecPayId", DRSRecPayId);
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

        public static string SaveDRSRecPayPosting(int DRSRecPayId,int FyearId,int BranchId,int CompanyId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DRSRecPayPosting";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RecPayId", DRSRecPayId);           
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            return "ok";

        }
        public static int AddDRSReceipt(DRSReceiptVM RecPy, string UserID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_InsertDRSRecPay";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RecPayDate", RecPy.DRSRecPayDate);
            cmd.Parameters.AddWithValue("@DocumentNo", RecPy.DocumentNo);
            cmd.Parameters.AddWithValue("@DRSID", RecPy.DRSID);
            cmd.Parameters.AddWithValue("@BankName", RecPy.BankName);
            cmd.Parameters.AddWithValue("@ChequeNo", RecPy.ChequeNo);
            cmd.Parameters.AddWithValue("@ChequeDate", RecPy.ChequeDate);
            cmd.Parameters.AddWithValue("@Remarks", RecPy.Remarks);
            cmd.Parameters.AddWithValue("@AcJournalID", RecPy.AcJournalID);            
            cmd.Parameters.AddWithValue("@StatusEntry", RecPy.StatusEntry);            
            cmd.Parameters.AddWithValue("@FYearID", RecPy.FYearID);
            cmd.Parameters.AddWithValue("@AcCompanyID", RecPy.AcCompanyID);
            cmd.Parameters.AddWithValue("@EXRate", RecPy.EXRate);
            cmd.Parameters.AddWithValue("@FMoney", RecPy.ReceivedAmount);
            cmd.Parameters.AddWithValue("@UserID", RecPy.UserID);
            cmd.Parameters.AddWithValue("@DeliveredBy", RecPy.DeliveredBy);
            cmd.Parameters.AddWithValue("@BranchId", RecPy.BranchId);
            cmd.Parameters.AddWithValue("@DRSBased", RecPy.DRSBased);
            cmd.Parameters.AddWithValue("@CollectedAmount", RecPy.CollectedAmount);
            cmd.Parameters.AddWithValue("@DRSDate", RecPy.DRSDate);
            cmd.Parameters.AddWithValue("@PODDate", RecPy.PODDate);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
            if (ds.Tables[0].Rows.Count > 0)
            {
                return Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
            }
            else
            {
                return 0;
            }


        }
        public static string GetMaxDRSReceiptNO()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetMaxDRSReceiptNo";
            cmd.CommandType = CommandType.StoredProcedure;
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

        public static List<DRRVM> GetDRRList(int FYearId, DateTime FromDate, DateTime ToDate, string DRRNo)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetDRRList";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@FYearId", FYearId);
            cmd.Parameters.AddWithValue("@DRRNo", DRRNo);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<DRRVM> objList = new List<DRRVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DRRVM obj = new DRRVM();
                    obj.DRRID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["DRRID"].ToString());
                    obj.DRRDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["DRRDate"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.DRSReceiptNo = ds.Tables[0].Rows[i]["DRSRecpayNo"].ToString();
                    obj.DRRNo = ds.Tables[0].Rows[i]["DRRNo"].ToString();
                    obj.DRSNo = ds.Tables[0].Rows[i]["DRSNO"].ToString();
                    obj.DRSReceiptDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["DRSRecPayDate"].ToString()).ToString("dd-MM-yyyy"); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.ReconciledAmount =Convert.ToDecimal(ds.Tables[0].Rows[i]["ReconciledAmount"].ToString());                  
                    obj.DeliveredByName = ds.Tables[0].Rows[i]["DeliveredByName"].ToString();
                    obj.UserName= ds.Tables[0].Rows[i]["UserName"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static List<DRSAWBITEM> GetDRSAWBPending(int DRSID,string AWBNo,int DRSRecPayId=0)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetDRSAWBPending";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@DRSID", DRSID);
            cmd.Parameters.AddWithValue("@AWBNo", AWBNo);
            cmd.Parameters.AddWithValue("@DRSRecPayId", DRSRecPayId);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<DRSAWBITEM> objList = new List<DRSAWBITEM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DRSAWBITEM obj = new DRSAWBITEM();
                    obj.InscanId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["InscanId"].ToString());
                    obj.ShipmentDetailID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ShipmentDetailID"].ToString());
                    obj.PaymentModeId= CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["PaymentModeId"].ToString());
                    obj.AWBDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["AWBDate"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.AWBNo = ds.Tables[0].Rows[i]["AWBNo"].ToString();
                    obj.CustomerName = ds.Tables[0].Rows[i]["CustomerName"].ToString();
                    obj.Shipper = ds.Tables[0].Rows[i]["Shipper"].ToString();
                    obj.PaymentMode = ds.Tables[0].Rows[i]["Paymode"].ToString();
                    obj.Consignee = ds.Tables[0].Rows[i]["Consignee"].ToString();
                    obj.MaterialCost = Convert.ToDecimal(ds.Tables[0].Rows[i]["MCPending"].ToString());
                    obj.CourierCharge = Convert.ToDecimal(ds.Tables[0].Rows[i]["CourierCharge"].ToString());
                    obj.CourierChargePending = Convert.ToDecimal(ds.Tables[0].Rows[i]["CODPending"].ToString());
                    obj.OtherCharge = Convert.ToDecimal(ds.Tables[0].Rows[i]["OtherCharge"].ToString());
                    obj.TotalCharge = Convert.ToDecimal(ds.Tables[0].Rows[i]["TotalCharge"].ToString());
                    obj.CODVatPending = Convert.ToDecimal(ds.Tables[0].Rows[i]["CODVATPending"].ToString());
                    obj.CODVAT = Convert.ToDecimal(ds.Tables[0].Rows[i]["CODVAT"].ToString());
                    obj.IsNCND = Convert.ToBoolean(ds.Tables[0].Rows[i]["IsNCND"].ToString());
                    obj.IsCashOnly = Convert.ToBoolean(ds.Tables[0].Rows[i]["IsCashOnly"].ToString());
                    obj.IsCollectMaterial = Convert.ToBoolean(ds.Tables[0].Rows[i]["IsCollectMaterial"].ToString());
                    obj.IsCheque = Convert.ToBoolean(ds.Tables[0].Rows[i]["IsCheque"].ToString());
                    obj.IsDoCopyBack = Convert.ToBoolean(ds.Tables[0].Rows[i]["IsDoCopyBack"].ToString());
                    obj.Pieces = ds.Tables[0].Rows[i]["Pieces"].ToString();
                    obj.Weight = Convert.ToDecimal(ds.Tables[0].Rows[i]["Weight"].ToString());
                    obj.LockedStatus= ds.Tables[0].Rows[i]["Locked"].ToString();
                    obj.CollectedBy = ds.Tables[0].Rows[i]["CollectedBy"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static List<DRRDetailVM> GetSklyarkAWB(int RecPayId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetSkylarkAWBforReconc";
            cmd.CommandType = CommandType.StoredProcedure;
 
            cmd.Parameters.AddWithValue("@DRSRecPayID", RecPayId);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<DRRDetailVM> objList = new List<DRRDetailVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DRRDetailVM obj = new DRRDetailVM();
                    
                    
                    obj.ReferenceId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ReferenceId"].ToString());
                    obj.TransactionType = ds.Tables[0].Rows[i]["TransactionType"].ToString();
                    obj.Reference = ds.Tables[0].Rows[i]["Reference"].ToString();
                    obj.PKPCash = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["PKPCash"].ToString());
                    obj.COD = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["CODAmount"].ToString());
                    obj.MCReceived = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["MCReceived"].ToString());
                    obj.MCPaid = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["MCPaid"].ToString());
                    obj.Receipt= CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Receipt"].ToString());
                    obj.Expense = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Expense"].ToString());
                    obj.Total = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Total"].ToString());
                    obj.Discount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Discount"].ToString());
                    obj.CustomerId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CustomerId"].ToString());
                    obj.InScanID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["InScanID"].ToString());
                    obj.ShipmentDetailID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ShipmentDetailId"].ToString());
                    obj.AWBDate1= ds.Tables[0].Rows[i]["AWBDate"].ToString();
                    obj.AcHeadID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ACHeadID"].ToString());
                    obj.CODVat = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["CODVat"].ToString());
                    obj.Confirmation =ds.Tables[0].Rows[i]["confirmation"].ToString();
                    obj.Total = obj.PKPCash + obj.COD + obj.MCReceived + obj.Receipt + obj.CODVat - obj.Expense;
                    obj.CustomerName = ds.Tables[0].Rows[i]["CustomerName"].ToString();
                    obj.PaymentModeId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["PaymentModeId"].ToString());
                    obj.Shipper = ds.Tables[0].Rows[i]["Consignor"].ToString();
                    obj.PaymentMode = ds.Tables[0].Rows[i]["PayMode"].ToString();
                    obj.Consignee = ds.Tables[0].Rows[i]["Consignee"].ToString();
                    obj.MaterialCost = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["MaterialCost"].ToString());
                     
                    obj.TotalCharge = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["TotalCharge"].ToString());
                    //obj.CODVatPending = 0;
                    //obj.CODVAT = Convert.ToDecimal(ds.Tables[0].Rows[i]["CODVAT"].ToString());
                    obj.IsNCND = Convert.ToBoolean(ds.Tables[0].Rows[i]["IsNCND"].ToString());
                    obj.IsCashOnly = Convert.ToBoolean(ds.Tables[0].Rows[i]["IsCashOnly"].ToString());
                    obj.IsCollectMaterial = Convert.ToBoolean(ds.Tables[0].Rows[i]["IsCollectMaterial"].ToString());
                    obj.IsCheque = Convert.ToBoolean(ds.Tables[0].Rows[i]["IsChequeOnly"].ToString());
                    obj.IsDoCopyBack = Convert.ToBoolean(ds.Tables[0].Rows[i]["IsDoCopyBack"].ToString());
                    obj.Pieces = ds.Tables[0].Rows[i]["Pieces"].ToString();
                    obj.Weight = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Weight"].ToString());
                    //obj.LockedStatus = ds.Tables[0].Rows[i]["Locked"].ToString();
                    obj.CollectedBy = ds.Tables[0].Rows[i]["CollectedBy"].ToString();
                    obj.TotalAWB = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["TotalAWB"].ToString());
                    //obj.Currency = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Currency"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static int GetSklyarkTotalAWB(int RecPayId)
        {
            int TotalAWB = 0;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetSkylarkTotalAWB";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@DRSRecPayID", RecPayId);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<DRRDetailVM> objList = new List<DRRDetailVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    TotalAWB= CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["TotalAWB"].ToString());                                         
                }
            }
            return TotalAWB;
        }

        public static List<DRSReceiptVM> GetDRSReceipts(int FYearId, DateTime FromDate, DateTime ToDate,string DocumentNo)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetAllDRSRecieptsDetails";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@FYearId", FYearId);
            cmd.Parameters.AddWithValue("@DocumentNo", DocumentNo);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<DRSReceiptVM> objList = new List<DRSReceiptVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DRSReceiptVM obj = new DRSReceiptVM();
                    obj.DRSRecPayID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["DRSRecPayID"].ToString());
                    obj.DRSRecPayDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["DRSRecPayDate"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.DocumentNo = ds.Tables[0].Rows[i]["DocumentNo"].ToString();
                    obj.DRSNo= ds.Tables[0].Rows[i]["DRSNo"].ToString();
                    obj.CourierEmpName = ds.Tables[0].Rows[i]["CourierName"].ToString();
                    obj.Remarks = ds.Tables[0].Rows[i]["Remarks"].ToString();
                    obj.Status  = ds.Tables[0].Rows[i]["Status"].ToString();
                    obj.DRRID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["DRRID"].ToString());
                    obj.DRRNo = ds.Tables[0].Rows[i]["DRRNo"].ToString();
                    if (ds.Tables[0].Rows[i]["Amount"] == DBNull.Value)
                    {
                        obj.ReceivedAmount = 0;
                    }
                    else
                    {
                       obj.ReceivedAmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Amount"].ToString());
                    }

                    if (ds.Tables[0].Rows[i]["CollectedAmount"] == DBNull.Value)
                    {
                        obj.CollectedAmount = 0;
                    }
                    else
                    {
                        obj.CollectedAmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["CollectedAmount"].ToString());
                    }
                    //obj.Currency = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Currency"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }


        //skylark courier epxnese
        public static List<CourierExpensesVM> GetDRSExpenses(int FYearId, DateTime FromDate, DateTime ToDate, int EmployeeId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetAllDRSExpenseDetails";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@FYearId", FYearId);
            cmd.Parameters.AddWithValue("@EmployeeID", EmployeeId);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<CourierExpensesVM> objList = new List<CourierExpensesVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    CourierExpensesVM obj = new CourierExpensesVM();
                    obj.ExpenseID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ExpenseID"].ToString());
                    obj.ExpenseDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["ExpenseDate"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.DRSDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["DRSDate"].ToString());
                    obj.DRSNo = ds.Tables[0].Rows[i]["DRSNo"].ToString();
                    obj.RevenueType = ds.Tables[0].Rows[i]["RevenueType"].ToString();
                    obj.Remarks = ds.Tables[0].Rows[i]["Remarks"].ToString();
                    obj.CourierName = ds.Tables[0].Rows[i]["CourierName"].ToString();
                    if (ds.Tables[0].Rows[i]["ExpenseAmount"] == DBNull.Value)
                    {
                        obj.ExpenseAmount = 0;
                    }
                    else
                    {
                        obj.ExpenseAmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["ExpenseAmount"].ToString());
                    }
                    //obj.Currency = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Currency"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }


        //skylark couriercollection
        public static List<CourierCollectionVM> GetDRSCourierCollection(int FYearId, DateTime FromDate, DateTime ToDate, int EmployeeId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetSkylarkCourierCollection";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FromDate", FromDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@ToDate", ToDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@FYearId", FYearId);
            cmd.Parameters.AddWithValue("@EmployeeID", EmployeeId);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<CourierCollectionVM> objList = new List<CourierCollectionVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    CourierCollectionVM obj = new CourierCollectionVM();
                    obj.CollectionId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CollectionID"].ToString());
                    obj.CollectedDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["CollectedDate"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.AWBNo = ds.Tables[0].Rows[i]["AWBNumber"].ToString();

                    obj.CollectionType = ds.Tables[0].Rows[i]["CollectionType"].ToString();
                    obj.Remarks = ds.Tables[0].Rows[i]["Remarks"].ToString();
                    obj.CourierName = ds.Tables[0].Rows[i]["CourierName"].ToString();
                    if (ds.Tables[0].Rows[i]["PickupCash"] == DBNull.Value)
                    {
                        obj.PickupCash = 0;
                    }
                    else
                    {
                        obj.PickupCash = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["PickupCash"].ToString());
                    }
                    if (ds.Tables[0].Rows[i]["COD"] == DBNull.Value)
                    {
                        obj.COD = 0;
                    }
                    else
                    {
                        obj.COD = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["COD"].ToString());
                    }

                    if (ds.Tables[0].Rows[i]["PickupCash"] == DBNull.Value)
                    {
                        obj.PickupCash = 0;
                    }
                    else
                    {
                        obj.PickupCash = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["PickupCash"].ToString());
                    }
                    if (ds.Tables[0].Rows[i]["MaterialCost"] == DBNull.Value)
                    {
                        obj.MaterialCost = 0;
                    }
                    else
                    {
                        obj.MaterialCost = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["MaterialCost"].ToString());
                    }

                    if (ds.Tables[0].Rows[i]["OtherAmount"] == DBNull.Value)
                    {
                        obj.OtherAmount = 0;
                    }
                    else
                    {
                        obj.OtherAmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["OtherAmount"].ToString());
                    }

                    obj.ModifiedDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["ModifiedDate"].ToString());
                    obj.LogDetail = ds.Tables[0].Rows[i]["ModifiedBy"].ToString() + " : " + Convert.ToDateTime(ds.Tables[0].Rows[i]["ModifiedDate"].ToString()).ToString("dd-MM-yyyy hh:mm");
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static DataTable DeleteCourierExpenses(int ExpenseId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteCourierExpenses";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ExpenseID", ExpenseId);
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
        public static List<DRSReceiptDetailVM> GetDRSAWB(int DRSID,int RecPayId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetDRSAWB";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@DRSID", DRSID );
            cmd.Parameters.AddWithValue("@DRSRecPayID",RecPayId);           

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<DRSReceiptDetailVM> objList = new List<DRSReceiptDetailVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DRSReceiptDetailVM obj = new DRSReceiptDetailVM();
                    obj.DRSRecPayID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["DRSRecPayID"].ToString());
                    obj.DRSRecPayDetailID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["DRSRecPayDetailID"].ToString());
                    obj.InScanID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["InScanID"].ToString());
                    obj.AWBDate = ds.Tables[0].Rows[i]["AWBDate"].ToString();
                    obj.AWBNO = ds.Tables[0].Rows[i]["AWBNo"].ToString();                    
                    obj.CourierCharge= CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["CourierCharge"].ToString());
                    obj.MaterialCost= CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["MaterialCost"].ToString());
                    obj.CCReceived = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["CCReceived"].ToString());
                    obj.MCReceived = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["MCReceived"].ToString());
                    obj.TotalAmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["TotalAmount"].ToString());
                    obj.Discount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Discount"].ToString());
                    //obj.Currency = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Currency"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static List<DRSVM> GetDRSSummary(string term,int DeliveredBy, int RecPayId,int BranchID,int FyearId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DRSRunSheetSummary";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@term ", term);
            cmd.Parameters.AddWithValue("@DeliveredBy", DeliveredBy);
            cmd.Parameters.AddWithValue("@RecPayId", RecPayId);
            cmd.Parameters.AddWithValue("@BranchID", BranchID);
            cmd.Parameters.AddWithValue("@FYearId", FyearId);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<DRSVM> objList = new List<DRSVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DRSVM obj = new DRSVM();
                    obj.DRSID= CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["DRSID"].ToString());
                    obj.DRSNo = ds.Tables[0].Rows[i]["DRSNo"].ToString();
                    
                    obj.DRSDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["DRSDate"].ToString());                     
                    obj.TotalAmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["TotalAmount"].ToString());
                    
                    objList.Add(obj);
                }
            }
            return objList;
        }
        #endregion

        #region "supplierInvoice"

        public static DataTable DeleteSupplierInvoice(int InvoiceId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_DeleteSupplierInvoice";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SupplierInvoiceId", InvoiceId);
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
        public static string SP_GetMaxSINo(int BranchId,int FyearId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetMaxFowardingSupplierInvoiceNo";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@BranchId", BranchId);
            cmd.Parameters.AddWithValue("@FYearId", FyearId);
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

        #region "Supplierpayment"
        public static int AddSupplierRecieptPayment(CustomerRcieptVM RecPy, string UserID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_InsertSupplierRecPay";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RecPayDate", RecPy.RecPayDate);
            cmd.Parameters.AddWithValue("@DocumentNo", RecPy.DocumentNo);
            //cmd.Parameters.AddWithValue("@CustomerID", RecPy.CustomerID);
            cmd.Parameters.AddWithValue("@SupplierID", RecPy.SupplierID);
            cmd.Parameters.AddWithValue("@BusinessCentreID", RecPy.BusinessCentreID);
            cmd.Parameters.AddWithValue("@BankName", RecPy.BankName);
            cmd.Parameters.AddWithValue("@ChequeNo", RecPy.ChequeNo);
            cmd.Parameters.AddWithValue("@ChequeDate", RecPy.ChequeDate);
            cmd.Parameters.AddWithValue("@Remarks", RecPy.Remarks);
            cmd.Parameters.AddWithValue("@AcJournalID", RecPy.AcJournalID);
            cmd.Parameters.AddWithValue("@StatusRec", RecPy.StatusRec);
            cmd.Parameters.AddWithValue("@StatusEntry", RecPy.StatusEntry);
            cmd.Parameters.AddWithValue("@StatusOrigin", RecPy.StatusOrigin);
            cmd.Parameters.AddWithValue("@FYearID", RecPy.FYearID);
            cmd.Parameters.AddWithValue("@AcCompanyID", RecPy.AcCompanyID);
            cmd.Parameters.AddWithValue("@EXRate", RecPy.EXRate);
            cmd.Parameters.AddWithValue("@FMoney", RecPy.FMoney);
            cmd.Parameters.AddWithValue("@UserID", RecPy.UserID);
            cmd.Parameters.AddWithValue("@UpdateDate", CommonFunctions.GetCurrentDateTime().ToString("MM/dd/yyyy HH:mm"));
            cmd.Parameters.AddWithValue("@AcOPInvoiceDetailId", RecPy.AcOPInvoiceDetailID);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
            if (ds.Tables[0].Rows.Count > 0)
            {
                return Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
            }
            else
            {
                return 0;
            }


        }

        public static void InsertJournalOfSupplier(int RecpayID, int fyearId)
        {
            //SP_InsertJournalEntryForRecPay
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_InsertJournalEntryForSupplierRecPay";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RecPayID", RecpayID);
            cmd.Parameters.AddWithValue("@AcFinnancialYearId", fyearId);
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();

            //Context1.SP_InsertJournalEntryForRecPay(RecpayID, fyaerId);
        }
        public static int InsertRecpayDetailsForSupplier(int RecPayID, int InvoiceID, int JInvoiceID, decimal Amount, string Remarks, string StatusInvoice, bool StatusAdvance, string statusReceip, string InvDate, string InvNo, int CurrencyID, int invoiceStatus, int JobID )
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_InsertRecPayDetailsForSupplier";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RecPayID", RecPayID);
            cmd.Parameters.AddWithValue("@InvoiceID", InvoiceID);
            cmd.Parameters.AddWithValue("@Amount", Amount);
            cmd.Parameters.AddWithValue("@Remarks", Remarks);
            cmd.Parameters.AddWithValue("@StatusInvoice", StatusInvoice);
            cmd.Parameters.AddWithValue("@StatusAdvance", StatusAdvance);
            cmd.Parameters.AddWithValue("@statusReceipt", statusReceip);
            cmd.Parameters.AddWithValue("@InvDate", InvDate);
            cmd.Parameters.AddWithValue("@InvNo", InvNo);
            cmd.Parameters.AddWithValue("@CurrencyID", CurrencyID);
            cmd.Parameters.AddWithValue("@invoiceStatus", invoiceStatus);
           
            
            //cmd.Parameters.AddWithValue("@InScanId", JobID);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
            if (ds.Tables[0].Rows.Count > 0)
            {
                return Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
            }
            else
            {
                return 0;
            }


        }

        #endregion
        #region "Reconc"
        public static int SaveReconc(int DRRID,DateTime  DRRDate, int DRSID, int DRSRecpayID, decimal ReconcAmount, int courierId ,int BranchId,int FYearId ,int UserId,string Details)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            if (DRRID==0)
                cmd.CommandText = "SP_SaveReconc";
            else
                cmd.CommandText = "SP_SaveReconcUpdate";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@DRRID", DRRID);
            cmd.CommandTimeout = 0;
            cmd.Parameters.AddWithValue("@DRRDate",DRRDate.ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@DRSID", DRSID);
            cmd.Parameters.AddWithValue("@DRSRecPayID", DRSRecpayID);            
            cmd.Parameters.AddWithValue("@CourierId", courierId);
            cmd.Parameters.AddWithValue("@Amount", ReconcAmount);
            cmd.Parameters.AddWithValue("@BranchId", BranchId);
            cmd.Parameters.AddWithValue("@FyearID", FYearId);
            cmd.Parameters.AddWithValue("@UserID", UserId);
            cmd.Parameters.AddWithValue("@FormatForXMLitem", Details);
            //cmd.Parameters.AddWithValue("@InScanId", JobID);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
            if (ds.Tables[0].Rows.Count > 0)
            {
                return Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
            }
            else
            {
                return 0;
            }


        }

        #endregion


        #region customersupplieradvance
        public static decimal SP_GetCustomerAdvance(int CustomerId, int RecPayId, int FyearId,string EntryType)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetCustomerAdvance";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
            cmd.Parameters.AddWithValue("@RecPayId", RecPayId);
            cmd.Parameters.AddWithValue("@FYearId", FyearId);
            cmd.Parameters.AddWithValue("@EntryType", EntryType);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
            if (ds.Tables[0].Rows.Count > 0)
            {
                return Convert.ToDecimal(ds.Tables[0].Rows[0][0].ToString());
            }
            else
            {
                return 0;
            }

        }

        public static decimal SP_GetCustomerInvoiceReceived(int CustomerId, int InvoiceId, int RecPayId, int CreditNoteId, string Type)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetCustomerInvoiceReceived";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
            cmd.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            cmd.Parameters.AddWithValue("@RecPayId", RecPayId);
            cmd.Parameters.AddWithValue("@CreditNoteId", CreditNoteId);
            cmd.Parameters.AddWithValue("@Type", Type);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
            if (ds.Tables[0].Rows.Count > 0)
            {
                return Convert.ToDecimal(ds.Tables[0].Rows[0][0].ToString());
            }
            else
            {
                return 0;
            }

        }
        public static List<CustomerTradeReceiptVM> GetCustomerReceiptAllocated(int CustomerId, int InvoiceId, int RecPayId, int CreditNoteId, string Type, string RecPayType,string EntryType)
        {
            List<CustomerTradeReceiptVM> list = new List<CustomerTradeReceiptVM>();
            CustomerTradeReceiptVM item = new CustomerTradeReceiptVM();
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetCustomerReceiptAllocated";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
            cmd.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            cmd.Parameters.AddWithValue("@RecPayId", RecPayId);
            cmd.Parameters.AddWithValue("@CreditNoteId", CreditNoteId);
            cmd.Parameters.AddWithValue("@Type", Type);
            cmd.Parameters.AddWithValue("@FYearId", yearid);
            cmd.Parameters.AddWithValue("@BranchId", branchid);
            cmd.Parameters.AddWithValue("@ReceiptType", RecPayType);
            cmd.Parameters.AddWithValue("@EntryType", EntryType);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow drrow = ds.Tables[0].Rows[i];
                    item = new CustomerTradeReceiptVM();
                    item.RecPayDetailID = CommonFunctions.ParseInt(drrow["RecPayDetailID"].ToString());
                    item.SalesInvoiceID =CommonFunctions.ParseInt(drrow["InvoiceID"].ToString());
                    item.AcOPInvoiceDetailID = CommonFunctions.ParseInt(drrow["AcOPInvoiceDetailID"].ToString());
                    item.InvoiceNo = drrow["InvoiceNo"].ToString();
                    item.InvoiceType = drrow["TransType"].ToString();
                    item.date = Convert.ToDateTime(Convert.ToDateTime(drrow["InvoiceDate"].ToString()).ToString("yyyy-MM-dd h:mm tt"));
                    item.DateTime = Convert.ToDateTime(drrow["InvoiceDate"].ToString()).ToString("dd-MM-yyyy");
                    item.InvoiceAmount = Convert.ToDecimal(drrow["InvoiceAmount"].ToString());
                    item.AmountReceived = Convert.ToDecimal(drrow["ReceivedAmount"].ToString());
 
                    item.Balance = Convert.ToDecimal(drrow["Balance"].ToString());
                    item.AdjustmentAmount = Convert.ToDecimal(drrow["AdjustmentAmount"].ToString());
                    item.Allocated = true;
                    list.Add(item);

                }
            }

            return list;

        }

        public static List<CustomerTradeReceiptVM> SP_GetCustomerInvoicePending(int CustomerId, int InvoiceId, int RecPayId, int CreditNoteId, string Type,string RecPayType)
        {
            List<CustomerTradeReceiptVM> list = new List<CustomerTradeReceiptVM>();
            CustomerTradeReceiptVM item = new CustomerTradeReceiptVM();
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetCustomerInvoicePending";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
            cmd.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            cmd.Parameters.AddWithValue("@RecPayId", RecPayId);
            cmd.Parameters.AddWithValue("@CreditNoteId", CreditNoteId);
            cmd.Parameters.AddWithValue("@Type", Type);
            cmd.Parameters.AddWithValue("@FYearId", yearid);
            cmd.Parameters.AddWithValue("@BranchId", branchid);
            cmd.Parameters.AddWithValue("@ReceiptType", RecPayType );
            
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow drrow = ds.Tables[0].Rows[i];
                    item = new CustomerTradeReceiptVM();
                    item.SalesInvoiceID = Convert.ToInt32(drrow["InvoiceID"].ToString());
                    item.InvoiceNo = drrow["InvoiceNo"].ToString();
                    item.InvoiceType = drrow["TransType"].ToString();
                    item.date = Convert.ToDateTime(drrow["InvoiceDate"].ToString());
                    item.DateTime = Convert.ToDateTime(drrow["InvoiceDate"].ToString()).ToString("dd-MM-yyyy");
                    item.InvoiceAmount = Convert.ToDecimal(drrow["InvoiceAmount"].ToString());
                    item.AmountReceived = Convert.ToDecimal(drrow["ReceivedAmount"].ToString());
                    item.Balance = Convert.ToDecimal(drrow["Balance"].ToString());
                    item.Allocated = false;
                    item.Amount = 0;
                    list.Add(item);

                }
            }

            return list;

        }
        public static List<CustomerTradeReceiptVM> SP_GetCustomerInvoicePendingDRS(int CustomerId, int InvoiceId, int RecPayId, int CreditNoteId, string Type, string RecPayType)
        {
            List<CustomerTradeReceiptVM> list = new List<CustomerTradeReceiptVM>();
            CustomerTradeReceiptVM item = new CustomerTradeReceiptVM();
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetCustomerInvoicePendingDRS";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
            cmd.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            cmd.Parameters.AddWithValue("@RecPayId", RecPayId);
            cmd.Parameters.AddWithValue("@CreditNoteId", CreditNoteId);
            cmd.Parameters.AddWithValue("@Type", Type);
            cmd.Parameters.AddWithValue("@FYearId", yearid);
            cmd.Parameters.AddWithValue("@BranchId", branchid);
            cmd.Parameters.AddWithValue("@ReceiptType", RecPayType);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow drrow = ds.Tables[0].Rows[i];
                    item = new CustomerTradeReceiptVM();
                    item.SalesInvoiceID = Convert.ToInt32(drrow["InvoiceID"].ToString());
                    item.InvoiceNo = drrow["InvoiceNo"].ToString();
                    item.InvoiceType = drrow["TransType"].ToString();
                    item.DateTime = Convert.ToDateTime(drrow["InvoiceDate"].ToString()).ToString("dd-MM-yyyy");
                    item.InvoiceAmount = Convert.ToDecimal(drrow["InvoiceAmount"].ToString());
                    item.AmountReceived = Convert.ToDecimal(drrow["ReceivedAmount"].ToString());
                    item.Balance = Convert.ToDecimal(drrow["Balance"].ToString());

                    list.Add(item);

                }
            }

            return list;

        }
        public static List<CustomerTradeReceiptVM> SP_GetCustomerReceiptPending(int CustomerId, int InvoiceId, int RecPayId, int CreditNoteId, string Type)
        {
            List<CustomerTradeReceiptVM> list = new List<CustomerTradeReceiptVM>();
            CustomerTradeReceiptVM item = new CustomerTradeReceiptVM();
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetCustomerReceiptPending";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
            cmd.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            cmd.Parameters.AddWithValue("@RecPayId", RecPayId);
            cmd.Parameters.AddWithValue("@CreditNoteId", CreditNoteId);
            cmd.Parameters.AddWithValue("@Type", Type);
            cmd.Parameters.AddWithValue("@FYearId", yearid);
            cmd.Parameters.AddWithValue("@BranchId", branchid);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow drrow = ds.Tables[0].Rows[i];
                    item = new CustomerTradeReceiptVM();
                    item.SalesInvoiceID = Convert.ToInt32(drrow["InvoiceID"].ToString());
                    item.InvoiceNo = drrow["InvoiceNo"].ToString();
                    item.DateTime = Convert.ToDateTime(drrow["InvoiceDate"].ToString()).ToString("dd-MM-yyyy");
                    item.InvoiceType = drrow["TransType"].ToString();
                    item.InvoiceAmount = Convert.ToDecimal(drrow["InvoiceAmount"].ToString());
                    item.AmountReceived = Convert.ToDecimal(drrow["ReceivedAmount"].ToString());
                    item.Balance = Convert.ToDecimal(drrow["Balance"].ToString());

                    list.Add(item);

                }
            }

            return list;

        }

        public static List<CustomerTradeReceiptVM> SP_GetSupplierInvoicePending(int SupplierId,int RecPayId, int SupplierTypeId, string Type)
        {
            List<CustomerTradeReceiptVM> list = new List<CustomerTradeReceiptVM>();
            CustomerTradeReceiptVM item = new CustomerTradeReceiptVM();
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetSupplierInvoicePending";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SupplierId", SupplierId);
         
            cmd.Parameters.AddWithValue("@RecPayId", RecPayId); 
          
            cmd.Parameters.AddWithValue("@FYearId", yearid);
            cmd.Parameters.AddWithValue("@BranchId", branchid);
            cmd.Parameters.AddWithValue("@SupplierTypeId",SupplierTypeId);
            cmd.Parameters.AddWithValue("@EntryType", Type);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow drrow = ds.Tables[0].Rows[i];
                    item = new CustomerTradeReceiptVM();
                    item.SalesInvoiceID = Convert.ToInt32(drrow["InvoiceID"].ToString());
                    item.InvoiceNo = drrow["InvoiceNo"].ToString();
                    item.InvoiceType = drrow["TransType"].ToString();
                    item.date = Convert.ToDateTime(drrow["InvoiceDate"].ToString());
                    item.DateTime = Convert.ToDateTime(drrow["InvoiceDate"].ToString()).ToString("dd-MM-yyyy");
                    item.InvoiceAmount = Math.Abs(Convert.ToDecimal(drrow["InvoiceAmount"].ToString()));
                    item.AmountReceived = Convert.ToDecimal(drrow["ReceivedAmount"].ToString());
                    item.Balance = Convert.ToDecimal(drrow["Balance"].ToString());
                    item.Remarks = drrow["Remarks"].ToString();
                    item.Amount = 0;
                    list.Add(item);

                }
            }

            return list;

        }

        public static List<CustomerTradeReceiptVM> SP_GetSupplierReceiptPending(int SupplierID, int InvoiceId, int RecPayId, int CreditNoteId, string Type)
        {
            List<CustomerTradeReceiptVM> list = new List<CustomerTradeReceiptVM>();
            CustomerTradeReceiptVM item = new CustomerTradeReceiptVM();
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetSupplierPaymentPending";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SupplierId", SupplierID);
            cmd.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            cmd.Parameters.AddWithValue("@RecPayId", RecPayId);
            cmd.Parameters.AddWithValue("@DebitNoteId", CreditNoteId);
            cmd.Parameters.AddWithValue("@Type", Type);
            cmd.Parameters.AddWithValue("@FYearId", yearid);
            cmd.Parameters.AddWithValue("@BranchId", branchid);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow drrow = ds.Tables[0].Rows[i];
                    item = new CustomerTradeReceiptVM();
                    item.SalesInvoiceID = Convert.ToInt32(drrow["InvoiceID"].ToString());
                    item.InvoiceNo = drrow["InvoiceNo"].ToString();
                    item.InvoiceType = drrow["TransType"].ToString();
                    item.DateTime = Convert.ToDateTime(drrow["InvoiceDate"].ToString()).ToString("dd-MM-yyyy");
                    item.InvoiceAmount = Convert.ToDecimal(drrow["InvoiceAmount"].ToString());
                    item.AmountReceived = Convert.ToDecimal(drrow["ReceivedAmount"].ToString());
                    item.Balance = Convert.ToDecimal(drrow["Balance"].ToString());

                    list.Add(item);

                }
            }

            return list;

        }


        public static decimal SP_GetSupplierAdvance(int SupplierId, int RecPayId, int FyearId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetSupplierAdvance";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SupplierId", SupplierId);
            cmd.Parameters.AddWithValue("@RecPayId", RecPayId);
            cmd.Parameters.AddWithValue("@FYearId", FyearId);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
            if (ds.Tables[0].Rows.Count > 0)
            {
                return Convert.ToDecimal(ds.Tables[0].Rows[0][0].ToString());
            }
            else
            {
                return 0;
            }

        }

        public static decimal SP_GetSupplierInvoicePaid(int SupplierId, int InvoiceId, int RecPayId, int DebitNoteId, string Type)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetSupplierInvoicePaid";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SupplierId", SupplierId);
            cmd.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            cmd.Parameters.AddWithValue("@RecPayId", RecPayId);
            cmd.Parameters.AddWithValue("@DebitNoteId", DebitNoteId);
            cmd.Parameters.AddWithValue("@Type", Type);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
            if (ds.Tables[0].Rows.Count > 0)
            {
                return Convert.ToDecimal(ds.Tables[0].Rows[0][0].ToString());
            }
            else
            {
                return 0;
            }

        }
        #endregion

        #region MCPayment
        public static List<DRRRecPayDetailVM> GetMCAWBPending( MCDatePicker datePicker, int MCPVVID, int BranchId,int FyearId )
        {
            List<DRRRecPayDetailVM> list = new List<DRRRecPayDetailVM>();
            try
            {
                              SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_GetMCRAWBPending";
                cmd.CommandType = CommandType.StoredProcedure;
                if (datePicker != null)
                {
                    if (datePicker.SearchOption == null)
                    {
                        datePicker.SearchOption = "Date";
                    }
                    cmd.Parameters.AddWithValue("@SearchOption", datePicker.SearchOption);
                    if (datePicker.AWBNo == null)
                        datePicker.AWBNo = "";
                    cmd.Parameters.AddWithValue("@AWBNo", datePicker.AWBNo);
                    if (datePicker.Shipper == null)
                    {
                        datePicker.Shipper = "";
                    }
                    cmd.Parameters.AddWithValue("@Shipper", datePicker.Shipper);
                    cmd.Parameters.AddWithValue("@FromDate", Convert.ToDateTime(datePicker.FromDate1).ToString("MM/dd/yyyy"));
                    cmd.Parameters.AddWithValue("@ToDate", Convert.ToDateTime(datePicker.ToDate1).ToString("MM/dd/yyyy"));
                }
                cmd.Parameters.AddWithValue("@MCPVID", MCPVVID);
                cmd.Parameters.AddWithValue("@FYearID", FyearId);
                cmd.Parameters.AddWithValue("@BranchID",BranchId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        DataRow drrow = ds.Tables[0].Rows[i];
                        DRRRecPayDetailVM item = new DRRRecPayDetailVM();
                        item.InScanID = Convert.ToInt32(drrow["InScanID"].ToString());
                        item.AWBNo = drrow["AWBNo"].ToString();
                        item.AWBDateTime = Convert.ToDateTime(drrow["AWBDate"].ToString());
                        item.AWBDate= Convert.ToDateTime(drrow["AWBDate"].ToString()).ToString("dd/MM/yyyy");
                        item.ConsigneeName = drrow["Consignee"].ToString();
                        item.ConsignorName = drrow["Consignor"].ToString();
                        item.MaterialCost = Convert.ToDecimal(drrow["MaterialCost"].ToString());
                        item.AmountReceived = Convert.ToDecimal(drrow["ReceivedAmount"].ToString());
                        item.AmountPaid = Convert.ToDecimal(drrow["PaidAmount"].ToString());
                        item.AmountPending = Convert.ToDecimal(drrow["PendingAmount"].ToString());
                        item.Amount= Convert.ToDecimal(drrow["PayingAmount"].ToString());
                        item.AdjustmentAmount = Convert.ToDecimal(drrow["AdjustmentAmount"].ToString());
                        list.Add(item);

                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                return list;
            }

            return list;
        }
        #endregion

        #region CodeGeneration

        public static void ReSaveForwardingAgentCode()
        {
            //SP_InsertJournalEntryForRecPay
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_ReSaveForwardingAgentCode";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();

            //Context1.SP_InsertJournalEntryForRecPay(RecpayID, fyaerId);
        }
        public static void ReSaveSupplierCode()
        {
            //SP_InsertJournalEntryForRecPay
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_ReSaveSupplierCode";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();

            //Context1.SP_InsertJournalEntryForRecPay(RecpayID, fyaerId);
        }
        public static void ReSaveEmployeeCode()
        {
            //SP_InsertJournalEntryForRecPay
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_ReSaveEmployeeCode";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();

            //Context1.SP_InsertJournalEntryForRecPay(RecpayID, fyaerId);
        }

        public static void ReSaveCustomerCode()
        {
            //SP_InsertJournalEntryForRecPay
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_ReSaveCustomerCode";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
        }

        public static string GetMaxCustomerCode(string CustomerName)
        {
            try
            {
                //SP_InsertJournalEntryForRecPay
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_GetMaxCustomerCode";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CustomerName", CustomerName);
                cmd.Connection.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string cutomercode = ds.Tables[0].Rows[0][0].ToString();

                    return cutomercode;
                }
                else
                {
                    return "";
                }
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }
        #endregion


        public static CustomerInvoiceVM CustomerInvoiceDetail(int id)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            string usertype = HttpContext.Current.Session["UserType"].ToString();
            CustomerInvoiceVM item = new CustomerInvoiceVM();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetCustomerInvoiceDetail";
            cmd.CommandType = CommandType.StoredProcedure;                        
            cmd.Parameters.AddWithValue("@InvoiceId", id);

            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = cmd;
            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds, "CustomerInvoice");
                     
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow drrow = ds.Tables[0].Rows[i];
                    item = new CustomerInvoiceVM();
                    item.CustomerInvoiceID = Convert.ToInt32(drrow["CustomerInvoiceID"].ToString());
                    item.CustomerInvoiceNo = drrow["CustomerInvoiceNo"].ToString();
                    item.InvoiceDate = Convert.ToDateTime(drrow["InvoiceDate"].ToString());                    
                    item.InvoiceTotal = Convert.ToDecimal(drrow["InvoiceTotal"].ToString());                   
                    item.CustomerName= drrow["CustomerName"].ToString();
                    item.CustomerCode = drrow["CustomerCode"].ToString();
                    item.VATTRN = drrow["VATTRN"].ToString();
                    item.CustomerCityName = drrow["CityName"].ToString();
                    item.CustomerCountryName = drrow["CountryName"].ToString();                    
                    item.CustomerPhoneNo = drrow["CustomerPhoneNo"].ToString();
                    item.Address1 = drrow["Address1"].ToString();
                    item.Pincode = drrow["Address3"].ToString();
                    item.InvoiceFooter1= drrow["InvoiceFooter1"].ToString();
                    item.InvoiceFooter2 = drrow["InvoiceFooter2"].ToString();
                    item.InvoiceFooter3 = drrow["InvoiceFooter3"].ToString();
                    item.InvoiceFooter4 = drrow["InvoiceFooter4"].ToString();
                    item.InvoiceFooter5 = drrow["InvoiceFooter5"].ToString();
                    item.BankDetail1 = drrow["BankDetail1"].ToString();
                    item.BankDetail2 = drrow["BankDetail2"].ToString();
                    item.BankDetail3 = drrow["BankDetail3"].ToString();
                    item.BankDetail4 = drrow["BankDetail4"].ToString();
                    item.BranchTRN = drrow["BranchVATRegistrationNo"].ToString();
                }
            }

            return item;

        }


        #region "ReceiptAllocation"
        public static List<ReceiptVM> GetAllCustomerReceipts(string EntryType,DateTime FromDate, DateTime ToDate, int FyearID, string ReceiptNo, string Type, int CustomerId, bool AllocationPending, string InvoiceNo)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            if (EntryType == "CN")
            {
                cmd.CommandText = "SP_GetAllCustomerCreditNotes";
            }
            else if (EntryType=="OP")
            {
                cmd.CommandText = "SP_GetAllCustomerOpeningCredits";
            }
            else
            {
                cmd.CommandText = "SP_GetAllCustomerReceipt";

            }
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@FromDate", Convert.ToDateTime(FromDate).ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@Todate", Convert.ToDateTime(ToDate).ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@FyearId", FyearID);
            cmd.Parameters.AddWithValue("@BranchId", branchid);
            if (ReceiptNo == null)
                ReceiptNo = "";
            cmd.Parameters.AddWithValue("@ReceiptNo", ReceiptNo);
            cmd.Parameters.AddWithValue("@Type", Type);
            cmd.Parameters.AddWithValue("@CustomerID", CustomerId);
            cmd.Parameters.AddWithValue("@AllocationPending", AllocationPending);
            if (InvoiceNo == null)
                InvoiceNo = "";
            cmd.Parameters.AddWithValue("@InvoiceNo", InvoiceNo);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<ReceiptVM> objList = new List<ReceiptVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    ReceiptVM obj = new ReceiptVM();
                    obj.RecPayID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["RecPayID"].ToString());
                    obj.RecPayDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.DocumentNo = ds.Tables[0].Rows[i]["DocumentNo"].ToString();
                    obj.PaymentMode = ds.Tables[0].Rows[i]["PaymentMode"].ToString();
                    obj.BankName = ds.Tables[0].Rows[i]["BankName"].ToString();
                    obj.Remarks = ds.Tables[0].Rows[i]["Remarks"].ToString();
                    obj.CustomerID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CustomerID"].ToString());
                    obj.PartyName = ds.Tables[0].Rows[i]["PartyName"].ToString();
                    if (ds.Tables[0].Rows[i]["Amount"] == DBNull.Value)
                    {
                        obj.Amount = 0;
                    }
                    else
                    {
                        obj.Amount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Amount"].ToString());
                    }
                    obj.Currency = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Currency"].ToString());
                    obj.ChequeNo = ds.Tables[0].Rows[i]["ChequeNo"].ToString();
                    obj.ChequeDate = ds.Tables[0].Rows[i]["ChequeDate"].ToString();
                    if (ds.Tables[0].Rows[i]["AllocatedAmount"] == DBNull.Value)
                    {
                        obj.AllocatedAmount = 0;
                    }
                    else
                    {
                        obj.AllocatedAmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["AllocatedAmount"].ToString());
                    }
                
                    if (ds.Tables[0].Rows[i]["DiscountAmount"] == DBNull.Value)
                    {
                        obj.DiscountAmount = 0;
                    }
                    else
                    {
                        obj.DiscountAmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["DiscountAmount"].ToString());
                    }
                    if (ds.Tables[0].Rows[i]["OtherReceipt"] == DBNull.Value)
                    {
                        obj.OtherReceipt = 0;
                    }
                    else
                    {
                        obj.OtherReceipt = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["OtherReceipt"].ToString());
                    }
                    obj.UnAllocatedAmount = Convert.ToDecimal(obj.Amount) - Convert.ToDecimal(obj.AllocatedAmount) - Convert.ToDecimal(obj.OtherReceipt);
                    objList.Add(obj);
                }
            }
            return objList;
        }
        #endregion


        #region "PaymentAllocation"
        public static List<CustomerTradeReceiptVM> GetSupplierPaymentAllocated(int SupplierId,int RecPayId, int SupplierTypeId, string EntryType)
        {
            List<CustomerTradeReceiptVM> list = new List<CustomerTradeReceiptVM>();
            CustomerTradeReceiptVM item = new CustomerTradeReceiptVM();
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            int yearid = Convert.ToInt32(HttpContext.Current.Session["fyearid"].ToString());
            int userid = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetSupplierPaymentAllocated";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SupplierId", SupplierId);            
            cmd.Parameters.AddWithValue("@RecPayId", RecPayId);
            cmd.Parameters.AddWithValue("@FYearId", yearid);
            cmd.Parameters.AddWithValue("@BranchId", branchid);
            cmd.Parameters.AddWithValue("@SupplierTypeId", SupplierTypeId);
            cmd.Parameters.AddWithValue("@EntryType", EntryType);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow drrow = ds.Tables[0].Rows[i];
                    item = new CustomerTradeReceiptVM();
                    item.RecPayDetailID = CommonFunctions.ParseInt(drrow["RecPayDetailID"].ToString());
                    item.SalesInvoiceID = CommonFunctions.ParseInt(drrow["InvoiceID"].ToString());
                    item.AcOPInvoiceDetailID = CommonFunctions.ParseInt(drrow["AcOPInvoiceDetailID"].ToString());
                    item.InvoiceNo = drrow["InvoiceNo"].ToString();
                    item.InvoiceType = drrow["TransType"].ToString();
                    item.date = Convert.ToDateTime(Convert.ToDateTime(drrow["InvoiceDate"].ToString()).ToString("yyyy-MM-dd h:mm tt"));
                    item.DateTime = Convert.ToDateTime(drrow["InvoiceDate"].ToString()).ToString("dd-MM-yyyy");
                    item.InvoiceAmount = Math.Abs(Convert.ToDecimal(drrow["InvoiceAmount"].ToString()));
                    item.AmountReceived = Math.Abs(Convert.ToDecimal(drrow["ReceivedAmount"].ToString()));

                    item.Balance = Convert.ToDecimal(drrow["Balance"].ToString());
                    item.AdjustmentAmount = Convert.ToDecimal(drrow["AdjustmentAmount"].ToString());
                    item.Allocated = true;
                    list.Add(item);

                }
            }

            return list;

        }
        public static decimal SP_GetSupplierAdvance(int SupplierId, int RecPayId, int FyearId, string EntryType)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetSuppilerAdvance";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SupplierId", SupplierId);
            cmd.Parameters.AddWithValue("@RecPayId", RecPayId);
            cmd.Parameters.AddWithValue("@FYearId", FyearId);
            cmd.Parameters.AddWithValue("@EntryType", EntryType);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
            if (ds.Tables[0].Rows.Count > 0)
            {
                return Convert.ToDecimal(ds.Tables[0].Rows[0][0].ToString());
            }
            else
            {
                return 0;
            }

        }
        public static List<ReceiptVM> GetAllSupplierPayments(string EntryType, DateTime FromDate, DateTime ToDate, int FyearID, string ReceiptNo, int @SupplierTypeID, int SupplierId, bool AllocationPending, string InvoiceNo)
        {
            int branchid = Convert.ToInt32(HttpContext.Current.Session["CurrentBranchID"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            if (EntryType == "DN")
            {
                cmd.CommandText = "SP_GetAllSupplierDebitNotes";
            }
            else if (EntryType == "OP")
            {
                cmd.CommandText = "SP_GetAllSupplierOpeningDebit";
            }
            else
            {
                cmd.CommandText = "SP_GetAllSupplierPayment";

            }
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@FromDate", Convert.ToDateTime(FromDate).ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@Todate", Convert.ToDateTime(ToDate).ToString("MM/dd/yyyy"));
            cmd.Parameters.AddWithValue("@FyearId", FyearID);
            cmd.Parameters.AddWithValue("@BranchId", branchid);
            if (ReceiptNo == null)
                ReceiptNo = "";
            cmd.Parameters.AddWithValue("@ReceiptNo", ReceiptNo);
            cmd.Parameters.AddWithValue("@SupplierTypeID", SupplierTypeID);
            cmd.Parameters.AddWithValue("@SupplierID", SupplierId);
            cmd.Parameters.AddWithValue("@AllocationPending", AllocationPending);
            if (InvoiceNo == null)
                InvoiceNo = "";
            cmd.Parameters.AddWithValue("@InvoiceNo", InvoiceNo);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<ReceiptVM> objList = new List<ReceiptVM>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    ReceiptVM obj = new ReceiptVM();
                    obj.RecPayID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["RecPayID"].ToString());
                    obj.RecPayDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.DocumentNo = ds.Tables[0].Rows[i]["DocumentNo"].ToString();
                    obj.PaymentMode = ds.Tables[0].Rows[i]["PaymentMode"].ToString();
                    obj.BankName = ds.Tables[0].Rows[i]["BankName"].ToString();
                    obj.Remarks = ds.Tables[0].Rows[i]["Remarks"].ToString();
                    obj.CustomerID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["SupplierID"].ToString());
                    obj.PartyName = ds.Tables[0].Rows[i]["PartyName"].ToString();
                    if (ds.Tables[0].Rows[i]["Amount"] == DBNull.Value)
                    {
                        obj.Amount = 0;
                    }
                    else
                    {
                        obj.Amount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Amount"].ToString());
                    }
                    obj.Currency = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Currency"].ToString());
                    obj.ChequeNo = ds.Tables[0].Rows[i]["ChequeNo"].ToString();
                    obj.ChequeDate = ds.Tables[0].Rows[i]["ChequeDate"].ToString();
                    if (ds.Tables[0].Rows[i]["AllocatedAmount"] == DBNull.Value)
                    {
                        obj.AllocatedAmount = 0;
                    }
                    else
                    {
                        obj.AllocatedAmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["AllocatedAmount"].ToString());
                    }
                    obj.UnAllocatedAmount = Convert.ToDecimal(obj.Amount) - Convert.ToDecimal(obj.AllocatedAmount);
                    if (ds.Tables[0].Rows[i]["DiscountAmount"] == DBNull.Value)
                    {
                        obj.DiscountAmount = 0;
                    }
                    else
                    {
                        obj.DiscountAmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["DiscountAmount"].ToString());
                    }
                    if (ds.Tables[0].Rows[i]["OtherReceipt"] == DBNull.Value)
                    {
                        obj.OtherReceipt = 0;
                    }
                    else
                    {
                        obj.OtherReceipt = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["OtherReceipt"].ToString());
                    }
                    objList.Add(obj);
                }
            }
            return objList;
        }
        #endregion
    }
}