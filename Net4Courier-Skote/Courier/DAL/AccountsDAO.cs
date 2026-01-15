using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Collections;
using Net4Courier.Models;
using System.Configuration;

namespace Net4Courier.DAL
{
    public class AccountsDAO
    {

        public static List<RevenueAcHeadVM> GetRevenueTypeList(int BranchId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_RevenueTypeList";
            cmd.CommandType = CommandType.StoredProcedure;

            //cmd.Parameters.AddWithValue("@FYearId", FYearId);
            cmd.Parameters.AddWithValue("@BranchId", BranchId);
            //cmd.Parameters.AddWithValue("@term", term);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<RevenueAcHeadVM> objList = new List<RevenueAcHeadVM>();
            RevenueAcHeadVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new RevenueAcHeadVM();
                    obj.RevenueTypeID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["RevenueTypeID"].ToString());
                    obj.RevenueType = ds.Tables[0].Rows[i]["RevenueType"].ToString();
                    obj.RevenueCode = ds.Tables[0].Rows[i]["RevenueCode"].ToString();

                    obj.AcHead = ds.Tables[0].Rows[i]["SalesHead"].ToString();
                    obj.CostAcHead = ds.Tables[0].Rows[i]["CostHead"].ToString();

                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static string GetMaxVoucherNo(int FYearId, int BranchId, string VoucherType)
        {

            SqlCommand cmd = new SqlCommand();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            cmd.Connection = new SqlConnection(strConnString);
            cmd.CommandText = "SP_GetMaxVoucherNo";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@FyearId", SqlDbType.Int);
            cmd.Parameters["@FyearId"].Value = FYearId;

            cmd.Parameters.AddWithValue("@VoucherType", VoucherType);

            cmd.Parameters.AddWithValue("@BranchId", BranchId);
            string voucherno = "";
            try
            {
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds != null && ds.Tables.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        voucherno = ds.Tables[0].Rows[0][0].ToString();

                    }
                }

            }
            catch (Exception ex)
            {
                return "";
            }
            return voucherno;
        }
        public static string GetMaxVoucherNo(int FYearId,int BranchId)
        {
            
            SqlCommand cmd = new SqlCommand();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            cmd.Connection = new SqlConnection(strConnString);
            cmd.CommandText = "SP_GetMaxVoucherNo";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@FyearId", SqlDbType.Int);
            cmd.Parameters["@FyearId"].Value = FYearId;

            cmd.Parameters.AddWithValue("@VoucherType", "");

            cmd.Parameters.AddWithValue("@BranchId", BranchId);
            string voucherno = "";
            try
            {
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);                
                if (ds != null && ds.Tables.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {                        
                        voucherno= ds.Tables[0].Rows[0][0].ToString();
                        
                    }
                }
                
            }
            catch (Exception ex)
            {
                return "";
            }
            return voucherno;
        }
        public static int InsertOrUpdateAcBankDetails(AcBankDetail ObjectAcBankDetail, int isupdate)
        {
            int iReturn = 0;
            SqlCommand cmd = new SqlCommand();
            string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            cmd.Connection = new SqlConnection(strConnString);
            cmd.CommandText = "SP_InsertOrUpdateAcBankDetails";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@AcBankDetailID", SqlDbType.Int);
            cmd.Parameters["@AcBankDetailID"].Value = ObjectAcBankDetail.AcBankDetailID;

            cmd.Parameters.Add("@AcJournalID", SqlDbType.Int);
            cmd.Parameters["@AcJournalID"].Value = ObjectAcBankDetail.AcJournalID;

            cmd.Parameters.Add("@BankName", SqlDbType.NVarChar);
            cmd.Parameters["@BankName"].Value = ObjectAcBankDetail.BankName;

            cmd.Parameters.Add("@ChequeNo", SqlDbType.NVarChar);
            cmd.Parameters["@ChequeNo"].Value = ObjectAcBankDetail.ChequeNo;

            if (ObjectAcBankDetail.ChequeDate != null)
            {
                cmd.Parameters.Add("@ChequeDate", SqlDbType.DateTime);
                cmd.Parameters["@ChequeDate"].Value = ObjectAcBankDetail.ChequeDate;
            }
            cmd.Parameters.Add("@PartyName", SqlDbType.NVarChar);
            cmd.Parameters["@PartyName"].Value = ObjectAcBankDetail.PartyName;

            cmd.Parameters.Add("@StatusTrans", SqlDbType.NVarChar);
            cmd.Parameters["@StatusTrans"].Value = ObjectAcBankDetail.StatusTrans;

            cmd.Parameters.Add("@IsUpdate", SqlDbType.Int);
            cmd.Parameters["@IsUpdate"].Value = isupdate;
            if (ObjectAcBankDetail.StatusReconciled != null)
            {
                cmd.Parameters.Add("@StatusReconciled", SqlDbType.Bit);
                cmd.Parameters["@StatusReconciled"].Value = ObjectAcBankDetail.StatusReconciled;
            }
            if (ObjectAcBankDetail.ValueDate != null)
            {
                cmd.Parameters.Add("@ValueDate", SqlDbType.DateTime);
                cmd.Parameters["@ValueDate"].Value = ObjectAcBankDetail.ValueDate;
            }
            try
            {
                cmd.Connection.Open();
                iReturn = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {

            }
            return iReturn;
        }

        public static List<AcJournalDetailsVM> GetAcJournalDetails(int AcJournalID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SELECT ah.AcHead,aj.Amount, aj.Remarks FROM AcJournalDetail as aj INNER JOIN AcHead as ah on aj.AcHeadID=ah.AcHeadID WHERE aj.AcJournalID = @AcJournalID";
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.Add("@AcJournalID", SqlDbType.Int);
            cmd.Parameters["@AcJournalID"].Value = AcJournalID;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<AcJournalDetailsVM> objList = new List<AcJournalDetailsVM>();
            AcJournalDetailsVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new AcJournalDetailsVM();
                    obj.AcHead = ds.Tables[0].Rows[i]["AcHead"].ToString();
                    obj.Amount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Amount"].ToString());
                    obj.Remarks = ds.Tables[0].Rows[i]["Remarks"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static int UpdateAcJournalDetail(AcJournalDetail ObjectAcJournalDetail)
        {
            int iReturn = 0;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "UPDATE AcJournalDetail SET AcJournalID=@AcJournalID,AcHeadID=@AcHeadID,AnalysisHeadID=@AnalysisHeadID,Amount=@Amount,Remarks=@Remarks,BranchID=@BranchID,AmountIncludingTax=@AmountIncludingTax,TaxPercent=@TaxPercent,TaxAmount=@TaxAmount, SupplierId=@SupplierId WHERE AcJournalDetailID = @AcJournalDetailID";
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.Add("@AcJournalDetailID", SqlDbType.Int);
            cmd.Parameters["@AcJournalDetailID"].Value = ObjectAcJournalDetail.AcJournalDetailID;

            cmd.Parameters.Add("@AcJournalID", SqlDbType.Int);
            cmd.Parameters["@AcJournalID"].Value = ObjectAcJournalDetail.AcJournalID;

            cmd.Parameters.Add("@AcHeadID", SqlDbType.Int);
            cmd.Parameters["@AcHeadID"].Value = ObjectAcJournalDetail.AcHeadID;

            cmd.Parameters.Add("@AnalysisHeadID", SqlDbType.Int);
            cmd.Parameters["@AnalysisHeadID"].Value = ObjectAcJournalDetail.AnalysisHeadID;

            cmd.Parameters.Add("@Amount", SqlDbType.Money);
            cmd.Parameters["@Amount"].Value = ObjectAcJournalDetail.Amount;

            cmd.Parameters.Add("@Remarks", SqlDbType.VarChar);
            cmd.Parameters["@Remarks"].Value = ObjectAcJournalDetail.Remarks;

            cmd.Parameters.Add("@BranchID", SqlDbType.Int);
            cmd.Parameters["@BranchID"].Value = ObjectAcJournalDetail.BranchID;

            cmd.Parameters.Add("@AmountIncludingTax", SqlDbType.Bit);
            cmd.Parameters["@AmountIncludingTax"].Value = ObjectAcJournalDetail.AmountIncludingTax;

            cmd.Parameters.Add("@TaxPercent", SqlDbType.Decimal);
            cmd.Parameters["@TaxPercent"].Value = ObjectAcJournalDetail.TaxPercent;

            cmd.Parameters.Add("@TaxAmount", SqlDbType.Money);
            cmd.Parameters["@TaxAmount"].Value = ObjectAcJournalDetail.TaxAmount;

            cmd.Parameters.Add("@SupplierId", SqlDbType.Int);
            cmd.Parameters["@SupplierId"].Value = ObjectAcJournalDetail.SupplierId;


            try
            {
                cmd.Connection.Open();
                iReturn = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {

            }

            return iReturn;
        }

        public static int DeleteAcJournalDetail(int AcJournalDetailID)
        {
            int iReturn = 0;
            SqlCommand cmd2 = new SqlCommand();
            cmd2.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd2.CommandText = "DELETE FROM AcAnalysisHeadAllocation WHERE AcjournalDetailID = @AcjournalDetailID";
            cmd2.CommandType = CommandType.Text;
            cmd2.Parameters.Add("@AcjournalDetailID", SqlDbType.Int);
            cmd2.Parameters["@AcjournalDetailID"].Value = AcJournalDetailID;
            try
            {
                cmd2.Connection.Open();
                iReturn = cmd2.ExecuteNonQuery();
                cmd2.Connection.Close();
            }
            catch (Exception ex)
            {

            }


            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "DELETE FROM AcJournalDetail WHERE AcJournalDetailID = @AcJournalDetailID";
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.Add("@AcJournalDetailID", SqlDbType.Int);
            cmd.Parameters["@AcJournalDetailID"].Value = AcJournalDetailID;

            try
            {
                cmd.Connection.Open();
                iReturn = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {

            }

            return iReturn;
        }
        public static int InsertAcJournalDetail(AcJournalDetail ObjectAcJournalDetail)
        {
            int iReturn = 0;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "INSERT INTO AcJournalDetail(AcJournalDetailID,AcJournalID,AcHeadID,Amount,Remarks,BranchID,AmountIncludingTax,TaxPercent,TaxAmount,SupplierId) VALUES(@AcJournalDetailID,@AcJournalID,@AcHeadID,@Amount,@Remarks,@BranchID,@AmountIncludingTax,@TaxPercent,@TaxAmount,@SupplierId)";
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.Add("@AcJournalDetailID", SqlDbType.Int);
            cmd.Parameters["@AcJournalDetailID"].Value = ObjectAcJournalDetail.AcJournalDetailID;

            cmd.Parameters.Add("@AcJournalID", SqlDbType.Int);
            cmd.Parameters["@AcJournalID"].Value = ObjectAcJournalDetail.AcJournalID;

            cmd.Parameters.Add("@AcHeadID", SqlDbType.Int);
            cmd.Parameters["@AcHeadID"].Value = ObjectAcJournalDetail.AcHeadID;

            //   cmd.Parameters.Add("@AnalysisHeadID", SqlDbType.Int);
            //    cmd.Parameters["@AnalysisHeadID"].Value = ObjectAcJournalDetail.AnalysisHeadID;

            cmd.Parameters.Add("@Amount", SqlDbType.Money);
            cmd.Parameters["@Amount"].Value = ObjectAcJournalDetail.Amount;

            cmd.Parameters.Add("@Remarks", SqlDbType.VarChar);
            cmd.Parameters["@Remarks"].Value = ObjectAcJournalDetail.Remarks;

            cmd.Parameters.Add("@BranchID", SqlDbType.Int);
            cmd.Parameters["@BranchID"].Value = ObjectAcJournalDetail.BranchID;

            cmd.Parameters.Add("@AmountIncludingTax", SqlDbType.Bit);
            cmd.Parameters["@AmountIncludingTax"].Value = ObjectAcJournalDetail.AmountIncludingTax;

            cmd.Parameters.Add("@TaxPercent", SqlDbType.Decimal);
            cmd.Parameters["@TaxPercent"].Value = ObjectAcJournalDetail.TaxPercent;

            cmd.Parameters.Add("@TaxAmount", SqlDbType.Money);
            cmd.Parameters["@TaxAmount"].Value = ObjectAcJournalDetail.TaxAmount;

            cmd.Parameters.Add("@SupplierId", SqlDbType.Int);
            cmd.Parameters["@SupplierId"].Value = ObjectAcJournalDetail.SupplierId;
            try
            {
                cmd.Connection.Open();
                iReturn = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {

            }

            return iReturn;
        }

        public static int UpdateAcAnalysisHeadAllocation(AcAnalysisHeadAllocation ObjectAcAnalysisHeadAllocation)
        {
            int iReturn = 0;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "UPDATE AcAnalysisHeadAllocation SET AcjournalDetailID=@AcjournalDetailID,AnalysisHeadID=@AnalysisHeadID,Amount=@Amount WHERE AcAnalysisHeadAllocationID=@AcAnalysisHeadAllocationID";
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.Add("@AcAnalysisHeadAllocationID", SqlDbType.Int);
            cmd.Parameters["@AcAnalysisHeadAllocationID"].Value = ObjectAcAnalysisHeadAllocation.AcAnalysisHeadAllocationID;

            cmd.Parameters.Add("@AcjournalDetailID", SqlDbType.Int);
            cmd.Parameters["@AcjournalDetailID"].Value = ObjectAcAnalysisHeadAllocation.AcjournalDetailID;

            cmd.Parameters.Add("@AnalysisHeadID", SqlDbType.Int);
            cmd.Parameters["@AnalysisHeadID"].Value = ObjectAcAnalysisHeadAllocation.AnalysisHeadID;

            cmd.Parameters.Add("@Amount", SqlDbType.Money);
            cmd.Parameters["@Amount"].Value = ObjectAcAnalysisHeadAllocation.Amount;

            try
            {
                cmd.Connection.Open();
                iReturn = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {

            }
            return iReturn;
        }

        public static int InsertAcAnalysisHeadAllocation(AcAnalysisHeadAllocation ObjectAcAnalysisHeadAllocation)
        {
            int iReturn = 0;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "INSERT INTO AcAnalysisHeadAllocation(AcjournalDetailID,AnalysisHeadID,Amount) VALUES(@AcjournalDetailID,@AnalysisHeadID,@Amount)";
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.Add("@AcjournalDetailID", SqlDbType.Int);
            cmd.Parameters["@AcjournalDetailID"].Value = ObjectAcAnalysisHeadAllocation.AcjournalDetailID;

            cmd.Parameters.Add("@AnalysisHeadID", SqlDbType.Int);
            cmd.Parameters["@AnalysisHeadID"].Value = ObjectAcAnalysisHeadAllocation.AnalysisHeadID;

            cmd.Parameters.Add("@Amount", SqlDbType.Money);
            cmd.Parameters["@Amount"].Value = ObjectAcAnalysisHeadAllocation.Amount;

            try
            {
                cmd.Connection.Open();
                iReturn = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {

            }
            return iReturn;
        }

        public static int DeleteAcAnalysisHeadAllocation(int AcAnalysisHeadAllocationID)
        {
            int iReturn = 0;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "DELETE FROM AcAnalysisHeadAllocation WHERE AcAnalysisHeadAllocationID=@AcAnalysisHeadAllocationID";
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.Add("@AcAnalysisHeadAllocationID", SqlDbType.Int);
            cmd.Parameters["@AcAnalysisHeadAllocationID"].Value = AcAnalysisHeadAllocationID;

            try
            {
                cmd.Connection.Open();
                iReturn = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {

            }
            return iReturn;
        }

        public static List<AcAnalysisHeadAllocationVM> GetAcJDetailsExpenseAllocation(int AcJournalDetailID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "GetAcJDetailsExpenseAllocation";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@AcJournalDetailID", SqlDbType.Int);
            cmd.Parameters["@AcJournalDetailID"].Value = AcJournalDetailID;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<AcAnalysisHeadAllocationVM> objList = new List<AcAnalysisHeadAllocationVM>();
            AcAnalysisHeadAllocationVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new AcAnalysisHeadAllocationVM();
                    obj.AcAnalysisHeadAllocationID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AcAnalysisHeadAllocationID"].ToString());
                    obj.AcjournalDetailID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AcjournalDetailID"].ToString());
                    obj.AnalysisHeadID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AnalysisHeadID"].ToString());
                    if (ds.Tables[0].Rows[i]["Amount"] == DBNull.Value)
                    {
                        obj.Amount = 0;
                    }
                    else
                    {
                        obj.Amount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Amount"].ToString());
                    }
                    obj.AnalysisHead = ds.Tables[0].Rows[i]["AnalysisHead"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static List<AcHeadSelectAllVM> GetAcHeadSelectAll(int BranchID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "AcHeadSelectAll";
            cmd.CommandTimeout = 0;
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@BranchID", SqlDbType.Int);
            cmd.Parameters["@BranchID"].Value = BranchID;

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
           da.Fill(ds);
            List<AcHeadSelectAllVM> objList = new List<AcHeadSelectAllVM>();
            AcHeadSelectAllVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new AcHeadSelectAllVM();
                    obj.AcHeadID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AcHeadID"].ToString());
                    obj.AcHeadKey = ds.Tables[0].Rows[i]["AcHeadKey"].ToString();
                    obj.AcHead = ds.Tables[0].Rows[i]["AcHead"].ToString();
                    obj.AcGroupID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AcGroupID"].ToString());
                    obj.ParentID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ParentID"].ToString());
                    obj.HeadOrder = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["HeadOrder"].ToString());
                    if (ds.Tables[0].Rows[i]["StatusHide"] == DBNull.Value)
                    {
                        obj.StatusHide = false;
                    }
                    else
                    {
                        obj.StatusHide = Convert.ToBoolean(ds.Tables[0].Rows[i]["StatusHide"].ToString());
                    }
                    obj.UserID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["UserID"].ToString());
                    obj.Prefix = ds.Tables[0].Rows[i]["Prefix"].ToString();
                    obj.AcGroup = ds.Tables[0].Rows[i]["AcGroup"].ToString();
                    obj.AccountType = ds.Tables[0].Rows[i]["AccountType"].ToString();
                    if (ds.Tables[0].Rows[i]["TaxApplicable"] == DBNull.Value)
                    {
                        obj.TaxApplicable = false;

                    }
                    else
                    {                        
                        obj.TaxApplicable = Convert.ToBoolean(ds.Tables[0].Rows[i]["TaxApplicable"].ToString());
                        if (obj.TaxApplicable==true)
                        {
                            obj.TaxPercent = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["TaxPercent"].ToString());
                        }
                        else
                        {
                            obj.TaxPercent = 0;
                        }
                        
                    }


                    objList.Add(obj);
                }
            }
            return objList;
        }
        public static List<AcHeadSelectAllVM> GetAcHeadSelectAllCreate(int BranchID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "AcHeadSelectAll";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@BranchID", SqlDbType.Int);
            cmd.Parameters["@BranchID"].Value = BranchID;

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<AcHeadSelectAllVM> objList = new List<AcHeadSelectAllVM>();
            AcHeadSelectAllVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new AcHeadSelectAllVM();
                    obj.AcHeadID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AcHeadID"].ToString());
                    obj.AcHeadKey = ds.Tables[0].Rows[i]["AcHeadKey"].ToString();
                    obj.AcHead = ds.Tables[0].Rows[i]["AcHead"].ToString();
                    obj.AcGroupID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AcGroupID"].ToString());
                    obj.ParentID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ParentID"].ToString());
                    obj.HeadOrder = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["HeadOrder"].ToString());
                    if (ds.Tables[0].Rows[i]["StatusHide"] == DBNull.Value)
                    {
                        obj.StatusHide = false;
                    }
                    else
                    {
                        obj.StatusHide = Convert.ToBoolean(ds.Tables[0].Rows[i]["StatusHide"].ToString());
                    }
                    obj.UserID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["UserID"].ToString());
                    obj.Prefix = ds.Tables[0].Rows[i]["Prefix"].ToString();
                    obj.AcGroup = ds.Tables[0].Rows[i]["AcGroup"].ToString();
                    obj.AccountType = ds.Tables[0].Rows[i]["AccountType"].ToString();
                    if (ds.Tables[0].Rows[i]["TaxApplicable"] == DBNull.Value)
                    {
                        obj.TaxApplicable = false;

                    }
                    else
                    {
                        obj.TaxApplicable = Convert.ToBoolean(ds.Tables[0].Rows[i]["TaxApplicable"].ToString());
                        if (obj.TaxApplicable == true)
                        {
                            obj.TaxPercent = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["TaxPercent"].ToString());
                        }
                        else
                        {
                            obj.TaxPercent = 0;
                        }

                    }


                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static List<AcJournalDetailVM> AcJournalDetailSelectByAcJournalID(int AcJournalID, string PaymentType)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "AcJournalDetailSelectByAcJournalID";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@AcJournalID", SqlDbType.Int);
            cmd.Parameters["@AcJournalID"].Value = AcJournalID;

            cmd.Parameters.Add("@PaymentType", SqlDbType.VarChar);
            cmd.Parameters["@PaymentType"].Value = PaymentType;

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<AcJournalDetailVM> objList = new List<AcJournalDetailVM>();
            AcJournalDetailVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new AcJournalDetailVM();
                    obj.AcJournalDetID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AcJournalDetailID"].ToString());
                    obj.AcHeadID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AcHeadID"].ToString());
                    obj.TaxPercent = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["TaxPercent"].ToString());
                    obj.TaxAmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["TaxAmount"].ToString());
                                        
                    if (ds.Tables[0].Rows[i]["AmountIncludingTax"]==null)
                    {
                        obj.AmountIncludingTax = false;                        
                    }
                    else
                    {
                        obj.AmountIncludingTax = Convert.ToBoolean(ds.Tables[0].Rows[i]["AmountIncludingTax"].ToString());
                    }

                    if (obj.AmountIncludingTax ==true && obj.TaxAmount>0)
                    {
                        obj.Amt = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Amount"].ToString())+ obj.TaxAmount;
                    }
                    else
                    {
                        obj.Amt = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Amount"].ToString());
                    }
                    
                    obj.Rem = ds.Tables[0].Rows[i]["Remarks"].ToString();
                    obj.AcHead = ds.Tables[0].Rows[i]["AcHead"].ToString();
                    obj.SupplierID = Convert.ToInt32(ds.Tables[0].Rows[i]["SupplierId"].ToString());
                    obj.SupplierName = ds.Tables[0].Rows[i]["SupplierName"].ToString();

                    objList.Add(obj);
                }
            }
            return objList;
        }

        //index jv voucher book
        public static List<AcJournalMasterVM> AcJournalMasterSelect(int FYearId, int BranchID, DateTime FromDate, DateTime ToDate,string VoucherNo)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "AcJournalMasterSelectAllJVNew";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 0;
            cmd.Parameters.Add("@FYearID", SqlDbType.Int);
            cmd.Parameters["@FYearID"].Value = FYearId;

            cmd.Parameters.Add("@BranchID", SqlDbType.Int);
            cmd.Parameters["@BranchID"].Value = BranchID;

            cmd.Parameters.Add("@FromDate", SqlDbType.VarChar);
            cmd.Parameters["@FromDate"].Value = FromDate.ToString("MM/dd/yyyy");

            cmd.Parameters.Add("@ToDate", SqlDbType.VarChar);
            cmd.Parameters["@ToDate"].Value = ToDate.ToString("MM/dd/yyyy");

            cmd.Parameters.Add("@VoucherNo", SqlDbType.VarChar);
            cmd.Parameters["@VoucherNo"].Value = VoucherNo;

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<AcJournalMasterVM> objList = new List<AcJournalMasterVM>();
            AcJournalMasterVM obj;

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new AcJournalMasterVM();
                    obj.AcJournalID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AcJournalID"].ToString());
                    obj.ID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ID"].ToString());
                    obj.VoucherNo = ds.Tables[0].Rows[i]["VoucherNo"].ToString();
                    obj.TransDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["TransDate"].ToString());
                    obj.Remarks = ds.Tables[0].Rows[i]["Remarks"].ToString();
                    obj.TransactionNo = ds.Tables[0].Rows[i]["TransactionNo"].ToString();
                    obj.Reference = ds.Tables[0].Rows[i]["Reference"].ToString();
                    obj.VoucherType = ds.Tables[0].Rows[i]["VoucherType"].ToString();
                    obj.Amount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["JournalAmount"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }


        //Indexacbook page
        public static List<AcJournalMasterVM> AcJournalMasterSelectAll(int FYearId, int BranchID, DateTime FromDate, DateTime ToDate,string VoucherType,string VoucherNo)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "AcJournalMasterAll";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 0;
            cmd.Parameters.Add("@FYearID", SqlDbType.Int);
            cmd.Parameters["@FYearID"].Value = FYearId;

            cmd.Parameters.Add("@BranchID", SqlDbType.Int);
            cmd.Parameters["@BranchID"].Value = BranchID;

            cmd.Parameters.Add("@FromDate", SqlDbType.VarChar);
            cmd.Parameters["@FromDate"].Value = FromDate.ToString("MM/dd/yyyy");

            cmd.Parameters.Add("@ToDate", SqlDbType.VarChar);
            cmd.Parameters["@ToDate"].Value = ToDate.ToString("MM/dd/yyyy");

            cmd.Parameters.Add("@VoucherType", SqlDbType.VarChar);
            cmd.Parameters["@VoucherType"].Value = VoucherType;

            cmd.Parameters.Add("@VoucherNo", SqlDbType.VarChar);
            cmd.Parameters["@VoucherNo"].Value = VoucherNo;

            

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<AcJournalMasterVM> objList = new List<AcJournalMasterVM>();
            AcJournalMasterVM obj;

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new AcJournalMasterVM();
                    obj.AcJournalID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AcJournalID"].ToString());
                    //obj.ID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ID"].ToString());
                    obj.VoucherNo = ds.Tables[0].Rows[i]["VoucherNo"].ToString();
                    obj.TransDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["TransDate"].ToString());
                    obj.Remarks = ds.Tables[0].Rows[i]["Remarks"].ToString();
                    //obj.TransactionNo = ds.Tables[0].Rows[i]["TransactionNo"].ToString();
                    //obj.Reference = ds.Tables[0].Rows[i]["Reference"].ToString();
                    obj.Amount = Convert.ToDecimal(ds.Tables[0].Rows[i]["Amount"].ToString());
                    obj.VoucherType = ds.Tables[0].Rows[i]["VoucherType"].ToString();
                    obj.MasterHead = ds.Tables[0].Rows[i]["AcHead"].ToString();
                    obj.MasterHeadAmount = Convert.ToDecimal(ds.Tables[0].Rows[i]["Amount"].ToString());
                    obj.VoucherType = ds.Tables[0].Rows[i]["VoucherType"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static DateTime CheckParamDate(DateTime EntryDate,int yearid)
        {
            DateTime pFromDate = Convert.ToDateTime(EntryDate);
            StatusModel obj = new StatusModel();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_CheckParamDate";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EntryDate", pFromDate.ToString("MM/dd/yyyy"));
                cmd.Parameters.AddWithValue("@FYearId", yearid);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    obj.Status = ds.Tables[0].Rows[0][0].ToString();
                    obj.Message = ds.Tables[0].Rows[0][1].ToString();
                    obj.ValidDate = Convert.ToDateTime(ds.Tables[0].Rows[0][2].ToString()).ToString("dd-MM-yyyy");
                }
            }
            catch (Exception ex)
            {
                obj.Status = "Failed";
                obj.Message = ex.Message;

            }
            return Convert.ToDateTime(obj.ValidDate);
            if (obj.Status!="OK")
            {
                return Convert.ToDateTime(obj.ValidDate);
            }
            else
            {
                return EntryDate;
            }
        }
        public static StatusModel CheckDateValidate(string EntryDate,int FyearId)
        {
            StatusModel obj = new StatusModel();
            if (EntryDate != null)
            {
                DateTime pFromDate = Convert.ToDateTime(EntryDate);
             
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                    cmd.CommandText = "SP_CheckDateValiate";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EntryDate", pFromDate.ToString("MM/dd/yyyy HH:mm"));
                    cmd.Parameters.AddWithValue("@FYearId", FyearId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        obj.Status = ds.Tables[0].Rows[0][0].ToString();
                        obj.Message = ds.Tables[0].Rows[0][1].ToString();
                        obj.ValidDate = Convert.ToDateTime(ds.Tables[0].Rows[0][2].ToString()).ToString("dd-MM-yyyy HH:mm");
                        //obj.ValidReportDate = Convert.ToDateTime(ds.Tables[0].Rows[0][3].ToString()).ToString("dd-MM-yyyy HH:mm");
                    }
                }
                catch (Exception ex)
                {
                    obj.Status = "Failed";
                    obj.Message = ex.Message;

                }
            }
            return obj;
        }
        //Account Opening
        public static List<AcOpeningMasterVm> GetAccountOpeningList(int FYearId, int BranchId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_AccountOpeningIndex";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FYearId", FYearId);
            cmd.Parameters.AddWithValue("@BranchId", BranchId);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<AcOpeningMasterVm> objList = new List<AcOpeningMasterVm>();
            AcOpeningMasterVm obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new AcOpeningMasterVm();
                    obj.AcOpeningID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AcOpeningID"].ToString());
                    obj.AcHeadID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AcHeadID"].ToString());
                    obj.AcHead = ds.Tables[0].Rows[i]["AcHead"].ToString();
                    obj.DebitAmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["DebitAmount"].ToString());
                    obj.CreditAmount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["CreditAmount"].ToString());
                    objList.Add(obj);
                }
            }
            return objList;
        }
        //Account Master Opening Posting
        public static string AccountOpeningPosting(int fyearid, int branchid,int AcOpeningID)
        {
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_AcOpeningMasterPosting " + fyearid.ToString() + "," + branchid + "," + AcOpeningID.ToString();
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
        //InvoiceOpeningPosting
        public static string CustomerInvoiceOpeningPosting(int fyearid, int branchid,string StatusSDSC)
        {
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_CustomerInvoiceOpeningPosting " + fyearid.ToString() + "," + branchid + ",'" + StatusSDSC + "'";//SP_AcInvoiceOpeningPosting
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
        public static string SupplierInvoiceOpeningPosting(int SupplierTypeID, int fyearid, int branchid)
        {
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_SupplierInvoiceOpeningPosting "  + SupplierTypeID.ToString() + ","  + fyearid.ToString() + "," + branchid;//SP_AcInvoiceOpeningPosting
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
        //InvoiceOpeningPosting
        public static string InvoiceOpeningPosting(int MasterId, int fyearid, int branchid,string StatusSDSC)
        {
            try
            {
                //string json = "";
                string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(strConnString))
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "SP_CustomerInvoiceOpeningPosting " + MasterId + "," + fyearid.ToString() + "," + branchid; //SP_AcInvoiceOpeningPosting
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
        public static List<AcInvoiceOpeningVM> GetInvoiceOpening(int FYearId, string Type)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetInvoiceOpening";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@FYearId", FYearId);
            cmd.Parameters.AddWithValue("@Type", Type);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<AcInvoiceOpeningVM> objList = new List<AcInvoiceOpeningVM>();
            AcInvoiceOpeningVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new AcInvoiceOpeningVM();
                    obj.AcOPInvoiceMasterID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AcOPInvoiceMasterID"].ToString());
                    obj.PartyID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["PartyId"].ToString());
                    obj.PartyName = ds.Tables[0].Rows[i]["PartName"].ToString();
                    obj.PartyType = ds.Tables[0].Rows[i]["PartType"].ToString();
                    obj.Debit = Convert.ToDecimal(ds.Tables[0].Rows[i]["Debit"].ToString());
                    obj.Credit = Convert.ToDecimal(ds.Tables[0].Rows[i]["Credit"].ToString());
                    obj.StatusSDSC = ds.Tables[0].Rows[i]["StatusSDSC"].ToString();

                    objList.Add(obj);
                }
            }
            return objList;
        }

        //ac journal master
        public static string GetMaxVoucherNo(string VoucherType, int yearid)
        {
            string voucherno = "";

            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "GetMaxVoucherNo";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@VoucherType", VoucherType);
                cmd.Parameters.AddWithValue("@FYearId", yearid);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    voucherno = ds.Tables[0].Rows[0][0].ToString();
                }
            }
            catch (Exception ex)
            {
                return voucherno;
            }

            return voucherno;
        }
        public static string GetMaxCreditNoteNo(int yearid,string TransType)
        {
            string voucherno = "";

            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "GetMaxCreditNoteNo";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FYearId", yearid);
                cmd.Parameters.AddWithValue("@TransType", TransType);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    voucherno = ds.Tables[0].Rows[0][0].ToString();
                }
            }
            catch (Exception ex)
            {
                return voucherno;
            }

            return voucherno;
        }

        public static string GetMaxDebiteNoteNo(int yearid,string TransType="DN")
        {
            string voucherno = "";

            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "GetMaxDebitNoteNo";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FYearId", yearid);
                cmd.Parameters.AddWithValue("@TransType", TransType);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    voucherno = ds.Tables[0].Rows[0][0].ToString();
                }
            }
            catch (Exception ex)
            {
                return voucherno;
            }

            return voucherno;
        }

        public static string DeleteDebiteNote(int id)
        {
            string voucherno = "";

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

                if (ds.Tables[0].Rows.Count > 0)
                {
                    voucherno = ds.Tables[0].Rows[0][0].ToString();
                }
            }
            catch (Exception ex)
            {
                return voucherno;
            }

            return voucherno;
        }

        public static string DeleteCreditNote(int id)
        {
            string voucherno = "";

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

                if (ds.Tables[0].Rows.Count > 0)
                {
                    voucherno = ds.Tables[0].Rows[0][0].ToString();
                }
            }
            catch (Exception ex)
            {
                return voucherno;
            }

            return voucherno;
        }

        public static int DeleteAcJournalAWBs(AcJournalDetail ObjectAcJournalDetail)
        {
            int iReturn = 0;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "UPDATE AcJournalAWB where  AcJournalID=@AcJournalID and AcJournalDetailID = @AcJournalDetailID";
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.Add("@AcJournalDetailID", SqlDbType.Int);
            cmd.Parameters["@AcJournalDetailID"].Value = ObjectAcJournalDetail.AcJournalDetailID;

            cmd.Parameters.Add("@AcJournalID", SqlDbType.Int);
            cmd.Parameters["@AcJournalID"].Value = ObjectAcJournalDetail.AcJournalID;

            try
            {
                cmd.Connection.Open();
                iReturn = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {

            }

            return iReturn;
        }

        public static List<AccountSetupMasterVM> GetAccountSetupList()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_AccountSetupList";
            cmd.CommandType = CommandType.StoredProcedure;

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<AccountSetupMasterVM> objList = new List<AccountSetupMasterVM>();
            AccountSetupMasterVM obj;
            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    obj = new AccountSetupMasterVM();
                    obj.ID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ID"].ToString());
                    obj.DebitAccountId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["DebitAccountId"].ToString());
                    obj.CreditAccountId = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["CreditAccountId"].ToString());
                    obj.PageName = ds.Tables[0].Rows[i]["PageName"].ToString();
                    obj.TransType = ds.Tables[0].Rows[i]["TransType"].ToString();
                    obj.SalesType = ds.Tables[0].Rows[i]["SalesType"].ToString();
                    obj.ParcelType = ds.Tables[0].Rows[i]["ParcelType"].ToString();
                    obj.DebitHead = ds.Tables[0].Rows[i]["DebitHead"].ToString();
                    obj.CreditHead = ds.Tables[0].Rows[i]["CreditHead"].ToString();                    

                    objList.Add(obj);
                }
            }
            return objList;
        }
     
        #region YearEndPRocess
        public static YearEndProcessSearch GetYearEndProcess(int YearId,int BranchId,int ProcessStatus)
        {
            YearEndProcessSearch vm = new YearEndProcessSearch();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_AccYearEndProcess";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 2000;
            cmd.Parameters.Add("@YearID", SqlDbType.Int);
            cmd.Parameters["@YearID"].Value = YearId;
            cmd.Parameters.Add("@BranchId", SqlDbType.Int);
            cmd.Parameters["@BranchId"].Value = BranchId;
            cmd.Parameters.AddWithValue("@Process", ProcessStatus);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<YearEndProcessIncomeExpense> objList = new List<YearEndProcessIncomeExpense>();
            List<YearEndProcessPL> objList1 = new List<YearEndProcessPL>();
            if (ProcessStatus == 0)
            {
              
                YearEndProcessIncomeExpense obj;
                if (ds != null && ds.Tables.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        obj = new YearEndProcessIncomeExpense();
                        obj.AcType = ds.Tables[0].Rows[i]["AcType"].ToString();
                        obj.AcHeadId = Convert.ToInt32(ds.Tables[0].Rows[i]["AcHeadId"].ToString());
                        obj.AcHeadName = ds.Tables[0].Rows[i]["AcHeadName"].ToString();
                        obj.ClosingBalance = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["ClosingBalance"].ToString());
                        objList.Add(obj);
                    }
                }
                YearEndProcessPL obj1;
                if (ds != null && ds.Tables.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        obj1 = new YearEndProcessPL();
                        obj1.AcHeadId = Convert.ToInt32(ds.Tables[1].Rows[i]["AcHeadId"].ToString());
                        obj1.VoucherNo = ds.Tables[1].Rows[i]["VoucherNo"].ToString();
                        obj1.AcHeadName = ds.Tables[1].Rows[i]["AcHeadName"].ToString();
                        obj1.Amount = CommonFunctions.ParseDecimal(ds.Tables[1].Rows[i]["Amount"].ToString());
                        obj1.updatestatus = Convert.ToBoolean(ds.Tables[1].Rows[i]["updatedstatus"].ToString());
                        objList1.Add(obj1);
                    }
                }
                vm.PLDetails = objList1;
                vm.IncomeExpDetails = objList;
            }
            else
            {
                vm.PLDetails = objList1;
                vm.IncomeExpDetails = objList;

            }
            return vm;
        }


        public static List<YearEndProcessAccounts> GetYearEndAccountOpening(int Userid,int YearId, int BranchId,int process)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_AcYearEndAccountOpeningProcess";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 0;
            cmd.Parameters.Add("@YearID", SqlDbType.Int);
            cmd.Parameters["@YearID"].Value = YearId;
            cmd.Parameters.Add("@BranchId", SqlDbType.Int);
            cmd.Parameters["@BranchId"].Value = BranchId;
            cmd.Parameters.AddWithValue("@UserId", Userid);
            cmd.Parameters.AddWithValue("@Process", process);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<YearEndProcessAccounts> objList = new List<YearEndProcessAccounts>();
            YearEndProcessAccounts obj;
            if (process == 0)
            {
                if (ds != null && ds.Tables.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        obj = new YearEndProcessAccounts();
                        obj.Particulars = ds.Tables[0].Rows[i]["AcHeadName"].ToString();
                        obj.OpeningBalance = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["OpeningBalance"].ToString());
                        obj.Transactions = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Transactions"].ToString());
                        obj.ClosingBalance = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["ClosingBalance"].ToString());
                        obj.NextYearOpening = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["NextYearOpening"].ToString());

                        objList.Add(obj);
                    }
                }
            }
            return objList;
        }


        public static YearEndProcessSearch GetYearEndProcessCustomerInv(string CustomerType, int YearId, int BranchId, int ProcessStatus, string CustomerIDs,int UserId)
        {
            YearEndProcessSearch vm = new YearEndProcessSearch();
            List<YearEndProcessCustomer> objList = new List<YearEndProcessCustomer>();
            List<YearEndProcessCustomer> objInvList = new List<YearEndProcessCustomer>();
            try
            {
                if (ProcessStatus==4) //customer wise statement
                {

                    var enddate = Convert.ToDateTime(HttpContext.Current.Session["FyearTo"]).ToString("MM/dd/yyyy");


                    string strConnString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                        SqlConnection sqlConn = new SqlConnection(strConnString);
                        SqlCommand comd;
                        comd = new SqlCommand();
                        comd.Connection = sqlConn;
                        comd.CommandTimeout = 0;
                        comd.CommandType = CommandType.StoredProcedure;
                        comd.CommandText = "SP_CustomerStatement";
                        comd.Parameters.AddWithValue("@CustomerId", CustomerIDs);
                        comd.Parameters.AddWithValue("@AsonDate", enddate);
                        //comd.Parameters.AddWithValue("@ToDate", reportparam.ToDate.ToString("MM/dd/yyyy"));
                        comd.Parameters.AddWithValue("@FYearId", YearId);
                        comd.Parameters.AddWithValue("@BranchID", BranchId);
                        comd.Parameters.AddWithValue("@CustomerType", CustomerType);
                        SqlDataAdapter sqlAdapter = new SqlDataAdapter();
                        sqlAdapter.SelectCommand = comd;
                        DataSet ds = new DataSet();
                        sqlAdapter.Fill(ds, "CustomerLedgerDetail");
                    YearEndProcessCustomer objInv;
                    for (int j = 0; j < ds.Tables[0].Rows.Count; j++)
                    {
                        objInv = new YearEndProcessCustomer();
                        objInv.CustomerId = Convert.ToInt32(ds.Tables[0].Rows[j]["CustomerId"].ToString());
                        objInv.CustomerName = ds.Tables[0].Rows[j]["CustomerName"].ToString();
                        objInv.InvoiceNo = ds.Tables[0].Rows[j]["CustomerInvoiceNo"].ToString();
                        objInv.InvoiceDate = Convert.ToDateTime(ds.Tables[0].Rows[j]["InvoiceDate"].ToString()).ToString("dd-MM-yyyy");
                        decimal debit= CommonFunctions.ParseDecimal(ds.Tables[0].Rows[j]["Debit"].ToString());
                        decimal credit = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[j]["Credit"].ToString());
                        objInv.Amount = debit - credit;
                        objInvList.Add(objInv);
                    }
                    vm.Status = "OK";
                    vm.CustomerInvDetails = objInvList;
                    return vm;

                }
                if (ProcessStatus == 0)
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                    cmd.CommandText = "SP_GetYearEndCustomerStatement";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    //cmd.Parameters.AddWithValue("CustomerType", CustomerType);
                    cmd.Parameters.Add("@FYearId", SqlDbType.Int);
                    cmd.Parameters["@FYearID"].Value = YearId;
                    cmd.Parameters.Add("@BranchId", SqlDbType.Int);
                    cmd.Parameters["@BranchId"].Value = BranchId;
                    cmd.Parameters.AddWithValue("@Process", ProcessStatus);
                    cmd.Parameters.AddWithValue("@CustomerType", CustomerType);

                    cmd.Parameters.AddWithValue("@CustomerIDs", CustomerIDs);
                    cmd.Parameters.AddWithValue("@UserId", UserId);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    vm.Status = "OK";
                    //if (ds != null && ds.Tables.Count > 0)
                    //{
                    //    pendingcustomercount= Convert.ToInt32(ds.Tables[1].Rows[0][0].ToString());
                    //}

                    //if (pendingcustomercount>0)
                    //{
                    //    GetYearEndProcessCustomerInv(CustomerType, YearId, BranchId, ProcessStatus, CustomerIDs, UserId);
                    //}
                    //else
                    //{
                    //  GetYearEndProcessCustomerInv(CustomerType, YearId, BranchId, -1, CustomerIDs, UserId); //last step to retrieve the customer balance
                    //}

                }

                else if (ProcessStatus == -1 || ProcessStatus==2)
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                    cmd.CommandText = "SP_GetYearEndCustomerStatement";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    //cmd.Parameters.AddWithValue("CustomerType", CustomerType);
                    cmd.Parameters.Add("@FYearId", SqlDbType.Int);
                    cmd.Parameters["@FYearID"].Value = YearId;
                    cmd.Parameters.Add("@BranchId", SqlDbType.Int);
                    cmd.Parameters["@BranchId"].Value = BranchId;
                    cmd.Parameters.AddWithValue("@Process", ProcessStatus);
                    cmd.Parameters.AddWithValue("@CustomerType", CustomerType);

                    cmd.Parameters.AddWithValue("@CustomerIDs", CustomerIDs);
                    cmd.Parameters.AddWithValue("@UserId", UserId);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    YearEndProcessCustomer obj;
                    YearEndProcessCustomer objInv;
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        //for (int j = 0; j < ds.Tables[2].Rows.Count; j++)
                        //{
                        //    objInv = new YearEndProcessCustomer();
                        //    objInv.CustomerId = Convert.ToInt32(ds.Tables[2].Rows[j]["CustomerId"].ToString());
                        //    objInv.InvoiceNo = ds.Tables[2].Rows[j]["Reference"].ToString();
                        //    objInv.InvoiceDate = Convert.ToDateTime(ds.Tables[2].Rows[j]["TransDate"].ToString()).ToString("dd-MM-yyyy");
                        //    objInv.Amount = CommonFunctions.ParseDecimal(ds.Tables[2].Rows[j]["Amount"].ToString());
                        //    objInvList.Add(objInv);
                        //}

                        for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                        {
                            obj = new YearEndProcessCustomer();
                            obj.CustomerId = Convert.ToInt32(ds.Tables[1].Rows[i]["CustomerId"].ToString());
                            obj.CustomerName = ds.Tables[1].Rows[i]["CustomerName"].ToString();
                            //obj.Selected = true;
                            obj.ClosingAmount = CommonFunctions.ParseDecimal(ds.Tables[1].Rows[i]["ClosingBalance"].ToString());
                            obj.OpeningAmount = CommonFunctions.ParseDecimal(ds.Tables[1].Rows[i]["CurrentYearOpening"].ToString());
                            obj.Difference = CommonFunctions.ParseDecimal(ds.Tables[1].Rows[i]["Difference"].ToString());
                            if (obj.Difference != 0)
                                obj.Selected = true;
                            else
                                obj.Selected = false;
                            obj.Details = new List<YearEndProcessCustomer>();
                            //var _details = objInvList.Where(cc => cc.CustomerId == obj.CustomerId).ToList();
                            //if (_details != null)
                            //{
                            //    obj.Details = _details;
                            //}

                            objList.Add(obj);
                        }
                    }

                    vm.CustomerInvDetails = objList;
                    vm.Status = "OK";
                }
                else if (ProcessStatus == 1)
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                    cmd.CommandText = "SP_GetYearEndCustomerStatement";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    //cmd.Parameters.AddWithValue("CustomerType", CustomerType);
                    cmd.Parameters.Add("@FYearId", SqlDbType.Int);
                    cmd.Parameters["@FYearID"].Value = YearId;
                    cmd.Parameters.Add("@BranchId", SqlDbType.Int);
                    cmd.Parameters["@BranchId"].Value = BranchId;
                    cmd.Parameters.AddWithValue("@Process", ProcessStatus);
                    cmd.Parameters.AddWithValue("@CustomerType", CustomerType);

                    cmd.Parameters.AddWithValue("@CustomerIDs", CustomerIDs);
                    cmd.Parameters.AddWithValue("@UserId", UserId);
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                    vm.Status = "OK";

                }
            }catch(Exception ex)
            {
                vm.Status = ex.Message;
            }

            return vm;
        }

        public static YearEndProcessSearch GetYearEndProcessSupplierInv(int SupplierTypeId, int YearId, int BranchId, int ProcessStatus, string SupplierIDs = "", int UserId = -1)
        {
            YearEndProcessSearch vm = new YearEndProcessSearch();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetYearEndSupplierStatement";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SupplierTypeId", SupplierTypeId);
            cmd.Parameters.Add("@FYearId", SqlDbType.Int);
            cmd.Parameters["@FYearID"].Value = YearId;
            cmd.Parameters.Add("@BranchId", SqlDbType.Int);
            cmd.Parameters["@BranchId"].Value = BranchId;
            cmd.Parameters.AddWithValue("@Process", ProcessStatus);
            cmd.Parameters.AddWithValue("@SupplierIDs", SupplierIDs);
            cmd.Parameters.AddWithValue("@UserId", UserId);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<YearEndProcessSupplier> objList = new List<YearEndProcessSupplier>();
            List<YearEndProcessSupplier> objInvList = new List<YearEndProcessSupplier>();
            if (ProcessStatus == 0)
            {
                YearEndProcessSupplier obj;
                YearEndProcessSupplier objInv;
                if (ds != null && ds.Tables.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                    {
                        obj = new YearEndProcessSupplier();
                        obj.SupplierId = Convert.ToInt32(ds.Tables[2].Rows[i]["SupplierId"].ToString());
                        obj.SupplierName = ds.Tables[2].Rows[i]["SupplierName"].ToString();
                        obj.InvoiceNo = ds.Tables[2].Rows[i]["Reference"].ToString();
                        obj.InvoiceDate = Convert.ToDateTime(ds.Tables[2].Rows[i]["TransDate"].ToString()).ToString("dd-MM-yyyy");
                        obj.Amount = CommonFunctions.ParseDecimal(ds.Tables[2].Rows[i]["Amount"].ToString());
                        objInvList.Add(obj);
                    }

                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        obj = new YearEndProcessSupplier();
                        obj.SupplierId = Convert.ToInt32(ds.Tables[1].Rows[i]["SupplierId"].ToString());
                        obj.SupplierName = ds.Tables[1].Rows[i]["SupplierName"].ToString();
                        obj.Selected = true;
                        obj.ClosingAmount = CommonFunctions.ParseDecimal(ds.Tables[1].Rows[i]["ClosingBalance"].ToString());
                        obj.OpeningAmount = CommonFunctions.ParseDecimal(ds.Tables[1].Rows[i]["CurrentYearOpening"].ToString());
                        obj.Difference = CommonFunctions.ParseDecimal(ds.Tables[1].Rows[i]["Difference"].ToString());
                        obj.Details = new List<YearEndProcessSupplier>();
                        var _details = objInvList.Where(cc => cc.SupplierId == obj.SupplierId).ToList();
                        if (_details != null)
                        {
                            obj.Details = _details;
                        }

                        objList.Add(obj);
                    }
                }



                vm.SupplierInvDetails = objList;
            }
            return vm;
        }
        public static YearEndProcessSearch GetYearEndProcessCustomerInvold(string CustomerType, int YearId, int BranchId, int ProcessStatus)
        {
            YearEndProcessSearch vm = new YearEndProcessSearch();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetYearEndCustomerStatement";
            cmd.CommandType = CommandType.StoredProcedure;
            //cmd.Parameters.AddWithValue("CustomerType", CustomerType);
            cmd.Parameters.Add("@FYearId", SqlDbType.Int);
            cmd.Parameters["@FYearID"].Value = YearId;
            cmd.Parameters.Add("@BranchId", SqlDbType.Int);
            cmd.Parameters["@BranchId"].Value = BranchId;
           cmd.Parameters.AddWithValue("@Process", ProcessStatus);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            List<YearEndProcessCustomer> objList = new List<YearEndProcessCustomer>();
            if (ProcessStatus == 0)
            {
                YearEndProcessCustomer obj;
                if (ds != null && ds.Tables.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        obj = new YearEndProcessCustomer();
                        obj.CustomerId = Convert.ToInt32(ds.Tables[0].Rows[i]["CustomerId"].ToString());
                        obj.CustomerName = ds.Tables[0].Rows[i]["CustomerName"].ToString();
                        obj.InvoiceNo = ds.Tables[0].Rows[i]["CustomerInvoiceNo"].ToString();
                        obj.InvoiceDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["InvoiceDate"].ToString()).ToString("dd-MM-yyyy");
                        obj.Amount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Amount"].ToString());
                        objList.Add(obj);
                    }
                }

                vm.CustomerInvDetails = objList;
            }          
            return vm;
        }


        #endregion

        public static List<BankDetails> GetBankDetails(int yearid,int branchid)
        {
            BankReconcSearch paramobj = (BankReconcSearch)(HttpContext.Current.Session["BankReconcSearch"]);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            cmd.CommandText = "SP_GetBankDetails";
            cmd.CommandType = CommandType.StoredProcedure;

            if (paramobj.FromDate != null)
                cmd.Parameters.AddWithValue("@FromDate", Convert.ToDateTime(paramobj.FromDate).ToString("MM/dd/yyyy"));

            if (paramobj.ToDate != null)
                cmd.Parameters.AddWithValue("@ToDate", Convert.ToDateTime(paramobj.ToDate).ToString("MM/dd/yyyy"));
            
            cmd.Parameters.AddWithValue("@BankHeadId", paramobj.BankHeadID);
            cmd.Parameters.AddWithValue("@BranchID", branchid);
            cmd.Parameters.AddWithValue("@FYearId", yearid);
            if (paramobj.FilterStatus==null)
            {
                paramobj.FilterStatus = "P";
            }
            cmd.Parameters.AddWithValue("@StatusTrans", paramobj.FilterStatus);
            
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<BankDetails> objList = new List<BankDetails>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    BankDetails obj = new BankDetails();
                    obj.AcBankDetailID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AcBankDetailID"].ToString());
                    obj.AcJournalID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["AcJournalID"].ToString());
                    obj.VoucherNo = ds.Tables[0].Rows[i]["VoucherNo"].ToString();
                    obj.VoucherType = ds.Tables[0].Rows[i]["VoucherType"].ToString();
                    obj.VoucherType1 = ds.Tables[0].Rows[i]["VoucherType1"].ToString();
                    obj.VoucherDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["VoucherDate"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.Amount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Amount"].ToString());
                    obj.Narration  = ds.Tables[0].Rows[i]["Particulars"].ToString();
                    obj.PartyName= ds.Tables[0].Rows[i]["PartyName"].ToString();
                    obj.ChequeNo = ds.Tables[0].Rows[i]["ChequeNo"].ToString();
                    obj.ChequeDate = CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["ChequeDate"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.StatusReconciled = Convert.ToBoolean(ds.Tables[0].Rows[i]["StatusReconciled"].ToString());
                    obj.ChangeStatus = ds.Tables[0].Rows[i]["StatusTrans"].ToString();
                    obj.TransactionPage = ds.Tables[0].Rows[i]["TransactionPage"].ToString();
                    obj.BankCharges = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["BankCharges"].ToString());
                    obj.Remarks = ds.Tables[0].Rows[i]["Remarks"].ToString();
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static string SaveBankReconc(int UserID, int BranchID, int FYearID, string Details, DateTime ReconcDate)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_SaveBankReconc";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ReconcDate", Convert.ToDateTime(ReconcDate).ToString("MM/dd/yyyy"));                
                cmd.Parameters.AddWithValue("@Details", Details);//formated xml param
                cmd.Parameters.AddWithValue("@FYearId", FYearID);
                cmd.Parameters.AddWithValue("@BranchId", BranchID);
                cmd.Parameters.AddWithValue("@CreatedBy", UserID);
                cmd.Parameters.AddWithValue("@CreatedDate", CommonFunctions.GetCurrentDateTime().ToString("MM/dd/yyyy HH:mm"));

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                int DocumentNo = 0;
                int VoucherCount = 0;
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DocumentNo = CommonFunctions.ParseInt(ds.Tables[0].Rows[0][0].ToString());                    
                    if (DocumentNo > 0)
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
                return ex.Message;
            }

        }

    

        #region  "PDCDeposit"
        public static List<BankDetails> GetChequeDetails(int yearid, int branchid)
        {
            BankReconcSearch paramobj = (BankReconcSearch)(HttpContext.Current.Session["ChequeDepositSearch"]);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
            if (paramobj.FilterStatus=="Pending")
                cmd.CommandText = "SP_GetPDCChequeDetails";
            else
                cmd.CommandText = "SP_GetPDCChequeDetailsDeposited";
            
            cmd.CommandType = CommandType.StoredProcedure;

            if (paramobj.FromDate != null)
                cmd.Parameters.AddWithValue("@FromDate", Convert.ToDateTime(paramobj.FromDate).ToString("MM/dd/yyyy"));

            if (paramobj.ToDate != null)
                cmd.Parameters.AddWithValue("@ToDate", Convert.ToDateTime(paramobj.ToDate).ToString("MM/dd/yyyy"));

            cmd.Parameters.AddWithValue("@BankHeadId", paramobj.BankHeadID);
            cmd.Parameters.AddWithValue("@BranchID", branchid);
            cmd.Parameters.AddWithValue("@FYearId", yearid);          

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            List<BankDetails> objList = new List<BankDetails>();

            if (ds != null && ds.Tables.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    BankDetails obj = new BankDetails();
                    obj.AcBankDetailID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["ID"].ToString());
                    obj.AcJournalID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["PDCAcJournalID"].ToString());
                    obj.PDCOpeningID = CommonFunctions.ParseInt(ds.Tables[0].Rows[i]["PDCOpeningID"].ToString());
                    obj.VoucherNo = ds.Tables[0].Rows[i]["VoucherNo"].ToString();
                    obj.TransType = ds.Tables[0].Rows[i]["Type"].ToString();
                    obj.VoucherType = ds.Tables[0].Rows[i]["VoucherType"].ToString();
                    obj.VoucherDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["VoucherDate"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.Amount = CommonFunctions.ParseDecimal(ds.Tables[0].Rows[i]["Amount"].ToString());
                    obj.Narration = ds.Tables[0].Rows[i]["Particulars"].ToString();
                    obj.PartyName = ds.Tables[0].Rows[i]["PartyName"].ToString();
                    obj.ChequeNo = ds.Tables[0].Rows[i]["ChequeNo"].ToString();
                    obj.ChequeDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["ChequeDate"].ToString()); // CommonFunctions.ParseDate(ds.Tables[0].Rows[i]["RecPayDate"].ToString());
                    obj.ChangeStatus = ds.Tables[0].Rows[i]["StatusTrans"].ToString();
                    if (paramobj.FilterStatus == "Deposited")
                    {
                        obj.ReconcDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["DepositDate"].ToString());
                        obj.DepositHeadName = ds.Tables[0].Rows[i]["DepositHead"].ToString();
                    }

                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static string SaveBankDeposit(int UserID, int BranchID, int FYearID, string Details, DateTime ReconcDate,int BankHeadID,int DepositBankHeadID)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(CommonFunctions.GetConnectionString);
                cmd.CommandText = "SP_SaveBankDeposit";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@DepositDate", Convert.ToDateTime(ReconcDate).ToString("MM/dd/yyyy"));
                cmd.Parameters.AddWithValue("@BankHeadID", BankHeadID);//formated xml param
                cmd.Parameters.AddWithValue("@DepositBankHeadId", DepositBankHeadID);//formated xml param                
                cmd.Parameters.AddWithValue("@Details", Details);//formated xml param
                cmd.Parameters.AddWithValue("@FYearId", FYearID);
                cmd.Parameters.AddWithValue("@BranchId", BranchID);
                cmd.Parameters.AddWithValue("@CreatedBy", UserID);
                cmd.Parameters.AddWithValue("@CreatedDate", CommonFunctions.GetCurrentDateTime().ToString("MM/dd/yyyy HH:mm"));

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                //int query = Context1.SP_InsertRecPay(RecPy.RecPayDate, RecPy.DocumentNo, RecPy.CustomerID, RecPy.SupplierID, RecPy.BusinessCentreID, RecPy.BankName, RecPy.ChequeNo, RecPy.ChequeDate, RecPy.Remarks, RecPy.AcJournalID, RecPy.StatusRec, RecPy.StatusEntry, RecPy.StatusOrigin, RecPy.FYearID, RecPy.AcCompanyID, RecPy.EXRate, RecPy.FMoney, Convert.ToInt32(UserID));
                int DocumentNo = 0;
                int VoucherCount = 0;
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DocumentNo = CommonFunctions.ParseInt(ds.Tables[0].Rows[0][0].ToString());
                    if (DocumentNo > 0)
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
                return ex.Message;
            }

        }
        #endregion

   
    }
}
    


