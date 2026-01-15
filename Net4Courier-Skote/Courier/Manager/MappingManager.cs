// Decompiled with JetBrains decompiler
// Type: HealthCareApp.MappingManager
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using Net4Courier.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace HealthCareApp
{
  public class MappingManager
  {
    private SqlConnection con;

    private void connection()
    {
      this.con = new SqlConnection(ConfigurationManager.ConnectionStrings["myConnectionString"].ToString());
    }

    public List<DBColumn> GetTableColumnNames(string tableName)
    {
      List<string> stringList = new List<string>();
      this.connection();
      this.con.Open();
      SqlDataReader sqlDataReader1 = new SqlCommand("SELECT KU.table_name as tablename,column_name as primarykeycolumn FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS TC INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KU ON TC.CONSTRAINT_TYPE = 'PRIMARY KEY' AND TC.CONSTRAINT_NAME = KU.CONSTRAINT_NAME and ku.table_name = '" + tableName + "' ORDER BY KU.TABLE_NAME, KU.ORDINAL_POSITION; ", this.con).ExecuteReader();
      while (sqlDataReader1.Read())
        stringList.Add(Convert.ToString(sqlDataReader1[1]));
      this.con.Close();
      SqlCommand sqlCommand = new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'" + tableName + "'", this.con);
      this.con.Open();
      SqlDataReader sqlDataReader2 = sqlCommand.ExecuteReader();
      List<DBColumn> dbColumnList = new List<DBColumn>();
      try
      {
        while (sqlDataReader2.Read())
        {
          if (!stringList.Contains(Convert.ToString(sqlDataReader2[0])))
            dbColumnList.Add(new DBColumn()
            {
              CoumnName = Convert.ToString(sqlDataReader2[0])
            });
        }
      }
      catch (Exception ex)
      {
      }
      this.con.Close();
      return dbColumnList;
    }

    public List<TName> GetTableList()
    {
      string appSetting = ConfigurationManager.AppSettings["DatabaseName"];
      List<TName> tnameList = new List<TName>();
      this.connection();
      SqlCommand sqlCommand = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_CATALOG = '" + appSetting + "'", this.con);
      this.con.Open();
      SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
      while (sqlDataReader.Read())
        tnameList.Add(new TName()
        {
          TableName = sqlDataReader[0].ToString()
        });
      this.con.Close();
      return tnameList;
    }
  }
}
