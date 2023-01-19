using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace DashboardWebApp.Data
{
    public class DataCommandBuilder
    {
        public static void DeriveParameters(IDbCommand cmdIn)
        {
            if (DataClassFactory.GetCommandType(cmdIn.Connection.ConnectionString) == CommandType.NORMAL)
            {
                SqlCommandBuilder.DeriveParameters((SqlCommand)cmdIn);
            }
        }
    }
}
