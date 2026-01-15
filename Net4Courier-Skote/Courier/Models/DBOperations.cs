using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Net4Courier
{
    public class DBOperations
    {
        public static string strcon = ConfigurationManager.ConnectionStrings["myConnectionString"].ToString();

        public static string usercon = ConfigurationManager.ConnectionStrings["myConnectionString"].ToString();

        public static DataTable GetTable(string sql, bool IsMainDatabase)
        {
            string connectionstring = string.Empty;
            DataTable dt = new DataTable();
            if (IsMainDatabase)
            {
                connectionstring = strcon;
            }
            else
            {
                connectionstring = usercon;
            }
            using (SqlConnection con = new SqlConnection(connectionstring))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                return dt;
            }
        }

        public static DataTable GetTable(SqlCommand cmd, bool IsMainDatabase)
        {
            string connectionstring = string.Empty;
            DataTable dt = new DataTable();
            if (IsMainDatabase)
            {
                connectionstring = strcon;
            }
            else
            {
                connectionstring = usercon;
            }
            using (SqlConnection con = new SqlConnection(connectionstring))
            {
                cmd.Connection = con;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                return dt;
            }
        }

        public static int ExecuteNonQuery(SqlCommand cmd)
        {
            int i = 0;
            using (SqlConnection con = new SqlConnection(strcon))
            {
                con.Open();
                cmd.Connection = con;
                cmd.CommandType = CommandType.Text;
                i = cmd.ExecuteNonQuery();
            }
            return i;
        }

        public static DataTable GetTableWithParameters(SqlCommand cmd, bool IsMainDatabase)
        {
            string connectionstring = string.Empty;
            DataTable dt = new DataTable();
            if (IsMainDatabase)
            {
                connectionstring = strcon;
            }
            else
            {
                connectionstring = usercon;
            }
            using (SqlConnection con = new SqlConnection(connectionstring))
            {
                con.Open();
                cmd.Connection = con;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                con.Close();
            }
            return dt;
        }


        public static DataTable BankRecon(DataTable dt, bool IsMainDatabase, DateTime fromDate, DateTime todate, string Bank)
        {
            string connectionstring = string.Empty;

            connectionstring = IsMainDatabase ? strcon : usercon;
            using (SqlConnection connection = new SqlConnection(connectionstring))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "dbo.proc_GET_CSV_RECON_DATA";
                    command.CommandType = CommandType.StoredProcedure;

                    SqlParameter parameter;
                    parameter = command.Parameters.AddWithValue("@TVP_CSV", dt);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "dbo.csv_list_tbltype";

                    command.Parameters.AddWithValue("@BankName", Bank);
                    command.Parameters.AddWithValue("@DateFrom", fromDate.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@Dateto", todate.ToString("yyyy-MM-dd"));

                    SqlDataAdapter da = new SqlDataAdapter(command);
                    dt = new DataTable();
                    da.Fill(dt);
                    connection.Close();
                }
            }

            return dt;
        }


        public static int SaveReconciliation(DataTable dt, bool IsMainDatabase)
        {
            string connectionstring = string.Empty;

            connectionstring = IsMainDatabase ? strcon : usercon;
            int i = 0;
            using (SqlConnection connection = new SqlConnection(connectionstring))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "dbo.proc_UPDATE_RECONCILIATION";
                    command.CommandType = CommandType.StoredProcedure;

                    SqlParameter parameter;
                    parameter = command.Parameters.AddWithValue("@TVP_RECON", dt);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "dbo.recon_list_tbltype";

                    i = command.ExecuteNonQuery();
                }
            }
            return i;
        }

        public static List<T> ConvertToList<T>(DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName.ToLower()).ToList();
            var properties = typeof(T).GetProperties();
            return dt.AsEnumerable().Select(row =>
            {
                var objT = Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    if (columnNames.Contains(pro.Name.ToLower()))
                    {
                        try
                        {
                            pro.SetValue(objT, row[pro.Name]);
                        }
                        catch (Exception ex) { }
                    }
                }
                return objT;
            }).ToList();
        }
    }
}