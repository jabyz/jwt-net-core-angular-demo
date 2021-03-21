using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace jwt_net_core_angular_demo_api.Services
{
  public class Sql
  {
    private readonly IConfiguration _conf;
    private readonly string _JWT_Secret;

    public Sql(IConfiguration conf)
    {
      _conf = conf;
      _JWT_Secret = _conf["ApplicationSettings:JWT_Secret"];
    }

    public SqlConnection getSqlConn()
    {
      string conn = _conf.GetConnectionString("localHost");
      SqlConnection sqlConn = new SqlConnection(conn);

      return sqlConn;
    }

    public DataTable cmdToDataTable(SqlCommand cmd)
    {
      using (SqlConnection sqlConn = getSqlConn())
      {
        if (sqlConn.State == ConnectionState.Open)
          sqlConn.Close();
        try
        {
          cmd.Connection = sqlConn;
          cmd.CommandTimeout = 0;
          sqlConn.Open();
          SqlDataAdapter da = new SqlDataAdapter(cmd);
          DataTable dt = new DataTable();
          da.Fill(dt);
          cmd.Dispose();
          sqlConn.Close();
          sqlConn.Dispose();
          return dt;
        }
        catch (Exception ex)
        {

          cmd.Dispose();
          sqlConn.Close();
          sqlConn.Dispose();
          return null;
        }
      }
    }

    public int xQuery(SqlCommand cmd)
    {
      int ra = 0;
      using (SqlConnection sqlConn = getSqlConn())
      {
        if (sqlConn.State == ConnectionState.Open)
          sqlConn.Close();
        try
        {
          cmd.Connection = sqlConn;
          cmd.CommandTimeout = 0;
          sqlConn.Open();
          ra = cmd.ExecuteNonQuery();

          cmd.Dispose();
          sqlConn.Close();
          sqlConn.Dispose();

          return ra;
        }
        catch (Exception ex)
        {

          cmd.Dispose();
          sqlConn.Close();
          sqlConn.Dispose();
          return 0;
        }
      }
    }
  }
}
